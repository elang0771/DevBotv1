Imports System
Imports System.IO

Namespace DevBot.Core.File

    Public Class BackupService

        Private ReadOnly backupRoot As String

        Public Sub New()

            backupRoot =
                System.IO.Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "DevBot",
                    "Backups")

            If Not System.IO.Directory.Exists(
                backupRoot) Then

                System.IO.Directory.CreateDirectory(
                    backupRoot)

            End If

        End Sub

        Public Function CreateBackup(
            filePath As String) As String

            ValidateFilePath(filePath)

            If Not System.IO.File.Exists(
                filePath) Then

                Throw New FileNotFoundException(
                    "File yang akan di-backup tidak ditemukan.",
                    filePath)

            End If

            Dim fullFilePath As String =
                System.IO.Path.GetFullPath(
                    filePath)

            Dim projectRoot As String =
                FindProjectRoot(
                    System.IO.Path.GetDirectoryName(
                        fullFilePath))

            Dim backupSession As String =
                DateTime.Now.ToString(
                    "yyyyMMdd_HHmmss_fff")

            Dim sessionFolder As String =
                System.IO.Path.Combine(
                    backupRoot,
                    backupSession)

            System.IO.Directory.CreateDirectory(
                sessionFolder)

            Dim relativePath As String =
                GetRelativePath(
                    projectRoot,
                    fullFilePath)

            Dim backupPath As String =
                System.IO.Path.Combine(
                    sessionFolder,
                    relativePath)

            Dim backupDirectory As String =
                System.IO.Path.GetDirectoryName(
                    backupPath)

            If Not String.IsNullOrWhiteSpace(
                backupDirectory) Then

                System.IO.Directory.CreateDirectory(
                    backupDirectory)

            End If

            System.IO.File.Copy(
                fullFilePath,
                backupPath,
                True)

            Return backupPath

        End Function

        Public Function CreateBackup(
            filePath As String,
            sessionName As String) As String

            ValidateFilePath(filePath)

            If Not System.IO.File.Exists(
                filePath) Then

                Throw New FileNotFoundException(
                    "File yang akan di-backup tidak ditemukan.",
                    filePath)

            End If

            If String.IsNullOrWhiteSpace(
                sessionName) Then

                sessionName =
                    DateTime.Now.ToString(
                        "yyyyMMdd_HHmmss_fff")

            End If

            Dim safeSessionName As String =
                MakeSafeFolderName(
                    sessionName)

            Dim sessionFolder As String =
                System.IO.Path.Combine(
                    backupRoot,
                    safeSessionName)

            System.IO.Directory.CreateDirectory(
                sessionFolder)

            Dim fullFilePath As String =
                System.IO.Path.GetFullPath(
                    filePath)

            Dim projectRoot As String =
                FindProjectRoot(
                    System.IO.Path.GetDirectoryName(
                        fullFilePath))

            Dim relativePath As String =
                GetRelativePath(
                    projectRoot,
                    fullFilePath)

            Dim backupPath As String =
                System.IO.Path.Combine(
                    sessionFolder,
                    relativePath)

            Dim backupDirectory As String =
                System.IO.Path.GetDirectoryName(
                    backupPath)

            If Not String.IsNullOrWhiteSpace(
                backupDirectory) Then

                System.IO.Directory.CreateDirectory(
                    backupDirectory)

            End If

            System.IO.File.Copy(
                fullFilePath,
                backupPath,
                True)

            Return backupPath

        End Function

        Public Function BackupExists(
            backupPath As String) As Boolean

            If String.IsNullOrWhiteSpace(
                backupPath) Then

                Return False

            End If

            Return System.IO.File.Exists(
                backupPath)

        End Function

        Public Function RestoreBackup(
            backupPath As String,
            originalFilePath As String) As Boolean

            If String.IsNullOrWhiteSpace(
                backupPath) Then

                Throw New ArgumentException(
                    "Path backup tidak boleh kosong.",
                    "backupPath")

            End If

            If String.IsNullOrWhiteSpace(
                originalFilePath) Then

                Throw New ArgumentException(
                    "Path file asli tidak boleh kosong.",
                    "originalFilePath")

            End If

            If Not System.IO.File.Exists(
                backupPath) Then

                Throw New FileNotFoundException(
                    "File backup tidak ditemukan.",
                    backupPath)

            End If

            Dim originalDirectory As String =
                System.IO.Path.GetDirectoryName(
                    System.IO.Path.GetFullPath(
                        originalFilePath))

            If Not String.IsNullOrWhiteSpace(
                originalDirectory) Then

                System.IO.Directory.CreateDirectory(
                    originalDirectory)

            End If

            System.IO.File.Copy(
                backupPath,
                originalFilePath,
                True)

            Return True

        End Function

        Public Function GetBackupRoot() As String

            Return backupRoot

        End Function

        Private Sub ValidateFilePath(
            filePath As String)

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Throw New ArgumentException(
                    "Path file tidak boleh kosong.",
                    "filePath")

            End If

        End Sub

        Private Function FindProjectRoot(
            startDirectory As String) As String

            If String.IsNullOrWhiteSpace(
                startDirectory) Then

                Return System.IO.Path.GetDirectoryName(
                    System.IO.Path.GetFullPath(
                        "dummy"))

            End If

            Dim currentDirectory As DirectoryInfo =
                New DirectoryInfo(
                    startDirectory)

            Do While currentDirectory IsNot Nothing

                If System.IO.File.Exists(
                    System.IO.Path.Combine(
                        currentDirectory.FullName,
                        "composer.json")) Then

                    Return currentDirectory.FullName

                End If

                If System.IO.File.Exists(
                    System.IO.Path.Combine(
                        currentDirectory.FullName,
                        "package.json")) Then

                    Return currentDirectory.FullName

                End If

                If System.IO.File.Exists(
                    System.IO.Path.Combine(
                        currentDirectory.FullName,
                        "requirements.txt")) Then

                    Return currentDirectory.FullName

                End If

                If System.IO.Directory.GetFiles(
                    currentDirectory.FullName,
                    "*.sln",
                    SearchOption.TopDirectoryOnly).Length > 0 Then

                    Return currentDirectory.FullName

                End If

                If System.IO.Directory.GetFiles(
                    currentDirectory.FullName,
                    "*.csproj",
                    SearchOption.TopDirectoryOnly).Length > 0 Then

                    Return currentDirectory.FullName

                End If

                If System.IO.Directory.GetFiles(
                    currentDirectory.FullName,
                    "*.vbproj",
                    SearchOption.TopDirectoryOnly).Length > 0 Then

                    Return currentDirectory.FullName

                End If

                currentDirectory =
                    currentDirectory.Parent

            Loop

            Return startDirectory

        End Function

        Private Function GetRelativePath(
            rootPath As String,
            fullPath As String) As String

            Dim rootUri As New Uri(
                EnsureTrailingSeparator(
                    rootPath))

            Dim fileUri As New Uri(
                System.IO.Path.GetFullPath(
                    fullPath))

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

        Private Function MakeSafeFolderName(
            folderName As String) As String

            Dim invalidCharacters() As Char =
                System.IO.Path.GetInvalidFileNameChars()

            Dim result As String =
                folderName

            For Each invalidCharacter As Char In
                invalidCharacters

                result =
                    result.Replace(
                        invalidCharacter,
                        "_"c)

            Next

            Return result

        End Function

    End Class

End Namespace