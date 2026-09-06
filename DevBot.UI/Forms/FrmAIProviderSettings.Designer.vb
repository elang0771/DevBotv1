Imports System
Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmAIProviderSettings
    Inherits Form

    Private components As System.ComponentModel.IContainer

    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblSubtitle As Label

    Friend WithEvents splitMain As SplitContainer
    Friend WithEvents pnlProviders As Panel
    Friend WithEvents pnlEditor As Panel

    Friend WithEvents lblProviders As Label
    Friend WithEvents lstProviders As ListBox
    Friend WithEvents btnNew As Button

    Friend WithEvents lblProvider As Label
    Friend WithEvents cboProvider As ComboBox

    Friend WithEvents lblModel As Label
    Friend WithEvents txtModel As TextBox

    Friend WithEvents lblApiKey As Label
    Friend WithEvents txtApiKey As TextBox
    Friend WithEvents chkShowKey As CheckBox

    Friend WithEvents lblBaseUrl As Label
    Friend WithEvents txtBaseUrl As TextBox

    Friend WithEvents lblApiFormat As Label
    Friend WithEvents cboApiFormat As ComboBox

    Friend WithEvents lblThinking As Label
    Friend WithEvents cboThinking As ComboBox

    Friend WithEvents lblTemperature As Label
    Friend WithEvents numTemperature As NumericUpDown

    Friend WithEvents lblMaxOutput As Label
    Friend WithEvents numMaxOutput As NumericUpDown

    Friend WithEvents lblTimeout As Label
    Friend WithEvents numTimeout As NumericUpDown

    Friend WithEvents lblPriority As Label
    Friend WithEvents numPriority As NumericUpDown

    Friend WithEvents chkEnabled As CheckBox

    Friend WithEvents lblInfo As Label

    Friend WithEvents pnlButtons As Panel
    Friend WithEvents flpButtons As FlowLayoutPanel
    Friend WithEvents btnTest As Button
    Friend WithEvents btnDefault As Button
    Friend WithEvents btnDelete As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents btnClose As Button

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

        Me.pnlHeader = New Panel()
        Me.lblTitle = New Label()
        Me.lblSubtitle = New Label()

        Me.splitMain = New SplitContainer()
        Me.pnlProviders = New Panel()
        Me.pnlEditor = New Panel()

        Me.lblProviders = New Label()
        Me.lstProviders = New ListBox()
        Me.btnNew = New Button()

        Me.lblProvider = New Label()
        Me.cboProvider = New ComboBox()

        Me.lblModel = New Label()
        Me.txtModel = New TextBox()

        Me.lblApiKey = New Label()
        Me.txtApiKey = New TextBox()
        Me.chkShowKey = New CheckBox()

        Me.lblBaseUrl = New Label()
        Me.txtBaseUrl = New TextBox()

        Me.lblApiFormat = New Label()
        Me.cboApiFormat = New ComboBox()

        Me.lblThinking = New Label()
        Me.cboThinking = New ComboBox()

        Me.lblTemperature = New Label()
        Me.numTemperature = New NumericUpDown()

        Me.lblMaxOutput = New Label()
        Me.numMaxOutput = New NumericUpDown()

        Me.lblTimeout = New Label()
        Me.numTimeout = New NumericUpDown()

        Me.lblPriority = New Label()
        Me.numPriority = New NumericUpDown()

        Me.chkEnabled = New CheckBox()
        Me.lblInfo = New Label()

        Me.pnlButtons = New Panel()
        Me.flpButtons = New FlowLayoutPanel()
        Me.btnTest = New Button()
        Me.btnDefault = New Button()
        Me.btnDelete = New Button()
        Me.btnSave = New Button()
        Me.btnClose = New Button()

        Me.pnlHeader.SuspendLayout()

        CType(Me.splitMain,
              System.ComponentModel.ISupportInitialize).BeginInit()

        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()

        CType(Me.numTemperature,
              System.ComponentModel.ISupportInitialize).BeginInit()

        CType(Me.numMaxOutput,
              System.ComponentModel.ISupportInitialize).BeginInit()

        CType(Me.numTimeout,
              System.ComponentModel.ISupportInitialize).BeginInit()

        CType(Me.numPriority,
              System.ComponentModel.ISupportInitialize).BeginInit()

        Me.pnlProviders.SuspendLayout()
        Me.pnlEditor.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()

        '========================================================
        ' FORM
        '========================================================

        Me.AutoScaleDimensions =
            New SizeF(6.0!, 13.0!)

        Me.AutoScaleMode =
            AutoScaleMode.Font

        Me.BackColor =
            Color.FromArgb(18, 20, 24)

        Me.ClientSize =
            New Size(1120, 720)

        Me.MinimumSize =
            New Size(980, 620)

        Me.Name =
            "FrmAIProviderSettings"

        Me.StartPosition =
            FormStartPosition.CenterParent

        Me.Text =
            "DevBot - AI Provider Settings"

        '========================================================
        ' HEADER
        '========================================================

        Me.pnlHeader.BackColor =
            Color.FromArgb(25, 28, 34)

        Me.pnlHeader.Dock =
            DockStyle.Top

        Me.pnlHeader.Height =
            72

        Me.pnlHeader.Name =
            "pnlHeader"

        Me.lblTitle.AutoSize = False
        Me.lblTitle.Font =
            New Font(
                "Segoe UI",
                15.0!,
                FontStyle.Bold)

        Me.lblTitle.ForeColor =
            Color.White

        Me.lblTitle.Location =
            New Point(18, 10)

        Me.lblTitle.Name =
            "lblTitle"

        Me.lblTitle.Size =
            New Size(500, 30)

        Me.lblTitle.Text =
            "AI Provider Settings"

        Me.lblSubtitle.AutoSize = False
        Me.lblSubtitle.Font =
            New Font(
                "Segoe UI",
                8.5!)

        Me.lblSubtitle.ForeColor =
            Color.FromArgb(165, 170, 180)

        Me.lblSubtitle.Location =
            New Point(20, 40)

        Me.lblSubtitle.Name =
            "lblSubtitle"

        Me.lblSubtitle.Size =
            New Size(800, 22)

        Me.lblSubtitle.Text =
            "Kelola provider AI, model, API key, endpoint, dan prioritas."

        Me.pnlHeader.Controls.Add(Me.lblSubtitle)
        Me.pnlHeader.Controls.Add(Me.lblTitle)

        '========================================================
        ' SPLIT
        '========================================================

        Me.splitMain.Dock =
            DockStyle.Fill

        Me.splitMain.FixedPanel =
            FixedPanel.Panel1

        Me.splitMain.IsSplitterFixed =
            False

        Me.splitMain.Name =
            "splitMain"

        Me.splitMain.Orientation =
            Orientation.Vertical

        ' SplitterDistance sengaja tidak di-set di Designer.
        ' Nilainya diatur setelah ukuran form benar-benar tersedia
        ' oleh FrmAIProviderSettings.SetSafeSplitterDistance().
        Me.splitMain.SplitterWidth =
            5

        ' IMPORTANT: Do not set SplitterDistance/Panel min sizes here.
        ' WinForms validates these values while the SplitContainer width
        ' is still being initialized and can throw at startup.
        ' They are applied safely after the form is shown.

        '========================================================
        ' PROVIDER PANEL
        '========================================================

        Me.pnlProviders.BackColor =
            Color.FromArgb(12, 14, 18)

        Me.pnlProviders.Dock =
            DockStyle.Fill

        Me.pnlProviders.Name =
            "pnlProviders"

        Me.lblProviders.AutoSize = False
        Me.lblProviders.Font =
            New Font(
                "Segoe UI",
                10.0!,
                FontStyle.Bold)

        Me.lblProviders.ForeColor =
            Color.Gainsboro

        Me.lblProviders.Location =
            New Point(16, 16)

        Me.lblProviders.Name =
            "lblProviders"

        Me.lblProviders.Size =
            New Size(320, 28)

        Me.lblProviders.Text =
            "AI PROVIDERS"

        Me.lstProviders.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Bottom Or
            AnchorStyles.Left Or
            AnchorStyles.Right

        Me.lstProviders.BackColor =
            Color.FromArgb(20, 23, 28)

        Me.lstProviders.BorderStyle =
            BorderStyle.FixedSingle

        Me.lstProviders.Font =
            New Font(
                "Segoe UI",
                9.0!)

        Me.lstProviders.ForeColor =
            Color.Gainsboro

        Me.lstProviders.HorizontalScrollbar =
            True

        Me.lstProviders.IntegralHeight =
            False

        Me.lstProviders.ItemHeight =
            32

        Me.lstProviders.Location =
            New Point(16, 52)

        Me.lstProviders.Name =
            "lstProviders"

        Me.lstProviders.Size =
            New Size(323, 550)

        Me.lstProviders.TabIndex =
            0

        Me.btnNew.Anchor =
            AnchorStyles.Bottom Or
            AnchorStyles.Left Or
            AnchorStyles.Right

        Me.btnNew.BackColor =
            Color.FromArgb(35, 39, 48)

        Me.btnNew.FlatStyle =
            FlatStyle.Flat

        Me.btnNew.FlatAppearance.BorderColor =
            Color.FromArgb(65, 70, 82)

        Me.btnNew.ForeColor =
            Color.White

        Me.btnNew.Location =
            New Point(16, 614)

        Me.btnNew.Name =
            "btnNew"

        Me.btnNew.Size =
            New Size(323, 40)

        Me.btnNew.TabIndex =
            1

        Me.btnNew.Text =
            "+ New Provider"

        Me.btnNew.UseVisualStyleBackColor =
            False

        Me.pnlProviders.Controls.Add(Me.btnNew)
        Me.pnlProviders.Controls.Add(Me.lstProviders)
        Me.pnlProviders.Controls.Add(Me.lblProviders)

        '========================================================
        ' EDITOR PANEL
        '========================================================

        Me.pnlEditor.BackColor =
            Color.FromArgb(18, 20, 24)

        Me.pnlEditor.Dock =
            DockStyle.Fill

        Me.pnlEditor.AutoScroll =
            True

        Me.pnlEditor.Name =
            "pnlEditor"

        Me.lblProvider.AutoSize = False
        Me.lblProvider.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblProvider.ForeColor = Color.Gainsboro
        Me.lblProvider.Location = New Point(24, 18)
        Me.lblProvider.Size = New Size(150, 22)
        Me.lblProvider.Text = "Provider"

        Me.cboProvider.DropDownStyle =
            ComboBoxStyle.DropDownList
        Me.cboProvider.Font =
            New Font("Segoe UI", 9.0!)
        Me.cboProvider.Location =
            New Point(190, 16)
        Me.cboProvider.Name =
            "cboProvider"
        Me.cboProvider.Size =
            New Size(330, 25)
        Me.cboProvider.Items.AddRange(
            New Object() {
                "Gemini",
                "OpenAI",
                "Claude",
                "DeepSeek",
                "Kimi",
                "Custom / OpenAI-Compatible"
            })

        Me.lblModel.AutoSize = False
        Me.lblModel.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblModel.ForeColor = Color.Gainsboro
        Me.lblModel.Location = New Point(24, 56)
        Me.lblModel.Size = New Size(150, 22)
        Me.lblModel.Text = "Model"

        Me.txtModel.Font =
            New Font("Segoe UI", 9.0!)
        Me.txtModel.Location =
            New Point(190, 54)
        Me.txtModel.Name =
            "txtModel"
        Me.txtModel.Size =
            New Size(430, 23)

        Me.lblApiKey.AutoSize = False
        Me.lblApiKey.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblApiKey.ForeColor = Color.Gainsboro
        Me.lblApiKey.Location = New Point(24, 94)
        Me.lblApiKey.Size = New Size(150, 22)
        Me.lblApiKey.Text = "API Key"

        Me.txtApiKey.Font =
            New Font("Segoe UI", 9.0!)
        Me.txtApiKey.Location =
            New Point(190, 92)
        Me.txtApiKey.Name =
            "txtApiKey"
        Me.txtApiKey.Size =
            New Size(430, 23)
        Me.txtApiKey.UseSystemPasswordChar =
            True

        Me.chkShowKey.AutoSize = True
        Me.chkShowKey.ForeColor = Color.Gainsboro
        Me.chkShowKey.Location = New Point(630, 94)
        Me.chkShowKey.Name = "chkShowKey"
        Me.chkShowKey.Size = New Size(84, 21)
        Me.chkShowKey.Text = "Show key"

        Me.lblBaseUrl.AutoSize = False
        Me.lblBaseUrl.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblBaseUrl.ForeColor = Color.Gainsboro
        Me.lblBaseUrl.Location = New Point(24, 132)
        Me.lblBaseUrl.Size = New Size(150, 22)
        Me.lblBaseUrl.Text = "Base URL"

        Me.txtBaseUrl.Font =
            New Font("Segoe UI", 9.0!)
        Me.txtBaseUrl.Location =
            New Point(190, 130)
        Me.txtBaseUrl.Name =
            "txtBaseUrl"
        Me.txtBaseUrl.Size =
            New Size(430, 23)

        Me.lblApiFormat.AutoSize = False
        Me.lblApiFormat.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblApiFormat.ForeColor = Color.Gainsboro
        Me.lblApiFormat.Location = New Point(24, 170)
        Me.lblApiFormat.Size = New Size(150, 22)
        Me.lblApiFormat.Text = "API Format"

        Me.cboApiFormat.DropDownStyle =
            ComboBoxStyle.DropDownList
        Me.cboApiFormat.Font =
            New Font("Segoe UI", 9.0!)
        Me.cboApiFormat.Location =
            New Point(190, 168)
        Me.cboApiFormat.Name =
            "cboApiFormat"
        Me.cboApiFormat.Size =
            New Size(330, 25)
        Me.cboApiFormat.Items.AddRange(
            New Object() {
                "OpenAI-Compatible",
                "Gemini",
                "Anthropic",
                "Custom"
            })

        Me.lblThinking.AutoSize = False
        Me.lblThinking.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblThinking.ForeColor = Color.Gainsboro
        Me.lblThinking.Location = New Point(24, 208)
        Me.lblThinking.Size = New Size(150, 22)
        Me.lblThinking.Text = "Thinking Level"

        Me.cboThinking.DropDownStyle =
            ComboBoxStyle.DropDownList
        Me.cboThinking.Font =
            New Font("Segoe UI", 9.0!)
        Me.cboThinking.Location =
            New Point(190, 206)
        Me.cboThinking.Name =
            "cboThinking"
        Me.cboThinking.Size =
            New Size(180, 25)
        Me.cboThinking.Items.AddRange(
            New Object() {
                "Low",
                "Medium",
                "High"
            })

        Me.lblTemperature.AutoSize = False
        Me.lblTemperature.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblTemperature.ForeColor = Color.Gainsboro
        Me.lblTemperature.Location = New Point(24, 246)
        Me.lblTemperature.Size = New Size(150, 22)
        Me.lblTemperature.Text = "Temperature"

        Me.numTemperature.DecimalPlaces = 2
        Me.numTemperature.Increment = 0.05D
        Me.numTemperature.Minimum = 0D
        Me.numTemperature.Maximum = 2D
        Me.numTemperature.Font =
            New Font("Segoe UI", 9.0!)
        Me.numTemperature.Location =
            New Point(190, 244)
        Me.numTemperature.Name =
            "numTemperature"
        Me.numTemperature.Size =
            New Size(120, 23)

        Me.lblMaxOutput.AutoSize = False
        Me.lblMaxOutput.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblMaxOutput.ForeColor = Color.Gainsboro
        Me.lblMaxOutput.Location = New Point(24, 284)
        Me.lblMaxOutput.Size = New Size(150, 22)
        Me.lblMaxOutput.Text = "Max Output"

        Me.numMaxOutput.Maximum = 1000000D
        Me.numMaxOutput.Minimum = 1D
        Me.numMaxOutput.Increment = 256D
        Me.numMaxOutput.Font =
            New Font("Segoe UI", 9.0!)
        Me.numMaxOutput.Location =
            New Point(190, 282)
        Me.numMaxOutput.Name =
            "numMaxOutput"
        Me.numMaxOutput.Size =
            New Size(150, 23)

        Me.lblTimeout.AutoSize = False
        Me.lblTimeout.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblTimeout.ForeColor = Color.Gainsboro
        Me.lblTimeout.Location = New Point(24, 322)
        Me.lblTimeout.Size = New Size(150, 22)
        Me.lblTimeout.Text = "Timeout (sec)"

        Me.numTimeout.Maximum = 3600D
        Me.numTimeout.Minimum = 5D
        Me.numTimeout.Increment = 5D
        Me.numTimeout.Font =
            New Font("Segoe UI", 9.0!)
        Me.numTimeout.Location =
            New Point(190, 320)
        Me.numTimeout.Name =
            "numTimeout"
        Me.numTimeout.Size =
            New Size(120, 23)

        Me.lblPriority.AutoSize = False
        Me.lblPriority.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.lblPriority.ForeColor = Color.Gainsboro
        Me.lblPriority.Location = New Point(24, 360)
        Me.lblPriority.Size = New Size(150, 22)
        Me.lblPriority.Text = "Priority"

        Me.numPriority.Maximum = 1000D
        Me.numPriority.Minimum = 1D
        Me.numPriority.Font =
            New Font("Segoe UI", 9.0!)
        Me.numPriority.Location =
            New Point(190, 358)
        Me.numPriority.Name =
            "numPriority"
        Me.numPriority.Size =
            New Size(120, 23)

        Me.chkEnabled.AutoSize = True
        Me.chkEnabled.Font =
            New Font("Segoe UI", 9.0!, FontStyle.Bold)
        Me.chkEnabled.ForeColor = Color.Gainsboro
        Me.chkEnabled.Location = New Point(190, 398)
        Me.chkEnabled.Name = "chkEnabled"
        Me.chkEnabled.Size = New Size(100, 21)
        Me.chkEnabled.Text = "Enabled"

        Me.lblInfo.AutoSize = False
        Me.lblInfo.BackColor =
            Color.FromArgb(24, 27, 33)
        Me.lblInfo.BorderStyle =
            BorderStyle.FixedSingle
        Me.lblInfo.Font =
            New Font("Segoe UI", 8.5!)
        Me.lblInfo.ForeColor =
            Color.FromArgb(175, 180, 190)
        Me.lblInfo.Location =
            New Point(24, 438)
        Me.lblInfo.Name =
            "lblInfo"
        Me.lblInfo.Padding =
            New Padding(10)
        Me.lblInfo.Size =
            New Size(720, 72)
        Me.lblInfo.Text =
            "API Key disimpan di file settings DevBot. " &
            "Untuk saat ini nilai API Key disimpan sebagai Base64 " &
            "(bukan enkripsi)."

        Me.pnlEditor.Controls.Add(Me.lblInfo)
        Me.pnlEditor.Controls.Add(Me.chkEnabled)
        Me.pnlEditor.Controls.Add(Me.numPriority)
        Me.pnlEditor.Controls.Add(Me.lblPriority)
        Me.pnlEditor.Controls.Add(Me.numTimeout)
        Me.pnlEditor.Controls.Add(Me.lblTimeout)
        Me.pnlEditor.Controls.Add(Me.numMaxOutput)
        Me.pnlEditor.Controls.Add(Me.lblMaxOutput)
        Me.pnlEditor.Controls.Add(Me.numTemperature)
        Me.pnlEditor.Controls.Add(Me.lblTemperature)
        Me.pnlEditor.Controls.Add(Me.cboThinking)
        Me.pnlEditor.Controls.Add(Me.lblThinking)
        Me.pnlEditor.Controls.Add(Me.cboApiFormat)
        Me.pnlEditor.Controls.Add(Me.lblApiFormat)
        Me.pnlEditor.Controls.Add(Me.txtBaseUrl)
        Me.pnlEditor.Controls.Add(Me.lblBaseUrl)
        Me.pnlEditor.Controls.Add(Me.chkShowKey)
        Me.pnlEditor.Controls.Add(Me.txtApiKey)
        Me.pnlEditor.Controls.Add(Me.lblApiKey)
        Me.pnlEditor.Controls.Add(Me.txtModel)
        Me.pnlEditor.Controls.Add(Me.lblModel)
        Me.pnlEditor.Controls.Add(Me.cboProvider)
        Me.pnlEditor.Controls.Add(Me.lblProvider)

        '========================================================
        ' BUTTONS
        '========================================================

        Me.pnlButtons.BackColor =
            Color.FromArgb(25, 28, 34)

        Me.pnlButtons.Dock =
            DockStyle.Bottom

        Me.pnlButtons.Height =
            58

        Me.pnlButtons.Name =
            "pnlButtons"

        ' Button host - fills the entire bottom bar.
        ' RightToLeft keeps the action buttons packed safely
        ' on the right side regardless of form width.
        Me.flpButtons.Dock =
            DockStyle.Fill

        Me.flpButtons.FlowDirection =
            FlowDirection.RightToLeft

        Me.flpButtons.WrapContents =
            False

        Me.flpButtons.Padding =
            New Padding(10, 13, 10, 13)

        Me.flpButtons.Margin =
            New Padding(0)

        Me.flpButtons.Name =
            "flpButtons"

        Me.flpButtons.AutoSize =
            False

        Me.btnClose.BackColor =
            Color.FromArgb(35, 39, 48)
        Me.btnClose.FlatStyle =
            FlatStyle.Flat
        Me.btnClose.FlatAppearance.BorderColor =
            Color.FromArgb(65, 70, 82)
        Me.btnClose.ForeColor =
            Color.White
        Me.btnClose.Margin =
            New Padding(5, 0, 0, 0)
        Me.btnClose.Name =
            "btnClose"
        Me.btnClose.Size =
            New Size(90, 32)
        Me.btnClose.Text =
            "Close"
        Me.btnClose.UseVisualStyleBackColor =
            False

        Me.btnSave.BackColor =
            Color.FromArgb(70, 55, 130)
        Me.btnSave.FlatStyle =
            FlatStyle.Flat
        Me.btnSave.FlatAppearance.BorderColor =
            Color.FromArgb(105, 85, 175)
        Me.btnSave.ForeColor =
            Color.White
        Me.btnSave.Margin =
            New Padding(5, 0, 0, 0)
        Me.btnSave.Name =
            "btnSave"
        Me.btnSave.Size =
            New Size(90, 32)
        Me.btnSave.Text =
            "Save"
        Me.btnSave.UseVisualStyleBackColor =
            False

        Me.btnDelete.BackColor =
            Color.FromArgb(35, 39, 48)
        Me.btnDelete.FlatStyle =
            FlatStyle.Flat
        Me.btnDelete.FlatAppearance.BorderColor =
            Color.FromArgb(65, 70, 82)
        Me.btnDelete.ForeColor =
            Color.White
        Me.btnDelete.Margin =
            New Padding(5, 0, 0, 0)
        Me.btnDelete.Name =
            "btnDelete"
        Me.btnDelete.Size =
            New Size(90, 32)
        Me.btnDelete.Text =
            "Delete"
        Me.btnDelete.UseVisualStyleBackColor =
            False

        Me.btnDefault.BackColor =
            Color.FromArgb(35, 39, 48)
        Me.btnDefault.FlatStyle =
            FlatStyle.Flat
        Me.btnDefault.FlatAppearance.BorderColor =
            Color.FromArgb(65, 70, 82)
        Me.btnDefault.ForeColor =
            Color.White
        Me.btnDefault.Margin =
            New Padding(5, 0, 0, 0)
        Me.btnDefault.Name =
            "btnDefault"
        Me.btnDefault.Size =
            New Size(120, 32)
        Me.btnDefault.Text =
            "★ Set Default"
        Me.btnDefault.UseVisualStyleBackColor =
            False

        Me.btnTest.BackColor =
            Color.FromArgb(35, 39, 48)
        Me.btnTest.FlatStyle =
            FlatStyle.Flat
        Me.btnTest.FlatAppearance.BorderColor =
            Color.FromArgb(65, 70, 82)
        Me.btnTest.ForeColor =
            Color.White
        Me.btnTest.Margin =
            New Padding(5, 0, 0, 0)
        Me.btnTest.Name =
            "btnTest"
        Me.btnTest.Size =
            New Size(125, 32)
        Me.btnTest.Text =
            "Test Connection"
        Me.btnTest.UseVisualStyleBackColor =
            False

        Me.flpButtons.Controls.Add(Me.btnClose)
        Me.flpButtons.Controls.Add(Me.btnSave)
        Me.flpButtons.Controls.Add(Me.btnDelete)
        Me.flpButtons.Controls.Add(Me.btnDefault)
        Me.flpButtons.Controls.Add(Me.btnTest)

        Me.pnlButtons.Controls.Add(Me.flpButtons)

        '========================================================
        ' SPLIT PANELS / FORM CONTROLS
        '========================================================

        Me.splitMain.Panel1.Controls.Add(Me.pnlProviders)
        Me.splitMain.Panel2.Controls.Add(Me.pnlEditor)

        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.pnlHeader)

        '========================================================
        ' RESUME
        '========================================================

        Me.pnlProviders.ResumeLayout(False)
        Me.pnlEditor.ResumeLayout(False)
        Me.pnlEditor.PerformLayout()
        Me.pnlButtons.ResumeLayout(False)

        CType(Me.numPriority,
              System.ComponentModel.ISupportInitialize).EndInit()

        CType(Me.numTimeout,
              System.ComponentModel.ISupportInitialize).EndInit()

        CType(Me.numMaxOutput,
              System.ComponentModel.ISupportInitialize).EndInit()

        CType(Me.numTemperature,
              System.ComponentModel.ISupportInitialize).EndInit()

        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)

        CType(Me.splitMain,
              System.ComponentModel.ISupportInitialize).EndInit()

        Me.splitMain.ResumeLayout(False)

        Me.pnlHeader.ResumeLayout(False)

        Me.ResumeLayout(False)

    End Sub

End Class
