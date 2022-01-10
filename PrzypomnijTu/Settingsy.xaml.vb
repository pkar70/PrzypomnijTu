' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Settingsy
    Inherits Page
    Private Async Sub uiClear_Click(sender As Object, e As RoutedEventArgs)

        If Not Await DialogBoxYNAsync("Aby na pewno wyczyścić listę w systemie i w App?") Then Return

        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        oGeoMon.Geofences.Clear()

        App.gMiejsca.Clear()
        Await App.gMiejsca.SaveAsync(True)

    End Sub

    Private Sub uiSave_Click(sender As Object, e As RoutedEventArgs)
        SetSettingsInt("silenceTo", uiDayStart.Value)
        SetSettingsInt("silenceFrom", uiDayStop.Value)
        SetSettingsBool(uiEmailSMS, "useSMS")
        Me.Frame.GoBack()
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiDayStart.Value = GetSettingsInt("silenceTo", 8)
        uiDayStop.Value = GetSettingsInt("silenceFrom", 21)
        GetSettingsBool(uiEmailSMS, "useSMS", IsFamilyMobile)

#If DEBUG Then
        uiDebug.Visibility = Visibility.Visible
#End If
    End Sub

    Private Sub uiSliderZmiana_Changed(sender As Object, e As RangeBaseValueChangedEventArgs)
        ' TextBlock z wartością zmieni się via Binding, ale teraz robimy szerokości pasków

        ' jeszcze za wcześnie, dopiero tworzymy Page, nie ma kontrolek
        If uiDayStart Is Nothing Then Return
        If uiDayStop Is Nothing Then Return
        If uiPasekStart Is Nothing Then Return
        If uiPasekStop Is Nothing Then Return
        If uiPasekMiddle Is Nothing Then Return

        Dim iStart As Integer = uiDayStart.Value
        Dim iStop As Integer = uiDayStop.Value

        uiPasekStart.Width = New GridLength(iStart, GridUnitType.Star)
        uiPasekStop.Width = New GridLength(24 - iStop, GridUnitType.Star)
        uiPasekMiddle.Width = New GridLength(Math.Max(0, iStop - iStart), GridUnitType.Star)
    End Sub

    Private Sub uiDebug_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(DebugRozne))
    End Sub
End Class
