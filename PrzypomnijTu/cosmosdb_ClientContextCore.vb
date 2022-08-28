''------------------------------------------------------------
'' Copyright (c) Microsoft Corporation.  All rights reserved.
''------------------------------------------------------------

''Imports System
''Imports System.Diagnostics
''Imports System.IO
'Imports System.Net.Http
''Imports System.Text
''Imports System.Threading
''Imports System.Threading.Tasks
''Imports Microsoft.Azure.Cosmos.Core.Trace
''Imports Microsoft.Azure.Cosmos.Handler
''Imports Microsoft.Azure.Cosmos.Handlers
''Imports Microsoft.Azure.Cosmos.Resource.CosmosExceptions
''Imports Microsoft.Azure.Cosmos.Routing
''Imports Microsoft.Azure.Cosmos.Telemetry
''Imports Microsoft.Azure.Cosmos.Tracing
''Imports Microsoft.Azure.Documents

'Namespace Microsoft.Azure.Cosmos
'    Friend Class ClientContextCore
'        Inherits CosmosClientContext

'        'Private ReadOnly batchExecutorCache As BatchAsyncContainerExecutorCache
'        Private ReadOnly clientField As CosmosClient
'        'Private ReadOnly documentClientField As DocumentClient
'        'Private ReadOnly serializerCoreField As CosmosSerializerCore
'        'Private ReadOnly responseFactoryField As CosmosResponseFactoryInternal
'        'Private ReadOnly requestHandlerField As RequestInvokerHandler
'        Private ReadOnly clientOptionsField As CosmosClientOptions
'        'Private ReadOnly telemetry As ClientTelemetry
'        'Private ReadOnly userAgentField As String
'        Private isDisposed As Boolean = False

'        'Private Sub New(ByVal client As CosmosClient, ByVal clientOptions As CosmosClientOptions, ByVal serializerCore As CosmosSerializerCore, ByVal cosmosResponseFactory As CosmosResponseFactoryInternal, ByVal requestHandler As RequestInvokerHandler, ByVal documentClient As DocumentClient, ByVal userAgent As String, ByVal batchExecutorCache As BatchAsyncContainerExecutorCache, ByVal telemetry As ClientTelemetry)
'        '    clientField = client
'        '    clientOptionsField = clientOptions
'        '    serializerCoreField = serializerCore
'        '    responseFactoryField = cosmosResponseFactory
'        '    requestHandlerField = requestHandler
'        '    documentClientField = documentClient
'        '    userAgentField = userAgent
'        '    Me.batchExecutorCache = batchExecutorCache
'        '    Me.telemetry = telemetry
'        'End Sub

'        Friend Shared Function Create(ByVal cosmosClient As CosmosClient, ByVal clientOptions As CosmosClientOptions) As CosmosClientContext
'            If cosmosClient Is Nothing Then
'                Throw New ArgumentNullException(NameOf(cosmosClient))
'            End If

'            clientOptions = ClientContextCore.CreateOrCloneClientOptions(clientOptions)
'            Dim httpMessageHandler As HttpMessageHandler = CosmosHttpClientCore.CreateHttpClientHandler(clientOptions.GatewayModeMaxConnectionLimit, clientOptions.WebProxy)
'            Dim _documentClient As DocumentClient = New DocumentClient(cosmosClient.Endpoint, cosmosClient.AuthorizationTokenProvider, apitype:=clientOptions.ApiType, sendingRequestEventArgs:=clientOptions.SendingRequestEventArgs, transportClientHandlerFactory:=clientOptions.TransportClientHandlerFactory, connectionPolicy:=clientOptions.GetConnectionPolicy(cosmosClient.ClientId), enableCpuMonitor:=clientOptions.EnableCpuMonitor, storeClientFactory:=clientOptions.StoreClientFactory, desiredConsistencyLevel:=clientOptions.GetDocumentsConsistencyLevel(), handler:=httpMessageHandler, sessionContainer:=clientOptions.SessionContainer)
'            Return ClientContextCore.Create(cosmosClient, _documentClient, clientOptions)
'        End Function

'        Friend Shared Function Create(ByVal cosmosClient As CosmosClient, ByVal documentClient As DocumentClient, ByVal clientOptions As CosmosClientOptions, ByVal Optional requestInvokerHandler As Handlers.RequestInvokerHandler = Nothing) As CosmosClientContext
'            If cosmosClient Is Nothing Then
'                Throw New ArgumentNullException(NameOf(cosmosClient))
'            End If

'            If documentClient Is Nothing Then
'                Throw New ArgumentNullException(NameOf(documentClient))
'            End If

'            clientOptions = ClientContextCore.CreateOrCloneClientOptions(clientOptions)
'            Dim connectionPolicy As ConnectionPolicy = clientOptions.GetConnectionPolicy(cosmosClient.ClientId)
'            'Dim telemetry As ClientTelemetry = Nothing

'            'If connectionPolicy.EnableClientTelemetry Then
'            '    Try
'            '        telemetry = ClientTelemetry.CreateAndStartBackgroundTelemetry(documentClient:=documentClient, userAgent:=connectionPolicy.UserAgentContainer.UserAgent, connectionMode:=connectionPolicy.ConnectionMode, authorizationTokenProvider:=cosmosClient.AuthorizationTokenProvider, diagnosticsHelper:=DiagnosticsHandlerHelper.Instance, preferredRegions:=clientOptions.ApplicationPreferredRegions)
'            '    Catch ex As Exception
'            '        DefaultTrace.TraceInformation($"Error While starting Telemetry Job : {ex.Message}. Hence disabling Client Telemetry")
'            '        connectionPolicy.EnableClientTelemetry = False
'            '    End Try
'            'Else
'            '    DefaultTrace.TraceInformation("Client Telemetry Disabled.")
'            'End If

'            If requestInvokerHandler Is Nothing Then
'                'Request pipeline 
'                Dim clientPipelineBuilder As ClientPipelineBuilder = New ClientPipelineBuilder(cosmosClient, clientOptions.ConsistencyLevel, clientOptions.CustomHandlers, telemetry:=telemetry)
'                requestInvokerHandler = clientPipelineBuilder.Build()
'            End If

'            Dim serializerCore As CosmosSerializerCore = CosmosSerializerCore.Create(clientOptions.Serializer, clientOptions.SerializerOptions)

'            ' This sets the serializer on client options which gives users access to it if a custom one is not configured.
'            clientOptions.SetSerializerIfNotConfigured(serializerCore.GetCustomOrDefaultSerializer())
'            Dim responseFactory As CosmosResponseFactoryInternal = New CosmosResponseFactoryCore(serializerCore)
'            Return New ClientContextCore(client:=cosmosClient, clientOptions:=clientOptions, serializerCore:=serializerCore, cosmosResponseFactory:=responseFactory, requestHandler:=requestInvokerHandler, documentClient:=documentClient, userAgent:=documentClient.ConnectionPolicy.UserAgentContainer.UserAgent, batchExecutorCache:=New BatchAsyncContainerExecutorCache(), telemetry:=telemetry)
'        End Function

'        '''' <summary>
'        '''' The Cosmos client that is used for the request
'        '''' </summary>
'        'Friend Overrides ReadOnly Property Client As CosmosClient
'        '    Get
'        '        Return Me.ThrowIfDisposed(clientField)
'        '    End Get
'        'End Property

'        'Friend Overrides ReadOnly Property DocumentClient As DocumentClient
'        '    Get
'        '        Return Me.ThrowIfDisposed(documentClientField)
'        '    End Get
'        'End Property

'        'Friend Overrides ReadOnly Property SerializerCore As CosmosSerializerCore
'        '    Get
'        '        Return Me.ThrowIfDisposed(serializerCoreField)
'        '    End Get
'        'End Property

'        'Friend Overrides ReadOnly Property ResponseFactory As CosmosResponseFactoryInternal
'        '    Get
'        '        Return Me.ThrowIfDisposed(responseFactoryField)
'        '    End Get
'        'End Property

'        'Friend Overrides ReadOnly Property RequestHandler As RequestInvokerHandler
'        '    Get
'        '        Return Me.ThrowIfDisposed(requestHandlerField)
'        '    End Get
'        'End Property

'        'Friend Overrides ReadOnly Property ClientOptions As CosmosClientOptions
'        '    Get
'        '        Return Me.ThrowIfDisposed(clientOptionsField)
'        '    End Get
'        'End Property

'        'Friend Overrides ReadOnly Property UserAgent As String
'        '    Get
'        '        Return ThrowIfDisposed(userAgentField)
'        '    End Get
'        'End Property

'        '''' <summary>
'        '''' Generates the URI link for the resource
'        '''' </summary>
'        '''' <paramname="parentLink">The parent link URI (/dbs/mydbId) </param>
'        '''' <paramname="uriPathSegment">The URI path segment</param>
'        '''' <paramname="id">The id of the resource</param>
'        '''' <returns>A resource link in the format of {parentLink}/this.UriPathSegment/this.Name with this.Name being a Uri escaped version</returns>
'        'Friend Overrides Function CreateLink(ByVal parentLink As String, ByVal uriPathSegment As String, ByVal id As String) As String
'        '    ThrowIfDisposed()
'        '    Dim parentLinkLength = If(parentLink?.Length, 0)
'        '    Dim idUriEscaped = Uri.EscapeUriString(id)
'        '    Debug.Assert(parentLinkLength = 0 OrElse Not parentLink.EndsWith("/"))
'        '    Dim stringBuilder As StringBuilder = New StringBuilder(parentLinkLength + 2 + uriPathSegment.Length + idUriEscaped.Length)

'        '    If parentLinkLength > 0 Then
'        '        stringBuilder.Append(parentLink)
'        '        stringBuilder.Append("/")
'        '    End If

'        '    stringBuilder.Append(uriPathSegment)
'        '    stringBuilder.Append("/")
'        '    stringBuilder.Append(idUriEscaped)
'        '    Return stringBuilder.ToString()
'        'End Function

'        'Friend Overrides Sub ValidateResource(ByVal resourceId As String)
'        '    ThrowIfDisposed()
'        '    DocumentClient.ValidateResource(resourceId)
'        'End Sub

'        'Friend Overrides Function OperationHelperAsync(Of TResult)(ByVal operationName As String, ByVal requestOptions As RequestOptions, ByVal task As Func(Of ITrace, Task(Of TResult)), ByVal Optional traceComponent As TraceComponent = TraceComponent.Transport, ByVal Optional traceLevel As Tracing.TraceLevel = Tracing.TraceLevel.Info) As Task(Of TResult)
'        '    Return If(SynchronizationContext.Current Is Nothing, Me.OperationHelperWithRootTraceAsync(operationName, requestOptions, task, traceComponent, traceLevel), Me.OperationHelperWithRootTraceWithSynchronizationContextAsync(operationName, requestOptions, task, traceComponent, traceLevel))
'        'End Function

'        'Private Async Function OperationHelperWithRootTraceAsync(Of TResult)(ByVal operationName As String, ByVal requestOptions As RequestOptions, ByVal task As Func(Of ITrace, Task(Of TResult)), ByVal traceComponent As TraceComponent, ByVal traceLevel As Tracing.TraceLevel) As Task(Of TResult)
'        '    Dim disableDiagnostics As Boolean = requestOptions IsNot Nothing AndAlso requestOptions.DisablePointOperationDiagnostics

'        '    Using trace As ITrace = If(disableDiagnostics, NoOpTrace.Singleton, CType(Tracing.Trace.GetRootTrace(operationName, traceComponent, traceLevel), ITrace))
'        '        trace.AddDatum("Client Configuration", clientField.ClientConfigurationTraceDatum)
'        '        Return Await Me.RunWithDiagnosticsHelperAsync(trace, task)
'        '    End Using
'        'End Function

'        'Private Function OperationHelperWithRootTraceWithSynchronizationContextAsync(Of TResult)(ByVal operationName As String, ByVal requestOptions As RequestOptions, ByVal task As Func(Of ITrace, Task(Of TResult)), ByVal traceComponent As TraceComponent, ByVal traceLevel As Tracing.TraceLevel) As Task(Of TResult)
'        '    Debug.Assert(SynchronizationContext.Current IsNot Nothing, "This should only be used when a SynchronizationContext is specified")
'        '    Dim syncContextVirtualAddress As String = SynchronizationContext.Current.ToString()

'        '    ' Used on NETFX applications with SynchronizationContext when doing locking calls
'        '    Return Tasks.Task.Run(Async Function()
'        '                              Dim disableDiagnostics As Boolean = requestOptions IsNot Nothing AndAlso requestOptions.DisablePointOperationDiagnostics

'        '                              Using trace As ITrace = If(disableDiagnostics, NoOpTrace.Singleton, CType(Tracing.Trace.GetRootTrace(operationName, traceComponent, traceLevel), ITrace))
'        '                                  trace.AddDatum("Synchronization Context", syncContextVirtualAddress)
'        '                                  Return Await Me.RunWithDiagnosticsHelperAsync(trace, task)
'        '                              End Using
'        '                          End Function)
'        'End Function

'        'Friend Overloads Overrides Function ProcessResourceOperationStreamAsync(ByVal resourceUri As String, ByVal resourceType As ResourceType, ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal cosmosContainerCore As ContainerInternal, ByVal partitionKey As PartitionKey?, ByVal itemId As String, ByVal streamPayload As Stream, ByVal requestEnricher As Action(Of RequestMessage), ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of ResponseMessage)
'        '    ThrowIfDisposed()

'        '    If Me.IsBulkOperationSupported(resourceType, operationType) Then
'        '        If Not partitionKey.HasValue Then
'        '            Throw New ArgumentOutOfRangeException(NameOf(partitionKey))
'        '        End If

'        '        If requestEnricher IsNot Nothing Then
'        '            Throw New ArgumentException($"Bulk does not support {NameOf(requestEnricher)}")
'        '        End If

'        '        Return Me.ProcessResourceOperationAsBulkStreamAsync(operationType:=operationType, requestOptions:=requestOptions, cosmosContainerCore:=cosmosContainerCore, partitionKey:=partitionKey.Value, itemId:=itemId, streamPayload:=streamPayload, trace:=trace, cancellationToken:=cancellationToken)
'        '    End If

'        '    Return Me.ProcessResourceOperationStreamAsync(resourceUri:=resourceUri, resourceType:=resourceType, operationType:=operationType, requestOptions:=requestOptions, cosmosContainerCore:=cosmosContainerCore, feedRange:=If(partitionKey.HasValue, New FeedRangePartitionKey(partitionKey.Value), Nothing), streamPayload:=streamPayload, requestEnricher:=requestEnricher, trace:=trace, cancellationToken:=cancellationToken)
'        'End Function

'        'Friend Overloads Overrides Function ProcessResourceOperationStreamAsync(ByVal resourceUri As String, ByVal resourceType As ResourceType, ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal cosmosContainerCore As ContainerInternal, ByVal feedRange As FeedRange, ByVal streamPayload As Stream, ByVal requestEnricher As Action(Of RequestMessage), ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of ResponseMessage)
'        '    ThrowIfDisposed()
'        '    Return RequestHandler.SendAsync(resourceUriString:=resourceUri, resourceType:=resourceType, operationType:=operationType, requestOptions:=requestOptions, cosmosContainerCore:=cosmosContainerCore, feedRange:=feedRange, streamPayload:=streamPayload, requestEnricher:=requestEnricher, trace:=trace, cancellationToken:=cancellationToken)
'        'End Function

'        'Friend Overrides Function ProcessResourceOperationAsync(Of T)(ByVal resourceUri As String, ByVal resourceType As ResourceType, ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal cosmosContainerCore As ContainerInternal, ByVal feedRange As FeedRange, ByVal streamPayload As Stream, ByVal requestEnricher As Action(Of RequestMessage), ByVal responseCreator As Func(Of ResponseMessage, T), ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of T)
'        '    ThrowIfDisposed()
'        '    Return RequestHandler.SendAsync(Of T)(resourceUri:=resourceUri, resourceType:=resourceType, operationType:=operationType, requestOptions:=requestOptions, cosmosContainerCore:=cosmosContainerCore, feedRange:=feedRange, streamPayload:=streamPayload, requestEnricher:=requestEnricher, responseCreator:=responseCreator, trace:=trace, cancellationToken:=cancellationToken)
'        'End Function

'        'Friend Overrides Async Function GetCachedContainerPropertiesAsync(ByVal containerUri As String, ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of ContainerProperties)
'        '    Using childTrace As ITrace = trace.StartChild("Get Container Properties", TraceComponent.Transport, Tracing.TraceLevel.Info)
'        '        ThrowIfDisposed()
'        '        Dim collectionCache As ClientCollectionCache = Await DocumentClient.GetCollectionCacheAsync(childTrace)

'        '        Try
'        '            Return Await collectionCache.ResolveByNameAsync(HttpConstants.Versions.CurrentVersion, containerUri, forceRefesh:=False, trace:=childTrace, clientSideRequestStatistics:=Nothing, cancellationToken:=cancellationToken)
'        '        Catch ex As DocumentClientException
'        '            Throw CosmosExceptionFactory.Create(ex, childTrace)
'        '        End Try
'        '    End Using
'        'End Function

'        'Friend Overrides Function GetExecutorForContainer(ByVal container As ContainerInternal) As BatchAsyncContainerExecutor
'        '    ThrowIfDisposed()

'        '    If Not ClientOptions.AllowBulkExecution Then
'        '        Return Nothing
'        '    End If

'        '    Return batchExecutorCache.GetExecutorForContainer(container, Me)
'        'End Function

'        Public Overrides Sub Dispose()
'            Dispose(True)
'        End Sub

'        '''' <summary>
'        '''' Dispose of cosmos client
'        '''' </summary>
'        '''' <paramname="disposing">True if disposing</param>
'        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
'            If Not isDisposed Then
'                If disposing Then
'                    'telemetry?.Dispose()
'                    'batchExecutorCache.Dispose()
'                    'DocumentClient.Dispose()
'                End If

'                isDisposed = True
'            End If
'        End Sub

'        'Private Async Function RunWithDiagnosticsHelperAsync(Of TResult)(ByVal trace As ITrace, ByVal task As Func(Of ITrace, Task(Of TResult))) As Task(Of TResult)
'        '    Using New ActivityScope(Guid.NewGuid())

'        '        Try
'        '            Return Await task(trace).ConfigureAwait(False)
'        '        Catch oe As OperationCanceledException When Not (TypeOf oe Is CosmosOperationCanceledException)
'        '            Throw New CosmosOperationCanceledException(oe, trace)
'        '        Catch objectDisposed As ObjectDisposedException When Not (TypeOf objectDisposed Is CosmosObjectDisposedException)
'        '            Throw New CosmosObjectDisposedException(objectDisposed, clientField, trace)
'        '        Catch nullRefException As NullReferenceException When Not (TypeOf nullRefException Is CosmosNullReferenceException)
'        '            Throw New CosmosNullReferenceException(nullRefException, trace)
'        '        End Try
'        '    End Using
'        'End Function

'        'Private Async Function ProcessResourceOperationAsBulkStreamAsync(ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal cosmosContainerCore As ContainerInternal, ByVal partitionKey As PartitionKey, ByVal itemId As String, ByVal streamPayload As Stream, ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of ResponseMessage)
'        '    ThrowIfDisposed()
'        '    Dim itemRequestOptions As ItemRequestOptions = TryCast(requestOptions, ItemRequestOptions)
'        '    Dim batchItemRequestOptions As TransactionalBatchItemRequestOptions = TransactionalBatchItemRequestOptions.FromItemRequestOptions(itemRequestOptions)
'        '    Dim itemBatchOperation As ItemBatchOperation = New ItemBatchOperation(operationType:=operationType, operationIndex:=0, partitionKey:=partitionKey, id:=itemId, resourceStream:=streamPayload, requestOptions:=batchItemRequestOptions, cosmosClientContext:=Me)
'        '    Dim batchOperationResult As TransactionalBatchOperationResult = Await cosmosContainerCore.BatchExecutor.AddAsync(itemBatchOperation, trace, itemRequestOptions, cancellationToken)
'        '    Return batchOperationResult.ToResponseMessage()
'        'End Function

'        'Private Function IsBulkOperationSupported(ByVal resourceType As ResourceType, ByVal operationType As OperationType) As Boolean
'        '    ThrowIfDisposed()

'        '    If Not ClientOptions.AllowBulkExecution Then
'        '        Return False
'        '    End If

'        '    Return resourceType Is resourceType.Document AndAlso (operationType Is operationType.Create OrElse operationType Is operationType.Upsert OrElse operationType Is operationType.Read OrElse operationType Is operationType.Delete OrElse operationType Is operationType.Replace OrElse operationType Is operationType.Patch)
'        'End Function

'        Private Shared Function CreateOrCloneClientOptions(ByVal clientOptions As CosmosClientOptions) As CosmosClientOptions
'            If clientOptions Is Nothing Then
'                Return New CosmosClientOptions()
'            End If

'            Return clientOptions.Clone()
'        End Function

'        'Friend Function ThrowIfDisposed(Of T)(ByVal input As T) As T
'        '    ThrowIfDisposed()
'        '    Return input
'        'End Function

'        'Private Sub ThrowIfDisposed()
'        '    If isDisposed Then
'        '        Throw New ObjectDisposedException($"Accessing {NameOf(CosmosClient)} after it is disposed is invalid.")
'        '    End If
'        'End Sub
'    End Class
'End Namespace
