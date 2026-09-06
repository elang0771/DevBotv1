Imports System
Imports System.Collections.Generic
Imports System.Text

Namespace DevBot.Core.Code

    Public Class DiffLine

        Public Property LineNumber As Integer
        Public Property Text As String
        Public Property ChangeType As String

        Public Sub New()

            LineNumber = 0
            Text = String.Empty
            ChangeType = "UNCHANGED"

        End Sub

        Public Sub New(
            lineNumber As Integer,
            text As String,
            changeType As String)

            Me.LineNumber = lineNumber
            Me.Text = text
            Me.ChangeType = changeType

        End Sub

    End Class


    Public Class DiffResult

        Public Property OriginalText As String
        Public Property ModifiedText As String

        Public Property Lines As List(Of DiffLine)

        Public Property AddedLines As Integer
        Public Property RemovedLines As Integer
        Public Property UnchangedLines As Integer

        Public ReadOnly Property HasChanges As Boolean

            Get

                Return AddedLines > 0 OrElse
                       RemovedLines > 0

            End Get

        End Property

        Public Sub New()

            OriginalText = String.Empty
            ModifiedText = String.Empty

            Lines =
                New List(Of DiffLine)()

            AddedLines = 0
            RemovedLines = 0
            UnchangedLines = 0

        End Sub

    End Class


    Public Class DiffService

        Public Function Compare(
            originalText As String,
            modifiedText As String) As DiffResult

            If originalText Is Nothing Then
                originalText = String.Empty
            End If

            If modifiedText Is Nothing Then
                modifiedText = String.Empty
            End If

            Dim result As New DiffResult()

            result.OriginalText =
                originalText

            result.ModifiedText =
                modifiedText

            Dim originalLines() As String =
                SplitLines(originalText)

            Dim modifiedLines() As String =
                SplitLines(modifiedText)

            Dim matrix(,) As Integer =
                BuildLcsMatrix(
                    originalLines,
                    modifiedLines)

            Dim output As New List(Of DiffLine)()

            Dim originalIndex As Integer = 0
            Dim modifiedIndex As Integer = 0
            Dim displayLineNumber As Integer = 1

            While originalIndex <
                  originalLines.Length OrElse
                  modifiedIndex <
                  modifiedLines.Length

                If originalIndex <
                   originalLines.Length AndAlso
                   modifiedIndex <
                   modifiedLines.Length AndAlso
                   String.Equals(
                       originalLines(originalIndex),
                       modifiedLines(modifiedIndex),
                       StringComparison.Ordinal) Then

                    output.Add(
                        New DiffLine(
                            displayLineNumber,
                            "  " &
                            originalLines(originalIndex),
                            "UNCHANGED"))

                    result.UnchangedLines += 1

                    originalIndex += 1
                    modifiedIndex += 1
                    displayLineNumber += 1

                ElseIf modifiedIndex <
                       modifiedLines.Length AndAlso
                       (
                           originalIndex >=
                           originalLines.Length OrElse
                           matrix(
                               originalIndex,
                               modifiedIndex + 1) >=
                           matrix(
                               originalIndex + 1,
                               modifiedIndex)
                       ) Then

                    output.Add(
                        New DiffLine(
                            displayLineNumber,
                            "+ " &
                            modifiedLines(modifiedIndex),
                            "ADDED"))

                    result.AddedLines += 1

                    modifiedIndex += 1
                    displayLineNumber += 1

                ElseIf originalIndex <
                       originalLines.Length Then

                    output.Add(
                        New DiffLine(
                            displayLineNumber,
                            "- " &
                            originalLines(originalIndex),
                            "REMOVED"))

                    result.RemovedLines += 1

                    originalIndex += 1
                    displayLineNumber += 1

                End If

            End While

            result.Lines =
                output

            Return result

        End Function


        Public Function CompareFiles(
            originalFilePath As String,
            modifiedFilePath As String) As DiffResult

            If String.IsNullOrWhiteSpace(
                originalFilePath) Then

                Throw New ArgumentException(
                    "Path file asli tidak boleh kosong.",
                    "originalFilePath")

            End If

            If String.IsNullOrWhiteSpace(
                modifiedFilePath) Then

                Throw New ArgumentException(
                    "Path file baru tidak boleh kosong.",
                    "modifiedFilePath")

            End If

            If Not System.IO.File.Exists(
                originalFilePath) Then

                Throw New System.IO.FileNotFoundException(
                    "File asli tidak ditemukan.",
                    originalFilePath)

            End If

            If Not System.IO.File.Exists(
                modifiedFilePath) Then

                Throw New System.IO.FileNotFoundException(
                    "File baru tidak ditemukan.",
                    modifiedFilePath)

            End If

            Dim originalText As String =
                System.IO.File.ReadAllText(
                    originalFilePath,
                    Encoding.UTF8)

            Dim modifiedText As String =
                System.IO.File.ReadAllText(
                    modifiedFilePath,
                    Encoding.UTF8)

            Return Compare(
                originalText,
                modifiedText)

        End Function


        Public Function FormatForDisplay(
            result As DiffResult) As String

            If result Is Nothing Then

                Return String.Empty

            End If

            Dim builder As New StringBuilder()

            builder.AppendLine(
                "========== DIFF ==========")

            builder.AppendLine()

            builder.AppendLine(
                "Added   : " &
                result.AddedLines.ToString())

            builder.AppendLine(
                "Removed : " &
                result.RemovedLines.ToString())

            builder.AppendLine(
                "Same    : " &
                result.UnchangedLines.ToString())

            builder.AppendLine()

            If Not result.HasChanges Then

                builder.AppendLine(
                    "Tidak ada perubahan.")

                Return builder.ToString()

            End If

            builder.AppendLine(
                "---------------------------")

            For Each line As DiffLine In
                result.Lines

                builder.AppendLine(
                    line.Text)

            Next

            builder.AppendLine(
                "===========================")

            Return builder.ToString()

        End Function


        Private Function SplitLines(
    text As String) As String()

            If String.IsNullOrEmpty(text) Then

                Return New String() {}

            End If

            Dim normalized As String =
        text.Replace(
            System.Convert.ToChar(13).ToString(),
            String.Empty)

            normalized =
        normalized.Replace(
            System.Convert.ToChar(10).ToString(),
            Environment.NewLine)

            Return normalized.Split(
        New String() {
            Environment.NewLine
        },
        StringSplitOptions.None)

        End Function

        Private Function BuildLcsMatrix(
            originalLines() As String,
            modifiedLines() As String) As Integer(,)

            Dim originalCount As Integer =
                originalLines.Length

            Dim modifiedCount As Integer =
                modifiedLines.Length

            Dim matrix(
                originalCount + 1,
                modifiedCount + 1) As Integer

            Dim i As Integer
            Dim j As Integer

            For i =
                originalCount - 1 To 0 Step -1

                For j =
                    modifiedCount - 1 To 0 Step -1

                    If String.Equals(
                        originalLines(i),
                        modifiedLines(j),
                        StringComparison.Ordinal) Then

                        matrix(i, j) =
                            matrix(i + 1, j + 1) + 1

                    Else

                        matrix(i, j) =
                            Math.Max(
                                matrix(i + 1, j),
                                matrix(i, j + 1))

                    End If

                Next

            Next

            Return matrix

        End Function

    End Class

End Namespace