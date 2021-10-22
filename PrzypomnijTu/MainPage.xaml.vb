'
' zmiany
' 2021.03 pierwsza wersja
' 2021.08:
'   * edycja Geofence
'   * ustawianie DwellTime ("zwłoka")
'   * EXIT event tylko w wersji DEBUG
'   * WinUI 2.6, i NumberBox w AddGeofence (dla Radius i DwellTime)


Public NotInheritable Class MainPage
    Inherits Page

    Private msNavigatedParam As String = ""

    Private mLadowanie As Boolean = False

    Private Async Sub uiRefresh_Click(sender As Object, e As RoutedEventArgs)
        ' WypelnPoleGdzie()

        Try

            mLadowanie = True

            Await App.gMiejsca.LoadAsync    ' zawsze wczytuje, nawet jak juz jest - bo moze Roaming jest nowszy
            If App.gMiejsca.ImportFencesFromSystem() Then    ' pewnie tylko raz to się wykorzysta, u mnie i teraz :)
                Await DialogBoxAsync("Wczytałem coś z systemu, co nie było w pliku")
                Await App.gMiejsca.SaveAsync(True)
            End If

            uiPointsList.ItemsSource = App.gMiejsca.GetList

            mLadowanie = True

        Catch ex As Exception
            CrashMessageAdd("CATCH uiRefresh_Click", ex)
        End Try

        'Await ShowZdarzenia()
        ' uiStatus.Text = "GeoMonitor status: " & Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Status.ToString

    End Sub

    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)

        If e.Parameter IsNot Nothing Then
            msNavigatedParam = e.Parameter.ToString
        Else
            msNavigatedParam = ""
        End If

    End Sub

    Private Function CreateGeofence(oMiejsce As JednoMiejsce) As Windows.Devices.Geolocation.Geofencing.Geofence

        Dim oPoint As Windows.Devices.Geolocation.BasicGeoposition = New Windows.Devices.Geolocation.BasicGeoposition
        oPoint.Latitude = oMiejsce.dLat
        oPoint.Longitude = oMiejsce.dLon

        Dim oGeocircle As Windows.Devices.Geolocation.Geocircle
        oGeocircle = New Windows.Devices.Geolocation.Geocircle(oPoint, 200) ' w metrach

        Dim oGeofence As Windows.Devices.Geolocation.Geofencing.Geofence
        ' When this constructor is used, the 
        ' MonitoredStates will default To monitor For both the Entered And Exited states, 
        ' SingleUse will default To False, 
        ' the DwellTime will default To 10 seconds, the 
        ' StartTime will default To 0 meaning start immediately, and the 
        ' Duration will default to 0, meaning forever.

        ' bardziej rozbudowany:
        ' .., states, singleUse
        ' bardziej rozbudowany:
        ' ....., dwellTime (secs)

        Dim oEventy As Windows.Devices.Geolocation.Geofencing.MonitoredGeofenceStates = Windows.Devices.Geolocation.Geofencing.MonitoredGeofenceStates.Entered
#If DEBUG Then
        oEventy = oEventy Or Windows.Devices.Geolocation.Geofencing.MonitoredGeofenceStates.Exited
#End If

        oGeofence = New Windows.Devices.Geolocation.Geofencing.Geofence(ListaMiejsc.sIdPrefix & oMiejsce.sName, oGeocircle, oEventy, False, TimeSpan.FromSeconds(oMiejsce.iZwloka))

        Return oGeofence

        ' bardziej rozbudowany:
        ' ....., startTime, duration

    End Function

    Private Sub DeleteAllOgrodzenia()
        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences IsNot Nothing Then
            oGeoMon.Geofences.Clear()
        End If
    End Sub


    Private Sub RegisterTrigger()

        Try
            UnregisterTriggers("PrzypomnijTu_")

            Dim oTrigger As Background.LocationTrigger =
            New Background.LocationTrigger(Windows.ApplicationModel.Background.LocationTriggerType.Geofence)

            Dim oTaskBuilder As Background.BackgroundTaskBuilder = New Background.BackgroundTaskBuilder()
            oTaskBuilder.SetTrigger(oTrigger)
            oTaskBuilder.Name = "PrzypomnijTu_Geofence"
            oTaskBuilder.TaskEntryPoint = "PracaTylna.OgrodzenieWTle"

            Dim oRet As Background.BackgroundTaskRegistration
            oRet = oTaskBuilder.Register()

            'AddHandler oRet.Completed, App.OnCompleted
            'RegisterBackgroundTask(BackgroundTaskState.LocationTriggerBackgroundTaskName)

            ' foreground handler
            'Try
            '    RemoveHandler Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.GeofenceStateChanged,
            '    AddressOf OnGeofenceStateChanged
            'Catch ex As Exception

            'End Try

            ' If you have set up both foreground and background geofence listeners, you should unregister your foreground event listener whenever your app is not visible to the user and re-register your app when it becomes visible again.
            ' https://docs.microsoft.com/en-us/windows/uwp/maps-and-location/guidelines-for-geofencing
            'AddHandler Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.GeofenceStateChanged,
            '        AddressOf OnGeofenceStateChanged

        Catch ex As Exception
            CrashMessageAdd("CATCH RegisterTrigger", ex)
        End Try

    End Sub

    Public Sub OnGeofenceStateChanged(sender As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor, e As Object)
        App.GeofenceEvents2Toasts(sender)
    End Sub

    Private Async Function SprawdzPermissionyGeo() As Task(Of Boolean)
        Try

            Dim accessStatus As Windows.Devices.Geolocation.GeolocationAccessStatus = Await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync
            If accessStatus = Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed Then Return True
            Await DialogBoxAsync("Nie mam permisionów geograficznych, to se idę")
        Catch ex As Exception
            CrashMessageAdd("CATCH SprawdzPermissionyGeo", ex)
        End Try

        Return False
    End Function
    Private Async Function SprawdzPermissionyBack() As Task(Of Boolean)

        Try

            Dim oStatus As Background.BackgroundAccessStatus = Await Windows.ApplicationModel.Background.BackgroundExecutionManager.RequestAccessAsync()
            Dim bDenied As Boolean = False
            If WinVer() > 14392 Then
                If oStatus = Windows.ApplicationModel.Background.BackgroundAccessStatus.DeniedBySystemPolicy Or
                    oStatus = Windows.ApplicationModel.Background.BackgroundAccessStatus.DeniedByUser Then
                    Return False
                End If
            Else
#Disable Warning BC40000 ' Type or member is obsolete
                If oStatus = Windows.ApplicationModel.Background.BackgroundAccessStatus.Denied Then
#Enable Warning BC40000 ' Type or member is obsolete
                    Return False
                End If
            End If

            Return True
        Catch ex As Exception
            CrashMessageAdd("CATCH SprawdzPermissionyBack", ex)
        End Try
        Return False
    End Function

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

        CrashMessageInit()
        Await CrashMessageShowAsync()

        ProgRingInit(True, False)
        GetAppVers(Nothing)

        If Not Await SprawdzPermissionyGeo() Then Return
        If Not Await SprawdzPermissionyBack() Then
            Await DialogBoxAsync("This app is not allowed to run in the background.")
        End If

        SetSettingsString("resOpen", "Open")    ' resource LANG; tekst na guziku OPEN na Toast

        RegisterTrigger()
        uiRefresh_Click(Nothing, Nothing)

        If Not String.IsNullOrEmpty(msNavigatedParam) Then
            ' *TODO* pokaz tekst 
            DialogBox("tekst dla eventu " & msNavigatedParam)
        End If

        If msNavigatedParam <> "" Then ShowPlaceMessage(App.gMiejsca.GetMiejsce(msNavigatedParam))

    End Sub

    Private Async Function ShowZdarzenia() As Task
        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
                    Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        Dim sReport As String = "Zdarzenia:"

        If oGeoMon.ReadReports IsNot Nothing Then
            For Each oItem As Windows.Devices.Geolocation.Geofencing.GeofenceStateChangeReport In oGeoMon.ReadReports
                sReport = sReport & vbCrLf & oItem.Geofence.Id & ": " & oItem.NewState.ToString
            Next
        End If

        sReport = sReport & vbCrLf & "Status: " & oGeoMon.Status.ToString

        Await DialogBoxAsync(sReport)

    End Function


    Private Async Sub uiAdd_Click(sender As Object, e As RoutedEventArgs)
        If App.gMiejsca.Count > 2 AndAlso Not Await IsFullVersion() Then
            DialogBox("Więcej tylko w wersji płatnej")
            Return
        End If
        Me.Frame.Navigate(GetType(AddGeofence))
    End Sub

    Public Function TryAddGeofence(oItem As JednoMiejsce) As Boolean

        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences IsNot Nothing Then
            For Each oFence As Windows.Devices.Geolocation.Geofencing.Geofence In oGeoMon.Geofences
                'dwa zabezpieczenia, którego mogłoby nie być, ale na wszelki wypadek... (nazwa, i kształt)
                If Not oFence.Id.ToUpper.StartsWith(ListaMiejsc.sIdPrefix) Then Continue For
                Dim oGeoCircle As Windows.Devices.Geolocation.Geocircle = TryCast(oFence.Geoshape, Windows.Devices.Geolocation.Geocircle)
                If oGeoCircle Is Nothing Then Continue For

                ' juz mamy takie
                If oGeoCircle.Center.DistanceTo(oItem.dLat, oItem.dLon) < 20 Then Return False
            Next
        End If

        Dim oNew As Windows.Devices.Geolocation.Geofencing.Geofence = CreateGeofence(oItem)
        oGeoMon.Geofences.Add(oNew)
        Return True
    End Function

    Private Async Function DodajUsun(bUsun As Boolean, oItem As JednoMiejsce) As Task
        If oItem Is Nothing Then Return

        ' Zakladam SYNC App.gMiejsca oraz GeofenceMonitor (bo przy wczytywaniu sie to zrobilo)

        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences Is Nothing Then Return


        If Not bUsun Then
            ' włącz monitorowanie
            TryAddGeofence(oItem)   ' z testem czy juz istnieje - bo MainPage.Loaded robi OnChecked!
        Else
            ' wyłącz
            For Each oFence As Windows.Devices.Geolocation.Geofencing.Geofence In oGeoMon.Geofences
                'dwa zabezpieczenia, którego mogłoby nie być, ale na wszelki wypadek... (nazwa, i kształt)
                If Not oFence.Id.ToUpper.StartsWith(ListaMiejsc.sIdPrefix) Then Continue For
                Dim oGeoCircle As Windows.Devices.Geolocation.Geocircle = TryCast(oFence.Geoshape, Windows.Devices.Geolocation.Geocircle)
                If oGeoCircle Is Nothing Then Continue For

                If oItem.dLat = oGeoCircle.Center.Latitude AndAlso
                                oItem.dLon = oGeoCircle.Center.Longitude Then
                    oGeoMon.Geofences.Remove(oFence)
                    Exit For
                End If
            Next
        End If


    End Function

    Private Async Sub uiHere_Checked(sender As Object, e As RoutedEventArgs)
        If mLadowanie Then Return   ' jest wlasnie robione ItemsSource , więc to do zignorowania
        Dim oCB As CheckBox = TryCast(sender, CheckBox)
        If oCB Is Nothing Then Return

        Dim oItem As JednoMiejsce = TryCast(oCB.DataContext, JednoMiejsce)
        If oItem Is Nothing Then Return

        Await DodajUsun(oCB.IsChecked, oItem)
        ' Zakladam SYNC App.gMiejsca oraz GeofenceMonitor (bo przy wczytywaniu sie to zrobilo)

        'Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
        '    Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        'If oGeoMon.Geofences Is Nothing Then Return


        'If oCB.IsChecked Then
        '    ' włącz monitorowanie
        '    TryAddGeofence(oItem)   ' z testem czy juz istnieje - bo MainPage.Loaded robi OnChecked!
        'Else
        '    ' wyłącz
        '    For Each oFence As Windows.Devices.Geolocation.Geofencing.Geofence In oGeoMon.Geofences
        '        'dwa zabezpieczenia, którego mogłoby nie być, ale na wszelki wypadek... (nazwa, i kształt)
        '        If Not oFence.Id.ToUpper.StartsWith(ListaMiejsc.sIdPrefix) Then Continue For
        '        Dim oGeoCircle As Windows.Devices.Geolocation.Geocircle = TryCast(oFence.Geoshape, Windows.Devices.Geolocation.Geocircle)
        '        If oGeoCircle Is Nothing Then Continue For

        '        If oItem.dLat = oGeoCircle.Center.Latitude AndAlso
        '                        oItem.dLon = oGeoCircle.Center.Longitude Then
        '            oGeoMon.Geofences.Remove(oFence)
        '            Exit For
        '        End If
        '    Next
        'End If



        ' zmiana w App.gMiejsca jest zrobiona przez XAML
        App.gMiejsca.MakeDirty()
        Await App.gMiejsca.SaveAsync(True)


    End Sub

    Private Sub uiHere_DTapped(sender As Object, e As DoubleTappedRoutedEventArgs)
        Dim oTB As TextBlock = TryCast(sender, TextBlock)
        If oTB Is Nothing Then Return

        Dim oItem As JednoMiejsce = TryCast(oTB.DataContext, JednoMiejsce)
        If oItem Is Nothing Then Return

        ShowPlaceMessage(oItem)
    End Sub

    Private Async Sub ShowPlaceMessage(oItem As JednoMiejsce)
        ' pokazanie/edycja tekstu dla danego miejsca
        If oItem Is Nothing Then Return

        If oItem.sRemindText Is Nothing Then oItem.sRemindText = ""

        Dim oInputTextBox = New TextBox
        oInputTextBox.AcceptsReturn = True
        oInputTextBox.Text = oItem.sRemindText
        oInputTextBox.IsSpellCheckEnabled = True
        oInputTextBox.Width = uiGrid.ActualWidth * 0.8
        oInputTextBox.TextWrapping = TextWrapping.Wrap
        oInputTextBox.MaxHeight = uiGrid.ActualHeight * 0.5
        ScrollViewer.SetVerticalScrollBarVisibility(oInputTextBox, ScrollBarVisibility.Auto)

        Dim oDlg As New ContentDialog
        oDlg.Content = oInputTextBox
        oDlg.PrimaryButtonText = "OK"
        oDlg.SecondaryButtonText = ""  ' bez tego guzika
        oDlg.Title = oItem.sName

        Dim oCmd = Await oDlg.ShowAsync
        If oCmd <> ContentDialogResult.Primary Then Return

        If oItem.sRemindText = oInputTextBox.Text Then Return

        oItem.sRemindText = oInputTextBox.Text
        Await App.gMiejsca.SaveAsync(True)

    End Sub

    Private Sub uiItemMsg_Click(sender As Object, e As RoutedEventArgs)
        Dim oTB As MenuFlyoutItem = TryCast(sender, MenuFlyoutItem)
        If oTB Is Nothing Then Return

        Dim oItem As JednoMiejsce = TryCast(oTB.DataContext, JednoMiejsce)
        If oItem Is Nothing Then Return

        ShowPlaceMessage(oItem)

    End Sub

    Private Sub uiSettings_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Settingsy))
    End Sub

    Private Async Sub uiItemRadius_Click(sender As Object, e As RoutedEventArgs)
        Dim oTB As MenuFlyoutItem = TryCast(sender, MenuFlyoutItem)
        If oTB Is Nothing Then Return

        Dim oItem As JednoMiejsce = TryCast(oTB.DataContext, JednoMiejsce)
        If oItem Is Nothing Then Return

        Dim sRadius As String = Await DialogBoxInputDirectAsync("Promień:", oItem.dRadius)
        If String.IsNullOrEmpty(sRadius) Then Return
        Dim dRadius As Double
        If Not Double.TryParse(sRadius, dRadius) Then
            DialogBox("to nie wygląda na liczbę")
            Return
        End If

        ' usun ze starym radius
        If oItem.bTutaj Then Await DodajUsun(True, oItem)

        ' a teraz dodaj z nowym
        oItem.dRadius = dRadius
        If oItem.bTutaj Then Await DodajUsun(False, oItem)

        Await App.gMiejsca.SaveAsync(True)

    End Sub

    Private Sub uiItemEdit_Click(sender As Object, e As RoutedEventArgs)
        Dim oTB As MenuFlyoutItem = TryCast(sender, MenuFlyoutItem)
        If oTB Is Nothing Then Return

        Dim oItem As JednoMiejsce = TryCast(oTB.DataContext, JednoMiejsce)
        If oItem Is Nothing Then Return

        Me.Frame.Navigate(GetType(AddGeofence), oItem.sName)
    End Sub

    Private Async Sub uiItemRename_Click(sender As Object, e As RoutedEventArgs)
        Dim oTB As MenuFlyoutItem = TryCast(sender, MenuFlyoutItem)
        If oTB Is Nothing Then Return

        Dim oItem As JednoMiejsce = TryCast(oTB.DataContext, JednoMiejsce)
        If oItem Is Nothing Then Return

        Dim sNewName As String = Await DialogBoxInputDirectAsync("Nowa nazwa:", oItem.sName)
        If String.IsNullOrEmpty(sNewName) Then Return
        If sNewName = oItem.sName Then Return

        For Each oFence As JednoMiejsce In App.gMiejsca.GetList
            If oFence.sName = sNewName Then
                DialogBox("Taka nazwa już istnieje")
                Return
            End If
        Next

        ' usun ze starym ID
        If oItem.bTutaj Then Await DodajUsun(True, oItem)

        ' a teraz dodaj z nowym ID
        oItem.sName = sNewName
        If oItem.bTutaj Then Await DodajUsun(False, oItem)

        Await App.gMiejsca.SaveAsync(True)

    End Sub

End Class

