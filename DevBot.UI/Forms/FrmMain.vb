Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports DevBot.Core.Agent

Partial Public Class FrmMain

    Private currentProjectPath As String = String.Empty

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

    '==========================================================
    ' WINDOWS DARK SCROLLBAR SUPPORT
    '==========================================================
    '
    ' WinForms memakai scrollbar native Windows. BackColor saja
    ' tidak cukup untuk membuat scrollbar ikut dark. Method ini
    ' meminta Windows memakai theme dark pada control yang memiliki
    ' scrollbar.
    '
    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Shared Function SetWindowTheme(
        hWnd As IntPtr,
        pszSubAppName As String,
        pszSubIdList As String) As Integer
    End Function

    Private Sub ApplyDarkScrollBars(
        parentControl As Control)

        If parentControl Is Nothing Then
            Return
        End If

        Try

            If TypeOf parentControl Is TreeView OrElse
               TypeOf parentControl Is RichTextBox OrElse
               TypeOf parentControl Is TextBoxBase OrElse
               TypeOf parentControl Is TabControl OrElse
               TypeOf parentControl Is Panel Then

                If parentControl.IsHandleCreated Then

                    SetWindowTheme(
                        parentControl.Handle,
                        "",
                        "")

                    If TypeOf parentControl Is TabControl Then
                        parentControl.BackColor =
                            Color.FromArgb(8, 10, 13)
                        parentControl.ForeColor =
                            Color.White
                    End If

                End If

            End If

        Catch

            ' Dark scrollbar is a UI enhancement.
            ' Never prevent DevBot from starting.

        End Try

        For Each child As Control In
            parentControl.Controls

            ApplyDarkScrollBars(child)

        Next

    End Sub

    Private Sub ApplyDarkUiAfterLayout()

        Try

            ApplyDarkScrollBars(Me)

            If splitVertical IsNot Nothing Then
                splitVertical.BackColor =
                    Color.FromArgb(8, 10, 13)
            End If

            If splitMain IsNot Nothing Then
                splitMain.BackColor =
                    Color.FromArgb(8, 10, 13)
            End If

            If splitCenter IsNot Nothing Then
                splitCenter.BackColor =
                    Color.FromArgb(8, 10, 13)
            End If

            If tabBottom IsNot Nothing AndAlso
               tabBottom.IsHandleCreated Then

                SetWindowTheme(
                    tabBottom.Handle,
                    "",
                    "")

                tabBottom.BackColor =
                    Color.FromArgb(8, 10, 13)

                tabBottom.ForeColor =
                    Color.White

                For Each page As TabPage In tabBottom.TabPages
                    page.BackColor =
                        Color.FromArgb(8, 10, 13)
                    page.ForeColor =
                        Color.White
                Next

                tabBottom.Invalidate()

            End If

        Catch

            ' UI enhancement only.

        End Try

    End Sub

    Private Sub SetStandardInitialLayout()

        Try

            '--------------------------------------------------
            ' MAIN / OUTPUT
            ' Default: approximately 72% workspace,
            '          28% bottom console.
            '--------------------------------------------------

            If splitVertical IsNot Nothing AndAlso
               splitVertical.Width > 0 AndAlso
               splitVertical.Height > 0 Then

                Dim availableHeight As Integer =
                    splitVertical.Height -
                    splitVertical.SplitterWidth

                Dim minimumWorkspace As Integer = 200
                Dim minimumBottom As Integer = 140

                Dim desiredMain As Integer =
                    CInt(availableHeight * 0.72)

                Dim maximumMain As Integer =
                    availableHeight -
                    minimumBottom

                If maximumMain >= minimumWorkspace Then

                    desiredMain =
                        Math.Max(
                            minimumWorkspace,
                            Math.Min(
                                desiredMain,
                                maximumMain))

                    splitVertical.SplitterDistance =
                        desiredMain

                End If

            End If

            '--------------------------------------------------
            ' PROJECT EXPLORER
            ' Default approximately 270 px.
            '--------------------------------------------------

            If splitMain IsNot Nothing AndAlso
               splitMain.Width > 0 Then

                Dim availableWidth As Integer =
                    splitMain.Width -
                    splitMain.SplitterWidth

                Dim minimumExplorer As Integer = 180
                Dim minimumWorkspace As Integer = 400

                Dim desiredExplorer As Integer = 270

                Dim maximumExplorer As Integer =
                    availableWidth -
                    minimumWorkspace

                If maximumExplorer >= minimumExplorer Then

                    desiredExplorer =
                        Math.Max(
                            minimumExplorer,
                            Math.Min(
                                desiredExplorer,
                                maximumExplorer))

                    splitMain.SplitterDistance =
                        desiredExplorer

                End If

            End If

            '--------------------------------------------------
            ' CHAT / CODE
            ' Chat approximately 48% of the right workspace.
            '--------------------------------------------------

            If splitCenter IsNot Nothing AndAlso
               splitCenter.Width > 0 Then

                Dim availableWidth As Integer =
                    splitCenter.Width -
                    splitCenter.SplitterWidth

                Dim minimumChat As Integer = 250
                Dim minimumCode As Integer = 350

                Dim desiredChat As Integer =
                    CInt(availableWidth * 0.48)

                Dim maximumChat As Integer =
                    availableWidth -
                    minimumCode

                If maximumChat >= minimumChat Then

                    desiredChat =
                        Math.Max(
                            minimumChat,
                            Math.Min(
                                desiredChat,
                                maximumChat))

                    splitCenter.SplitterDistance =
                        desiredChat

                End If

            End If

        Catch

            ' Keep startup safe even on unusual screen sizes.

        End Try

    End Sub

    Public Sub New()

        InitializeComponent()

        AddHandler Me.Load,
            AddressOf FrmMain_LoadUiLayout

        AddHandler tabBottom.DrawItem,
            AddressOf tabBottom_DrawItem

        '==========================================================
        ' EVENT HANDLERS
        '==========================================================

        AddHandler btnOpenProject.Click,
            AddressOf btnOpenProject_Click

        AddHandler pnlProject.Resize,
            AddressOf pnlProject_Resize

        AddHandler tabBottom.Paint,
            AddressOf tabBottom_Paint

        AddHandler btnSettings.Click,
            AddressOf btnSettings_Click

        AddHandler btnScanProject.Click,
            AddressOf btnScanProject_Click

        AddHandler ucChat.SendCommand,
            AddressOf ucChat_SendCommand

        AddHandler tvProject.NodeMouseClick,
            AddressOf tvProject_NodeMouseClick

        AddHandler ucCodeViewer.FileSaved,
            AddressOf ucCodeViewer_FileSaved

        AddHandler ucCodeViewer.FileReloaded,
            AddressOf ucCodeViewer_FileReloaded

        AddHandler ucCodeViewer.DiffRequested,
            AddressOf ucCodeViewer_DiffRequested

    End Sub

    '==========================================================
    ' UI LAYOUT / DARK TABS
    '==========================================================

    Private Sub FrmMain_LoadUiLayout(
        sender As Object,
        e As EventArgs)

        Try

            ' The first layout pass can still have zero/temporary
            ' dimensions. Run the final sizing after WinForms has
            ' completed the initial layout.
            SetStandardInitialLayout()
            ResizeOpenProjectButton()

            If ucCodeViewer IsNot Nothing Then

                ucCodeViewer.BackColor =
                    Color.FromArgb(
                        10,
                        12,
                        15)

            End If

            Me.BeginInvoke(
                New MethodInvoker(
                    AddressOf FinalizeDarkUiLayout))

        Catch

            ' UI initialization must never prevent DevBot from opening.

        End Try

    End Sub

    Private Sub FinalizeDarkUiLayout()

        Try

            SetStandardInitialLayout()
            ResizeOpenProjectButton()

            If splitVertical IsNot Nothing Then
                splitVertical.BackColor = Color.FromArgb(12, 14, 18)
                splitVertical.Panel1.BackColor = Color.FromArgb(12, 14, 18)
                splitVertical.Panel2.BackColor = Color.FromArgb(12, 14, 18)
            End If

            If splitMain IsNot Nothing Then
                splitMain.BackColor = Color.FromArgb(12, 14, 18)
            End If

            If splitCenter IsNot Nothing Then
                splitCenter.BackColor = Color.FromArgb(12, 14, 18)
            End If

            ApplyDarkUiAfterLayout()

        Catch

            ' UI initialization must never prevent DevBot from opening.

        End Try

    End Sub

    Private Sub tabBottom_DrawItem(
        sender As Object,
        e As DrawItemEventArgs)

        If tabBottom Is Nothing Then
            Return
        End If

        Dim page As TabPage =
            tabBottom.TabPages(e.Index)

        Dim selected As Boolean =
            ((e.State And DrawItemState.Selected) =
             DrawItemState.Selected)

        Dim background As Color =
            Color.FromArgb(
                12,
                14,
                18)

        If selected Then

            background =
                Color.FromArgb(
                    25,
                    28,
                    36)

        End If

        Using brush As New SolidBrush(
            background)

            e.Graphics.FillRectangle(
                brush,
                e.Bounds)

        End Using

        Dim textColor As Color =
            Color.FromArgb(
                170,
                175,
                188)

        If selected Then

            textColor =
                Color.White

        End If

        Using brush As New SolidBrush(
            textColor)

            Dim textBounds As Rectangle =
                New Rectangle(
                    e.Bounds.X + 10,
                    e.Bounds.Y + 2,
                    e.Bounds.Width - 20,
                    e.Bounds.Height - 4)

            Using format As New StringFormat()

                format.Alignment =
                    StringAlignment.Center

                format.LineAlignment =
                    StringAlignment.Center

                e.Graphics.DrawString(
                    page.Text,
                    tabBottom.Font,
                    brush,
                    textBounds,
                    format)

            End Using

        End Using

        If selected Then

            Using accentBrush As New SolidBrush(
                Color.FromArgb(
                    120,
                    75,
                    220))

                e.Graphics.FillRectangle(
                    accentBrush,
                    e.Bounds.X,
                    e.Bounds.Bottom - 2,
                    e.Bounds.Width,
                    2)

            End Using

        End If

    End Sub

    '==========================================================
    ' CHAT COMMAND
    '==========================================================

    Private Sub ucChat_SendCommand(
        sender As Object,
        e As ChatCommandEventArgs)

        If e Is Nothing Then
            Return
        End If

        Dim command As String =
            e.Command.Trim()

        If String.IsNullOrWhiteSpace(command) Then
            Return
        End If

        '------------------------------------------------------
        ' OUTPUT CHAT COMMAND
        '------------------------------------------------------

        txtOutput.AppendText(
            "========================================" &
            Environment.NewLine)

        txtOutput.AppendText(
            "CHAT COMMAND" &
            Environment.NewLine)

        txtOutput.AppendText(
            command &
            Environment.NewLine)

        txtOutput.AppendText(
            "========================================" &
            Environment.NewLine &
            Environment.NewLine)

        txtHistory.AppendText(
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss") &
            "  CHAT COMMAND  " &
            command &
            Environment.NewLine)

        '------------------------------------------------------
        ' CEK PROJECT
        '------------------------------------------------------

        If String.IsNullOrWhiteSpace(
            currentProjectPath) OrElse
           Not Directory.Exists(
               currentProjectPath) Then

            ucChat.AppendBotMessage(
                "Buka project terlebih dahulu sebelum menjalankan perintah.")

            txtError.AppendText(
                "CHAT ERROR" &
                Environment.NewLine &
                "Belum ada project yang dibuka." &
                Environment.NewLine &
                Environment.NewLine)

            Return

        End If

        Try

            '--------------------------------------------------
            ' JALANKAN AGENT ENGINE
            '--------------------------------------------------

            txtOutput.AppendText(
                "AgentEngine menjalankan perintah..." &
                Environment.NewLine)

            Dim agentResult =
                ucChat.GetAgentEngine().Execute(command)

            If agentResult Is Nothing Then

                txtError.AppendText(
                    "AGENT ERROR" &
                    Environment.NewLine &
                    "AgentEngine mengembalikan hasil kosong." &
                    Environment.NewLine &
                    Environment.NewLine)

                ucChat.AppendBotMessage(
                    "AgentEngine tidak mengembalikan hasil.")

                Return

            End If

            '--------------------------------------------------
            ' TAMPILKAN HASIL AGENT
            '--------------------------------------------------

            txtOutput.AppendText(
                "AgentEngine selesai." &
                Environment.NewLine)

            txtOutput.AppendText(
                "Status: " &
                If(
                    agentResult.IsSuccess,
                    "BERHASIL",
                    "GAGAL") &
                Environment.NewLine &
                Environment.NewLine)

            If Not String.IsNullOrWhiteSpace(
                agentResult.Message) Then

                txtOutput.AppendText(
                    agentResult.Message &
                    Environment.NewLine &
                    Environment.NewLine)

            End If

            '--------------------------------------------------
            ' TAMPILKAN KE CHAT
            '--------------------------------------------------

            ucChat.AppendBotMessage(
                agentResult.Message)

            '--------------------------------------------------
            ' JIKA BERHASIL
            ' MAKA REFRESH EXPLORER OTOMATIS
            '--------------------------------------------------

            If agentResult.IsSuccess Then

                '--------------------------------------------------
                ' REFRESH PROJECT EXPLORER
                ' DAN CODE VIEWER SETELAH AGENT MENGUBAH FILE
                '--------------------------------------------------

                RefreshProjectExplorerAutomatically()

                RefreshCodeViewerAfterAgent()

                ShowAgentDiffIfAvailable(
                    agentResult)

            End If

        Catch ex As Exception

            txtError.AppendText(
                "AGENT EXECUTION ERROR" &
                Environment.NewLine)

            txtError.AppendText(
                ex.Message &
                Environment.NewLine &
                Environment.NewLine)

            txtOutput.AppendText(
                "❌ Agent gagal menjalankan perintah." &
                Environment.NewLine &
                ex.Message &
                Environment.NewLine &
                Environment.NewLine)

            ucChat.AppendBotMessage(
                "❌ Perintah gagal dijalankan." &
                Environment.NewLine &
                ex.Message)

        End Try

    End Sub

    '==========================================================
    ' AUTOMATIC CODE VIEWER REFRESH
    '==========================================================
    '
    ' AgentEngine dapat mengubah file langsung di disk.
    ' Code Viewer mempunyai salinan source di memory, sehingga
    ' Explorer bisa sudah berubah tetapi editor masih menampilkan
    ' isi lama.
    '
    ' Setelah Agent berhasil menjalankan command:
    ' - Jika file yang sedang dibuka masih ada -> baca ulang dari disk.
    ' - Jika file sudah dihapus -> kosongkan Code Viewer.
    ' - Jika tidak ada file yang sedang dibuka -> tidak melakukan apa-apa.
    '
    '==========================================================

    Private Sub RefreshCodeViewerAfterAgent()

        Try

            If ucCodeViewer Is Nothing Then

                Return

            End If

            Dim currentFile As String =
                ucCodeViewer.GetCurrentFilePath()

            If String.IsNullOrWhiteSpace(
                currentFile) Then

                Return

            End If

            If System.IO.File.Exists(
                currentFile) Then

                ucCodeViewer.OpenFile(
                    currentFile)

                txtOutput.AppendText(
                    "🔄 Code Viewer otomatis diperbarui: " &
                    currentFile &
                    Environment.NewLine)

                txtHistory.AppendText(
                    DateTime.Now.ToString(
                        "yyyy-MM-dd HH:mm:ss") &
                    "  AUTO REFRESH CODE  " &
                    currentFile &
                    Environment.NewLine)

            Else

                ucCodeViewer.ClearEditor()

                txtOutput.AppendText(
                    "🗑 Code Viewer: file yang sedang dibuka sudah dihapus." &
                    Environment.NewLine)

                txtHistory.AppendText(
                    DateTime.Now.ToString(
                        "yyyy-MM-dd HH:mm:ss") &
                    "  CLEAR CODE VIEWER  " &
                    currentFile &
                    Environment.NewLine)

            End If

        Catch ex As Exception

            txtError.AppendText(
                "AUTO REFRESH CODE VIEWER ERROR" &
                Environment.NewLine &
                ex.Message &
                Environment.NewLine &
                Environment.NewLine)

        End Try

    End Sub

    '==========================================================
    ' AGENT DIFF
    '==========================================================

    Private Sub ShowAgentDiffIfAvailable(
        agentResult As AgentResult)

        If agentResult Is Nothing OrElse
           agentResult.Data Is Nothing Then

            Return

        End If

        Dim changeResult As FileOperationResult =
            TryCast(
                agentResult.Data,
                FileOperationResult)

        If changeResult Is Nothing Then
            Return
        End If

        Dim filePath As String = String.Empty

        If Not String.IsNullOrWhiteSpace(
            currentProjectPath) AndAlso
           Not String.IsNullOrWhiteSpace(
               changeResult.RelativePath) Then

            filePath =
                Path.Combine(
                    currentProjectPath,
                    changeResult.RelativePath)
        End If

        '------------------------------------------------------
        ' JANGAN BUKA NOTEPAD / WINDOW EXTERNAL.
        ' DIFF DITAMPILKAN LANGSUNG DI PANEL CODE / DIFF.
        '------------------------------------------------------

        Dim oldContent As String =
            If(changeResult.OldContent,
               String.Empty)

        Dim newContent As String =
            If(changeResult.NewContent,
               String.Empty)

        Dim diffService As New DevBot.Core.Code.DiffService()

        Dim diffResult As DevBot.Core.Code.DiffResult =
            diffService.Compare(
                oldContent,
                newContent)

        Dim diffText As String =
            diffService.FormatForDisplay(
                diffResult)

        If changeResult.Operation.Equals(
            "CREATE",
            StringComparison.OrdinalIgnoreCase) AndAlso
           String.IsNullOrEmpty(newContent) Then

            diffText =
                "========== CREATE FILE ==========" &
                Environment.NewLine &
                Environment.NewLine &
                "File berhasil dibuat:" &
                Environment.NewLine &
                changeResult.RelativePath &
                Environment.NewLine &
                Environment.NewLine &
                "File masih kosong." &
                Environment.NewLine

        ElseIf changeResult.Operation.Equals(
            "DELETE",
            StringComparison.OrdinalIgnoreCase) Then

            diffText =
                "========== DELETE FILE ==========" &
                Environment.NewLine &
                Environment.NewLine &
                "File:" &
                Environment.NewLine &
                changeResult.RelativePath &
                Environment.NewLine &
                Environment.NewLine &
                diffText

        End If

        '------------------------------------------------------
        ' TAMPILKAN DI CODE / DIFF PANEL
        '------------------------------------------------------

        If ucCodeViewer IsNot Nothing Then

            If Not String.IsNullOrWhiteSpace(
                filePath) Then

                ucCodeViewer.SetAgentDiffSideBySide(
                    filePath,
                    oldContent,
                    newContent,
                    changeResult.Operation,
                    diffResult)

            Else

                ucCodeViewer.SetAgentDiffSideBySide(
                    changeResult.RelativePath,
                    oldContent,
                    newContent,
                    changeResult.Operation,
                    diffResult)

            End If

        End If

        txtOutput.AppendText(
            "AGENT DIFF DITAMPILKAN DI CODE / DIFF" &
            Environment.NewLine)

        txtOutput.AppendText(
            "Operation : " &
            changeResult.Operation &
            Environment.NewLine)

        txtOutput.AppendText(
            "File      : " &
            changeResult.RelativePath &
            Environment.NewLine &
            Environment.NewLine)

        txtHistory.AppendText(
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss") &
            "  AGENT DIFF  " &
            changeResult.Operation &
            "  " &
            changeResult.RelativePath &
            Environment.NewLine)

    End Sub

    '==========================================================
    ' PROJECT EXPLORER - OPEN PROJECT BUTTON RESIZE
    '==========================================================

    Private Sub ResizeOpenProjectButton()

        If pnlProject Is Nothing OrElse
           btnOpenProject Is Nothing Then
            Return
        End If

        Try

            Dim sideMargin As Integer = 10

            btnOpenProject.Left = sideMargin
            btnOpenProject.Top = 48
            btnOpenProject.Width =
                Math.Max(
                    40,
                    pnlProject.ClientSize.Width -
                    (sideMargin * 2))

            btnOpenProject.Height = 68

            btnOpenProject.Anchor =
                AnchorStyles.Top Or
                AnchorStyles.Left Or
                AnchorStyles.Right

        Catch

            ' UI sizing only.

        End Try

    End Sub

    Private Sub pnlProject_Resize(
        sender As Object,
        e As EventArgs)

        ResizeOpenProjectButton()

    End Sub

    '==========================================================
    ' BOTTOM TAB - FILL UNUSED HEADER AREA DARK
    '==========================================================

    Private Sub tabBottom_Paint(
        sender As Object,
        e As PaintEventArgs)

        Try

            If tabBottom Is Nothing Then
                Return
            End If

            Using brush As New SolidBrush(
                Color.FromArgb(12, 14, 18))

                e.Graphics.FillRectangle(
                    brush,
                    tabBottom.ClientRectangle)

            End Using

        Catch

            ' Painting enhancement only.

        End Try

    End Sub

    '==========================================================
    ' OPEN PROJECT
    '==========================================================

    Private Sub btnOpenProject_Click(
        sender As Object,
        e As EventArgs)

        Using dialog As New FolderBrowserDialog()

            dialog.Description =
                "Pilih folder project"

            dialog.ShowNewFolderButton =
                False

            If Not String.IsNullOrWhiteSpace(
                currentProjectPath) Then

                If Directory.Exists(
                    currentProjectPath) Then

                    dialog.SelectedPath =
                        currentProjectPath

                End If

            End If

            If dialog.ShowDialog() <>
                DialogResult.OK Then

                Return

            End If

            LoadProject(
                dialog.SelectedPath)

        End Using

    End Sub

    '==========================================================
    ' LOAD PROJECT
    '==========================================================

    Private Sub LoadProject(
        projectPath As String)

        Try

            If String.IsNullOrWhiteSpace(
                projectPath) Then

                Return

            End If

            If Not Directory.Exists(
                projectPath) Then

                Throw New DirectoryNotFoundException(
                    "Folder project tidak ditemukan:" &
                    Environment.NewLine &
                    projectPath)

            End If

            currentProjectPath =
                Path.GetFullPath(
                    projectPath)

            txtOutput.AppendText(
                "========================================" &
                Environment.NewLine)

            txtOutput.AppendText(
                "OPEN PROJECT" &
                Environment.NewLine)

            txtOutput.AppendText(
                currentProjectPath &
                Environment.NewLine)

            txtOutput.AppendText(
                "Membaca struktur project..." &
                Environment.NewLine)

            '--------------------------------------------------
            ' AGENT ENGINE MENGGUNAKAN PROJECT INI
            '--------------------------------------------------

            Try

                Dim agentResult =
                    ucChat.GetAgentEngine().OpenProject(
                        currentProjectPath)

                If agentResult IsNot Nothing Then

                    txtOutput.AppendText(
                        "AgentEngine: " &
                        agentResult.Message &
                        Environment.NewLine)

                End If

            Catch agentEx As Exception

                txtError.AppendText(
                    "AgentEngine warning:" &
                    Environment.NewLine &
                    agentEx.Message &
                    Environment.NewLine &
                    Environment.NewLine)

            End Try

            '--------------------------------------------------
            ' BUILD PROJECT EXPLORER
            '--------------------------------------------------

            RefreshProjectExplorer(
                False)

            '--------------------------------------------------
            ' PROJECT SUMMARY
            '--------------------------------------------------

            Dim fileCount As Integer = 0
            Dim directoryCount As Integer = 0

            CountProjectItems(
                currentProjectPath,
                fileCount,
                directoryCount)

            txtOutput.AppendText(
                "PROJECT SUMMARY" &
                Environment.NewLine)

            txtOutput.AppendText(
                "Name        : " &
                New DirectoryInfo(
                    currentProjectPath).Name &
                Environment.NewLine)

            txtOutput.AppendText(
                "Path        : " &
                currentProjectPath &
                Environment.NewLine)

            txtOutput.AppendText(
                "Directories : " &
                directoryCount.ToString() &
                Environment.NewLine)

            txtOutput.AppendText(
                "Files       : " &
                fileCount.ToString() &
                Environment.NewLine &
                Environment.NewLine)

            txtHistory.AppendText(
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss") &
                "  OPEN PROJECT  " &
                currentProjectPath &
                Environment.NewLine)

            btnScanProject.Enabled =
                True

            ucChat.AppendBotMessage(
                "📂 Project berhasil dibuka." &
                Environment.NewLine &
                Environment.NewLine &
                "Project Explorer sudah diperbarui." &
                Environment.NewLine &
                "AgentEngine sudah menggunakan project ini." &
                Environment.NewLine &
                Environment.NewLine &
                "Klik file untuk melihat source code.")

        Catch ex As Exception

            txtError.AppendText(
                "OPEN PROJECT ERROR" &
                Environment.NewLine &
                ex.Message &
                Environment.NewLine &
                Environment.NewLine)

            txtOutput.AppendText(
                "❌ Gagal membuka project: " &
                ex.Message &
                Environment.NewLine)

            MessageBox.Show(
                ex.Message,
                "DevBot - Open Project",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        End Try

    End Sub

    '==========================================================
    ' MANUAL SCAN PROJECT
    '==========================================================

    Private Sub btnScanProject_Click(
        sender As Object,
        e As EventArgs)

        If String.IsNullOrWhiteSpace(
            currentProjectPath) OrElse
           Not Directory.Exists(
               currentProjectPath) Then

            ucChat.AppendBotMessage(
                "Buka project terlebih dahulu sebelum scan.")

            Return

        End If

        RefreshProjectExplorer(
            True)

    End Sub

    '==========================================================
    ' AUTOMATIC PROJECT EXPLORER REFRESH
    '==========================================================

    Private Sub RefreshProjectExplorerAutomatically()

        If String.IsNullOrWhiteSpace(
            currentProjectPath) Then

            Return

        End If

        If Not Directory.Exists(
            currentProjectPath) Then

            Return

        End If

        Try

            RefreshProjectExplorer(
                True)

            txtOutput.AppendText(
                "🔄 Project Explorer otomatis diperbarui." &
                Environment.NewLine &
                Environment.NewLine)

            txtHistory.AppendText(
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss") &
                "  AUTO REFRESH EXPLORER  " &
                currentProjectPath &
                Environment.NewLine)

        Catch ex As Exception

            txtError.AppendText(
                "AUTO REFRESH ERROR" &
                Environment.NewLine &
                ex.Message &
                Environment.NewLine &
                Environment.NewLine)

        End Try

    End Sub

    '==========================================================
    ' REFRESH PROJECT EXPLORER
    '==========================================================

    Private Sub RefreshProjectExplorer(
        showOutput As Boolean)

        If String.IsNullOrWhiteSpace(
            currentProjectPath) Then

            Return

        End If

        If Not Directory.Exists(
            currentProjectPath) Then

            Return

        End If

        If showOutput Then

            txtOutput.AppendText(
                "Membaca ulang struktur project..." &
                Environment.NewLine)

        End If

        tvProject.BeginUpdate()

        Try

            tvProject.Nodes.Clear()

            Dim rootNode As TreeNode =
                BuildDirectoryTree(
                    currentProjectPath)

            tvProject.Nodes.Add(
                rootNode)

            rootNode.Expand()

        Finally

            tvProject.EndUpdate()

        End Try

        btnScanProject.Enabled =
            True

        If showOutput Then

            txtOutput.AppendText(
                "Project Explorer diperbarui." &
                Environment.NewLine)

        End If

    End Sub

    '==========================================================
    ' BUILD DIRECTORY TREE
    '==========================================================

    Private Function BuildDirectoryTree(
        directoryPath As String) As TreeNode

        Dim directoryInfo As New DirectoryInfo(
            directoryPath)

        Dim rootNode As New TreeNode(
            "📁 " &
            directoryInfo.Name)

        rootNode.Tag =
            directoryInfo.FullName

        AddDirectoryChildren(
            directoryInfo,
            rootNode)

        Return rootNode

    End Function

    '==========================================================
    ' ADD DIRECTORY CHILDREN
    '==========================================================

    Private Sub AddDirectoryChildren(
        directoryInfo As DirectoryInfo,
        parentNode As TreeNode)

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

            Dim directoryNode As New TreeNode(
                "📁 " &
                childDirectory.Name)

            directoryNode.Tag =
                childDirectory.FullName

            parentNode.Nodes.Add(
                directoryNode)

            AddDirectoryChildren(
                childDirectory,
                directoryNode)

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

            Dim fileNode As New TreeNode(
                "📄 " &
                fileInfo.Name)

            fileNode.Tag =
                fileInfo.FullName

            parentNode.Nodes.Add(
                fileNode)

        Next

    End Sub

    '==========================================================
    ' DIRECTORY SORT
    '==========================================================

    Private Function CompareDirectories(
        x As DirectoryInfo,
        y As DirectoryInfo) As Integer

        Return StringComparer.OrdinalIgnoreCase.Compare(
            x.Name,
            y.Name)

    End Function

    '==========================================================
    ' FILE SORT
    '==========================================================

    Private Function CompareFiles(
        x As FileInfo,
        y As FileInfo) As Integer

        Return StringComparer.OrdinalIgnoreCase.Compare(
            x.Name,
            y.Name)

    End Function

    '==========================================================
    ' IGNORED DIRECTORY
    '==========================================================

    Private Function IsIgnoredDirectory(
        directoryName As String) As Boolean

        If String.IsNullOrWhiteSpace(
            directoryName) Then

            Return False

        End If

        For Each ignoredName As String In
            ignoredDirectories

            If String.Equals(
                directoryName,
                ignoredName,
                StringComparison.OrdinalIgnoreCase) Then

                Return True

            End If

        Next

        Return False

    End Function

    '==========================================================
    ' COUNT PROJECT ITEMS
    '==========================================================

    Private Sub CountProjectItems(
        directoryPath As String,
        ByRef fileCount As Integer,
        ByRef directoryCount As Integer)

        Dim directories As DirectoryInfo()

        Try

            directories =
                New DirectoryInfo(
                    directoryPath).GetDirectories()

        Catch ex As UnauthorizedAccessException

            Return

        Catch ex As IOException

            Return

        End Try

        For Each childDirectory As DirectoryInfo In
            directories

            If IsIgnoredDirectory(
                childDirectory.Name) Then

                Continue For

            End If

            directoryCount +=
                1

            CountProjectItems(
                childDirectory.FullName,
                fileCount,
                directoryCount)

        Next

        Dim files As FileInfo()

        Try

            files =
                New DirectoryInfo(
                    directoryPath).GetFiles()

        Catch ex As UnauthorizedAccessException

            Return

        Catch ex As IOException

            Return

        End Try

        fileCount +=
            files.Length

    End Sub

    '==========================================================
    ' TREEVIEW FILE CLICK
    '==========================================================

    Private Sub tvProject_NodeMouseClick(
        sender As Object,
        e As TreeNodeMouseClickEventArgs)

        If e Is Nothing Then
            Return
        End If

        If e.Node Is Nothing Then
            Return
        End If

        If e.Node.Tag Is Nothing Then
            Return
        End If

        Dim selectedPath As String =
            TryCast(
                e.Node.Tag,
                String)

        If String.IsNullOrWhiteSpace(
            selectedPath) Then

            Return

        End If

        If Directory.Exists(
            selectedPath) Then

            Return

        End If

        If Not File.Exists(
            selectedPath) Then

            Return

        End If

        LoadFileIntoCodeViewer(
            selectedPath)

    End Sub

    '==========================================================
    ' LOAD FILE INTO CODE VIEWER
    '==========================================================

    Private Sub LoadFileIntoCodeViewer(
        filePath As String)

        Try

            Dim fileInfo As New FileInfo(
                filePath)

            If fileInfo.Length >
                5 * 1024 * 1024 Then

                txtError.AppendText(
                    "File terlalu besar untuk ditampilkan:" &
                    Environment.NewLine &
                    filePath &
                    Environment.NewLine &
                    Environment.NewLine)

                MessageBox.Show(
                    "File lebih besar dari 5 MB dan tidak ditampilkan.",
                    "DevBot - Code Viewer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                Return

            End If

            If IsBinaryExtension(
                fileInfo.Extension) Then

                txtError.AppendText(
                    "File binary dipilih:" &
                    Environment.NewLine &
                    filePath &
                    Environment.NewLine &
                    Environment.NewLine)

                MessageBox.Show(
                    "File binary tidak dapat ditampilkan sebagai source code.",
                    "DevBot - Code Viewer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                Return

            End If

            ucCodeViewer.OpenFile(
                filePath)

            txtOutput.AppendText(
                "File dibuka di Code Viewer: " &
                filePath &
                Environment.NewLine)

            txtHistory.AppendText(
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss") &
                "  OPEN FILE  " &
                filePath &
                Environment.NewLine)

        Catch ex As Exception

            txtError.AppendText(
                "READ FILE ERROR" &
                Environment.NewLine &
                filePath &
                Environment.NewLine &
                ex.Message &
                Environment.NewLine &
                Environment.NewLine)

            MessageBox.Show(
                ex.Message,
                "DevBot - Code Viewer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        End Try

    End Sub

    '==========================================================
    ' BINARY FILE CHECK
    '==========================================================

    Private Function IsBinaryExtension(
        extension As String) As Boolean

        If String.IsNullOrWhiteSpace(
            extension) Then

            Return False

        End If

        Select Case extension.ToLowerInvariant()

            Case ".png",
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

                Return True

        End Select

        Return False

    End Function

    '==========================================================
    ' CODE VIEWER - SAVE
    '==========================================================

    Private Sub ucCodeViewer_FileSaved(
        sender As Object,
        e As EventArgs)

        txtOutput.AppendText(
            "Code Viewer: file berhasil disimpan." &
            Environment.NewLine)

        txtHistory.AppendText(
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss") &
            "  SAVE FILE  " &
            ucCodeViewer.GetCurrentFilePath() &
            Environment.NewLine)

        '------------------------------------------------------
        ' FILE DIEDIT DARI CODE VIEWER
        ' EXPLORER JUGA DIREFRESH
        '------------------------------------------------------

        RefreshProjectExplorerAutomatically()

    End Sub

    '==========================================================
    ' CODE VIEWER - RELOAD
    '==========================================================

    Private Sub ucCodeViewer_FileReloaded(
        sender As Object,
        e As EventArgs)

        txtOutput.AppendText(
            "Code Viewer: file berhasil di-reload." &
            Environment.NewLine)

        txtHistory.AppendText(
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss") &
            "  RELOAD FILE  " &
            ucCodeViewer.GetCurrentFilePath() &
            Environment.NewLine)

    End Sub

    '==========================================================
    ' CODE VIEWER - DIFF
    '==========================================================

    Private Sub ucCodeViewer_DiffRequested(
        sender As Object,
        e As EventArgs)

        txtOutput.AppendText(
            "Code Viewer: Diff dibuka." &
            Environment.NewLine)

        txtHistory.AppendText(
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss") &
            "  SHOW DIFF  " &
            ucCodeViewer.GetCurrentFilePath() &
            Environment.NewLine)

    End Sub

    '==========================================================
    ' SETTINGS
    '==========================================================

    Private Sub btnSettings_Click(
        sender As Object,
        e As EventArgs)

        MessageBox.Show(
            "Settings akan kita buat pada tahap berikutnya.",
            "DevBot Settings",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information)

    End Sub

End Class