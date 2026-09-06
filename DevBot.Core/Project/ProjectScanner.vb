Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq

Namespace DevBot.Core.Project

    Public Class ProjectScanner

        Private Shared ReadOnly IgnoredDirectories As String() = {
            ".git",
            ".svn",
            ".vs",
            "bin",
            "obj",
            "node_modules",
            "packages",
            "vendor",
            "__pycache__",
            ".idea",
            ".gradle"
        }

        Private Shared ReadOnly SupportedProjectFiles As String() = {
            "*.sln",
            "*.vbproj",
            "*.csproj",
            "*.fsproj",
            "*.vcxproj",
            "package.json",
            "composer.json",
            "pom.xml",
            "build.gradle",
            "build.gradle.kts",
            "settings.gradle",
            "settings.gradle.kts",
            "CMakeLists.txt",
            "requirements.txt",
            "pyproject.toml",
            "setup.py"
        }

        Public Function Scan(
            projectPath As String) As ProjectScanResult

            If String.IsNullOrWhiteSpace(projectPath) Then
                Throw New ArgumentException(
                    "Path project tidak boleh kosong.",
                    NameOf(projectPath))
            End If

            Dim fullPath As String =
                Path.GetFullPath(projectPath)

            If Not Directory.Exists(fullPath) Then
                Throw New DirectoryNotFoundException(
                    "Folder project tidak ditemukan: " &
                    fullPath)
            End If

            Dim result As New ProjectScanResult()

            result.ProjectPath =
                fullPath

            result.ProjectName =
                New DirectoryInfo(fullPath).Name

            ScanDirectory(
                fullPath,
                result.Root,
                result)

            DetectProjectTypes(
                result)

            Return result

        End Function

        Private Sub ScanDirectory(
            directoryPath As String,
            parentNode As ProjectTreeNode,
            result As ProjectScanResult)

            Dim directoryInfo As New DirectoryInfo(
                directoryPath)

            Dim directories() As DirectoryInfo

            Try

                directories =
                    directoryInfo.GetDirectories()

            Catch ex As UnauthorizedAccessException

                Return

            Catch ex As IOException

                Return

            End Try

            For Each childDirectory As DirectoryInfo In
                directories.OrderBy(
                    Function(x) x.Name)

                If ShouldIgnoreDirectory(
                    childDirectory.Name) Then

                    Continue For

                End If

                Dim directoryNode As New ProjectTreeNode()

                directoryNode.Name =
                    childDirectory.Name

                directoryNode.FullPath =
                    childDirectory.FullName

                directoryNode.IsDirectory =
                    True

                directoryNode.Extension =
                    String.Empty

                parentNode.Children.Add(
                    directoryNode)

                ScanDirectory(
                    childDirectory.FullName,
                    directoryNode,
                    result)

            Next

            Dim files() As FileInfo

            Try

                files =
                    directoryInfo.GetFiles()

            Catch ex As UnauthorizedAccessException

                Return

            Catch ex As IOException

                Return

            End Try

            For Each fileInfo As FileInfo In
                files.OrderBy(
                    Function(x) x.Name)

                Dim fileNode As New ProjectTreeNode()

                fileNode.Name =
                    fileInfo.Name

                fileNode.FullPath =
                    fileInfo.FullName

                fileNode.IsDirectory =
                    False

                fileNode.Extension =
                    fileInfo.Extension

                fileNode.Size =
                    fileInfo.Length

                parentNode.Children.Add(
                    fileNode)

                result.TotalFiles +=
                    1

            Next

            result.TotalDirectories +=
                directories.Count(
                    Function(x)
                        Return Not ShouldIgnoreDirectory(
                            x.Name)
                    End Function)

        End Sub

        Private Function ShouldIgnoreDirectory(
            directoryName As String) As Boolean

            If String.IsNullOrWhiteSpace(
                directoryName) Then

                Return True

            End If

            For Each ignoredName As String In
                IgnoredDirectories

                If String.Equals(
                    directoryName,
                    ignoredName,
                    StringComparison.OrdinalIgnoreCase) Then

                    Return True

                End If

            Next

            Return False

        End Function

        Private Sub DetectProjectTypes(
            result As ProjectScanResult)

            Dim projectFiles As New List(Of String)()

            FindProjectFiles(
                result.ProjectPath,
                projectFiles)

            For Each projectFile As String In
                projectFiles

                Dim fileName As String =
                    Path.GetFileName(
                        projectFile)

                Dim extension As String =
                    Path.GetExtension(
                        projectFile).ToLowerInvariant()

                If extension = ".sln" Then

                    AddProjectType(
                        result,
                        "Visual Studio Solution")

                ElseIf extension = ".vbproj" Then

                    AddProjectType(
                        result,
                        "VB.NET")

                ElseIf extension = ".csproj" Then

                    AddProjectType(
                        result,
                        "C#")

                ElseIf extension = ".fsproj" Then

                    AddProjectType(
                        result,
                        "F#")

                ElseIf extension = ".vcxproj" Then

                    AddProjectType(
                        result,
                        "C++")

                ElseIf String.Equals(
                    fileName,
                    "package.json",
                    StringComparison.OrdinalIgnoreCase) Then

                    AddProjectType(
                        result,
                        "Node.js / JavaScript")

                ElseIf String.Equals(
                    fileName,
                    "composer.json",
                    StringComparison.OrdinalIgnoreCase) Then

                    AddProjectType(
                        result,
                        "PHP / Composer")

                ElseIf String.Equals(
                    fileName,
                    "pom.xml",
                    StringComparison.OrdinalIgnoreCase) Then

                    AddProjectType(
                        result,
                        "Java / Maven")

                ElseIf String.Equals(
                    fileName,
                    "build.gradle",
                    StringComparison.OrdinalIgnoreCase) OrElse
                    String.Equals(
                        fileName,
                        "build.gradle.kts",
                        StringComparison.OrdinalIgnoreCase) Then

                    AddProjectType(
                        result,
                        "Java / Gradle")

                ElseIf String.Equals(
                    fileName,
                    "CMakeLists.txt",
                    StringComparison.OrdinalIgnoreCase) Then

                    AddProjectType(
                        result,
                        "CMake / C++")

                ElseIf String.Equals(
                    fileName,
                    "requirements.txt",
                    StringComparison.OrdinalIgnoreCase) OrElse
                    String.Equals(
                        fileName,
                        "pyproject.toml",
                        StringComparison.OrdinalIgnoreCase) OrElse
                    String.Equals(
                        fileName,
                        "setup.py",
                        StringComparison.OrdinalIgnoreCase) Then

                    AddProjectType(
                        result,
                        "Python")

                End If

            Next

        End Sub

        Private Sub FindProjectFiles(
            directoryPath As String,
            projectFiles As List(Of String))

            Dim directoryInfo As New DirectoryInfo(
                directoryPath)

            Dim directories() As DirectoryInfo

            Try

                directories =
                    directoryInfo.GetDirectories()

            Catch

                Return

            End Try

            For Each childDirectory As DirectoryInfo In
                directories

                If ShouldIgnoreDirectory(
                    childDirectory.Name) Then

                    Continue For

                End If

                FindProjectFiles(
                    childDirectory.FullName,
                    projectFiles)

            Next

            Dim files() As FileInfo

            Try

                files =
                    directoryInfo.GetFiles()

            Catch

                Return

            End Try

            For Each fileInfo As FileInfo In
                files

                If IsProjectFile(
                    fileInfo.Name) Then

                    projectFiles.Add(
                        fileInfo.FullName)

                End If

            Next

        End Sub

        Private Function IsProjectFile(
            fileName As String) As Boolean

            If String.IsNullOrWhiteSpace(
                fileName) Then

                Return False

            End If

            Dim lowerName As String =
                fileName.ToLowerInvariant()

            If lowerName.EndsWith(
                ".sln") OrElse
               lowerName.EndsWith(
                ".vbproj") OrElse
               lowerName.EndsWith(
                ".csproj") OrElse
               lowerName.EndsWith(
                ".fsproj") OrElse
               lowerName.EndsWith(
                ".vcxproj") Then

                Return True

            End If

            Select Case lowerName

                Case "package.json",
                     "composer.json",
                     "pom.xml",
                     "build.gradle",
                     "build.gradle.kts",
                     "settings.gradle",
                     "settings.gradle.kts",
                     "cmakelists.txt",
                     "requirements.txt",
                     "pyproject.toml",
                     "setup.py"

                    Return True

            End Select

            Return False

        End Function

        Private Sub AddProjectType(
            result As ProjectScanResult,
            projectType As String)

            If result.ProjectTypes.Contains(
                projectType) Then

                Return

            End If

            result.ProjectTypes.Add(
                projectType)

        End Sub

        Public Function GetProjectFiles(
            projectPath As String) As List(Of String)

            If String.IsNullOrWhiteSpace(
                projectPath) Then

                Throw New ArgumentException(
                    "Path project tidak boleh kosong.",
                    NameOf(projectPath))

            End If

            Dim fullPath As String =
                Path.GetFullPath(projectPath)

            If Not Directory.Exists(
                fullPath) Then

                Throw New DirectoryNotFoundException(
                    "Folder project tidak ditemukan: " &
                    fullPath)

            End If

            Dim files As New List(Of String)()

            CollectAllFiles(
                fullPath,
                files)

            Return files

        End Function

        Private Sub CollectAllFiles(
            directoryPath As String,
            files As List(Of String))

            Dim directoryInfo As New DirectoryInfo(
                directoryPath)

            Dim childDirectories() As DirectoryInfo

            Try

                childDirectories =
                    directoryInfo.GetDirectories()

            Catch

                Return

            End Try

            For Each childDirectory As DirectoryInfo In
                childDirectories

                If ShouldIgnoreDirectory(
                    childDirectory.Name) Then

                    Continue For

                End If

                CollectAllFiles(
                    childDirectory.FullName,
                    files)

            Next

            Dim childFiles() As FileInfo

            Try

                childFiles =
                    directoryInfo.GetFiles()

            Catch

                Return

            End Try

            For Each fileInfo As FileInfo In
                childFiles

                files.Add(
                    fileInfo.FullName)

            Next

        End Sub

    End Class

    Public Class ProjectScanResult

        Public Sub New()

            ProjectTypes =
                New List(Of String)()

            Root =
                New ProjectTreeNode()

        End Sub

        Public Property ProjectPath As String

        Public Property ProjectName As String

        Public Property TotalFiles As Integer

        Public Property TotalDirectories As Integer

        Public Property ProjectTypes As List(Of String)

        Public Property Root As ProjectTreeNode

        Public Function HasProjectType(
            projectType As String) As Boolean

            If ProjectTypes Is Nothing Then
                Return False
            End If

            Return ProjectTypes.Contains(
                projectType)

        End Function

    End Class

    Public Class ProjectTreeNode

        Public Sub New()

            Children =
                New List(Of ProjectTreeNode)()

        End Sub

        Public Property Name As String

        Public Property FullPath As String

        Public Property IsDirectory As Boolean

        Public Property Extension As String

        Public Property Size As Long

        Public Property Children As List(Of ProjectTreeNode)

    End Class

End Namespace