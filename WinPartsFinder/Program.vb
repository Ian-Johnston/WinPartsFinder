Friend Module Program

    <STAThread>
    Friend Sub Main()
        ' (High-DPI awareness is set in app.manifest.)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())
    End Sub

End Module
