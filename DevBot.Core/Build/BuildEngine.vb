Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text

Namespace DevBot.Core.Build

    Public Class BuildEngine

        Public Function Build(projectPath As String) As BuildResult

            Dim result As New BuildResult()

            If String.IsNullOrWhiteSpace(projectPath) Then
                result.Success = False
                result.Message = "Project path kosong."
                Return result
            End If

            If Not Directory.Exists(projectPath) Then
                result.Success = False
                result.Message = "Folder project tidak ditemukan: " & projectPath
                Return result
            End If

            Try

                '==================================================
                ' PHP project
                ' PHP biasa tidak membutuhkan composer untuk
                ' dilakukan syntax check.
                '==================================================

                Dim phpFiles As List(Of String) = GetPhpFiles(projectPath)

                If phpFiles.Count > 0 Then
                    Return BuildPhpProject(projectPath, phpFiles)
                End If

                '==================================================
                ' Build system umum
                '==================================================

                Dim command As String = Nothing
                Dim arguments As String = Nothing
                Dim workingDirectory As String = projectPath

                DetectBuildCommand(
                    projectPath,
                    command,
                    arguments,
                    workingDirectory)

                If String.IsNullOrWhiteSpace(command) Then
                    result.Success = False
                    result.Message =
                        "Build system belum dikenali." &
                        Environment.NewLine &
                        "Project: " & projectPath
                    Return result
                End If

                result.Command = command & " " & arguments
                result.StartTime = DateTime.Now

                Dim output As New StringBuilder()
                Dim errorOutput As New StringBuilder()

                Using process As New Process()

                    process.StartInfo = New ProcessStartInfo()
                    process.StartInfo.FileName = command
                    process.StartInfo.Arguments = arguments
                    process.StartInfo.WorkingDirectory = workingDirectory
                    process.StartInfo.UseShellExecute = False
                    process.StartInfo.CreateNoWindow = True
                    process.StartInfo.RedirectStandardOutput = True
                    process.StartInfo.RedirectStandardError = True
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8

                    AddHandler process.OutputDataReceived,
                        Sub(sender As Object, e As DataReceivedEventArgs)
                            If e.Data IsNot Nothing Then
                                output.AppendLine(e.Data)
                            End If
                        End Sub

                    AddHandler process.ErrorDataReceived,
                        Sub(sender As Object, e As DataReceivedEventArgs)
                            If e.Data IsNot Nothing Then
                                errorOutput.AppendLine(e.Data)
                            End If
                        End Sub

                    process.Start()
                    process.BeginOutputReadLine()
                    process.BeginErrorReadLine()
                    process.WaitForExit()
                    process.WaitForExit()

                    result.ExitCode = process.ExitCode

                End Using

                result.EndTime = DateTime.Now
                result.Output = output.ToString()
                result.ErrorOutput = errorOutput.ToString()
                result.Success = (result.ExitCode = 0)

                If result.Success Then
                    result.Message =
                        "Build berhasil." &
                        Environment.NewLine &
                        "Exit Code: 0"
                Else
                    result.Message =
                        "Build gagal." &
                        Environment.NewLine &
                        "Exit Code: " & result.ExitCode.ToString()
                End If

                Return result

            Catch ex As Exception

                result.Success = False
                result.EndTime = DateTime.Now
                result.Message = "Build error: " & ex.Message
                result.ErrorOutput = ex.ToString()

                Return result

            End Try

        End Function

        '==========================================================
        ' PHP BUILD / SYNTAX CHECK
        '==========================================================

        Private Function BuildPhpProject(
            projectPath As String,
            phpFiles As List(Of String)) As BuildResult

            Dim result As New BuildResult()
            Dim phpExe As String = FindPhpExecutable()

            result.StartTime = DateTime.Now

            If String.IsNullOrWhiteSpace(phpExe) Then

                result.Success = False
                result.ExitCode = -1
                result.Command = "php -l"
                result.Message =
                    "PHP terdeteksi, tetapi php.exe tidak ditemukan." &
                    Environment.NewLine &
                    "DevBot mencoba lokasi XAMPP dan PATH." &
                    Environment.NewLine &
                    "Project: " & projectPath
                result.ErrorOutput =
                    "Install/aktifkan PHP atau XAMPP agar DevBot dapat melakukan PHP syntax check."
                result.EndTime = DateTime.Now

                Return result

            End If

            Dim output As New StringBuilder()
            Dim errorOutput As New StringBuilder()
            Dim failed As Boolean = False
            Dim firstExitCode As Integer = 0
            Dim checked As Integer = 0

            output.AppendLine("PHP BUILD / SYNTAX CHECK")
            output.AppendLine("=========================")
            output.AppendLine("Project: " & projectPath)
            output.AppendLine("PHP: " & phpExe)
            output.AppendLine("Files: " & phpFiles.Count.ToString())
            output.AppendLine()

            For Each phpFile As String In phpFiles

                checked += 1

                Dim relativePath As String = GetRelativePath(projectPath, phpFile)
                Dim oneOutput As String = Nothing
                Dim oneError As String = Nothing
                Dim exitCode As Integer = -1

                Try

                    exitCode = RunProcess(
                        phpExe,
                        "-l " & Quote(phpFile),
                        projectPath,
                        oneOutput,
                        oneError)

                Catch ex As Exception

                    exitCode = -1
                    oneError = ex.Message

                End Try

                If exitCode = 0 Then

                    output.AppendLine("[PASS] " & relativePath)

                    If Not String.IsNullOrWhiteSpace(oneOutput) Then
                        AppendIndented(output, oneOutput)
                    End If

                Else

                    failed = True

                    If firstExitCode = 0 Then
                        firstExitCode = exitCode
                    End If

                    output.AppendLine("[FAIL] " & relativePath)

                    If Not String.IsNullOrWhiteSpace(oneOutput) Then
                        AppendIndented(output, oneOutput)
                    End If

                    If Not String.IsNullOrWhiteSpace(oneError) Then
                        AppendIndented(errorOutput, relativePath & ": " & oneError)
                    End If

                End If

            Next

            result.EndTime = DateTime.Now
            result.Output = output.ToString()
            result.ErrorOutput = errorOutput.ToString()
            result.Command = phpExe & " -l <all *.php>"
            result.ExitCode = If(failed, If(firstExitCode = 0, 1, firstExitCode), 0)
            result.Success = Not failed

            If result.Success Then

                result.Message =
                    "PHP build/syntax check berhasil." &
                    Environment.NewLine &
                    "Files checked: " & checked.ToString() &
                    Environment.NewLine &
                    "Semua file PHP PASS."

            Else

                result.Message =
                    "PHP build/syntax check gagal." &
                    Environment.NewLine &
                    "Files checked: " & checked.ToString() &
                    Environment.NewLine &
                    "Ada file PHP yang memiliki syntax error."

            End If

            Return result

        End Function

        Private Function GetPhpFiles(projectPath As String) As List(Of String)

            Dim result As New List(Of String)()
            Dim ignored As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
                ".git",
                ".svn",
                ".vs",
                "bin",
                "obj",
                "node_modules",
                "vendor",
                "packages",
                "__pycache__",
                ".idea",
                ".gradle"
            }

            CollectPhpFiles(projectPath, result, ignored)
            result.Sort(StringComparer.OrdinalIgnoreCase)

            Return result

        End Function

        Private Sub CollectPhpFiles(
            directoryPath As String,
            result As List(Of String),
            ignored As HashSet(Of String))

            Dim directoryInfo As New DirectoryInfo(directoryPath)

            If ignored.Contains(directoryInfo.Name) Then
                Return
            End If

            Dim files() As String = Nothing

            Try
                files = Directory.GetFiles(directoryPath, "*.php", SearchOption.TopDirectoryOnly)
            Catch
                Return
            End Try

            For Each filePath As String In files
                result.Add(filePath)
            Next

            Dim directories() As String = Nothing

            Try
                directories = Directory.GetDirectories(directoryPath)
            Catch
                Return
            End Try

            For Each childDirectory As String In directories
                CollectPhpFiles(childDirectory, result, ignored)
            Next

        End Sub

        Private Function FindPhpExecutable() As String

            Dim candidates As New List(Of String)()

            ' XAMPP - lokasi utama yang umum digunakan di Windows.
            candidates.Add("C:\xampp\php\php.exe")
            candidates.Add("C:\xampp\php\php8\php.exe")
            candidates.Add("C:\xampp\php\php7\php.exe")

            ' Beberapa lokasi PHP Windows yang umum.
            candidates.Add(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "PHP\php.exe"))

            candidates.Add(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "PHP\php.exe"))

            For Each candidate As String In candidates

                If Not String.IsNullOrWhiteSpace(candidate) AndAlso
                   System.IO.File.Exists(candidate) Then

                    Return candidate

                End If

            Next

            ' Fallback ke PHP yang tersedia di PATH.
            If CanRunCommand("php.exe", "-v") Then
                Return "php.exe"
            End If

            Return Nothing

        End Function

        Private Function CanRunCommand(command As String, arguments As String) As Boolean

            Try

                Dim output As String = Nothing
                Dim errorOutput As String = Nothing

                Dim exitCode As Integer = RunProcess(
                    command,
                    arguments,
                    Environment.CurrentDirectory,
                    output,
                    errorOutput)

                Return exitCode = 0

            Catch
                Return False
            End Try

        End Function

        '==========================================================
        ' GENERIC BUILD DETECTION
        '==========================================================

        Private Sub DetectBuildCommand(
            projectPath As String,
            ByRef command As String,
            ByRef arguments As String,
            ByRef workingDirectory As String)

            command = Nothing
            arguments = Nothing
            workingDirectory = projectPath

            '==================================================
            ' Visual Studio / .NET Framework
            '==================================================

            Dim sln As String =
                Directory.GetFiles(
                    projectPath,
                    "*.sln",
                    SearchOption.TopDirectoryOnly).
                FirstOrDefault()

            If Not String.IsNullOrWhiteSpace(sln) Then

                Dim msbuild As String = FindMSBuild()

                If Not String.IsNullOrWhiteSpace(msbuild) Then

                    command = msbuild
                    arguments =
                        Quote(sln) &
                        " /t:Build" &
                        " /p:Configuration=Debug" &
                        " /p:Platform=AnyCPU" &
                        " /m"

                    Return

                End If

            End If

            '==================================================
            ' VB.NET
            '==================================================

            Dim vbproj As String =
                Directory.GetFiles(
                    projectPath,
                    "*.vbproj",
                    SearchOption.TopDirectoryOnly).
                FirstOrDefault()

            If vbproj IsNot Nothing Then

                Dim msbuild As String = FindMSBuild()

                If Not String.IsNullOrWhiteSpace(msbuild) Then

                    command = msbuild
                    arguments =
                        Quote(vbproj) &
                        " /t:Build" &
                        " /p:Configuration=Debug" &
                        " /p:Platform=AnyCPU" &
                        " /m"

                    Return

                End If

            End If

            '==================================================
            ' C#
            '==================================================

            Dim csproj As String =
                Directory.GetFiles(
                    projectPath,
                    "*.csproj",
                    SearchOption.TopDirectoryOnly).
                FirstOrDefault()

            If csproj IsNot Nothing Then

                Dim msbuild As String = FindMSBuild()

                If Not String.IsNullOrWhiteSpace(msbuild) Then

                    command = msbuild
                    arguments =
                        Quote(csproj) &
                        " /t:Build" &
                        " /p:Configuration=Debug" &
                        " /p:Platform=AnyCPU" &
                        " /m"

                    Return

                End If

            End If

            '==================================================
            ' Node.js
            '==================================================

            Dim packageJson As String =
                Path.Combine(projectPath, "package.json")

            If System.IO.File.Exists(packageJson) Then

                command = "npm"
                arguments = "run build"
                Return

            End If

            '==================================================
            ' PHP / Composer
            ' Jika PHP file tidak ada, composer project masih
            ' bisa dikenali di sini.
            '==================================================

            Dim composerJson As String =
                Path.Combine(projectPath, "composer.json")

            If System.IO.File.Exists(composerJson) Then

                command = "composer"
                arguments = "install --no-interaction"
                Return

            End If

            '==================================================
            ' Java Maven
            '==================================================

            Dim pom As String =
                Path.Combine(projectPath, "pom.xml")

            If System.IO.File.Exists(pom) Then

                Dim mvnw As String =
                    Path.Combine(projectPath, "mvnw.cmd")

                If System.IO.File.Exists(mvnw) Then
                    command = mvnw
                Else
                    command = "mvn"
                End If

                arguments = "clean package -DskipTests"
                Return

            End If

            '==================================================
            ' Gradle
            '==================================================

            Dim gradlew As String =
                Path.Combine(projectPath, "gradlew.bat")

            If System.IO.File.Exists(gradlew) Then

                command = gradlew
                arguments = "build"
                Return

            End If

            '==================================================
            ' CMake
            '==================================================

            Dim cmakeLists As String =
                Path.Combine(projectPath, "CMakeLists.txt")

            If System.IO.File.Exists(cmakeLists) Then

                command = "cmake"
                arguments =
                    "-S " & Quote(projectPath) &
                    " -B " & Quote(Path.Combine(projectPath, "build"))

                Return

            End If

            '==================================================
            ' Python
            '==================================================

            Dim pyproject As String =
                Path.Combine(projectPath, "pyproject.toml")

            Dim setupPy As String =
                Path.Combine(projectPath, "setup.py")

            If System.IO.File.Exists(pyproject) OrElse
               System.IO.File.Exists(setupPy) Then

                command = "python"
                arguments = "-m compileall ."
                Return

            End If

            Dim pythonFiles() As String =
                Directory.GetFiles(
                    projectPath,
                    "*.py",
                    SearchOption.TopDirectoryOnly)

            If pythonFiles.Length > 0 Then

                command = "python"
                arguments = "-m compileall ."
                Return

            End If

        End Sub

        '==========================================================
        ' PROCESS HELPERS
        '==========================================================

        Private Function RunProcess(
            command As String,
            arguments As String,
            workingDirectory As String,
            ByRef output As String,
            ByRef errorOutput As String) As Integer

            Dim outputBuilder As New StringBuilder()
            Dim errorBuilder As New StringBuilder()

            Using process As New Process()

                process.StartInfo = New ProcessStartInfo()
                process.StartInfo.FileName = command
                process.StartInfo.Arguments = arguments
                process.StartInfo.WorkingDirectory = workingDirectory
                process.StartInfo.UseShellExecute = False
                process.StartInfo.CreateNoWindow = True
                process.StartInfo.RedirectStandardOutput = True
                process.StartInfo.RedirectStandardError = True
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8

                AddHandler process.OutputDataReceived,
                    Sub(sender As Object, e As DataReceivedEventArgs)
                        If e.Data IsNot Nothing Then
                            outputBuilder.AppendLine(e.Data)
                        End If
                    End Sub

                AddHandler process.ErrorDataReceived,
                    Sub(sender As Object, e As DataReceivedEventArgs)
                        If e.Data IsNot Nothing Then
                            errorBuilder.AppendLine(e.Data)
                        End If
                    End Sub

                process.Start()
                process.BeginOutputReadLine()
                process.BeginErrorReadLine()
                process.WaitForExit()
                process.WaitForExit()

                output = outputBuilder.ToString()
                errorOutput = errorBuilder.ToString()

                Return process.ExitCode

            End Using

        End Function

        Private Sub AppendIndented(builder As StringBuilder, text As String)

            If builder Is Nothing OrElse String.IsNullOrWhiteSpace(text) Then
                Return
            End If

            Dim normalized As String =
                text.Replace(Microsoft.VisualBasic.vbCrLf, Microsoft.VisualBasic.vbLf).Replace(Microsoft.VisualBasic.vbCr, Microsoft.VisualBasic.vbLf)

            For Each line As String In normalized.Split(New String() {Microsoft.VisualBasic.vbLf}, StringSplitOptions.None)

                If line.Length > 0 Then
                    builder.AppendLine("    " & line)
                End If

            Next

        End Sub

        Private Function GetRelativePath(rootPath As String, fullPath As String) As String

            Try

                Dim root As String = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) &
                                     Path.DirectorySeparatorChar
                Dim full As String = Path.GetFullPath(fullPath)

                If full.StartsWith(root, StringComparison.OrdinalIgnoreCase) Then
                    Return full.Substring(root.Length)
                End If

            Catch
            End Try

            Return fullPath

        End Function

        Private Function FindMSBuild() As String

            Dim candidates As New List(Of String)()

            candidates.Add(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "MSBuild\14.0\Bin\MSBuild.exe"))

            candidates.Add(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "MSBuild\14.0\Bin\MSBuild.exe"))

            candidates.Add(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft Visual Studio 14.0\VC\bin\MSBuild.exe"))

            For Each item As String In candidates

                If System.IO.File.Exists(item) Then
                    Return item
                End If

            Next

            Return "MSBuild.exe"

        End Function

        Private Function Quote(value As String) As String

            If value Is Nothing Then
                Return """"""
            End If

            Return """" & value & """"

        End Function

    End Class

    Public Class BuildResult

        Public Property Success As Boolean
        Public Property ExitCode As Integer
        Public Property Command As String
        Public Property Output As String
        Public Property ErrorOutput As String
        Public Property Message As String
        Public Property StartTime As DateTime
        Public Property EndTime As DateTime

        Public ReadOnly Property Duration As TimeSpan

            Get

                If EndTime = DateTime.MinValue OrElse
                   StartTime = DateTime.MinValue Then

                    Return TimeSpan.Zero

                End If

                Return EndTime - StartTime

            End Get

        End Property

    End Class

End Namespace
