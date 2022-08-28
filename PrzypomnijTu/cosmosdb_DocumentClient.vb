''------------------------------------------------------------
'' Copyright (c) Microsoft Corporation.  All rights reserved.
''------------------------------------------------------------

''Imports System
''Imports System.Diagnostics
'Imports System.Net.Http
''Imports System.Security
''Imports System.Threading.Tasks
''Imports Global.Azure.Core
''Imports Microsoft.Azure.Cosmos.Common
''Imports Microsoft.Azure.Cosmos.Core.Trace
''Imports Microsoft.Azure.Cosmos.Query
''Imports Microsoft.Azure.Cosmos.Query.Core.QueryPlan
''Imports Microsoft.Azure.Cosmos.Routing
''Imports Microsoft.Azure.Cosmos.Tracing
''Imports Microsoft.Azure.Cosmos.Tracing.TraceData
''Imports Microsoft.Azure.Documents
''Imports Microsoft.Azure.Documents.Client
''Imports Microsoft.Azure.Documents.Collections
''Imports Microsoft.Azure.Documents.Routing
''Imports Newtonsoft.Json

'Namespace Microsoft.Azure.Cosmos

'    ''' <summary>
'    ''' Provides a client-side logical representation for the Azure Cosmos DB service.
'    ''' This client is used to configure and execute requests against the service.
'    ''' </summary>
'    ''' <threadSafety>
'    ''' This type is thread safe.
'    ''' </threadSafety>
'    ''' <remarks>
'    ''' The service client that encapsulates the endpoint and credentials and connection policy used to access the Azure Cosmos DB service.
'    ''' It is recommended to cache and reuse this instance within your application rather than creating a new instance for every operation.
'    '''
'    ''' <para>
'    ''' When your app uses DocumentClient, you should call its IDisposable.Dispose implementation when you are finished using it.
'    ''' Depending on your programming technique, you can do this in one of two ways:
'    ''' </para>
'    '''
'    ''' <para>
'    ''' 1. By using a language construct such as the using statement in C#.
'    ''' The using statement is actually a syntactic convenience.
'    ''' At compile time, the language compiler implements the intermediate language (IL) for a try/catch block.
'    ''' <codelanguage="c#">
'    ''' <![CDATA[
'    ''' using (IDocumentClient client = new DocumentClient(new Uri("endpoint"), "authKey"))
'    ''' {
'    '''     ...
'    ''' }
'    ''' ]]>
'    ''' </code>
'    ''' </para>
'    '''
'    ''' <para>
'    ''' 2. By wrapping the call to the IDisposable.Dispose implementation in a try/catch block.
'    ''' The following example replaces the using block in the previous example with a try/catch/finally block.
'    ''' <codelanguage="c#">
'    ''' <![CDATA[
'    ''' IDocumentClient client = new DocumentClient(new Uri("endpoint"), "authKey"))
'    ''' try{
'    '''     ...
'    ''' }
'    ''' finally{
'    '''     if (client != null) client.Dispose();
'    ''' }
'    ''' ]]>
'    ''' </code>
'    ''' </para>
'    '''
'    ''' </remarks>
'    Partial Friend Class DocumentClient
'        Implements IDisposable, IAuthorizationTokenProvider, ICosmosAuthorizationTokenProvider, IDocumentClient, IDocumentClientInternal

'        'Private _httpClient As CosmosHttpClient
'        'Private Const AllowOverrideStrongerConsistency As String = "AllowOverrideStrongerConsistency"
'        'Private Const MaxConcurrentConnectionOpenConfig As String = "MaxConcurrentConnectionOpenRequests"
'        'Private Const IdleConnectionTimeoutInSecondsConfig As String = "IdleConnectionTimeoutInSecondsConfig"
'        'Private Const OpenConnectionTimeoutInSecondsConfig As String = "OpenConnectionTimeoutInSecondsConfig"
'        'Private Const TransportTimerPoolGranularityInSecondsConfig As String = "TransportTimerPoolGranularityInSecondsConfig"
'        'Private Const EnableTcpChannelConfig As String = "CosmosDbEnableTcpChannel"
'        'Private Const MaxRequestsPerChannelConfig As String = "CosmosDbMaxRequestsPerTcpChannel"
'        'Private Const TcpPartitionCount As String = "CosmosDbTcpPartitionCount"
'        'Private Const MaxChannelsPerHostConfig As String = "CosmosDbMaxTcpChannelsPerHost"
'        'Private Const RntbdPortReuseMode As String = "CosmosDbTcpPortReusePolicy"
'        'Private Const RntbdPortPoolReuseThreshold As String = "CosmosDbTcpPortReuseThreshold"
'        'Private Const RntbdPortPoolBindAttempts As String = "CosmosDbTcpPortReuseBindAttempts"
'        'Private Const RntbdReceiveHangDetectionTimeConfig As String = "CosmosDbTcpReceiveHangDetectionTimeSeconds"
'        'Private Const RntbdSendHangDetectionTimeConfig As String = "CosmosDbTcpSendHangDetectionTimeSeconds"
'        'Private Const EnableCpuMonitorConfig As String = "CosmosDbEnableCpuMonitor"
'        '' Env variable
'        'Private Const RntbdMaxConcurrentOpeningConnectionCountConfig As String = "AZURE_COSMOS_TCP_MAX_CONCURRENT_OPENING_CONNECTION_COUNT"
'        'Private Const MaxConcurrentConnectionOpenRequestsPerProcessor As Integer = 25
'        'Private Const DefaultMaxRequestsPerRntbdChannel As Integer = 30
'        'Private Const DefaultRntbdPartitionCount As Integer = 1
'        'Private Const DefaultMaxRntbdChannelsPerHost As Integer = UShort.MaxValue
'        'Private Const DefaultRntbdPortReuseMode As PortReuseMode = PortReuseMode.ReuseUnicastPort
'        'Private Const DefaultRntbdPortPoolReuseThreshold As Integer = 256
'        'Private Const DefaultRntbdPortPoolBindAttempts As Integer = 5
'        'Private Const DefaultRntbdReceiveHangDetectionTimeSeconds As Integer = 65
'        'Private Const DefaultRntbdSendHangDetectionTimeSeconds As Integer = 10
'        'Private Const DefaultEnableCpuMonitor As Boolean = True

'        ''Auth
'        'Private ReadOnly cosmosAuthorization As AuthorizationTokenProvider

'        '' Gateway has backoff/retry logic to hide transient errors.
'        'Private retryPolicy As RetryPolicy
'        'Private allowOverrideStrongerConsistencyField As Boolean = False
'        'Private maxConcurrentConnectionOpenRequests As Integer = Environment.ProcessorCount * MaxConcurrentConnectionOpenRequestsPerProcessor
'        'Private openConnectionTimeoutInSeconds As Integer = 5
'        'Private idleConnectionTimeoutInSeconds As Integer = -1
'        'Private timerPoolGranularityInSeconds As Integer = 1
'        'Private enableRntbdChannel As Boolean = True
'        'Private maxRequestsPerRntbdChannel As Integer = DefaultMaxRequestsPerRntbdChannel
'        'Private rntbdPartitionCount As Integer = DefaultRntbdPartitionCount
'        'Private maxRntbdChannels As Integer = DefaultMaxRntbdChannelsPerHost
'        'Private rntbdPortReuseModeField As PortReuseMode = DefaultRntbdPortReuseMode
'        'Private rntbdPortPoolReuseThresholdField As Integer = DefaultRntbdPortPoolReuseThreshold
'        'Private rntbdPortPoolBindAttemptsField As Integer = DefaultRntbdPortPoolBindAttempts
'        'Private rntbdReceiveHangDetectionTimeSeconds As Integer = DefaultRntbdReceiveHangDetectionTimeSeconds
'        'Private rntbdSendHangDetectionTimeSeconds As Integer = DefaultRntbdSendHangDetectionTimeSeconds
'        'Private enableCpuMonitor As Boolean = DefaultEnableCpuMonitor
'        'Private rntbdMaxConcurrentOpeningConnectionCount As Integer = 5

'        ''Consistency
'        'Private desiredConsistencyLevel As Documents.ConsistencyLevel?
'        'Private accountServiceConfiguration As CosmosAccountServiceConfiguration
'        'Private collectionCache As ClientCollectionCache
'        'Private partitionKeyRangeCache As PartitionKeyRangeCache

'        ''Private state.
'        'Private isSuccessfullyInitialized As Boolean
'        'Private isDisposed As Boolean
'        'Private initializationSyncLock As Object  ' guards initializeTask
'        '' creator of TransportClient is responsible for disposing it.
'        'Private storeClientFactory As IStoreClientFactory

'        'Friend Property httpClient As CosmosHttpClient
'        '    Get
'        '        Return _httpClient
'        '    End Get
'        '    Private Set(ByVal value As CosmosHttpClient)
'        '        _httpClient = value
'        '    End Set
'        'End Property

'        '' Flag that indicates whether store client factory must be disposed whenever client is disposed.
'        '' Setting this flag to false will result in store client factory not being disposed when client is disposed.
'        '' This flag is used to allow shared store client factory survive disposition of a document client while other clients continue using it.
'        'Private isStoreClientFactoryCreatedInternally As Boolean

'        ''Id counter.
'        'Private Shared idCounter As Integer
'        ''Trace Id.
'        'Private traceId As Integer

'        ''SessionContainer.
'        'Friend sessionContainer As ISessionContainer
'        'Private queryPartitionProvider As AsyncLazy(Of QueryPartitionProvider)
'        'Private eventSource As DocumentClientEventSource
'        'Friend initializeTask As Task
'        'Private serializerSettings As JsonSerializerSettings
'        'Private Event sendingRequest As EventHandler(Of SendingRequestEventArgs)
'        'Private Event receivedResponse As EventHandler(Of ReceivedResponseEventArgs)
'        'Private transportClientHandlerFactory As Func(Of TransportClient, TransportClient)

'        ''' <summary>
'        ''' Initializes a new instance of the <seecref="DocumentClient"/> class using the
'        ''' specified Azure Cosmos DB service endpoint, key, and connection policy for the Azure Cosmos DB service.
'        ''' </summary>
'        ''' <paramname="serviceEndpoint">
'        ''' The service endpoint to use to create the client.
'        ''' </param>
'        ''' <paramname="authKey">
'        ''' The list of Permission objects to use to create the client.
'        ''' </param>
'        ''' <paramname="connectionPolicy">
'        ''' (Optional) The connection policy for the client. If none is passed, the default is used <seecref="ConnectionPolicy"/>
'        ''' </param>
'        ''' <paramname="desiredConsistencyLevel">
'        ''' (Optional) This can be used to weaken the database account consistency level for read operations.
'        ''' If this is not set the database account consistency level will be used for all requests.
'        ''' </param>
'        ''' <remarks>
'        ''' The service endpoint and the authorization key can be obtained from the Azure Management Portal.
'        ''' The authKey used here is encrypted for privacy when being used, and deleted from computer memory when no longer needed
'        ''' <para>
'        ''' Using Direct connectivity, wherever possible, is recommended
'        ''' </para>
'        ''' </remarks>
'        ''' <seealsocref="Uri"/>
'        ''' <seealsocref="SecureString"/>
'        ''' <seealsocref="ConnectionPolicy"/>
'        ''' <seealsocref="ConsistencyLevel"/>
'        Public Sub New(ByVal serviceEndpoint As Uri, ByVal authKey As SecureString, ByVal Optional connectionPolicy As ConnectionPolicy = Nothing, ByVal Optional desiredConsistencyLevel As Documents.ConsistencyLevel? = Nothing)
'            If authKey Is Nothing Then
'                Throw New ArgumentNullException("authKey")
'            End If

'            If authKey IsNot Nothing Then
'                cosmosAuthorization = New AuthorizationTokenProviderMasterKey(authKey)
'            End If

'            Me.Initialize(serviceEndpoint, connectionPolicy, desiredConsistencyLevel)
'        End Sub

'        ''' <summary>
'        ''' Initializes a new instance of the <seecref="DocumentClient"/> class using the
'        ''' specified Azure Cosmos DB service endpoint, key, connection policy and a custom JsonSerializerSettings
'        ''' for the Azure Cosmos DB service.
'        ''' </summary>
'        ''' <paramname="serviceEndpoint">
'        ''' The service endpoint to use to create the client.
'        ''' </param>
'        ''' <paramname="authKey">
'        ''' The list of Permission objects to use to create the client.
'        ''' </param>
'        ''' <paramname="connectionPolicy">
'        ''' The connection policy for the client.
'        ''' </param>
'        ''' <paramname="desiredConsistencyLevel">
'        ''' This can be used to weaken the database account consistency level for read operations.
'        ''' If this is not set the database account consistency level will be used for all requests.
'        ''' </param>
'        ''' <paramname="serializerSettings">
'        ''' The custom JsonSerializer settings to be used for serialization/derialization.
'        ''' </param>
'        ''' <remarks>
'        ''' The service endpoint and the authorization key can be obtained from the Azure Management Portal.
'        ''' The authKey used here is encrypted for privacy when being used, and deleted from computer memory when no longer needed
'        ''' <para>
'        ''' Using Direct connectivity, wherever possible, is recommended
'        ''' </para>
'        ''' </remarks>
'        ''' <seealsocref="Uri"/>
'        ''' <seealsocref="SecureString"/>
'        ''' <seealsocref="ConnectionPolicy"/>
'        ''' <seealsocref="ConsistencyLevel"/>
'        ''' <seealsocref="JsonSerializerSettings"/>
'        <Obsolete("Please use the constructor that takes JsonSerializerSettings as the third parameter.")>
'        Public Sub New(ByVal serviceEndpoint As Uri, ByVal authKey As SecureString, ByVal connectionPolicy As ConnectionPolicy, ByVal desiredConsistencyLevel As Documents.ConsistencyLevel?, ByVal serializerSettings As JsonSerializerSettings)
'            Me.New(serviceEndpoint, authKey, connectionPolicy, desiredConsistencyLevel)
'            Me.serializerSettings = serializerSettings
'        End Sub

'        ''' <summary>
'        ''' Initializes a new instance of the <seecref="DocumentClient"/> class using the
'        ''' specified Azure Cosmos DB service endpoint, key, connection policy and a custom JsonSerializerSettings
'        ''' for the Azure Cosmos DB service.
'        ''' </summary>
'        ''' <paramname="serviceEndpoint">
'        ''' The service endpoint to use to create the client.
'        ''' </param>
'        ''' <paramname="authKey">
'        ''' The list of Permission objects to use to create the client.
'        ''' </param>
'        ''' <paramname="serializerSettings">
'        ''' The custom JsonSerializer settings to be used for serialization/derialization.
'        ''' </param>
'        ''' <paramname="connectionPolicy">
'        ''' (Optional) The connection policy for the client. If none is passed, the default is used <seecref="ConnectionPolicy"/>
'        ''' </param>
'        ''' <paramname="desiredConsistencyLevel">
'        ''' (Optional) This can be used to weaken the database account consistency level for read operations.
'        ''' If this is not set the database account consistency level will be used for all requests.
'        ''' </param>
'        ''' <remarks>
'        ''' The service endpoint and the authorization key can be obtained from the Azure Management Portal.
'        ''' The authKey used here is encrypted for privacy when being used, and deleted from computer memory when no longer needed
'        ''' <para>
'        ''' Using Direct connectivity, wherever possible, is recommended
'        ''' </para>
'        ''' </remarks>
'        ''' <seealsocref="Uri"/>
'        ''' <seealsocref="SecureString"/>
'        ''' <seealsocref="JsonSerializerSettings"/>
'        ''' <seealsocref="ConnectionPolicy"/>
'        ''' <seealsocref="ConsistencyLevel"/>
'        Public Sub New(ByVal serviceEndpoint As Uri, ByVal authKey As SecureString, ByVal serializerSettings As JsonSerializerSettings, ByVal Optional connectionPolicy As ConnectionPolicy = Nothing, ByVal Optional desiredConsistencyLevel As Documents.ConsistencyLevel? = Nothing)
'            Me.New(serviceEndpoint, authKey, connectionPolicy, desiredConsistencyLevel)
'            Me.serializerSettings = serializerSettings
'        End Sub

'        ''' <summary>
'        ''' Initializes a new instance of the <seecref="DocumentClient"/> class using the
'        ''' specified service endpoint, an authorization key (or resource token) and a connection policy
'        ''' for the Azure Cosmos DB service.
'        ''' </summary>
'        ''' <paramname="serviceEndpoint">The service endpoint to use to create the client.</param>
'        ''' <paramname="authKeyOrResourceToken">The authorization key or resource token to use to create the client.</param>
'        ''' <paramname="connectionPolicy">(Optional) The connection policy for the client.</param>
'        ''' <paramname="desiredConsistencyLevel">(Optional) The default consistency policy for client operations.</param>
'        ''' <remarks>
'        ''' The service endpoint can be obtained from the Azure Management Portal.
'        ''' If you are connecting using one of the Master Keys, these can be obtained along with the endpoint from the Azure Management Portal
'        ''' If however you are connecting as a specific Azure Cosmos DB User, the value passed to <paramrefname="authKeyOrResourceToken"/> is the ResourceToken obtained from the permission feed for the user.
'        ''' <para>
'        ''' Using Direct connectivity, wherever possible, is recommended.
'        ''' </para>
'        ''' </remarks>
'        ''' <seealsocref="Uri"/>
'        ''' <seealsocref="ConnectionPolicy"/>
'        ''' <seealsocref="ConsistencyLevel"/>
'        Public Sub New(ByVal serviceEndpoint As Uri, ByVal authKeyOrResourceToken As String, ByVal Optional connectionPolicy As ConnectionPolicy = Nothing, ByVal Optional desiredConsistencyLevel As Documents.ConsistencyLevel? = Nothing)
'            Me.New(serviceEndpoint, authKeyOrResourceToken, sendingRequestEventArgs:=Nothing, connectionPolicy:=connectionPolicy, desiredConsistencyLevel:=desiredConsistencyLevel)
'        End Sub

'        ''' <summary>
'        ''' Initializes a new instance of the <seecref="DocumentClient"/> class using the
'        ''' specified service endpoint, an authorization key (or resource token) and a connection policy
'        ''' for the Azure Cosmos DB service.
'        ''' </summary>
'        ''' <paramname="serviceEndpoint">The service endpoint to use to create the client.</param>
'        ''' <paramname="authKeyOrResourceToken">The authorization key or resource token to use to create the client.</param>
'        ''' <paramname="handler">The HTTP handler stack to use for sending requests (e.g., HttpClientHandler).</param>
'        ''' <paramname="connectionPolicy">(Optional) The connection policy for the client.</param>
'        ''' <paramname="desiredConsistencyLevel">(Optional) The default consistency policy for client operations.</param>
'        ''' <remarks>
'        ''' The service endpoint can be obtained from the Azure Management Portal.
'        ''' If you are connecting using one of the Master Keys, these can be obtained along with the endpoint from the Azure Management Portal
'        ''' If however you are connecting as a specific Azure Cosmos DB User, the value passed to <paramrefname="authKeyOrResourceToken"/> is the ResourceToken obtained from the permission feed for the user.
'        ''' <para>
'        ''' Using Direct connectivity, wherever possible, is recommended.
'        ''' </para>
'        ''' </remarks>
'        ''' <seealsocref="Uri"/>
'        ''' <seealsocref="ConnectionPolicy"/>
'        ''' <seealsocref="ConsistencyLevel"/>
'        Public Sub New(ByVal serviceEndpoint As Uri, ByVal authKeyOrResourceToken As String, ByVal handler As HttpMessageHandler, ByVal Optional connectionPolicy As ConnectionPolicy = Nothing, ByVal Optional desiredConsistencyLevel As Documents.ConsistencyLevel? = Nothing)
'            Me.New(serviceEndpoint, authKeyOrResourceToken, sendingRequestEventArgs:=Nothing, connectionPolicy:=connectionPolicy, desiredConsistencyLevel:=desiredConsistencyLevel, handler:=handler)
'        End Sub

'        Friend Sub New(ByVal serviceEndpoint As Uri, ByVal authKeyOrResourceToken As String, ByVal sendingRequestEventArgs As EventHandler(Of SendingRequestEventArgs), ByVal Optional connectionPolicy As ConnectionPolicy = Nothing, ByVal Optional desiredConsistencyLevel As Documents.ConsistencyLevel? = Nothing, ByVal Optional serializerSettings As JsonSerializerSettings = Nothing, ByVal Optional apitype As ApiType = ApiType.None, ByVal Optional receivedResponseEventArgs As EventHandler(Of ReceivedResponseEventArgs) = Nothing, ByVal Optional handler As HttpMessageHandler = Nothing, ByVal Optional sessionContainer As ISessionContainer = Nothing, ByVal Optional enableCpuMonitor As Boolean? = Nothing, ByVal Optional transportClientHandlerFactory As Func(Of TransportClient, TransportClient) = Nothing, ByVal Optional storeClientFactory As IStoreClientFactory = Nothing)
'            Me.New(serviceEndpoint, AuthorizationTokenProvider.CreateWithResourceTokenOrAuthKey(authKeyOrResourceToken), sendingRequestEventArgs, connectionPolicy, desiredConsistencyLevel, serializerSettings, apitype, receivedResponseEventArgs, handler, sessionContainer, enableCpuMonitor, transportClientHandlerFactory, storeClientFactory)
'        End Sub

'        ''' <summary>
'        ''' Initializes a new instance of the <seecref="DocumentClient"/> class using the
'        ''' specified service endpoint, an authorization key (or resource token) and a connection policy
'        ''' for the Azure Cosmos DB service.
'        ''' </summary>
'        ''' <paramname="serviceEndpoint">The service endpoint to use to create the client.</param>
'        ''' <paramname="cosmosAuthorization">The cosmos authorization for the client.</param>
'        ''' <paramname="sendingRequestEventArgs"> The event handler to be invoked before the request is sent.</param>
'        ''' <paramname="receivedResponseEventArgs"> The event handler to be invoked after a response has been received.</param>
'        ''' <paramname="connectionPolicy">(Optional) The connection policy for the client.</param>
'        ''' <paramname="desiredConsistencyLevel">(Optional) The default consistency policy for client operations.</param>
'        ''' <paramname="serializerSettings">The custom JsonSerializer settings to be used for serialization/derialization.</param>
'        ''' <paramname="apitype">Api type for the account</param>
'        ''' <paramname="handler">The HTTP handler stack to use for sending requests (e.g., HttpClientHandler).</param>
'        ''' <paramname="sessionContainer">The default session container with which DocumentClient is created.</param>
'        ''' <paramname="enableCpuMonitor">Flag that indicates whether client-side CPU monitoring is enabled for improved troubleshooting.</param>
'        ''' <paramname="transportClientHandlerFactory">Transport client handler factory.</param>
'        ''' <paramname="storeClientFactory">Factory that creates store clients sharing the same transport client to optimize network resource reuse across multiple document clients in the same process.</param>
'        ''' <remarks>
'        ''' The service endpoint can be obtained from the Azure Management Portal.
'        ''' If you are connecting using one of the Master Keys, these can be obtained along with the endpoint from the Azure Management Portal
'        ''' If however you are connecting as a specific Azure Cosmos DB User, the value passed to is the ResourceToken obtained from the permission feed for the user.
'        ''' <para>
'        ''' Using Direct connectivity, wherever possible, is recommended.
'        ''' </para>
'        ''' </remarks>
'        ''' <seealsocref="Uri"/>
'        ''' <seealsocref="ConnectionPolicy"/>
'        ''' <seealsocref="ConsistencyLevel"/>
'        Friend Sub New(ByVal serviceEndpoint As Uri, ByVal cosmosAuthorization As AuthorizationTokenProvider, ByVal sendingRequestEventArgs As EventHandler(Of SendingRequestEventArgs), ByVal Optional connectionPolicy As ConnectionPolicy = Nothing, ByVal Optional desiredConsistencyLevel As Documents.ConsistencyLevel? = Nothing, ByVal Optional serializerSettings As JsonSerializerSettings = Nothing, ByVal Optional apitype As ApiType = ApiType.None, ByVal Optional receivedResponseEventArgs As EventHandler(Of ReceivedResponseEventArgs) = Nothing, ByVal Optional handler As HttpMessageHandler = Nothing, ByVal Optional sessionContainer As ISessionContainer = Nothing, ByVal Optional enableCpuMonitor As Boolean? = Nothing, ByVal Optional transportClientHandlerFactory As Func(Of TransportClient, TransportClient) = Nothing, ByVal Optional storeClientFactory As IStoreClientFactory = Nothing)
'            If sendingRequestEventArgs IsNot Nothing Then
'                AddHandler sendingRequest, sendingRequestEventArgs
'            End If

'            If serializerSettings IsNot Nothing Then
'                Me.serializerSettings = serializerSettings
'            End If

'            Me.ApiType = apitype

'            If receivedResponseEventArgs IsNot Nothing Then
'                AddHandler receivedResponse, receivedResponseEventArgs
'            End If

'            Me.cosmosAuthorization = If(cosmosAuthorization, CSharpImpl.__Throw(Of Object)(New ArgumentNullException(NameOf(cosmosAuthorization))))
'            Me.transportClientHandlerFactory = transportClientHandlerFactory
'            Me.Initialize(serviceEndpoint:=serviceEndpoint, connectionPolicy:=connectionPolicy, desiredConsistencyLevel:=desiredConsistencyLevel, handler:=handler, sessionContainer:=sessionContainer, enableCpuMonitor:=enableCpuMonitor, storeClientFactory:=storeClientFactory)
'        End Sub

'        ''' <summary>
'        ''' Initializes a new instance of the <seecref="DocumentClient"/> class using the
'        ''' specified service endpoint, an authorization key (or resource token), a connection policy
'        ''' and a custom JsonSerializerSettings for the Azure Cosmos DB service.
'        ''' </summary>
'        ''' <paramname="serviceEndpoint">The service endpoint to use to create the client.</param>
'        ''' <paramname="authKeyOrResourceToken">The authorization key or resource token to use to create the client.</param>
'        ''' <paramname="connectionPolicy">The connection policy for the client.</param>
'        ''' <paramname="desiredConsistencyLevel">The default consistency policy for client operations.</param>
'        ''' <paramname="serializerSettings">The custom JsonSerializer settings to be used for serialization/derialization.</param>
'        ''' <remarks>
'        ''' The service endpoint can be obtained from the Azure Management Portal.
'        ''' If you are connecting using one of the Master Keys, these can be obtained along with the endpoint from the Azure Management Portal
'        ''' If however you are connecting as a specific Azure Cosmos DB User, the value passed to <paramrefname="authKeyOrResourceToken"/> is the ResourceToken obtained from the permission feed for the user.
'        ''' <para>
'        ''' Using Direct connectivity, wherever possible, is recommended.
'        ''' </para>
'        ''' </remarks>
'        ''' <seealsocref="Uri"/>
'        ''' <seealsocref="ConnectionPolicy"/>
'        ''' <seealsocref="ConsistencyLevel"/>
'        ''' <seealsocref="JsonSerializerSettings"/>
'        <Obsolete("Please use the constructor that takes JsonSerializerSettings as the third parameter.")>
'        Public Sub New(ByVal serviceEndpoint As Uri, ByVal authKeyOrResourceToken As String, ByVal connectionPolicy As ConnectionPolicy, ByVal desiredConsistencyLevel As Documents.ConsistencyLevel?, ByVal serializerSettings As JsonSerializerSettings)
'            Me.New(serviceEndpoint, authKeyOrResourceToken, CType(Nothing, HttpMessageHandler), connectionPolicy, desiredConsistencyLevel)
'            Me.serializerSettings = serializerSettings
'        End Sub

'        ''' <summary>
'        ''' Initializes a new instance of the <seecref="DocumentClient"/> class using the
'        ''' specified service endpoint, an authorization key (or resource token), a connection policy
'        ''' and a custom JsonSerializerSettings for the Azure Cosmos DB service.
'        ''' </summary>
'        ''' <paramname="serviceEndpoint">The service endpoint to use to create the client.</param>
'        ''' <paramname="authKeyOrResourceToken">The authorization key or resource token to use to create the client.</param>
'        ''' <paramname="serializerSettings">The custom JsonSerializer settings to be used for serialization/derialization.</param>
'        ''' <paramname="connectionPolicy">(Optional) The connection policy for the client.</param>
'        ''' <paramname="desiredConsistencyLevel">(Optional) The default consistency policy for client operations.</param>
'        ''' <remarks>
'        ''' The service endpoint can be obtained from the Azure Management Portal.
'        ''' If you are connecting using one of the Master Keys, these can be obtained along with the endpoint from the Azure Management Portal
'        ''' If however you are connecting as a specific Azure Cosmos DB User, the value passed to <paramrefname="authKeyOrResourceToken"/> is the ResourceToken obtained from the permission feed for the user.
'        ''' <para>
'        ''' Using Direct connectivity, wherever possible, is recommended.
'        ''' </para>
'        ''' </remarks>
'        ''' <seealsocref="Uri"/>
'        ''' <seealsocref="JsonSerializerSettings"/>
'        ''' <seealsocref="ConnectionPolicy"/>
'        ''' <seealsocref="ConsistencyLevel"/>
'        Public Sub New(ByVal serviceEndpoint As Uri, ByVal authKeyOrResourceToken As String, ByVal serializerSettings As JsonSerializerSettings, ByVal Optional connectionPolicy As ConnectionPolicy = Nothing, ByVal Optional desiredConsistencyLevel As Documents.ConsistencyLevel? = Nothing)
'            Me.New(serviceEndpoint, authKeyOrResourceToken, CType(Nothing, HttpMessageHandler), connectionPolicy, desiredConsistencyLevel)
'            Me.serializerSettings = serializerSettings
'        End Sub

'        ''' <summary>
'        ''' Internal constructor purely for unit-testing
'        ''' </summary>
'        Friend Sub New(ByVal serviceEndpoint As Uri, ByVal authKey As String)
'            ' do nothing 
'            Me.ServiceEndpoint = serviceEndpoint
'            Me.ConnectionPolicy = New ConnectionPolicy()
'        End Sub

'        Friend Overridable Async Function GetCollectionCacheAsync(ByVal trace As ITrace) As Task(Of ClientCollectionCache)
'            Using childTrace As ITrace = trace.StartChild("Get Collection Cache", TraceComponent.Routing, Tracing.TraceLevel.Info)
'                Await Me.EnsureValidClientAsync(childTrace)
'                Return collectionCache
'            End Using
'        End Function

'        Private Class CSharpImpl
'            <Obsolete("Please refactor calling code to use normal throw statements")>
'            Shared Function __Throw(Of T)(ByVal e As Exception) As T
'                Throw e
'            End Function
'        End Class
'    End Class
'End Namespace
