Imports System
Imports System.Windows.Forms

Module Program

    <STAThread()>
    Sub Main()

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Application.Run(New FrmMain())

    End Sub

End Module