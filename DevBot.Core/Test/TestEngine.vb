Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace DevBot.Core.Test

    Public Class TestEngine

        Private ReadOnly adapters As List(Of ITestAdapter)

        Public Sub New()
            adapters = New List(Of ITestAdapter)()

            ' Built-in adapters
            RegisterAdapter(New WebTestAdapter())
        End Sub

        Public Sub RegisterAdapter(adapter As ITestAdapter)
            If adapter Is Nothing Then Return

            If Not adapters.Contains(adapter) Then
                adapters.Add(adapter)
            End If
        End Sub

        Public Function GetAdapters() As List(Of ITestAdapter)
            Return New List(Of ITestAdapter)(adapters)
        End Function

        Public Function DetectAdapter(projectPath As String) As ITestAdapter

            If String.IsNullOrWhiteSpace(projectPath) Then
                Return Nothing
            End If

            For Each adapter As ITestAdapter In adapters

                Try
                    If adapter.CanHandle(projectPath) Then
                        Return adapter
                    End If
                Catch
                End Try

            Next

            Return Nothing

        End Function

        Public Function Run(projectPath As String) As TestRunResult

            Dim result As New TestRunResult()

            result.ProjectPath = projectPath
            result.StartedAt = DateTime.Now
            result.Status = TestStatus.Running

            If String.IsNullOrWhiteSpace(projectPath) Then

                result.Status = TestStatus.TestError
                result.Message = "Project path kosong."

                Return FinishRun(result)

            End If

            If Not Directory.Exists(projectPath) Then

                result.Status = TestStatus.TestError
                result.Message =
                    "Project tidak ditemukan: " & projectPath

                Return FinishRun(result)

            End If

            Dim adapter As ITestAdapter =
                DetectAdapter(projectPath)

            If adapter Is Nothing Then

                result.Status = TestStatus.NotSupported
                result.Message =
                    "Belum ada Test Adapter yang cocok untuk project ini."

                Return FinishRun(result)

            End If

            Dim context As New TestContext()

            context.ProjectPath = projectPath
            context.WorkingDirectory = projectPath
            context.TestId = "AUTO-001"
            context.TestName = "Automatic Web UI diagnostic test"

            Dim adapterResult As TestAdapterResult = Nothing

            Try

                adapterResult = adapter.Run(context)

            Catch ex As Exception

                adapterResult = New TestAdapterResult()

                adapterResult.Success = False
                adapterResult.Status = "ERROR"
                adapterResult.Message = ex.Message
                adapterResult.ErrorOutput = ex.ToString()

            End Try

            Dim testCase As New TestCaseResult()

            testCase.Id = context.TestId
            testCase.Name = context.TestName
            testCase.AdapterName = adapter.GetAdapterName()
            testCase.StartedAt = result.StartedAt
            testCase.FinishedAt = DateTime.Now

            If adapterResult Is Nothing Then

                testCase.Status = TestStatus.TestError
                testCase.Message =
                    "Test adapter tidak mengembalikan hasil."

            Else

                testCase.Output = adapterResult.Output
                testCase.ErrorOutput = adapterResult.ErrorOutput
                testCase.Message = adapterResult.Message
                testCase.Duration = adapterResult.Duration

                If adapterResult.Success Then

                    testCase.Status = TestStatus.Passed

                ElseIf String.Equals(
                    adapterResult.Status,
                    "NOT_SUPPORTED",
                    StringComparison.OrdinalIgnoreCase) Then

                    testCase.Status = TestStatus.NotSupported

                ElseIf String.Equals(
                    adapterResult.Status,
                    "ERROR",
                    StringComparison.OrdinalIgnoreCase) Then

                    testCase.Status = TestStatus.TestError

                Else

                    testCase.Status = TestStatus.Failed

                End If

            End If

            If testCase.Duration = TimeSpan.Zero Then
                testCase.Duration =
                    testCase.FinishedAt -
                    testCase.StartedAt
            End If

            result.Tests.Add(testCase)

            Select Case testCase.Status

                Case TestStatus.Passed

                    result.Success = True
                    result.Status = TestStatus.Passed
                    result.Message =
                        "Test selesai. Semua pemeriksaan otomatis yang dijalankan PASS."

                Case TestStatus.NotSupported

                    result.Success = False
                    result.Status = TestStatus.NotSupported
                    result.Message = testCase.Message

                Case TestStatus.TestError

                    result.Success = False
                    result.Status = TestStatus.TestError
                    result.Message =
                        "Test Engine mengalami error. Lihat detail TEST CASE."

                Case Else

                    result.Success = False
                    result.Status = TestStatus.Failed
                    result.Message =
                        "Test gagal. Lihat detail TEST CASE."

            End Select

            Return FinishRun(result)

        End Function

        Public Function RunWithUrl(
            projectPath As String,
            url As String) As TestRunResult

            Dim result As New TestRunResult()

            result.ProjectPath = projectPath
            result.StartedAt = DateTime.Now
            result.Status = TestStatus.Running

            If String.IsNullOrWhiteSpace(projectPath) OrElse
               Not Directory.Exists(projectPath) Then

                result.Status = TestStatus.TestError
                result.Message =
                    "Project tidak ditemukan: " & projectPath

                Return FinishRun(result)

            End If

            Dim adapter As ITestAdapter =
                DetectAdapter(projectPath)

            If adapter Is Nothing Then

                result.Status = TestStatus.NotSupported
                result.Message =
                    "Tidak ada Test Adapter yang cocok."

                Return FinishRun(result)

            End If

            Dim context As New TestContext()

            context.ProjectPath = projectPath
            context.WorkingDirectory = projectPath
            context.TestId = "AUTO-URL-001"
            context.TestName = "Automatic browser UI test"
            context.Arguments = url

            Dim adapterResult As TestAdapterResult = Nothing

            Try

                adapterResult = adapter.Run(context)

            Catch ex As Exception

                adapterResult = New TestAdapterResult()
                adapterResult.Success = False
                adapterResult.Status = "ERROR"
                adapterResult.Message = ex.Message
                adapterResult.ErrorOutput = ex.ToString()

            End Try

            Dim testCase As New TestCaseResult()

            testCase.Id = context.TestId
            testCase.Name = context.TestName
            testCase.AdapterName = adapter.GetAdapterName()
            testCase.StartedAt = result.StartedAt
            testCase.FinishedAt = DateTime.Now

            If adapterResult Is Nothing Then

                testCase.Status = TestStatus.TestError
                testCase.Message =
                    "Test adapter tidak mengembalikan hasil."

            Else

                testCase.Output = adapterResult.Output
                testCase.ErrorOutput = adapterResult.ErrorOutput
                testCase.Message = adapterResult.Message
                testCase.Duration = adapterResult.Duration

                If adapterResult.Success Then
                    testCase.Status = TestStatus.Passed
                ElseIf String.Equals(
                    adapterResult.Status,
                    "NOT_SUPPORTED",
                    StringComparison.OrdinalIgnoreCase) Then
                    testCase.Status = TestStatus.NotSupported
                ElseIf String.Equals(
                    adapterResult.Status,
                    "ERROR",
                    StringComparison.OrdinalIgnoreCase) Then
                    testCase.Status = TestStatus.TestError
                Else
                    testCase.Status = TestStatus.Failed
                End If

            End If

            If testCase.Duration = TimeSpan.Zero Then
                testCase.Duration =
                    testCase.FinishedAt -
                    testCase.StartedAt
            End If

            result.Tests.Add(testCase)

            If testCase.Status = TestStatus.Passed Then
                result.Success = True
                result.Status = TestStatus.Passed
                result.Message =
                    "Browser test selesai PASS."
            Else
                result.Success = False
                result.Status = testCase.Status
                result.Message = testCase.Message
            End If

            Return FinishRun(result)

        End Function

        Public Function GetStatusText(
            result As TestRunResult) As String

            If result Is Nothing Then
                Return "TEST RESULT KOSONG"
            End If

            Return result.ToDisplayText()

        End Function

        Public Function RunDiagnostic(
            projectPath As String) As TestRunResult

            Return Run(projectPath)

        End Function

        Private Function FinishRun(
            result As TestRunResult) As TestRunResult

            result.FinishedAt = DateTime.Now
            result.Duration =
                result.FinishedAt -
                result.StartedAt

            Return result

        End Function

    End Class

End Namespace
