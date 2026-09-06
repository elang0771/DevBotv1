Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports DevBot.Core.File

Namespace DevBot.Core.Agent

    Public Class AgentCommandEngine

        Private ReadOnly agentEngine As AgentEngine
        Private ReadOnly fileManager As FileManager

        Public Sub New(engine As AgentEngine)

            If engine Is Nothing Then
                Throw New ArgumentNullException(NameOf(engine))
            End If

            agentEngine = engine
            fileManager = New FileManager()

        End Sub

        Public Function ExecuteCommand(command As String) As AgentCommandResult

            If String.IsNullOrWhiteSpace(command) Then
                Return AgentCommandResult.Fail(
                    "Perintah kosong."
                )
            End If

            If Not agentEngine.HasProject() Then
                Return AgentCommandResult.Fail(
                    "Buka project terlebih dahulu sebelum menjalankan perintah."
                )
            End If

            Dim cleanCommand As String = command.Trim()

            Try

                ' =========================================================
                ' 1. EDIT SELURUH PROJECT
                ' =========================================================

                Dim wholeProjectResult As AgentCommandResult =
                    TryExecuteWholeProjectReplace(cleanCommand)

                If wholeProjectResult IsNot Nothing Then
                    Return wholeProjectResult
                End If


                ' =========================================================
                ' 2. RENAME FILE
                ' =========================================================

                Dim renameResult As AgentCommandResult =
                    TryExecuteRename(cleanCommand)

                If renameResult IsNot Nothing Then
                    Return renameResult
                End If


                ' =========================================================
                ' 3. DELETE FILE
                ' =========================================================

                Dim deleteResult As AgentCommandResult =
                    TryExecuteDelete(cleanCommand)

                If deleteResult IsNot Nothing Then
                    Return deleteResult
                End If


                ' =========================================================
                ' =========================================================
                ' 4. WRITE / ISI FILE
                ' =========================================================

                Dim writeResult As AgentCommandResult =
                    TryExecuteWrite(cleanCommand)

                If writeResult IsNot Nothing Then
                    Return writeResult
                End If


                ' 4. CREATE FILE
                ' =========================================================

                Dim createResult As AgentCommandResult =
                    TryExecuteCreate(cleanCommand)

                If createResult IsNot Nothing Then
                    Return createResult
                End If


                ' =========================================================
                ' 5. READ / SHOW FILE
                ' =========================================================

                Dim readResult As AgentCommandResult =
                    TryExecuteRead(cleanCommand)

                If readResult IsNot Nothing Then
                    Return readResult
                End If


                ' =========================================================
                ' COMMAND BELUM DIKENALI
                ' =========================================================

                Return AgentCommandResult.Fail(
                    "Perintah belum dikenali oleh Agent Command Engine." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Contoh perintah yang didukung:" &
                    Environment.NewLine &
                    "- buat file login.php" &
                    Environment.NewLine &
                    "- hapus file login.php" &
                    Environment.NewLine &
                    "- rename login.php menjadi signin.php" &
                    Environment.NewLine &
                    "- ubah tulisan ""A"" menjadi ""B"" di seluruh project" &
                    Environment.NewLine &
                    "- buka file login.php"
                )

            Catch ex As Exception

                Return AgentCommandResult.Fail(
                    "Agent gagal menjalankan perintah: " & ex.Message
                )

            End Try

        End Function


        ' ================================================================
        ' EDIT SELURUH PROJECT
        ' ================================================================

        Private Function TryExecuteWholeProjectReplace(
            command As String
        ) As AgentCommandResult

            Dim pattern As String =
                "(?i)ubah\s+(?:tulisan\s+)?[""'](.+?)[""']\s+menjadi\s+[""'](.+?)[""']\s+di\s+seluruh\s+project"

            Dim match As Match =
                Regex.Match(command, pattern)

            If Not match.Success Then
                Return Nothing
            End If

            Dim searchText As String = match.Groups(1).Value
            Dim replaceText As String = match.Groups(2).Value

            Return ReplaceInWholeProject(
                searchText,
                replaceText
            )

        End Function


        Private Function ReplaceInWholeProject(
            searchText As String,
            replaceText As String
        ) As AgentCommandResult

            Dim projectPath As String =
                agentEngine.GetCurrentProjectPath()

            If String.IsNullOrWhiteSpace(projectPath) Then
                Return AgentCommandResult.Fail(
                    "Project belum dibuka."
                )
            End If

            If Not Directory.Exists(projectPath) Then
                Return AgentCommandResult.Fail(
                    "Folder project tidak ditemukan: " & projectPath
                )
            End If

            Dim changedFiles As New List(Of ChangedFileInfo)()

            Dim files() As String =
                Directory.GetFiles(
                    projectPath,
                    "*.*",
                    SearchOption.AllDirectories
                )

            For Each filePath As String In files

                If ShouldSkipFile(filePath) Then
                    Continue For
                End If

                Dim content As String = String.Empty
                Dim errorMessage As String = String.Empty

                If Not fileManager.TryReadText(
                    filePath,
                    content,
                    errorMessage
                ) Then
                    Continue For
                End If

                If content.IndexOf(
                    searchText,
                    StringComparison.OrdinalIgnoreCase
                ) < 0 Then
                    Continue For
                End If

                Dim occurrenceCount As Integer =
                    CountOccurrences(
                        content,
                        searchText
                    )

                Dim newContent As String =
                    ReplaceIgnoreCase(
                        content,
                        searchText,
                        replaceText
                    )

                If String.Equals(
                    content,
                    newContent,
                    StringComparison.Ordinal
                ) Then
                    Continue For
                End If

                fileManager.UpdateFile(
                    filePath,
                    newContent
                )

                changedFiles.Add(
                    New ChangedFileInfo(
                        filePath,
                        occurrenceCount
                    )
                )

            Next

            If changedFiles.Count = 0 Then

                Return AgentCommandResult.Fail(
                    "Teks tidak ditemukan di file source project." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Cari : " & searchText
                )

            End If

            Dim result As New AgentCommandResult()

            result.IsSuccess = True
            result.Action = "EDIT_WHOLE_PROJECT"
            result.Message =
                BuildWholeProjectMessage(
                    searchText,
                    replaceText,
                    changedFiles
                )

            result.ChangedFiles = changedFiles

            Return result

        End Function


        ' ================================================================
        ' RENAME
        ' ================================================================

        Private Function TryExecuteRename(
            command As String
        ) As AgentCommandResult

            Dim pattern As String =
                "(?i)rename\s+(.+?)\s+menjadi\s+(.+)$"

            Dim match As Match =
                Regex.Match(command, pattern)

            If Not match.Success Then
                Return Nothing
            End If

            Dim oldName As String =
                match.Groups(1).Value.Trim()

            Dim newName As String =
                match.Groups(2).Value.Trim()

            Return RenameFile(
                oldName,
                newName
            )

        End Function


        Private Function RenameFile(
            oldName As String,
            newName As String
        ) As AgentCommandResult

            Dim projectPath As String =
                agentEngine.GetCurrentProjectPath()

            If String.IsNullOrWhiteSpace(projectPath) Then
                Return AgentCommandResult.Fail(
                    "Project belum dibuka."
                )
            End If

            Dim sourcePath As String =
                FindProjectFile(
                    projectPath,
                    oldName
                )

            If String.IsNullOrWhiteSpace(sourcePath) Then
                Return AgentCommandResult.Fail(
                    "File tidak ditemukan: " & oldName
                )
            End If

            Dim directoryPath As String =
                Path.GetDirectoryName(sourcePath)

            Dim destinationPath As String =
                Path.Combine(
                    directoryPath,
                    newName
                )

            If System.IO.File.Exists(destinationPath) Then
                Return AgentCommandResult.Fail(
                    "File tujuan sudah ada: " & destinationPath
                )
            End If

            Dim backupPath As String = String.Empty

            Try

                Dim backupService As New BackupService()

                backupPath =
                    backupService.CreateBackup(
                        sourcePath
                    )

            Catch
                ' Backup gagal tidak menghentikan rename.
            End Try

            System.IO.File.Move(
                sourcePath,
                destinationPath
            )

            Dim result As New AgentCommandResult()

            result.IsSuccess = True
            result.Action = "RENAME_FILE"
            result.Message =
                "RENAME FILE BERHASIL" &
                Environment.NewLine &
                "===================" &
                Environment.NewLine &
                Environment.NewLine &
                "Dari   : " & oldName &
                Environment.NewLine &
                "Ke     : " & newName &
                Environment.NewLine &
                "Status : file berhasil dipindahkan/rename" &
                Environment.NewLine &
                "Backup : " &
                If(
                    String.IsNullOrWhiteSpace(backupPath),
                    "tidak tersedia",
                    "dibuat otomatis"
                )

            result.ChangedFiles =
                New List(Of ChangedFileInfo) From {
                    New ChangedFileInfo(
                        destinationPath,
                        1
                    )
                }

            Return result

        End Function


        ' ================================================================
        ' DELETE
        ' ================================================================

        Private Function TryExecuteDelete(
            command As String
        ) As AgentCommandResult

            Dim pattern As String =
                "(?is)^\s*(?:hapus|delete)\s+(?:file\s+)?(.+?)\s*$"

            Dim match As Match = Regex.Match(command, pattern)

            If Not match.Success Then Return Nothing

            Dim fileName As String = match.Groups(1).Value.Trim()

            If String.IsNullOrWhiteSpace(fileName) Then Return Nothing

            Return DeleteFile(fileName)

        End Function


        Private Function DeleteFile(
            fileName As String
        ) As AgentCommandResult

            Dim projectPath As String = agentEngine.GetCurrentProjectPath()

            Dim filePath As String = FindProjectFile(projectPath, fileName)

            If String.IsNullOrWhiteSpace(filePath) Then
                Return AgentCommandResult.Fail("File tidak ditemukan: " & fileName)
            End If

            fileManager.DeleteFile(filePath, True)

            Dim result As New AgentCommandResult()
            result.IsSuccess = True
            result.Action = "DELETE_FILE"
            result.Message =
                "DELETE FILE BERHASIL" & Environment.NewLine &
                "====================" & Environment.NewLine & Environment.NewLine &
                "File   : " & fileName & Environment.NewLine &
                "Backup : dibuat otomatis" & Environment.NewLine &
                "Status : file berhasil dihapus"
            result.FilePath = filePath

            Return result

        End Function



        ' ================================================================
        ' WRITE / ISI FILE
        ' ================================================================

        Private Function TryExecuteWrite(
            command As String
        ) As AgentCommandResult

            Dim patterns As String() = {
                "(?is)^\s*isi\s+(?:file\s+)?(.+?)\s+(?:dengan|menjadi|:)[ \t]*(.*)$",
                "(?is)^\s*tulis\s+(.*?)[ \t]+ke\s+(?:file\s+)?(.+?)\s*$"
            }

            Dim match As Match = Regex.Match(command, patterns(0))

            Dim fileName As String = String.Empty
            Dim content As String = String.Empty

            If match.Success Then
                fileName = match.Groups(1).Value.Trim()
                content = CleanFileContent(match.Groups(2).Value)
            Else
                match = Regex.Match(command, patterns(1))
                If Not match.Success Then Return Nothing
                content = CleanFileContent(match.Groups(1).Value)
                fileName = match.Groups(2).Value.Trim()
            End If

            If String.IsNullOrWhiteSpace(fileName) Then Return Nothing

            Return WriteFile(fileName, content)

        End Function


        Private Function WriteFile(
            fileName As String,
            content As String
        ) As AgentCommandResult

            Dim projectPath As String = agentEngine.GetCurrentProjectPath()

            If String.IsNullOrWhiteSpace(projectPath) Then
                Return AgentCommandResult.Fail("Project belum dibuka.")
            End If

            Dim filePath As String = FindProjectFile(projectPath, fileName)

            If String.IsNullOrWhiteSpace(filePath) Then
                Return AgentCommandResult.Fail("File tidak ditemukan: " & fileName)
            End If

            fileManager.UpdateFile(filePath, content)

            Dim result As New AgentCommandResult()
            result.IsSuccess = True
            result.Action = "WRITE_FILE"
            result.Message =
                "ISI FILE BERHASIL" & Environment.NewLine &
                "==================" & Environment.NewLine & Environment.NewLine &
                "File   : " & fileName & Environment.NewLine &
                "Baris  : " & CountLines(content).ToString() & Environment.NewLine &
                "Backup : dibuat otomatis" & Environment.NewLine &
                "Status : isi file berhasil diperbarui"
            result.FilePath = filePath
            result.FileContent = content

            Return result

        End Function


        Private Function CleanFileContent(content As String) As String

            If content Is Nothing Then Return String.Empty

            Dim value As String = content.Trim()

            If value.Length >= 2 Then
                Dim firstChar As Char = value(0)
                Dim lastChar As Char = value(value.Length - 1)

                If (firstChar = System.Convert.ToChar(34) AndAlso lastChar = System.Convert.ToChar(34)) OrElse
                   (firstChar = System.Convert.ToChar(39) AndAlso lastChar = System.Convert.ToChar(39)) Then
                    value = value.Substring(1, value.Length - 2)
                End If
            End If

            Return value

        End Function


        Private Function CountLines(content As String) As Integer

            If String.IsNullOrEmpty(content) Then Return 0

            Dim normalized As String = content.Replace(System.Convert.ToChar(13).ToString() & System.Convert.ToChar(10).ToString(), System.Convert.ToChar(10).ToString()).Replace(System.Convert.ToChar(13).ToString(), System.Convert.ToChar(10).ToString())

            Return normalized.Split(System.Convert.ToChar(10)).Length

        End Function


        Private Function TryExecuteCreate(
                    command As String
                ) As AgentCommandResult

            Dim pattern As String =
                "(?is)^\s*(?:buat|create)\s+(?:file\s+)?(.+?)(?:\s+(?:dengan\s+isi|is[ií]nya|isi)\s*[:=]?\s*(.*))?\s*$"

            Dim match As Match = Regex.Match(command, pattern)

            If Not match.Success Then Return Nothing

            Dim fileName As String = match.Groups(1).Value.Trim()
            Dim content As String = String.Empty

            If match.Groups.Count > 2 AndAlso match.Groups(2).Success Then
                content = CleanFileContent(match.Groups(2).Value)
            End If

            ' Remove accidental trailing content marker from filename capture.
            If String.IsNullOrWhiteSpace(fileName) Then Return Nothing

            Return CreateFile(fileName, content)

        End Function


        Private Function CreateFile(
            fileName As String,
            content As String
        ) As AgentCommandResult

            Dim projectPath As String = agentEngine.GetCurrentProjectPath()

            If String.IsNullOrWhiteSpace(projectPath) Then
                Return AgentCommandResult.Fail("Project belum dibuka.")
            End If

            Dim fullPath As String

            If Path.IsPathRooted(fileName) Then
                fullPath = Path.GetFullPath(fileName)
            Else
                fullPath = Path.GetFullPath(Path.Combine(projectPath, fileName))
            End If

            If Not IsPathInsideProject(projectPath, fullPath) Then
                Return AgentCommandResult.Fail("Path file berada di luar project.")
            End If

            If System.IO.File.Exists(fullPath) Then
                Return AgentCommandResult.Fail("File sudah ada: " & fullPath)
            End If

            fileManager.CreateFile(fullPath, content)

            Dim result As New AgentCommandResult()
            result.IsSuccess = True
            result.Action = "CREATE_FILE"
            result.Message =
                "CREATE FILE BERHASIL" & Environment.NewLine &
                "====================" & Environment.NewLine & Environment.NewLine &
                "File   : " & fileName & Environment.NewLine &
                "Baris  : " & CountLines(content).ToString() & Environment.NewLine &
                "Status : file berhasil dibuat"
            result.FilePath = fullPath
            result.FileContent = content

            Return result

        End Function



        Private Function TryExecuteRead(
            command As String
        ) As AgentCommandResult

            Dim pattern As String =
                "(?i)(?:buka|lihat|tampilkan)\s+(?:file\s+)?(.+)$"

            Dim match As Match =
                Regex.Match(command, pattern)

            If Not match.Success Then
                Return Nothing
            End If

            Dim fileName As String =
                match.Groups(1).Value.Trim()

            Dim projectPath As String =
                agentEngine.GetCurrentProjectPath()

            Dim filePath As String =
                FindProjectFile(
                    projectPath,
                    fileName
                )

            If String.IsNullOrWhiteSpace(filePath) Then
                Return AgentCommandResult.Fail(
                    "File tidak ditemukan: " & fileName
                )
            End If

            Dim content As String =
                fileManager.ReadText(
                    filePath
                )

            Dim result As New AgentCommandResult()

            result.IsSuccess = True
            result.Action = "READ_FILE"
            result.Message =
                "FILE DIBUKA" &
                Environment.NewLine &
                "===========" &
                Environment.NewLine &
                Environment.NewLine &
                "File : " & fileName

            result.FilePath = filePath
            result.FileContent = content

            Return result

        End Function


        ' ================================================================
        ' FIND FILE
        ' ================================================================

        Private Function FindProjectFile(
            projectPath As String,
            fileName As String
        ) As String

            If String.IsNullOrWhiteSpace(projectPath) Then
                Return String.Empty
            End If

            If String.IsNullOrWhiteSpace(fileName) Then
                Return String.Empty
            End If

            If Path.IsPathRooted(fileName) Then

                Dim absolutePath As String =
                    Path.GetFullPath(fileName)

                If System.IO.File.Exists(absolutePath) AndAlso
                   IsPathInsideProject(
                       projectPath,
                       absolutePath
                   ) Then

                    Return absolutePath

                End If

                Return String.Empty

            End If

            Dim directPath As String =
                Path.GetFullPath(
                    Path.Combine(
                        projectPath,
                        fileName
                    )
                )

            If System.IO.File.Exists(directPath) Then
                Return directPath
            End If

            Dim files() As String

            Try

                files =
                    Directory.GetFiles(
                        projectPath,
                        "*.*",
                        SearchOption.AllDirectories
                    )

            Catch
                Return String.Empty
            End Try

            For Each filePath As String In files

                If ShouldSkipFile(filePath) Then
                    Continue For
                End If

                If String.Equals(
                    Path.GetFileName(filePath),
                    fileName,
                    StringComparison.OrdinalIgnoreCase
                ) Then

                    Return filePath

                End If

            Next

            Return String.Empty

        End Function


        ' ================================================================
        ' SKIP FILE / FOLDER
        ' ================================================================

        Private Function ShouldSkipFile(
            filePath As String
        ) As Boolean

            If String.IsNullOrWhiteSpace(filePath) Then
                Return True
            End If

            Dim normalized As String =
                filePath.Replace(
                    Path.DirectorySeparatorChar,
                    "/"c
                ).ToLowerInvariant()

            Dim ignoredDirectories As String() = {
                "/.git/",
                "/.svn/",
                "/.vs/",
                "/bin/",
                "/obj/",
                "/node_modules/",
                "/packages/",
                "/vendor/",
                "/__pycache__/",
                "/.idea/",
                "/.gradle/"
            }

            For Each ignored As String In ignoredDirectories

                If normalized.Contains(ignored) Then
                    Return True
                End If

            Next

            If fileManager.IsBinaryFile(
                Path.GetExtension(filePath)
            ) Then

                Return True

            End If

            Return False

        End Function


        ' ================================================================
        ' PATH SECURITY
        ' ================================================================

        Private Function IsPathInsideProject(
            projectPath As String,
            filePath As String
        ) As Boolean

            Dim projectFullPath As String =
                Path.GetFullPath(
                    projectPath
                ).TrimEnd(
                    Path.DirectorySeparatorChar
                ) & Path.DirectorySeparatorChar

            Dim fileFullPath As String =
                Path.GetFullPath(
                    filePath
                )

            Return fileFullPath.StartsWith(
                projectFullPath,
                StringComparison.OrdinalIgnoreCase
            )

        End Function


        ' ================================================================
        ' TEXT HELPERS
        ' ================================================================

        Private Function CountOccurrences(
            source As String,
            searchText As String
        ) As Integer

            If String.IsNullOrEmpty(source) Then
                Return 0
            End If

            If String.IsNullOrEmpty(searchText) Then
                Return 0
            End If

            Dim count As Integer = 0
            Dim position As Integer = 0

            While True

                Dim found As Integer =
                    source.IndexOf(
                        searchText,
                        position,
                        StringComparison.OrdinalIgnoreCase
                    )

                If found < 0 Then
                    Exit While
                End If

                count += 1
                position =
                    found + searchText.Length

                If position >= source.Length Then
                    Exit While
                End If

            End While

            Return count

        End Function


        Private Function ReplaceIgnoreCase(
            source As String,
            searchText As String,
            replaceText As String
        ) As String

            Return Regex.Replace(
                source,
                Regex.Escape(searchText),
                Function(m As Match)
                    Return replaceText
                End Function,
                RegexOptions.IgnoreCase
            )

        End Function


        ' ================================================================
        ' RESULT MESSAGE
        ' ================================================================

        Private Function BuildWholeProjectMessage(
            searchText As String,
            replaceText As String,
            changedFiles As List(Of ChangedFileInfo)
        ) As String

            Dim sb As New StringBuilder()

            sb.AppendLine(
                "EDIT SELURUH PROJECT BERHASIL"
            )

            sb.AppendLine(
                "============================="
            )

            sb.AppendLine()

            sb.AppendLine(
                "Cari       : " & searchText
            )

            sb.AppendLine(
                "Ganti      : " & replaceText
            )

            sb.AppendLine(
                "File       : " & changedFiles.Count.ToString()
            )

            Dim totalChanges As Integer = 0

            For Each item As ChangedFileInfo In changedFiles
                totalChanges += item.Occurrences
            Next

            sb.AppendLine(
                "Perubahan  : " &
                totalChanges.ToString() &
                " occurrence"
            )

            sb.AppendLine(
                "Backup     : dibuat otomatis"
            )

            sb.AppendLine(
                "Status     : seluruh file berhasil diperbarui"
            )

            sb.AppendLine()
            sb.AppendLine(
                "FILE YANG BERUBAH"
            )
            sb.AppendLine(
                "================="
            )

            For Each item As ChangedFileInfo In changedFiles

                sb.AppendLine(
                    "✓ " &
                    Path.GetFileName(item.FilePath) &
                    " (" &
                    item.Occurrences.ToString() &
                    " perubahan)"
                )

            Next

            Return sb.ToString()

        End Function

    End Class


    ' ====================================================================
    ' AGENT COMMAND RESULT
    ' ====================================================================

    Public Class AgentCommandResult

        Public Property IsSuccess As Boolean

        Public Property Action As String

        Public Property Message As String

        Public Property FilePath As String

        Public Property FileContent As String

        Public Property ChangedFiles As List(Of ChangedFileInfo)

        Public Sub New()

            ChangedFiles =
                New List(Of ChangedFileInfo)()

            FilePath = String.Empty
            FileContent = String.Empty
            Action = String.Empty
            Message = String.Empty

        End Sub


        Public Shared Function Success(
            message As String
        ) As AgentCommandResult

            Dim result As New AgentCommandResult()

            result.IsSuccess = True
            result.Message = message

            Return result

        End Function


        Public Shared Function Fail(
            message As String
        ) As AgentCommandResult

            Dim result As New AgentCommandResult()

            result.IsSuccess = False
            result.Message = message

            Return result

        End Function

    End Class


    ' ====================================================================
    ' CHANGED FILE INFO
    ' ====================================================================

    Public Class ChangedFileInfo

        Public Property FilePath As String

        Public Property Occurrences As Integer

        Public Sub New(
            filePath As String,
            occurrences As Integer
        )

            Me.FilePath = filePath
            Me.Occurrences = occurrences

        End Sub

    End Class

End Namespace