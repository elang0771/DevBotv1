Imports System
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports DevBot.Core.Build

Namespace DevBot.Core.Agent

    Public Class AgentBuildEngine

        Private ReadOnly buildEngine As BuildEngine

        Public Sub New()
            buildEngine = New BuildEngine()
        End Sub

        Public Function BuildProject(projectPath As String) As AgentBuildResult

            Dim result As New AgentBuildResult()

            result.StartTime = DateTime.Now

            Try

                If String.IsNullOrWhiteSpace(projectPath) Then
                    result.Success = False
                    result.Message = "Project path kosong."
                    result.EndTime = DateTime.Now
                    Return result
                End If

                If Not System.IO.Directory.Exists(projectPath) AndAlso
                   Not System.IO.File.Exists(projectPath) Then

                    result.Success = False
                    result.Message = "Project tidak ditemukan: " & projectPath
                    result.EndTime = DateTime.Now
                    Return result

                End If

                Dim buildResult As BuildResult =
                    buildEngine.Build(projectPath)

                result.Success = buildResult.Success
                result.ExitCode = buildResult.ExitCode
                result.Command = buildResult.Command
                result.Output = buildResult.Output
                result.ErrorOutput = buildResult.ErrorOutput
                result.Message = buildResult.Message

                If Not String.IsNullOrWhiteSpace(buildResult.Output) Then
                    ParseErrors(buildResult.Output, result.Errors)
                End If

                If Not String.IsNullOrWhiteSpace(buildResult.ErrorOutput) Then
                    ParseErrors(buildResult.ErrorOutput, result.Errors)
                End If

            Catch ex As Exception

                result.Success = False
                result.Message = ex.Message

                If result.Errors Is Nothing Then
                    result.Errors = New List(Of BuildError)()
                End If

            End Try

            result.EndTime = DateTime.Now

            Return result

        End Function

        Private Sub ParseErrors(
            output As String,
            errors As List(Of BuildError))

            If String.IsNullOrWhiteSpace(output) Then
                Return
            End If

            Dim normalized As String = output.Replace(
                Microsoft.VisualBasic.vbCrLf,
                Microsoft.VisualBasic.vbLf)

            normalized = normalized.Replace(
                Microsoft.VisualBasic.vbCr,
                Microsoft.VisualBasic.vbLf)

            Dim lines() As String =
                normalized.Split(
                    New String() {
                        Microsoft.VisualBasic.vbLf
                    },
                    StringSplitOptions.None)

            For Each rawLine As String In lines

                If String.IsNullOrWhiteSpace(rawLine) Then
                    Continue For
                End If

                Dim line As String = rawLine.Trim()

                ' ---------------------------------------------------------
                ' Format:
                ' file.vb(10,5): error BC30001: message
                '
                ' file.cs(10,5): error CS1002: message
                ' ---------------------------------------------------------

                Dim match As Match =
                    Regex.Match(
                        line,
                        "^(.*)\((\d+),(\d+)\):\s*(error|warning)\s+([A-Za-z0-9]+)\s*:\s*(.*)$",
                        RegexOptions.IgnoreCase)

                If match.Success Then

                    Dim buildError As New BuildError()

                    buildError.FilePath = match.Groups(1).Value.Trim()
                    buildError.LineNumber =
                        Integer.Parse(match.Groups(2).Value)

                    buildError.ColumnNumber =
                        Integer.Parse(match.Groups(3).Value)

                    buildError.Code =
                        match.Groups(5).Value.Trim()

                    buildError.Message =
                        match.Groups(6).Value.Trim()

                    buildError.RawLine = line

                    errors.Add(buildError)

                    Continue For

                End If

                ' ---------------------------------------------------------
                ' Format:
                ' error BC30001: message
                ' warning CS0103: message
                ' ---------------------------------------------------------

                match =
                    Regex.Match(
                        line,
                        "^(error|warning)\s+([A-Za-z0-9]+)\s*:\s*(.*)$",
                        RegexOptions.IgnoreCase)

                If match.Success Then

                    Dim buildError As New BuildError()

                    buildError.FilePath = String.Empty
                    buildError.LineNumber = 0
                    buildError.ColumnNumber = 0

                    buildError.Code =
                        match.Groups(2).Value.Trim()

                    buildError.Message =
                        match.Groups(3).Value.Trim()

                    buildError.RawLine = line

                    errors.Add(buildError)

                End If

            Next

        End Sub

    End Class


    Public Class AgentBuildResult

        Public Sub New()

            Success = False
            ExitCode = -1

            Command = String.Empty
            Output = String.Empty
            ErrorOutput = String.Empty
            Message = String.Empty

            StartTime = DateTime.MinValue
            EndTime = DateTime.MinValue

            Errors = New List(Of BuildError)()

        End Sub

        Public Property Success As Boolean

        Public Property ExitCode As Integer

        Public Property Command As String

        Public Property Output As String

        Public Property ErrorOutput As String

        Public Property Message As String

        Public Property StartTime As DateTime

        Public Property EndTime As DateTime

        Public Property Errors As List(Of BuildError)

        Public ReadOnly Property Duration As TimeSpan

            Get

                If EndTime = DateTime.MinValue OrElse
                   StartTime = DateTime.MinValue Then

                    Return TimeSpan.Zero

                End If

                Return EndTime - StartTime

            End Get

        End Property

    End Class


    Public Class BuildError

        Public Sub New()

            FilePath = String.Empty
            LineNumber = 0
            ColumnNumber = 0
            Code = String.Empty
            Message = String.Empty
            RawLine = String.Empty

        End Sub

        Public Property FilePath As String

        Public Property LineNumber As Integer

        Public Property ColumnNumber As Integer

        Public Property Code As String

        Public Property Message As String

        Public Property RawLine As String

        Public Overrides Function ToString() As String

            If String.IsNullOrWhiteSpace(FilePath) Then

                If LineNumber > 0 Then
                    Return FilePath & "(" &
                           LineNumber.ToString() & "," &
                           ColumnNumber.ToString() & "): " &
                           Code & ": " & Message
                End If

                Return Code & ": " & Message

            End If

            Return FilePath & "(" &
                   LineNumber.ToString() & "," &
                   ColumnNumber.ToString() & "): " &
                   Code & ": " & Message

        End Function

    End Class

End Namespace