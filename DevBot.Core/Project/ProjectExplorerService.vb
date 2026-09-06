Imports System
Imports System.Collections.Generic
Imports System.IO
Imports DevBot.Models

Namespace DevBot.Core.Project

    Public Class ProjectExplorerService

        Private ReadOnly ignoredFolders As HashSet(Of String)

        Public Sub New()

            ignoredFolders =
                New HashSet(Of String)(
                    StringComparer.OrdinalIgnoreCase)

            ignoredFolders.Add(".git")
            ignoredFolders.Add(".vs")
            ignoredFolders.Add("bin")
            ignoredFolders.Add("obj")
            ignoredFolders.Add("node_modules")
            ignoredFolders.Add("vendor")

        End Sub

        Public Function LoadProject(
            projectPath As String) As DevBot.Models.ProjectNode

            If String.IsNullOrWhiteSpace(projectPath) Then

                Throw New ArgumentException(
                    "Path project tidak boleh kosong.",
                    "projectPath")

            End If

            If Not Directory.Exists(projectPath) Then

                Throw New DirectoryNotFoundException(
                    "Folder project tidak ditemukan: " &
                    projectPath)

            End If

            Dim fullPath As String =
                Path.GetFullPath(projectPath)

            Return BuildNode(fullPath)

        End Function

        Private Function BuildNode(
            path As String) As DevBot.Models.ProjectNode

            Dim directoryInfo As New DirectoryInfo(path)

            Dim node As New DevBot.Models.ProjectNode()

            node.Name =
                directoryInfo.Name

            node.FullPath =
                directoryInfo.FullName

            node.IsDirectory =
                True

            node.Children =
                New List(Of DevBot.Models.ProjectNode)()

            Dim files() As FileInfo =
                directoryInfo.GetFiles()

            Array.Sort(
                files,
                Function(a As FileInfo,
                         b As FileInfo)

                    Return StringComparer.OrdinalIgnoreCase.Compare(
                        a.Name,
                        b.Name)

                End Function)

            For Each fileInfo As FileInfo In files

                Dim fileNode As New DevBot.Models.ProjectNode()

                fileNode.Name =
                    fileInfo.Name

                fileNode.FullPath =
                    fileInfo.FullName

                fileNode.IsDirectory =
                    False

                fileNode.Children =
                    New List(Of DevBot.Models.ProjectNode)()

                node.Children.Add(
                    fileNode)

            Next

            Dim directories() As DirectoryInfo =
                directoryInfo.GetDirectories()

            Array.Sort(
                directories,
                Function(a As DirectoryInfo,
                         b As DirectoryInfo)

                    Return StringComparer.OrdinalIgnoreCase.Compare(
                        a.Name,
                        b.Name)

                End Function)

            For Each childDirectory As DirectoryInfo In directories

                If ignoredFolders.Contains(
                    childDirectory.Name) Then

                    Continue For

                End If

                Dim directoryNode As DevBot.Models.ProjectNode =
                    BuildNode(
                        childDirectory.FullName)

                node.Children.Add(
                    directoryNode)

            Next

            Return node

        End Function

    End Class

End Namespace