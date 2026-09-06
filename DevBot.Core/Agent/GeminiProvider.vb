Imports System
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Globalization
Imports DevBot.Core.AI

Namespace DevBot.Core.AI

    Public Class GeminiProvider
        Implements IAIProvider

        Private ReadOnly apiKey As String
        Private ReadOnly model As String
        Private ReadOnly baseUrl As String
        Private ReadOnly temperature As Double
        Private ReadOnly maxOutput As Integer
        Private ReadOnly timeoutMilliseconds As Integer

        ' ============================================================
        ' CONSTRUCTOR
        ' ============================================================

        Public Sub New(
            providerSettings As AIProviderSettings)

            If providerSettings Is Nothing Then
                Throw New ArgumentNullException(
                    NameOf(providerSettings))
            End If

            apiKey =
                If(
                    providerSettings.ApiKey,
                    String.Empty).Trim()

            model =
                If(
                    providerSettings.Model,
                    String.Empty).Trim()

            baseUrl =
                If(
                    providerSettings.BaseUrl,
                    String.Empty).Trim()

            temperature =
                NormalizeTemperature(
                    providerSettings.Temperature)

            maxOutput =
                NormalizeMaxOutput(
                    providerSettings.MaxOutput)

            timeoutMilliseconds =
                NormalizeTimeout(
                    providerSettings.TimeoutSeconds)

        End Sub

        ' ============================================================
        ' CONFIGURATION
        ' ============================================================

        Public Function IsConfigured() As Boolean _
            Implements IAIProvider.IsConfigured

            If String.IsNullOrWhiteSpace(apiKey) Then
                Return False
            End If

            If String.IsNullOrWhiteSpace(model) Then
                Return False
            End If

            If String.IsNullOrWhiteSpace(baseUrl) Then
                Return False
            End If

            Return True

        End Function

        ' ============================================================
        ' PROVIDER NAME
        ' ============================================================

        Public Function GetProviderName() As String _
            Implements IAIProvider.GetProviderName

            If String.IsNullOrWhiteSpace(model) Then
                Return "Google Gemini"
            End If

            Return "Google Gemini - " & model

        End Function

        ' ============================================================
        ' TEST CONNECTION
        ' ============================================================

        Public Function TestConnection() As AIConnectionTestResult _
            Implements IAIProvider.TestConnection

            If String.IsNullOrWhiteSpace(apiKey) Then

                Return AIConnectionTestResult.Fail(
                    "Gemini API Key belum diisi pada AI Provider Settings.")

            End If

            If String.IsNullOrWhiteSpace(model) Then

                Return AIConnectionTestResult.Fail(
                    "Gemini Model belum diisi pada AI Provider Settings.")

            End If

            If String.IsNullOrWhiteSpace(baseUrl) Then

                Return AIConnectionTestResult.Fail(
                    "Gemini Base URL belum diisi pada AI Provider Settings.")

            End If

            Try

                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12

                Dim started As DateTime =
                    DateTime.Now

                Dim url As String =
                    BuildGenerateContentUrl()

                Dim requestBody As String =
                    BuildRequestJson(
                        "Reply with exactly: OK")

                Dim responseText As String =
                    SendPostRequest(
                        url,
                        requestBody)

                Dim elapsed As Integer =
                    CInt(
                        (DateTime.Now - started).
                        TotalMilliseconds)

                If String.IsNullOrWhiteSpace(
                    responseText) Then

                    Return AIConnectionTestResult.Fail(
                        BuildDiagnosticMessage(
                            "Gemini merespons tetapi body kosong.",
                            url),
                        elapsed)

                End If

                Dim aiText As String =
                    ExtractFirstText(
                        responseText)

                If Not String.IsNullOrWhiteSpace(
                    aiText) Then

                    Return AIConnectionTestResult.Ok(
                        "Gemini connection OK. Model merespons.",
                        elapsed)

                End If

                Return AIConnectionTestResult.Ok(
                    "HTTP request ke Gemini berhasil. Response diterima.",
                    elapsed)

            Catch ex As WebException

                Return AIConnectionTestResult.Fail(
                    BuildWebExceptionMessage(
                        ex,
                        BuildGenerateContentUrl()),
                    0)

            Catch ex As Exception

                Return AIConnectionTestResult.Fail(
                    BuildDiagnosticMessage(
                        "Gemini connection gagal: " &
                        ex.Message,
                        BuildGenerateContentUrl()),
                    0)

            End Try

        End Function

        ' ============================================================
        ' ANALYZE AND FIX
        ' ============================================================

        Public Function AnalyzeAndFix(
            systemPrompt As String,
            userPrompt As String
        ) As AIFixResponse _
            Implements IAIProvider.AnalyzeAndFix

            If Not IsConfigured() Then

                Return AIFixResponse.Fail(
                    "Gemini belum dikonfigurasi lengkap. " &
                    "Periksa API Key, Model, dan Base URL " &
                    "pada AI Provider Settings.")

            End If

            Dim url As String = String.Empty

            Try

                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12

                url =
                    BuildGenerateContentUrl()

                Dim requestBody As String =
                    BuildAnalyzeRequestJson(
                        systemPrompt,
                        userPrompt)

                Dim responseText As String =
                    SendPostRequest(
                        url,
                        requestBody)

                If String.IsNullOrWhiteSpace(
                    responseText) Then

                    Return AIFixResponse.Fail(
                        BuildDiagnosticMessage(
                            "Gemini merespons tetapi body kosong.",
                            url))

                End If

                Dim aiText As String =
                    ExtractFirstText(
                        responseText)

                If String.IsNullOrWhiteSpace(
                    aiText) Then

                    Return AIFixResponse.Fail(
                        BuildDiagnosticMessage(
                            "Gemini tidak mengembalikan text response.",
                            url))

                End If

                Return ParseFixResponse(
                    aiText,
                    responseText)

            Catch ex As WebException

                Return AIFixResponse.Fail(
                    BuildWebExceptionMessage(
                        ex,
                        url))

            Catch ex As Exception

                Return AIFixResponse.Fail(
                    BuildDiagnosticMessage(
                        "Gemini request gagal: " &
                        ex.Message,
                        url))

            End Try

        End Function

        ' ============================================================
        ' BUILD GEMINI ENDPOINT
        '
        ' API KEY SENGAJA TIDAK DIMASUKKAN KE URL.
        ' API KEY dikirim melalui HTTP header:
        '
        ' x-goog-api-key
        ' ============================================================

        Private Function BuildGenerateContentUrl() As String

            Dim cleanBaseUrl As String =
                baseUrl.Trim()

            cleanBaseUrl =
                cleanBaseUrl.TrimEnd("/"c)

            Dim cleanModel As String =
                model.Trim()

            ' Jika Base URL sudah mengandung /models/
            ' jangan menambahkan /models/ lagi.
            '
            ' Contoh:
            '
            ' https://generativelanguage.googleapis.com/v1beta/models
            '
            ' menjadi:
            '
            ' https://generativelanguage.googleapis.com/v1beta/models/model:generateContent

            If cleanBaseUrl.EndsWith(
                "/models",
                StringComparison.OrdinalIgnoreCase) Then

                Return cleanBaseUrl &
                       "/" &
                       Uri.EscapeDataString(
                           cleanModel) &
                       ":generateContent"

            End If

            ' Jika Base URL sudah menunjuk ke model tertentu,
            ' jangan menggandakan model.

            If cleanBaseUrl.IndexOf(
                "/models/",
                StringComparison.OrdinalIgnoreCase) >= 0 Then

                If cleanBaseUrl.EndsWith(
                    cleanModel,
                    StringComparison.OrdinalIgnoreCase) Then

                    Return cleanBaseUrl &
                           ":generateContent"

                End If

            End If

            Return cleanBaseUrl &
                   "/" &
                   Uri.EscapeDataString(
                       cleanModel) &
                   ":generateContent"

        End Function

        ' ============================================================
        ' SEND POST REQUEST
        ' ============================================================

        Private Function SendPostRequest(
            url As String,
            requestBody As String
        ) As String

            Dim request As HttpWebRequest =
                DirectCast(
                    WebRequest.Create(url),
                    HttpWebRequest)

            request.Method = "POST"

            request.ContentType =
                "application/json; charset=utf-8"

            request.Accept =
                "application/json"

            request.Timeout =
                timeoutMilliseconds

            request.ReadWriteTimeout =
                timeoutMilliseconds

            request.KeepAlive = False

            ' ========================================================
            ' GEMINI API KEY
            '
            ' JANGAN PERNAH MASUKKAN API KEY KE URL.
            ' ========================================================

            request.Headers("x-goog-api-key") =
                apiKey

            Dim bodyBytes() As Byte =
                Encoding.UTF8.GetBytes(
                    requestBody)

            request.ContentLength =
                bodyBytes.Length

            Using requestStream As Stream =
                request.GetRequestStream()

                requestStream.Write(
                    bodyBytes,
                    0,
                    bodyBytes.Length)

            End Using

            Using response As HttpWebResponse =
                DirectCast(
                    request.GetResponse(),
                    HttpWebResponse)

                Using responseStream As Stream =
                    response.GetResponseStream()

                    If responseStream Is Nothing Then
                        Return String.Empty
                    End If

                    Using reader As New StreamReader(
                        responseStream,
                        Encoding.UTF8)

                        Return reader.ReadToEnd()

                    End Using

                End Using

            End Using

        End Function

        ' ============================================================
        ' TEST REQUEST JSON
        ' ============================================================

        Private Function BuildRequestJson(
            text As String
        ) As String

            Dim builder As New StringBuilder()

            builder.Append("{")

            builder.Append(
                """contents"":[{""parts"":[{""text"":")

            builder.Append(
                JsonQuote(text))

            builder.Append(
                "}]}],")

            builder.Append(
                """generationConfig"":")

            builder.Append("{")

            builder.Append(
                """temperature"":")

            builder.Append(
                temperature.ToString(
                    CultureInfo.InvariantCulture))

            builder.Append(",")

            builder.Append(
                """maxOutputTokens"":")

            builder.Append(
                maxOutput.ToString(
                    CultureInfo.InvariantCulture))

            builder.Append("}")

            builder.Append("}")

            Return builder.ToString()

        End Function

        ' ============================================================
        ' ANALYZE REQUEST JSON
        ' ============================================================

        Private Function BuildAnalyzeRequestJson(
            systemPrompt As String,
            userPrompt As String
        ) As String

            Dim builder As New StringBuilder()

            builder.Append("{")

            builder.Append(
                """systemInstruction"":")

            builder.Append(
                "{""parts"":[{""text"":")

            builder.Append(
                JsonQuote(systemPrompt))

            builder.Append(
                "}]},")

            builder.Append(
                """contents"":[{""role"":""user"",""parts"":[{""text"":")

            builder.Append(
                JsonQuote(userPrompt))

            builder.Append(
                "}]}],")

            builder.Append(
                """generationConfig"":")

            builder.Append("{")

            builder.Append(
                """temperature"":")

            builder.Append(
                temperature.ToString(
                    CultureInfo.InvariantCulture))

            builder.Append(",")

            builder.Append(
                """maxOutputTokens"":")

            builder.Append(
                maxOutput.ToString(
                    CultureInfo.InvariantCulture))

            builder.Append("}")

            builder.Append("}")

            Return builder.ToString()

        End Function

        ' ============================================================
        ' JSON QUOTE
        ' ============================================================

        Private Function JsonQuote(
            value As String
        ) As String

            If value Is Nothing Then
                value = String.Empty
            End If

            Dim builder As New StringBuilder()

            builder.Append(
                Microsoft.VisualBasic.ChrW(34))

            For Each ch As Char In value

                Select Case ch

                    Case Microsoft.VisualBasic.ChrW(34)

                        builder.Append(
                            Microsoft.VisualBasic.ChrW(92) &
                            Microsoft.VisualBasic.ChrW(34))

                    Case Microsoft.VisualBasic.ChrW(92)

                        builder.Append(
                            Microsoft.VisualBasic.ChrW(92) &
                            Microsoft.VisualBasic.ChrW(92))

                    Case Microsoft.VisualBasic.ChrW(8)

                        builder.Append(
                            Microsoft.VisualBasic.ChrW(92) &
                            "b")

                    Case Microsoft.VisualBasic.ChrW(12)

                        builder.Append(
                            Microsoft.VisualBasic.ChrW(92) &
                            "f")

                    Case Microsoft.VisualBasic.ChrW(10)

                        builder.Append(
                            Microsoft.VisualBasic.ChrW(92) &
                            "n")

                    Case Microsoft.VisualBasic.ChrW(13)

                        builder.Append(
                            Microsoft.VisualBasic.ChrW(92) &
                            "r")

                    Case Microsoft.VisualBasic.ChrW(9)

                        builder.Append(
                            Microsoft.VisualBasic.ChrW(92) &
                            "t")

                    Case Else

                        If Microsoft.VisualBasic.AscW(ch) < 32 Then

                            builder.Append(
                                "\u" &
                                Microsoft.VisualBasic.AscW(ch).
                                ToString("x4"))

                        Else

                            builder.Append(ch)

                        End If

                End Select

            Next

            builder.Append(
                Microsoft.VisualBasic.ChrW(34))

            Return builder.ToString()

        End Function

        ' ============================================================
        ' EXTRACT FIRST TEXT FROM GEMINI RESPONSE
        ' ============================================================

        Private Function ExtractFirstText(
            json As String
        ) As String

            If String.IsNullOrWhiteSpace(json) Then
                Return String.Empty
            End If

            Dim searchStart As Integer = 0

            While searchStart < json.Length

                Dim textNameIndex As Integer =
                    json.IndexOf(
                        Microsoft.VisualBasic.ChrW(34) &
                        "text" &
                        Microsoft.VisualBasic.ChrW(34),
                        searchStart,
                        StringComparison.OrdinalIgnoreCase)

                If textNameIndex < 0 Then
                    Exit While
                End If

                Dim colonIndex As Integer =
                    json.IndexOf(
                        Microsoft.VisualBasic.ChrW(58),
                        textNameIndex + 6)

                If colonIndex < 0 Then
                    Exit While
                End If

                Dim valueStart As Integer =
                    colonIndex + 1

                While valueStart < json.Length AndAlso
                      Char.IsWhiteSpace(
                          json(valueStart))

                    valueStart += 1

                End While

                If valueStart < json.Length AndAlso
                   json(valueStart) =
                   Microsoft.VisualBasic.ChrW(34) Then

                    valueStart += 1

                    Dim builder As New StringBuilder()

                    Dim escaped As Boolean = False

                    Dim index As Integer =
                        valueStart

                    While index < json.Length

                        Dim ch As Char =
                            json(index)

                        If escaped Then

                            Select Case ch

                                Case Microsoft.VisualBasic.ChrW(34)

                                    builder.Append(
                                        Microsoft.VisualBasic.ChrW(34))

                                Case Microsoft.VisualBasic.ChrW(92)

                                    builder.Append(
                                        Microsoft.VisualBasic.ChrW(92))

                                Case Microsoft.VisualBasic.ChrW(110)

                                    builder.Append(
                                        Microsoft.VisualBasic.ChrW(10))

                                Case Microsoft.VisualBasic.ChrW(114)

                                    builder.Append(
                                        Microsoft.VisualBasic.ChrW(13))

                                Case Microsoft.VisualBasic.ChrW(116)

                                    builder.Append(
                                        Microsoft.VisualBasic.ChrW(9))

                                Case Microsoft.VisualBasic.ChrW(98)

                                    builder.Append(
                                        Microsoft.VisualBasic.ChrW(8))

                                Case Microsoft.VisualBasic.ChrW(102)

                                    builder.Append(
                                        Microsoft.VisualBasic.ChrW(12))

                                Case Microsoft.VisualBasic.ChrW(117)

                                    If index + 4 < json.Length Then

                                        Dim hex As String =
                                            json.Substring(
                                                index + 1,
                                                4)

                                        Dim number As Integer

                                        If Integer.TryParse(
                                            hex,
                                            NumberStyles.HexNumber,
                                            CultureInfo.InvariantCulture,
                                            number) Then

                                            builder.Append(
                                                Microsoft.VisualBasic.ChrW(
                                                    number))

                                            index += 4

                                        Else

                                            builder.Append(ch)

                                        End If

                                    Else

                                        builder.Append(ch)

                                    End If

                                Case Else

                                    builder.Append(ch)

                            End Select

                            escaped = False

                        ElseIf ch =
                               Microsoft.VisualBasic.ChrW(92) Then

                            escaped = True

                        ElseIf ch =
                               Microsoft.VisualBasic.ChrW(34) Then

                            Dim result As String =
                                builder.ToString()

                            If Not String.IsNullOrWhiteSpace(
                                result) Then

                                Return result

                            End If

                            Exit While

                        Else

                            builder.Append(ch)

                        End If

                        index += 1

                    End While

                End If

                searchStart =
                    textNameIndex + 6

            End While

            Return String.Empty

        End Function

        ' ============================================================
        ' PARSE AI FIX RESPONSE
        ' ============================================================

        Private Function ParseFixResponse(
            aiText As String,
            rawResponse As String
        ) As AIFixResponse

            Dim filePath As String =
                ExtractLineValue(
                    aiText,
                    "FILE=")

            Dim oldText As String =
                ExtractBlock(
                    aiText,
                    "OLD_BEGIN",
                    "OLD_END")

            Dim newText As String =
                ExtractBlock(
                    aiText,
                    "NEW_BEGIN",
                    "NEW_END")

            Dim explanation As String =
                ExtractLineValue(
                    aiText,
                    "EXPLANATION=")

            If String.IsNullOrWhiteSpace(
                filePath) Then

                Return AIFixResponse.Fail(
                    "Gemini response tidak memiliki FILE=.")

            End If

            If String.IsNullOrWhiteSpace(
                oldText) Then

                Return AIFixResponse.Fail(
                    "Gemini response tidak memiliki OLD_BEGIN/OLD_END.")

            End If

            If String.IsNullOrWhiteSpace(
                newText) Then

                Return AIFixResponse.Fail(
                    "Gemini response tidak memiliki NEW_BEGIN/NEW_END.")

            End If

            Return AIFixResponse.Fixed(
                filePath.Trim(),
                oldText,
                newText,
                explanation,
                rawResponse)

        End Function

        ' ============================================================
        ' EXTRACT LINE VALUE
        ' ============================================================

        Private Function ExtractLineValue(
            text As String,
            prefix As String
        ) As String

            If String.IsNullOrWhiteSpace(text) Then
                Return String.Empty
            End If

            Dim lines() As String =
                text.Replace(
                    Microsoft.VisualBasic.vbCrLf,
                    Microsoft.VisualBasic.vbLf).
                Replace(
                    Microsoft.VisualBasic.vbCr,
                    Microsoft.VisualBasic.vbLf).
                Split(
                    New Char() {
                        Microsoft.VisualBasic.ChrW(10)
                    })

            For Each line As String In lines

                If line.TrimStart().StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase) Then

                    Return line.TrimStart().
                        Substring(prefix.Length).
                        Trim()

                End If

            Next

            Return String.Empty

        End Function

        ' ============================================================
        ' EXTRACT BLOCK
        ' ============================================================

        Private Function ExtractBlock(
            text As String,
            beginMarker As String,
            endMarker As String
        ) As String

            If String.IsNullOrWhiteSpace(text) Then
                Return String.Empty
            End If

            Dim beginIndex As Integer =
                text.IndexOf(
                    beginMarker,
                    StringComparison.OrdinalIgnoreCase)

            If beginIndex < 0 Then
                Return String.Empty
            End If

            beginIndex +=
                beginMarker.Length

            If beginIndex < text.Length AndAlso
               text(beginIndex) =
               Microsoft.VisualBasic.ChrW(13) Then

                beginIndex += 1

            End If

            If beginIndex < text.Length AndAlso
               text(beginIndex) =
               Microsoft.VisualBasic.ChrW(10) Then

                beginIndex += 1

            End If

            Dim endIndex As Integer =
                text.IndexOf(
                    endMarker,
                    beginIndex,
                    StringComparison.OrdinalIgnoreCase)

            If endIndex < 0 Then
                Return String.Empty
            End If

            Return text.Substring(
                beginIndex,
                endIndex - beginIndex).
                TrimEnd(
                    Microsoft.VisualBasic.ChrW(13),
                    Microsoft.VisualBasic.ChrW(10))

        End Function

        ' ============================================================
        ' WEB EXCEPTION
        '
        ' INI BAGIAN PENTING UNTUK DEBUG 404.
        ' ============================================================

        Private Function BuildWebExceptionMessage(
            ex As WebException,
            requestUrl As String
        ) As String

            If ex Is Nothing Then

                Return BuildDiagnosticMessage(
                    "Gemini request gagal.",
                    requestUrl)

            End If

            Dim statusCode As String =
                String.Empty

            Dim statusDescription As String =
                String.Empty

            Dim responseBody As String =
                String.Empty

            ' --------------------------------------------------------
            ' HTTP STATUS
            ' --------------------------------------------------------

            Try

                If ex.Response IsNot Nothing Then

                    Dim httpResponse As HttpWebResponse =
                        TryCast(
                            ex.Response,
                            HttpWebResponse)

                    If httpResponse IsNot Nothing Then

                        statusCode =
                            CInt(
                                httpResponse.StatusCode).
                            ToString()

                        statusDescription =
                            httpResponse.StatusDescription

                    End If

                End If

            Catch

            End Try

            ' --------------------------------------------------------
            ' RESPONSE BODY
            ' --------------------------------------------------------

            Try

                If ex.Response IsNot Nothing Then

                    Dim httpResponse As HttpWebResponse =
                        TryCast(
                            ex.Response,
                            HttpWebResponse)

                    If httpResponse IsNot Nothing Then

                        Using stream As Stream =
                            httpResponse.GetResponseStream()

                            If stream IsNot Nothing Then

                                Using reader As New StreamReader(
                                    stream,
                                    Encoding.UTF8)

                                    responseBody =
                                        reader.ReadToEnd()

                                End Using

                            End If

                        End Using

                    End If

                End If

            Catch bodyException As Exception

                responseBody =
                    "[Tidak dapat membaca response body: " &
                    bodyException.Message &
                    "]"

            End Try

            ' --------------------------------------------------------
            ' BUILD DIAGNOSTIC
            ' --------------------------------------------------------

            Dim builder As New StringBuilder()

            builder.AppendLine(
                "Gemini request gagal.")

            builder.AppendLine(
                "====================")

            builder.AppendLine()

            builder.AppendLine(
                "Provider : Google Gemini")

            builder.AppendLine(
                "Model    : " &
                If(
                    String.IsNullOrWhiteSpace(model),
                    "(kosong)",
                    model))

            builder.AppendLine(
                "URL      : " &
                SanitizeDiagnosticUrl(
                    requestUrl))

            If Not String.IsNullOrWhiteSpace(
                statusCode) Then

                builder.AppendLine(
                    "HTTP     : " &
                    statusCode &
                    If(
                        String.IsNullOrWhiteSpace(
                            statusDescription),
                        String.Empty,
                        " " & statusDescription))

            Else

                builder.AppendLine(
                    "HTTP     : (status tidak tersedia)")

            End If

            builder.AppendLine()

            If Not String.IsNullOrWhiteSpace(
                responseBody) Then

                Dim apiMessage As String =
                    ExtractJsonErrorMessage(
                        responseBody)

                If Not String.IsNullOrWhiteSpace(
                    apiMessage) Then

                    builder.AppendLine(
                        "Gemini Message:")

                    builder.AppendLine(
                        apiMessage)

                    builder.AppendLine()

                End If

                builder.AppendLine(
                    "Response Body:")

                builder.AppendLine(
                    LimitDiagnosticResponse(
                        responseBody))

            Else

                builder.AppendLine(
                    "Response Body:")

                builder.AppendLine(
                    "(kosong / tidak tersedia)")

            End If

            builder.AppendLine()

            builder.AppendLine(
                "Exception:")

            builder.AppendLine(
                ex.Message)

            Return builder.ToString().TrimEnd()

        End Function

        ' ============================================================
        ' DIAGNOSTIC MESSAGE
        ' ============================================================

        Private Function BuildDiagnosticMessage(
            message As String,
            requestUrl As String
        ) As String

            Dim builder As New StringBuilder()

            builder.AppendLine(
                message)

            builder.AppendLine()

            builder.AppendLine(
                "Provider : Google Gemini")

            builder.AppendLine(
                "Model    : " &
                If(
                    String.IsNullOrWhiteSpace(model),
                    "(kosong)",
                    model))

            builder.AppendLine(
                "URL      : " &
                SanitizeDiagnosticUrl(
                    requestUrl))

            Return builder.ToString().TrimEnd()

        End Function

        ' ============================================================
        ' SANITIZE URL
        '
        ' API KEY TIDAK BOLEH TAMPIL.
        ' ============================================================

        Private Function SanitizeDiagnosticUrl(
            value As String
        ) As String

            If String.IsNullOrWhiteSpace(value) Then
                Return "(URL kosong)"
            End If

            Try

                Dim uri As New Uri(value)

                Dim builder As New UriBuilder(uri)

                ' Hapus query string dan fragment.
                builder.Query = String.Empty
                builder.Fragment = String.Empty

                Return builder.Uri.AbsoluteUri.TrimEnd("?"c)

            Catch

                ' Fallback jika URL tidak dapat diparse.
                Dim result As String =
                    value

                Dim queryIndex As Integer =
                    result.IndexOf("?"c)

                If queryIndex >= 0 Then

                    result =
                        result.Substring(
                            0,
                            queryIndex)

                End If

                Dim fragmentIndex As Integer =
                    result.IndexOf("#"c)

                If fragmentIndex >= 0 Then

                    result =
                        result.Substring(
                            0,
                            fragmentIndex)

                End If

                Return result

            End Try

        End Function

        ' ============================================================
        ' LIMIT RESPONSE BODY
        ' ============================================================

        Private Function LimitDiagnosticResponse(
            value As String
        ) As String

            If String.IsNullOrWhiteSpace(value) Then
                Return "(kosong)"
            End If

            Dim text As String =
                value.Trim()

            Const maxLength As Integer = 12000

            If text.Length <= maxLength Then
                Return text
            End If

            Return text.Substring(
                0,
                maxLength) &
                Environment.NewLine &
                "... [response body dipotong oleh DevBot]"

        End Function

        ' ============================================================
        ' EXTRACT API ERROR MESSAGE
        '
        ' Contoh response Gemini:
        '
        ' {
        '   "error": {
        '      "code": 404,
        '      "message": "...",
        '      "status": "NOT_FOUND"
        '   }
        ' }
        ' ============================================================

        Private Function ExtractJsonErrorMessage(
            json As String
        ) As String

            If String.IsNullOrWhiteSpace(json) Then
                Return String.Empty
            End If

            Dim marker As String =
                Microsoft.VisualBasic.ChrW(34) &
                "message" &
                Microsoft.VisualBasic.ChrW(34)

            Dim start As Integer =
                json.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase)

            If start < 0 Then
                Return String.Empty
            End If

            Dim colonIndex As Integer =
                json.IndexOf(
                    Microsoft.VisualBasic.ChrW(58),
                    start + marker.Length)

            If colonIndex < 0 Then
                Return String.Empty
            End If

            Dim valueStart As Integer =
                colonIndex + 1

            While valueStart < json.Length AndAlso
                  Char.IsWhiteSpace(
                      json(valueStart))

                valueStart += 1

            End While

            If valueStart >= json.Length OrElse
               json(valueStart) <>
               Microsoft.VisualBasic.ChrW(34) Then

                Return String.Empty

            End If

            valueStart += 1

            Dim builder As New StringBuilder()

            Dim escaped As Boolean = False

            For index As Integer =
                valueStart To json.Length - 1

                Dim ch As Char =
                    json(index)

                If escaped Then

                    Select Case ch

                        Case Microsoft.VisualBasic.ChrW(34)

                            builder.Append(
                                Microsoft.VisualBasic.ChrW(34))

                        Case Microsoft.VisualBasic.ChrW(92)

                            builder.Append(
                                Microsoft.VisualBasic.ChrW(92))

                        Case Microsoft.VisualBasic.ChrW(110)

                            builder.Append(
                                Microsoft.VisualBasic.ChrW(10))

                        Case Microsoft.VisualBasic.ChrW(114)

                            builder.Append(
                                Microsoft.VisualBasic.ChrW(13))

                        Case Microsoft.VisualBasic.ChrW(116)

                            builder.Append(
                                Microsoft.VisualBasic.ChrW(9))

                        Case Else

                            builder.Append(ch)

                    End Select

                    escaped = False

                ElseIf ch =
                       Microsoft.VisualBasic.ChrW(92) Then

                    escaped = True

                ElseIf ch =
                       Microsoft.VisualBasic.ChrW(34) Then

                    Exit For

                Else

                    builder.Append(ch)

                End If

            Next

            Return builder.ToString().Trim()

        End Function

        ' ============================================================
        ' NORMALIZE TEMPERATURE
        ' ============================================================

        Private Function NormalizeTemperature(
            value As Double
        ) As Double

            If Double.IsNaN(value) OrElse
               Double.IsInfinity(value) Then

                Return 0.2R

            End If

            If value < 0.0R Then
                Return 0.0R
            End If

            If value > 2.0R Then
                Return 2.0R
            End If

            Return value

        End Function

        ' ============================================================
        ' NORMALIZE MAX OUTPUT
        ' ============================================================

        Private Function NormalizeMaxOutput(
            value As Integer
        ) As Integer

            If value <= 0 Then
                Return 8192
            End If

            If value > 1000000 Then
                Return 1000000
            End If

            Return value

        End Function

        ' ============================================================
        ' NORMALIZE TIMEOUT
        ' ============================================================

        Private Function NormalizeTimeout(
            seconds As Integer
        ) As Integer

            If seconds <= 0 Then
                Return 120000
            End If

            If seconds > 3600 Then
                seconds = 3600
            End If

            Return seconds * 1000

        End Function

    End Class

End Namespace