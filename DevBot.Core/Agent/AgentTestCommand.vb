Imports System
Imports DevBot.Core.Test

Namespace DevBot.Core.Agent

    Public Class AgentTestCommand

        Private ReadOnly agentTestEngine As AgentTestEngine

        Public Sub New()

            agentTestEngine =
                New AgentTestEngine()

        End Sub

        Public Function Run(
            projectPath As String) As AgentResult

            If String.IsNullOrWhiteSpace(projectPath) Then

                Return AgentResult.Fail(
                    "Project belum dibuka.")

            End If

            Try

                Dim result As TestRunResult =
                    agentTestEngine.RunProject(
                        projectPath)

                If result Is Nothing Then

                    Return AgentResult.Fail(
                        "TestEngine tidak mengembalikan hasil.")

                End If

                If result.Success Then

                    Return AgentResult.Success(
                        result.ToDisplayText(),
                        result)

                End If

                Return AgentResult.Fail(
                    result.ToDisplayText())

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal menjalankan TestEngine: " &
                    ex.Message)

            End Try

        End Function

        Public Function RunWithUrl(
            projectPath As String,
            url As String) As AgentResult

            If String.IsNullOrWhiteSpace(projectPath) Then

                Return AgentResult.Fail(
                    "Project belum dibuka.")

            End If

            If String.IsNullOrWhiteSpace(url) Then

                Return Run(projectPath)

            End If

            Try

                Dim result As TestRunResult =
                    agentTestEngine.RunProjectWithUrl(
                        projectPath,
                        url)

                If result Is Nothing Then

                    Return AgentResult.Fail(
                        "TestEngine tidak mengembalikan hasil.")

                End If

                If result.Success Then

                    Return AgentResult.Success(
                        result.ToDisplayText(),
                        result)

                End If

                Return AgentResult.Fail(
                    result.ToDisplayText())

            Catch ex As Exception

                Return AgentResult.Fail(
                    "Gagal menjalankan Browser Test: " &
                    ex.Message)

            End Try

        End Function

        Public Function GetAdapters() As System.Collections.Generic.List(Of ITestAdapter)

            Return agentTestEngine.GetAdapters()

        End Function

    End Class

End Namespace