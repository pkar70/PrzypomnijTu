NotInheritable Class App
    Inherits Application

#Region "wizard"
    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        ' bo wspolne z OnActivated (toast)
        Dim mRootFrame As Frame = OnLaunchFragment(e.PreviousExecutionState)

        If e.PrelaunchActivated = False Then
            Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub
#End Region

    Protected Function OnLaunchFragment(aes As ApplicationExecutionState) As Frame
        Dim mRootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If mRootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            mRootFrame = New Frame()

            AddHandler mRootFrame.NavigationFailed, AddressOf OnNavigationFailed

            ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
            AddHandler mRootFrame.Navigated, AddressOf OnNavigatedAddBackButton
            AddHandler Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested, AddressOf OnBackButtonPressed

            ' Place the frame in the current Window
            Window.Current.Content = mRootFrame
        End If

        Return mRootFrame
    End Function

    ' wedle https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast
    ' foreground activation
    ' CommandLine, Toasts
    Protected Overrides Async Sub OnActivated(args As IActivatedEventArgs)
        ' to jest m.in. dla Toast i tak dalej?

        ' próba czy to commandline
        If args.Kind = ActivationKind.CommandLineLaunch Then

            Dim commandLine As CommandLineActivatedEventArgs = TryCast(args, CommandLineActivatedEventArgs)
            Dim operation As CommandLineActivationOperation = commandLine?.Operation
            Dim strArgs As String = operation?.Arguments

            If Not String.IsNullOrEmpty(strArgs) Then
                Await ObsluzCommandLine(strArgs)
                Window.Current.Close()
                Return
            End If
        End If

        ' jesli nie cmdline (a np. toast), albo cmdline bez parametrow, to pokazujemy okno
        Dim rootFrame As Frame = OnLaunchFragment(args.PreviousExecutionState)

        If args.Kind = ActivationKind.ToastNotification Then
            Dim oToastAct As ToastNotificationActivatedEventArgs
            oToastAct = TryCast(args, ToastNotificationActivatedEventArgs)
            If oToastAct IsNot Nothing Then
                Dim sArgs As String = oToastAct.Argument
                Select Case sArgs.Substring(0, 4)
                    Case "OPEN"
                        ' było skomplikowane, ale przecież zawsze ma pójść do MainPage, prawda?
                        'If rootFrame.Content Is Nothing Then
                        rootFrame.Navigate(GetType(MainPage), sArgs)
                        'Else
                        '    CrashMessageAdd("OnActivated - OPEN not null", "")
                        'End If
                End Select
            End If
            rootFrame.Navigate(GetType(MainPage))
        End If

        If args.Kind = ActivationKind.Protocol Then
            Dim argsProt As ProtocolActivatedEventArgs = args
            rootFrame.Navigate(GetType(MainPage), argsProt.Uri)
        End If

        Window.Current.Activate()

    End Sub


    Public Shared gMiejsca As ListaMiejsc = New ListaMiejsc(Windows.Storage.ApplicationData.Current.RoamingFolder.Path)

    Shared Sub GeofenceEvents2Toasts(oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor)

        If oGeoMon.ReadReports IsNot Nothing Then
            For Each oItem As Windows.Devices.Geolocation.Geofencing.GeofenceStateChangeReport In oGeoMon.ReadReports
                MakeToast(oItem.Geofence.Id, oItem.NewState.ToString)
            Next
        Else
            MakeToast("OnBackgroundActivated called, ale oGeoMon.ReadReports = null?")
        End If


    End Sub

    Private Shared Function AppServiceLocalCommand(sCommand As String)
        Return ""
    End Function

    Public Shared Async Function GetCurrentPoint(iTimeoutSecs As Integer) As Task(Of Windows.Devices.Geolocation.BasicGeoposition)
        DumpCurrMethod()

        Dim rVal As Windows.Devices.Geolocation.GeolocationAccessStatus = Await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync()
        If rVal <> Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed Then
            Await DialogBoxAsync("resErrorNoGPSAllowed")
            Return GetDomekGeopos(1)
        End If

        Dim oDevGPS As Windows.Devices.Geolocation.Geolocator = New Windows.Devices.Geolocation.Geolocator()

        oDevGPS.DesiredAccuracyInMeters = GetSettingsInt("gpsPrec", 75) ' dla 4 km/h; 100 m = 90 sec, 75 m = 67 sec
        Dim oCacheTime As TimeSpan = New TimeSpan(0, 1, 0)  ' minuta ≈ 80 m (ale nie autobusem! wtedy 400 m)
        Dim oTimeout As TimeSpan = New TimeSpan(0, 0, iTimeoutSecs)

        Try
            Dim oPos As Windows.Devices.Geolocation.Geoposition = Await oDevGPS.GetGeopositionAsync(oCacheTime, oTimeout)
            Return oPos.Coordinate.Point.Position
        Catch ex As Exception   ' zapewne timeout
        End Try

        Await DialogBoxAsync("resErrorGettingPos")
        Return GetDomekGeopos(1)

    End Function


    'Protected Overrides Sub OnBackgroundActivated(args As BackgroundActivatedEventArgs)
    '    ' tile update / warnings
    '    Dim oTimerDeferal As Background.BackgroundTaskDeferral
    '    oTimerDeferal = args.TaskInstance.GetDeferral()

    '    Select Case args.TaskInstance.Task.Name
    '        Case "PrzypomnijTu_Geofence"
    '            Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
    '                Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current
    '            GeofenceEvents2Toasts(oGeoMon)
    '    End Select

    '    oTimerDeferal.Complete()

    'End Sub

    'Public Shared Sub OnCompleted(sender As Background.IBackgroundTaskRegistration,
    '                                    e As Background.BackgroundTaskCompletedEventArgs)

    '    If sender Is Nothing Then Return

    '    ' Update the UI with progress reported by the background task.
    '    ' Await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () >=
    '    ' {
    '    Try
    '        ' If the background task threw an exception, display the exception in
    '        ' the error text box.
    '        e.CheckResult()
    '        MakeToast("cosdostalom")
    '    Catch ex As Exception
    '        ' // The background task had an error.
    '        MakeToast("dostalem ale error")
    '    End Try
    'End Sub
End Class
