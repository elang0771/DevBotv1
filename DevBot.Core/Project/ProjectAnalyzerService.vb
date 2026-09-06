Imports System
Imports System.Collections.Generic
Imports System.IO
Imports DevBot.Models

Namespace DevBot.Core.Project

    Public Class ProjectAnalyzerService

        Private ReadOnly ignoredFolders As HashSet(Of String)

        Public Sub New()

            ignoredFolders =
                New HashSet(Of String)(
                    StringComparer.OrdinalIgnoreCase)

            ignoredFolders.Add(".git")
            ignoredFolders.Add(".vs")
            ignoredFolders.Add("bin")
            ignoredFolders.Add("obj")
            ignoredFolders.Add("node_modules")
            ignoredFolders.Add("vendor")

        End Sub

        Public Function AnalyzeProject(
            projectPath As String) As ProjectInfo

            If String.IsNullOrWhiteSpace(projectPath) Then

                Throw New ArgumentException(
                    "Path project tidak boleh kosong.",
                    "projectPath")

            End If

            If Not System.IO.Directory.Exists(
                projectPath) Then

                Throw New DirectoryNotFoundException(
                    "Folder project tidak ditemukan: " &
                    projectPath)

            End If

            Dim fullPath As String =
                System.IO.Path.GetFullPath(
                    projectPath)

            Dim info As New ProjectInfo()

            Dim directoryInfo As New DirectoryInfo(
                fullPath)

            info.Name =
                directoryInfo.Name

            info.RootPath =
                directoryInfo.FullName

            info.ProjectType =
                DetectProjectType(
                    fullPath)

            ScanDirectory(
                fullPath,
                info)

            Return info

        End Function

        Private Sub ScanDirectory(
            directoryPath As String,
            info As ProjectInfo)

            Dim directoryInfo As New DirectoryInfo(
                directoryPath)

            For Each fileInfo As FileInfo In
                directoryInfo.GetFiles()

                Dim relativePath As String =
                    GetRelativePath(
                        info.RootPath,
                        fileInfo.FullName)

                info.Files.Add(
                    relativePath)

            Next

            For Each childDirectory As DirectoryInfo In
                directoryInfo.GetDirectories()

                If ignoredFolders.Contains(
                    childDirectory.Name) Then

                    Continue For

                End If

                Dim relativeDirectory As String =
                    GetRelativePath(
                        info.RootPath,
                        childDirectory.FullName)

                info.Folders.Add(
                    relativeDirectory)

                ScanDirectory(
                    childDirectory.FullName,
                    info)

            Next

        End Sub

        Private Function DetectProjectType(
            projectPath As String) As String

            If System.IO.File.Exists(
                System.IO.Path.Combine(
                    projectPath,
                    "composer.json")) Then

                Return "PHP / Composer"

            End If

            If System.IO.File.Exists(
                System.IO.Path.Combine(
                    projectPath,
                    "package.json")) Then

                Return "JavaScript / Node.js"

            End If

            If System.IO.File.Exists(
                System.IO.Path.Combine(
                    projectPath,
                    "pom.xml")) Then

                Return "Java / Maven"

            End If

            If System.IO.File.Exists(
                System.IO.Path.Combine(
                    projectPath,
                    "build.gradle")) OrElse
               System.IO.File.Exists(
                   System.IO.Path.Combine(
                       projectPath,
                       "build.gradle.kts")) Then

                Return "Java / Gradle"

            End If

            If System.IO.Directory.GetFiles(
                projectPath,
                "*.sln",
                SearchOption.TopDirectoryOnly).Length > 0 Then

                Return "Visual Studio Solution"

            End If

            If System.IO.Directory.GetFiles(
                projectPath,
                "*.csproj",
                SearchOption.AllDirectories).Length > 0 Then

                Return "C# / .NET"

            End If

            If System.IO.Directory.GetFiles(
                projectPath,
                "*.vbproj",
                SearchOption.AllDirectories).Length > 0 Then

                Return "VB.NET / .NET"

            End If

            If System.IO.File.Exists(
                System.IO.Path.Combine(
                    projectPath,
                    "requirements.txt")) Then

                Return "Python"

            End If

            If System.IO.Directory.GetFiles(
                projectPath,
                "*.py",
                SearchOption.AllDirectories).Length > 0 Then

                Return "Python"

            End If

            If System.IO.Directory.GetFiles(
                projectPath,
                "*.php",
                SearchOption.AllDirectories).Length > 0 Then

                Return "PHP"

            End If

            If System.IO.Directory.GetFiles(
                projectPath,
                "*.java",
                SearchOption.AllDirectories).Length > 0 Then

                Return "Java"

            End If

            If System.IO.Directory.GetFiles(
                projectPath,
                "*.js",
                SearchOption.AllDirectories).Length > 0 Then

                Return "JavaScript"

            End If

            Return "Unknown"

        End Function

        Private Function GetRelativePath(
            rootPath As String,
            fullPath As String) As String

            Dim rootUri As New Uri(
                EnsureTrailingSeparator(
                    rootPath))

            Dim fileUri As New Uri(
                fullPath)

            Dim relativeUri As Uri =
                rootUri.MakeRelativeUri(
                    fileUri)

            Return Uri.UnescapeDataString(
                relativeUri.ToString().Replace(
                    "/"c,
                    System.IO.Path.DirectorySeparatorChar))

        End Function

        Private Function EnsureTrailingSeparator(
            path As String) As String

            If path.EndsWith(
                System.IO.Path.DirectorySeparatorChar.ToString(),
                StringComparison.Ordinal) Then

                Return path

            End If

            Return path &
                   System.IO.Path.DirectorySeparatorChar

        End Function

    End Class

End Namespace