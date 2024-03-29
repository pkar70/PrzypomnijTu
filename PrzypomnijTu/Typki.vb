﻿Public Class JednoMiejsce
    Public Property sName As String
    Public Property sRemindText As String = ""
    Public Property dLat As Double
    Public Property dLon As Double
    Public Property dRadius As Double = 75
    Public Property iZwloka As Integer = 10 ' UWP default gdy nie ma w ctor

    '    Public Property sLastVisitTime As String

    <Newtonsoft.Json.JsonIgnore>
    Public Property bTutaj As Boolean

End Class

Public Class ListaMiejsc
    Private mItems As ObservableCollection(Of JednoMiejsce) = Nothing
    Private bModified As Boolean = False
    Private Const FILE_NAME As String = "miejsca.json"
    Private msFileName As String = ""
    Public Const sIdPrefix As String = "PTU_"

    Public Sub New(sFolderPath As String)
        msFileName = IO.Path.Combine(sFolderPath, FILE_NAME)
    End Sub

    Public Function Load(Optional bForce As Boolean = False) As Boolean
        DumpCurrMethod()

        If IsLoaded() AndAlso Not bForce Then Return True

        bModified = False

        If Not IO.File.Exists(msFileName) Then
            mItems = New ObservableCollection(Of JednoMiejsce)
            Return False
        End If

        Dim sTxt As String = IO.File.ReadAllText(msFileName)
        If sTxt Is Nothing OrElse sTxt.Length < 5 Then
            mItems = New ObservableCollection(Of JednoMiejsce)
            Return False
        End If

        mItems = Newtonsoft.Json.JsonConvert.DeserializeObject(sTxt, GetType(ObservableCollection(Of JednoMiejsce)))

        ZaznaczKtoreSaMonitorowaneLokalnie()

        Return True

    End Function

    Public Function Save(bForce As Boolean) As Boolean
        If Not bModified AndAlso Not bForce Then Return False
        If mItems.Count < 1 Then Return False

        Dim sTxt As String = Newtonsoft.Json.JsonConvert.SerializeObject(mItems, Newtonsoft.Json.Formatting.Indented)
        IO.File.WriteAllText(msFileName, sTxt)

        bModified = False
        Return True

    End Function

    Public Function Add(oNew As JednoMiejsce) As Boolean
        If oNew Is Nothing Then Return False

        If mItems Is Nothing Then
            mItems = New ObservableCollection(Of JednoMiejsce)
        End If

        For Each oItem As JednoMiejsce In mItems
            If oItem.dLat = oNew.dLat AndAlso oItem.dLon = oNew.dLon Then Return False
        Next

        bModified = True

        mItems.Add(oNew)

        Return True
    End Function

    Public Function RemoveFromMonitor(oDel As JednoMiejsce) As Boolean
        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences Is Nothing Then Return False

        For Each oGeoF In oGeoMon.Geofences
            If oGeoF.Id = sIdPrefix & oDel.sName Then
                oGeoMon.Geofences.Remove(oGeoF)
                Return True
            End If
        Next

        Return False
    End Function

    Public Function Remove(oDel As JednoMiejsce) As Boolean
        Try
            mItems.Remove(oDel)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Sub Clear()
        bModified = True
        mItems.Clear()
    End Sub

    Public Function IsLoaded() As Boolean
        If mItems Is Nothing Then Return False
        Return True
    End Function

    Public Function GetList() As ObservableCollection(Of JednoMiejsce)
        Return mItems
    End Function

    Public Function Count() As Integer
        If mItems Is Nothing Then Return -1
        Return mItems.Count
    End Function

    Public Function GetMiejsce(oPos As Windows.Devices.Geolocation.BasicGeoposition) As JednoMiejsce
        ' znajdz takie w φ<20 metrów (i tak precyzja GPS jest mniejsza)
        If Count() < 1 Then Return Nothing

        For Each oItem As JednoMiejsce In mItems
            If oPos.DistanceTo(oItem.dLat, oItem.dLon) < 20 Then Return oItem
        Next

        Return Nothing
    End Function
    Public Function GetMiejsce(dLat As Double, dLon As Double) As JednoMiejsce
        Dim oPkt As Windows.Devices.Geolocation.BasicGeoposition
        oPkt.Longitude = dLon
        oPkt.Latitude = dLat
        Return GetMiejsce(oPkt)
    End Function

    Public Function GetMiejsce(sName As String) As JednoMiejsce
        If Count() < 1 Then Return Nothing

        For Each oItem As JednoMiejsce In mItems
            If sName = oItem.sName Then Return oItem
        Next

        Return Nothing
    End Function

    'Public Function Delete(sAddr As String) As Boolean
    '    If Count() < 1 Then Return False

    '    For Each oItem As JedenSocket In mItems
    '        If oItem.sAddr.ToLower = sAddr.ToLower Then
    '            mItems.Remove(oItem)
    '            bModified = True
    '            Return True
    '        End If
    '    Next

    '    Return False
    'End Function

    Public Sub MakeDirty()
        bModified = True
    End Sub

    Private Sub ZaznaczKtoreSaMonitorowaneLokalnie()
        DumpCurrMethod()
        For Each oItem As JednoMiejsce In mItems
            oItem.bTutaj = False
        Next


        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences IsNot Nothing Then
            For Each oFence As Windows.Devices.Geolocation.Geofencing.Geofence In oGeoMon.Geofences
                'dwa zabezpieczenia, którego mogłoby nie być, ale na wszelki wypadek... (nazwa, i kształt)
                If Not oFence.Id.ToUpper.StartsWith(sIdPrefix) Then Continue For
                Dim oGeoCircle As Windows.Devices.Geolocation.Geocircle = TryCast(oFence.Geoshape, Windows.Devices.Geolocation.Geocircle)
                If oGeoCircle Is Nothing Then Continue For

                For Each oItem As JednoMiejsce In mItems
                    If oItem.dLat = oGeoCircle.Center.Latitude AndAlso
                                oItem.dLon = oGeoCircle.Center.Longitude Then
                        DumpMessage("Circle " & oItem.sName & " is monitored lokalnie")
                        oItem.bTutaj = True
                        Exit For
                    End If
                Next
            Next
        End If

    End Sub

    Public Function ImportFencesFromSystem() As Boolean
        DumpCurrMethod()

        Dim bWas As Boolean = False

        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences IsNot Nothing Then
            For Each oFence As Windows.Devices.Geolocation.Geofencing.Geofence In oGeoMon.Geofences
                If Not oFence.Id.ToUpper.StartsWith(sIdPrefix) Then Continue For
                Dim oGeoCircle As Windows.Devices.Geolocation.Geocircle = TryCast(oFence.Geoshape, Windows.Devices.Geolocation.Geocircle)
                If oGeoCircle Is Nothing Then Continue For

                If GetMiejsce(oGeoCircle.Center) IsNot Nothing Then Continue For

                ' skoro go nie ma na liscie, to go dodaj - od razu jako lokalne
                Dim oNew As JednoMiejsce = New JednoMiejsce
                oNew.bTutaj = True
                oNew.dLat = oGeoCircle.Center.Latitude
                oNew.dLon = oGeoCircle.Center.Longitude
                oNew.dRadius = oGeoCircle.Radius
                oNew.sName = oFence.Id.Substring(sIdPrefix.Length)
                oNew.iZwloka = oFence.DwellTime.TotalSeconds
                mItems.Add(oNew)

                bModified = True
                bWas = True
            Next
        End If

        Return bWas
    End Function

    Private Shared Function CreateGeofence(oMiejsce As JednoMiejsce) As Windows.Devices.Geolocation.Geofencing.Geofence
        DumpCurrMethod()

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

    Private Shared Function TryAddSystemGeofence(oItem As JednoMiejsce) As Boolean
        DumpCurrMethod()

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

    Public Sub DodajUsunSystem(bUsun As Boolean, oItem As JednoMiejsce)
        DumpCurrMethod()
        If oItem Is Nothing Then Return

        ' Zakladam SYNC App.gMiejsca oraz GeofenceMonitor (bo przy wczytywaniu sie to zrobilo)

        Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
            Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current

        If oGeoMon.Geofences Is Nothing Then Return

        If Not bUsun Then
            ' włącz monitorowanie
            TryAddSystemGeofence(oItem)   ' z testem czy juz istnieje - bo MainPage.Loaded robi OnChecked!
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

    End Sub
End Class

