Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text

Namespace DevBot.Core.File

    Public Class FileManager

        Private Const MaxReadSize As Long = 10485760L

        Private Shared ReadOnly BinaryExtensions As String() = {
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".bmp",
            ".ico",
            ".webp",
            ".exe",
            ".dll",
            ".zip",
            ".rar",
            ".7z",
            ".pdf",
            ".mp3",
            ".mp4",
            ".avi",
            ".mov",
            ".class",
            ".jar",
            ".so",
            ".bin"
        }

        Private ReadOnly backupService As BackupService

        Public Sub New()

            backupService =
                New BackupService()

        End Sub

        Public Function Exists(
            filePath As String) As Boolean

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Return False

            End If

            Return System.IO.File.Exists(
                filePath)

        End Function

        Public Function ReadText(
            filePath As String) As String

            ValidateFilePath(
                filePath)

            Dim fileInfo As New FileInfo(
                filePath)

            If fileInfo.Length >
                MaxReadSize Then

                Throw New IOException(
                    "File terlalu besar untuk dibaca. " &
                    "Maksimum ukuran adalah 10 MB.")

            End If

            If IsBinaryFile(
                fileInfo.Extension) Then

                Throw New IOException(
                    "File binary tidak dapat dibaca sebagai " &
                    "source code.")

            End If

            Return System.IO.File.ReadAllText(
                filePath,
                Encoding.UTF8)

        End Function

        Public Function TryReadText(
            filePath As String,
            ByRef content As String,
            ByRef errorMessage As String) As Boolean

            content =
                String.Empty

            errorMessage =
                String.Empty

            Try

                content =
                    ReadText(
                        filePath)

                Return True

            Catch ex As Exception

                errorMessage =
                    ex.Message

                Return False

            End Try

        End Function

        Public Function WriteText(
            filePath As String,
            content As String,
            Optional createBackup As Boolean = True) As Boolean

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Throw New ArgumentException(
                    "Path file tidak boleh kosong.",
                    NameOf(filePath))

            End If

            If content Is Nothing Then

                content =
                    String.Empty

            End If

            Dim fullPath As String =
                Path.GetFullPath(
                    filePath)

            If createBackup AndAlso
               System.IO.File.Exists(
                   fullPath) Then

                backupService.CreateBackup(
                    fullPath)

            End If

            Dim directoryPath As String =
                Path.GetDirectoryName(
                    fullPath)

            If Not String.IsNullOrWhiteSpace(
                directoryPath) AndAlso
                Not Directory.Exists(
                    directoryPath) Then

                Directory.CreateDirectory(
                    directoryPath)

            End If

            System.IO.File.WriteAllText(
                fullPath,
                content,
                New UTF8Encoding(False))

            Return True

        End Function

        Public Function CreateFile(
            filePath As String,
            Optional content As String = "") As Boolean

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Throw New ArgumentException(
                    "Path file tidak boleh kosong.",
                    NameOf(filePath))

            End If

            Dim fullPath As String =
                Path.GetFullPath(
                    filePath)

            If System.IO.File.Exists(
                fullPath) Then

                Throw New IOException(
                    "File sudah ada: " &
                    fullPath)

            End If

            Return WriteText(
                fullPath,
                content,
                False)

        End Function

        Public Function UpdateFile(
            filePath As String,
            content As String) As Boolean

            ValidateFilePath(
                filePath)

            Return WriteText(
                filePath,
                content,
                True)

        End Function

        Public Function DeleteFile(
            filePath As String,
            Optional createBackup As Boolean = True) As Boolean

            ValidateFilePath(
                filePath)

            If createBackup Then

                backupService.CreateBackup(
                    filePath)

            End If

            System.IO.File.Delete(
                filePath)

            Return True

        End Function

        Public Function CopyFile(
            sourceFile As String,
            destinationFile As String,
            Optional overwrite As Boolean = False) As Boolean

            ValidateFilePath(
                sourceFile)

            If String.IsNullOrWhiteSpace(
                destinationFile) Then

                Throw New ArgumentException(
                    "Destination file tidak boleh kosong.",
                    NameOf(destinationFile))

            End If

            Dim destinationPath As String =
                Path.GetFullPath(
                    destinationFile)

            Dim destinationDirectory As String =
                Path.GetDirectoryName(
                    destinationPath)

            If Not String.IsNullOrWhiteSpace(
                destinationDirectory) AndAlso
                Not Directory.Exists(
                    destinationDirectory) Then

                Directory.CreateDirectory(
                    destinationDirectory)

            End If

            System.IO.File.Copy(
                sourceFile,
                destinationPath,
                overwrite)

            Return True

        End Function

        Public Function MoveFile(
            sourceFile As String,
            destinationFile As String,
            Optional overwrite As Boolean = False) As Boolean

            ValidateFilePath(
                sourceFile)

            If String.IsNullOrWhiteSpace(
                destinationFile) Then

                Throw New ArgumentException(
                    "Destination file tidak boleh kosong.",
                    NameOf(destinationFile))

            End If

            Dim destinationPath As String =
                Path.GetFullPath(
                    destinationFile)

            If System.IO.File.Exists(
                destinationPath) Then

                If Not overwrite Then

                    Throw New IOException(
                        "Destination file sudah ada: " &
                        destinationPath)

                End If

                System.IO.File.Delete(
                    destinationPath)

            End If

            System.IO.File.Move(
                sourceFile,
                destinationPath)

            Return True

        End Function

        Public Function GetFileInfo(
            filePath As String) As FileInfo

            ValidateFilePath(
                filePath)

            Return New FileInfo(
                filePath)

        End Function

        Public Function GetFiles(
            directoryPath As String,
            Optional recursive As Boolean = True) As List(Of String)

            If String.IsNullOrWhiteSpace(
                directoryPath) Then

                Throw New ArgumentException(
                    "Path folder tidak boleh kosong.",
                    NameOf(directoryPath))

            End If

            Dim fullPath As String =
                Path.GetFullPath(
                    directoryPath)

            If Not Directory.Exists(
                fullPath) Then

                Throw New DirectoryNotFoundException(
                    "Folder tidak ditemukan: " &
                    fullPath)

            End If

            Dim result As New List(Of String)()

            CollectFiles(
                fullPath,
                recursive,
                result)

            result.Sort(
                StringComparer.OrdinalIgnoreCase)

            Return result

        End Function

        Private Sub CollectFiles(
            directoryPath As String,
            recursive As Boolean,
            result As List(Of String))

            Dim files() As String

            Try

                files =
                    Directory.GetFiles(
                        directoryPath)

            Catch ex As UnauthorizedAccessException

                Return

            Catch ex As IOException

                Return

            End Try

            For Each filePath As String In
                files

                result.Add(
                    filePath)

            Next

            If Not recursive Then

                Return

            End If

            Dim directories() As String

            Try

                directories =
                    Directory.GetDirectories(
                        directoryPath)

            Catch ex As UnauthorizedAccessException

                Return

            Catch ex As IOException

                Return

            End Try

            For Each childDirectory As String In
                directories

                CollectFiles(
                    childDirectory,
                    True,
                    result)

            Next

        End Sub

        Public Function GetExtension(
            filePath As String) As String

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Return String.Empty

            End If

            Return Path.GetExtension(
                filePath)

        End Function

        Public Function GetFileName(
            filePath As String) As String

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Return String.Empty

            End If

            Return Path.GetFileName(
                filePath)

        End Function

        Public Function GetDirectory(
            filePath As String) As String

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Return String.Empty

            End If

            Return Path.GetDirectoryName(
                Path.GetFullPath(
                    filePath))

        End Function

        Public Function IsBinaryFile(
            filePathOrExtension As String) As Boolean

            If String.IsNullOrWhiteSpace(
                filePathOrExtension) Then

                Return False

            End If

            Dim extension As String

            If filePathOrExtension.Contains(
                "."c) Then

                extension =
                    Path.GetExtension(
                        filePathOrExtension)

            Else

                extension =
                    filePathOrExtension

            End If

            If String.IsNullOrWhiteSpace(
                extension) Then

                Return False

            End If

            extension =
                extension.ToLowerInvariant()

            For Each binaryExtension As String In
                BinaryExtensions

                If String.Equals(
                    extension,
                    binaryExtension,
                    StringComparison.OrdinalIgnoreCase) Then

                    Return True

                End If

            Next

            Return False

        End Function

        Public Function EnsureDirectory(
            directoryPath As String) As Boolean

            If String.IsNullOrWhiteSpace(
                directoryPath) Then

                Throw New ArgumentException(
                    "Path folder tidak boleh kosong.",
                    NameOf(directoryPath))

            End If

            Dim fullPath As String =
                Path.GetFullPath(
                    directoryPath)

            If Not Directory.Exists(
                fullPath) Then

                Directory.CreateDirectory(
                    fullPath)

            End If

            Return True

        End Function

        Private Sub ValidateFilePath(
            filePath As String)

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Throw New ArgumentException(
                    "Path file tidak boleh kosong.",
                    NameOf(filePath))

            End If

            Dim fullPath As String =
                Path.GetFullPath(
                    filePath)

            If Not System.IO.File.Exists(
                fullPath) Then

                Throw New FileNotFoundException(
                    "File tidak ditemukan.",
                    fullPath)

            End If

        End Sub

    End Class

End Namespace