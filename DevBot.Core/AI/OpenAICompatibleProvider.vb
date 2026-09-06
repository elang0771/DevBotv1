Imports System
Imports System.IO
Imports System.Net
Imports System.Text
Imports DevBot.Core.AI

Namespace DevBot.Core.AI

    Public Class OpenAICompatibleProvider
        Implements IAIProvider

        Private ReadOnly settings As AIProviderSettings

        Public Sub New()
            Dim store As New AIProviderSettingsStore()
            settings = store.GetDefaultProvider()
            If settings Is Nothing Then
                settings = New AIProviderSettings()
            End If
        End Sub

        Public Sub New(providerSettings As AIProviderSettings)
            If providerSettings Is Nothing Then
                settings = New AIProviderSettings()
            Else
                settings = providerSettings.Clone()
            End If
        End Sub

        Public Function IsConfigured() As Boolean _
            Implements IAIProvider.IsConfigured

            Return Not String.IsNullOrWhiteSpace(settings.ApiKey) AndAlso
                   Not String.IsNullOrWhiteSpace(settings.BaseUrl) AndAlso
                   Not String.IsNullOrWhiteSpace(settings.Model)
        End Function

        Public Function GetProviderName() As String _
            Implements IAIProvider.GetProviderName

            Return If(String.IsNullOrWhiteSpace(settings.Provider),
                      "OpenAI Compatible",
                      settings.Provider)
        End Function

        Public Function TestConnection() As AIConnectionTestResult _
            Implements IAIProvider.TestConnection

            If Not IsConfigured() Then
                Return AIConnectionTestResult.Fail(
                    "Provider belum lengkap. Isi API Key, Base URL, dan Model.")
            End If

            Try
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12

                Dim started As DateTime = DateTime.Now
                Dim endpoint As String = BuildChatEndpoint(settings.BaseUrl)
                Dim json As String =
                    BuildRequestJson(
                        "You are a connection test endpoint.",
                        "Reply with exactly: OK")

                Dim request As HttpWebRequest =
                    DirectCast(WebRequest.Create(endpoint), HttpWebRequest)

                request.Method = "POST"
                request.ContentType = "application/json"
                request.Accept = "application/json"
                request.Timeout = Math.Max(5000, settings.TimeoutSeconds * 1000)
                request.ReadWriteTimeout = request.Timeout
                request.KeepAlive = False
                request.ProtocolVersion = HttpVersion.Version11
                request.Headers(HttpRequestHeader.Authorization) =
                    "Bearer " & settings.ApiKey

                WriteRequest(request, json)

                Dim responseText As String = ReadResponse(request)
                Dim elapsed As Integer =
                    CInt((DateTime.Now - started).TotalMilliseconds)

                If String.IsNullOrWhiteSpace(responseText) Then
                    Return AIConnectionTestResult.Fail(
                        "Server merespons tetapi body kosong.",
                        elapsed)
                End If

                If responseText.IndexOf(
                    """choices""",
                    StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   responseText.IndexOf(
                    """content""",
                    StringComparison.OrdinalIgnoreCase) >= 0 Then

                    Return AIConnectionTestResult.Ok(
                        "Request berhasil dan response AI diterima.",
                        elapsed)
                End If

                Return AIConnectionTestResult.Ok(
                    "HTTP request berhasil. Response: " &
                    LimitText(responseText, 1000),
                    elapsed)

            Catch ex As WebException
                Return AIConnectionTestResult.Fail(
                    BuildWebExceptionMessage(ex),
                    0)
            Catch ex As Exception
                Return AIConnectionTestResult.Fail(
                    ex.Message,
                    0)
            End Try
        End Function

        Public Function AnalyzeAndFix(
            systemPrompt As String,
            userPrompt As String
        ) As AIFixResponse _
            Implements IAIProvider.AnalyzeAndFix

            If Not IsConfigured() Then
                Return AIFixResponse.Fail(
                    "AI belum dikonfigurasi. Isi API Key, Base URL, dan Model.")
            End If

            Try
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12

                Dim endpoint As String =
                    BuildChatEndpoint(settings.BaseUrl)

                Dim json As String =
                    BuildRequestJson(systemPrompt, userPrompt)

                Dim request As HttpWebRequest =
                    DirectCast(WebRequest.Create(endpoint), HttpWebRequest)

                request.Method = "POST"
                request.ContentType = "application/json"
                request.Accept = "application/json"
                request.Timeout = Math.Max(5000, settings.TimeoutSeconds * 1000)
                request.ReadWriteTimeout = request.Timeout
                request.KeepAlive = False
                request.ProtocolVersion = HttpVersion.Version11
                request.Headers(HttpRequestHeader.Authorization) =
                    "Bearer " & settings.ApiKey

                WriteRequest(request, json)

                Dim responseText As String =
                    ReadResponse(request)

                If String.IsNullOrWhiteSpace(responseText) Then
                    Return AIFixResponse.Fail(
                        "AI mengembalikan response kosong.")
                End If

                Dim content As String =
                    ExtractAssistantContent(responseText)

                If String.IsNullOrWhiteSpace(content) Then
                    Return AIFixResponse.Fail(
                        "Response AI diterima tetapi content tidak ditemukan." &
                        Environment.NewLine &
                        "Raw response: " &
                        LimitText(responseText, 4000))
                End If

                Return ParseFixResponse(content, responseText)

            Catch ex As WebException
                Return AIFixResponse.Fail(
                    "AI request gagal: " &
                    BuildWebExceptionMessage(ex))
            Catch ex As Exception
                Return AIFixResponse.Fail(
                    "AI request gagal: " & ex.Message)
            End Try
        End Function

        Private Function BuildChatEndpoint(
            baseUrl As String) As String

            Dim url As String = baseUrl.Trim()

            If url.EndsWith("/", StringComparison.Ordinal) Then
                url = url.TrimEnd("/"c)
            End If

            If url.EndsWith(
                "/chat/completions",
                StringComparison.OrdinalIgnoreCase) Then
                Return url
            End If

            Return url & "/chat/completions"
        End Function

        Private Function BuildRequestJson(
            systemPrompt As String,
            userPrompt As String) As String

            Dim builder As New StringBuilder()

            builder.Append("{")
            builder.Append("""model"":")
            builder.Append(JsonString(settings.Model))
            builder.Append(",")
            builder.Append("""messages"":[")
            builder.Append("{""role"":""system"",""content"":")
            builder.Append(JsonString(systemPrompt))
            builder.Append("},")
            builder.Append("{""role"":""user"",""content"":")
            builder.Append(JsonString(userPrompt))
            builder.Append("}],")
            builder.Append("""temperature"":")
            builder.Append(
                settings.Temperature.ToString(
                    Globalization.CultureInfo.InvariantCulture))
            builder.Append("}")

            Return builder.ToString()
        End Function

        Private Function JsonString(value As String) As String

            If value Is Nothing Then
                value = String.Empty
            End If

            Dim builder As New StringBuilder()
            builder.Append(Convert.ToChar(34))

            For Each character As Char In value
                Select Case Microsoft.VisualBasic.AscW(character)
                    Case 8
                        builder.Append(Convert.ToChar(92))
                        builder.Append("b")
                    Case 9
                        builder.Append(Convert.ToChar(92))
                        builder.Append("t")
                    Case 10
                        builder.Append(Convert.ToChar(92))
                        builder.Append("n")
                    Case 12
                        builder.Append(Convert.ToChar(92))
                        builder.Append("f")
                    Case 13
                        builder.Append(Convert.ToChar(92))
                        builder.Append("r")
                    Case 34
                        builder.Append(Convert.ToChar(92))
                        builder.Append(Convert.ToChar(34))
                    Case 92
                        builder.Append(Convert.ToChar(92))
                        builder.Append(Convert.ToChar(92))
                    Case Else
                        If Microsoft.VisualBasic.AscW(character) < 32 Then
                            builder.Append("\u")
                            builder.Append(Microsoft.VisualBasic.AscW(character).ToString("x4", Globalization.CultureInfo.InvariantCulture))
                        Else
                            builder.Append(character)
                        End If
                End Select
            Next

            builder.Append(Convert.ToChar(34))
            Return builder.ToString()

        End Function

        Private Sub WriteRequest(
            request As HttpWebRequest,
            json As String)

            Dim bytes() As Byte =
                Encoding.UTF8.GetBytes(json)

            request.ContentLength = bytes.Length

            Using stream As Stream =
                request.GetRequestStream()

                stream.Write(bytes, 0, bytes.Length)
            End Using
        End Sub

        Private Function ReadResponse(
            request As HttpWebRequest) As String

            Using response As HttpWebResponse =
                DirectCast(request.GetResponse(), HttpWebResponse)

                Using stream As Stream =
                    response.GetResponseStream()

                    Using reader As New StreamReader(
                        stream,
                        Encoding.UTF8)

                        Return reader.ReadToEnd()
                    End Using
                End Using
            End Using
        End Function

        Private Function ExtractAssistantContent(
            json As String) As String

            If String.IsNullOrWhiteSpace(json) Then
                Return String.Empty
            End If

            Dim marker As String = """content"":"
            Dim startIndex As Integer =
                json.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase)

            If startIndex < 0 Then
                Return String.Empty
            End If

            startIndex += marker.Length

            While startIndex < json.Length AndAlso
                  Char.IsWhiteSpace(json(startIndex))
                startIndex += 1
            End While

            If startIndex >= json.Length OrElse
               json(startIndex) <> """"c Then
                Return String.Empty
            End If

            startIndex += 1

            Dim builder As New StringBuilder()
            Dim escaped As Boolean = False

            For index As Integer =
                startIndex To json.Length - 1

                Dim character As Char = json(index)

                If escaped Then
                    Select Case character
                        Case """"c
                            builder.Append(""""c)
                        Case "\"c
                            builder.Append("\"c)
                        Case "/"c
                            builder.Append("/"c)
                        Case "n"c
                            builder.Append(Microsoft.VisualBasic.vbLf)
                        Case "r"c
                            builder.Append(Microsoft.VisualBasic.vbCr)
                        Case "t"c
                            builder.Append(Microsoft.VisualBasic.vbTab)
                        Case "b"c
                            builder.Append(Microsoft.VisualBasic.ChrW(8))
                        Case "f"c
                            builder.Append(Microsoft.VisualBasic.ChrW(12))
                        Case "u"c
                            If index + 4 < json.Length Then
                                Dim hexValue As String =
                                    json.Substring(index + 1, 4)
                                Dim number As Integer
                                If Integer.TryParse(
                                    hexValue,
                                    Globalization.NumberStyles.HexNumber,
                                    Globalization.CultureInfo.InvariantCulture,
                                    number) Then
                                    builder.Append(Microsoft.VisualBasic.ChrW(number))
                                    index += 4
                                End If
                            End If
                    End Select

                    escaped = False

                ElseIf character = "\"c Then
                    escaped = True

                ElseIf character = """"c Then
                    Exit For

                Else
                    builder.Append(character)
                End If
            Next

            Return builder.ToString().Trim()
        End Function

        Private Function ParseFixResponse(
            content As String,
            rawResponse As String) As AIFixResponse

            Dim normalized As String =
                content.Replace(Microsoft.VisualBasic.vbCrLf, Microsoft.VisualBasic.vbLf).
                        Replace(Microsoft.VisualBasic.vbCr, Microsoft.VisualBasic.vbLf)

            Dim lines() As String =
                normalized.Split(
                    New Char() {Microsoft.VisualBasic.ChrW(10)})

            Dim filePath As String = String.Empty
            Dim oldText As New StringBuilder()
            Dim newText As New StringBuilder()
            Dim explanation As String = String.Empty
            Dim mode As String = String.Empty
            Dim foundOld As Boolean = False
            Dim foundNew As Boolean = False

            For Each rawLine As String In lines
                Dim line As String = rawLine

                If line.StartsWith(
                    "FILE=",
                    StringComparison.OrdinalIgnoreCase) Then

                    filePath = line.Substring(5).Trim()
                    Continue For
                End If

                If line.Equals(
                    "OLD_BEGIN",
                    StringComparison.OrdinalIgnoreCase) Then

                    mode = "OLD"
                    foundOld = True
                    Continue For
                End If

                If line.Equals(
                    "OLD_END",
                    StringComparison.OrdinalIgnoreCase) Then

                    mode = String.Empty
                    Continue For
                End If

                If line.Equals(
                    "NEW_BEGIN",
                    StringComparison.OrdinalIgnoreCase) Then

                    mode = "NEW"
                    foundNew = True
                    Continue For
                End If

                If line.Equals(
                    "NEW_END",
                    StringComparison.OrdinalIgnoreCase) Then

                    mode = String.Empty
                    Continue For
                End If

                If line.StartsWith(
                    "EXPLANATION=",
                    StringComparison.OrdinalIgnoreCase) Then

                    explanation = line.Substring(12).Trim()
                    Continue For
                End If

                If mode = "OLD" Then
                    oldText.AppendLine(line)
                ElseIf mode = "NEW" Then
                    newText.AppendLine(line)
                End If
            Next

            If String.IsNullOrWhiteSpace(filePath) Then
                Return AIFixResponse.Fail(
                    "AI tidak mengembalikan FILE=.")
            End If

            If Not foundOld Then
                Return AIFixResponse.Fail(
                    "AI tidak mengembalikan OLD_BEGIN.")
            End If

            If Not foundNew Then
                Return AIFixResponse.Fail(
                    "AI tidak mengembalikan NEW_BEGIN.")
            End If

            Dim oldValue As String =
                RemoveFinalNewLine(oldText.ToString())

            Dim newValue As String =
                RemoveFinalNewLine(newText.ToString())

            If String.IsNullOrWhiteSpace(oldValue) Then
                Return AIFixResponse.Fail(
                    "OLD_TEXT dari AI kosong.")
            End If

            If String.IsNullOrWhiteSpace(newValue) Then
                Return AIFixResponse.Fail(
                    "NEW_TEXT dari AI kosong.")
            End If

            Return AIFixResponse.Fixed(
                filePath,
                oldValue,
                newValue,
                explanation,
                rawResponse)
        End Function

        Private Function RemoveFinalNewLine(
            value As String) As String

            If value Is Nothing Then
                Return String.Empty
            End If

            Return value.TrimEnd(
                Microsoft.VisualBasic.vbCr(0),
                Microsoft.VisualBasic.vbLf(0))
        End Function

        Private Function BuildWebExceptionMessage(
            ex As WebException) As String

            Dim detail As String =
                If(ex Is Nothing,
                   "Unknown network error.",
                   ex.Message)

            If ex Is Nothing OrElse
               ex.Response Is Nothing Then
                Return detail
            End If

            Try
                Using response As HttpWebResponse =
                    DirectCast(ex.Response, HttpWebResponse)

                    Using stream As Stream =
                        response.GetResponseStream()

                        Using reader As New StreamReader(
                            stream,
                            Encoding.UTF8)

                            Dim body As String =
                                reader.ReadToEnd()

                            If Not String.IsNullOrWhiteSpace(body) Then
                                detail =
                                    detail &
                                    Environment.NewLine &
                                    "HTTP " &
                                    CInt(response.StatusCode).ToString() &
                                    " " &
                                    response.StatusDescription &
                                    Environment.NewLine &
                                    "Server response: " &
                                    LimitText(body, 4000)
                            End If
                        End Using
                    End Using
                End Using
            Catch
            End Try

            Return detail
        End Function

        Private Function LimitText(
            value As String,
            maximumLength As Integer) As String

            If String.IsNullOrEmpty(value) Then
                Return String.Empty
            End If

            If value.Length <= maximumLength Then
                Return value
            End If

            Return value.Substring(0, maximumLength) &
                   Environment.NewLine &
                   "... [dipotong]"
        End Function

    End Class

    Public Class AIConnectionTestResult

        Public Property Success As Boolean
        Public Property Message As String
        Public Property ElapsedMilliseconds As Integer

        Public Shared Function Ok(
            message As String,
            elapsedMilliseconds As Integer) As AIConnectionTestResult

            Return New AIConnectionTestResult With {
                .Success = True,
                .Message = message,
                .ElapsedMilliseconds = elapsedMilliseconds
            }
        End Function

        Public Shared Function Fail(
            message As String,
            Optional elapsedMilliseconds As Integer = 0) As AIConnectionTestResult

            Return New AIConnectionTestResult With {
                .Success = False,
                .Message = message,
                .ElapsedMilliseconds = elapsedMilliseconds
            }
        End Function

    End Class

End Namespace
