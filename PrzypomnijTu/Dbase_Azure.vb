' Imports Microsoft.Azure.Documents


Partial Public Class Dbase_Azure
    Inherits Dbase_base

    'Private gmCosmosClient As Microsoft.Azure.Documents.Client.DocumentClient = Nothing
    'Private gmCosmosDatabase As Microsoft.Azure.Documents.Client.Database = Nothing
    'Private mCosmosContainer As Microsoft.Azure.Documents.Client.DocumentCollection = Nothing

    Public Overrides ReadOnly Property Nazwa As String = "dbase_Azure"

    Public Overrides Async Function Init() As Task(Of String)
        ' podpięcie bazy
        'If gmCosmosClient Is Nothing Then
        '    gmCosmosClient = New Microsoft.Azure.Cosmos.CosmosClient(COSMOS_ENDPOINT_URI, COSMOS_PRIMARY_KEY_RW)
        'End If
        'If gmCosmosClient Is Nothing Then Return "Cannot create dbase client"

        'If gmCosmosDatabase Is Nothing Then
        '    gmCosmosDatabase = gmCosmosClient.GetDatabase(COSMOS_DB_NAME)
        'End If

        'If gmCosmosDatabase Is Nothing Then Return "Cannot connect to dbase"

        'If mCosmosContainer Is Nothing Then
        '    mCosmosContainer = gmCosmosDatabase.GetContainer("przyptu")
        'End If

        'If mCosmosContainer Is Nothing Then Return "cannot get container"

        ' Return ""

        Return "ERROR: not implemented"


    End Function

    Protected Overrides Async Function CheckNewIdUniq(sId As String) As Task(Of Boolean)
        ' sprawdzenie czy sId jest unikalny
        'If mCosmosContainer Is Nothing Then Return Nothing
        'Dim sQry As String = "SELECT VALUE COUNT(1) FROM c WHERE c.id='" & sId & "'"

        'Dim sqlQueryDef As New Microsoft.Azure.Cosmos.QueryDefinition(sQry)
        'Using oIterator As Microsoft.Azure.Cosmos.FeedIterator(Of Long) = oCont.GetItemQueryIterator(Of Long)(sqlQueryDef)

        '    While oIterator.HasMoreResults
        '        Dim currentResultSet As Microsoft.Azure.Cosmos.FeedResponse(Of Long) = Await oIterator.ReadNextAsync()
        '        For Each oItem As Integer In currentResultSet
        '            If oItem <> 1 Then Return False
        '        Next
        '    End While

        'End Using

        'Return True
        Throw New NotImplementedException()

    End Function
    Public Overrides Async Function GetData(sId As String) As Task(Of JednoMiejsce)
        '' wczytanie danych wedle sId do JednoMiejsce
        'If mCosmosContainer Is Nothing Then Return Nothing

        'Dim sQry As String = "SELECT * FROM c WHERE c.id='" & sId & "'"
        'Using oIterator As Microsoft.Azure.Cosmos.FeedIterator(Of JednoMiejsce) =
        '        mCosmosFilesContainer.GetItemQueryIterator(Of JednoMiejsce)(sQry)

        '    While oIterator.HasMoreResults

        '        Dim currentResultSet As Microsoft.Azure.Cosmos.FeedResponse(Of JednoMiejsce) = Await oIterator.ReadNextAsync()

        '        'Dim oMsg1 As New oneStoreFiles
        '        'oMsg1.len = 0
        '        'oMsg1.name = "HasMoreResults"
        '        'oMsg1.path = currentResultSet.Count
        '        'oRet.Add(oMsg1)

        '        For Each oItem As JednoMiejsce In currentResultSet
        '            Return oItem
        '        Next

        '    End While

        'End Using

        'Return Nothing
        Throw New NotImplementedException()

    End Function


    Public Overrides Async Function SendData(sId As String, oItem As JednoMiejsce, sMsg As String) As Task(Of Boolean)
        ' wysłaie pod sId, zawartości oItem wraz z sMsg (a nie z tekstem z oItem)
        'If mCosmosContainer Is Nothing Then Return Nothing
        Throw New NotImplementedException()
    End Function

End Class
