' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Settingsy
    Inherits Page
    Private Async Sub uiClear_Click(sender As Object, e As RoutedEventArgs)

        If Not Await DialogBoxYNAsync("Aby na pewno wyczyścić listę?") Then Return

        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        oGeoMon.Geofences.Clear()

        App.gMiejsca.Clear()
        Await App.gMiejsca.SaveAsync(True)

    End Sub

    Private Sub uiSave_Click(sender As Object, e As RoutedEventArgs)
        SetSettingsInt("silenceTo", uiDayStart.Value)
        SetSettingsInt("silenceFrom", uiDayStop.Value)
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiDayStart.Value = GetSettingsInt("silenceTo", 8)
        uiDayStop.Value = GetSettingsInt("silenceFrom", 21)
    End Sub
End Class
