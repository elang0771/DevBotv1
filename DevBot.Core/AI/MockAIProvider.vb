Imports System
Imports DevBot.Core.AI

Namespace DevBot.Core.AI

    Public Class MockAIProvider
        Implements IAIProvider

        Public Function IsConfigured() As Boolean _
            Implements IAIProvider.IsConfigured

            Return True

        End Function

        Public Function GetProviderName() As String _
            Implements IAIProvider.GetProviderName

            Return "Mock AI"

        End Function

        Public Function SendMessage(
            prompt As String
        ) As String

            If String.IsNullOrWhiteSpace(prompt) Then
                Return "Mock AI: prompt kosong."
            End If

            Return _
                "Mock AI aktif." &
                Microsoft.VisualBasic.vbCrLf &
                "Prompt diterima." &
                Microsoft.VisualBasic.vbCrLf &
                "Belum ada perubahan code yang dilakukan."

        End Function


        Public Function TestConnection() As AIConnectionTestResult _
            Implements IAIProvider.TestConnection

            Return AIConnectionTestResult.Ok(
                "Mock AI connection OK.",
                0)

        End Function

        Public Function AnalyzeAndFix(
            systemPrompt As String,
            userPrompt As String
        ) As AIFixResponse _
            Implements IAIProvider.AnalyzeAndFix

            Return AIFixResponse.Fail(
                "MockAIProvider tidak melakukan perubahan code. " &
                "Gunakan OpenAICompatibleProvider untuk AI Auto Fix.")

        End Function

    End Class

End Namespace
