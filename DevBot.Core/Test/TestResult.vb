Imports System
Imports System.Collections.Generic

Namespace DevBot.Core.Test

    Public Enum TestStatus
        NotStarted
        Running
        Passed
        Failed
        Skipped
        NotSupported
        TestError
    End Enum

    Public Class TestCaseResult

        Public Property Id As String
        Public Property Name As String
        Public Property Status As TestStatus
        Public Property Message As String
        Public Property Output As String
        Public Property ErrorOutput As String
        Public Property AdapterName As String
        Public Property Duration As TimeSpan
        Public Property StartedAt As DateTime
        Public Property FinishedAt As DateTime

        Public Sub New()
            Id = ""
            Name = ""
            Status = TestStatus.NotStarted
            Message = ""
            Output = ""
            ErrorOutput = ""
            AdapterName = ""
            Duration = TimeSpan.Zero
            StartedAt = DateTime.MinValue
            FinishedAt = DateTime.MinValue
        End Sub

    End Class

    Public Class TestRunResult

        Public Property Success As Boolean
        Public Property Status As TestStatus
        Public Property ProjectPath As String
        Public Property Message As String
        Public Property StartedAt As DateTime
        Public Property FinishedAt As DateTime
        Public Property Duration As TimeSpan
        Public Property Tests As List(Of TestCaseResult)

        Public ReadOnly Property PassedCount As Integer
            Get
                Dim count As Integer = 0
                For Each item As TestCaseResult In Tests
                    If item.Status = TestStatus.Passed Then
                        count += 1
                    End If
                Next
                Return count
            End Get
        End Property

        Public ReadOnly Property FailedCount As Integer
            Get
                Dim count As Integer = 0
                For Each item As TestCaseResult In Tests
                    If item.Status = TestStatus.Failed OrElse
                       item.Status = TestStatus.TestError Then
                        count += 1
                    End If
                Next
                Return count
            End Get
        End Property

        Public ReadOnly Property TotalCount As Integer
            Get
                Return Tests.Count
            End Get
        End Property

        Public Sub New()
            Success = False
            Status = TestStatus.NotStarted
            ProjectPath = ""
            Message = ""
            StartedAt = DateTime.MinValue
            FinishedAt = DateTime.MinValue
            Duration = TimeSpan.Zero
            Tests = New List(Of TestCaseResult)()
        End Sub

        Public Function ToDisplayText() As String

            Dim lines As New List(Of String)()

            lines.Add("TEST ENGINE")
            lines.Add("===========")
            lines.Add("Project : " & ProjectPath)
            lines.Add("Status  : " & Status.ToString().ToUpperInvariant())
            lines.Add("Tests   : " & TotalCount.ToString())
            lines.Add("PASS    : " & PassedCount.ToString())
            lines.Add("FAIL    : " & FailedCount.ToString())

            If Duration <> TimeSpan.Zero Then
                lines.Add("Duration: " & Duration.TotalSeconds.ToString("0.00") & " detik")
            End If

            If Not String.IsNullOrWhiteSpace(Message) Then
                lines.Add("")
                lines.Add(Message)
            End If

            If Tests.Count > 0 Then
                lines.Add("")
                lines.Add("TEST CASES")
                lines.Add("----------")

                For Each item As TestCaseResult In Tests
                    lines.Add("[" & item.Status.ToString().ToUpperInvariant() & "] " &
                              item.Id & " - " & item.Name)

                    If Not String.IsNullOrWhiteSpace(item.Message) Then
                        lines.Add("    " & item.Message)
                    End If
                Next
            End If

            Return String.Join(Microsoft.VisualBasic.vbCrLf, lines.ToArray())

        End Function

    End Class

End Namespace
