Imports System
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports DevBot.Core.File

Partial Public Class UcCodeViewer

    Private currentFilePath As String = String.Empty
    Private originalSourceCode As String = String.Empty
    Private isLoading As Boolean = False

    Private pnlDiff As Panel
    Private pnlBefore As Panel
    Private pnlAfter As Panel

    Private lblBefore As Label
    Private lblAfter As Label

    Private txtBefore As RichTextBox
    Private txtAfter As RichTextBox

    Private diffSplitter As Splitter

    Private syncingScroll As Boolean = False


    '==========================================================
    ' EVENTS
    '==========================================================

    Public Event FileSaved As EventHandler
    Public Event DiffRequested As EventHandler
    Public Event FileReloaded As EventHandler


    '==========================================================
    ' CONSTRUCTOR
    '==========================================================

    Public Sub New()

        InitializeComponent()

        ConfigureDarkViewer()

        AddHandler btnSave.Click,
            AddressOf btnSave_Click

        AddHandler btnDiff.Click,
            AddressOf btnDiff_Click

        AddHandler btnReload.Click,
            AddressOf btnReload_Click

        AddHandler txtCode.TextChanged,
            AddressOf txtCode_TextChanged

        CreateSideBySideViewer()

        SetEmptyState()

    End Sub


    '==========================================================
    ' DARK UI
    '==========================================================

    Private Sub ConfigureDarkViewer()

        Me.BackColor =
            Color.FromArgb(
                10,
                12,
                15)

        If pnlToolbar IsNot Nothing Then

            pnlToolbar.BackColor =
                Color.FromArgb(
                    24,
                    27,
                    32)

        End If

        If lblFileName IsNot Nothing Then

            lblFileName.ForeColor =
                Color.White

        End If

        If lblFilePath IsNot Nothing Then

            lblFilePath.ForeColor =
                Color.Silver

        End If

        If txtCode IsNot Nothing Then

            txtCode.BackColor =
                Color.FromArgb(
                    10,
                    12,
                    15)

            txtCode.ForeColor =
                Color.Gainsboro

            txtCode.Font =
                New Font(
                    "Consolas",
                    10.0!)

            txtCode.BorderStyle =
                BorderStyle.None

        End If

    End Sub


    '==========================================================
    ' CREATE BEFORE / AFTER
    '==========================================================

    Private Sub CreateSideBySideViewer()

        If pnlDiff IsNot Nothing Then

            Return

        End If

        '------------------------------------------------------
        ' Hide old single editor.
        '------------------------------------------------------

        If txtCode IsNot Nothing Then

            txtCode.Visible =
                False

        End If

        '------------------------------------------------------
        ' MAIN DIFF PANEL
        '------------------------------------------------------

        pnlDiff =
            New Panel()

        pnlDiff.Name =
            "pnlDiff"

        pnlDiff.Dock =
            DockStyle.Fill

        pnlDiff.BackColor =
            Color.FromArgb(
                10,
                12,
                15)

        '------------------------------------------------------
        ' BEFORE PANEL
        '------------------------------------------------------

        pnlBefore =
            New Panel()

        pnlBefore.Name =
            "pnlBefore"

        pnlBefore.Dock =
            DockStyle.Left

        pnlBefore.Width =
            350

        pnlBefore.BackColor =
            Color.FromArgb(
                10,
                12,
                15)

        lblBefore =
            New Label()

        lblBefore.Name =
            "lblBefore"

        lblBefore.Text =
            "BEFORE  •  ORIGINAL"

        lblBefore.Dock =
            DockStyle.Top

        lblBefore.Height =
            32

        lblBefore.Padding =
            New Padding(
                10,
                0,
                0,
                0)

        lblBefore.TextAlign =
            ContentAlignment.MiddleLeft

        lblBefore.Font =
            New Font(
                "Segoe UI",
                9.0!,
                FontStyle.Bold)

        lblBefore.ForeColor =
            Color.Gainsboro

        lblBefore.BackColor =
            Color.FromArgb(
                25,
                28,
                34)

        txtBefore =
            CreateCodeEditor()

        txtBefore.Name =
            "txtBefore"

        pnlBefore.Controls.Add(
            txtBefore)

        pnlBefore.Controls.Add(
            lblBefore)

        '------------------------------------------------------
        ' SPLITTER
        '------------------------------------------------------

        diffSplitter =
            New Splitter()

        diffSplitter.Name =
            "diffSplitter"

        diffSplitter.Dock =
            DockStyle.Left

        diffSplitter.Width =
            4

        diffSplitter.BackColor =
            Color.FromArgb(
                70,
                74,
                82)

        '------------------------------------------------------
        ' AFTER PANEL
        '------------------------------------------------------

        pnlAfter =
            New Panel()

        pnlAfter.Name =
            "pnlAfter"

        pnlAfter.Dock =
            DockStyle.Fill

        pnlAfter.BackColor =
            Color.FromArgb(
                10,
                12,
                15)

        lblAfter =
            New Label()

        lblAfter.Name =
            "lblAfter"

        lblAfter.Text =
            "AFTER  •  CURRENT"

        lblAfter.Dock =
            DockStyle.Top

        lblAfter.Height =
            32

        lblAfter.Padding =
            New Padding(
                10,
                0,
                0,
                0)

        lblAfter.TextAlign =
            ContentAlignment.MiddleLeft

        lblAfter.Font =
            New Font(
                "Segoe UI",
                9.0!,
                FontStyle.Bold)

        lblAfter.ForeColor =
            Color.Gainsboro

        lblAfter.BackColor =
            Color.FromArgb(
                25,
                28,
                34)

        txtAfter =
            CreateCodeEditor()

        txtAfter.Name =
            "txtAfter"

        txtAfter.ReadOnly =
            False

        pnlAfter.Controls.Add(
            txtAfter)

        pnlAfter.Controls.Add(
            lblAfter)

        '------------------------------------------------------
        ' ADD TO MAIN PANEL
        '------------------------------------------------------

        pnlDiff.Controls.Add(
            pnlAfter)

        pnlDiff.Controls.Add(
            diffSplitter)

        pnlDiff.Controls.Add(
            pnlBefore)

        '------------------------------------------------------
        ' Add to UserControl.
        '------------------------------------------------------

        Me.Controls.Add(
            pnlDiff)

        ' Toolbar must remain above diff.
        pnlToolbar.BringToFront()

        AddHandler txtBefore.VScroll,
            AddressOf txtBefore_VScroll

        AddHandler txtAfter.VScroll,
            AddressOf txtAfter_VScroll

        AddHandler Me.Resize,
            AddressOf UcCodeViewer_Resize

        AddHandler pnlDiff.Resize,
            AddressOf pnlDiff_Resize

        ' Force the initial Before/After split to 50:50.
        ' The controls are created at runtime, so the Designer cannot
        ' guarantee the first layout width.
        ApplyHalfWidth()

    End Sub


    '==========================================================
    ' CODE EDITOR
    '==========================================================

    Private Function CreateCodeEditor() As RichTextBox

        Dim box As New RichTextBox()

        box.Dock =
            DockStyle.Fill

        box.BackColor =
            Color.FromArgb(
                10,
                12,
                15)

        box.ForeColor =
            Color.Gainsboro

        box.Font =
            New Font(
                "Consolas",
                10.0!)

        box.BorderStyle =
            BorderStyle.None

        box.WordWrap =
            False

        box.ScrollBars =
            RichTextBoxScrollBars.Both

        box.DetectUrls =
            False

        box.HideSelection =
            False

        box.AcceptsTab =
            True

        box.Padding =
            New Padding(
                8)

        Return box

    End Function


    '==========================================================
    ' RESIZE
    '==========================================================

    Private Sub pnlDiff_Resize(
        sender As Object,
        e As EventArgs)

        ApplyHalfWidth()

    End Sub


    Private Sub UcCodeViewer_Resize(
        sender As Object,
        e As EventArgs)

        ApplyHalfWidth()

    End Sub


    Private Sub ApplyHalfWidth()

        Try

            If pnlDiff Is Nothing OrElse
               pnlBefore Is Nothing OrElse
               diffSplitter Is Nothing Then

                Return

            End If

            Dim availableWidth As Integer =
                pnlDiff.ClientSize.Width -
                diffSplitter.Width

            If availableWidth <= 0 Then
                Return
            End If

            ' EXACT 50:50.
            ' pnlBefore is DockStyle.Left.
            ' pnlAfter is DockStyle.Fill.
            Dim halfWidth As Integer =
                availableWidth \ 2

            If halfWidth < 1 Then
                Return
            End If

            If pnlBefore.Width <> halfWidth Then

                pnlBefore.Width =
                    halfWidth

            End If

        Catch

            ' Layout must never stop the UI.

        End Try

    End Sub


    '==========================================================
    ' SCROLL SYNC
    '==========================================================

    Private Sub txtBefore_VScroll(
        sender As Object,
        e As EventArgs)

        If syncingScroll Then
            Return
        End If

        SyncVerticalScroll(
            txtBefore,
            txtAfter)

    End Sub


    Private Sub txtAfter_VScroll(
        sender As Object,
        e As EventArgs)

        If syncingScroll Then
            Return
        End If

        SyncVerticalScroll(
            txtAfter,
            txtBefore)

    End Sub


    Private Sub SyncVerticalScroll(
        source As RichTextBox,
        target As RichTextBox)

        Try

            syncingScroll =
                True

            Dim sourceLine As Integer =
                source.GetLineFromCharIndex(
                    source.GetCharIndexFromPosition(
                        New Point(
                            0,
                            0)))

            If sourceLine < 0 Then
                sourceLine = 0
            End If

            If sourceLine >=
                target.Lines.Length Then

                sourceLine =
                    target.Lines.Length - 1

            End If

            If sourceLine < 0 Then
                Return
            End If

            Dim targetIndex As Integer =
                target.GetFirstCharIndexFromLine(
                    sourceLine)

            If targetIndex >= 0 Then

                target.Select(
                    targetIndex,
                    0)

                target.ScrollToCaret()

            End If

        Catch

        Finally

            syncingScroll =
                False

        End Try

    End Sub


    '==========================================================
    ' EMPTY STATE
    '==========================================================

    Private Sub SetEmptyState()

        currentFilePath =
            String.Empty

        originalSourceCode =
            String.Empty

        isLoading =
            True

        If lblFileName IsNot Nothing Then

            lblFileName.Text =
                "No file selected"

        End If

        If lblFilePath IsNot Nothing Then

            lblFilePath.Text =
                String.Empty

        End If

        If txtBefore IsNot Nothing Then

            txtBefore.Clear()

            txtBefore.ReadOnly =
                True

        End If

        If txtAfter IsNot Nothing Then

            txtAfter.Clear()

            txtAfter.ReadOnly =
                False

        End If

        If txtCode IsNot Nothing Then

            txtCode.Clear()

            txtCode.ReadOnly =
                True

        End If

        If btnSave IsNot Nothing Then
            btnSave.Enabled = False
        End If

        If btnDiff IsNot Nothing Then
            btnDiff.Enabled = False
        End If

        If btnReload IsNot Nothing Then
            btnReload.Enabled = False
        End If

        isLoading =
            False

    End Sub


    '==========================================================
    ' OPEN FILE
    '==========================================================

    Public Sub OpenFile(
        filePath As String)

        If String.IsNullOrWhiteSpace(
            filePath) Then

            Return

        End If

        If Not System.IO.File.Exists(
            filePath) Then

            Throw New FileNotFoundException(
                "File tidak ditemukan.",
                filePath)

        End If

        Dim info As New FileInfo(
            filePath)

        If info.Length >
            10L * 1024L * 1024L Then

            Throw New IOException(
                "File terlalu besar untuk dibuka. Maksimum 10 MB.")

        End If

        If IsBinaryExtension(
            info.Extension) Then

            Throw New IOException(
                "File binary tidak dapat ditampilkan.")

        End If

        Dim source As String =
            System.IO.File.ReadAllText(
                filePath,
                Encoding.UTF8)

        isLoading =
            True

        Try

            currentFilePath =
                Path.GetFullPath(
                    filePath)

            originalSourceCode =
                source

            lblFileName.Text =
                info.Name

            lblFilePath.Text =
                currentFilePath

            txtBefore.Text =
                source

            txtAfter.Text =
                source

            txtBefore.ReadOnly =
                True

            txtAfter.ReadOnly =
                False

            txtCode.Text =
                source

            txtCode.ReadOnly =
                False

            txtBefore.SelectionStart =
                0

            txtAfter.SelectionStart =
                0

            txtCode.SelectionStart =
                0

        Finally

            isLoading =
                False

        End Try

        UpdateEditorState()

    End Sub


    '==========================================================
    ' CURRENT FILE
    '==========================================================

    Public Function GetCurrentFilePath() As String

        Return currentFilePath

    End Function


    Public Function GetCurrentSourceCode() As String

        If txtAfter IsNot Nothing Then

            Return txtAfter.Text

        End If

        Return txtCode.Text

    End Function


    '==========================================================
    ' UNSAVED
    '==========================================================

    Public Function HasUnsavedChanges() As Boolean

        If String.IsNullOrWhiteSpace(
            currentFilePath) Then

            Return False

        End If

        Return Not String.Equals(
            GetCurrentSourceCode(),
            originalSourceCode,
            StringComparison.Ordinal)

    End Function


    Private Sub UpdateEditorState()

        Dim hasFile As Boolean =
            Not String.IsNullOrWhiteSpace(
                currentFilePath)

        Dim changed As Boolean =
            HasUnsavedChanges()

        btnSave.Enabled =
            hasFile AndAlso changed

        btnDiff.Enabled =
            hasFile AndAlso changed

        btnReload.Enabled =
            hasFile

    End Sub


    '==========================================================
    ' OLD EDITOR COMPATIBILITY
    '==========================================================

    Private Sub txtCode_TextChanged(
        sender As Object,
        e As EventArgs)

        If isLoading Then
            Return
        End If

        If txtAfter IsNot Nothing AndAlso
           txtCode IsNot Nothing Then

            If txtAfter.Text <>
               txtCode.Text Then

                txtAfter.Text =
                    txtCode.Text

            End If

        End If

        UpdateEditorState()

    End Sub


    '==========================================================
    ' SAVE
    '==========================================================

    Private Sub btnSave_Click(
        sender As Object,
        e As EventArgs)

        SaveCurrentFile()

    End Sub


    Public Function SaveCurrentFile() As Boolean

        If String.IsNullOrWhiteSpace(
            currentFilePath) Then

            Return False

        End If

        Try

            Dim backupService As New BackupService()

            Try

                backupService.CreateBackup(
                    currentFilePath)

            Catch

                ' Backup failure does not stop save.
            End Try

            System.IO.File.WriteAllText(
                currentFilePath,
                GetCurrentSourceCode(),
                New UTF8Encoding(False))

            originalSourceCode =
                GetCurrentSourceCode()

            txtBefore.Text =
                originalSourceCode

            txtAfter.Text =
                originalSourceCode

            txtCode.Text =
                originalSourceCode

            UpdateEditorState()

            RaiseEvent FileSaved(
                Me,
                EventArgs.Empty)

            Return True

        Catch ex As Exception

            MessageBox.Show(
                ex.Message,
                "DevBot - Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            Return False

        End Try

    End Function


    '==========================================================
    ' RELOAD
    '==========================================================

    Private Sub btnReload_Click(
        sender As Object,
        e As EventArgs)

        If String.IsNullOrWhiteSpace(
            currentFilePath) Then

            Return

        End If

        Try

            OpenFile(
                currentFilePath)

            RaiseEvent FileReloaded(
                Me,
                EventArgs.Empty)

        Catch ex As Exception

            MessageBox.Show(
                ex.Message,
                "DevBot - Reload",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        End Try

    End Sub


    '==========================================================
    ' DIFF
    '==========================================================

    Private Sub btnDiff_Click(
        sender As Object,
        e As EventArgs)

        ShowDiff()

    End Sub


    Public Sub ShowDiff()

        If String.IsNullOrWhiteSpace(
            currentFilePath) Then

            Return

        End If

        PaintDifferences()

        RaiseEvent DiffRequested(
            Me,
            EventArgs.Empty)

    End Sub


    '==========================================================
    ' AGENT DIFF
    '
    ' Dipakai AgentAutoFix:
    '
    ' OLD = BEFORE
    ' NEW = AFTER
    '==========================================================

    Public Sub SetAgentDiffSideBySide(
        filePath As String,
        oldContent As String,
        newContent As String,
        operation As String,
        Optional diffResult As Object = Nothing)

        Try

            currentFilePath =
                filePath

            originalSourceCode =
                If(
                    oldContent,
                    String.Empty)

            If txtBefore IsNot Nothing Then

                txtBefore.Text =
                    originalSourceCode

                txtBefore.ReadOnly =
                    True

            End If

            If txtAfter IsNot Nothing Then

                txtAfter.Text =
                    If(
                        newContent,
                        String.Empty)

                txtAfter.ReadOnly =
                    True

            End If

            If txtCode IsNot Nothing Then

                txtCode.Text =
                    If(
                        newContent,
                        String.Empty)

            End If

            If lblFileName IsNot Nothing Then

                If String.IsNullOrWhiteSpace(
                    filePath) Then

                    lblFileName.Text =
                        "AI DIFF"

                Else

                    lblFileName.Text =
                        Path.GetFileName(
                            filePath)

                End If

            End If

            If lblFilePath IsNot Nothing Then

                lblFilePath.Text =
                    filePath

            End If

            If lblBefore IsNot Nothing Then

                lblBefore.Text =
                    "BEFORE  •  ORIGINAL"

            End If

            If lblAfter IsNot Nothing Then

                lblAfter.Text =
                    "AFTER  •  " &
                    If(
                        operation,
                        "FIXED").ToUpperInvariant()

            End If

            PaintDifferences()

            If pnlDiff IsNot Nothing Then

                pnlDiff.Visible =
                    True

                pnlDiff.BringToFront()

            End If

            pnlToolbar.BringToFront()

        Catch

            ' UI diff must not stop AgentEngine.

        End Try

    End Sub


    '==========================================================
    ' COMPATIBILITY
    '==========================================================

    Public Sub SetAgentDiff(
        filePath As String,
        diffText As String,
        operation As String)

        Try

            currentFilePath =
                filePath

            originalSourceCode =
                String.Empty

            txtBefore.Text =
                String.Empty

            txtAfter.Text =
                If(
                    diffText,
                    String.Empty)

            txtAfter.ReadOnly =
                True

            lblFileName.Text =
                "AI DIFF"

            lblFilePath.Text =
                filePath

            lblBefore.Text =
                "BEFORE  •  ORIGINAL"

            lblAfter.Text =
                "AFTER  •  " &
                operation.ToUpperInvariant()

        Catch

        End Try

    End Sub


    '==========================================================
    ' PAINT DIFF
    '==========================================================

    Private Sub PaintDifferences()

        If txtBefore Is Nothing OrElse
           txtAfter Is Nothing Then

            Return

        End If

        ClearDiffColors(
            txtBefore)

        ClearDiffColors(
            txtAfter)

        Dim oldLines() As String =
            SplitLines(
                txtBefore.Text)

        Dim newLines() As String =
            SplitLines(
                txtAfter.Text)

        Dim maxLines As Integer =
            Math.Max(
                oldLines.Length,
                newLines.Length)

        For i As Integer =
            0 To maxLines - 1

            Dim oldLine As String =
                String.Empty

            Dim newLine As String =
                String.Empty

            If i <
                oldLines.Length Then

                oldLine =
                    oldLines(i)

            End If

            If i <
                newLines.Length Then

                newLine =
                    newLines(i)

            End If

            If Not String.Equals(
                oldLine,
                newLine,
                StringComparison.Ordinal) Then

                PaintLine(
                    txtBefore,
                    i,
                    Color.FromArgb(
                        70,
                        35,
                        35))

                PaintLine(
                    txtAfter,
                    i,
                    Color.FromArgb(
                        30,
                        75,
                        45))

            End If

        Next

    End Sub


    Private Sub ClearDiffColors(
        box As RichTextBox)

        If box Is Nothing Then
            Return
        End If

        Dim wasReadOnly As Boolean =
            box.ReadOnly

        box.ReadOnly =
            False

        box.SelectAll()

        box.SelectionBackColor =
            Color.FromArgb(
                10,
                12,
                15)

        box.SelectionColor =
            Color.Gainsboro

        box.SelectionStart =
            0

        box.SelectionLength =
            0

        box.ReadOnly =
            wasReadOnly

    End Sub


    Private Sub PaintLine(
        box As RichTextBox,
        lineIndex As Integer,
        background As Color)

        Try

            If lineIndex < 0 OrElse
               lineIndex >= box.Lines.Length Then

                Return

            End If

            Dim startIndex As Integer =
                box.GetFirstCharIndexFromLine(
                    lineIndex)

            If startIndex < 0 Then
                Return
            End If

            Dim length As Integer =
                box.Lines(lineIndex).Length

            If length <= 0 Then
                length = 1
            End If

            Dim wasReadOnly As Boolean =
                box.ReadOnly

            box.ReadOnly =
                False

            box.Select(
                startIndex,
                Math.Min(
                    length,
                    box.TextLength -
                    startIndex))

            box.SelectionBackColor =
                background

            box.SelectionColor =
                Color.Gainsboro

            box.SelectionStart =
                0

            box.SelectionLength =
                0

            box.ReadOnly =
                wasReadOnly

        Catch

        End Try

    End Sub


    '==========================================================
    ' SPLIT LINES
    '==========================================================

    Private Function SplitLines(
    value As String) As String()

        If value Is Nothing Then
            value = String.Empty
        End If

        Return System.Text.RegularExpressions.Regex.Split(
        value,
        "\r\n|\r|\n")

    End Function


    '==========================================================
    ' BINARY
    '==========================================================

    Private Function IsBinaryExtension(
    extension As String) As Boolean

        If String.IsNullOrWhiteSpace(
        extension) Then

            Return False

        End If

        Dim ext As String =
        extension.Trim().ToLowerInvariant()

        Select Case ext

            Case ".png"
                Return True

            Case ".jpg"
                Return True

            Case ".jpeg"
                Return True

            Case ".gif"
                Return True

            Case ".bmp"
                Return True

            Case ".ico"
                Return True

            Case ".webp"
                Return True

            Case ".exe"
                Return True

            Case ".dll"
                Return True

            Case ".zip"
                Return True

            Case ".rar"
                Return True

            Case ".7z"
                Return True

            Case ".pdf"
                Return True

            Case ".mp3"
                Return True

            Case ".mp4"
                Return True

            Case ".avi"
                Return True

            Case ".mov"
                Return True

            Case ".class"
                Return True

            Case ".jar"
                Return True

            Case ".so"
                Return True

            Case ".bin"
                Return True

        End Select

        Return False

    End Function

    '==========================================================
    ' CLEAR
    '==========================================================

    Public Sub ClearEditor()

        SetEmptyState()

    End Sub


    Public Sub FocusEditor()

        If txtAfter IsNot Nothing Then

            txtAfter.Focus()

        Else

            txtCode.Focus()

        End If

    End Sub

End Class