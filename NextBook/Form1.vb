Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Timers
Imports System.Text
Imports System.Linq

' Possibilidades 

'          INSERIDAS:
'                   :  Nº DE LOCALIDADES
'                   :  EVALUATION YES/NO 
'                   :  ESCOLHA DO CROP PARA CAPTURA DA CAPA  
'                   :  ESCOLHA DO CROP APÓS A CAPTURA DA CAPA
'                   :  UMA SÓ APLICAÇÃO A CORRER
'                   :  DATA    + | = |  CUSTOM
'                   :  PARAR NO FIELD lOCALIDADES


Public Class NextBook


#Region "Hot Key Handler"

    Public Const ModAlt As Integer = &H1
    Public Const WmHotkey As Integer = &H312

    <DllImport("User32.dll")> _
    Public Shared Function RegisterHotKey(ByVal hwnd As IntPtr, _
                        ByVal id As Integer, ByVal fsModifiers As Integer, _
                        ByVal vk As Integer) As Integer
    End Function

    <DllImport("User32.dll")> _
    Public Shared Function UnregisterHotKey(ByVal hwnd As IntPtr, _
                        ByVal id As Integer) As Integer
    End Function

    Protected Overrides Sub WndProc(ByRef m As Windows.Forms.Message)

        If m.Msg = WmHotkey Then

            Dim id As IntPtr = m.WParam

            If id.ToString = "100" Then

                Application.Exit()


            ElseIf id.ToString = "200" Then

                StopFlag = True


            End If
        End If
        MyBase.WndProc(m)
    End Sub

#End Region

    Public Shared DoEval As Boolean
    Public Shared NLocalityLevels As Integer
    Public Shared ChoosedCoverCrop As Integer
    Public Shared ChoosedYearAdd As Integer
    Public Shared NextCrop As Integer
    Public Shared CropMode As String
    Public Shared StopFlag As Boolean
    Public Shared PauseLocation As Boolean
    Public Shared InfoString As String
    Public Shared Preview As String


    Private _t1 As Thread

    Private Enum CmdArgs
        App
        Eval
        NLocality
        Crop
        YearType
        NextCrop
        PauseLocation
        Preview
    End Enum

    Shared ReadOnly ArgID() = {"nextbook", "eval=", "nlocal=", "cover=", "year=", "nextcrop=", "pauselocal=", "prev="}

    Shared ReadOnly ArgDefault() = {"nextbook", False, 4, 1, 1, 0, False, True}



    Public ReadOnly form2 As New SplashScreen1

    Public Shared ReadOnly SplashTimer As New Timers.Timer
    Public Delegate Sub DoStuffDelegate()
    Public Sub ShowSplash()
        If Me.InvokeRequired Then
            Me.Invoke(New DoStuffDelegate(AddressOf ShowSplash))
        Else
            form2.Show()
        End If
    End Sub

    Public Sub CloseSplash()
        If Me.InvokeRequired Then
            Me.Invoke(New DoStuffDelegate(AddressOf CloseSplash))
        Else
            form2.Close()
        End If
    End Sub




    Private Sub Form1_Load_1(ByVal sender As System.Object, ByVal e As EventArgs) Handles MyBase.Load


        Try

            InfoString = "P R O B L E M ???"

            If CheckInstance() Then Application.Exit()

            RegisterHotKey(Handle, 200, 0, Keys.Enter)

            RegisterHotKey(Handle, 100, 0, Keys.Escape)

            CommandArg()

            Tests.CallWindows()

            If Tests.CheckDcam Then

                _t1 = New Thread(AddressOf DoWork)
                _t1.SetApartmentState(ApartmentState.STA)
                _t1.IsBackground = True
                _t1.Start()

            Else

                Application.Exit()

            End If

        Catch ex As Exception

            MessageBox.Show(ex.Message)
            Application.Exit()

        End Try


    End Sub


    ' 2min

    Private Shared ReadOnly AppTimer As New Timers.Timer



    Public Shared Sub AppLifeTimer()


        AddHandler AppTimer.Elapsed, New ElapsedEventHandler(AddressOf AppTimerClick)
        AppTimer.Interval = 180000 ' 3min of app life time
        AppTimer.Start()

    End Sub


    Private Shared Sub AppTimerClick(ByVal source As [Object], ByVal e As ElapsedEventArgs)

        Application.Exit()

    End Sub


    Public Function CheckInstance() As Boolean


        CheckInstance = False

        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then

            CheckInstance = True
        End If


    End Function


    Sub CommandArg()

        'Load comand line arguments to string
        Dim nbArgs() As String = System.Environment.GetCommandLineArgs()

        'get number of elements in enum cmdArgs
        Dim enumValues As Array = System.[Enum].GetValues(GetType(CmdArgs))

        ' Loop through each type element of emun cmdArgs 
        For Each argType As CmdArgs In enumValues

            'loop through each arg 
            For Each ThisArg As String In nbArgs

                ThisArg = ThisArg.ToLower

                ' find if arg is the looped arg type 
                If ThisArg.Contains(ArgID(argType)) Then

                    ' Parse arg
                    sortArg(ThisArg, argType)
                    Exit For                  ' catch only the first arg apearance, ignore the rest if present

                End If
            Next
        Next

    End Sub


    Sub sortArg(ByVal Arg As String, ByVal argType As Integer)

        Arg = Replace(Arg, ArgID(argType), "") ' Retira o id do arg

        Select Case argType 'PARSE ARGUMENTS

            Case CmdArgs.App

                DoEval = ArgDefault(CmdArgs.Eval)
                NLocalityLevels = ArgDefault(CmdArgs.NLocality)
                ChoosedCoverCrop = ArgDefault(CmdArgs.Crop)
                ChoosedYearAdd = ArgDefault(CmdArgs.YearType)

                NextCrop = ArgDefault(CmdArgs.NextCrop)
                CropMode = "none"

                PauseLocation = ArgDefault(CmdArgs.PauseLocation)

                Preview = ArgDefault(CmdArgs.Preview)

            Case CmdArgs.Eval

                If StrComp(Arg, "y") Or StrComp(Arg, "yes") Then

                    DoEval = True

                End If

            Case CmdArgs.NLocality

                If IsNumeric(CInt(Arg)) And CInt(Arg) > 1 And CInt(Arg) < 5 Then

                    NLocalityLevels = CInt(Arg)

                End If

            Case CmdArgs.Crop

                If IsNumeric(CInt(Arg)) And CInt(Arg) > 0 And CInt(Arg) < 9 Then

                    ChoosedCoverCrop = CInt(Arg)

                End If


            Case CmdArgs.YearType
                ' 
                If IsNumeric(CInt(Arg)) And CInt(Arg) >= 0 And CInt(Arg) < 4 Then

                    ChoosedYearAdd = CInt(Arg)

                End If

            Case CmdArgs.NextCrop

                If IsNumeric(CInt(Arg)) And CInt(Arg) > 0 And CInt(Arg) < 9 Then

                    NextCrop = CInt(Arg)

                End If

            Case CmdArgs.PauseLocation

                If StrComp(Arg, "y") Or StrComp(Arg, "yes") Then

                    PauseLocation = True

                End If

            Case CmdArgs.Preview

                If StrComp(Arg, "n") Or StrComp(Arg, "no") Then

                    Preview = False

                End If



        End Select

    End Sub



    Private Sub DoWork()

 
        
        Try

            AppLifeTimer() ' SET 3 MIN AS APPLICATION LIFETIME

            Clipboard.Clear()

            GetNexTitle.ToClipBoard()

            InfoString = GetNexTitle.ToString

            If DoEval Then

                GetMovin.Evaluation()

            Else

                GetMovin.FolderList()

            End If


            GetMovin.FolderInfo()


            ShowSplash()


            GetMovin.CaptureScreen()

            Thread.Sleep(5000)


            Application.Exit()


        Catch abortException As ThreadAbortException

            Application.Exit()

        Catch ex As Exception

            MessageBox.Show(ex.Message)
            Application.Exit()

        End Try

    End Sub

End Class


Public Class GetNexTitle


    Shared Sub ToClipBoard()

        Try
            Dim aux(), previousBook, nextBook, lastParcel, numPart, letterPart As String
            Dim entry As Byte

            Dim p() As Process = Process.GetProcessesByName("DCam")


            aux = p(0).MainWindowTitle.Split("•")


            If aux.GetUpperBound(0) <> 2 Then Application.Exit()

            previousBook = aux(1)

            aux = aux(1).Split("-")


            entry = aux.Length
            lastParcel = aux(entry - 1)


            If Not IsNumeric(lastParcel(0)) Then
                numPart = Right(lastParcel, lastParcel.Length - 1)
                letterPart = lastParcel(0)
            Else

                numPart = lastParcel
                letterPart = ""

            End If


            aux((entry - 1)) = letterPart + Right(numPart + 100000001, numPart.Length - 1)

            nextBook = Trim(Join(aux, "-"))

            My.Computer.Clipboard.SetText(nextBook)


        Catch ex As Exception

        End Try
        

    End Sub


    Shared Function ToString() As String
        Try
            Dim titleString = My.Computer.Clipboard.GetText()
            Return titleString

        Catch ex As Exception

        End Try
        
    End Function

End Class


Public Class GetMovin

    Shared ReadOnly VolumeDesig() = {278, 278}

    Shared ReadOnly ClickYear() = {275, 374}
    Shared ReadOnly ClickYear1() = {407, 373}
    Public Shared ReadOnly LocalPos() = {500, 300}

    Shared ReadOnly NLocalityPos(,) = {
    {375, 624},
    {375, 651},
    {375, 678},
    {375, 706},
    {1026, 517}}

    Shared ReadOnly AllLocalities = {500, 593}

    Shared ReadOnly Crop() = {"", "+{F1}", "+{F2}", "+{F3}", "+{F4}", "+{F5}", "+{F6}", "+{F7}", "+{F8}"}


    Shared Sub FolderList()


        '''''''''''''''''''''''' IN FOLDERS LIST

        'GO TO FOLDERS LIST
        AutoItX.AU3_MouseClick("left", 1148, 37, 1, 0)

        Tests.TestFolderListFrame()
         
        'Create Folder
        AutoItX.AU3_Send("^n", 0)

    End Sub



    Shared Sub FolderInfo() ''''''''''''''''' IN FOLDER INFORMATION  

        Dim auxPos = {0, 0}

        If NextBook.DoEval = False Then

            Tests.TestFolderInfoBar()

            'USE INFORMATION FROM FOLDER
            AutoItX.AU3_Send("{DOWN}", 0)
        End If


        AutoItX.AU3_Sleep(500)

        AutoItX.AU3_Send("{TAB 4}", 0)

        'RIGHT CLICK ON VOLUME DESIGNATIO and NPASTE NEXT BOOK FROM CLIPBOARD INFO
        AutoItX.AU3_MouseClick("left", VolumeDesig(0), VolumeDesig(1), 2, 0)
        AutoItX.AU3_Send("^a", 0)
        AutoItX.AU3_Send("^v", 0)
     

        'WAIT FOR THE VD FIELD TO BE READY
        Tests.MouseCatch(Tests.IDC_STANDARD_CURSORS.IDC_IBEAM)


        'GO TO DATE FIELD
        AutoItX.AU3_Send("{TAB 4}", 0)

        'DO YEAR CASE
        YearField()


        AutoItX.AU3_Sleep(100)

        'RIGHT CLICK ON ARCHIVE REFERENCE NEXT BOOK FROM CLIPBOARD INFO
        AutoItX.AU3_MouseClick("left", 296, 494, 2, 0)
        AutoItX.AU3_Send("^a", 0)
        AutoItX.AU3_Send("^v", 0)


        If NextBook.PauseLocation = True Then

            For i As Integer = 0 To NextBook.NLocalityLevels - 2

                'Click and Wait for input in pretend locality Level
                PauseMode({NLocalityPos(i, 0), NLocalityPos(i, 1)})
                AutoItX.AU3_Sleep(300)
                'Go to main Locality Field
                AutoItX.AU3_MouseClick("left", AllLocalities(0), AllLocalities(1), 1, 0)

                ' go to the next locality field
                For ii = 0 To i + 1
                    AutoItX.AU3_Send("{TAB 1}", 0)
                Next

            Next

        End If
        


        ' CLICK SAVE AND CAPTURE '<"[Alt+c]"> 
        AutoItX.AU3_Send("!c", 0)
        Thread.Sleep(500)
        AutoItX.AU3_Send("n", 0)
        Thread.Sleep(200)
        AutoItX.AU3_Send("s", 0)

        Thread.Sleep(800)



    End Sub



    Shared Sub Evaluation()


        ' select "evaluate"
        'AutoItX.AU3_Send("!e", 0)
        AutoItX.AU3_MouseClick("left", 134, 67, 1, 0)

        Thread.Sleep(500)

        ' start "evaluate"
        AutoItX.AU3_Send("{SPACE}", 0)

        ' Test if Eval is finished
        Tests.TestEndEval()

        Thread.Sleep(500)

        'SELECT CREATE NEW FOLDER FROM THIS FOLDER INFO
        AutoItX.AU3_Send("!n", 0)

        'CHOOSE CREATE A NEW FOLDER AND PRESS ENTER
        AutoItX.AU3_Send("o", 0)

    End Sub


    Shared Sub CaptureScreen()

        Tests.TestDeleteFrame()
        AutoItX.AU3_Sleep(300)

        'CHANGE CROP TEMPLATE TO FRONT COVER <"[Shift+F1]">>
        AutoItX.AU3_Send(Crop(NextBook.ChoosedCoverCrop), 0)
        AutoItX.AU3_Sleep(200)
        'CAPTURE COVER'<"<F2>">
        AutoItX.AU3_Send("{F2}", 0)

        AutoItX.AU3_Sleep(1000)


        
        ''CHANGE CROP 
        If NextBook.NextCrop <> 0 Then

            Tests.TestThumbSelection()

            AutoItX.AU3_Send(Crop(NextBook.NextCrop), 0)


        End If

        If NextBook.Preview Then
            ShowPreview()
        End If

    End Sub


    Shared Sub YearField()



        If NextBook.ChoosedYearAdd = 0 Then
            AutoItX.AU3_Send("{TAB 8}", 0)
            'PRESSS SPACE FOR CONSTANT DATE
            AutoItX.AU3_Send("{SPACE}", 0)
        End If

        If NextBook.ChoosedYearAdd = 1 Then
            'CURSOR UP TO CHANGE DATE +1
            AutoItX.AU3_Send("{UP}", 0) ' UP ONE LEVEL
        End If

        If NextBook.ChoosedYearAdd = 2 Or NextBook.ChoosedYearAdd = 3 Then

            'WAIT UNTIL ENTER PRESS
            PauseMode(ClickYear)
            AutoItX.AU3_MouseClick("left", ClickYear(0), ClickYear(1), 1, 0)

            If NextBook.ChoosedYearAdd = 3 Then

                PauseMode(ClickYear1)
                AutoItX.AU3_MouseClick("left", ClickYear(0), ClickYear(1), 1, 0)


            End If

            'check if data check box doesn't have a tick mark 
            If Tests.DateCheckMark = AutoItX.AU3_PixelGetColor(257, 397) Then
                AutoItX.AU3_MouseClick("left", 210, 403, 1, 0)
            End If

        End If



    End Sub


    Shared Sub PauseMode(ByVal position() As Object)

        AutoItX.AU3_MouseClick("left", position(0), position(1), 2, 0)
        AutoItX.AU3_Send("^a", 0)
        Tests.PauseInfo()

    End Sub




    Shared Sub ShowPreview()

        AutoItX.AU3_Send("!v", 0)
        Thread.Sleep(100)
        AutoItX.AU3_Send("v", 0)
        AutoItX.AU3_Send("p", 0)
    End Sub



End Class


Public Class Tests

    Const WinEndEvalTitle = "Evaluation"
    Shared ReadOnly BarInFolderInfo() = {10, 200}
    Private Const ColorBarInFolderInfo As Integer = 16446177

    Shared ReadOnly ThumbSelPos() = {342, 839}
    Shared ReadOnly ThumbSelColor = 3381759

    Public Shared ReadOnly DateCheckMark = 15918018

    Shared ReadOnly ListingFramePos() = {42, 66}
    Private Const ColorListingFrame As Integer = 16446177


    Shared ReadOnly DeleteFramePos() = {21, 844}
    Shared ReadOnly DeleteFrameColor = 14016485



    '==>TestEndEval
    Shared Sub TestEndEval()

        Try

            AutoItX.AU3_Opt("WinTitleMatchMode", 2)
            AutoItX.AU3_Opt("WinSearchChildren", 1)

            While 1

                Thread.Sleep(500)

                If AutoItX.AU3_WinGetState(WinEndEvalTitle, "") = 15 Then
                    Exit While
                End If


            End While

            Thread.Sleep(1000)

        Catch ex As Exception


        End Try

    End Sub


    '==>CallWindows
    Public Shared Sub CallWindows()

        Try

            Dim winTitle As New StringBuilder()

            AutoItX.AU3_Opt("WinSearchChildren", 0)
            AutoItX.AU3_Opt("WinTitleMatchMode", 2)

            winTitle.Length = 300

            AutoItX.AU3_WinGetTitle("DCam", "", winTitle, 256)

            If winTitle.ToString() = "0" Then

                MsgBox("DCam window not found!", vbCritical + vbOKOnly + vbSystemModal)


            Else
                AutoItX.AU3_WinActivate(winTitle.ToString(), "") 'call dCamX window to front

            End If

        Catch ex As Exception

        End Try

    End Sub


    '==>Test Folder Info Bar
    Shared Sub TestFolderInfoBar()

        Try

            While 1

                Thread.Sleep(300)

                If (ColorBarInFolderInfo = AutoItX.AU3_PixelGetColor(BarInFolderInfo(0), BarInFolderInfo(1))) Then

                    Exit While

                End If


            End While

            Thread.Sleep(300)

        Catch ex As Exception


        End Try


    End Sub




    '==>Test Folder Listings Frame
    Shared Sub TestFolderListFrame()

        Try

            While 1

                Thread.Sleep(300)

                If (ColorListingFrame = AutoItX.AU3_PixelGetColor(ListingFramePos(0), ListingFramePos(1))) Then

                    Exit While

                End If


            End While

            Thread.Sleep(200)

        Catch ex As Exception


        End Try


    End Sub



    '==>Test Folder Listings Frame
    Shared Sub TestDeleteFrame()

        Try

            While 1

                Thread.Sleep(300)

                If (DeleteFrameColor = AutoItX.AU3_PixelGetColor(DeleteFramePos(0), DeleteFramePos(1))) Then

                    Exit While

                End If


            End While

            Thread.Sleep(200)

        Catch ex As Exception


        End Try


    End Sub




    '==>Test Folder Info Bar
    Shared Sub TestThumbSelection()

        Try

            While 1

                Thread.Sleep(300)

                If (ThumbSelColor = AutoItX.AU3_PixelGetColor(ThumbSelPos(0), ThumbSelPos(1))) Then

                    Exit While

                End If


            End While


        Catch ex As Exception


        End Try


    End Sub


    Const DcamWin = "DCam"
    Shared Function CheckDcam() As Boolean

        Dim existDcam As Boolean = False
        Dim winTitle As New StringBuilder()

        AutoItX.AU3_Opt("WinTitleMatchMode", 2)

        winTitle.Length = 300

        If AutoItX.AU3_WinGetState(DcamWin, "") = 47 Then
            existDcam = True
        End If

        Return existDcam

    End Function



    Shared Sub PauseInfo()

        NextBook.StopFlag = False
        While NextBook.StopFlag = False

            Thread.Sleep(500)

        End While

    End Sub


    <StructLayout(LayoutKind.Sequential)> _
    Private Structure POINTAPI
        Public x As Int32
        Public y As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure CURSORINFO
        Public cbSize As Int32
        Public flags As Int32
        Public hCursor As IntPtr
        Public ptScreenPos As POINTAPI
    End Structure

    <DllImport("user32.dll", EntryPoint:="GetCursorInfo")> _
    Private Shared Function GetCursorInfo(ByRef pci As CURSORINFO) As Boolean
    End Function

    <DllImport("user32.dll")> _
    Private Shared Function LoadCursor(ByVal hInstance As IntPtr, ByVal lpCursorName As Integer) As IntPtr
    End Function

    Public Enum IDC_STANDARD_CURSORS As Integer
        IDC_ARROW = 32512
        IDC_IBEAM = 32513
        IDC_WAIT = 32514
        IDC_CROSS = 32515
        IDC_UPARROW = 32516
        IDC_SIZE = 32640
        IDC_ICON = 32641
        IDC_SIZENWSE = 32642
        IDC_SIZENESW = 32643
        IDC_SIZEWE = 32644
        IDC_SIZENS = 32645
        IDC_SIZEALL = 32646
        IDC_NO = 32648
        IDC_HAND = 32649
        IDC_APPSTARTING = 32650
        IDC_HELP = 32651
    End Enum

    Private Shared _hWnd As IntPtr
    Private Shared _currentPointer As IntPtr
    Private Shared ReadOnly MouseTimer As New Timers.Timer()
    Private Shared _tickCounter As Integer
    Private Const MaxTickCounter As Integer = 1200
    ' 2min


    Public Shared Sub MouseCatch(ByVal mousepointer As Integer)

        ' Get handles to windows cursors
        _hWnd = LoadCursor(IntPtr.Zero, mousepointer)

        '' start timer / monitor cursor
        _tickCounter = 0
        AddHandler MouseTimer.Elapsed, New ElapsedEventHandler(AddressOf MouseTimerTick)
        MouseTimer.Interval = 100
        MouseTimer.Start()

        While 1

            If _currentPointer = _hWnd Then
                MouseTimer.Enabled = False
                MouseTimer.Stop()
                Exit Sub

            End If

            If _tickCounter > MaxTickCounter Then
                MouseTimer.Enabled = False
                MouseTimer.Stop()

                Application.Exit()

            End If

        End While


    End Sub


    Private Shared Sub MouseTimerTick(ByVal source As [Object], ByVal e As ElapsedEventArgs)

        Dim cursorNfo As New CURSORINFO()
        _tickCounter = +_tickCounter
        cursorNfo.cbSize = Marshal.SizeOf(cursorNfo)

        If GetCursorInfo(cursorNfo) Then

            If cursorNfo.flags = 1 Then

                _currentPointer = cursorNfo.hCursor

            End If

        End If


    End Sub



End Class
