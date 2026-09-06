Imports System
Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UcChat
    Inherits UserControl

    Private components As System.ComponentModel.IContainer

    Friend WithEvents flpMessages As FlowLayoutPanel
    Friend WithEvents pnlChatInput As Panel
    Friend WithEvents txtCommand As TextBox
    Friend WithEvents btnSend As Button

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(
        ByVal disposing As Boolean)

        Try

            If disposing AndAlso
               components IsNot Nothing Then

                components.Dispose()

            End If

        Finally

            MyBase.Dispose(disposing)

        End Try

    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()

        Me.components =
            New System.ComponentModel.Container()

        Me.flpMessages =
            New FlowLayoutPanel()

        Me.pnlChatInput =
            New Panel()

        Me.txtCommand =
            New TextBox()

        Me.btnSend =
            New Button()

        Me.pnlChatInput.SuspendLayout()
        Me.SuspendLayout()

        '
        ' flpMessages
        '
        Me.flpMessages.AutoScroll =
            True

        Me.flpMessages.BackColor =
            Color.FromArgb(
                10,
                11,
                14)

        Me.flpMessages.Dock =
            DockStyle.Fill

        Me.flpMessages.FlowDirection =
            FlowDirection.TopDown

        Me.flpMessages.Location =
            New Point(
                0,
                0)

        Me.flpMessages.Margin =
            New Padding(0)

        Me.flpMessages.Name =
            "flpMessages"

        Me.flpMessages.Padding =
            New Padding(
                8,
                8,
                8,
                8)

        Me.flpMessages.Size =
            New Size(
                748,
                523)

        Me.flpMessages.TabIndex =
            0

        Me.flpMessages.WrapContents =
            False

        '
        ' pnlChatInput
        '
        Me.pnlChatInput.BackColor =
            Color.FromArgb(
                10,
                11,
                14)

        Me.pnlChatInput.Dock =
            DockStyle.Bottom

        Me.pnlChatInput.Location =
            New Point(
                0,
                523)

        Me.pnlChatInput.MinimumSize =
            New Size(
                0,
                72)

        Me.pnlChatInput.Name =
            "pnlChatInput"

        Me.pnlChatInput.Padding =
            New Padding(8)

        Me.pnlChatInput.Size =
            New Size(
                748,
                72)

        Me.pnlChatInput.TabIndex =
            1

        '
        ' txtCommand
        '
        Me.txtCommand.AcceptsReturn =
            True

        Me.txtCommand.AcceptsTab =
            True

        Me.txtCommand.BackColor =
            Color.FromArgb(
                25,
                27,
                32)

        Me.txtCommand.BorderStyle =
            BorderStyle.FixedSingle

        Me.txtCommand.Font =
            New Font(
                "Segoe UI",
                10.0!)

        Me.txtCommand.ForeColor =
            Color.Gainsboro

        Me.txtCommand.Location =
            New Point(
                8,
                8)

        Me.txtCommand.Multiline =
            True

        Me.txtCommand.Name =
            "txtCommand"

        Me.txtCommand.ScrollBars =
            ScrollBars.Vertical

        Me.txtCommand.Size =
            New Size(
                645,
                56)

        Me.txtCommand.TabIndex =
            0

        Me.txtCommand.WordWrap =
            True

        '
        ' btnSend
        '
        Me.btnSend.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Right

        Me.btnSend.BackColor =
            Color.FromArgb(
                45,
                48,
                58)

        Me.btnSend.FlatAppearance.BorderColor =
            Color.FromArgb(
                70,
                74,
                85)

        Me.btnSend.FlatStyle =
            FlatStyle.Flat

        Me.btnSend.Font =
            New Font(
                "Segoe UI",
                9.0!,
                FontStyle.Bold)

        Me.btnSend.ForeColor =
            Color.White

        Me.btnSend.Location =
            New Point(
                661,
                8)

        Me.btnSend.Name =
            "btnSend"

        Me.btnSend.Size =
            New Size(
                70,
                56)

        Me.btnSend.TabIndex =
            1

        Me.btnSend.Text =
            "Send"

        Me.btnSend.UseVisualStyleBackColor =
            False

        '
        ' pnlChatInput Controls
        '
        Me.pnlChatInput.Controls.Add(
            Me.btnSend)

        Me.pnlChatInput.Controls.Add(
            Me.txtCommand)

        '
        ' UcChat
        '
        Me.AutoScaleDimensions =
            New SizeF(
                7.0!,
                15.0!)

        Me.AutoScaleMode =
            AutoScaleMode.Font

        Me.BackColor =
            Color.FromArgb(
                10,
                11,
                14)

        Me.Controls.Add(
            Me.flpMessages)

        Me.Controls.Add(
            Me.pnlChatInput)

        Me.MinimumSize =
            New Size(
                250,
                150)

        Me.Name =
            "UcChat"

        Me.Size =
            New Size(
                748,
                595)

        Me.pnlChatInput.ResumeLayout(
            False)

        Me.pnlChatInput.PerformLayout()

        Me.ResumeLayout(
            False)

    End Sub

End Class
