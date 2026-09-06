Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports DevBot.Core.AI

Partial Public Class FrmAIProviderSettings

    Private providers As List(Of AIProviderSettings)
    Private currentProviderId As String = String.Empty
    Private ReadOnly settingsStore As New AIProviderSettingsStore()

    Public Sub New()
        InitializeComponent()

        AddHandler lstProviders.SelectedIndexChanged,
            AddressOf lstProviders_SelectedIndexChanged

        AddHandler cboProvider.SelectedIndexChanged,
            AddressOf cboProvider_SelectedIndexChanged

        AddHandler btnNew.Click,
            AddressOf btnNew_Click

        AddHandler btnSave.Click,
            AddressOf btnSave_Click

        AddHandler btnDelete.Click,
            AddressOf btnDelete_Click

        AddHandler btnDefault.Click,
            AddressOf btnDefault_Click

        AddHandler btnTest.Click,
            AddressOf btnTest_Click

        AddHandler btnClose.Click,
            AddressOf btnClose_Click

        AddHandler Me.Shown,
            AddressOf FrmAIProviderSettings_Shown

        AddHandler Me.Resize,
            AddressOf FrmAIProviderSettings_Resize

        AddHandler chkShowKey.CheckedChanged,
            AddressOf chkShowKey_CheckedChanged

        LoadProviders()
    End Sub

    Private Sub LoadProviders()
        Try
            providers = settingsStore.LoadAll()

            If providers Is Nothing Then
                providers = New List(Of AIProviderSettings)()
            End If

            SortProviders()
            RefreshProviderList()

            If lstProviders.Items.Count > 0 Then
                lstProviders.SelectedIndex = 0
            Else
                ClearEditor()
            End If

        Catch ex As Exception
            providers = New List(Of AIProviderSettings)()

            MessageBox.Show(
                "Gagal membaca AI Provider Settings." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "DevBot - AI Provider Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SortProviders()
        providers =
            providers.
            OrderByDescending(Function(x) x.Enabled).
            ThenBy(Function(x) x.Priority).
            ThenBy(Function(x) x.Provider).
            ToList()
    End Sub

    Private Sub RefreshProviderList()
        lstProviders.BeginUpdate()

        Try
            lstProviders.Items.Clear()

            For Each item As AIProviderSettings In providers
                lstProviders.Items.Add(
                    BuildProviderListText(item))
            Next
        Finally
            lstProviders.EndUpdate()
        End Try
    End Sub

    Private Function BuildProviderListText(
        item As AIProviderSettings) As String

        If item Is Nothing Then
            Return String.Empty
        End If

        Dim status As String =
            If(item.Enabled, "ON", "OFF")

        Dim marker As String =
            If(
                IsDefaultProvider(item),
                " ★ DEFAULT",
                String.Empty)

        Return item.Provider &
               "  |  " &
               If(
                   String.IsNullOrWhiteSpace(item.Model),
                   "(model belum diisi)",
                   item.Model) &
               "  |  " &
               status &
               marker
    End Function

    Private Function IsDefaultProvider(
        item As AIProviderSettings) As Boolean

        If item Is Nothing Then
            Return False
        End If

        Dim defaultProvider =
            settingsStore.GetDefaultProvider()

        If defaultProvider Is Nothing Then
            Return False
        End If

        Return String.Equals(
            defaultProvider.Id,
            item.Id,
            StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub lstProviders_SelectedIndexChanged(
        sender As Object,
        e As EventArgs)

        If lstProviders.SelectedIndex < 0 OrElse
           lstProviders.SelectedIndex >= providers.Count Then
            Return
        End If

        Dim item As AIProviderSettings =
            providers(lstProviders.SelectedIndex)

        If item Is Nothing Then
            Return
        End If

        currentProviderId = item.Id
        LoadEditor(item)
    End Sub

    Private Sub LoadEditor(
        item As AIProviderSettings)

        If item Is Nothing Then
            ClearEditor()
            Return
        End If

        cboProvider.Text = item.Provider
        txtModel.Text = item.Model
        txtApiKey.Text = item.ApiKey
        txtBaseUrl.Text = item.BaseUrl
        cboApiFormat.Text = item.ApiFormat
        cboThinking.Text = item.ThinkingLevel
        numTemperature.Value =
            ClampDecimal(
                CDec(item.Temperature),
                numTemperature.Minimum,
                numTemperature.Maximum)
        numMaxOutput.Value =
            ClampDecimal(
                CDec(item.MaxOutput),
                numMaxOutput.Minimum,
                numMaxOutput.Maximum)
        numTimeout.Value =
            ClampDecimal(
                CDec(item.TimeoutSeconds),
                numTimeout.Minimum,
                numTimeout.Maximum)
        chkEnabled.Checked = item.Enabled
        numPriority.Value =
            ClampDecimal(
                CDec(item.Priority),
                numPriority.Minimum,
                numPriority.Maximum)

        UpdateProviderHints()
        UpdateDefaultButtonText()
    End Sub

    Private Function ClampDecimal(
        value As Decimal,
        minimum As Decimal,
        maximum As Decimal) As Decimal

        If value < minimum Then
            Return minimum
        End If

        If value > maximum Then
            Return maximum
        End If

        Return value
    End Function

    Private Sub ClearEditor()
        currentProviderId = String.Empty

        cboProvider.SelectedIndex = 0
        txtModel.Text = String.Empty
        txtApiKey.Text = String.Empty
        txtBaseUrl.Text = String.Empty
        cboApiFormat.SelectedIndex = 0
        cboThinking.SelectedIndex = 1
        numTemperature.Value = 0.2D
        numMaxOutput.Value = 8192D
        numTimeout.Value = 120D
        chkEnabled.Checked = True
        numPriority.Value = 1D

        UpdateProviderHints()
        UpdateDefaultButtonText()
    End Sub

    Private Function GetEditorSettings() As AIProviderSettings
        Dim item As AIProviderSettings = Nothing

        If Not String.IsNullOrWhiteSpace(currentProviderId) Then
            item =
                providers.FirstOrDefault(
                    Function(x)
                        Return String.Equals(
                            x.Id,
                            currentProviderId,
                            StringComparison.OrdinalIgnoreCase)
                    End Function)
        End If

        If item Is Nothing Then
            item = New AIProviderSettings()
        End If

        item.Provider = cboProvider.Text.Trim()
        item.Model = txtModel.Text.Trim()
        item.ApiKey = txtApiKey.Text
        item.BaseUrl = txtBaseUrl.Text.Trim()
        item.ApiFormat = cboApiFormat.Text.Trim()
        item.ThinkingLevel = cboThinking.Text.Trim()
        item.Temperature = CDbl(numTemperature.Value)
        item.MaxOutput = CInt(numMaxOutput.Value)
        item.TimeoutSeconds = CInt(numTimeout.Value)
        item.Enabled = chkEnabled.Checked
        item.Priority = CInt(numPriority.Value)

        Return item
    End Function

    Private Function ValidateEditor(
        item As AIProviderSettings) As Boolean

        If item Is Nothing Then
            Return False
        End If

        If String.IsNullOrWhiteSpace(item.Provider) Then
            MessageBox.Show(
                "Provider belum dipilih.",
                "DevBot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return False
        End If

        If String.IsNullOrWhiteSpace(item.Model) Then
            MessageBox.Show(
                "Model belum diisi.",
                "DevBot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            txtModel.Focus()
            Return False
        End If

        If item.Enabled AndAlso
           String.IsNullOrWhiteSpace(item.ApiKey) Then

            MessageBox.Show(
                "API Key belum diisi untuk provider yang aktif.",
                "DevBot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtApiKey.Focus()
            Return False
        End If

        If Not String.IsNullOrWhiteSpace(item.BaseUrl) Then
            If Not Uri.IsWellFormedUriString(
                item.BaseUrl,
                UriKind.Absolute) Then

                MessageBox.Show(
                    "Base URL tidak valid.",
                    "DevBot",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                txtBaseUrl.Focus()
                Return False
            End If
        End If

        If Not item.IsValid() Then
            MessageBox.Show(
                "Data provider belum valid. Periksa Provider, Model, API Key, dan Base URL.",
                "DevBot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return False
        End If

        Return True
    End Function

    Private Sub btnNew_Click(
        sender As Object,
        e As EventArgs)

        ClearEditor()
        cboProvider.Focus()
    End Sub

    Private Sub btnSave_Click(
        sender As Object,
        e As EventArgs)

        Try
            Dim item As AIProviderSettings =
                GetEditorSettings()

            If Not ValidateEditor(item) Then
                Return
            End If

            Dim existingIndex As Integer =
                providers.FindIndex(
                    Function(x)
                        Return String.Equals(
                            x.Id,
                            item.Id,
                            StringComparison.OrdinalIgnoreCase)
                    End Function)

            If existingIndex >= 0 Then
                providers(existingIndex) = item
            Else
                providers.Add(item)
            End If

            currentProviderId = item.Id

            settingsStore.SaveAll(providers)

            SortProviders()
            RefreshProviderList()

            SelectProviderById(
                currentProviderId)

            MessageBox.Show(
                "AI Provider berhasil disimpan." &
                Environment.NewLine &
                Environment.NewLine &
                item.Provider & " / " & item.Model,
                "DevBot - AI Provider Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(
                "Gagal menyimpan AI Provider." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "DevBot - AI Provider Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnDelete_Click(
        sender As Object,
        e As EventArgs)

        If String.IsNullOrWhiteSpace(currentProviderId) Then
            Return
        End If

        Dim item =
            providers.FirstOrDefault(
                Function(x)
                    Return String.Equals(
                        x.Id,
                        currentProviderId,
                        StringComparison.OrdinalIgnoreCase)
                End Function)

        If item Is Nothing Then
            Return
        End If

        If MessageBox.Show(
            "Hapus provider berikut?" &
            Environment.NewLine &
            Environment.NewLine &
            item.Provider & " / " & item.Model,
            "DevBot - Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Try
            settingsStore.Delete(item.Id)

            providers.RemoveAll(
                Function(x)
                    Return String.Equals(
                        x.Id,
                        item.Id,
                        StringComparison.OrdinalIgnoreCase)
                End Function)

            currentProviderId = String.Empty

            RefreshProviderList()

            If lstProviders.Items.Count > 0 Then
                lstProviders.SelectedIndex = 0
            Else
                ClearEditor()
            End If

        Catch ex As Exception
            MessageBox.Show(
                "Gagal menghapus provider." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "DevBot - AI Provider Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnDefault_Click(
        sender As Object,
        e As EventArgs)

        If String.IsNullOrWhiteSpace(currentProviderId) Then
            Return
        End If

        Try
            Dim item =
                providers.FirstOrDefault(
                    Function(x)
                        Return String.Equals(
                            x.Id,
                            currentProviderId,
                            StringComparison.OrdinalIgnoreCase)
                    End Function)

            If item Is Nothing Then
                Return
            End If

            For Each provider As AIProviderSettings In providers
                If provider IsNot Nothing Then
                    provider.Priority = 100
                End If
            Next

            item.Priority = 1
            item.Enabled = True

            settingsStore.SaveAll(providers)

            SortProviders()
            RefreshProviderList()
            SelectProviderById(item.Id)

            MessageBox.Show(
                item.Provider & " / " & item.Model &
                Environment.NewLine &
                "sekarang menjadi Default Provider.",
                "DevBot - AI Provider Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(
                "Gagal menetapkan Default Provider." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "DevBot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SelectProviderById(
        providerId As String)

        For i As Integer = 0 To lstProviders.Items.Count - 1

            If i >= providers.Count Then
                Exit For
            End If

            If String.Equals(
                providers(i).Id,
                providerId,
                StringComparison.OrdinalIgnoreCase) Then

                lstProviders.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

    Private Sub btnTest_Click(
        sender As Object,
        e As EventArgs)

        Dim item As AIProviderSettings =
            GetEditorSettings()

        If Not ValidateEditor(item) Then
            Return
        End If

        Try
            Dim provider As IAIProvider =
                AIProviderFactory.Create(item)

            If provider Is Nothing Then
                MessageBox.Show(
                    "Provider tidak dapat dibuat.",
                    "DevBot - Test Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
                Return
            End If

            Dim started As DateTime = DateTime.Now
            Dim result As AIConnectionTestResult =
                provider.TestConnection()
            Dim elapsed As Double =
                (DateTime.Now - started).TotalMilliseconds

            If result Is Nothing Then
                MessageBox.Show(
                    "Provider tidak mengembalikan hasil test.",
                    "DevBot - Test Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
                Return
            End If

            Dim message As String =
                If(result.Success,
                   "CONNECTION OK",
                   "CONNECTION FAILED") &
                Environment.NewLine &
                Environment.NewLine &
                "Provider : " & item.Provider &
                Environment.NewLine &
                "Model    : " & item.Model &
                Environment.NewLine &
                "Format   : " & item.ApiFormat &
                Environment.NewLine &
                "Latency  : " & CInt(elapsed).ToString() & " ms" &
                Environment.NewLine &
                Environment.NewLine &
                result.Message

            MessageBox.Show(
                message,
                "DevBot - Test Connection",
                MessageBoxButtons.OK,
                If(result.Success,
                   MessageBoxIcon.Information,
                   MessageBoxIcon.Error))

        Catch ex As Exception
            MessageBox.Show(
                "Test Connection gagal." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "DevBot - Test Connection",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FrmAIProviderSettings_Shown(
        sender As Object,
        e As EventArgs)

        SetSafeSplitterDistance()
    End Sub

    Private Sub FrmAIProviderSettings_Resize(
        sender As Object,
        e As EventArgs)

        SetSafeSplitterDistance()
    End Sub

    Private Sub SetSafeSplitterDistance()

        If splitMain Is Nothing Then
            Return
        End If

        If splitMain.Width <= 0 Then
            Return
        End If

        'Apply minimum sizes only after the control has a real width.
        'Setting these during InitializeComponent can make WinForms
        'validate an invalid SplitterDistance and throw at startup.
        Dim panel1Minimum As Integer = 240
        Dim panel2Minimum As Integer = 420

        If splitMain.Width <=
           panel1Minimum + panel2Minimum + splitMain.SplitterWidth Then
            panel1Minimum = 180
            panel2Minimum = 300
        End If

        Try
            splitMain.Panel1MinSize = panel1Minimum
            splitMain.Panel2MinSize = panel2Minimum
        Catch
            Return
        End Try

        Dim minimum As Integer =
            splitMain.Panel1MinSize

        Dim maximum As Integer =
            splitMain.Width -
            splitMain.Panel2MinSize -
            splitMain.SplitterWidth

        If maximum < minimum Then
            Return
        End If

        Dim desired As Integer =
            CInt(splitMain.Width * 0.32R)

        If desired < minimum Then
            desired = minimum
        End If

        If desired > maximum Then
            desired = maximum
        End If

        Try
            If splitMain.SplitterDistance <> desired Then
                splitMain.SplitterDistance = desired
            End If
        Catch
            'Never allow splitter layout to prevent the settings form opening.
        End Try

    End Sub

    Private Sub btnClose_Click(
        sender As Object,
        e As EventArgs)

        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub chkShowKey_CheckedChanged(
        sender As Object,
        e As EventArgs)

        txtApiKey.UseSystemPasswordChar =
            Not chkShowKey.Checked
    End Sub

    Private Sub cboProvider_SelectedIndexChanged(
        sender As Object,
        e As EventArgs)

        UpdateProviderHints()
    End Sub

    Private Sub UpdateProviderHints()
        Dim provider As String =
            cboProvider.Text.Trim().ToLowerInvariant()

        Select Case provider

            Case "gemini"
                If String.IsNullOrWhiteSpace(txtBaseUrl.Text) Then
                    txtBaseUrl.Text =
                        "https://generativelanguage.googleapis.com"
                End If

                cboApiFormat.Text = "Gemini"

            Case "openai"
                If String.IsNullOrWhiteSpace(txtBaseUrl.Text) Then
                    txtBaseUrl.Text =
                        "https://api.openai.com/v1"
                End If

                cboApiFormat.Text = "OpenAI-Compatible"

            Case "deepseek"
                If String.IsNullOrWhiteSpace(txtBaseUrl.Text) Then
                    txtBaseUrl.Text =
                        "https://api.deepseek.com"
                End If

                cboApiFormat.Text = "OpenAI-Compatible"

            Case "kimi"
                If String.IsNullOrWhiteSpace(txtBaseUrl.Text) Then
                    txtBaseUrl.Text =
                        "https://api.moonshot.cn/v1"
                End If

                cboApiFormat.Text = "OpenAI-Compatible"

            Case "claude"
                If String.IsNullOrWhiteSpace(txtBaseUrl.Text) Then
                    txtBaseUrl.Text =
                        "https://api.anthropic.com"
                End If

                cboApiFormat.Text = "Anthropic"

            Case "custom / openai-compatible"
                cboApiFormat.Text = "OpenAI-Compatible"

        End Select

        UpdateDefaultButtonText()
    End Sub

    Private Sub UpdateDefaultButtonText()
        If String.IsNullOrWhiteSpace(currentProviderId) Then
            btnDefault.Text = "★ Set Default"
            Return
        End If

        Dim item =
            providers.FirstOrDefault(
                Function(x)
                    Return String.Equals(
                        x.Id,
                        currentProviderId,
                        StringComparison.OrdinalIgnoreCase)
                End Function)

        If item IsNot Nothing AndAlso
           IsDefaultProvider(item) Then

            btnDefault.Text = "★ Default"
        Else
            btnDefault.Text = "★ Set Default"
        End If
    End Sub

End Class
