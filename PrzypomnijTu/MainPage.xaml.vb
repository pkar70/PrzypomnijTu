
' moje pomysły:
' * wysłanie miejsca (a nie zadania)? "zsynchronizujmy sobie miejsca"

' posiedzenie Wojtek+Magda+ja
' * termin ważności przypomnienia, i larum że nie odklikane/potwierdzone
' * edycja POI w OSM?
' * punkt z mapy - to Biedronka, jest wiele innych, dowolna niekoniecznie ta
' * link z miejsca dokądś (np. z POI do ich strony)
' * auto wyłączanie punktów odległych ≥ ode mnie
' * potwierdzenie wykonania - uwaga! jak wysłane do kilku osób
' * do zrobienia nie jedna rzezc, a lista (np. zakupów), kilka osób dostało, każdy kupi inny kawałek
' * - a na razie blokada wysłania do więcej niż jednego?
' * wysyłanie SMS po dodaniu zadania dla miejsca
' * przychodzi SMS - akceptacja/odrzucenie, SMSem odpowiedź
' * dotarcie do statusu SMS, sent/delivered/read
' * MAUI status sms (jest iOS, Android, UWP), geofence (nie ma), i mapy
' * czas od pierwszej instalacji?
' * import/export danych (miejsc) z opisami, zwłaszcza jak większymi listami (np. co do zwiedzenia w Rzymie)
' * limit geofences dla Windows (no limit, ale spada wydajność), Android (100), iOS (20)
' * settings:defaultUserName, na żądanie wypełnienie numerem telefonu (gdy są permission)?
' * z przyptu://id= oznacza przejście przez bazę
' * gdy z przyptu://d=s=..., to możliwość zmiany tekstu (można sobie wpisać do kogo)

' scenariusz: jak tu będę, to warto zrobić zdjęcie, z tego samego miejsca w 4 pory roku
' scenariusz: kupić w OBI jak tam będę (lista)
' scenariusz: będę u rodziców, pożyczyć wiertarkę
' scenariusz: będziesz w Rzymie w okolicy, to wpadnij tu a tu bo dobrze dają jeść
' scenariusz: lista punktów do zobaczenia z opisami

' link do app UWP Shopping List?

' może MAPSUI, ale 1.4.8, bo nowsze nie pójdą na telefon

' 2022.01.11
' * próba zrobienia cosmosdb (przeniesienie z GitHub.C# tutaj do VB) - za dużo jednak roboty, by tak robić do testów tylko
'           - przecież Azure trzeba byłoby kupić; a poza tym wersja VB jest tylko na Windows, i to telefon - bo desktop będzie miał nowsze MinTarget
' * a więc robię bazę w oparciu o mój publiczny IP, WWW/ASP i mój SQL (dbase_pkSQLasp)
' * settings:defaultUserName, jako informacja o wysyłaczu, to można wypełniać nawet na żądanie numerem telefonu (jak będą permissiony do tego?)

' 2022.01.10
' * wysyłanie via SMS (default dla Mobile), albo via Email. Przełączalne w Settings (czyli można z desktop)
' * SearchPOI, a tak naprawdę nominatim.osm, max. 10 punktów z nazwą - ale to wymagało włączenia Internet dla tego!

' 2022.01.08
' * BUG nie było default action w Toast, więc nie było OPENid (param=empty).
'       * w OgrodzenieWTle zmieniłem w MakeToast (do XML default action)
'       * w MainPage dodałem kontrolę uruchomienia

' 2022.01.03
' * max mapy jak jest aktywna, z wygaszaniem niepotrzebnych pól (zostaje tylko Name)
' * Main:Setup:Debug jako podstrona, (view/del) (sys/app) list
' * BUG MainPage:Check/uncheck - była blokada na mLadowanie, ale tenże znacznik nie był wyłączany :)

' 2022.01.02
' * przerzuciłem trochę z MainPage.vb do typki.vb (czasem dodając shared)
' * poprawka w obsłudze zaznaczania checkbox na liście
' * MainPage: z protocolActivation, jeśli nowe miejsce, to dodaje do listy i do systemu

' 2022.01.01
' * AddPoint (bug) zanim powie że już jest nazwa, to sprawdza czy w ogóle mamy taki punkt (brakowało jednego IF)
' * Settings: TextBlock z wartością, oraz rozrysowana doba
' * Settings: (bug) można nie tylko dane zapisać ale i wraca (po zapisaniu) do MainPage
' * MainPage: contextMenu "edit" -> "edit geofence" (zeby nie było wątpliwośći)
' * MainPage: contextMenu delete punktu (z listy i systemu)
' * Settings, guzik Debug - podaje aktualne odległości od wszystkich fences
' * MainPage: contextMenu "SMS" - compose SMS z prymitywnym tekstem
' * App: protocol activation (przyptu://?s=&d= , jak szerokość i długość; bez ? wylatuje błąd w Uri na &)


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

            App.gMiejsca.Load()    ' zawsze wczytuje, nawet jak juz jest - bo moze Roaming jest nowszy
            If App.gMiejsca.ImportFencesFromSystem() Then    ' pewnie tylko raz to się wykorzysta, u mnie i teraz :)
                Await DialogBoxAsync("Wczytałem coś z systemu, co nie było w pliku")
                App.gMiejsca.Save(True)
            End If

            uiPointsList.ItemsSource = App.gMiejsca.GetList

            mLadowanie = False

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

    'Private Sub DeleteAllOgrodzenia()
    '    Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
    '        Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

    '    If oGeoMon.Geofences IsNot Nothing Then
    '        oGeoMon.Geofences.Clear()
    '    End If
    'End Sub


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
        Me.ShowAppVers()

        If Not Await SprawdzPermissionyGeo() Then Return
        If Not Await SprawdzPermissionyBack() Then
            Await DialogBoxAsync("This app is not allowed to run in the background.")
        End If

        SetSettingsString("resOpen", "Open")    ' resource LANG; tekst na guziku OPEN na Toast

        RegisterTrigger()
        uiRefresh_Click(Nothing, Nothing)   ' tu jest wczytanie listy

        If Not String.IsNullOrEmpty(msNavigatedParam) Then
            If msNavigatedParam.StartsWith("OPEN") Then
                If msNavigatedParam.Length < 5 Then
                    Await DialogBoxAsync("FAIL: call from Toast without Fence ID")
                    Return
                End If
                Dim sFenceId As String = msNavigatedParam.Substring(4)
                Await DialogBoxAsync("niby z Toast, = " & msNavigatedParam)
                ShowPlaceMessage(App.gMiejsca.GetMiejsce(msNavigatedParam.Substring(4)))
            ElseIf msNavigatedParam.ToLower.StartsWith("przyptu") Then
                Await ProtocolActivatedEventArgs(msNavigatedParam)
            End If
        End If

        msNavigatedParam = ""   ' ponowne Page_Load nie bedzie się powtarzać (tym komunikatem)
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

    Private Function MiejsceFromSender(sender As Object) As JednoMiejsce
        Dim oCB As FrameworkElement = TryCast(sender, FrameworkElement)
        If oCB Is Nothing Then Return Nothing

        Return TryCast(oCB.DataContext, JednoMiejsce)

    End Function

    Private Sub uiHere_Checked(sender As Object, e As RoutedEventArgs)
        DumpCurrMethod()
        If mLadowanie Then Return   ' jest wlasnie robione ItemsSource , więc to do zignorowania
        Dim oCB As CheckBox = TryCast(sender, CheckBox)
        If oCB Is Nothing Then Return

        Dim oItem As JednoMiejsce = MiejsceFromSender(sender)
        If oItem Is Nothing Then Return

        App.gMiejsca.DodajUsunSystem(Not oCB.IsChecked, oItem)

        ' zmiana w App.gMiejsca jest zrobiona przez XAML
        App.gMiejsca.MakeDirty()
        App.gMiejsca.Save(True)

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
        If oItem Is Nothing Then
            DialogBox("nie moge znalezc miejsca do którego trafiłeś")
            Return
        End If

        Await DialogBoxAsync("msg dla miejsca")

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
        App.gMiejsca.Save(True)

    End Sub

    Private Sub uiItemMsg_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem As JednoMiejsce = MiejsceFromSender(sender)
        If oItem Is Nothing Then Return

        ShowPlaceMessage(oItem)

    End Sub

    Private Sub uiSettings_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Settingsy))
    End Sub

    Private Async Sub uiItemRadius_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem As JednoMiejsce = MiejsceFromSender(sender)
        If oItem Is Nothing Then Return

        Dim sRadius As String = Await DialogBoxInputDirectAsync("Promień:", oItem.dRadius)
        If String.IsNullOrEmpty(sRadius) Then Return
        Dim dRadius As Double
        If Not Double.TryParse(sRadius, dRadius) Then
            DialogBox("to nie wygląda na liczbę")
            Return
        End If

        ' usun ze starym radius
        If oItem.bTutaj Then App.gMiejsca.DodajUsunSystem(True, oItem)

        ' a teraz dodaj z nowym
        oItem.dRadius = dRadius
        If oItem.bTutaj Then App.gMiejsca.DodajUsunSystem(False, oItem)

        App.gMiejsca.Save(True)

    End Sub

    Private Sub uiItemEdit_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem As JednoMiejsce = MiejsceFromSender(sender)
        If oItem Is Nothing Then Return

        Me.Frame.Navigate(GetType(AddGeofence), oItem.sName)
    End Sub

    Private Async Sub uiItemRename_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem As JednoMiejsce = MiejsceFromSender(sender)
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
        If oItem.bTutaj Then App.gMiejsca.DodajUsunSystem(True, oItem)

        ' a teraz dodaj z nowym ID
        oItem.sName = sNewName
        If oItem.bTutaj Then App.gMiejsca.DodajUsunSystem(False, oItem)

        App.gMiejsca.Save(True)

    End Sub

    Private Async Sub uiDelete_Click(sender As Object, e As RoutedEventArgs)

        Dim oItem As JednoMiejsce = MiejsceFromSender(sender)
        If oItem Is Nothing Then Return

        ' dopiero po sprawdzeniu że rzeczywiście jest co usuwać
        If Not Await DialogBoxYNAsync("Na pewno usunąć i z app i z systemu?") Then Return

        App.gMiejsca.RemoveFromMonitor(oItem)
        App.gMiejsca.Remove(oItem)

        uiRefresh_Click(Nothing, Nothing)

    End Sub

    Private Async Sub uiSendSMS_Click(sender As Object, e As RoutedEventArgs)

        'If Not Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.ApplicationModel.Chat") Then
        '    DialogBox("Sorry, tu nie ma SMSów")
        '    Return
        'End If

        Dim oItem As JednoMiejsce = MiejsceFromSender(sender)
        If oItem Is Nothing Then Return


        Dim sTxt As String = "Zadzwon jak bedziesz tu przyptu://?s=" & oItem.dLat.ToString("#0.0000") & "&d=" & oItem.dLon.ToString("#0.0000") & "&r=" & oItem.dRadius.ToString("###0")

        If GetSettingsBool("useSMS", IsFamilyMobile) Then
            Dim oSMS As New Windows.ApplicationModel.Chat.ChatMessage

            oSMS.Body = sTxt

            ' Windows.ApplicationModel.Chat.ChatMessageManager.ShowSmsSettings()
            Await Windows.ApplicationModel.Chat.ChatMessageManager.ShowComposeSmsMessageAsync(oSMS)
        Else
            Dim oMsg As Email.EmailMessage = New Windows.ApplicationModel.Email.EmailMessage()
            oMsg.Subject = "Proszę zadzwoń"
            oMsg.Body = sTxt
            Await Email.EmailManager.ShowComposeNewEmailAsync(oMsg)
        End If

    End Sub

    Private Async Function ProtocolActivatedEventArgs(msNavigatedParam As String) As Task

        Dim oUri As Uri = New Uri(msNavigatedParam)
        If oUri.Scheme.ToLower <> "przyptu" Then
            DialogBox("protocolActivation: scheme error  " & msNavigatedParam)
        End If


        Dim dLat As Double = 100
        Dim dLon As Double = -1
        Dim dRad As Double = 0

        ' System.Web.HttpUtility.ParseQueryString 
        Dim aParams As String() = oUri.Query.Split("&")
        For Each sParam As String In aParams
            If sParam.StartsWith("s=") Then Double.TryParse(sParam.Substring(2), dLat)
            If sParam.StartsWith("d=") Then Double.TryParse(sParam.Substring(2), dLon)
            If sParam.StartsWith("r=") Then Double.TryParse(sParam.Substring(2), dRad)
        Next

        If dLat = 100 OrElse dLon < 0 OrElse dRad = 0 Then
            Await DialogBoxAsync("protocolActivation: paramErrors  " & msNavigatedParam)
        End If

        Dim oPoint As JednoMiejsce = App.gMiejsca.GetMiejsce(dLat, dLon)
        If oPoint IsNot Nothing Then
            Await DialogBoxAsync("protocolActivation: mam miejsce = " & oPoint.sName)
        Else
            If Await DialogBoxYNAsync("Dodać miejsce z linku?") Then
                Dim oNew As JednoMiejsce = New JednoMiejsce
                oNew.bTutaj = True
                oNew.dLat = dLat
                oNew.dLon = dLon
                oNew.dRadius = dRad
                oNew.sName = "from LINK"
                oNew.iZwloka = 30
                App.gMiejsca.Add(oNew)
                App.gMiejsca.DodajUsunSystem(False, oNew)
                App.gMiejsca.Save(True)
            End If
        End If

    End Function

End Class

