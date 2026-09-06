Imports System
Imports System.Collections.Generic

Namespace DevBot.Models

    Public Class ProjectInfo

        Public Sub New()

            Name = String.Empty
            ProjectType = String.Empty
            RootPath = String.Empty

            Folders =
                New List(Of String)()

            Files =
                New List(Of String)()

        End Sub

        Public Property Name As String

        Public Property ProjectType As String

        Public Property RootPath As String

        Public Property Folders As List(Of String)

        Public Property Files As List(Of String)

    End Class


    Public Class ProjectNode

        Public Sub New()

            Name = String.Empty
            FullPath = String.Empty
            IsDirectory = False

            Children =
                New List(Of ProjectNode)()

        End Sub

        Public Sub New(
            name As String,
            fullPath As String,
            isDirectory As Boolean)

            Me.Name = name
            Me.FullPath = fullPath
            Me.IsDirectory = isDirectory

            Children =
                New List(Of ProjectNode)()

        End Sub

        Public Property Name As String

        Public Property FullPath As String

        Public Property IsDirectory As Boolean

        Public Property Children As List(Of ProjectNode)

    End Class

End Namespace