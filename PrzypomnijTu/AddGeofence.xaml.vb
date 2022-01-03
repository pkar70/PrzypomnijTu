
Public NotInheritable Class AddGeofence
    Inherits Page

    Private mEditId As String = ""
    Private mPlace As JednoMiejsce = Nothing

    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)
        DumpCurrMethod()
        If e.Parameter Is Nothing Then Return
        mEditId = e.Parameter.ToString
    End Sub

    Private Async Sub uiGetGPS_Click(sender As Object, e As RoutedEventArgs)
        DumpCurrMethod()
        ProgRingShow(True)
        Dim oPos As Windows.Devices.Geolocation.BasicGeoposition = Await App.GetCurrentPoint(10)  ' lub default
        ProgRingShow(False)

        uiLat.Text = oPos.Latitude
        uiLon.Text = oPos.Longitude
        ' uiMapka.Center = New Windows.Devices.Geolocation.Geopoint(oPos)
    End Sub

    Private Async Sub uiAdd_Click(sender As Object, e As RoutedEventArgs)
        DumpCurrMethod()

        ' testy nazwy
        If uiNazwa.Text.Length < 2 Then
            DialogBox("ale jednak jakas nazwa jest potrzebna")
            Return
        End If
        If uiNazwa.Text.Length > 60 Then
            DialogBox("za dluga nazwa!")
            Return
        End If

        If uiNazwa.Text = gInitKey Then
            InitMojeMiejsca()
        Else

            Dim oPrev As JednoMiejsce = App.gMiejsca.GetMiejsce(uiNazwa.Text)
            If oPrev IsNot Nothing Then
                If mEditId = "" Then
                    DialogBox("taka nazwa już istnieje")
                    Return
                End If
                If mEditId <> uiNazwa.Text Then
                    DialogBox("zmieniłeś nazwę na taką co już istnieje")
                    Return
                End If
            End If

            Dim dLat As Double = SprobujParseLat(uiLat.Text)
            Dim dLon As Double = SprobujParseLon(uiLon.Text)
            Dim oItem As JednoMiejsce = App.gMiejsca.GetMiejsce(dLat, dLon)
            If oItem IsNot Nothing AndAlso oItem.sName <> mEditId Then
                DialogBox("To miejsce jest już zdefiniowaneW tym miejscu już istnieje jako: " & vbCrLf & oItem.sName)
                Return
            End If

            Dim oNew As JednoMiejsce = New JednoMiejsce
            oNew.sName = uiNazwa.Text
            oNew.dLat = dLat
            oNew.dLon = dLon
            oNew.dRadius = SprobujParseFi(uiRadius.Text)    ' choc to jest juz w samym NumberBox
            oNew.iZwloka = SprobujParseZwloka(uiZwloka.Text) ' choc to jest juz w samym NumberBox
            App.gMiejsca.Add(oNew)

            If mEditId <> "" AndAlso mEditId <> uiNazwa.Text Then
                App.gMiejsca.Remove(mPlace)
                If mPlace.bTutaj Then
                    ' i zeby nie wrocilo samo jako ze jest zdefiniowane w systemie również
                    App.gMiejsca.RemoveFromMonitor(mPlace)
                End If
            End If
        End If

        Await App.gMiejsca.SaveAsync(True)
        Me.Frame.GoBack()
    End Sub

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        DumpCurrMethod()

        If mEditId <> "" Then
            uiTitle.Text = "Zmiana Geofence"
            uiNazwa.IsReadOnly = True
            uiSaveSymbol.Symbol = Symbol.Save
        Else
            uiTitle.Text = "Dodawanie Geofence"
            uiNazwa.IsReadOnly = False
            uiSaveSymbol.Symbol = Symbol.Add
        End If

        ProgRingInit(True, False)
        If mEditId <> "" Then
            mPlace = App.gMiejsca.GetMiejsce(mEditId)
            If mPlace IsNot Nothing Then
                uiNazwa.Text = mEditId
                uiLat.Text = mPlace.dLat
                uiLon.Text = mPlace.dLon
                uiRadius.Text = mPlace.dRadius
                uiZwloka.Text = mPlace.iZwloka
                Dim oBGeo As Windows.Devices.Geolocation.BasicGeoposition = New Windows.Devices.Geolocation.BasicGeoposition
                oBGeo.Latitude = mPlace.dLat
                oBGeo.Longitude = mPlace.dLon
                uiMapka.Center = New Windows.Devices.Geolocation.Geopoint(oBGeo)
            End If
        Else
            Await SprobujZClip()
        End If
    End Sub

    Private Function SprobujParseLon(sLon As String) As Double
        DumpCurrMethod()
        Dim dTemp As Double

        If Not Double.TryParse(sLon, dTemp) Then Return 0
        Return dTemp.MinMax(0, 360)

    End Function
    Private Function SprobujParseLat(sLat As String) As Double
        DumpCurrMethod()
        Dim dTemp As Double

        If Not Double.TryParse(sLat, dTemp) Then Return 0
        Return dTemp.MinMax(-180, 180)
    End Function

    Private Function SprobujParseFi(sFi As String) As Double
        DumpCurrMethod()
        Dim dTemp As Double

        If Not Double.TryParse(sFi, dTemp) Then Return 0
        Return Math.Max(dTemp, 10)
    End Function

    Private Function SprobujParseZwloka(sFi As String) As Double
        DumpCurrMethod()
        Dim dTemp As Double

        If Not Double.TryParse(sFi, dTemp) Then Return 0
        Return dTemp.MinMax(10, 60 * 60)
    End Function

    Private Sub SprobujParse(sLat As String, sLon As String)
        DumpCurrMethod()
        uiLat.Text = SprobujParseLat(sLat)
        uiLon.Text = SprobujParseLon(sLon)
    End Sub

    Private Async Function SprobujZClip() As Task(Of Boolean)
        DumpCurrMethod()
        Dim sTxt As String = Await ClipGetAsync()
        Dim iInd As Integer

        If sTxt.StartsWith("https://www.openstreetmap.org/#map=") Then
            'https://www.openstreetmap.org/#map=16/50.0485/19.9155
            sTxt = sTxt.Substring("https://www.openstreetmap.org/#map=".Length)
            iInd = sTxt.IndexOf("/")
            If iInd < 0 Then
                DebugOut("SprobujZClip, niby link openstreetmap, ale brakuje pierwszego '/'")
                Return False
            End If
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("/")
            If iInd < 1 Then
                DebugOut("SprobujZClip, niby link openstreetmap, ale brakuje '/' pomiedzy Lat/Lon")
                Return False
            End If

            sTxt = sTxt.Replace(",", ".")
            SprobujParse(sTxt.Substring(0, iInd), sTxt.Substring(iInd + 1))
            Return True
        End If

        If sTxt.Contains(", ") Then
            'openstre   50.0485, 19.9155
            'BING:      50,039696, 19,949188
            iInd = sTxt.IndexOf(", ")
            sTxt = sTxt.Replace(",", ".")
            SprobujParse(sTxt.Substring(0, iInd), sTxt.Substring(iInd + 2))
            Return True
        End If

        Return False
    End Function



    Private Sub uiMapka_Loaded(sender As Object, e As RoutedEventArgs)
        DumpCurrMethod()

        uiMapka.Center = New Windows.Devices.Geolocation.Geopoint(GetDomekGeopos(0))

        If mEditId <> "" Then
            If IsFamilyMobile() Then
                uiMapka.ZoomLevel = 14
            Else
                uiMapka.ZoomLevel = 15
            End If
        Else
            If IsFamilyMobile() Then
                uiMapka.ZoomLevel = 10
            Else
                uiMapka.ZoomLevel = 12
            End If
        End If

        uiMapka.PedestrianFeaturesVisible = True
        uiMapka.TransitFeaturesVisible = True
        uiMapka.TransitFeaturesEnabled = True ' // od 14393, ale że Not implemented?


        If GetSettingsBool("pkarmode", IsThisMoje()) Then
            uiMapka.MapServiceToken = "RKDLI7ZllfFkdbkREcOC~niO_Btu7TQqqdATzYiXrpg~AoOmCAwWit2ryWbRcdp_NqW51w55BG9ZBKAkymCUgxNxtFi6ipSSHrOVYQofAP2n"
        Else
            uiMapka.MapServiceToken = "JE3FbSOjD5XNJQ0zYZmo~HJYorAlpbkaItKMJq653fA~AtIurL9F7tLZObKFiO-jhXMSfpHRevj9Ngw6S8EeQ6YcPCfNy4TCAGs0m33BBxW-"
            uiMapka.Style = Maps.MapStyle.Road
        End If

    End Sub


    Private Sub uiMapka_Holding(sender As Maps.MapControl, args As Maps.MapInputEventArgs)
        DumpCurrMethod()
        uiLat.Text = args.Location.Position.Latitude
        uiLon.Text = args.Location.Position.Longitude
        MapkaFocus(False)
    End Sub

    Private Sub uiMapka_DTapped(sender As Maps.MapControl, args As Maps.MapInputEventArgs)
        DumpCurrMethod()
        uiLat.Text = args.Location.Position.Latitude
        uiLon.Text = args.Location.Position.Longitude
        MapkaFocus(False)
    End Sub

    Private Sub MapkaFocus(bFocus As Boolean)
        DumpCurrMethod()
        For Each oItem As FrameworkElement In uiGridFields.Children
            If Not oItem.Name.StartsWith("uiNazwa") Then
                oItem.Visibility = If(bFocus, Visibility.Collapsed, Visibility.Visible)
            End If
        Next
    End Sub


    Private Sub uiMapka_FocusEngaged(sender As Control, args As FocusEngagedEventArgs) Handles uiMapka.FocusEngaged
        DumpCurrMethod()
        MapkaFocus(True)
    End Sub

    Private Sub uiMapka_Tapped(sender As Control, e As TappedRoutedEventArgs) Handles uiMapka.Tapped
        DumpCurrMethod()
        MapkaFocus(True)
        ' TappedRoutedEventArgs
        ' Maps.MapInputEventArgs
    End Sub

    Private Sub uiMapka_MapTapped(sender As Maps.MapControl, args As Maps.MapInputEventArgs) Handles uiMapka.MapTapped
        DumpCurrMethod()
        MapkaFocus(True)
    End Sub
End Class
