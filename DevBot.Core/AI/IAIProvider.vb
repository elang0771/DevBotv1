Imports System

Namespace DevBot.Core.AI

    Public Interface IAIProvider

        Function IsConfigured() As Boolean

        Function GetProviderName() As String

        Function AnalyzeAndFix(
            systemPrompt As String,
            userPrompt As String
        ) As AIFixResponse

        Function TestConnection() As AIConnectionTestResult

    End Interface

    Public Class AIFixResponse

        Public Property Success As Boolean
        Public Property RawResponse As String
        Public Property FilePath As String
        Public Property OldText As String
        Public Property NewText As String
        Public Property Explanation As String
        Public Property ErrorMessage As String

        Public Shared Function Fail(
            message As String
        ) As AIFixResponse

            Return New AIFixResponse With {
                .Success = False,
                .ErrorMessage = message
            }

        End Function

        Public Shared Function Fixed(
            filePath As String,
            oldText As String,
            newText As String,
            explanation As String,
            rawResponse As String
        ) As AIFixResponse

            Return New AIFixResponse With {
                .Success = True,
                .FilePath = filePath,
                .OldText = oldText,
                .NewText = newText,
                .Explanation = explanation,
                .RawResponse = rawResponse
            }

        End Function

    End Class

End Namespace
