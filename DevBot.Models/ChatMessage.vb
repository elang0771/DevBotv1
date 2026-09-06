Imports System

Namespace DevBot.Models

    Public Class ChatMessage

        Public Property Role As String

        Public Property Content As String

        Public Property CreatedAt As DateTime

        Public Sub New()
            CreatedAt = DateTime.Now
        End Sub

        Public Sub New(role As String, content As String)
            Me.Role = role
            Me.Content = content
            Me.CreatedAt = DateTime.Now
        End Sub

    End Class

End Namespace