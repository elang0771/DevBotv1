Imports System
Imports System.Windows.Forms

Partial Public Class FrmMain

    Private Sub FrmMain_WireAIProviderSettings(
        sender As Object,
        e As EventArgs) Handles MyBase.Load

        Try
            RemoveHandler btnSettings.Click,
                AddressOf btnSettings_Click

            AddHandler btnSettings.Click,
                AddressOf OpenAIProviderSettings

        Catch
            ' Settings wiring must never prevent DevBot startup.
        End Try
    End Sub

    Private Sub OpenAIProviderSettings(
        sender As Object,
        e As EventArgs)

        Try
            Using form As New FrmAIProviderSettings()

                form.ShowDialog(Me)

            End Using

        Catch ex As Exception

            MessageBox.Show(
                "Gagal membuka AI Provider Settings." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "DevBot - AI Provider Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        End Try
    End Sub

End Class
