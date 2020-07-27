Imports System.Timers

Public NotInheritable Class SplashScreen1

    'TODO: This form can easily be set as the splash screen for the application by going to the "Application" tab
    '  of the Project Designer ("Properties" under the "Project" menu).

    Public Shared ReadOnly SplashTimer As New Timers.Timer


    Private Sub SplashScreen1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Set up the dialog text at runtime according to the application's assembly information.  

        'TODO: Customize the application's assembly information in the "Application" pane of the project 
        '  properties dialog (under the "Project" menu).
        TopMost = True
        ApplicationTitle.Text = NextBook.InfoString
        Location = New Point(350, 100)

        BackColor = Color.White
        TransparencyKey = BackColor
        ApplicationTitle.BackColor = TransparencyKey


    End Sub




    Private Sub MainLayoutPanel_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles MainLayoutPanel.Paint

    End Sub
End Class
