Imports System
Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmMain
    Inherits Form

    Private components As System.ComponentModel.IContainer

    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents btnSettings As Button

    Friend WithEvents splitVertical As SplitContainer
    Friend WithEvents pnlMain As Panel

    Friend WithEvents splitMain As SplitContainer
    Friend WithEvents splitCenter As SplitContainer

    Friend WithEvents pnlProject As Panel
    Friend WithEvents lblProjectTitle As Label
    Friend WithEvents btnOpenProject As Button
    Friend WithEvents btnScanProject As Button
    Friend WithEvents tvProject As TreeView

    Friend WithEvents pnlChat As Panel
    Friend WithEvents ucChat As UcChat

    Friend WithEvents pnlCode As Panel
    Friend WithEvents lblCodeTitle As Label
    Friend WithEvents ucCodeViewer As UcCodeViewer

    Friend WithEvents pnlBottom As Panel
    Friend WithEvents tabBottom As TabControl

    Friend WithEvents tabOutput As TabPage
    Friend WithEvents tabBuild As TabPage
    Friend WithEvents tabError As TabPage
    Friend WithEvents tabTerminal As TabPage
    Friend WithEvents tabHistory As TabPage

    Friend WithEvents txtOutput As RichTextBox
    Friend WithEvents txtBuild As RichTextBox
    Friend WithEvents txtError As RichTextBox
    Friend WithEvents txtTerminal As RichTextBox
    Friend WithEvents txtHistory As RichTextBox

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
        Me.pnlHeader = New System.Windows.Forms.Panel()
        Me.btnSettings = New System.Windows.Forms.Button()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.splitVertical = New System.Windows.Forms.SplitContainer()
        Me.pnlMain = New System.Windows.Forms.Panel()
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.pnlProject = New System.Windows.Forms.Panel()
        Me.tvProject = New System.Windows.Forms.TreeView()
        Me.btnScanProject = New System.Windows.Forms.Button()
        Me.btnOpenProject = New System.Windows.Forms.Button()
        Me.lblProjectTitle = New System.Windows.Forms.Label()
        Me.splitCenter = New System.Windows.Forms.SplitContainer()
        Me.pnlChat = New System.Windows.Forms.Panel()
        Me.ucChat = New UcChat()
        Me.pnlCode = New System.Windows.Forms.Panel()
        Me.ucCodeViewer = New UcCodeViewer()
        Me.lblCodeTitle = New System.Windows.Forms.Label()
        Me.pnlBottom = New System.Windows.Forms.Panel()
        Me.tabBottom = New System.Windows.Forms.TabControl()
        Me.tabOutput = New System.Windows.Forms.TabPage()
        Me.txtOutput = New System.Windows.Forms.RichTextBox()
        Me.tabBuild = New System.Windows.Forms.TabPage()
        Me.txtBuild = New System.Windows.Forms.RichTextBox()
        Me.tabError = New System.Windows.Forms.TabPage()
        Me.txtError = New System.Windows.Forms.RichTextBox()
        Me.tabTerminal = New System.Windows.Forms.TabPage()
        Me.txtTerminal = New System.Windows.Forms.RichTextBox()
        Me.tabHistory = New System.Windows.Forms.TabPage()
        Me.txtHistory = New System.Windows.Forms.RichTextBox()
        Me.pnlHeader.SuspendLayout()
        CType(Me.splitVertical, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitVertical.Panel1.SuspendLayout()
        Me.splitVertical.Panel2.SuspendLayout()
        Me.splitVertical.SuspendLayout()
        Me.pnlMain.SuspendLayout()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.pnlProject.SuspendLayout()
        CType(Me.splitCenter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitCenter.Panel1.SuspendLayout()
        Me.splitCenter.Panel2.SuspendLayout()
        Me.splitCenter.SuspendLayout()
        Me.pnlChat.SuspendLayout()
        Me.pnlCode.SuspendLayout()
        Me.pnlBottom.SuspendLayout()
        Me.tabBottom.SuspendLayout()
        Me.tabOutput.SuspendLayout()
        Me.tabBuild.SuspendLayout()
        Me.tabError.SuspendLayout()
        Me.tabTerminal.SuspendLayout()
        Me.tabHistory.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlHeader
        '
        Me.pnlHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(25, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(32, Byte), Integer))
        Me.pnlHeader.Controls.Add(Me.btnSettings)
        Me.pnlHeader.Controls.Add(Me.lblTitle)
        Me.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Size = New System.Drawing.Size(1400, 58)
        Me.pnlHeader.TabIndex = 0
        '
        'btnSettings
        '
        Me.btnSettings.Dock = System.Windows.Forms.DockStyle.Right
        Me.btnSettings.FlatAppearance.BorderSize = 0
        Me.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSettings.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.btnSettings.ForeColor = System.Drawing.Color.White
        Me.btnSettings.Location = New System.Drawing.Point(1295, 0)
        Me.btnSettings.Name = "btnSettings"
        Me.btnSettings.Size = New System.Drawing.Size(105, 58)
        Me.btnSettings.TabIndex = 0
        Me.btnSettings.Text = "⚙ Settings"
        Me.btnSettings.UseVisualStyleBackColor = False
        '
        'lblTitle
        '
        Me.lblTitle.Dock = System.Windows.Forms.DockStyle.Left
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 16.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Padding = New System.Windows.Forms.Padding(16, 0, 0, 0)
        Me.lblTitle.Size = New System.Drawing.Size(320, 58)
        Me.lblTitle.TabIndex = 1
        Me.lblTitle.Text = "🤖 DevBot"
        Me.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'splitVertical
        '
        Me.splitVertical.BackColor = System.Drawing.Color.FromArgb(CType(CType(12, Byte), Integer), CType(CType(14, Byte), Integer), CType(CType(18, Byte), Integer))
        Me.splitVertical.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitVertical.Location = New System.Drawing.Point(0, 58)
        Me.splitVertical.Name = "splitVertical"
        Me.splitVertical.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'splitVertical.Panel1
        '
        Me.splitVertical.Panel1.Controls.Add(Me.pnlMain)
        Me.splitVertical.Panel1MinSize = 0
        '
        'splitVertical.Panel2
        '
        Me.splitVertical.Panel2.Controls.Add(Me.pnlBottom)
        Me.splitVertical.Panel2MinSize = 0
        Me.splitVertical.Size = New System.Drawing.Size(1400, 792)
        Me.splitVertical.SplitterDistance = 198
        Me.splitVertical.SplitterWidth = 5
        Me.splitVertical.TabIndex = 0
        '
        'pnlMain
        '
        Me.pnlMain.BackColor = System.Drawing.Color.FromArgb(CType(CType(15, Byte), Integer), CType(CType(17, Byte), Integer), CType(CType(21, Byte), Integer))
        Me.pnlMain.Controls.Add(Me.splitMain)
        Me.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMain.Location = New System.Drawing.Point(0, 0)
        Me.pnlMain.Name = "pnlMain"
        Me.pnlMain.Size = New System.Drawing.Size(1400, 198)
        Me.pnlMain.TabIndex = 0
        '
        'splitMain
        '
        Me.splitMain.BackColor = System.Drawing.Color.FromArgb(CType(CType(12, Byte), Integer), CType(CType(14, Byte), Integer), CType(CType(18, Byte), Integer))
        Me.splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitMain.Location = New System.Drawing.Point(0, 0)
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.pnlProject)
        Me.splitMain.Panel1MinSize = 0
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.splitCenter)
        Me.splitMain.Panel2MinSize = 0
        Me.splitMain.Size = New System.Drawing.Size(1400, 198)
        Me.splitMain.SplitterDistance = 233
        Me.splitMain.SplitterWidth = 7
        Me.splitMain.TabIndex = 0
        '
        'pnlProject
        '
        Me.pnlProject.BackColor = System.Drawing.Color.FromArgb(CType(CType(20, Byte), Integer), CType(CType(22, Byte), Integer), CType(CType(27, Byte), Integer))
        Me.pnlProject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlProject.Controls.Add(Me.tvProject)
        Me.pnlProject.Controls.Add(Me.btnScanProject)
        Me.pnlProject.Controls.Add(Me.btnOpenProject)
        Me.pnlProject.Controls.Add(Me.lblProjectTitle)
        Me.pnlProject.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlProject.Location = New System.Drawing.Point(0, 0)
        Me.pnlProject.Name = "pnlProject"
        Me.pnlProject.Size = New System.Drawing.Size(233, 198)
        Me.pnlProject.TabIndex = 0
        '
        'tvProject
        '
        Me.tvProject.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tvProject.BackColor = System.Drawing.Color.FromArgb(CType(CType(20, Byte), Integer), CType(CType(22, Byte), Integer), CType(CType(27, Byte), Integer))
        Me.tvProject.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.tvProject.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.tvProject.ForeColor = System.Drawing.Color.Gainsboro
        Me.tvProject.HideSelection = False
        Me.tvProject.Location = New System.Drawing.Point(8, 126)
        Me.tvProject.Name = "tvProject"
        Me.tvProject.ShowNodeToolTips = True
        Me.tvProject.Size = New System.Drawing.Size(251, 396)
        Me.tvProject.TabIndex = 0
        '
        'btnScanProject
        '
        Me.btnScanProject.Enabled = False
        Me.btnScanProject.Location = New System.Drawing.Point(0, 0)
        Me.btnScanProject.Name = "btnScanProject"
        Me.btnScanProject.Size = New System.Drawing.Size(75, 23)
        Me.btnScanProject.TabIndex = 1
        Me.btnScanProject.Visible = False
        '
        'btnOpenProject
        '
        Me.btnOpenProject.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOpenProject.BackColor = System.Drawing.Color.FromArgb(CType(CType(34, Byte), Integer), CType(CType(31, Byte), Integer), CType(CType(58, Byte), Integer))
        Me.btnOpenProject.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnOpenProject.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(78, Byte), Integer), CType(CType(82, Byte), Integer), CType(CType(105, Byte), Integer))
        Me.btnOpenProject.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(43, Byte), Integer), CType(CType(78, Byte), Integer))
        Me.btnOpenProject.Font = New System.Drawing.Font("Segoe UI", 9.5!, System.Drawing.FontStyle.Bold)
        Me.btnOpenProject.ForeColor = System.Drawing.Color.White
        Me.btnOpenProject.Location = New System.Drawing.Point(10, 48)
        Me.btnOpenProject.Name = "btnOpenProject"
        Me.btnOpenProject.Size = New System.Drawing.Size(281, 68)
        Me.btnOpenProject.TabIndex = 2
        Me.btnOpenProject.Text = "📁  Open Project" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "      Pilih folder project DevBot"
        Me.btnOpenProject.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnOpenProject.UseVisualStyleBackColor = False
        '
        'lblProjectTitle
        '
        Me.lblProjectTitle.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblProjectTitle.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblProjectTitle.ForeColor = System.Drawing.Color.White
        Me.lblProjectTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblProjectTitle.Name = "lblProjectTitle"
        Me.lblProjectTitle.Padding = New System.Windows.Forms.Padding(12, 0, 0, 0)
        Me.lblProjectTitle.Size = New System.Drawing.Size(231, 42)
        Me.lblProjectTitle.TabIndex = 3
        Me.lblProjectTitle.Text = "PROJECT EXPLORER"
        Me.lblProjectTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'splitCenter
        '
        Me.splitCenter.BackColor = System.Drawing.Color.FromArgb(CType(CType(12, Byte), Integer), CType(CType(14, Byte), Integer), CType(CType(18, Byte), Integer))
        Me.splitCenter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitCenter.Location = New System.Drawing.Point(0, 0)
        Me.splitCenter.Name = "splitCenter"
        '
        'splitCenter.Panel1
        '
        Me.splitCenter.Panel1.Controls.Add(Me.pnlChat)
        Me.splitCenter.Panel1MinSize = 0
        '
        'splitCenter.Panel2
        '
        Me.splitCenter.Panel2.Controls.Add(Me.pnlCode)
        Me.splitCenter.Panel2MinSize = 0
        Me.splitCenter.Size = New System.Drawing.Size(1160, 198)
        Me.splitCenter.SplitterDistance = 193
        Me.splitCenter.SplitterWidth = 7
        Me.splitCenter.TabIndex = 0
        '
        'pnlChat
        '
        Me.pnlChat.BackColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(12, Byte), Integer), CType(CType(15, Byte), Integer))
        Me.pnlChat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlChat.Controls.Add(Me.ucChat)
        Me.pnlChat.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlChat.Location = New System.Drawing.Point(0, 0)
        Me.pnlChat.Name = "pnlChat"
        Me.pnlChat.Size = New System.Drawing.Size(193, 198)
        Me.pnlChat.TabIndex = 0
        '
        'ucChat
        '
        Me.ucChat.BackColor = System.Drawing.Color.FromArgb(CType(CType(18, Byte), Integer), CType(CType(20, Byte), Integer), CType(CType(24, Byte), Integer))
        Me.ucChat.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ucChat.Location = New System.Drawing.Point(0, 0)
        Me.ucChat.MinimumSize = New System.Drawing.Size(214, 130)
        Me.ucChat.Name = "ucChat"
        Me.ucChat.Size = New System.Drawing.Size(214, 196)
        Me.ucChat.TabIndex = 0
        '
        'pnlCode
        '
        Me.pnlCode.BackColor = System.Drawing.Color.FromArgb(CType(CType(10, Byte), Integer), CType(CType(12, Byte), Integer), CType(CType(15, Byte), Integer))
        Me.pnlCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlCode.Controls.Add(Me.ucCodeViewer)
        Me.pnlCode.Controls.Add(Me.lblCodeTitle)
        Me.pnlCode.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCode.Location = New System.Drawing.Point(0, 0)
        Me.pnlCode.Name = "pnlCode"
        Me.pnlCode.Size = New System.Drawing.Size(960, 198)
        Me.pnlCode.TabIndex = 1
        '
        'ucCodeViewer
        '
        Me.ucCodeViewer.BackColor = System.Drawing.Color.FromArgb(CType(CType(15, Byte), Integer), CType(CType(17, Byte), Integer), CType(CType(21, Byte), Integer))
        Me.ucCodeViewer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ucCodeViewer.Location = New System.Drawing.Point(0, 40)
        Me.ucCodeViewer.Name = "ucCodeViewer"
        Me.ucCodeViewer.Size = New System.Drawing.Size(958, 156)
        Me.ucCodeViewer.TabIndex = 0
        '
        'lblCodeTitle
        '
        Me.lblCodeTitle.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblCodeTitle.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblCodeTitle.ForeColor = System.Drawing.Color.White
        Me.lblCodeTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblCodeTitle.Name = "lblCodeTitle"
        Me.lblCodeTitle.Padding = New System.Windows.Forms.Padding(12, 0, 0, 0)
        Me.lblCodeTitle.Size = New System.Drawing.Size(958, 40)
        Me.lblCodeTitle.TabIndex = 1
        Me.lblCodeTitle.Text = "CODE / DIFF"
        Me.lblCodeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'pnlBottom
        '
        Me.pnlBottom.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.pnlBottom.Controls.Add(Me.tabBottom)
        Me.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlBottom.Location = New System.Drawing.Point(0, 0)
        Me.pnlBottom.Name = "pnlBottom"
        Me.pnlBottom.Size = New System.Drawing.Size(1400, 589)
        Me.pnlBottom.TabIndex = 1
        '
        'tabBottom
        '
        Me.tabBottom.Controls.Add(Me.tabOutput)
        Me.tabBottom.Controls.Add(Me.tabBuild)
        Me.tabBottom.Controls.Add(Me.tabError)
        Me.tabBottom.Controls.Add(Me.tabTerminal)
        Me.tabBottom.Controls.Add(Me.tabHistory)
        Me.tabBottom.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tabBottom.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed
        Me.tabBottom.ForeColor = System.Drawing.Color.Gainsboro
        Me.tabBottom.ItemSize = New System.Drawing.Size(92, 28)
        Me.tabBottom.Location = New System.Drawing.Point(0, 0)
        Me.tabBottom.Margin = New System.Windows.Forms.Padding(0)
        Me.tabBottom.Name = "tabBottom"
        Me.tabBottom.Padding = New System.Drawing.Point(0, 0)
        Me.tabBottom.SelectedIndex = 0
        Me.tabBottom.Size = New System.Drawing.Size(1400, 589)
        Me.tabBottom.SizeMode = System.Windows.Forms.TabSizeMode.Fixed
        Me.tabBottom.TabIndex = 0
        '
        'tabOutput
        '
        Me.tabOutput.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.tabOutput.Controls.Add(Me.txtOutput)
        Me.tabOutput.Location = New System.Drawing.Point(4, 32)
        Me.tabOutput.Name = "tabOutput"
        Me.tabOutput.Padding = New System.Windows.Forms.Padding(3)
        Me.tabOutput.Size = New System.Drawing.Size(1392, 553)
        Me.tabOutput.TabIndex = 0
        Me.tabOutput.Text = "OUTPUT"
        '
        'txtOutput
        '
        Me.txtOutput.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.txtOutput.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtOutput.Font = New System.Drawing.Font("Consolas", 9.0!)
        Me.txtOutput.ForeColor = System.Drawing.Color.Gainsboro
        Me.txtOutput.Location = New System.Drawing.Point(3, 3)
        Me.txtOutput.Name = "txtOutput"
        Me.txtOutput.ReadOnly = True
        Me.txtOutput.Size = New System.Drawing.Size(1386, 547)
        Me.txtOutput.TabIndex = 0
        Me.txtOutput.Text = ""
        Me.txtOutput.WordWrap = False
        '
        'tabBuild
        '
        Me.tabBuild.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.tabBuild.Controls.Add(Me.txtBuild)
        Me.tabBuild.Location = New System.Drawing.Point(4, 32)
        Me.tabBuild.Name = "tabBuild"
        Me.tabBuild.Padding = New System.Windows.Forms.Padding(3)
        Me.tabBuild.Size = New System.Drawing.Size(142, 34)
        Me.tabBuild.TabIndex = 1
        Me.tabBuild.Text = "BUILD"
        '
        'txtBuild
        '
        Me.txtBuild.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.txtBuild.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtBuild.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtBuild.Font = New System.Drawing.Font("Consolas", 9.0!)
        Me.txtBuild.ForeColor = System.Drawing.Color.Gainsboro
        Me.txtBuild.Location = New System.Drawing.Point(3, 3)
        Me.txtBuild.Name = "txtBuild"
        Me.txtBuild.ReadOnly = True
        Me.txtBuild.Size = New System.Drawing.Size(136, 28)
        Me.txtBuild.TabIndex = 0
        Me.txtBuild.Text = ""
        Me.txtBuild.WordWrap = False
        '
        'tabError
        '
        Me.tabError.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.tabError.Controls.Add(Me.txtError)
        Me.tabError.Location = New System.Drawing.Point(4, 32)
        Me.tabError.Name = "tabError"
        Me.tabError.Padding = New System.Windows.Forms.Padding(3)
        Me.tabError.Size = New System.Drawing.Size(142, 34)
        Me.tabError.TabIndex = 2
        Me.tabError.Text = "ERROR"
        '
        'txtError
        '
        Me.txtError.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.txtError.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtError.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtError.Font = New System.Drawing.Font("Consolas", 9.0!)
        Me.txtError.ForeColor = System.Drawing.Color.Gainsboro
        Me.txtError.Location = New System.Drawing.Point(3, 3)
        Me.txtError.Name = "txtError"
        Me.txtError.ReadOnly = True
        Me.txtError.Size = New System.Drawing.Size(136, 28)
        Me.txtError.TabIndex = 0
        Me.txtError.Text = ""
        Me.txtError.WordWrap = False
        '
        'tabTerminal
        '
        Me.tabTerminal.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.tabTerminal.Controls.Add(Me.txtTerminal)
        Me.tabTerminal.Location = New System.Drawing.Point(4, 32)
        Me.tabTerminal.Name = "tabTerminal"
        Me.tabTerminal.Padding = New System.Windows.Forms.Padding(3)
        Me.tabTerminal.Size = New System.Drawing.Size(142, 34)
        Me.tabTerminal.TabIndex = 3
        Me.tabTerminal.Text = "TERMINAL"
        '
        'txtTerminal
        '
        Me.txtTerminal.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.txtTerminal.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtTerminal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtTerminal.Font = New System.Drawing.Font("Consolas", 9.0!)
        Me.txtTerminal.ForeColor = System.Drawing.Color.Gainsboro
        Me.txtTerminal.Location = New System.Drawing.Point(3, 3)
        Me.txtTerminal.Name = "txtTerminal"
        Me.txtTerminal.ReadOnly = True
        Me.txtTerminal.Size = New System.Drawing.Size(136, 28)
        Me.txtTerminal.TabIndex = 0
        Me.txtTerminal.Text = ""
        Me.txtTerminal.WordWrap = False
        '
        'tabHistory
        '
        Me.tabHistory.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.tabHistory.Controls.Add(Me.txtHistory)
        Me.tabHistory.Location = New System.Drawing.Point(4, 32)
        Me.tabHistory.Name = "tabHistory"
        Me.tabHistory.Padding = New System.Windows.Forms.Padding(3)
        Me.tabHistory.Size = New System.Drawing.Size(142, 34)
        Me.tabHistory.TabIndex = 4
        Me.tabHistory.Text = "HISTORY"
        '
        'txtHistory
        '
        Me.txtHistory.BackColor = System.Drawing.Color.FromArgb(CType(CType(14, Byte), Integer), CType(CType(16, Byte), Integer), CType(CType(20, Byte), Integer))
        Me.txtHistory.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtHistory.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtHistory.Font = New System.Drawing.Font("Consolas", 9.0!)
        Me.txtHistory.ForeColor = System.Drawing.Color.Gainsboro
        Me.txtHistory.Location = New System.Drawing.Point(3, 3)
        Me.txtHistory.Name = "txtHistory"
        Me.txtHistory.ReadOnly = True
        Me.txtHistory.Size = New System.Drawing.Size(136, 28)
        Me.txtHistory.TabIndex = 0
        Me.txtHistory.Text = ""
        Me.txtHistory.WordWrap = False
        '
        'FrmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(18, Byte), Integer), CType(CType(20, Byte), Integer), CType(CType(24, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1400, 850)
        Me.Controls.Add(Me.splitVertical)
        Me.Controls.Add(Me.pnlHeader)
        Me.MinimumSize = New System.Drawing.Size(1050, 650)
        Me.Name = "FrmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "DevBot - AI Development Assistant"
        Me.pnlHeader.ResumeLayout(False)
        Me.splitVertical.Panel1.ResumeLayout(False)
        Me.splitVertical.Panel2.ResumeLayout(False)
        CType(Me.splitVertical, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitVertical.ResumeLayout(False)
        Me.pnlMain.ResumeLayout(False)
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.pnlProject.ResumeLayout(False)
        Me.splitCenter.Panel1.ResumeLayout(False)
        Me.splitCenter.Panel2.ResumeLayout(False)
        CType(Me.splitCenter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitCenter.ResumeLayout(False)
        Me.pnlChat.ResumeLayout(False)
        Me.pnlCode.ResumeLayout(False)
        Me.pnlBottom.ResumeLayout(False)
        Me.tabBottom.ResumeLayout(False)
        Me.tabOutput.ResumeLayout(False)
        Me.tabBuild.ResumeLayout(False)
        Me.tabError.ResumeLayout(False)
        Me.tabTerminal.ResumeLayout(False)
        Me.tabHistory.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

End Class
