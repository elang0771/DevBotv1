Imports System
Imports System.IO
Imports System.Text

Namespace DevBot.Core.File

    Public Class FileService

        Private Const MaxReadSize As Long =
            10L * 1024L * 1024L

        Public Function FileExists(
            filePath As String) As Boolean

            If String.IsNullOrWhiteSpace(filePath) Then
                Return False
            End If

            Return System.IO.File.Exists(filePath)

        End Function

        Public Function DirectoryExists(
            directoryPath As String) As Boolean

            If String.IsNullOrWhiteSpace(directoryPath) Then
                Return False
            End If

            Return System.IO.Directory.Exists(
                directoryPath)

        End Function

        Public Function ReadFile(
            filePath As String) As String

            ValidateFilePath(filePath)

            Dim fileInfo As New FileInfo(filePath)

            If fileInfo.Length > MaxReadSize Then

                Throw New IOException(
                    "File terlalu besar untuk dibaca. " &
                    "Maksimum " &
                    (MaxReadSize \ 1024L \ 1024L).ToString() &
                    " MB.")

            End If

            Return System.IO.File.ReadAllText(
                filePath,
                Encoding.UTF8)

        End Function

        Public Sub WriteFile(
            filePath As String,
            content As String)

            If String.IsNullOrWhiteSpace(filePath) Then

                Throw New ArgumentException(
                    "Path file tidak boleh kosong.",
                    "filePath")

            End If

            If content Is Nothing Then
                content = String.Empty
            End If

            Dim fullPath As String =
                System.IO.Path.GetFullPath(filePath)

            Dim directoryPath As String =
                System.IO.Path.GetDirectoryName(fullPath)

            If String.IsNullOrWhiteSpace(directoryPath) Then

                Throw New IOException(
                    "Folder tujuan file tidak valid.")

            End If

            If Not Directory.Exists(directoryPath) Then

                Directory.CreateDirectory(
                    directoryPath)

            End If

            System.IO.File.WriteAllText(
                fullPath,
                content,
                Encoding.UTF8)

        End Sub

        Public Sub CreateFile(
            filePath As String,
            Optional content As String = "")

            If String.IsNullOrWhiteSpace(filePath) Then

                Throw New ArgumentException(
                    "Path file tidak boleh kosong.",
                    "filePath")

            End If

            Dim fullPath As String =
                System.IO.Path.GetFullPath(filePath)

            If System.IO.File.Exists(fullPath) Then

                Throw New IOException(
                    "File sudah ada: " &
                    fullPath)

            End If

            WriteFile(
                fullPath,
                content)

        End Sub

        Public Sub DeleteFile(
            filePath As String)

            ValidateFilePath(filePath)

            If Not System.IO.File.Exists(filePath) Then

                Throw New FileNotFoundException(
                    "File tidak ditemukan.",
                    filePath)

            End If

            System.IO.File.Delete(
                filePath)

        End Sub

        Public Sub CreateDirectory(
            directoryPath As String)

            If String.IsNullOrWhiteSpace(
                directoryPath) Then

                Throw New ArgumentException(
                    "Path folder tidak boleh kosong.",
                    "directoryPath")

            End If

            If System.IO.Directory.Exists(
                directoryPath) Then

                Return

            End If

            System.IO.Directory.CreateDirectory(
                directoryPath)

        End Sub

        Public Function GetFileSize(
            filePath As String) As Long

            ValidateFilePath(filePath)

            Dim fileInfo As New FileInfo(filePath)

            Return fileInfo.Length

        End Function

        Public Function GetFileExtension(
            filePath As String) As String

            If String.IsNullOrWhiteSpace(filePath) Then
                Return String.Empty
            End If

            Return System.IO.Path.GetExtension(
                filePath)

        End Function

        Public Function GetFileName(
            filePath As String) As String

            If String.IsNullOrWhiteSpace(filePath) Then
                Return String.Empty
            End If

            Return System.IO.Path.GetFileName(
                filePath)

        End Function

        Public Function GetDirectoryName(
            filePath As String) As String

            If String.IsNullOrWhiteSpace(filePath) Then
                Return String.Empty
            End If

            Return System.IO.Path.GetDirectoryName(
                filePath)

        End Function

        Private Sub ValidateFilePath(
            filePath As String)

            If String.IsNullOrWhiteSpace(filePath) Then

                Throw New ArgumentException(
                    "Path file tidak boleh kosong.",
                    "filePath")

            End If

            If Not System.IO.File.Exists(
                filePath) Then

                Throw New FileNotFoundException(
                    "File tidak ditemukan.",
                    filePath)

            End If

        End Sub

    End Class

End Namespace