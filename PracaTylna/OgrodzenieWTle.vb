Imports Windows.ApplicationModel.Background

Public NotInheritable Class OgrodzenieWTle
    Implements Background.IBackgroundTask

    Public Sub Run(taskInstance As IBackgroundTaskInstance) Implements IBackgroundTask.Run
        Dim deferral As BackgroundTaskDeferral = taskInstance.GetDeferral

        'CrashMessageAdd("Run", "Nowy Run()")

        If GetSettingsInt("silenceFrom", 21) < Date.Now.Hour Then Return
        If GetSettingsInt("silenceTo", 8) > Date.Now.Hour Then Return

        Select Case taskInstance.Task.Name
            Case "PrzypomnijTu_Geofence"
                Try
                    Dim oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor =
                    Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current
                    If oGeoMon Is Nothing Then
                        MakeToast("oGeoMon NULL", "a")
                    Else
                        GeofenceEvents2Toasts(oGeoMon)
                    End If
                Catch ex As Exception
                    CrashMessageAdd("Run-Catch", ex, True)
                End Try

            Case Else
                CrashMessageAdd("Run", "w Select Case Else")

        End Select

        'CrashMessageAdd("Run", "Run() przed Complete")

        deferral.Complete()

    End Sub

    Private Sub GeofenceEvents2Toasts(oGeoMon As Windows.Devices.Geolocation.Geofencing.GeofenceMonitor)

        Dim oLista As IReadOnlyList(Of Windows.Devices.Geolocation.Geofencing.GeofenceStateChangeReport) = Nothing

        Try
            oLista = oGeoMon.ReadReports()
        Catch ex As Exception
            CrashMessageAdd("Geo..Read", ex, True)
            oLista = Nothing
        End Try

        If oLista Is Nothing Then
            CrashMessageAdd("Geo..Read", "oList NULL")
            MakeToast("oGeoMon NULL", "a")
            Return
        End If

        If oLista.Count < 1 Then
            CrashMessageAdd("Geo..Read", "oList <1")
            MakeToast("oGeoMon NULL", "a")
            Return
        End If

        For Each oItem As Windows.Devices.Geolocation.Geofencing.GeofenceStateChangeReport In oLista
            Dim iInd As Integer = oItem.Geofence.Id.IndexOf("_")
            Dim sName As String = oItem.Geofence.Id.Substring(iInd + 1)
            Dim sMsg As String = sName
            If oItem.NewState = Windows.Devices.Geolocation.Geofencing.GeofenceState.Entered Then
                sMsg = Name2Message(sName)
                If String.IsNullOrEmpty(sMsg) Then sMsg = sName
            End If
            MakeToast(sMsg, oItem.NewState.ToString)
        Next

    End Sub

    Private Function Name2Message(sName As String) As String
        ' *TODO* zamiana name na message - ale to musi przeczytać całą listę, bo jej tu nie ma...
        Return ""
    End Function


#Region "Fragmenty pkarmodule"
    Public Shared Sub MakeToast(sFenceId As String, sMsg1 As String)
        ' lekko zmodyfikowane o guziki, z FilteredRSS
        Dim sVisual As String = "<visual><binding template='ToastGeneric'><text>" & XmlSafeString(sFenceId)
        If sMsg1 <> "" Then sVisual = sVisual & "</text><text>" & XmlSafeString(sMsg1)
        sVisual = sVisual & "</text></binding></visual>"

        Dim sAction As String = "<actions>" & vbCrLf &
            ToastAction("system", "", "dismiss", "") & vbCrLf &
            ToastAction("foreground", "OPEN", sFenceId, "resOpen") & vbCrLf &
            "</actions>"

        Dim sGlobalAction As String = " launch=""OPEN" & XmlSafeString(sFenceId) & """ "

        Dim oXml = New Windows.Data.Xml.Dom.XmlDocument
        oXml.LoadXml("<toast" & sGlobalAction & ">" & sVisual & sAction & "</toast>")

        Dim oToast = New Windows.UI.Notifications.ToastNotification(oXml)

        Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(oToast)
    End Sub
    Public Shared Function XmlSafeString(sInput As String) As String
        ' 2022.01.08 zmiana na niby bardziej elastyczny?
        'Dim sTmp As String
        'sTmp = sInput.Replace("&", "&amp;")
        'sTmp = sTmp.Replace("<", "&lt;")
        'sTmp = sTmp.Replace(">", "&gt;")
        'Return sTmp
        sInput = New XText(sInput).ToString()
        sInput = sInput.Replace("""", "&quote;")
        Return sInput
    End Function
    Public Shared Function ToastAction(sAType As String, sAct As String, sGuid As String, sContent As String) As String
        Dim sTmp As String = sContent
        If sTmp <> "" Then sTmp = GetSettingsString(sTmp, sTmp)

        Dim sTxt As String = "<action " &
            "activationType=""" & sAType & """ " &
            "arguments=""" & sAct & sGuid & """ " &
            "content=""" & sTmp & """/> "
        Return sTxt
    End Function

    Public Shared Function GetSettingsString(sName As String, sDefault As String) As String
        Dim sTmp As String = sDefault

        With Windows.Storage.ApplicationData.Current
            If .RoamingSettings.Values.ContainsKey(sName) Then
                sTmp = .RoamingSettings.Values(sName).ToString
            End If
            If .LocalSettings.Values.ContainsKey(sName) Then
                sTmp = .LocalSettings.Values(sName).ToString
            End If
        End With

        Return sTmp

    End Function

    Public Shared Sub SetSettingsString(sName As String, sValue As String, bRoam As Boolean)
        Try
            If bRoam Then Windows.Storage.ApplicationData.Current.RoamingSettings.Values(sName) = sValue
            Windows.Storage.ApplicationData.Current.LocalSettings.Values(sName) = sValue
        Catch ex As Exception
            ' jesli przepełniony bufor (za długa zmienna) - nie zapisuj dalszych błędów
        End Try
    End Sub

    Public Function GetSettingsInt(sName As String, iDefault As Integer) As Integer
        Dim sTmp As Integer

        sTmp = iDefault

        With Windows.Storage.ApplicationData.Current
            If .RoamingSettings.Values.ContainsKey(sName) Then
                sTmp = CInt(.RoamingSettings.Values(sName).ToString)
            End If
            If .LocalSettings.Values.ContainsKey(sName) Then
                sTmp = CInt(.LocalSettings.Values(sName).ToString)
            End If
        End With

        Return sTmp

    End Function
    ''' <summary>
    ''' Dodaj do logu,
    ''' gdy debug to pokaż toast i wyślij DebugOut,
    ''' gdy release to toast gdy GetSettingsBool("crashShowToast") 
    ''' </summary>
    Public Shared Sub CrashMessageAdd(sTxt As String, exMsg As String)
        Dim sAdd As String = Date.Now.ToString("HH:mm:ss") & " " & sTxt & vbCrLf & exMsg & vbCrLf
#If DEBUG Then
        ' linia z MyCameras - Toast replikowany, więc powinien podać z którego telefonu :)
        MakeToast("OgrodzenieWTle @" & Date.Now.ToString("HH:mm:ss"), exMsg)
        ' MakeToast(sAdd)
#Else
                If GetSettingsBool("crashShowToast") Then MakeToast(sAdd)
#End If
        SetSettingsString("appFailData", GetSettingsString("appFailData", "") & sAdd, False)
    End Sub


    ' wersja w MyCameras nie miała optional ze stack
    ''' <summary>
    ''' Dodaj do logu,
    ''' gdy debug to pokaż toast i wyślij DebugOut,
    ''' gdy release to toast gdy GetSettingsBool("crashShowToast") 
    ''' </summary>
    Public Shared Sub CrashMessageAdd(sTxt As String, ex As Exception, bWithStack As Boolean)
        Dim sMsg As String = ex.Message
        If bWithStack AndAlso ex.StackTrace IsNot Nothing Then
            sMsg = sMsg & vbCrLf & ex.StackTrace
        End If
        CrashMessageAdd(sTxt, sMsg)
    End Sub


#End Region

End Class
