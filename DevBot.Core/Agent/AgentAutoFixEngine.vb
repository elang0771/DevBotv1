Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports DevBot.Core.AI
Imports DevBot.Core.Build
Imports DevBot.Core.File

Namespace DevBot.Core.Agent

    Public Class AgentAutoFixEngine

        Private ReadOnly buildEngine As AgentBuildEngine
        Private ReadOnly fileManager As FileManager
        Private ReadOnly aiProvider As IAIProvider

        Private ReadOnly ignoredDirectories As String() = {
            ".git", ".svn", ".vs", "bin", "obj", "node_modules",
            "packages", "vendor", "__pycache__", ".idea", ".gradle"
        }

        Public Sub New()
            buildEngine = New AgentBuildEngine()
            fileManager = New FileManager()
            aiProvider = New OpenAICompatibleProvider()
        End Sub

        Public Sub New(provider As IAIProvider)
            buildEngine = New AgentBuildEngine()
            fileManager = New FileManager()

            If provider Is Nothing Then
                aiProvider = New OpenAICompatibleProvider()
            Else
                aiProvider = provider
            End If
        End Sub

        Public Function Run(projectPath As String, maxIterations As Integer) As AgentAutoFixResult
            Dim result As New AgentAutoFixResult()

            If String.IsNullOrWhiteSpace(projectPath) Then
                result.Message = "Path project kosong."
                Return result
            End If

            Try
                projectPath = Path.GetFullPath(projectPath.Trim())
            Catch ex As Exception
                result.Message = "Path project tidak valid: " & ex.Message
                Return result
            End Try

            result.ProjectPath = projectPath

            If Not Directory.Exists(projectPath) Then
                result.Message = "Folder project tidak ditemukan: " & projectPath
                Return result
            End If

            If maxIterations < 1 Then maxIterations = 1
            If maxIterations > 10 Then maxIterations = 10

            For iterationNumber As Integer = 1 To maxIterations

                Dim iteration As New AgentAutoFixIteration()
                iteration.IterationNumber = iterationNumber
                result.Iterations.Add(iteration)

                Dim buildResult As AgentBuildResult = Nothing

                Try
                    buildResult = buildEngine.BuildProject(projectPath)
                Catch ex As Exception
                    iteration.Action = "Build engine error: " & ex.Message
                    result.Message = BuildFailureMessage(projectPath, iterationNumber, ex.Message)
                    Return result
                End Try

                If buildResult Is Nothing Then
                    iteration.Action = "Build engine tidak mengembalikan hasil."
                    result.Message = BuildFailureMessage(projectPath, iterationNumber, iteration.Action)
                    Return result
                End If

                iteration.BuildSuccess = buildResult.Success
                iteration.BuildOutput = If(buildResult.Output, String.Empty)
                iteration.BuildErrorOutput = If(buildResult.ErrorOutput, String.Empty)
                iteration.Errors = If(buildResult.Errors, New List(Of BuildError)())

                If buildResult.Success Then
                    result.Success = True
                    result.Message =
                        "AUTO FIX BERHASIL" & Environment.NewLine &
                        "=================" & Environment.NewLine & Environment.NewLine &
                        "Project : " & projectPath & Environment.NewLine &
                        "Status  : BUILD SUCCESS" & Environment.NewLine &
                        "Iterasi : " & iterationNumber
                    Return result
                End If

                If aiProvider Is Nothing OrElse Not aiProvider.IsConfigured() Then
                    iteration.Action = "AI belum dikonfigurasi."
                    result.Message = BuildFailureMessage(
                        projectPath,
                        iterationNumber,
                        "Build gagal tetapi AI belum dikonfigurasi.")
                    Return result
                End If

                Dim fixResult As AutoFixActionResult = ApplyAIFix(projectPath, buildResult)
                iteration.Action = fixResult.Message

                If fixResult.ChangedFile IsNot Nothing Then
                    iteration.ChangedFiles.Add(fixResult.ChangedFile)
                End If

                If Not fixResult.Success Then
                    result.Message = BuildFailureMessage(
                        projectPath,
                        iterationNumber,
                        fixResult.Message)
                    Return result
                End If
            Next

            result.Message = BuildFailureMessage(
                projectPath,
                maxIterations,
                "Batas maksimum iterasi tercapai." &
                Environment.NewLine & Environment.NewLine &
                BuildIterationDiagnostics(result.Iterations))

            Return result
        End Function

        Private Function ApplyAIFix(
            projectPath As String,
            buildResult As AgentBuildResult) As AutoFixActionResult

            Dim errorsText As String = BuildErrorsText(buildResult.Errors)

            If buildResult.Errors Is Nothing OrElse buildResult.Errors.Count = 0 Then
                errorsText = BuildRawBuildFailureText(buildResult)
            End If

            Dim sourceText As String = BuildRelevantSource(projectPath, buildResult.Errors)

            If String.IsNullOrWhiteSpace(sourceText) OrElse
               sourceText = "(source error tidak berhasil dibaca)" Then
                sourceText = BuildFallbackSource(projectPath)
            End If

            Dim aiResult As AIFixResponse

            Try
                aiResult = aiProvider.AnalyzeAndFix(
                    BuildSystemPrompt(),
                    BuildUserPrompt(projectPath, errorsText, sourceText))
            Catch ex As Exception
                Return AutoFixActionResult.Fail(
                    "AI exception [" & aiProvider.GetProviderName() & "]: " & ex.Message)
            End Try

            If aiResult Is Nothing Then
                Return AutoFixActionResult.Fail("AI tidak mengembalikan hasil.")
            End If

            If Not aiResult.Success Then
                Return AutoFixActionResult.Fail(
                    "AI GAGAL [" & aiProvider.GetProviderName() & "]: " &
                    If(String.IsNullOrWhiteSpace(aiResult.ErrorMessage),
                       "Tidak ada detail error dari provider.",
                       aiResult.ErrorMessage))
            End If

            If String.IsNullOrWhiteSpace(aiResult.FilePath) Then
                Return AutoFixActionResult.Fail("AI tidak menentukan file yang harus diperbaiki.")
            End If

            Dim targetPath As String = ResolveProjectPath(projectPath, aiResult.FilePath)

            If String.IsNullOrWhiteSpace(targetPath) Then
                Return AutoFixActionResult.Fail(
                    "Path hasil AI tidak valid atau berada di luar project: " &
                    aiResult.FilePath)
            End If

            If Not System.IO.File.Exists(targetPath) Then
                Return AutoFixActionResult.Fail(
                    "File target dari AI tidak ditemukan: " &
                    GetRelativePath(projectPath, targetPath))
            End If

            If fileManager.IsBinaryFile(Path.GetExtension(targetPath)) Then
                Return AutoFixActionResult.Fail("File target adalah binary dan tidak dapat diedit.")
            End If

            Dim originalContent As String = fileManager.ReadText(targetPath)

            If String.IsNullOrEmpty(aiResult.OldText) Then
                Return AutoFixActionResult.Fail("AI tidak memberikan OLD_TEXT.")
            End If

            Dim occurrenceCount As Integer =
                CountOccurrences(originalContent, aiResult.OldText)

            If occurrenceCount = 0 Then
                Return AutoFixActionResult.Fail(
                    "OLD_TEXT dari AI tidak ditemukan di file: " &
                    GetRelativePath(projectPath, targetPath))
            End If

            If occurrenceCount > 20 Then
                Return AutoFixActionResult.Fail(
                    "Perubahan ditolak karena OLD_TEXT cocok " &
                    occurrenceCount.ToString() &
                    " kali. AutoFix hanya menerima perubahan yang terkontrol.")
            End If

            Dim newContent As String =
                originalContent.Replace(aiResult.OldText, aiResult.NewText)

            If String.Equals(originalContent, newContent, StringComparison.Ordinal) Then
                Return AutoFixActionResult.Fail("AI tidak menghasilkan perubahan file.")
            End If

            Dim backupPath As String

            Try
                backupPath = CreateBackup(targetPath)
                fileManager.UpdateFile(targetPath, newContent)
            Catch ex As Exception
                Return AutoFixActionResult.Fail(
                    "Gagal menerapkan perubahan ke " &
                    GetRelativePath(projectPath, targetPath) &
                    ": " & ex.Message)
            End Try

            Dim changedFile As New AutoFixChangedFile()
            changedFile.FilePath = targetPath
            changedFile.RelativePath = GetRelativePath(projectPath, targetPath)
            changedFile.BackupPath = backupPath
            changedFile.OccurrenceCount = occurrenceCount
            changedFile.Explanation = If(aiResult.Explanation, String.Empty)
            changedFile.OldText = aiResult.OldText
            changedFile.NewText = aiResult.NewText

            Dim messageBuilder As New StringBuilder()
            messageBuilder.AppendLine("AI FIX BERHASIL")
            messageBuilder.AppendLine("File    : " & changedFile.RelativePath)
            messageBuilder.AppendLine("Backup  : " & backupPath)
            messageBuilder.AppendLine("Replace : " & occurrenceCount.ToString() & " occurrence")

            If Not String.IsNullOrWhiteSpace(changedFile.Explanation) Then
                messageBuilder.AppendLine("Reason  : " & changedFile.Explanation)
            End If

            messageBuilder.AppendLine("Provider: " & aiProvider.GetProviderName())

            Return AutoFixActionResult.SuccessResult(
                messageBuilder.ToString(),
                changedFile)
        End Function

        Private Function BuildSystemPrompt() As String
            Return "Anda adalah DevBot Coding Agent." & Environment.NewLine &
                   "Perbaiki error build secara aman." & Environment.NewLine &
                   "FILE wajib relative path di dalam project." & Environment.NewLine &
                   "OLD_BEGIN/OLD_END dan NEW_BEGIN/NEW_END wajib ada." & Environment.NewLine &
                   "OLD_TEXT harus benar-benar ada di source." & Environment.NewLine &
                   "Jangan gunakan markdown fence." & Environment.NewLine &
                   "Jangan mengembalikan seluruh project." & Environment.NewLine & Environment.NewLine &
                   "FORMAT RESPONSE:" & Environment.NewLine &
                   "FILE=relative/path/file.ext" & Environment.NewLine &
                   "OLD_BEGIN" & Environment.NewLine &
                   "kode lama yang tepat" & Environment.NewLine &
                   "OLD_END" & Environment.NewLine &
                   "NEW_BEGIN" & Environment.NewLine &
                   "kode baru yang tepat" & Environment.NewLine &
                   "NEW_END" & Environment.NewLine &
                   "EXPLANATION=penjelasan singkat"
        End Function

        Private Function BuildUserPrompt(
            projectPath As String,
            errorsText As String,
            sourceText As String) As String

            Return "PROJECT:" & Environment.NewLine & projectPath & Environment.NewLine &
                   Environment.NewLine &
                   "BUILD ERRORS:" & Environment.NewLine & errorsText & Environment.NewLine &
                   Environment.NewLine &
                   "RELEVANT SOURCE:" & Environment.NewLine & sourceText & Environment.NewLine &
                   Environment.NewLine &
                   "Analisa error dan lakukan satu perbaikan paling aman."
        End Function

        Private Function BuildErrorsText(errors As List(Of BuildError)) As String
            If errors Is Nothing OrElse errors.Count = 0 Then
                Return "(tidak ada error terstruktur)"
            End If

            Dim builder As New StringBuilder()

            For Each item As BuildError In errors
                If item Is Nothing Then Continue For

                builder.AppendLine("FILE: " & If(item.FilePath, String.Empty))
                builder.AppendLine("LINE: " & item.LineNumber.ToString())
                builder.AppendLine("COLUMN: " & item.ColumnNumber.ToString())
                builder.AppendLine("CODE: " & If(item.Code, String.Empty))
                builder.AppendLine("MESSAGE: " & If(item.Message, String.Empty))
                builder.AppendLine("RAW: " & If(item.RawLine, String.Empty))
                builder.AppendLine("-----")
            Next

            Return builder.ToString()
        End Function

        Private Function BuildRelevantSource(
            projectPath As String,
            errors As List(Of BuildError)) As String

            Dim builder As New StringBuilder()
            Dim added As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            If errors Is Nothing Then
                Return "(source tidak tersedia)"
            End If

            For Each item As BuildError In errors
                If item Is Nothing Then Continue For

                Dim fullPath As String =
                    ResolveProjectPath(projectPath, item.FilePath)

                If String.IsNullOrWhiteSpace(fullPath) Then Continue For
                If added.Contains(fullPath) Then Continue For
                If Not System.IO.File.Exists(fullPath) Then Continue For
                If fileManager.IsBinaryFile(Path.GetExtension(fullPath)) Then Continue For

                Try
                    Dim info As New FileInfo(fullPath)
                    If info.Length > 1048576L Then Continue For

                    builder.AppendLine(
                        "===== " & GetRelativePath(projectPath, fullPath) & " =====")
                    builder.AppendLine(
                        LimitSource(
                            fileManager.ReadText(fullPath),
                            item.LineNumber))
                    builder.AppendLine()

                    added.Add(fullPath)

                    If added.Count >= 8 Then Exit For
                Catch
                End Try
            Next

            If builder.Length = 0 Then
                Return "(source error tidak berhasil dibaca)"
            End If

            Return builder.ToString()
        End Function

        Private Function LimitSource(content As String, errorLine As Integer) As String
            If content Is Nothing Then Return String.Empty

            Dim lines() As String =
                content.Replace(Microsoft.VisualBasic.vbCrLf, Microsoft.VisualBasic.vbLf).
                Replace(Microsoft.VisualBasic.vbCr, Microsoft.VisualBasic.vbLf).
                Split(New Char() {Microsoft.VisualBasic.ChrW(10)})

            If lines.Length <= 400 Then Return content

            Dim center As Integer = errorLine - 1
            If center < 0 Then center = 0
            If center >= lines.Length Then center = lines.Length - 1

            Dim startLine As Integer = Math.Max(0, center - 120)
            Dim endLine As Integer = Math.Min(lines.Length - 1, center + 120)

            Dim builder As New StringBuilder()

            For index As Integer = startLine To endLine
                builder.AppendLine((index + 1).ToString() & ": " & lines(index))
            Next

            Return builder.ToString()
        End Function

        Private Function ResolveProjectPath(
            projectPath As String,
            relativeOrFullPath As String) As String

            If String.IsNullOrWhiteSpace(projectPath) OrElse
               String.IsNullOrWhiteSpace(relativeOrFullPath) Then
                Return String.Empty
            End If

            Try
                Dim root As String = Path.GetFullPath(projectPath.Trim())

                Dim candidate As String =
                    SanitizeCompilerFilePath(relativeOrFullPath)

                If String.IsNullOrWhiteSpace(candidate) Then
                    Return String.Empty
                End If

                candidate = candidate.Replace("/"c, "\"c).Trim()

                Dim candidateFullPath As String

                If Path.IsPathRooted(candidate) Then
                    candidateFullPath = Path.GetFullPath(candidate)
                Else
                    candidateFullPath = Path.GetFullPath(
                        Path.Combine(root, candidate))
                End If

                If Not IsInsideProject(root, candidateFullPath) Then
                    Return String.Empty
                End If

                Return candidateFullPath

            Catch
                Return String.Empty
            End Try
        End Function

        Private Function SanitizeCompilerFilePath(value As String) As String
            If String.IsNullOrWhiteSpace(value) Then Return String.Empty

            Dim candidate As String = value.Trim()

            candidate = candidate.Trim(""""c, "'"c)
            candidate = Regex.Replace(
                candidate,
                "\s*\(\s*\d+\s*(?:,\s*\d+)?\s*\)\s*:?.*$",
                String.Empty)

            candidate = candidate.Trim()
            candidate = candidate.Trim(""""c, "'"c)

            Dim errorMarker As Integer =
                candidate.IndexOf("):", StringComparison.Ordinal)

            If errorMarker >= 0 Then
                candidate = candidate.Substring(0, errorMarker + 1)
                candidate = candidate.TrimEnd(")"c)
            End If

            If candidate.StartsWith("file://", StringComparison.OrdinalIgnoreCase) Then
                Try
                    candidate = New Uri(candidate).LocalPath
                Catch
                    Return String.Empty
                End Try
            End If

            candidate = candidate.Replace("/"c, "\"c)

            If candidate.IndexOfAny(Path.GetInvalidPathChars()) >= 0 Then
                Dim cleaned As New StringBuilder(candidate.Length)

                For Each ch As Char In candidate
                    If Array.IndexOf(Path.GetInvalidPathChars(), ch) < 0 Then
                        cleaned.Append(ch)
                    End If
                Next

                candidate = cleaned.ToString()
            End If

            Return candidate.Trim()
        End Function

        Private Function IsInsideProject(projectPath As String, filePath As String) As Boolean
            Try
                Dim root As String =
                    Path.GetFullPath(projectPath).TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar) &
                    Path.DirectorySeparatorChar

                Dim target As String = Path.GetFullPath(filePath)

                Return target.StartsWith(
                    root,
                    StringComparison.OrdinalIgnoreCase)
            Catch
                Return False
            End Try
        End Function

        Private Function GetRelativePath(
            projectPath As String,
            filePath As String) As String

            Try
                Dim root As String =
                    Path.GetFullPath(projectPath).TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar) &
                    Path.DirectorySeparatorChar

                Dim fullPath As String = Path.GetFullPath(filePath)

                If fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) Then
                    Return fullPath.Substring(root.Length).
                        Replace(Path.DirectorySeparatorChar, "/"c)
                End If

                Return fullPath
            Catch
                Return filePath
            End Try
        End Function

        Private Function CreateBackup(filePath As String) As String
            Dim directoryPath As String = Path.GetDirectoryName(filePath)

            If String.IsNullOrWhiteSpace(directoryPath) Then
                Throw New IOException("Folder file target tidak valid.")
            End If

            Dim backupDirectory As String =
                Path.Combine(directoryPath, ".devbot-backup")

            If Not Directory.Exists(backupDirectory) Then
                Directory.CreateDirectory(backupDirectory)
            End If

            Dim fileName As String = Path.GetFileName(filePath)

            Dim backupPath As String =
                Path.Combine(
                    backupDirectory,
                    fileName & "." &
                    DateTime.Now.ToString("yyyyMMdd_HHmmssfff") &
                    ".bak")

            System.IO.File.Copy(filePath, backupPath, True)

            Return backupPath
        End Function

        Private Function CountOccurrences(text As String, value As String) As Integer
            If String.IsNullOrEmpty(text) OrElse String.IsNullOrEmpty(value) Then
                Return 0
            End If

            Dim count As Integer = 0
            Dim startIndex As Integer = 0

            While True
                Dim index As Integer =
                    text.IndexOf(value, startIndex, StringComparison.Ordinal)

                If index < 0 Then Exit While

                count += 1
                startIndex = index + value.Length

                If startIndex >= text.Length Then Exit While
            End While

            Return count
        End Function

        Private Function BuildRawBuildFailureText(
            buildResult As AgentBuildResult) As String

            Dim builder As New StringBuilder()

            builder.AppendLine("BUILD FAILED")
            builder.AppendLine("============")

            If buildResult Is Nothing Then
                builder.AppendLine("BuildResult kosong.")
                Return builder.ToString()
            End If

            builder.AppendLine("ExitCode: " & buildResult.ExitCode.ToString())

            If Not String.IsNullOrWhiteSpace(buildResult.Command) Then
                builder.AppendLine("Command: " & buildResult.Command)
            End If

            If Not String.IsNullOrWhiteSpace(buildResult.Output) Then
                builder.AppendLine("STDOUT:")
                builder.AppendLine(LimitDiagnosticText(buildResult.Output))
            End If

            If Not String.IsNullOrWhiteSpace(buildResult.ErrorOutput) Then
                builder.AppendLine("STDERR:")
                builder.AppendLine(LimitDiagnosticText(buildResult.ErrorOutput))
            End If

            Return builder.ToString()
        End Function

        Private Function LimitDiagnosticText(value As String) As String
            If String.IsNullOrWhiteSpace(value) Then Return String.Empty

            Dim text As String = value.Trim()

            If text.Length <= 12000 Then Return text

            Return text.Substring(0, 12000) &
                   Environment.NewLine &
                   "... [output dipotong oleh DevBot]"
        End Function

        Private Function BuildFallbackSource(projectPath As String) As String
            Dim builder As New StringBuilder()
            Dim count As Integer = 0

            Dim extensions As String() = {
                "*.vb", "*.cs", "*.php", "*.js", "*.ts", "*.py",
                "*.java", "*.kt", "*.html", "*.css", "*.json", "*.xml"
            }

            For Each pattern As String In extensions
                Dim files() As String = Nothing

                Try
                    files = Directory.GetFiles(
                        projectPath,
                        pattern,
                        SearchOption.AllDirectories)
                Catch
                    Continue For
                End Try

                For Each fullPath As String In files
                    If IsIgnoredPath(fullPath) Then Continue For
                    If fileManager.IsBinaryFile(Path.GetExtension(fullPath)) Then Continue For

                    Try
                        Dim info As New FileInfo(fullPath)
                        If info.Length > 1048576L Then Continue For

                        builder.AppendLine(
                            "===== " & GetRelativePath(projectPath, fullPath) & " =====")
                        builder.AppendLine(fileManager.ReadText(fullPath))
                        builder.AppendLine()

                        count += 1
                        If count >= 8 Then Return builder.ToString()
                    Catch
                    End Try
                Next
            Next

            If count = 0 Then
                Return "(tidak ada source yang dapat dibaca)"
            End If

            Return builder.ToString()
        End Function

        Private Function IsIgnoredPath(filePath As String) As Boolean
            If String.IsNullOrWhiteSpace(filePath) Then Return False

            Dim normalized As String =
                filePath.Replace(
                    Path.AltDirectorySeparatorChar,
                    Path.DirectorySeparatorChar)

            For Each directoryName As String In ignoredDirectories
                Dim token As String =
                    Path.DirectorySeparatorChar &
                    directoryName &
                    Path.DirectorySeparatorChar

                If normalized.IndexOf(
                    token,
                    StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Return True
                End If
            Next

            Return False
        End Function

        Private Function BuildIterationDiagnostics(
            iterations As List(Of AgentAutoFixIteration)) As String

            If iterations Is Nothing OrElse iterations.Count = 0 Then
                Return "Tidak ada detail iterasi."
            End If

            Dim builder As New StringBuilder()
            builder.AppendLine("DETAIL ITERASI")
            builder.AppendLine("--------------")

            For Each item As AgentAutoFixIteration In iterations
                If item Is Nothing Then Continue For

                builder.AppendLine(
                    "Iterasi " & item.IterationNumber.ToString() &
                    " | Build=" &
                    If(item.BuildSuccess, "SUCCESS", "FAIL"))

                If Not String.IsNullOrWhiteSpace(item.Action) Then
                    builder.AppendLine("Action: " & item.Action)
                End If

                builder.AppendLine(
                    "Errors: " &
                    If(item.Errors Is Nothing, 0, item.Errors.Count).ToString())
                builder.AppendLine()
            Next

            Return builder.ToString().TrimEnd()
        End Function

        Private Function BuildFailureMessage(
            projectPath As String,
            iteration As Integer,
            reason As String) As String

            Return "AUTO FIX GAGAL" & Environment.NewLine &
                   "===============" & Environment.NewLine & Environment.NewLine &
                   "Project : " & projectPath & Environment.NewLine &
                   "Iterasi : " & iteration.ToString() & Environment.NewLine &
                   "Reason  : " & reason
        End Function

    End Class

    Public Class AgentAutoFixResult
        Public Property Success As Boolean
        Public Property ProjectPath As String
        Public Property Message As String
        Public Property Iterations As List(Of AgentAutoFixIteration)

        Public Sub New()
            Iterations = New List(Of AgentAutoFixIteration)()
        End Sub
    End Class

    Public Class AgentAutoFixIteration
        Public Property IterationNumber As Integer
        Public Property BuildSuccess As Boolean
        Public Property BuildOutput As String
        Public Property BuildErrorOutput As String
        Public Property Errors As List(Of BuildError)
        Public Property Action As String
        Public Property ChangedFiles As List(Of AutoFixChangedFile)

        Public Sub New()
            Errors = New List(Of BuildError)()
            ChangedFiles = New List(Of AutoFixChangedFile)()
            BuildOutput = String.Empty
            BuildErrorOutput = String.Empty
            Action = String.Empty
        End Sub
    End Class

    Public Class AutoFixChangedFile
        Public Property FilePath As String
        Public Property RelativePath As String
        Public Property BackupPath As String
        Public Property OccurrenceCount As Integer
        Public Property Explanation As String
        Public Property OldText As String
        Public Property NewText As String
    End Class

    Public Class AutoFixActionResult
        Public Property Success As Boolean
        Public Property Message As String
        Public Property ChangedFile As AutoFixChangedFile

        Public Shared Function Fail(message As String) As AutoFixActionResult
            Return New AutoFixActionResult With {
                .Success = False,
                .Message = message
            }
        End Function

        Public Shared Function SuccessResult(
            message As String,
            changedFile As AutoFixChangedFile) As AutoFixActionResult

            Return New AutoFixActionResult With {
                .Success = True,
                .Message = message,
                .ChangedFile = changedFile
            }
        End Function
    End Class

End Namespace
