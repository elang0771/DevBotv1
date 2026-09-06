Imports System

Namespace DevBot.Core.Test

    Public Interface ITestAdapter

        Function CanHandle(projectPath As String) As Boolean

        Function GetAdapterName() As String

        Function Run(context As TestContext) As TestAdapterResult

    End Interface

    Public Class TestContext

        Public Property ProjectPath As String
        Public Property TestId As String
        Public Property TestName As String
        Public Property WorkingDirectory As String
        Public Property Arguments As String
        Public Property TimeoutMilliseconds As Integer

        Public Sub New()
            TimeoutMilliseconds = 120000
        End Sub

    End Class

    Public Class TestAdapterResult

        Public Property Success As Boolean
        Public Property Status As String
        Public Property Message As String
        Public Property Output As String
        Public Property ErrorOutput As String
        Public Property ExitCode As Integer
        Public Property Duration As TimeSpan
        Public Property AdapterName As String

        Public Sub New()
            Success = False
            Status = "UNKNOWN"
            Message = ""
            Output = ""
            ErrorOutput = ""
            ExitCode = -1
            Duration = TimeSpan.Zero
            AdapterName = ""
        End Sub

    End Class

End Namespace
