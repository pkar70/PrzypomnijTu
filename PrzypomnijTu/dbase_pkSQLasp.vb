Partial Public Class dbase_pkSQLasp
    Inherits Dbase_base

    Public Overrides ReadOnly Property Nazwa As String = "pkSQLasp"

    Public Overrides Async Function Init() As Task(Of String)
        HttpPageSetAgent("PrzypTu " & GetAppVers())
        Return ""
    End Function

    Public Overrides Async Function CheckConnectionAvailable() As Task(Of Boolean)
        DumpCurrMethod()
        If Not NetIsIPavailable(False) Then
            DumpMessage("ERROR: no IP network")
            Return False
        End If

        Dim sPage As String = ""
        Try
            sPage = Await HttpPageAsync(BaseUri & "conntest.htm", "", False)
        Catch ex As Exception
            DumpMessage("ERROR: exception on connecting to dbase")
            Return False
        End Try

        If Not sPage.Contains("Jestem") Then
            DumpMessage("ERROR: unexpected content while connecting to dbase")
            Return False
        End If

        Return True
    End Function

    Private Function NormalizeId(sId As String) As String
        sId = sId.Replace("{", "")
        sId = sId.Replace("}", "")
        sId = sId.Replace("-", "")
        Return sId
    End Function

    Public Overrides Function SendData(sId As String, oItem As JednoMiejsce, sMsg As String) As Task(Of Boolean)
        Throw New NotImplementedException()
    End Function

    Public Overrides Async Function GetData(sId As String) As Task(Of JednoMiejsce)
        Dim sPage As String = ""

        sPage = Await HttpPageAsync(BaseUri & "GetData.asp?id=" & NormalizeId(sId), "", False)
        If sPage = "" Then Return Nothing

        ' wczytanie jako JSON
        Dim mItems As List(Of dbaseSQLzadanie) = Nothing
        Dim sError As String = ""
        Try
            mItems = Newtonsoft.Json.JsonConvert.DeserializeObject(sPage, GetType(List(Of dbaseSQLzadanie)))
        Catch ex As Exception
            mItems = Nothing
            sError = ex.Message
        End Try

        If mItems Is Nothing Then Return Nothing
        If mItems.Count < 1 Then Return Nothing

        Dim oNew As New JednoMiejsce
        oNew.sName = mItems.Item(0).sender
        oNew.sRemindText = mItems.Item(0).msg
        oNew.dLat = mItems.Item(0).lat
        oNew.dLon = mItems.Item(0).lon

        Return oNew
    End Function

    Protected Overrides Async Function CheckNewIdUniq(sId As String) As Task(Of Boolean)
        Dim sPage As String = ""

        sPage = Await HttpPageAsync(BaseUri & "CheckNewIdUniq.asp?id=" & NormalizeId(sId), "", False)

        If Not sPage.Contains("ABSENT") Then Return False

        Return True
    End Function
End Class

Public Class dbaseSQLzadanie
    Public Property id As String    ' [nchar](32)
    Public Property lat As Double ' [real]
    Public Property lon As Double ' [real]
    Public Property sender As String '[nchar](32) 
    Public Property msg As String ' [nvarchar](64) 
    Public Property created As DateTime
End Class