Imports System
Imports System.Collections.Generic

Namespace DevBot.Models

    Public Class ProjectInfo

        Public Property ProjectName As String

        Public Property ProjectPath As String

        Public Property ProjectType As String

        Public Property TotalFiles As Integer

        Public Property TotalDirectories As Integer

        Public Property Languages As List(Of String)

        Public Property DetectedFiles As List(Of String)

        Public Property EntryPoint As String

        Public Property DatabaseType As String

        Public Property Files As List(Of String)

        Public Property Folders As List(Of String)

        Public Property Name As String

        Public Property RootPath As String

        Public Property OpenedAt As DateTime


        Public Sub New()

            ProjectName =
                String.Empty

            ProjectPath =
                String.Empty

            ProjectType =
                String.Empty

            TotalFiles =
                0

            TotalDirectories =
                0

            Languages =
                New List(Of String)()

            DetectedFiles =
                New List(Of String)()

            EntryPoint =
                String.Empty

            DatabaseType =
                String.Empty

            Files =
                New List(Of String)()

            Folders =
                New List(Of String)()

            Name =
                String.Empty

            RootPath =
                String.Empty

            OpenedAt =
                DateTime.Now

        End Sub


        Public Sub New(
            name As String,
            rootPath As String)

            Me.ProjectName =
                name

            Me.ProjectPath =
                rootPath

            Me.ProjectType =
                String.Empty

            Me.TotalFiles =
                0

            Me.TotalDirectories =
                0

            Me.Languages =
                New List(Of String)()

            Me.DetectedFiles =
                New List(Of String)()

            Me.EntryPoint =
                String.Empty

            Me.DatabaseType =
                String.Empty

            Me.Files =
                New List(Of String)()

            Me.Folders =
                New List(Of String)()

            Me.Name =
                name

            Me.RootPath =
                rootPath

            Me.OpenedAt =
                DateTime.Now

        End Sub

    End Class

End Namespace