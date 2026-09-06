Imports System
Imports System.Collections.Generic

Namespace DevBot.Models

    Public Class ProjectNode

        Public Property Name As String

        Public Property FullPath As String

        Public Property IsDirectory As Boolean

        Public Property Children As List(Of ProjectNode)

        Public Sub New()

            Children =
                New List(Of ProjectNode)()

        End Sub

        Public Sub New(
            name As String,
            fullPath As String,
            isDirectory As Boolean)

            Me.Name =
                name

            Me.FullPath =
                fullPath

            Me.IsDirectory =
                isDirectory

            Children =
                New List(Of ProjectNode)()

        End Sub

    End Class

End Namespace