Imports System

Namespace DevBot.Core.AI

    Public NotInheritable Class AIProviderFactory

        Private Sub New()
        End Sub

        ' ============================================================
        ' CREATE PROVIDER FROM SETTINGS
        ' ============================================================
        Public Shared Function Create(
            settings As AIProviderSettings
        ) As IAIProvider

            If settings Is Nothing Then
                Return Nothing
            End If

            Dim provider As String =
                If(
                    settings.Provider,
                    String.Empty).
                Trim().
                ToLowerInvariant()

            Select Case provider

                ' ----------------------------------------------------
                ' GOOGLE GEMINI
                ' ----------------------------------------------------
                Case "gemini",
                     "google gemini",
                     "google"

                    Return New GeminiProvider(settings)

                ' ----------------------------------------------------
                ' OPENAI
                ' ----------------------------------------------------
                Case "openai"

                    Return New OpenAICompatibleProvider(settings)

                ' ----------------------------------------------------
                ' DEEPSEEK
                ' ----------------------------------------------------
                Case "deepseek"

                    Return New OpenAICompatibleProvider(settings)

                ' ----------------------------------------------------
                ' KIMI
                ' ----------------------------------------------------
                Case "kimi",
                     "moonshot",
                     "moonshot ai"

                    Return New OpenAICompatibleProvider(settings)

                ' ----------------------------------------------------
                ' CLAUDE
                '
                ' Untuk sementara Claude menggunakan adapter
                ' OpenAI-Compatible hanya jika API Format memang
                ' diset ke OpenAI-Compatible.
                ' ----------------------------------------------------
                Case "claude",
                     "anthropic"

                    If String.Equals(
                        settings.ApiFormat,
                        "OpenAI-Compatible",
                        StringComparison.OrdinalIgnoreCase) Then

                        Return New OpenAICompatibleProvider(settings)

                    End If

                    Return Nothing

                ' ----------------------------------------------------
                ' CUSTOM / OPENAI COMPATIBLE
                ' ----------------------------------------------------
                Case "custom",
                     "custom / openai-compatible",
                     "openai-compatible",
                     "openai compatible"

                    Return New OpenAICompatibleProvider(settings)

                ' ----------------------------------------------------
                ' UNKNOWN PROVIDER
                ' ----------------------------------------------------
                Case Else

                    If String.Equals(
                        settings.ApiFormat,
                        "OpenAI-Compatible",
                        StringComparison.OrdinalIgnoreCase) Then

                        Return New OpenAICompatibleProvider(settings)

                    End If

                    Return Nothing

            End Select

        End Function

        ' ============================================================
        ' CREATE DEFAULT PROVIDER
        '
        ' Provider default diambil dari AIProviderSettingsStore.
        ' Tidak ada API Key / Model / URL hardcode di sini.
        ' ============================================================
        Public Shared Function CreateDefault() As IAIProvider

            Dim store As New AIProviderSettingsStore()

            Dim settings As AIProviderSettings =
                store.GetDefaultProvider()

            If settings Is Nothing Then
                Return Nothing
            End If

            Return Create(settings)

        End Function

    End Class

End Namespace