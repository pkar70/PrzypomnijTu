
' strona dostępna tylko w wersji Debug, do testowania różnych zachowań app

Public NotInheritable Class DebugRozne
    Inherits Page

    Private mListDebug As New List(Of EntryDebug)


    Private Sub uiViewSysList_Click(sender As Object, e As RoutedEventArgs)

        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences Is Nothing Then
            DialogBox("no sys list")
            Return
        End If

        ProgRingShow(True)

        mListDebug.Clear()
        For Each oFence As Windows.Devices.Geolocation.Geofencing.Geofence In oGeoMon.Geofences
            Dim oNew As New EntryDebug
            oNew.sName = oFence.Id

            Dim oGeoCircle As Windows.Devices.Geolocation.Geocircle = TryCast(oFence.Geoshape, Windows.Devices.Geolocation.Geocircle)
            If oGeoCircle Is Nothing Then Continue For

            oNew.sTxt = "s=" & oGeoCircle.Center.Latitude.ToString("#0.0000") &
                        " d=" & oGeoCircle.Center.Longitude.ToString("#0.0000") &
                        " r=" & oGeoCircle.Radius &
                        " z=" & oFence.DwellTime.TotalSeconds

            mListDebug.Add(oNew)
        Next

        uiPointsList.ItemsSource = Nothing ' wymuszenie przerysowania
        uiPointsList.ItemsSource = mListDebug

        ProgRingShow(False)

    End Sub

    Private Async Sub uiViewAppList_Click(sender As Object, e As RoutedEventArgs)

        ProgRingShow(True)

        Dim oPos As Windows.Devices.Geolocation.BasicGeoposition = Await App.GetCurrentPoint(10)  ' lub default

        mListDebug.Clear()
        For Each oItem As JednoMiejsce In App.gMiejsca.GetList
            Dim oNew As New EntryDebug
            oNew.sName = oItem.sName
            oNew.sTxt = oPos.DistanceTo(oItem.dLat, oItem.dLon)
            mListDebug.Add(oNew)
        Next

        uiPointsList.ItemsSource = Nothing ' wymuszenie przerysowania
        uiPointsList.ItemsSource = mListDebug

        ProgRingShow(False)

    End Sub

    Private Async Sub uiDelSysList_Click(sender As Object, e As RoutedEventArgs)
        If Not Await DialogBoxYNAsync("DEL list in system?") Then Return

        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences Is Nothing Then
            DialogBox("no sys list")
            Return
        End If

        oGeoMon.Geofences.Clear()
        uiViewSysList_Click(Nothing, Nothing)
    End Sub

    Private Async Sub uiDelAppList_Click(sender As Object, e As RoutedEventArgs)
        If Not Await DialogBoxYNAsync("DEL list in app?") Then Return
        App.gMiejsca.Clear()

        If Await DialogBoxYNAsync("save empty list?") Then Await App.gMiejsca.SaveAsync(True)
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        ProgRingInit(True, False)
    End Sub
End Class

Public Class EntryDebug
    Public Property sName As String
    Public Property sTxt As String

End Class