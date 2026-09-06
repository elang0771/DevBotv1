Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports DevBot.Core.File
Imports DevBot.Core.Project

Namespace DevBot.Core.Agent

    Public Class AgentEngine

        Private ReadOnly projectScanner As ProjectScanner
        Private ReadOnly fileManager As FileManager
        Private ReadOnly agentBuildEngine As AgentBuildEngine
        Private ReadOnly agentAutoFixEngine As AgentAutoFixEngine

        Private currentProjectPath As String = String.Empty

        Private lastScanResult As ProjectScanResult

        Private ReadOnly ignoredDirectories As String() = {
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

        Public Sub New()

            projectScanner = New ProjectScanner()
            fileManager = New FileManager()
            agentBuildEngine = New AgentBuildEngine()
            agentAutoFixEngine = New AgentAutoFixEngine()

        End Sub

        '==========================================================
        ' OPEN PROJECT
        '==========================================================

        Public Function OpenProject(
            projectPath As String) As AgentResult

            If String.IsNullOrWhiteSpace(projectPath) Then

                Return AgentResult.Fail(
                    "Path project tidak boleh kosong.")

            End If

            Try

                Dim fullPath As String =
                    Path.GetFullPath(projectPath)

                If Not Directory.Exists(fullPath) Then

                    Return AgentResult.Fail(
                        "Folder project tidak ditemukan: " &
                        fullPath)

                End If

                currentProjectPath = fullPath
                lastScanResult = Nothing

                Return AgentResult.Success(
                    "Project berhasil dibuka: " &
                    fullPath)

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal membuka project: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' SCAN PROJECT
        '==========================================================

        Public Function ScanProject() As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Try

                lastScanResult =
                    projectScanner.Scan(
                        currentProjectPath)

                Return AgentResult.Success(
                    BuildScanSummary(lastScanResult),
                    lastScanResult)

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal melakukan scan project: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' EXECUTE COMMAND
        '==========================================================

        Public Function Execute(
            command As String) As AgentResult

            If String.IsNullOrWhiteSpace(command) Then

                Return AgentResult.Fail(
                    "Perintah tidak boleh kosong.")

            End If

            Dim normalizedCommand As String =
                command.Trim().ToLowerInvariant()

            '------------------------------------------------------
            ' TEST / AUTO FIX PROJECT
            '
            ' "test" is intentionally routed through AutoFix for now.
            ' This allows DevBot to validate the complete cycle:
            ' BUILD -> DETECT ERROR -> AI ANALYZE -> APPLY FIX ->
            ' BUILD AGAIN.
            '
            ' This is the integration test entry point for the
            ' VB.NET and PHP AutoFix test projects.
            '------------------------------------------------------

            If normalizedCommand = "test" OrElse
               normalizedCommand = "test project" OrElse
               normalizedCommand = "auto test" OrElse
               normalizedCommand = "auto fix" OrElse
               normalizedCommand = "autofix" OrElse
               normalizedCommand = "fix build" OrElse
               normalizedCommand = "fix errors" OrElse
               normalizedCommand = "repair build" OrElse
               normalizedCommand = "repair project" Then

                Return AutoFixProject()

            End If

            '------------------------------------------------------
            ' BUILD PROJECT
            '------------------------------------------------------

            If normalizedCommand = "build" OrElse
               normalizedCommand = "build project" OrElse
               normalizedCommand = "compile" OrElse
               normalizedCommand = "compile project" Then

                Return BuildProject()

            End If

            '------------------------------------------------------
            ' SCAN
            '------------------------------------------------------

            If normalizedCommand = "scan" OrElse
               normalizedCommand = "scan project" Then

                Return ScanProject()

            End If

            '------------------------------------------------------
            ' PROJECT INFO
            '------------------------------------------------------

            If normalizedCommand = "project" OrElse
               normalizedCommand = "project info" Then

                Return GetProjectInfo()

            End If

            '------------------------------------------------------
            ' HELP
            '------------------------------------------------------

            If normalizedCommand = "help" OrElse
               normalizedCommand = "?" Then

                Return GetHelp()

            End If

            '------------------------------------------------------
            ' CREATE FILE
            '------------------------------------------------------

            If IsCreateFileCommand(command) Then

                Return ExecuteCreateFileCommand(command)

            End If

            '------------------------------------------------------
            ' DELETE FILE
            '------------------------------------------------------

            If IsDeleteFileCommand(command) Then

                Return ExecuteDeleteFileCommand(command)

            End If

            '------------------------------------------------------
            ' RENAME / MOVE FILE
            '------------------------------------------------------

            If IsRenameFileCommand(command) Then

                Return ExecuteRenameFileCommand(command)

            End If

            '------------------------------------------------------
            ' WRITE / ISI FILE
            '------------------------------------------------------

            If IsWriteFileCommand(command) Then

                Return ExecuteWriteFileCommand(command)

            End If

            '------------------------------------------------------
            ' EDIT
            '------------------------------------------------------

            If IsEditCommand(command) Then

                If IsProjectWideEditCommand(command) Then

                    Return ExecuteProjectWideEditCommand(command)

                End If

                Return ExecuteEditCommand(command)

            End If

            '------------------------------------------------------
            ' READ
            '------------------------------------------------------

            If normalizedCommand.StartsWith(
                "read ",
                StringComparison.OrdinalIgnoreCase) Then

                Dim filePath As String =
                    command.Trim().Substring(5).Trim()

                If String.IsNullOrWhiteSpace(filePath) Then

                    Return AgentResult.Fail(
                        "Path file untuk read belum diberikan.")

                End If

                Return ReadFile(filePath)

            End If

            '------------------------------------------------------
            ' UNKNOWN
            '------------------------------------------------------

            Return AgentResult.Success(
                "Perintah diterima oleh AgentEngine." &
                Environment.NewLine &
                Environment.NewLine &
                "Perintah: " &
                command &
                Environment.NewLine &
                Environment.NewLine &
                "Perintah tersebut belum memiliki handler." &
                Environment.NewLine &
                "Gunakan 'help' untuk melihat perintah yang tersedia.")

        End Function

        '==========================================================
        ' DETECT CREATE FILE COMMAND
        '==========================================================

        Private Function IsCreateFileCommand(
            command As String) As Boolean

            If String.IsNullOrWhiteSpace(command) Then

                Return False

            End If

            Return Regex.IsMatch(
                command.Trim(),
                "^\s*(buat|create)(?:\s+(file|berkas))?\s+\S",
                RegexOptions.IgnoreCase)

        End Function

        '==========================================================
        ' EXECUTE CREATE FILE COMMAND
        '
        ' Contoh:
        '
        ' buat file test.php
        '
        ' buat file test.php dengan isi "<?php echo 'Halo'; ?>"
        '
        ' create file test.php
        '==========================================================

        Private Function ExecuteCreateFileCommand(
            command As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Dim request As CreateFileRequest =
                ParseCreateFileCommand(command)

            If request Is Nothing Then

                Return AgentResult.Fail(
                    "Format perintah buat file belum dikenali." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Contoh:" &
                    Environment.NewLine &
                    "buat file test.php" &
                    Environment.NewLine &
                    "buat file test.php dengan isi ""<?php echo 'Halo'; ?>""")

            End If

            Return CreateFile(
                request.FilePath,
                request.Content)

        End Function

        '==========================================================
        ' PARSE CREATE FILE COMMAND
        '==========================================================

        Private Function ParseCreateFileCommand(
            command As String) As CreateFileRequest

            If String.IsNullOrWhiteSpace(command) Then

                Return Nothing

            End If

            Dim text As String =
                command.Trim()

            Dim pattern As String =
                "^\s*" &
                "(?:buat|create)" &
                "\s+" &
                "(?:(?:file|berkas)\s+)?" &
                "(?<file>[^\s]+)" &
                "(?:\s+(?:(?:dengan\s+)?(?:isi|isinya)|dengan)\s*(?<content>.*))?" &
                "\s*$"

            Dim match As Match =
                Regex.Match(
                    text,
                    pattern,
                    RegexOptions.IgnoreCase Or
                    RegexOptions.Singleline)

            If Not match.Success Then

                Return Nothing

            End If

            Dim filePath As String =
                match.Groups("file").Value.Trim()

            If String.IsNullOrWhiteSpace(filePath) Then

                Return Nothing

            End If

            Dim content As String =
                String.Empty

            If match.Groups("content").Success Then

                content =
                    CleanCommandValue(
                        match.Groups("content").Value)

            End If

            Return New CreateFileRequest(
                filePath,
                content)

        End Function

        '==========================================================
        ' DETECT DELETE FILE COMMAND
        '==========================================================

        Private Function IsDeleteFileCommand(
            command As String) As Boolean

            If String.IsNullOrWhiteSpace(command) Then

                Return False

            End If

            Return Regex.IsMatch(
                command.Trim(),
                "^\s*(hapus|delete|remove)(?:\s+(file|berkas))?\s+\S",
                RegexOptions.IgnoreCase)

        End Function

        '==========================================================
        ' EXECUTE DELETE FILE COMMAND
        '
        ' Contoh:
        '
        ' hapus file test.php
        ' delete file test.php
        ' remove file test.php
        '==========================================================

        Private Function ExecuteDeleteFileCommand(
            command As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Dim filePath As String =
                ParseDeleteFileCommand(command)

            If String.IsNullOrWhiteSpace(filePath) Then

                Return AgentResult.Fail(
                    "Format perintah hapus file belum dikenali." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Contoh:" &
                    Environment.NewLine &
                    "hapus file test.php")

            End If

            Return DeleteFile(filePath)

        End Function

        '==========================================================
        ' PARSE DELETE FILE COMMAND
        '==========================================================

        Private Function ParseDeleteFileCommand(
            command As String) As String

            If String.IsNullOrWhiteSpace(command) Then

                Return String.Empty

            End If

            Dim pattern As String =
                "^\s*" &
                "(?:hapus|delete|remove)" &
                "\s+" &
                "(?:(?:file|berkas)\s+)?" &
                "(?<file>.+?)" &
                "\s*$"

            Dim match As Match =
                Regex.Match(
                    command.Trim(),
                    pattern,
                    RegexOptions.IgnoreCase Or
                    RegexOptions.Singleline)

            If Not match.Success Then

                Return String.Empty

            End If

            Return CleanCommandValue(
                match.Groups("file").Value.Trim())

        End Function

        '==========================================================
        ' DETECT RENAME FILE COMMAND
        '==========================================================

        Private Function IsRenameFileCommand(
            command As String) As Boolean

            If String.IsNullOrWhiteSpace(command) Then

                Return False

            End If

            Return Regex.IsMatch(
                command.Trim(),
                "^\s*(rename|ren|ganti\s+nama)\b",
                RegexOptions.IgnoreCase)

        End Function

        '==========================================================
        ' EXECUTE RENAME FILE COMMAND
        '
        ' Contoh:
        '
        ' rename test.php menjadi demo.php
        ' ganti nama test.php menjadi demo.php
        ' ren test.php ke demo.php
        '==========================================================

        Private Function ExecuteRenameFileCommand(
            command As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Dim request As RenameFileRequest =
                ParseRenameFileCommand(command)

            If request Is Nothing Then

                Return AgentResult.Fail(
                    "Format perintah rename belum dikenali." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Contoh:" &
                    Environment.NewLine &
                    "rename test.php menjadi demo.php" &
                    Environment.NewLine &
                    "ganti nama test.php menjadi demo.php")

            End If

            Return RenameFile(
                request.OldPath,
                request.NewPath)

        End Function

        '==========================================================
        ' PARSE RENAME FILE COMMAND
        '==========================================================

        Private Function ParseRenameFileCommand(
            command As String) As RenameFileRequest

            If String.IsNullOrWhiteSpace(command) Then

                Return Nothing

            End If

            Dim pattern As String =
                "^\s*" &
                "(?:rename|ren|ganti\s+nama)" &
                "\s+" &
                "(?<old>.+?)" &
                "\s+" &
                "(?:menjadi|ke|dengan\s+nama)" &
                "\s+" &
                "(?<new>.+?)" &
                "\s*$"

            Dim match As Match =
                Regex.Match(
                    command.Trim(),
                    pattern,
                    RegexOptions.IgnoreCase Or
                    RegexOptions.Singleline)

            If Not match.Success Then

                Return Nothing

            End If

            Dim oldPath As String =
                CleanCommandValue(
                    match.Groups("old").Value.Trim())

            Dim newPath As String =
                CleanCommandValue(
                    match.Groups("new").Value.Trim())

            If String.IsNullOrWhiteSpace(oldPath) Then

                Return Nothing

            End If

            If String.IsNullOrWhiteSpace(newPath) Then

                Return Nothing

            End If

            Return New RenameFileRequest(
                oldPath,
                newPath)

        End Function

        '==========================================================
        ' CLEAN COMMAND VALUE
        '==========================================================

        Private Function CleanCommandValue(
            value As String) As String

            If value Is Nothing Then

                Return String.Empty

            End If

            Dim result As String =
                value.Trim()

            If result.Length >= 2 Then

                Dim firstChar As Char =
                    result(0)

                Dim lastChar As Char =
                    result(result.Length - 1)

                If (firstChar = """"c AndAlso
                    lastChar = """"c) OrElse
                   (firstChar = "'"c AndAlso
                    lastChar = "'"c) Then

                    result =
                        result.Substring(
                            1,
                            result.Length - 2)

                End If

            End If

            Return result

        End Function

        '==========================================================
        ' CREATE FILE
        '==========================================================

        Public Function CreateFile(
            relativePath As String,
            content As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Try

                Dim fullPath As String =
                    ResolveProjectPath(relativePath)

                If Not IsInsideProject(fullPath) Then

                    Return AgentResult.Fail(
                        "Pembuatan file ditolak. " &
                        "Path berada di luar folder project.")

                End If

                If System.IO.File.Exists(fullPath) Then

                    Return AgentResult.Fail(
                        "File sudah ada: " &
                        relativePath)

                End If

                Dim directoryPath As String =
                    Path.GetDirectoryName(fullPath)

                If Not String.IsNullOrWhiteSpace(
                    directoryPath) AndAlso
                    Not Directory.Exists(directoryPath) Then

                    Directory.CreateDirectory(
                        directoryPath)

                End If

                If content Is Nothing Then

                    content = String.Empty

                End If

                fileManager.CreateFile(
                    fullPath,
                    content)

                lastScanResult = Nothing

                Return AgentResult.Success(
                    BuildCreateFileSuccessMessage(
                        relativePath),
                    New FileOperationResult(
                        "CREATE",
                        relativePath,
                        String.Empty,
                        content))

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal membuat file: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' DELETE FILE
        '==========================================================

        Public Function DeleteFile(
            relativePath As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Try

                Dim fullPath As String =
                    ResolveProjectPath(relativePath)

                If Not IsInsideProject(fullPath) Then

                    Return AgentResult.Fail(
                        "Penghapusan file ditolak. " &
                        "Path berada di luar folder project.")

                End If

                If Not System.IO.File.Exists(fullPath) Then

                    Return AgentResult.Fail(
                        "File tidak ditemukan: " &
                        relativePath)

                End If

                Dim oldContent As String =
                    fileManager.ReadText(
                        fullPath)

                fileManager.DeleteFile(
                    fullPath,
                    True)

                lastScanResult = Nothing

                Return AgentResult.Success(
                    "DELETE FILE BERHASIL" &
                    Environment.NewLine &
                    "====================" &
                    Environment.NewLine &
                    Environment.NewLine &
                    "File    : " &
                    relativePath &
                    Environment.NewLine &
                    "Backup  : dibuat otomatis" &
                    Environment.NewLine &
                    "Status  : file berhasil dihapus",
                    New FileOperationResult(
                        "DELETE",
                        relativePath,
                        oldContent,
                        String.Empty))

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal menghapus file: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' RENAME FILE
        '==========================================================

        Public Function RenameFile(
            oldRelativePath As String,
            newRelativePath As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            If String.IsNullOrWhiteSpace(
                oldRelativePath) Then

                Return AgentResult.Fail(
                    "Nama file lama tidak boleh kosong.")

            End If

            If String.IsNullOrWhiteSpace(
                newRelativePath) Then

                Return AgentResult.Fail(
                    "Nama file baru tidak boleh kosong.")

            End If

            Try

                Dim oldFullPath As String =
                    ResolveProjectPath(
                        oldRelativePath)

                Dim newFullPath As String =
                    ResolveProjectPath(
                        newRelativePath)

                If Not IsInsideProject(
                    oldFullPath) Then

                    Return AgentResult.Fail(
                        "Rename ditolak. File lama berada di luar project.")

                End If

                If Not IsInsideProject(
                    newFullPath) Then

                    Return AgentResult.Fail(
                        "Rename ditolak. File baru berada di luar project.")

                End If

                If Not System.IO.File.Exists(
                    oldFullPath) Then

                    Return AgentResult.Fail(
                        "File lama tidak ditemukan: " &
                        oldRelativePath)

                End If

                If System.IO.File.Exists(
                    newFullPath) Then

                    Return AgentResult.Fail(
                        "File tujuan sudah ada: " &
                        newRelativePath)

                End If

                Dim newDirectory As String =
                    Path.GetDirectoryName(
                        newFullPath)

                If Not String.IsNullOrWhiteSpace(
                    newDirectory) AndAlso
                    Not Directory.Exists(newDirectory) Then

                    Directory.CreateDirectory(
                        newDirectory)

                End If

                System.IO.File.Move(
                    oldFullPath,
                    newFullPath)

                lastScanResult = Nothing

                Return AgentResult.Success(
                    "RENAME FILE BERHASIL" &
                    Environment.NewLine &
                    "===================" &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Dari   : " &
                    oldRelativePath &
                    Environment.NewLine &
                    "Ke     : " &
                    newRelativePath &
                    Environment.NewLine &
                    "Status : file berhasil dipindahkan/rename")

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal rename file: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' UPDATE FILE
        '==========================================================

        Public Function UpdateFile(
            relativePath As String,
            content As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Try

                Dim fullPath As String =
                    ResolveProjectPath(relativePath)

                If Not IsInsideProject(fullPath) Then

                    Return AgentResult.Fail(
                        "Perubahan file ditolak. " &
                        "Path berada di luar folder project.")

                End If

                If Not System.IO.File.Exists(fullPath) Then

                    Return AgentResult.Fail(
                        "File tidak ditemukan: " &
                        relativePath)

                End If

                Dim oldContent As String =
                    fileManager.ReadText(
                        fullPath)

                If content Is Nothing Then
                    content = String.Empty
                End If

                fileManager.UpdateFile(
                    fullPath,
                    content)

                lastScanResult = Nothing

                Return AgentResult.Success(
                    "File berhasil diperbarui: " &
                    relativePath,
                    New FileOperationResult(
                        "UPDATE",
                        relativePath,
                        oldContent,
                        content))

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal memperbarui file: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' DETECT WRITE / ISI FILE COMMAND
        '==========================================================

        Private Function IsWriteFileCommand(
            command As String) As Boolean

            If String.IsNullOrWhiteSpace(command) Then

                Return False

            End If

            Return Regex.IsMatch(
                command.Trim(),
                "^\s*(isi|tulis|tuliskan)\s+(?:file\s+)?\S+\s+(?:dengan\s+)?(?:isi\s+)?",
                RegexOptions.IgnoreCase Or RegexOptions.Singleline)

        End Function

        '==========================================================
        ' EXECUTE WRITE / ISI FILE COMMAND
        '==========================================================

        Private Function ExecuteWriteFileCommand(
            command As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Dim text As String =
                command.Trim()

            Dim pattern As String =
                "^\s*(?:isi|tulis|tuliskan)\s+" &
                "(?:file\s+)?" &
                "(?<file>\S+)" &
                "\s+(?:dengan\s+)?(?:isi\s+)?(?<content>[\s\S]*)$"

            Dim match As Match =
                Regex.Match(
                    text,
                    pattern,
                    RegexOptions.IgnoreCase Or
                    RegexOptions.Singleline)

            If Not match.Success Then

                Return AgentResult.Fail(
                    "Format perintah isi file belum dikenali.")

            End If

            Dim filePath As String =
                match.Groups("file").Value.Trim()

            Dim content As String =
                match.Groups("content").Value

            If String.IsNullOrWhiteSpace(filePath) Then

                Return AgentResult.Fail(
                    "Nama file belum diberikan.")

            End If

            If content Is Nothing Then
                content = String.Empty
            End If

            content =
                CleanCommandValue(content)

            Return UpdateFile(
                filePath,
                content)

        End Function

        '==========================================================
        ' EDIT COMMAND
        '==========================================================

        Private Function IsEditCommand(
            command As String) As Boolean

            If String.IsNullOrWhiteSpace(command) Then

                Return False

            End If

            Return Regex.IsMatch(
                command.Trim(),
                "^\s*(ubah|ganti)\b",
                RegexOptions.IgnoreCase)

        End Function

        '==========================================================
        ' DETECT PROJECT-WIDE EDIT
        '==========================================================

        Private Function IsProjectWideEditCommand(
            command As String) As Boolean

            If String.IsNullOrWhiteSpace(command) Then

                Return False

            End If

            Return Regex.IsMatch(
                command.Trim(),
                "\b(seluruh\s+project|seluruh\s+proyek|semua\s+project|semua\s+proyek)\b",
                RegexOptions.IgnoreCase)

        End Function

        '==========================================================
        ' SINGLE FILE EDIT
        '==========================================================

        Private Function ExecuteEditCommand(
            command As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Dim request As EditRequest =
                ParseEditCommand(command)

            If request Is Nothing Then

                Return AgentResult.Fail(
                    "Format perintah edit belum dikenali." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Contoh:" &
                    Environment.NewLine &
                    "ubah tulisan ""lama"" menjadi ""baru"" di index.php")

            End If

            Return ReplaceTextInFile(
                request.FilePath,
                request.OldText,
                request.NewText)

        End Function

        '==========================================================
        ' PROJECT-WIDE EDIT
        '==========================================================

        Private Function ExecuteProjectWideEditCommand(
            command As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Dim request As EditRequest =
                ParseProjectWideEditCommand(command)

            If request Is Nothing Then

                Return AgentResult.Fail(
                    "Format edit seluruh project belum dikenali." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Contoh:" &
                    Environment.NewLine &
                    "ubah tulisan ""lama"" menjadi ""baru"" di seluruh project")

            End If

            Return ReplaceTextInProject(
                request.OldText,
                request.NewText)

        End Function

        '==========================================================
        ' PARSE SINGLE FILE EDIT
        '==========================================================

        Private Function ParseEditCommand(
            command As String) As EditRequest

            If String.IsNullOrWhiteSpace(command) Then

                Return Nothing

            End If

            Dim pattern As String =
                "^\s*" &
                "(?:ubah|ganti)" &
                "\s+" &
                "(?:tulisan\s+)?" &
                "[""'](?<old>.*?)[""']" &
                "\s+" &
                "(?:menjadi|dengan)" &
                "\s+" &
                "[""'](?<new>.*?)[""']" &
                "\s+" &
                "(?:di|pada|dalam)" &
                "\s+" &
                "(?<file>.+?)" &
                "\s*$"

            Dim match As Match =
                Regex.Match(
                    command,
                    pattern,
                    RegexOptions.IgnoreCase Or
                    RegexOptions.Singleline)

            If Not match.Success Then

                Return Nothing

            End If

            Dim oldText As String =
                match.Groups("old").Value

            Dim newText As String =
                match.Groups("new").Value

            Dim filePath As String =
                match.Groups("file").Value.Trim()

            If String.IsNullOrWhiteSpace(oldText) Then

                Return Nothing

            End If

            If String.IsNullOrWhiteSpace(filePath) Then

                Return Nothing

            End If

            Return New EditRequest(
                filePath,
                oldText,
                newText)

        End Function

        '==========================================================
        ' PARSE PROJECT-WIDE EDIT
        '==========================================================

        Private Function ParseProjectWideEditCommand(
            command As String) As EditRequest

            If String.IsNullOrWhiteSpace(command) Then

                Return Nothing

            End If

            Dim pattern As String =
                "^\s*" &
                "(?:ubah|ganti)" &
                "\s+" &
                "(?:tulisan\s+)?" &
                "[""'](?<old>.*?)[""']" &
                "\s+" &
                "(?:menjadi|dengan)" &
                "\s+" &
                "[""'](?<new>.*?)[""']" &
                "\s+" &
                "(?:di|pada|dalam)" &
                "\s+" &
                "(?:seluruh\s+project|" &
                "seluruh\s+proyek|" &
                "semua\s+project|" &
                "semua\s+proyek)" &
                "\s*$"

            Dim match As Match =
                Regex.Match(
                    command,
                    pattern,
                    RegexOptions.IgnoreCase Or
                    RegexOptions.Singleline)

            If Not match.Success Then

                Return Nothing

            End If

            Dim oldText As String =
                match.Groups("old").Value

            Dim newText As String =
                match.Groups("new").Value

            If String.IsNullOrWhiteSpace(oldText) Then

                Return Nothing

            End If

            Return New EditRequest(
                String.Empty,
                oldText,
                newText)

        End Function

        '==========================================================
        ' REPLACE TEXT IN ONE FILE
        '==========================================================

        Public Function ReplaceTextInFile(
            relativePath As String,
            oldText As String,
            newText As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            If String.IsNullOrWhiteSpace(relativePath) Then

                Return AgentResult.Fail(
                    "Path file tidak boleh kosong.")

            End If

            If String.IsNullOrWhiteSpace(oldText) Then

                Return AgentResult.Fail(
                    "Teks yang akan dicari tidak boleh kosong.")

            End If

            If newText Is Nothing Then

                newText = String.Empty

            End If

            Try

                Dim fullPath As String =
                    ResolveProjectPath(relativePath)

                If Not IsInsideProject(fullPath) Then

                    Return AgentResult.Fail(
                        "Perubahan file ditolak. " &
                        "Path berada di luar folder project.")

                End If

                If Not System.IO.File.Exists(fullPath) Then

                    Return AgentResult.Fail(
                        "File tidak ditemukan: " &
                        relativePath)

                End If

                Dim fileInfo As New FileInfo(
                    fullPath)

                If fileInfo.Length > 10485760L Then

                    Return AgentResult.Fail(
                        "File terlalu besar untuk diedit. " &
                        "Maksimum ukuran adalah 10 MB.")

                End If

                If fileManager.IsBinaryFile(
                    fileInfo.Extension) Then

                    Return AgentResult.Fail(
                        "File binary tidak dapat diedit sebagai source code.")

                End If

                Dim originalContent As String =
                    fileManager.ReadText(fullPath)

                Dim occurrenceCount As Integer =
                    CountOccurrences(
                        originalContent,
                        oldText)

                If occurrenceCount = 0 Then

                    Return AgentResult.Fail(
                        "Teks yang dicari tidak ditemukan di file." &
                        Environment.NewLine &
                        Environment.NewLine &
                        "File : " &
                        relativePath &
                        Environment.NewLine &
                        "Cari : " &
                        oldText)

                End If

                Dim newContent As String =
                    originalContent.Replace(
                        oldText,
                        newText)

                If String.Equals(
                    originalContent,
                    newContent,
                    StringComparison.Ordinal) Then

                    Return AgentResult.Fail(
                        "Tidak ada perubahan yang diperlukan.")

                End If

                Dim relativeDisplayPath As String =
                    GetRelativeProjectPath(
                        fullPath)

                Dim diffText As String =
                    BuildSimpleDiff(
                        relativeDisplayPath,
                        oldText,
                        newText,
                        occurrenceCount)

                fileManager.UpdateFile(
                    fullPath,
                    newContent)

                lastScanResult = Nothing

                Dim resultData As New EditResult()

                resultData.FilePath =
                    fullPath

                resultData.RelativePath =
                    relativeDisplayPath

                resultData.OldText =
                    oldText

                resultData.NewText =
                    newText

                resultData.OccurrenceCount =
                    occurrenceCount

                resultData.DiffText =
                    diffText

                resultData.OriginalContent =
                    originalContent

                resultData.NewContent =
                    newContent

                Return AgentResult.Success(
                    BuildEditSuccessMessage(
                        relativeDisplayPath,
                        occurrenceCount,
                        diffText),
                    resultData)

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal melakukan edit file: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' REPLACE TEXT IN ENTIRE PROJECT
        '==========================================================

        Public Function ReplaceTextInProject(
            oldText As String,
            newText As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            If String.IsNullOrWhiteSpace(oldText) Then

                Return AgentResult.Fail(
                    "Teks yang akan dicari tidak boleh kosong.")

            End If

            If newText Is Nothing Then

                newText = String.Empty

            End If

            Try

                Dim files As List(Of String) =
                    GetEditableProjectFiles()

                Dim changes As New List(Of ProjectEditItem)()

                Dim totalOccurrences As Integer = 0

                '--------------------------------------------------
                ' SEARCH PHASE
                '--------------------------------------------------

                For Each filePath As String In files

                    Try

                        Dim fileInfo As New FileInfo(
                            filePath)

                        If fileInfo.Length >
                            10485760L Then

                            Continue For

                        End If

                        If fileManager.IsBinaryFile(
                            fileInfo.Extension) Then

                            Continue For

                        End If

                        Dim originalContent As String =
                            fileManager.ReadText(
                                filePath)

                        Dim occurrenceCount As Integer =
                            CountOccurrences(
                                originalContent,
                                oldText)

                        If occurrenceCount <= 0 Then

                            Continue For

                        End If

                        Dim newContent As String =
                            originalContent.Replace(
                                oldText,
                                newText)

                        Dim item As New ProjectEditItem()

                        item.FilePath =
                            filePath

                        item.RelativePath =
                            GetRelativeProjectPath(
                                filePath)

                        item.OldText =
                            oldText

                        item.NewText =
                            newText

                        item.OccurrenceCount =
                            occurrenceCount

                        item.OriginalContent =
                            originalContent

                        item.NewContent =
                            newContent

                        item.DiffText =
                            BuildSimpleDiff(
                                item.RelativePath,
                                oldText,
                                newText,
                                occurrenceCount)

                        changes.Add(item)

                    Catch ex As UnauthorizedAccessException

                        Continue For

                    Catch ex As IOException

                        Continue For

                    Catch ex As Exception

                        Continue For

                    End Try

                Next

                '--------------------------------------------------
                ' NOTHING FOUND
                '--------------------------------------------------

                If changes.Count = 0 Then

                    Return AgentResult.Success(
                        "EDIT SELURUH PROJECT" &
                        Environment.NewLine &
                        "=====================" &
                        Environment.NewLine &
                        Environment.NewLine &
                        "Teks tidak ditemukan di file source project." &
                        Environment.NewLine &
                        Environment.NewLine &
                        "Cari : " &
                        oldText)

                End If

                '--------------------------------------------------
                ' APPLY PHASE
                '--------------------------------------------------

                Dim appliedChanges As New List(
                    Of ProjectEditItem)()

                For Each item As ProjectEditItem In changes

                    Try

                        fileManager.UpdateFile(
                            item.FilePath,
                            item.NewContent)

                        appliedChanges.Add(item)

                        totalOccurrences +=
                            item.OccurrenceCount

                    Catch ex As Exception

                        Dim errorText As New StringBuilder()

                        errorText.AppendLine(
                            "EDIT SELURUH PROJECT GAGAL")

                        errorText.AppendLine(
                            "=========================")

                        errorText.AppendLine()

                        errorText.AppendLine(
                            "Gagal memperbarui:")

                        errorText.AppendLine(
                            item.RelativePath)

                        errorText.AppendLine()

                        errorText.AppendLine(
                            "File yang sudah berhasil diperbarui:")

                        For Each applied As ProjectEditItem In
                            appliedChanges

                            errorText.AppendLine(
                                "- " &
                                applied.RelativePath)

                        Next

                        errorText.AppendLine()

                        errorText.AppendLine(
                            "Error: " &
                            ex.Message)

                        Return AgentResult.Fail(
                            errorText.ToString())

                    End Try

                Next

                lastScanResult = Nothing

                Dim resultData As New ProjectEditResult()

                resultData.OldText =
                    oldText

                resultData.NewText =
                    newText

                resultData.FilesChanged =
                    appliedChanges.Count

                resultData.TotalOccurrences =
                    totalOccurrences

                resultData.Changes =
                    appliedChanges

                Return AgentResult.Success(
                    BuildProjectEditSuccessMessage(
                        appliedChanges,
                        oldText,
                        newText,
                        totalOccurrences),
                    resultData)

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal melakukan edit seluruh project: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' GET EDITABLE PROJECT FILES
        '==========================================================

        Private Function GetEditableProjectFiles() As List(Of String)

            Dim result As New List(Of String)()

            If Not HasProject() Then

                Return result

            End If

            CollectEditableFiles(
                currentProjectPath,
                result)

            result.Sort(
                StringComparer.OrdinalIgnoreCase)

            Return result

        End Function

        '==========================================================
        ' COLLECT EDITABLE FILES
        '==========================================================

        Private Sub CollectEditableFiles(
            directoryPath As String,
            result As List(Of String))

            Dim directoryInfo As DirectoryInfo

            Try

                directoryInfo =
                    New DirectoryInfo(
                        directoryPath)

            Catch ex As Exception

                Return

            End Try

            Dim directories As DirectoryInfo()

            Try

                directories =
                    directoryInfo.GetDirectories()

            Catch ex As UnauthorizedAccessException

                Return

            Catch ex As IOException

                Return

            End Try

            Array.Sort(
                directories,
                AddressOf CompareDirectories)

            For Each childDirectory As DirectoryInfo In
                directories

                If IsIgnoredDirectory(
                    childDirectory.Name) Then

                    Continue For

                End If

                CollectEditableFiles(
                    childDirectory.FullName,
                    result)

            Next

            Dim files As FileInfo()

            Try

                files =
                    directoryInfo.GetFiles()

            Catch ex As UnauthorizedAccessException

                Return

            Catch ex As IOException

                Return

            End Try

            Array.Sort(
                files,
                AddressOf CompareFiles)

            For Each fileInfo As FileInfo In
                files

                If fileInfo.Length >
                    10485760L Then

                    Continue For

                End If

                If fileManager.IsBinaryFile(
                    fileInfo.Extension) Then

                    Continue For

                End If

                result.Add(
                    fileInfo.FullName)

            Next

        End Sub

        '==========================================================
        ' IGNORED DIRECTORY
        '==========================================================

        Private Function IsIgnoredDirectory(
            directoryName As String) As Boolean

            If String.IsNullOrWhiteSpace(
                directoryName) Then

                Return False

            End If

            For Each ignored As String In
                ignoredDirectories

                If String.Equals(
                    directoryName,
                    ignored,
                    StringComparison.OrdinalIgnoreCase) Then

                    Return True

                End If

            Next

            Return False

        End Function

        '==========================================================
        ' COUNT OCCURRENCES
        '==========================================================

        Private Function CountOccurrences(
            sourceText As String,
            searchText As String) As Integer

            If String.IsNullOrEmpty(sourceText) Then

                Return 0

            End If

            If String.IsNullOrEmpty(searchText) Then

                Return 0

            End If

            Dim count As Integer = 0
            Dim startIndex As Integer = 0

            While True

                Dim index As Integer =
                    sourceText.IndexOf(
                        searchText,
                        startIndex,
                        StringComparison.Ordinal)

                If index < 0 Then

                    Exit While

                End If

                count += 1

                startIndex =
                    index +
                    Math.Max(
                        searchText.Length,
                        1)

                If startIndex >=
                    sourceText.Length Then

                    Exit While

                End If

            End While

            Return count

        End Function

        '==========================================================
        ' SIMPLE DIFF
        '==========================================================

        Private Function BuildSimpleDiff(
            relativePath As String,
            oldText As String,
            newText As String,
            occurrenceCount As Integer) As String

            Dim text As New StringBuilder()

            text.AppendLine("DIFF")
            text.AppendLine("====")

            text.AppendLine(
                "File: " &
                relativePath)

            text.AppendLine(
                "Occurrences: " &
                occurrenceCount.ToString())

            text.AppendLine()

            text.AppendLine("- OLD")
            text.AppendLine(oldText)

            text.AppendLine()

            text.AppendLine("+ NEW")
            text.AppendLine(newText)

            Return text.ToString()

        End Function

        '==========================================================
        ' SINGLE FILE SUCCESS MESSAGE
        '==========================================================

        Private Function BuildEditSuccessMessage(
            relativePath As String,
            occurrenceCount As Integer,
            diffText As String) As String

            Dim text As New StringBuilder()

            text.AppendLine("EDIT BERHASIL")
            text.AppendLine("============")

            text.AppendLine(
                "File       : " &
                relativePath)

            text.AppendLine(
                "Perubahan  : " &
                occurrenceCount.ToString() &
                " occurrence")

            text.AppendLine(
                "Backup     : dibuat otomatis")

            text.AppendLine(
                "Status     : file berhasil diperbarui")

            text.AppendLine()

            text.AppendLine(diffText)

            Return text.ToString()

        End Function

        '==========================================================
        ' CREATE SUCCESS MESSAGE
        '==========================================================

        Private Function BuildCreateFileSuccessMessage(
            relativePath As String) As String

            Dim text As New StringBuilder()

            text.AppendLine(
                "CREATE FILE BERHASIL")

            text.AppendLine(
                "===================")

            text.AppendLine()

            text.AppendLine(
                "File   : " &
                relativePath)

            text.AppendLine(
                "Status : file berhasil dibuat")

            Return text.ToString()

        End Function

        '==========================================================
        ' PROJECT SUCCESS MESSAGE
        '==========================================================

        Private Function BuildProjectEditSuccessMessage(
            changes As List(Of ProjectEditItem),
            oldText As String,
            newText As String,
            totalOccurrences As Integer) As String

            Dim text As New StringBuilder()

            text.AppendLine(
                "EDIT SELURUH PROJECT BERHASIL")

            text.AppendLine(
                "============================")

            text.AppendLine()

            text.AppendLine(
                "Cari       : " &
                oldText)

            text.AppendLine(
                "Ganti      : " &
                newText)

            text.AppendLine(
                "File       : " &
                changes.Count.ToString())

            text.AppendLine(
                "Perubahan  : " &
                totalOccurrences.ToString() &
                " occurrence")

            text.AppendLine(
                "Backup     : dibuat otomatis")

            text.AppendLine(
                "Status     : seluruh file berhasil diperbarui")

            text.AppendLine()

            text.AppendLine(
                "FILE YANG BERUBAH")

            text.AppendLine(
                "=================")

            For Each item As ProjectEditItem In
                changes

                text.AppendLine(
                    "✓ " &
                    item.RelativePath &
                    " (" &
                    item.OccurrenceCount.ToString() &
                    " perubahan)")

            Next

            text.AppendLine()

            text.AppendLine("DIFF")
            text.AppendLine("====")

            For Each item As ProjectEditItem In
                changes

                text.AppendLine()

                text.AppendLine(
                    item.DiffText)

            Next

            Return text.ToString()

        End Function

        '==========================================================
        ' SORT DIRECTORIES
        '==========================================================

        Private Function CompareDirectories(
            x As DirectoryInfo,
            y As DirectoryInfo) As Integer

            Return StringComparer.OrdinalIgnoreCase.Compare(
                x.Name,
                y.Name)

        End Function

        '==========================================================
        ' SORT FILES
        '==========================================================

        Private Function CompareFiles(
            x As FileInfo,
            y As FileInfo) As Integer

            Return StringComparer.OrdinalIgnoreCase.Compare(
                x.Name,
                y.Name)

        End Function

        '==========================================================
        ' BUILD PROJECT
        '==========================================================

        Public Function BuildProject() As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Try

                Dim buildResult As AgentBuildResult =
                    agentBuildEngine.BuildProject(
                        currentProjectPath)

                Dim text As New StringBuilder()

                If buildResult.Success Then

                    text.AppendLine("BUILD BERHASIL")
                    text.AppendLine("==============")
                    text.AppendLine()
                    text.AppendLine("Project : " & currentProjectPath)
                    text.AppendLine("Status  : SUCCESS")
                    text.AppendLine("ExitCode: " & buildResult.ExitCode.ToString())

                    If Not String.IsNullOrWhiteSpace(buildResult.Command) Then
                        text.AppendLine()
                        text.AppendLine("Command :")
                        text.AppendLine(buildResult.Command)
                    End If

                    If buildResult.Duration <> TimeSpan.Zero Then
                        text.AppendLine()
                        text.AppendLine("Duration: " & buildResult.Duration.TotalSeconds.ToString("0.00") & " detik")
                    End If

                Else

                    text.AppendLine("BUILD GAGAL")
                    text.AppendLine("===========")
                    text.AppendLine()
                    text.AppendLine("Project : " & currentProjectPath)
                    text.AppendLine("Status  : FAILED")
                    text.AppendLine("ExitCode: " & buildResult.ExitCode.ToString())

                    If Not String.IsNullOrWhiteSpace(buildResult.Message) Then
                        text.AppendLine()
                        text.AppendLine("Message :")
                        text.AppendLine(buildResult.Message)
                    End If

                    If buildResult.Errors IsNot Nothing AndAlso
                       buildResult.Errors.Count > 0 Then

                        text.AppendLine()
                        text.AppendLine("ERROR TERDETEKSI")
                        text.AppendLine("================")

                        For Each buildError As BuildError In buildResult.Errors

                            text.AppendLine(
                                "- " & buildError.ToString())

                        Next

                    End If

                    If Not String.IsNullOrWhiteSpace(buildResult.ErrorOutput) Then
                        text.AppendLine()
                        text.AppendLine("BUILD OUTPUT")
                        text.AppendLine("============")
                        text.AppendLine(buildResult.ErrorOutput)
                    End If

                End If

                Return AgentResult.Success(
                    text.ToString(),
                    buildResult)

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal menjalankan build: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' AUTO FIX PROJECT
        '
        ' Alur:
        ' BUILD -> ANALISA ERROR -> AUTO FIX AMAN -> BUILD ULANG
        ' Maksimum 3 iterasi.
        '==========================================================

        Public Function AutoFixProject() As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Try

                Dim autoFixResult As AgentAutoFixResult =
                    agentAutoFixEngine.Run(
                        currentProjectPath,
                        3)

                If autoFixResult Is Nothing Then

                    Return AgentResult.Fail(
                        "AutoFix Engine tidak mengembalikan hasil.")

                End If

                If autoFixResult.Success Then

                    Return AgentResult.Success(
                        autoFixResult.Message,
                        autoFixResult)

                End If

                Return AgentResult.Fail(
                    autoFixResult.Message)

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal menjalankan AutoFix: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' GET PROJECT INFO
        '==========================================================

        Public Function GetProjectInfo() As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            If lastScanResult Is Nothing Then

                Dim scanResult As AgentResult =
                    ScanProject()

                If Not scanResult.IsSuccess Then

                    Return scanResult

                End If

            End If

            Return AgentResult.Success(
                BuildScanSummary(
                    lastScanResult),
                lastScanResult)

        End Function

        '==========================================================
        ' READ FILE
        '==========================================================

        Public Function ReadFile(
            filePath As String) As AgentResult

            If Not HasProject() Then

                Return AgentResult.Fail(
                    "Belum ada project yang dibuka.")

            End If

            Try

                Dim fullPath As String =
                    ResolveProjectPath(
                        filePath)

                If Not IsInsideProject(
                    fullPath) Then

                    Return AgentResult.Fail(
                        "Akses file ditolak. " &
                        "File berada di luar folder project.")

                End If

                Dim content As String =
                    fileManager.ReadText(
                        fullPath)

                Return AgentResult.Success(
                    content)

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal membaca file: " &
                    ex.Message)

            End Try

        End Function

        '==========================================================
        ' HAS PROJECT
        '==========================================================

        Public Function HasProject() As Boolean

            Return Not String.IsNullOrWhiteSpace(
                currentProjectPath) AndAlso
                Directory.Exists(
                    currentProjectPath)

        End Function

        '==========================================================
        ' CURRENT PROJECT PATH
        '==========================================================

        Public Function GetCurrentProjectPath() As String

            Return currentProjectPath

        End Function

        '==========================================================
        ' LAST SCAN RESULT
        '==========================================================

        Public Function GetLastScanResult() As ProjectScanResult

            Return lastScanResult

        End Function

        '==========================================================
        ' RESOLVE PROJECT PATH
        '==========================================================

        Private Function ResolveProjectPath(
            targetPath As String) As String

            If String.IsNullOrWhiteSpace(
                targetPath) Then

                Throw New ArgumentException(
                    "Path file tidak boleh kosong.")

            End If

            If System.IO.Path.IsPathRooted(
                targetPath) Then

                Return System.IO.Path.GetFullPath(
                    targetPath)

            End If

            Return System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    currentProjectPath,
                    targetPath))

        End Function

        '==========================================================
        ' CHECK PATH INSIDE PROJECT
        '==========================================================

        Private Function IsInsideProject(
            filePath As String) As Boolean

            Dim projectRoot As String =
                System.IO.Path.GetFullPath(
                    currentProjectPath)

            Dim targetPath As String =
                System.IO.Path.GetFullPath(
                    filePath)

            If Not projectRoot.EndsWith(
                System.IO.Path.DirectorySeparatorChar.ToString(),
                StringComparison.Ordinal) Then

                projectRoot &=
                    System.IO.Path.DirectorySeparatorChar

            End If

            Return targetPath.StartsWith(
                projectRoot,
                StringComparison.OrdinalIgnoreCase)

        End Function

        '==========================================================
        ' RELATIVE PROJECT PATH
        '==========================================================

        Private Function GetRelativeProjectPath(
            fullPath As String) As String

            Dim projectRoot As String =
                System.IO.Path.GetFullPath(
                    currentProjectPath)

            Dim targetPath As String =
                System.IO.Path.GetFullPath(
                    fullPath)

            If Not projectRoot.EndsWith(
                System.IO.Path.DirectorySeparatorChar.ToString(),
                StringComparison.Ordinal) Then

                projectRoot &=
                    System.IO.Path.DirectorySeparatorChar

            End If

            If targetPath.StartsWith(
                projectRoot,
                StringComparison.OrdinalIgnoreCase) Then

                Return targetPath.Substring(
                    projectRoot.Length)

            End If

            Return targetPath

        End Function

        '==========================================================
        ' BUILD SCAN SUMMARY
        '==========================================================

        Private Function BuildScanSummary(
            result As ProjectScanResult) As String

            If result Is Nothing Then

                Return "Belum ada hasil scan."

            End If

            Dim text As New StringBuilder()

            text.AppendLine("PROJECT SCAN")
            text.AppendLine("============")

            text.AppendLine(
                "Name       : " &
                result.ProjectName)

            text.AppendLine(
                "Path       : " &
                result.ProjectPath)

            text.AppendLine(
                "Directories: " &
                result.TotalDirectories.ToString())

            text.AppendLine(
                "Files      : " &
                result.TotalFiles.ToString())

            text.AppendLine()

            text.AppendLine("Project Type:")

            If result.ProjectTypes.Count = 0 Then

                text.AppendLine(
                    "- Tidak terdeteksi")

            Else

                For Each projectType As String In
                    result.ProjectTypes

                    text.AppendLine(
                        "- " &
                        projectType)

                Next

            End If

            Return text.ToString()

        End Function

        '==========================================================
        ' HELP
        '==========================================================

        Private Function GetHelp() As AgentResult

            Dim text As New StringBuilder()

            text.AppendLine("DEVBOT AGENT")
            text.AppendLine("===========")
            text.AppendLine()

            text.AppendLine("Perintah Agent Build / AutoFix:")
            text.AppendLine("  test")
            text.AppendLine("  test project")
            text.AppendLine("  auto test")
            text.AppendLine("  auto fix")
            text.AppendLine("  fix build")
            text.AppendLine("  fix errors")
            text.AppendLine("  repair build")
            text.AppendLine("  repair project")
            text.AppendLine("  build")
            text.AppendLine("  build project")
            text.AppendLine("  compile project")
            text.AppendLine("  auto fix")
            text.AppendLine("  fix build")
            text.AppendLine()

            text.AppendLine("Perintah dasar:")
            text.AppendLine("  scan")
            text.AppendLine("  scan project")
            text.AppendLine("  project")
            text.AppendLine("  project info")
            text.AppendLine("  build")
            text.AppendLine("  build project")
            text.AppendLine("  compile project")
            text.AppendLine("  read index.php")
            text.AppendLine("  help")

            text.AppendLine()

            text.AppendLine("Buat file:")
            text.AppendLine(
                "  buat file test.php")

            text.AppendLine(
                "  buat file test.php dengan isi ""<?php echo 'Halo'; ?>""")

            text.AppendLine()

            text.AppendLine("Hapus file:")
            text.AppendLine(
                "  hapus file test.php")

            text.AppendLine()

            text.AppendLine("Rename file:")
            text.AppendLine(
                "  rename test.php menjadi demo.php")

            text.AppendLine(
                "  ganti nama test.php menjadi demo.php")

            text.AppendLine()

            text.AppendLine("Edit satu file:")
            text.AppendLine(
                "  ubah tulisan ""lama"" menjadi ""baru"" di index.php")

            text.AppendLine(
                "  ganti ""lama"" dengan ""baru"" di index.php")

            text.AppendLine()

            text.AppendLine("Edit seluruh project:")
            text.AppendLine(
                "  ubah tulisan ""lama"" menjadi ""baru"" di seluruh project")

            text.AppendLine(
                "  ganti ""lama"" dengan ""baru"" di seluruh proyek")

            text.AppendLine(
                "  ubah ""lama"" menjadi ""baru"" di semua project")

            text.AppendLine()

            text.AppendLine("Kemampuan file:")
            text.AppendLine("  ReadFile")
            text.AppendLine("  CreateFile")
            text.AppendLine("  UpdateFile")
            text.AppendLine("  DeleteFile")
            text.AppendLine("  RenameFile")

            text.AppendLine()

            text.AppendLine(
                "Edit otomatis membuat backup sebelum file diperbarui.")

            text.AppendLine(
                "Delete otomatis membuat backup sebelum file dihapus.")

            Return AgentResult.Success(
                text.ToString())

        End Function

    End Class

    '==============================================================
    ' CREATE FILE REQUEST
    '==============================================================

    Public Class CreateFileRequest

        Public Sub New(
            filePath As String,
            content As String)

            Me.FilePath = filePath
            Me.Content = content

        End Sub

        Public Property FilePath As String

        Public Property Content As String

    End Class

    '==============================================================
    ' RENAME FILE REQUEST
    '==============================================================

    Public Class RenameFileRequest

        Public Sub New(
            oldPath As String,
            newPath As String)

            Me.OldPath = oldPath
            Me.NewPath = newPath

        End Sub

        Public Property OldPath As String

        Public Property NewPath As String

    End Class

    '==============================================================
    ' FILE OPERATION RESULT
    '==============================================================

    Public Class FileOperationResult

        Public Sub New(
            operation As String,
            relativePath As String,
            oldContent As String,
            newContent As String)

            Me.Operation = operation
            Me.RelativePath = relativePath
            Me.OldContent = oldContent
            Me.NewContent = newContent

        End Sub

        Public Property Operation As String

        Public Property RelativePath As String

        Public Property OldContent As String

        Public Property NewContent As String

    End Class

    '==============================================================
    ' EDIT REQUEST
    '==============================================================

    Public Class EditRequest

        Public Sub New(
            filePath As String,
            oldText As String,
            newText As String)

            Me.FilePath = filePath
            Me.OldText = oldText
            Me.NewText = newText

        End Sub

        Public Property FilePath As String

        Public Property OldText As String

        Public Property NewText As String

    End Class

    '==============================================================
    ' EDIT RESULT
    '==============================================================

    Public Class EditResult

        Public Property FilePath As String

        Public Property RelativePath As String

        Public Property OldText As String

        Public Property NewText As String

        Public Property OccurrenceCount As Integer

        Public Property DiffText As String

        Public Property OriginalContent As String

        Public Property NewContent As String

    End Class

    '==============================================================
    ' PROJECT EDIT ITEM
    '==============================================================

    Public Class ProjectEditItem

        Public Property FilePath As String

        Public Property RelativePath As String

        Public Property OldText As String

        Public Property NewText As String

        Public Property OccurrenceCount As Integer

        Public Property OriginalContent As String

        Public Property NewContent As String

        Public Property DiffText As String

    End Class

    '==============================================================
    ' PROJECT EDIT RESULT
    '==============================================================

    Public Class ProjectEditResult

        Public Property OldText As String

        Public Property NewText As String

        Public Property FilesChanged As Integer

        Public Property TotalOccurrences As Integer

        Public Property Changes As List(Of ProjectEditItem)

    End Class

    '==============================================================
    ' AGENT RESULT
    '==============================================================

    Public Class AgentResult

        Private Sub New()

        End Sub

        Public Property IsSuccess As Boolean

        Public Property Message As String

        Public Property Data As Object

        Public Shared Function Success(
            message As String) As AgentResult

            Return Success(
                message,
                Nothing)

        End Function

        Public Shared Function Success(
            message As String,
            data As Object) As AgentResult

            Dim result As New AgentResult()

            result.IsSuccess =
                True

            result.Message =
                If(
                    message,
                    String.Empty)

            result.Data =
                data

            Return result

        End Function

        Public Shared Function Fail(
            message As String) As AgentResult

            Dim result As New AgentResult()

            result.IsSuccess =
                False

            result.Message =
                If(
                    message,
                    String.Empty)

            result.Data =
                Nothing

            Return result

        End Function

    End Class

End Namespace