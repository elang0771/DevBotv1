Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports DevBot.Core.Agent

Partial Public Class UcChat

    Private ReadOnly agentEngine As AgentEngine
    Private ReadOnly agentTestCommand As AgentTestCommand

    Public Event SendCommand As EventHandler(Of ChatCommandEventArgs)

    '==========================================================
    ' COLORS
    '==========================================================

    Private ReadOnly ChatBackground As Color =
        Color.FromArgb(10, 11, 14)

    Private ReadOnly MessageBackground As Color =
        Color.FromArgb(18, 20, 25)

    Private ReadOnly InputBackground As Color =
        Color.FromArgb(25, 27, 32)

    Private ReadOnly ChatTextColor As Color =
        Color.Gainsboro

    Private ReadOnly SenderTextColor As Color =
        Color.White

    '==========================================================
    ' CONSTRUCTOR
    '==========================================================

    Public Sub New()

        InitializeComponent()

        agentEngine =
            New AgentEngine()

        agentTestCommand =
            New AgentTestCommand()

        ConfigureChat()

        AddHandler btnSend.Click,
            AddressOf btnSend_Click

        AddHandler txtCommand.KeyDown,
            AddressOf txtCommand_KeyDown

        AddHandler Me.Resize,
            AddressOf UcChat_Resize

        AddHandler flpMessages.ControlAdded,
            AddressOf flpMessages_ControlAdded

        ShowWelcomeMessage()

    End Sub

    '==========================================================
    ' CHAT CONFIGURATION
    '==========================================================

    Private Sub ConfigureChat()

        Me.BackColor =
            ChatBackground

        flpMessages.AutoScroll =
            True

        flpMessages.FlowDirection =
            FlowDirection.TopDown

        flpMessages.WrapContents =
            False

        flpMessages.Padding =
            New Padding(
                8,
                8,
                8,
                8)

        flpMessages.Margin =
            New Padding(0)

        flpMessages.BackColor =
            ChatBackground

        ' Multiline:
        ' Enter       = Send
        ' Shift+Enter = New Line

        txtCommand.Multiline =
            True

        txtCommand.AcceptsReturn =
            True

        txtCommand.AcceptsTab =
            True

        txtCommand.WordWrap =
            True

        txtCommand.ScrollBars =
            ScrollBars.Vertical

        txtCommand.BackColor =
            InputBackground

        txtCommand.ForeColor =
            ChatTextColor

        txtCommand.BorderStyle =
            BorderStyle.FixedSingle

        txtCommand.Visible =
            True

        txtCommand.Enabled =
            True

        If pnlChatInput IsNot Nothing Then

            pnlChatInput.Visible =
                True

            pnlChatInput.Dock =
                DockStyle.Bottom

            pnlChatInput.Height =
                72

            pnlChatInput.MinimumSize =
                New Size(
                    0,
                    72)

            pnlChatInput.BackColor =
                ChatBackground

            pnlChatInput.Padding =
                New Padding(8)

            pnlChatInput.BringToFront()

        End If

        If btnSend IsNot Nothing Then

            btnSend.Visible =
                True

            btnSend.Enabled =
                True

            btnSend.Text =
                "Send"

            btnSend.BackColor =
                Color.FromArgb(
                    45,
                    48,
                    58)

            btnSend.ForeColor =
                Color.White

            btnSend.UseVisualStyleBackColor =
                False

        End If

        LayoutChatInput()

    End Sub

    '==========================================================
    ' CHAT RESIZE
    '==========================================================

    Private Sub UcChat_Resize(
        sender As Object,
        e As EventArgs)

        ResizeMessagePanels()
        LayoutChatInput()

    End Sub

    '==========================================================
    ' INPUT LAYOUT
    '==========================================================

    Private Sub LayoutChatInput()

        If pnlChatInput Is Nothing OrElse
           txtCommand Is Nothing OrElse
           btnSend Is Nothing Then

            Return

        End If

        If pnlChatInput.ClientSize.Width <= 0 OrElse
           pnlChatInput.ClientSize.Height <= 0 Then

            Return

        End If

        Dim leftMargin As Integer = 8
        Dim topMargin As Integer = 8
        Dim rightMargin As Integer = 8
        Dim gap As Integer = 8
        Dim sendWidth As Integer = 70

        Dim controlHeight As Integer =
            pnlChatInput.ClientSize.Height -
            topMargin -
            8

        If controlHeight < 32 Then
            controlHeight = 32
        End If

        Dim inputWidth As Integer =
            pnlChatInput.ClientSize.Width -
            leftMargin -
            rightMargin -
            gap -
            sendWidth

        If inputWidth < 100 Then
            inputWidth = 100
        End If

        txtCommand.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Left Or
            AnchorStyles.Right Or
            AnchorStyles.Bottom

        txtCommand.Location =
            New Point(
                leftMargin,
                topMargin)

        txtCommand.Size =
            New Size(
                inputWidth,
                controlHeight)

        btnSend.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Right

        btnSend.Location =
            New Point(
                pnlChatInput.ClientSize.Width -
                rightMargin -
                sendWidth,
                topMargin)

        btnSend.Size =
            New Size(
                sendWidth,
                controlHeight)

        txtCommand.BringToFront()
        btnSend.BringToFront()

    End Sub

    '==========================================================
    ' RESIZE MESSAGE PANELS
    '==========================================================

    Private Sub ResizeMessagePanels()

        If flpMessages Is Nothing Then
            Return
        End If

        Dim availableWidth As Integer =
            flpMessages.ClientSize.Width -
            flpMessages.Padding.Left -
            flpMessages.Padding.Right -
            25

        If availableWidth < 150 Then
            Return
        End If

        flpMessages.SuspendLayout()

        Try

            For Each control As Control In
                flpMessages.Controls

                Dim messagePanel As Panel =
                    TryCast(
                        control,
                        Panel)

                If messagePanel Is Nothing Then
                    Continue For
                End If

                messagePanel.Width =
                    availableWidth

                messagePanel.BackColor =
                    MessageBackground

                StyleMessagePanel(
                    messagePanel)

                UpdateMessagePanelHeight(
                    messagePanel)

            Next

        Finally

            flpMessages.ResumeLayout(
                True)

        End Try

    End Sub

    '==========================================================
    ' NEW MESSAGE CONTROL
    '==========================================================

    Private Sub flpMessages_ControlAdded(
        sender As Object,
        e As ControlEventArgs)

        If e Is Nothing OrElse
           e.Control Is Nothing Then

            Return

        End If

        Dim messagePanel As Panel =
            TryCast(
                e.Control,
                Panel)

        If messagePanel Is Nothing Then
            Return
        End If

        StyleMessagePanel(
            messagePanel)

    End Sub

    '==========================================================
    ' MESSAGE THEME
    '==========================================================

    Private Sub StyleMessagePanel(
        messagePanel As Panel)

        If messagePanel Is Nothing Then
            Return
        End If

        messagePanel.BackColor =
            MessageBackground

        For Each control As Control In
            messagePanel.Controls

            If TypeOf control Is Label Then

                Dim labelControl As Label =
                    DirectCast(
                        control,
                        Label)

                labelControl.BackColor =
                    Color.Transparent

                If labelControl.Font.Bold Then

                    labelControl.ForeColor =
                        SenderTextColor

                Else

                    labelControl.ForeColor =
                        ChatTextColor

                End If

            Else

                control.BackColor =
                    MessageBackground

                control.ForeColor =
                    ChatTextColor

            End If

        Next

    End Sub

    '==========================================================
    ' WELCOME MESSAGE
    '==========================================================

    Private Sub ShowWelcomeMessage()

        flpMessages.Controls.Clear()

        AppendBotMessage(
            "Halo Bos 👋" &
            Environment.NewLine &
            Environment.NewLine &
            "Saya DevBot." &
            Environment.NewLine &
            "AgentEngine sudah aktif." &
            Environment.NewLine &
            Environment.NewLine &
            "Perintah yang tersedia:" &
            Environment.NewLine &
            "• help" &
            Environment.NewLine &
            "• project" &
            Environment.NewLine &
            "• scan" &
            Environment.NewLine &
            Environment.NewLine &
            "Buka project terlebih dahulu sebelum scan.")

    End Sub

    '==========================================================
    ' SEND BUTTON
    '==========================================================

    Private Sub btnSend_Click(
        sender As Object,
        e As EventArgs)

        SendCurrentCommand()

    End Sub

    '==========================================================
    ' ENTER / SHIFT+ENTER
    '==========================================================

    Private Sub txtCommand_KeyDown(
        sender As Object,
        e As KeyEventArgs)

        If e.KeyCode <>
            Keys.Enter Then

            Return

        End If

        ' Shift + Enter = newline.
        If e.Shift Then

            e.Handled =
                False

            e.SuppressKeyPress =
                False

            Return

        End If

        ' Enter biasa = Send.
        e.SuppressKeyPress =
            True

        e.Handled =
            True

        SendCurrentCommand()

    End Sub

    '==========================================================
    ' SEND CURRENT COMMAND
    '==========================================================

    Private Sub SendCurrentCommand()

        Dim command As String =
            txtCommand.Text.Trim()

        If String.IsNullOrWhiteSpace(
            command) Then

            Return

        End If

        AppendUserMessage(
            command)

        txtCommand.Clear()

        SetInputEnabled(
            False)

        Try

            Dim args As New ChatCommandEventArgs(
                command)

            RaiseEvent SendCommand(
                Me,
                args)

            Dim result As AgentResult = Nothing

            '==================================================
            ' TEST ENGINE COMMAND
            '
            ' test
            ' test project
            ' run test
            ' run tests
            '
            ' browser test <url>
            ' test <url>
            '==================================================

            If IsTestCommand(
                command) Then

                Dim testUrl As String =
                    ExtractTestUrl(
                        command)

                If String.IsNullOrWhiteSpace(
                    testUrl) Then

                    result =
                        agentTestCommand.Run(
                            agentEngine.GetCurrentProjectPath())

                Else

                    result =
                        agentTestCommand.RunWithUrl(
                            agentEngine.GetCurrentProjectPath(),
                            testUrl)

                End If

            Else

                result =
                    agentEngine.Execute(
                        command)

            End If

            If result Is Nothing Then

                AppendBotMessage(
                    "Agent tidak mengembalikan hasil.")

            ElseIf result.IsSuccess Then

                AppendBotMessage(
                    result.Message)

            Else

                AppendBotMessage(
                    "❌ " &
                    result.Message)

            End If

        Catch ex As Exception

            AppendBotMessage(
                "❌ Terjadi error pada Agent:" &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message)

        Finally

            SetInputEnabled(
                True)

            txtCommand.Focus()

        End Try

    End Sub

    '==========================================================
    ' DETECT TEST COMMAND
    '==========================================================

    Private Function IsTestCommand(
        command As String) As Boolean

        If String.IsNullOrWhiteSpace(
            command) Then

            Return False

        End If

        Dim text As String =
            command.Trim()

        If text.Equals(
            "test",
            StringComparison.OrdinalIgnoreCase) Then

            Return True

        End If

        If text.Equals(
            "test project",
            StringComparison.OrdinalIgnoreCase) Then

            Return True

        End If

        If text.Equals(
            "run test",
            StringComparison.OrdinalIgnoreCase) Then

            Return True

        End If

        If text.Equals(
            "run tests",
            StringComparison.OrdinalIgnoreCase) Then

            Return True

        End If

        If text.StartsWith(
            "test ",
            StringComparison.OrdinalIgnoreCase) Then

            Return True

        End If

        If text.StartsWith(
            "browser test ",
            StringComparison.OrdinalIgnoreCase) Then

            Return True

        End If

        Return False

    End Function

    '==========================================================
    ' EXTRACT TEST URL
    '==========================================================

    Private Function ExtractTestUrl(
        command As String) As String

        If String.IsNullOrWhiteSpace(
            command) Then

            Return String.Empty

        End If

        Dim text As String =
            command.Trim()

        If text.StartsWith(
            "browser test ",
            StringComparison.OrdinalIgnoreCase) Then

            Return text.Substring(
                "browser test ".Length).Trim()

        End If

        If text.StartsWith(
            "test ",
            StringComparison.OrdinalIgnoreCase) Then

            Dim value As String =
                text.Substring(
                    "test ".Length).Trim()

            If value.StartsWith(
                "http://",
                StringComparison.OrdinalIgnoreCase) OrElse
               value.StartsWith(
                "https://",
                StringComparison.OrdinalIgnoreCase) Then

                Return value

            End If

        End If

        Return String.Empty

    End Function

    '==========================================================
    ' OPEN PROJECT
    '==========================================================

    Public Function OpenProject(
        projectPath As String) As Boolean

        If String.IsNullOrWhiteSpace(
            projectPath) Then

            Return False

        End If

        Try

            Dim result As AgentResult =
                agentEngine.OpenProject(
                    projectPath)

            If result Is Nothing Then

                AppendBotMessage(
                    "❌ Agent tidak mengembalikan hasil.")

                Return False

            End If

            If result.IsSuccess Then

                AppendBotMessage(
                    "📂 " &
                    result.Message)

                Return True

            End If

            AppendBotMessage(
                "❌ " &
                result.Message)

            Return False

        Catch ex As Exception

            AppendBotMessage(
                "❌ Gagal membuka project:" &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message)

            Return False

        End Try

    End Function

    '==========================================================
    ' SCAN PROJECT
    '==========================================================

    Public Function ScanProject() As Boolean

        Try

            AppendUserMessage(
                "scan")

            Dim result As AgentResult =
                agentEngine.ScanProject()

            If result Is Nothing Then

                AppendBotMessage(
                    "Agent tidak mengembalikan hasil.")

                Return False

            End If

            If result.IsSuccess Then

                AppendBotMessage(
                    result.Message)

                Return True

            End If

            AppendBotMessage(
                "❌ " &
                result.Message)

            Return False

        Catch ex As Exception

            AppendBotMessage(
                "❌ Gagal scan project:" &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message)

            Return False

        End Try

    End Function

    '==========================================================
    ' GET AGENT ENGINE
    '==========================================================

    Public Function GetAgentEngine() As AgentEngine

        Return agentEngine

    End Function

    '==========================================================
    ' GET CURRENT PROJECT
    '==========================================================

    Public Function GetCurrentProjectPath() As String

        Return agentEngine.GetCurrentProjectPath()

    End Function

    '==========================================================
    ' CLEAR CHAT
    '==========================================================

    Public Sub ClearChat()

        ShowWelcomeMessage()

    End Sub

    '==========================================================
    ' USER MESSAGE
    '==========================================================

    Public Sub AppendUserMessage(
        message As String)

        If String.IsNullOrWhiteSpace(
            message) Then

            Return

        End If

        AppendMessage(
            "👤 Bos",
            message)

    End Sub

    '==========================================================
    ' BOT MESSAGE
    '==========================================================

    Public Sub AppendBotMessage(
        message As String)

        If message Is Nothing Then

            message =
                String.Empty

        End If

        AppendMessage(
            "🤖 DevBot",
            message)

    End Sub

    '==========================================================
    ' APPEND MESSAGE
    '==========================================================

    Private Sub AppendMessage(
        senderName As String,
        message As String)

        If flpMessages Is Nothing Then
            Return
        End If

        Dim availableWidth As Integer =
            flpMessages.ClientSize.Width -
            flpMessages.Padding.Left -
            flpMessages.Padding.Right -
            25

        If availableWidth < 150 Then

            availableWidth = 500

        End If

        Dim messagePanel As New Panel()

        messagePanel.Width =
            availableWidth

        messagePanel.Margin =
            New Padding(
                0,
                0,
                0,
                10)

        messagePanel.Padding =
            New Padding(10)

        messagePanel.BackColor =
            MessageBackground

        Dim lblSender As New Label()

        lblSender.Dock =
            DockStyle.Top

        lblSender.AutoSize =
            False

        lblSender.Height =
            24

        lblSender.Font =
            New Font(
                "Segoe UI",
                9.0!,
                FontStyle.Bold)

        lblSender.Text =
            senderName

        lblSender.ForeColor =
            SenderTextColor

        lblSender.BackColor =
            Color.Transparent

        lblSender.TextAlign =
            ContentAlignment.MiddleLeft

        Dim lblMessage As New Label()

        lblMessage.Dock =
            DockStyle.Fill

        lblMessage.AutoSize =
            False

        lblMessage.Font =
            New Font(
                "Segoe UI",
                9.0!,
                FontStyle.Regular)

        lblMessage.Text =
            message

        lblMessage.ForeColor =
            ChatTextColor

        lblMessage.BackColor =
            Color.Transparent

        lblMessage.TextAlign =
            ContentAlignment.TopLeft

        lblMessage.Padding =
            New Padding(
                0,
                4,
                0,
                4)

        messagePanel.Controls.Add(
            lblMessage)

        messagePanel.Controls.Add(
            lblSender)

        ' Add handler before adding to FlowLayoutPanel is not
        ' needed because the controls are already styled above.
        flpMessages.Controls.Add(
            messagePanel)

        StyleMessagePanel(
            messagePanel)

        UpdateMessagePanelHeight(
            messagePanel)

        flpMessages.PerformLayout()

        flpMessages.ScrollControlIntoView(
            messagePanel)

    End Sub

    '==========================================================
    ' MESSAGE HEIGHT
    '==========================================================

    Private Sub UpdateMessagePanelHeight(
        messagePanel As Panel)

        If messagePanel Is Nothing Then
            Return
        End If

        Dim senderLabel As Label =
            Nothing

        Dim messageLabel As Label =
            Nothing

        For Each control As Control In
            messagePanel.Controls

            If TypeOf control Is Label Then

                Dim labelControl As Label =
                    DirectCast(
                        control,
                        Label)

                If labelControl.Font.Bold Then

                    senderLabel =
                        labelControl

                Else

                    messageLabel =
                        labelControl

                End If

            End If

        Next

        If senderLabel Is Nothing OrElse
           messageLabel Is Nothing Then

            Return

        End If

        Dim availableWidth As Integer =
            messagePanel.Width -
            messagePanel.Padding.Left -
            messagePanel.Padding.Right -
            5

        If availableWidth < 100 Then

            availableWidth = 100

        End If

        Dim messageSize As Size =
            TextRenderer.MeasureText(
                messageLabel.Text,
                messageLabel.Font,
                New Size(
                    availableWidth,
                    Integer.MaxValue),
                TextFormatFlags.WordBreak Or
                TextFormatFlags.NoPadding)

        Dim senderHeight As Integer =
            24

        Dim messageHeight As Integer =
            messageSize.Height + 12

        If messageHeight < 25 Then

            messageHeight = 25

        End If

        messagePanel.Height =
            senderHeight +
            messageHeight +
            messagePanel.Padding.Top +
            messagePanel.Padding.Bottom

    End Sub

    '==========================================================
    ' ENABLE / DISABLE INPUT
    '==========================================================

    Private Sub SetInputEnabled(
        enabled As Boolean)

        If txtCommand IsNot Nothing Then

            txtCommand.Enabled =
                enabled

        End If

        If btnSend IsNot Nothing Then

            btnSend.Enabled =
                enabled

        End If

    End Sub

End Class


'==============================================================
' CHAT COMMAND EVENT ARGS
'==============================================================

Public Class ChatCommandEventArgs
    Inherits EventArgs

    Public ReadOnly Property Command As String

    Public Sub New(
        command As String)

        Me.Command =
            If(
                command,
                String.Empty)

    End Sub

End Class
