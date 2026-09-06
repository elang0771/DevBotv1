Imports System
Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UcCodeViewer
    Inherits UserControl

    Private components As System.ComponentModel.IContainer

    Friend WithEvents pnlToolbar As Panel
    Friend WithEvents lblFileName As Label
    Friend WithEvents lblFilePath As Label
    Friend WithEvents btnSave As Button
    Friend WithEvents btnDiff As Button
    Friend WithEvents btnReload As Button
    Friend WithEvents txtCode As RichTextBox

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

        Me.pnlToolbar =
            New System.Windows.Forms.Panel()

        Me.lblFileName =
            New System.Windows.Forms.Label()

        Me.lblFilePath =
            New System.Windows.Forms.Label()

        Me.btnSave =
            New System.Windows.Forms.Button()

        Me.btnDiff =
            New System.Windows.Forms.Button()

        Me.btnReload =
            New System.Windows.Forms.Button()

        Me.txtCode =
            New System.Windows.Forms.RichTextBox()

        Me.pnlToolbar.SuspendLayout()
        Me.SuspendLayout()

        '
        ' pnlToolbar
        '
        Me.pnlToolbar.BackColor =
            System.Drawing.Color.FromArgb(
                CType(24, Byte),
                CType(27, Byte),
                CType(32, Byte))

        Me.pnlToolbar.Controls.Add(
            Me.btnReload)

        Me.pnlToolbar.Controls.Add(
            Me.btnDiff)

        Me.pnlToolbar.Controls.Add(
            Me.btnSave)

        Me.pnlToolbar.Controls.Add(
            Me.lblFilePath)

        Me.pnlToolbar.Controls.Add(
            Me.lblFileName)

        Me.pnlToolbar.Dock =
            System.Windows.Forms.DockStyle.Top

        Me.pnlToolbar.Height =
            58

        Me.pnlToolbar.Location =
            New System.Drawing.Point(
                0,
                0)

        Me.pnlToolbar.Name =
            "pnlToolbar"

        Me.pnlToolbar.Padding =
            New System.Windows.Forms.Padding(8)

        Me.pnlToolbar.Size =
            New System.Drawing.Size(
                900,
                58)

        Me.pnlToolbar.TabIndex =
            0

        '
        ' lblFileName
        '
        Me.lblFileName.AutoSize =
            False

        Me.lblFileName.Font =
            New System.Drawing.Font(
                "Segoe UI",
                10.0!,
                System.Drawing.FontStyle.Bold,
                System.Drawing.GraphicsUnit.Point,
                CType(0, Byte))

        Me.lblFileName.ForeColor =
            System.Drawing.Color.White

        Me.lblFileName.Location =
            New System.Drawing.Point(
                10,
                6)

        Me.lblFileName.Name =
            "lblFileName"

        Me.lblFileName.Size =
            New System.Drawing.Size(
                400,
                23)

        Me.lblFileName.TabIndex =
            0

        Me.lblFileName.Text =
            "No file selected"

        Me.lblFileName.TextAlign =
            System.Drawing.ContentAlignment.MiddleLeft

        '
        ' lblFilePath
        '
        Me.lblFilePath.AutoSize =
            False

        Me.lblFilePath.Font =
            New System.Drawing.Font(
                "Segoe UI",
                8.0!,
                System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point,
                CType(0, Byte))

        Me.lblFilePath.ForeColor =
            System.Drawing.Color.Silver

        Me.lblFilePath.Location =
            New System.Drawing.Point(
                10,
                31)

        Me.lblFilePath.Name =
            "lblFilePath"

        Me.lblFilePath.Size =
            New System.Drawing.Size(
                500,
                20)

        Me.lblFilePath.TabIndex =
            1

        Me.lblFilePath.Text =
            ""

        Me.lblFilePath.TextAlign =
            System.Drawing.ContentAlignment.MiddleLeft

        '
        ' btnSave
        '
        Me.btnSave.Anchor =
            CType(
                System.Windows.Forms.AnchorStyles.Top Or
                System.Windows.Forms.AnchorStyles.Right,
                System.Windows.Forms.AnchorStyles)

        Me.btnSave.Location =
            New System.Drawing.Point(
                640,
                12)

        Me.btnSave.Name =
            "btnSave"

        Me.btnSave.Size =
            New System.Drawing.Size(
                75,
                32)

        Me.btnSave.TabIndex =
            2

        Me.btnSave.Text =
            "Save"

        Me.btnSave.UseVisualStyleBackColor =
            True

        '
        ' btnDiff
        '
        Me.btnDiff.Anchor =
            CType(
                System.Windows.Forms.AnchorStyles.Top Or
                System.Windows.Forms.AnchorStyles.Right,
                System.Windows.Forms.AnchorStyles)

        Me.btnDiff.Location =
            New System.Drawing.Point(
                720,
                12)

        Me.btnDiff.Name =
            "btnDiff"

        Me.btnDiff.Size =
            New System.Drawing.Size(
                75,
                32)

        Me.btnDiff.TabIndex =
            3

        Me.btnDiff.Text =
            "Diff"

        Me.btnDiff.UseVisualStyleBackColor =
            True

        '
        ' btnReload
        '
        Me.btnReload.Anchor =
            CType(
                System.Windows.Forms.AnchorStyles.Top Or
                System.Windows.Forms.AnchorStyles.Right,
                System.Windows.Forms.AnchorStyles)

        Me.btnReload.Location =
            New System.Drawing.Point(
                800,
                12)

        Me.btnReload.Name =
            "btnReload"

        Me.btnReload.Size =
            New System.Drawing.Size(
                75,
                32)

        Me.btnReload.TabIndex =
            4

        Me.btnReload.Text =
            "Reload"

        Me.btnReload.UseVisualStyleBackColor =
            True

        '
        ' txtCode
        '
        Me.txtCode.BackColor =
            System.Drawing.Color.FromArgb(
                CType(10, Byte),
                CType(12, Byte),
                CType(15, Byte))

        Me.txtCode.BorderStyle =
            System.Windows.Forms.BorderStyle.None

        Me.txtCode.Dock =
            System.Windows.Forms.DockStyle.Fill

        Me.txtCode.Font =
            New System.Drawing.Font(
                "Consolas",
                10.0!,
                System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point,
                CType(0, Byte))

        Me.txtCode.ForeColor =
            System.Drawing.Color.Gainsboro

        Me.txtCode.Location =
            New System.Drawing.Point(
                0,
                58)

        Me.txtCode.Name =
            "txtCode"

        Me.txtCode.ScrollBars =
            System.Windows.Forms.RichTextBoxScrollBars.Both

        Me.txtCode.Size =
            New System.Drawing.Size(
                900,
                542)

        Me.txtCode.TabIndex =
            5

        Me.txtCode.Text =
            ""

        Me.txtCode.WordWrap =
            False

        Me.txtCode.DetectUrls =
            False

        Me.txtCode.HideSelection =
            False

        Me.txtCode.AcceptsTab =
            True

        '
        ' UcCodeViewer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)

        Me.AutoScaleMode =
            System.Windows.Forms.AutoScaleMode.Font

        Me.BackColor =
            System.Drawing.Color.FromArgb(
                CType(10, Byte),
                CType(12, Byte),
                CType(15, Byte))

        Me.Controls.Add(
            Me.txtCode)

        Me.Controls.Add(
            Me.pnlToolbar)

        Me.Name =
            "UcCodeViewer"

        Me.Size =
            New System.Drawing.Size(
                900,
                600)

        Me.pnlToolbar.ResumeLayout(
            False)

        Me.ResumeLayout(
            False)

    End Sub

End Class