Imports System
Imports System.Collections.Generic
Imports DevBot.Core.Test

Namespace DevBot.Core.Agent

    Public Class AgentTestEngine

        Private ReadOnly testEngine As TestEngine

        Public Sub New()
            testEngine = New TestEngine()
        End Sub

        Public Function RunProject(projectPath As String) As TestRunResult
            Return testEngine.Run(projectPath)
        End Function

        Public Function RunProjectWithUrl(
            projectPath As String,
            url As String) As TestRunResult

            Return testEngine.RunWithUrl(
                projectPath,
                url)

        End Function

        Public Function GetAdapters() As List(Of ITestAdapter)
            Return testEngine.GetAdapters()
        End Function

        Public Function GetStatusText(
            result As TestRunResult) As String

            Return testEngine.GetStatusText(result)

        End Function

    End Class

End Namespace