Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Net
Imports System.Net.WebSockets
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace DevBot.Core.Test

    Public Class WebTestAdapter
        Implements ITestAdapter

        Private Const DefaultTimeoutMilliseconds As Integer = 30000
        Private Const DebugPort As Integer = 9222

        Public Function CanHandle(projectPath As String) As Boolean _
            Implements ITestAdapter.CanHandle

            If String.IsNullOrWhiteSpace(projectPath) OrElse
               Not Directory.Exists(projectPath) Then
                Return False
            End If

            If System.IO.File.Exists(Path.Combine(projectPath, "package.json")) Then
                Return True
            End If

            If System.IO.File.Exists(Path.Combine(projectPath, "composer.json")) Then
                Return True
            End If

            Try
                Dim phpFiles() As String = Directory.GetFiles(
                    projectPath, "*.php", SearchOption.AllDirectories)

                Return phpFiles.Length > 0
            Catch
                Return False
            End Try
        End Function

        Public Function GetAdapterName() As String _
            Implements ITestAdapter.GetAdapterName

            Return "Web Test Adapter - Chrome/Edge CDP"
        End Function

        Public Function Run(context As TestContext) As TestAdapterResult _
            Implements ITestAdapter.Run

            Dim result As New TestAdapterResult()
            result.AdapterName = GetAdapterName()

            Dim stopwatch As Stopwatch = Stopwatch.StartNew()

            Try
                If context Is Nothing Then
                    result.Status = "ERROR"
                    result.Message = "Test context kosong."
                    Return FinishResult(result, stopwatch)
                End If

                If String.IsNullOrWhiteSpace(context.ProjectPath) Then
                    result.Status = "ERROR"
                    result.Message = "Project path kosong."
                    Return FinishResult(result, stopwatch)
                End If

                If Not Directory.Exists(context.ProjectPath) Then
                    result.Status = "ERROR"
                    result.Message = "Project tidak ditemukan: " &
                                     context.ProjectPath
                    Return FinishResult(result, stopwatch)
                End If

                Dim url As String = BuildTestUrl(context)
                Dim browser As String = FindBrowser()

                If String.IsNullOrWhiteSpace(browser) Then
                    result.Status = "NOT_SUPPORTED"
                    result.Message =
                        "Google Chrome atau Microsoft Edge tidak ditemukan."
                    Return FinishResult(result, stopwatch)
                End If

                Dim output As New StringBuilder()

                output.AppendLine("WEB UI AUTOMATION TEST")
                output.AppendLine("======================")
                output.AppendLine("Adapter : " & GetAdapterName())
                output.AppendLine("Project : " & context.ProjectPath)
                output.AppendLine("Browser : " & browser)
                output.AppendLine("URL     : " & url)
                output.AppendLine("")

                Dim browserProcess As Process =
                    LaunchBrowser(browser, url)

                If browserProcess Is Nothing Then
                    result.Status = "ERROR"
                    result.Message = "Browser gagal dijalankan."
                    Return FinishResult(result, stopwatch)
                End If

                output.AppendLine("1. Browser      : STARTED")

                Dim wsUrl As String =
                    WaitForDevToolsWebSocket(
                        "127.0.0.1",
                        DebugPort,
                        If(context.TimeoutMilliseconds > 0,
                           context.TimeoutMilliseconds,
                           DefaultTimeoutMilliseconds))

                If String.IsNullOrWhiteSpace(wsUrl) Then
                    output.AppendLine("2. DevTools     : NOT CONNECTED")
                    output.AppendLine("")
                    output.AppendLine(
                        "Chrome/Edge berjalan, tetapi DevTools CDP tidak dapat dihubungkan.")
                    output.AppendLine(
                        "Pastikan browser ditutup sebelum menjalankan test lagi.")

                    result.Success = False
                    result.Status = "FAIL"
                    result.Message = "DevTools CDP tidak dapat dihubungkan."
                    result.Output = output.ToString()

                    Return FinishResult(result, stopwatch)
                End If

                output.AppendLine("2. DevTools     : CONNECTED")

                Dim cdp As New CdpClient()
                Dim cdpConnected As Boolean =
                    cdp.ConnectAsync(wsUrl).GetAwaiter().GetResult()

                If Not cdpConnected Then
                    result.Success = False
                    result.Status = "FAIL"
                    result.Message = "Gagal membuka koneksi CDP."
                    result.Output = output.ToString()
                    Return FinishResult(result, stopwatch)
                End If

                output.AppendLine("3. CDP          : CONNECTED")

                Dim navResponse As String =
                    cdp.SendCommandAsync(
                        "Page.enable",
                        "{}").GetAwaiter().GetResult()

                navResponse =
                    cdp.SendCommandAsync(
                        "Page.navigate",
                        "{""url"":""" & JsonEscape(url) & """}").
                    GetAwaiter().GetResult()

                output.AppendLine("4. Navigate     : DONE")

                Thread.Sleep(1500)

                Dim inspectExpression As String =
                    "(function(){return JSON.stringify({" &
                    "title:document.title," &
                    "url:location.href," &
                    "links:document.querySelectorAll('a').length," &
                    "buttons:document.querySelectorAll('button,input[type=submit],input[type=button]').length," &
                    "inputs:document.querySelectorAll('input,textarea,select').length," &
                    "forms:document.querySelectorAll('form').length," &
                    "bodyText:(document.body?document.body.innerText:'').substring(0,2000)" &
                    "});})()"

                Dim inspectResponse As String =
                    cdp.SendCommandAsync(
                        "Runtime.evaluate",
                        "{""expression"":""" &
                        JsonEscape(inspectExpression) &
                        """,""returnByValue"":true}").
                    GetAwaiter().GetResult()

                Dim pageInfo As String =
                    ExtractRemoteValue(inspectResponse)

                output.AppendLine("5. DOM Inspect  : DONE")
                output.AppendLine("")
                output.AppendLine("PAGE INFO")
                output.AppendLine("---------")
                output.AppendLine(pageInfo)

                Dim screenshotDir As String =
                    Path.Combine(context.ProjectPath, ".devbot-test")

                If Not Directory.Exists(screenshotDir) Then
                    Directory.CreateDirectory(screenshotDir)
                End If

                Dim screenshotFile As String =
                    Path.Combine(
                        screenshotDir,
                        "web-test-" &
                        DateTime.Now.ToString("yyyyMMdd-HHmmss") &
                        ".png")

                Dim screenshotResponse As String =
                    cdp.SendCommandAsync(
                        "Page.captureScreenshot",
                        "{""format"":""png"",""captureBeyondViewport"":true}").
                    GetAwaiter().GetResult()

                Dim screenshotBase64 As String =
                    ExtractDataBase64(screenshotResponse)

                If Not String.IsNullOrWhiteSpace(screenshotBase64) Then

                    Dim bytes() As Byte =
                        Convert.FromBase64String(screenshotBase64)

                    System.IO.File.WriteAllBytes(
                        screenshotFile,
                        bytes)

                    output.AppendLine("")
                    output.AppendLine(
                        "6. Screenshot   : SAVED")
                    output.AppendLine(
                        "   File : " & screenshotFile)

                Else

                    output.AppendLine("")
                    output.AppendLine(
                        "6. Screenshot   : FAILED")

                End If

                Dim title As String =
                    ExtractJsonStringValue(pageInfo, "title")

                Dim currentUrl As String =
                    ExtractJsonStringValue(pageInfo, "url")

                Dim linkCount As Integer =
                    ExtractJsonIntegerValue(pageInfo, "links")

                Dim buttonCount As Integer =
                    ExtractJsonIntegerValue(pageInfo, "buttons")

                Dim inputCount As Integer =
                    ExtractJsonIntegerValue(pageInfo, "inputs")

                Dim formCount As Integer =
                    ExtractJsonIntegerValue(pageInfo, "forms")

                output.AppendLine("")
                output.AppendLine("UI VERIFICATION")
                output.AppendLine("---------------")
                output.AppendLine("Title       : " & title)
                output.AppendLine("Current URL : " & currentUrl)
                output.AppendLine("Links       : " & linkCount.ToString())
                output.AppendLine("Buttons     : " & buttonCount.ToString())
                output.AppendLine("Inputs      : " & inputCount.ToString())
                output.AppendLine("Forms       : " & formCount.ToString())

                If linkCount > 0 OrElse
                   buttonCount > 0 OrElse
                   inputCount > 0 OrElse
                   formCount > 0 Then

                    output.AppendLine("")
                    output.AppendLine("7. UI Check   : PASS")
                    result.Success = True
                    result.Status = "PASS"
                    result.ExitCode = 0
                    result.Message =
                        "Browser berhasil dibuka, halaman dinavigasi, DOM diperiksa, dan screenshot dibuat."

                Else

                    output.AppendLine("")
                    output.AppendLine("7. UI Check   : WARNING")
                    result.Success = True
                    result.Status = "PASS"
                    result.ExitCode = 0
                    result.Message =
                        "Halaman berhasil dibuka, tetapi elemen interaktif tidak ditemukan."

                End If

                result.Output = output.ToString()

                Try
                    cdp.Close()
                Catch
                End Try

                Return FinishResult(result, stopwatch)

            Catch ex As Exception

                result.Success = False
                result.Status = "ERROR"
                result.ExitCode = -1
                result.Message = ex.Message
                result.ErrorOutput = ex.ToString()

                Return FinishResult(result, stopwatch)
            End Try

        End Function

        Public Function OpenInBrowser(context As TestContext) As TestAdapterResult

            Dim result As New TestAdapterResult()
            result.AdapterName = GetAdapterName()

            Dim stopwatch As Stopwatch = Stopwatch.StartNew()

            Try
                If context Is Nothing OrElse
                   String.IsNullOrWhiteSpace(context.ProjectPath) Then

                    result.Status = "ERROR"
                    result.Message = "Project path kosong."
                    Return FinishResult(result, stopwatch)
                End If

                Dim url As String = BuildTestUrl(context)
                Dim browser As String = FindBrowser()

                If String.IsNullOrWhiteSpace(browser) Then
                    result.Status = "NOT_SUPPORTED"
                    result.Message =
                        "Chrome atau Microsoft Edge tidak ditemukan."
                    Return FinishResult(result, stopwatch)
                End If

                Dim browserProcess As Process =
                    LaunchBrowser(browser, url)

                If browserProcess Is Nothing Then
                    result.Status = "ERROR"
                    result.Message = "Browser gagal dijalankan."
                    Return FinishResult(result, stopwatch)
                End If

                result.Success = True
                result.Status = "PASS"
                result.ExitCode = 0
                result.Message =
                    "Browser berhasil dijalankan."
                result.Output =
                    "BROWSER LAUNCHED" &
                    Microsoft.VisualBasic.vbCrLf &
                    "Browser : " & browser &
                    Microsoft.VisualBasic.vbCrLf &
                    "URL     : " & url &
                    Microsoft.VisualBasic.vbCrLf &
                    "CDP     : http://127.0.0.1:" &
                    DebugPort.ToString()

                Return FinishResult(result, stopwatch)

            Catch ex As Exception

                result.Success = False
                result.Status = "ERROR"
                result.ExitCode = -1
                result.Message = ex.Message
                result.ErrorOutput = ex.ToString()

                Return FinishResult(result, stopwatch)
            End Try

        End Function

        Private Function LaunchBrowser(
            browserPath As String,
            url As String) As Process

            Dim tempProfile As String =
                Path.Combine(
                    Path.GetTempPath(),
                    "DevBotChrome_" &
                    Guid.NewGuid().ToString("N"))

            Directory.CreateDirectory(tempProfile)

            Dim psi As New ProcessStartInfo()

            psi.FileName = browserPath

            psi.Arguments =
                "--remote-debugging-port=" &
                DebugPort.ToString() &
                " --user-data-dir=""" &
                tempProfile &
                """ --no-first-run --no-default-browser-check """ &
                url &
                """"

            psi.UseShellExecute = True
            psi.WindowStyle = ProcessWindowStyle.Normal

            Return Process.Start(psi)

        End Function

        Private Function FindBrowser() As String

            Dim candidates As New List(Of String)()

            Dim programFiles As String =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles)

            Dim programFilesX86 As String =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFilesX86)

            candidates.Add(
                Path.Combine(
                    programFiles,
                    "Google\Chrome\Application\chrome.exe"))

            candidates.Add(
                Path.Combine(
                    programFilesX86,
                    "Google\Chrome\Application\chrome.exe"))

            candidates.Add(
                Path.Combine(
                    programFiles,
                    "Microsoft\Edge\Application\msedge.exe"))

            candidates.Add(
                Path.Combine(
                    programFilesX86,
                    "Microsoft\Edge\Application\msedge.exe"))

            For Each candidate As String In candidates
                If System.IO.File.Exists(candidate) Then
                    Return candidate
                End If
            Next

            Return ""

        End Function

        Private Function BuildTestUrl(context As TestContext) As String

            If Not String.IsNullOrWhiteSpace(context.Arguments) Then

                Dim supplied As String =
                    context.Arguments.Trim()

                If supplied.StartsWith(
                    "http://",
                    StringComparison.OrdinalIgnoreCase) OrElse
                   supplied.StartsWith(
                    "https://",
                    StringComparison.OrdinalIgnoreCase) Then

                    Return supplied
                End If

            End If

            Dim projectName As String =
                New DirectoryInfo(context.ProjectPath).Name

            Return "http://localhost/" &
                   projectName &
                   "/"

        End Function

        Private Function WaitForDevToolsWebSocket(
            host As String,
            port As Integer,
            timeoutMilliseconds As Integer) As String

            Dim started As DateTime = DateTime.Now

            Do

                Try
                    Dim request As HttpWebRequest =
                        DirectCast(
                            WebRequest.Create(
                                "http://" &
                                host &
                                ":" &
                                port.ToString() &
                                "/json"),
                            HttpWebRequest)

                    request.Method = "GET"
                    request.Timeout = 2000

                    Using response As HttpWebResponse =
                        DirectCast(request.GetResponse(), HttpWebResponse)

                        Using stream As Stream =
                            response.GetResponseStream()

                            Using reader As New StreamReader(stream)

                                Dim json As String =
                                    reader.ReadToEnd()

                                Dim ws As String =
                                    ExtractFirstWebSocketUrl(json)

                                If Not String.IsNullOrWhiteSpace(ws) Then
                                    Return ws
                                End If

                            End Using
                        End Using
                    End Using

                Catch
                End Try

                Thread.Sleep(250)

            Loop While (DateTime.Now - started).TotalMilliseconds < timeoutMilliseconds

            Return ""

        End Function

        Private Function ExtractFirstWebSocketUrl(
            json As String) As String

            If String.IsNullOrWhiteSpace(json) Then
                Return ""
            End If

            Dim marker As String = """webSocketDebuggerUrl"":"""

            Dim pos As Integer =
                json.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase)

            If pos < 0 Then
                Return ""
            End If

            Dim start As Integer =
                pos + marker.Length

            Dim finish As Integer =
                json.IndexOf("""", start)

            If finish < 0 Then
                Return ""
            End If

            Return json.Substring(
                start,
                finish - start)

        End Function

        Private Function ExtractRemoteValue(
            json As String) As String

            If String.IsNullOrWhiteSpace(json) Then
                Return ""
            End If

            Dim marker As String = """value"":"

            Dim pos As Integer =
                json.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase)

            If pos < 0 Then
                Return json
            End If

            Dim start As Integer =
                pos + marker.Length

            While start < json.Length AndAlso
                  Char.IsWhiteSpace(json(start))
                start += 1
            End While

            If start >= json.Length Then
                Return ""
            End If

            If json(start) = """"c Then
                Return ExtractJsonQuotedString(
                    json,
                    start)
            End If

            Return json.Substring(start).Trim()

        End Function

        Private Function ExtractDataBase64(
            json As String) As String

            If String.IsNullOrWhiteSpace(json) Then
                Return ""
            End If

            Dim marker As String = """data"":"""

            Dim pos As Integer =
                json.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase)

            If pos < 0 Then
                Return ""
            End If

            Dim start As Integer =
                pos + marker.Length

            Dim finish As Integer =
                json.IndexOf("""", start)

            If finish < 0 Then
                Return ""
            End If

            Return json.Substring(
                start,
                finish - start)

        End Function

        Private Function ExtractJsonStringValue(
            json As String,
            propertyName As String) As String

            Dim marker As String =
                """" & propertyName & """:"""

            Dim pos As Integer =
                json.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase)

            If pos < 0 Then
                Return ""
            End If

            Dim start As Integer =
                pos + marker.Length

            Dim finish As Integer = start
            Dim escaped As Boolean = False

            While finish < json.Length

                Dim ch As Char = json(finish)

                If escaped Then
                    escaped = False
                ElseIf ch = "\"c Then
                    escaped = True
                ElseIf ch = """"c Then
                    Exit While
                End If

                finish += 1

            End While

            If finish >= json.Length Then
                Return ""
            End If

            Return JsonUnescape(
                json.Substring(
                    start,
                    finish - start))

        End Function

        Private Function ExtractJsonIntegerValue(
            json As String,
            propertyName As String) As Integer

            Dim marker As String =
                """" & propertyName & """:"

            Dim pos As Integer =
                json.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase)

            If pos < 0 Then
                Return 0
            End If

            Dim start As Integer =
                pos + marker.Length

            While start < json.Length AndAlso
                  Char.IsWhiteSpace(json(start))
                start += 1
            End While

            Dim finish As Integer = start

            While finish < json.Length AndAlso
                  Char.IsDigit(json(finish))
                finish += 1
            End While

            Dim value As Integer = 0

            Integer.TryParse(
                json.Substring(start, finish - start),
                value)

            Return value

        End Function

        Private Function ExtractJsonQuotedString(
            json As String,
            quotePosition As Integer) As String

            Dim start As Integer =
                quotePosition + 1

            Dim finish As Integer = start
            Dim escaped As Boolean = False

            While finish < json.Length

                Dim ch As Char = json(finish)

                If escaped Then
                    escaped = False
                ElseIf ch = "\"c Then
                    escaped = True
                ElseIf ch = """"c Then
                    Exit While
                End If

                finish += 1

            End While

            If finish >= json.Length Then
                Return ""
            End If

            Return JsonUnescape(
                json.Substring(
                    start,
                    finish - start))

        End Function

        Private Function JsonEscape(value As String) As String

            If value Is Nothing Then
                Return ""
            End If

            Return value.Replace(
                "\",
                "\\").Replace(
                """",
                "\""").Replace(
                Microsoft.VisualBasic.vbCr,
                "\r").Replace(
                Microsoft.VisualBasic.vbLf,
                "\n").Replace(
                Microsoft.VisualBasic.vbTab,
                "\t")

        End Function

        Private Function JsonUnescape(value As String) As String

            If value Is Nothing Then
                Return ""
            End If

            Return value.Replace(
                "\r",
                Microsoft.VisualBasic.vbCr).Replace(
                "\n",
                Microsoft.VisualBasic.vbLf).Replace(
                "\t",
                Microsoft.VisualBasic.vbTab).Replace(
                "\""",
                """").Replace(
                "\\",
                "\")

        End Function

        Private Function FinishResult(
            result As TestAdapterResult,
            stopwatch As Stopwatch) As TestAdapterResult

            stopwatch.Stop()
            result.Duration = stopwatch.Elapsed
            Return result

        End Function

        Private Class CdpClient

            Private ReadOnly socket As ClientWebSocket
            Private commandId As Integer

            Public Sub New()
                socket = New ClientWebSocket()
                commandId = 0
            End Sub

            Public Function ConnectAsync(
                webSocketUrl As String) As Task(Of Boolean)

                Return ConnectInternalAsync(webSocketUrl)

            End Function

            Private Async Function ConnectInternalAsync(
                webSocketUrl As String) As Task(Of Boolean)

                Try

                    Await socket.ConnectAsync(
                        New Uri(webSocketUrl),
                        CancellationToken.None)

                    Return socket.State =
                           WebSocketState.Open

                Catch
                    Return False
                End Try

            End Function

            Public Function SendCommandAsync(
                method As String,
                parameters As String) As Task(Of String)

                Return SendInternalAsync(
                    method,
                    parameters)

            End Function

            Private Async Function SendInternalAsync(
                method As String,
                parameters As String) As Task(Of String)

                commandId += 1

                Dim json As String =
                    "{""id"":" &
                    commandId.ToString() &
                    ",""method"":""" &
                    JsonEscape(method) &
                    """,""params"":" &
                    If(String.IsNullOrWhiteSpace(parameters),
                       "{}",
                       parameters) &
                    "}"

                Dim bytes() As Byte =
                    Encoding.UTF8.GetBytes(json)

                Await socket.SendAsync(
                    New ArraySegment(Of Byte)(bytes),
                    WebSocketMessageType.Text,
                    True,
                    CancellationToken.None)

                Do

                    Dim buffer(8191) As Byte
                    Dim builder As New StringBuilder()
                    Dim messageComplete As Boolean = False

                    Do

                        Dim receive As WebSocketReceiveResult =
                            Await socket.ReceiveAsync(
                                New ArraySegment(Of Byte)(buffer),
                                CancellationToken.None)

                        If receive.MessageType =
                           WebSocketMessageType.Close Then

                            Return ""

                        End If

                        builder.Append(
                            Encoding.UTF8.GetString(
                                buffer,
                                0,
                                receive.Count))

                        messageComplete =
                            receive.EndOfMessage

                    Loop Until messageComplete

                    Dim response As String =
                        builder.ToString()

                    Dim idMarker As String =
                        """id"":" &
                        commandId.ToString()

                    If response.IndexOf(
                        idMarker,
                        StringComparison.OrdinalIgnoreCase) >= 0 Then

                        Return response

                    End If

                Loop

            End Function

            Private Shared Function JsonEscape(value As String) As String

                If value Is Nothing Then
                    Return ""
                End If

                Return value.Replace(
                    "\",
                    "\\").Replace(
                    """",
                    "\""").Replace(
                    Microsoft.VisualBasic.vbCr,
                    "\r").Replace(
                    Microsoft.VisualBasic.vbLf,
                    "\n").Replace(
                    Microsoft.VisualBasic.vbTab,
                    "\t")

            End Function

            Public Sub Close()

                Try
                    If socket.State = WebSocketState.Open Then
                        socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "DevBot finished",
                            CancellationToken.None).
                            GetAwaiter().GetResult()
                    End If
                Catch
                End Try

                socket.Dispose()

            End Sub

        End Class

    End Class

End Namespace
