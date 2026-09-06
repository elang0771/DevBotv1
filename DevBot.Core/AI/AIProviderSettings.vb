Imports System
Imports System.Collections.Generic

Namespace DevBot.Core.AI

    Public Class AIProviderSettings

        Public Property Id As String

        Public Property Provider As String

        Public Property Model As String

        Public Property ApiKey As String

        Public Property BaseUrl As String

        Public Property ApiFormat As String

        Public Property ThinkingLevel As String

        Public Property Temperature As Double

        Public Property MaxOutput As Integer

        Public Property TimeoutSeconds As Integer

        Public Property Enabled As Boolean

        Public Property Priority As Integer

        Public Sub New()

            Id = Guid.NewGuid().ToString("N")

            Provider =
                String.Empty

            Model =
                String.Empty

            ApiKey =
                String.Empty

            BaseUrl =
                String.Empty

            ApiFormat =
                "OpenAI-Compatible"

            ThinkingLevel =
                "Medium"

            Temperature =
                0.2R

            MaxOutput =
                8192

            TimeoutSeconds =
                120

            Enabled =
                True

            Priority =
                1

        End Sub

        Public Function Clone() As AIProviderSettings

            Return New AIProviderSettings With {
                .Id = Me.Id,
                .Provider = Me.Provider,
                .Model = Me.Model,
                .ApiKey = Me.ApiKey,
                .BaseUrl = Me.BaseUrl,
                .ApiFormat = Me.ApiFormat,
                .ThinkingLevel = Me.ThinkingLevel,
                .Temperature = Me.Temperature,
                .MaxOutput = Me.MaxOutput,
                .TimeoutSeconds = Me.TimeoutSeconds,
                .Enabled = Me.Enabled,
                .Priority = Me.Priority
            }

        End Function

        Public Function IsValid() As Boolean

            If String.IsNullOrWhiteSpace(
                Provider) Then

                Return False

            End If

            If String.IsNullOrWhiteSpace(
                Model) Then

                Return False

            End If

            If Temperature < 0 Then
                Return False
            End If

            If Temperature > 2 Then
                Return False
            End If

            If MaxOutput < 1 Then
                Return False
            End If

            If TimeoutSeconds < 1 Then
                Return False
            End If

            If Priority < 1 Then
                Return False
            End If

            Return True

        End Function

        Public Overrides Function ToString() As String

            Dim providerText As String =
                If(
                    String.IsNullOrWhiteSpace(Provider),
                    "Unknown",
                    Provider)

            Dim modelText As String =
                If(
                    String.IsNullOrWhiteSpace(Model),
                    "No model",
                    Model)

            Return providerText &
                   " - " &
                   modelText

        End Function

    End Class


    Public Class AIProviderSettingsCollection

        Public Property Providers As List(Of AIProviderSettings)

        Public Sub New()

            Providers =
                New List(Of AIProviderSettings)()

        End Sub

    End Class

End Namespace