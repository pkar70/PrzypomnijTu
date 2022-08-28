'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------

'Imports System
'Imports System.Collections.Generic
'Imports System.Diagnostics
'Imports System.Globalization
'Imports System.IO
'Imports System.Net.Http
'Imports System.Threading
'Imports System.Threading.Tasks
'Imports Microsoft.Azure.Cosmos.Routing
'Imports Microsoft.Azure.Cosmos.Tracing
'Imports Microsoft.Azure.Documents
'Imports Microsoft.Azure.Documents.Routing

Namespace Microsoft.Azure.Cosmos.Handlers

    ''' <summary>
    ''' HttpMessageHandler can only be invoked by derived classed or internal classes inside http assembly
    ''' </summary>
    Friend Class RequestInvokerHandler
        'Inherits RequestHandler

        'Private Shared ReadOnly httpPatchMethod As HttpMethod = New HttpMethod(HttpConstants.HttpMethods.Patch)
        'Private Shared clientIsValid As (Boolean, ResponseMessage) = (False, Nothing)
        'Private ReadOnly client As CosmosClient
        'Private ReadOnly RequestedClientConsistencyLevel As Cosmos.ConsistencyLevel?
        'Private AccountConsistencyLevel As Cosmos.ConsistencyLevel? = Nothing

        'Public Sub New(ByVal client As CosmosClient, ByVal requestedClientConsistencyLevel As Cosmos.ConsistencyLevel?)
        '    Me.client = client
        '    Me.RequestedClientConsistencyLevel = requestedClientConsistencyLevel
        'End Sub

        'Public Overrides Async Function SendAsync(ByVal request As RequestMessage, ByVal cancellationToken As CancellationToken) As Task(Of ResponseMessage)
        '    If request Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(request))
        '    End If

        '    Dim promotedRequestOptions As RequestOptions = request.RequestOptions

        '    If promotedRequestOptions IsNot Nothing Then
        '        ' Fill request options
        '        promotedRequestOptions.PopulateRequestOptions(request)
        '    End If

        '    ' Adds the NoContent header if not already added based on Client Level flag
        '    If RequestInvokerHandler.ShouldSetNoContentResponseHeaders(request.RequestOptions, client.ClientOptions, request.OperationType, request.ResourceType) Then
        '        request.Headers.Add(HttpConstants.HttpHeaders.Prefer, HttpConstants.HttpHeaderValues.PreferReturnMinimal)
        '    End If

        '    Await Me.ValidateAndSetConsistencyLevelAsync(request)
        '    Dim isError As Boolean = Nothing, errorResponse As ResponseMessage = Nothing
        '    (isError, errorResponse) = Await Me.EnsureValidClientAsync(request, request.Trace)

        '    If isError Then
        '        Return errorResponse
        '    End If

        '    Await request.AssertPartitioningDetailsAsync(client, cancellationToken, request.Trace)
        '    Me.FillMultiMasterContext(request)
        '    Return Await MyBase.SendAsync(request, cancellationToken)
        'End Function

        'Public Overridable Overloads Async Function SendAsync(Of T)(ByVal resourceUri As String, ByVal resourceType As ResourceType, ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal cosmosContainerCore As ContainerInternal, ByVal feedRange As FeedRange, ByVal streamPayload As Stream, ByVal requestEnricher As Action(Of RequestMessage), ByVal responseCreator As Func(Of ResponseMessage, T), ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of T)
        '    If responseCreator Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(responseCreator))
        '    End If

        '    Dim responseMessage As ResponseMessage = Await Me.SendAsync(resourceUriString:=resourceUri, resourceType:=resourceType, operationType:=operationType, requestOptions:=requestOptions, cosmosContainerCore:=cosmosContainerCore, feedRange:=feedRange, streamPayload:=streamPayload, requestEnricher:=requestEnricher, trace:=trace, cancellationToken:=cancellationToken)
        '    Return responseCreator(responseMessage)
        'End Function

        'Public Overridable Overloads Async Function SendAsync(ByVal resourceUriString As String, ByVal resourceType As ResourceType, ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal cosmosContainerCore As ContainerInternal, ByVal feedRange As FeedRange, ByVal streamPayload As Stream, ByVal requestEnricher As Action(Of RequestMessage), ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of ResponseMessage)
        '    If Equals(resourceUriString, Nothing) Then
        '        Throw New ArgumentNullException(NameOf(resourceUriString))
        '    End If

        '    If trace Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(trace))
        '    End If

        '    ' This is needed for query where a single
        '    ' user request might span multiple backend requests.
        '    ' This will still have a single request id for retry scenarios
        '    Dim activityScope As ActivityScope = activityScope.CreateIfDefaultActivityId()
        '    Debug.Assert(activityScope Is Nothing OrElse activityScope IsNot Nothing AndAlso (operationType IsNot operationType.SqlQuery OrElse operationType IsNot operationType.Query OrElse operationType IsNot operationType.QueryPlan), "There should be an activity id already set")
        '    Dim feedRangePartitionKey As FeedRangePartitionKey = Nothing, feedRangeEpk As FeedRangeEpk = Nothing, feedRangePartitionKeyRange As FeedRangePartitionKeyRange = Nothing

        '    Using childTrace As ITrace = trace.StartChild(Me.FullHandlerName, TraceComponent.RequestHandler, Tracing.TraceLevel.Info)

        '        Try
        '            Dim method = RequestInvokerHandler.GetHttpMethod(resourceType, operationType)
        '            Dim request As RequestMessage = New RequestMessage(method, resourceUriString, childTrace) With {
        '                .operationType = operationType,
        '                .resourceType = resourceType,
        '                .requestOptions = requestOptions,
        '                .Content = streamPayload
        '            }

        '            If feedRange IsNot Nothing Then
        '                If CSharpImpl.__Assign(feedRangePartitionKey, TryCast(feedRange, FeedRangePartitionKey)) IsNot Nothing Then
        '                    If cosmosContainerCore Is Nothing AndAlso Object.ReferenceEquals(feedRangePartitionKey.PartitionKey, Cosmos.PartitionKey.None) Then
        '                        Throw New ArgumentException($"{NameOf(cosmosContainerCore)} can not be null with partition key as PartitionKey.None")
        '                    ElseIf feedRangePartitionKey.PartitionKey.IsNone Then

        '                        Try
        '                            Dim partitionKeyInternal As PartitionKeyInternal = Await cosmosContainerCore.GetNonePartitionKeyValueAsync(childTrace, cancellationToken)
        '                            request.Headers.PartitionKey = partitionKeyInternal.ToJsonString()
        '                        Catch dce As DocumentClientException
        '                            Return dce.ToCosmosResponseMessage(request)
        '                        Catch ce As CosmosException
        '                            Return ce.ToCosmosResponseMessage(request)
        '                        End Try
        '                    Else
        '                        request.Headers.PartitionKey = feedRangePartitionKey.PartitionKey.ToJsonString()
        '                    End If
        '                ElseIf CSharpImpl.__Assign(feedRangeEpk, TryCast(feedRange, FeedRangeEpk)) IsNot Nothing Then
        '                    Dim collectionFromCache As ContainerProperties

        '                    Try

        '                        If cosmosContainerCore Is Nothing Then
        '                            Throw New ArgumentException($"The container core can not be null for FeedRangeEpk")
        '                        End If

        '                        collectionFromCache = Await cosmosContainerCore.GetCachedContainerPropertiesAsync(forceRefresh:=False, childTrace, cancellationToken)
        '                    Catch ex As CosmosException
        '                        Return ex.ToCosmosResponseMessage(request)
        '                    End Try

        '                    Dim routingMapProvider As PartitionKeyRangeCache = Await client.DocumentClient.GetPartitionKeyRangeCacheAsync(childTrace)
        '                    Dim overlappingRanges As IReadOnlyList(Of PartitionKeyRange) = Await routingMapProvider.TryGetOverlappingRangesAsync(collectionFromCache.ResourceId, feedRangeEpk.Range, childTrace, forceRefresh:=False)

        '                    If overlappingRanges Is Nothing Then
        '                        Dim notFound As CosmosException = New CosmosException($"Stale cache for rid '{collectionFromCache.ResourceId}'", statusCode:=Net.HttpStatusCode.NotFound, subStatusCode:=Nothing, activityId:=Guid.Empty.ToString(), requestCharge:=Nothing)
        '                        Return notFound.ToCosmosResponseMessage(request)
        '                    End If

        '                    ' For epk range filtering we can end up in one of 3 cases:
        '                    If overlappingRanges.Count > 1 Then
        '                        ' 1) The EpkRange spans more than one physical partition
        '                        ' In this case it means we have encountered a split and 
        '                        ' we need to bubble that up to the higher layers to update their datastructures
        '                        Dim goneException As CosmosException = New CosmosException(message:=$"Epk Range: {feedRangeEpk.Range} is gone.", statusCode:=Net.HttpStatusCode.Gone, subStatusCode:=CInt(SubStatusCodes.PartitionKeyRangeGone), activityId:=Guid.NewGuid().ToString(), requestCharge:=Nothing)
        '                        ' overlappingRanges.Count == 1
        '                        Return goneException.ToCosmosResponseMessage(request)
        '                    Else
        '                        Dim singleRange As Range(Of String) = overlappingRanges(0).ToRange()

        '                        If singleRange.Min Is feedRangeEpk.Range.Min AndAlso singleRange.Max Is feedRangeEpk.Range.Max Then
        '                            ' 2) The EpkRange spans exactly one physical partition
        '                            ' In this case we can route to the physical pkrange id
        '                            request.PartitionKeyRangeId = New Documents.PartitionKeyRangeIdentity(overlappingRanges(0).Id)
        '                        Else
        '                            ' 3) The EpkRange spans less than single physical partition
        '                            ' In this case we route to the physical partition and 
        '                            ' pass the epk range headers to filter within partition
        '                            request.PartitionKeyRangeId = New Documents.PartitionKeyRangeIdentity(overlappingRanges(0).Id)
        '                            request.Headers(HttpConstants.HttpHeaders.ReadFeedKeyType) = RntbdConstants.RntdbReadFeedKeyType.EffectivePartitionKeyRange.ToString()
        '                            request.Headers(HttpConstants.HttpHeaders.StartEpk) = feedRangeEpk.Range.Min
        '                            request.Headers(HttpConstants.HttpHeaders.EndEpk) = feedRangeEpk.Range.Max
        '                        End If
        '                    End If
        '                Else
        '                    request.PartitionKeyRangeId = If(CSharpImpl.__Assign(feedRangePartitionKeyRange, TryCast(feedRange, FeedRangePartitionKeyRange)) IsNot Nothing, New Documents.PartitionKeyRangeIdentity(feedRangePartitionKeyRange.PartitionKeyRangeId), CSharpImpl.__Throw(Of Documents.PartitionKeyRangeIdentity)(New InvalidOperationException($"Unknown feed range type: '{feedRange.[GetType]()}'.")))
        '                End If
        '            End If

        '            If operationType Is operationType.Upsert Then
        '                request.Headers.IsUpsert = Boolean.TrueString
        '            ElseIf operationType Is operationType.Patch Then
        '                request.Headers.ContentType = RuntimeConstants.MediaTypes.JsonPatch
        '            End If

        '            If cosmosContainerCore IsNot Nothing Then
        '                request.ContainerId = cosmosContainerCore?.Id
        '                request.DatabaseId = cosmosContainerCore?.Database.Id
        '            End If

        '            requestEnricher?.Invoke(request)
        '            Return Await Me.SendAsync(request, cancellationToken)
        '        Finally
        '            activityScope?.Dispose()
        '        End Try
        '    End Using
        'End Function

        'Friend Shared Function GetHttpMethod(ByVal resourceType As ResourceType, ByVal operationType As OperationType) As HttpMethod
        '    If operationType Is operationType.Create OrElse operationType Is operationType.Upsert OrElse operationType Is operationType.Query OrElse operationType Is operationType.SqlQuery OrElse operationType Is operationType.QueryPlan OrElse operationType Is operationType.Batch OrElse operationType Is operationType.ExecuteJavaScript OrElse operationType Is operationType.CompleteUserTransaction OrElse resourceType Is resourceType.PartitionKey AndAlso operationType Is operationType.Delete Then
        '        Return HttpMethod.Post
        '    ElseIf operationType Is operationType.Read OrElse operationType Is operationType.ReadFeed Then
        '        Return HttpMethod.Get
        '    ElseIf operationType Is operationType.Replace OrElse operationType Is operationType.CollectionTruncate Then
        '        Return HttpMethod.Put
        '    ElseIf operationType Is operationType.Delete Then
        '        Return HttpMethod.Delete
        '    ElseIf operationType Is operationType.Patch Then
        '        ' There isn't support for PATCH method in .NetStandard 2.0
        '        Return httpPatchMethod
        '    Else
        '        Throw New NotImplementedException()
        '    End If
        'End Function

        'Private Async Function EnsureValidClientAsync(ByVal request As RequestMessage, ByVal trace As ITrace) As Task(Of (Boolean, ResponseMessage))
        '    Try
        '        Await client.DocumentClient.EnsureValidClientAsync(trace)
        '        Return clientIsValid
        '    Catch dce As DocumentClientException
        '        Return (True, dce.ToCosmosResponseMessage(request))
        '    End Try
        'End Function

        'Private Sub FillMultiMasterContext(ByVal request As RequestMessage)
        '    If client.DocumentClient.UseMultipleWriteLocations Then
        '        request.Headers.Set(HttpConstants.HttpHeaders.AllowTentativeWrites, Boolean.TrueString)
        '    End If
        'End Sub

        'Private Async Function ValidateAndSetConsistencyLevelAsync(ByVal requestMessage As RequestMessage) As Task
        '    ' Validate the request consistency compatibility with account consistency
        '    ' Type based access context for requested consistency preferred for performance
        '    Dim consistencyLevel As Microsoft.Azure.Cosmos.ConsistencyLevel? = Nothing
        '    Dim promotedRequestOptions As RequestOptions = requestMessage.RequestOptions

        '    If promotedRequestOptions IsNot Nothing AndAlso promotedRequestOptions.BaseConsistencyLevel.HasValue Then
        '        consistencyLevel = promotedRequestOptions.BaseConsistencyLevel
        '    ElseIf RequestedClientConsistencyLevel.HasValue Then
        '        consistencyLevel = RequestedClientConsistencyLevel
        '    End If

        '    If consistencyLevel.HasValue Then
        '        If Not AccountConsistencyLevel.HasValue Then
        '            AccountConsistencyLevel = Await client.GetAccountConsistencyLevelAsync()
        '        End If

        '        If ValidationHelpers.IsValidConsistencyLevelOverwrite(AccountConsistencyLevel.Value, consistencyLevel.Value) Then
        '            ' ConsistencyLevel compatibility with back-end configuration will be done by RequestInvokeHandler
        '            requestMessage.Headers.Add(HttpConstants.HttpHeaders.ConsistencyLevel, consistencyLevel.Value.ToString())
        '        Else
        '            Throw New ArgumentException(String.Format(CultureInfo.CurrentUICulture, RMResources.InvalidConsistencyLevel, consistencyLevel.Value.ToString(), AccountConsistencyLevel))
        '        End If
        '    End If
        'End Function

        'Friend Shared Function ShouldSetNoContentResponseHeaders(ByVal requestOptions As RequestOptions, ByVal clientOptions As CosmosClientOptions, ByVal operationType As OperationType, ByVal resourceType As ResourceType) As Boolean
        '    If resourceType IsNot resourceType.Document Then
        '        Return False
        '    End If

        '    If requestOptions Is Nothing Then
        '        Return RequestInvokerHandler.IsClientNoResponseSet(clientOptions, operationType)
        '    End If

        '    Dim itemRequestOptions As ItemRequestOptions = Nothing

        '    If CSharpImpl.__Assign(itemRequestOptions, TryCast(requestOptions, ItemRequestOptions)) IsNot Nothing Then
        '        If itemRequestOptions.EnableContentResponseOnWrite.HasValue Then
        '            Return RequestInvokerHandler.IsItemNoRepsonseSet(itemRequestOptions.EnableContentResponseOnWrite.Value, operationType)
        '        Else
        '            Return RequestInvokerHandler.IsClientNoResponseSet(clientOptions, operationType)
        '        End If
        '    End If

        '    Dim batchRequestOptions As TransactionalBatchItemRequestOptions = Nothing

        '    If CSharpImpl.__Assign(batchRequestOptions, TryCast(requestOptions, TransactionalBatchItemRequestOptions)) IsNot Nothing Then
        '        If batchRequestOptions.EnableContentResponseOnWrite.HasValue Then
        '            Return RequestInvokerHandler.IsItemNoRepsonseSet(batchRequestOptions.EnableContentResponseOnWrite.Value, operationType)
        '        Else
        '            Return RequestInvokerHandler.IsClientNoResponseSet(clientOptions, operationType)
        '        End If
        '    End If

        '    Return False
        'End Function

        'Private Shared Function IsItemNoRepsonseSet(ByVal enableContentResponseOnWrite As Boolean, ByVal operationType As OperationType) As Boolean
        '    Return Not enableContentResponseOnWrite AndAlso (operationType Is operationType.Create OrElse operationType Is operationType.Replace OrElse operationType Is operationType.Upsert OrElse operationType Is operationType.Patch)
        'End Function

        'Private Shared Function IsClientNoResponseSet(ByVal clientOptions As CosmosClientOptions, ByVal operationType As OperationType) As Boolean
        '    Return clientOptions IsNot Nothing AndAlso clientOptions.EnableContentResponseOnWrite.HasValue AndAlso RequestInvokerHandler.IsItemNoRepsonseSet(clientOptions.EnableContentResponseOnWrite.Value, operationType)
        'End Function

        'Private Class CSharpImpl
        '    <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
        '    Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
        '        target = value
        '        Return value
        '    End Function <Obsolete("Please refactor calling code to use normal throw statements")>
        '    Shared Function __Throw(Of T)(ByVal e As Exception) As T
        '        Throw e
        '    End Function
        'End Class
    End Class
End Namespace
