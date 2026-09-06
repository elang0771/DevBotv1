Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text

Namespace DevBot.Core.AI

    Public Class AIProviderSettingsStore

        Private ReadOnly settingsDirectory As String
        Private ReadOnly settingsFilePath As String

        Public Sub New()

            settingsDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DevBot")

            settingsFilePath = System.IO.Path.Combine(
                settingsDirectory,
                "ai-providers.dat")

        End Sub

        Public Function GetSettingsFilePath() As String
            Return settingsFilePath
        End Function

        Public Function LoadAll() As List(Of AIProviderSettings)

            If Not System.IO.File.Exists(settingsFilePath) Then
                Return CreateDefaultProviders()
            End If

            Try
                Dim result As New List(Of AIProviderSettings)()

                Dim lines() As String =
                    System.IO.File.ReadAllLines(settingsFilePath, Encoding.UTF8)

                For Each line As String In lines

                    If String.IsNullOrWhiteSpace(line) Then
                        Continue For
                    End If

                    Dim parts() As String = line.Split(New Char() {"|"c})

                    If parts.Length < 12 Then
                        Continue For
                    End If

                    Dim item As New AIProviderSettings()

                    item.Id = Decode(parts(0))
                    item.Provider = Decode(parts(1))
                    item.Model = Decode(parts(2))
                    item.ApiKey = Decode(parts(3))
                    item.BaseUrl = Decode(parts(4))
                    item.ApiFormat = Decode(parts(5))
                    item.ThinkingLevel = Decode(parts(6))

                    Dim d As Double
                    If Double.TryParse(Decode(parts(7)), Globalization.NumberStyles.Float,
                                       Globalization.CultureInfo.InvariantCulture, d) Then
                        item.Temperature = d
                    End If

                    Dim i As Integer

                    If Integer.TryParse(Decode(parts(8)), i) Then
                        item.MaxOutput = i
                    End If

                    If Integer.TryParse(Decode(parts(9)), i) Then
                        item.TimeoutSeconds = i
                    End If

                    Dim b As Boolean
                    If Boolean.TryParse(Decode(parts(10)), b) Then
                        item.Enabled = b
                    End If

                    If Integer.TryParse(Decode(parts(11)), i) Then
                        item.Priority = i
                    End If

                    If String.IsNullOrWhiteSpace(item.Id) Then
                        item.Id = Guid.NewGuid().ToString("N")
                    End If

                    result.Add(item)

                Next

                If result.Count = 0 Then
                    Return CreateDefaultProviders()
                End If

                Return result

            Catch
                Return CreateDefaultProviders()
            End Try

        End Function

        Public Function LoadEnabled() As List(Of AIProviderSettings)

            Return LoadAll().
                Where(Function(item)
                          Return item IsNot Nothing AndAlso item.Enabled
                      End Function).
                OrderBy(Function(item)
                            Return item.Priority
                        End Function).
                ToList()

        End Function

        Public Function GetDefaultProvider() As AIProviderSettings

            Dim providers As List(Of AIProviderSettings) = LoadEnabled()

            If providers.Count = 0 Then
                Return Nothing
            End If

            Return providers(0).Clone()

        End Function

        Public Function SaveAll(
            providers As IEnumerable(Of AIProviderSettings)
        ) As Boolean

            If providers Is Nothing Then
                Return False
            End If

            Try

                If Not System.IO.Directory.Exists(settingsDirectory) Then
                    System.IO.Directory.CreateDirectory(settingsDirectory)
                End If

                Dim tempFile As String = settingsFilePath & ".tmp"

                Using writer As New StreamWriter(
                    tempFile,
                    False,
                    New UTF8Encoding(False))

                    For Each provider As AIProviderSettings In providers

                        If provider Is Nothing Then
                            Continue For
                        End If

                        If String.IsNullOrWhiteSpace(provider.Id) Then
                            provider.Id = Guid.NewGuid().ToString("N")
                        End If

                        Dim values As String() = {
                            Encode(provider.Id),
                            Encode(provider.Provider),
                            Encode(provider.Model),
                            Encode(provider.ApiKey),
                            Encode(provider.BaseUrl),
                            Encode(provider.ApiFormat),
                            Encode(provider.ThinkingLevel),
                            Encode(provider.Temperature.ToString(
                                Globalization.CultureInfo.InvariantCulture)),
                            Encode(provider.MaxOutput.ToString(
                                Globalization.CultureInfo.InvariantCulture)),
                            Encode(provider.TimeoutSeconds.ToString(
                                Globalization.CultureInfo.InvariantCulture)),
                            Encode(provider.Enabled.ToString()),
                            Encode(provider.Priority.ToString(
                                Globalization.CultureInfo.InvariantCulture))
                        }

                        writer.WriteLine(String.Join("|", values))

                    Next

                End Using

                If System.IO.File.Exists(settingsFilePath) Then
                    System.IO.File.Delete(settingsFilePath)
                End If

                System.IO.File.Move(tempFile, settingsFilePath)

                Return True

            Catch
                Return False
            End Try

        End Function

        Public Function Save(
            provider As AIProviderSettings
        ) As Boolean

            If provider Is Nothing Then
                Return False
            End If

            Dim providers As List(Of AIProviderSettings) = LoadAll()

            Dim existing As AIProviderSettings =
                providers.FirstOrDefault(
                    Function(item)
                        Return item IsNot Nothing AndAlso
                               String.Equals(
                                   item.Id,
                                   provider.Id,
                                   StringComparison.OrdinalIgnoreCase)
                    End Function)

            If existing Is Nothing Then
                providers.Add(provider.Clone())
            Else
                Dim index As Integer = providers.IndexOf(existing)
                providers(index) = provider.Clone()
            End If

            Return SaveAll(providers)

        End Function

        Public Function Delete(providerId As String) As Boolean

            If String.IsNullOrWhiteSpace(providerId) Then
                Return False
            End If

            Dim providers As List(Of AIProviderSettings) = LoadAll()

            Dim existing As AIProviderSettings =
                providers.FirstOrDefault(
                    Function(item)
                        Return item IsNot Nothing AndAlso
                               String.Equals(
                                   item.Id,
                                   providerId,
                                   StringComparison.OrdinalIgnoreCase)
                    End Function)

            If existing Is Nothing Then
                Return False
            End If

            providers.Remove(existing)

            Return SaveAll(providers)

        End Function

        Private Function CreateDefaultProviders() As List(Of AIProviderSettings)

            Dim providers As New List(Of AIProviderSettings)()

            Dim gemini As New AIProviderSettings()
            gemini.Provider = "Gemini"
            gemini.Model = "gemini-2.5-flash"
            gemini.ApiFormat = "Gemini"
            gemini.BaseUrl = "https://generativelanguage.googleapis.com"
            gemini.ThinkingLevel = "Medium"
            gemini.Priority = 1
            gemini.Enabled = True
            providers.Add(gemini)

            Dim openai As New AIProviderSettings()
            openai.Provider = "OpenAI"
            openai.Model = "gpt-4.1-mini"
            openai.ApiFormat = "OpenAI"
            openai.BaseUrl = "https://api.openai.com/v1"
            openai.ThinkingLevel = "Medium"
            openai.Priority = 2
            openai.Enabled = False
            providers.Add(openai)

            Dim claude As New AIProviderSettings()
            claude.Provider = "Claude"
            claude.Model = "claude-sonnet-4"
            claude.ApiFormat = "Anthropic"
            claude.BaseUrl = "https://api.anthropic.com"
            claude.ThinkingLevel = "Medium"
            claude.Priority = 3
            claude.Enabled = False
            providers.Add(claude)

            Dim deepseek As New AIProviderSettings()
            deepseek.Provider = "DeepSeek"
            deepseek.Model = "deepseek-chat"
            deepseek.ApiFormat = "OpenAI-Compatible"
            deepseek.BaseUrl = "https://api.deepseek.com"
            deepseek.ThinkingLevel = "Medium"
            deepseek.Priority = 4
            deepseek.Enabled = False
            providers.Add(deepseek)

            Dim kimi As New AIProviderSettings()
            kimi.Provider = "Kimi"
            kimi.Model = "moonshot-v1-8k"
            kimi.ApiFormat = "OpenAI-Compatible"
            kimi.BaseUrl = "https://api.moonshot.ai/v1"
            kimi.ThinkingLevel = "Medium"
            kimi.Priority = 5
            kimi.Enabled = False
            providers.Add(kimi)

            Dim custom As New AIProviderSettings()
            custom.Provider = "Custom"
            custom.Model = String.Empty
            custom.ApiFormat = "OpenAI-Compatible"
            custom.BaseUrl = String.Empty
            custom.ThinkingLevel = "Medium"
            custom.Priority = 99
            custom.Enabled = False
            providers.Add(custom)

            Return providers

        End Function

        Private Function Encode(value As String) As String

            If value Is Nothing Then
                value = String.Empty
            End If

            Return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(value))

        End Function

        Private Function Decode(value As String) As String

            If String.IsNullOrEmpty(value) Then
                Return String.Empty
            End If

            Try
                Return Encoding.UTF8.GetString(
                    Convert.FromBase64String(value))
            Catch
                Return String.Empty
            End Try

        End Function

    End Class

End Namespace
