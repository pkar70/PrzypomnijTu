'
' funkcjonalność bazy danych - do uszczegóławiania dla każdej podpinanej bazy

Public MustInherit Class Dbase_base

    Public MustOverride ReadOnly Property Nazwa As String

    ''' <summary>
    ''' ret != "" oznacza error (jego message)
    ''' </summary>
    Public MustOverride Async Function Init() As Task(Of String)

    Public MustOverride Async Function CheckConnectionAvailable() As Task(Of Boolean)

    ''' <summary>
    ''' cos jak new GUID, ale sprawdza w bazie czy aby na pewno nie ma takiego
    ''' </summary>
    Public Async Function GetNewId() As Task(Of String)
        DumpCurrMethod()
        Dim sId As String = ""

        For iGuard = 1 To 10
            sId = (New Guid).ToString
            If Await CheckNewIdUniq(sId) Then Exit For
            sId = ""
        Next
        If sId = "" Then
            DumpMessage("ERROR: wszystkie próby dają powtórkę GUID")
            Return ""
        End If

        Return sId

    End Function

    ''' <summary>
    ''' weryfikacja istnienia ID, true: nie ma
    ''' </summary>
    Protected MustOverride Async Function CheckNewIdUniq(sId As String) As Task(Of Boolean)

    ''' <summary>
    ''' zapisanie danych do bazy, Ret: OK, ale z tekstem sMsg a nie z oitem (jakby było okrojone)
    ''' </summary>
    Public MustOverride Async Function SendData(sId As String, oItem As JednoMiejsce, sMsg As String) As Task(Of Boolean)

    ''' <summary>
    ''' pobranie danych z bazy, error: ret NULL
    ''' </summary>
    Public MustOverride Async Function GetData(sId As String) As Task(Of JednoMiejsce)

End Class
