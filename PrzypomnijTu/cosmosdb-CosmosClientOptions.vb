''------------------------------------------------------------
'' Copyright (c) Microsoft Corporation.  All rights reserved.
''------------------------------------------------------------

''Imports System
''Imports System.Collections.Generic
''Imports System.Collections.ObjectModel
''Imports System.Data.Common
''Imports System.Linq
'Imports System.Net
'Imports System.Net.Http
''Imports Microsoft.Azure.Cosmos.Fluent
''Imports Microsoft.Azure.Documents
''Imports Microsoft.Azure.Documents.Client
'Imports Newtonsoft.Json

'Namespace Microsoft.Azure.Cosmos

'    ''' <summary>
'    ''' Defines all the configurable options that the CosmosClient requires.
'    ''' </summary>
'    ''' <example>
'    ''' An example on how to configure the serialization option to ignore null values.
'    ''' <codelanguage="c#">
'    ''' <![CDATA[
'    ''' CosmosClientOptions clientOptions = new CosmosClientOptions()
'    ''' {
'    '''     SerializerOptions = new CosmosSerializationOptions(){
'    '''         IgnoreNullValues = true
'    '''     },
'    '''     _ConnectionMode = ConnectionMode.Gateway,
'    ''' };
'    ''' 
'    ''' CosmosClient client = new CosmosClient("endpoint", "key", clientOptions);
'    ''' ]]>
'    ''' </code>
'    ''' </example>
'    Public Class CosmosClientOptions
'        ''' <summary>
'        ''' Default connection mode
'        ''' </summary>
'        Private Const DefaultConnectionMode As ConnectionMode = ConnectionMode.Direct

'        '''' <summary>
'        '''' Default Protocol mode
'        '''' </summary>
'        Private Const DefaultProtocol As Protocol = Protocol.Tcp
'        'Private Const ConnectionStringAccountEndpoint As String = "AccountEndpoint"
'        'Private Const ConnectionStringAccountKey As String = "AccountKey"
'        Private Const DefaultApiType As ApiType = ApiType.None

'        '''' <summary>
'        '''' Default request timeout
'        '''' </summary>
'        Private gatewayModeMaxConnectionLimitField As Integer
'        'Private serializerOptionsField As CosmosSerializationOptions
'        'Private serializerInternal As CosmosSerializer
'        Private connectionModeField As ConnectionMode
'        Private connectionProtocolField As Protocol
'        'Private idleTcpConnectionTimeoutField As TimeSpan?
'        'Private openTcpConnectionTimeoutField As TimeSpan?
'        'Private maxRequestsPerTcpConnectionField As Integer?
'        'Private maxTcpConnectionsPerEndpointField As Integer?
'        'Private portReuseModeField As PortReuseMode?
'        Private webProxyField As IWebProxy
'        Private httpClientFactoryField As Func(Of HttpClient)

'        '''' <summary>
'        '''' Creates a new CosmosClientOptions
'        '''' </summary>
'        Public Sub New()
'            '    GatewayModeMaxConnectionLimit = ConnectionPolicy.[Default].MaxConnectionLimit
'            '    RequestTimeout = ConnectionPolicy.[Default].RequestTimeout
'            '    TokenCredentialBackgroundRefreshInterval = Nothing
'            _ConnectionMode = DefaultConnectionMode
'            ConnectionProtocol = DefaultProtocol
'            _ApiType = DefaultApiType
'            '    CustomHandlers = New Collection(Of RequestHandler)()
'        End Sub

'        '''' <summary>
'        '''' Get or set user-agent suffix to include with every Azure Cosmos DB service interaction.
'        '''' </summary>
'        '''' <remarks>
'        '''' Setting this property after sending any request won't have any effect.
'        '''' </remarks>
'        'Public Property ApplicationName As String

'        '''' <summary>
'        '''' Get or set session container for the client
'        '''' </summary>
'        Friend Property SessionContainer As ISessionContainer

'        '''' <summary>
'        '''' Gets or sets the location where the application is running. This will influence the SDK's choice for the Azure Cosmos DB service interaction.
'        '''' </summary>
'        '''' <remarks>
'        '''' When the specified region is available, the SDK will prefer it to perform operations. When the region specified is not available,
'        '''' the SDK auto-selects fallback regions based on proximity from the given region. When
'        '''' this property is not specified at all, the SDK uses the write region
'        '''' as the preferred region for all operations. See also 
'        '''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/sql/troubleshoot-sdk-availability">Diagnose
'        '''' and troubleshoot the availability of Cosmos SDKs</seealso> for more details.
'        '''' This configuration is an alternative to <seecref="ApplicationPreferredRegions"/>, either one can be set but not both.
'        '''' </remarks>
'        '''' <seealsocref="CosmosClientBuilder.WithApplicationRegion(String)"/>
'        '''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/high-availability#high-availability-with-cosmos-db-in-the-event-of-regional-outages">High availability on regional outages</seealso>
'        'Public Property ApplicationRegion As String

'        '''' <summary>
'        '''' Gets and sets the preferred regions for geo-replicated database accounts in the Azure Cosmos DB service. 
'        '''' </summary>
'        '''' <remarks>
'        '''' When this property is specified, the SDK will use the region list in the provided order to define the endpoint failover order.
'        '''' This configuration is an alternative to <seecref="ApplicationRegion"/>, either one can be set but not both.
'        '''' See also <seealsohref="https://docs.microsoft.com/azure/cosmos-db/sql/troubleshoot-sdk-availability">Diagnose
'        '''' and troubleshoot the availability of Cosmos SDKs</seealso> for more details.
'        '''' </remarks>
'        '''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/high-availability#high-availability-with-cosmos-db-in-the-event-of-regional-outages">High availability on regional outages</seealso>
'        'Public Property ApplicationPreferredRegions As IReadOnlyList(Of String)

'        '''' <summary>
'        '''' Get or set the maximum number of concurrent connections allowed for the target
'        '''' service endpoint in the Azure Cosmos DB service.
'        '''' </summary>
'        '''' <remarks>
'        '''' This setting is only applicable in Gateway mode.
'        '''' </remarks>
'        '''' <value>Default value is 50.</value>
'        '''' <seealsocref="CosmosClientBuilder.WithConnectionModeGateway(,IWebProxy)"/>
'        Public Property GatewayModeMaxConnectionLimit As Integer
'            Get
'                Return gatewayModeMaxConnectionLimitField
'            End Get
'            Set(ByVal value As Integer)

'                If value <= 0 Then
'                    Throw New ArgumentOutOfRangeException(NameOf(value))
'                End If

'                If HttpClientFactory IsNot Nothing AndAlso value <> ConnectionPolicy.[Default].MaxConnectionLimit Then
'                    Throw New ArgumentException($"{NameOf(httpClientFactoryField)} can not be set along with {NameOf(Me.GatewayModeMaxConnectionLimit)}. This must be set on the HttpClientHandler.MaxConnectionsPerServer property.")
'                End If

'                gatewayModeMaxConnectionLimitField = value
'            End Set
'        End Property

'        '''' <summary>
'        '''' Gets the request timeout in seconds when connecting to the Azure Cosmos DB service.
'        '''' The number specifies the time to wait for response to come back from network peer.
'        '''' </summary>
'        '''' <value>Default value is 1 minute.</value>
'        '''' <seealsocref="CosmosClientBuilder.WithRequestTimeout(TimeSpan)"/>
'        'Public Property RequestTimeout As TimeSpan

'        '''' <summary>
'        '''' The SDK does a background refresh based on the time interval set to refresh the token credentials.
'        '''' This avoids latency issues because the old token is used until the new token is retrieved.
'        '''' </summary>
'        '''' <remarks>
'        '''' The recommended minimum value is 5 minutes. The default value is 50% of the token expire time.
'        '''' </remarks>
'        'Public Property TokenCredentialBackgroundRefreshInterval As TimeSpan?

'        '''' <summary>
'        '''' Gets the handlers run before the process
'        '''' </summary>
'        '''' <seealsocref="CosmosClientBuilder.AddCustomHandlers()"/>
'        '<JsonConverter(GetType(ClientOptionJsonConverter))>
'        'Public ReadOnly Property CustomHandlers As Collection(Of RequestHandler)

'        '''' <summary>
'        '''' Get or set the connection mode used by the client when connecting to the Azure Cosmos DB service.
'        '''' </summary>
'        '''' <value>
'        '''' Default value is <seecref="Cosmos.ConnectionMode.Direct"/>
'        '''' </value>
'        '''' <remarks>
'        '''' For more information, see <seehref="https://docs.microsoft.com/azure/documentdb/documentdb-performance-tips#direct-connection">Connection policy: Use direct connection mode</see>.
'        '''' </remarks>
'        '''' <seealsocref="CosmosClientBuilder.WithConnectionModeDirect()"/>
'        '''' <seealsocref="CosmosClientBuilder.WithConnectionModeGateway(,IWebProxy)"/>
'        Public Property _ConnectionMode As ConnectionMode
'            Get
'                Return connectionModeField
'            End Get
'            Set(ByVal value As ConnectionMode)

'                If value = ConnectionMode.Gateway Then
'                    _ConnectionProtocol = Protocol.Https
'                ElseIf value = ConnectionMode.Direct Then
'                    connectionProtocolField = Protocol.Tcp
'                End If

'                ValidateDirectTCPSettings()
'                connectionModeField = value
'            End Set
'        End Property

'        '''' <summary>
'        '''' This can be used to weaken the database account consistency level for read operations.
'        '''' If this is not set the database account consistency level will be used for all requests.
'        '''' </summary>
'        'Public Property ConsistencyLevel As ConsistencyLevel?

'        '''' <summary>
'        '''' Gets or sets the maximum number of retries in the case where the request fails
'        '''' because the Azure Cosmos DB service has applied rate limiting on the client.
'        '''' </summary>
'        '''' <value>
'        '''' The default value is 9. This means in the case where the request is rate limited,
'        '''' the same request will be issued for a maximum of 10 times to the server before
'        '''' an error is returned to the application.
'        ''''
'        '''' If the value of this property is set to 0, there will be no automatic retry on rate
'        '''' limiting requests from the client and the exception needs to be handled at the
'        '''' application level.
'        '''' </value>
'        '''' <remarks>
'        '''' <para>
'        '''' When a client is sending requests faster than the allowed rate,
'        '''' the service will return HttpStatusCode 429 (Too Many Requests) to rate limit the client. The current
'        '''' implementation in the SDK will then wait for the amount of time the service tells it to wait and
'        '''' retry after the time has elapsed.
'        '''' </para>
'        '''' <para>
'        '''' For more information, see <seehref="https://docs.microsoft.com/azure/cosmos-db/performance-tips#throughput">Handle rate limiting/request rate too large</see>.
'        '''' </para>
'        '''' </remarks>
'        '''' <seealsocref="CosmosClientBuilder.WithThrottlingRetryOptions(TimeSpan,Integer)"/>
'        'Public Property MaxRetryAttemptsOnRateLimitedRequests As Integer?

'        '''' <summary>
'        '''' Gets or sets the maximum retry time in seconds for the Azure Cosmos DB service.
'        '''' </summary>
'        '''' <value>
'        '''' The default value is 30 seconds. 
'        '''' </value>
'        '''' <remarks>
'        '''' <para>
'        '''' The minimum interval is seconds. Any interval that is smaller will be ignored.
'        '''' </para>
'        '''' <para>
'        '''' When a request fails due to a rate limiting error, the service sends back a response that
'        '''' contains a value indicating the client should not retry before the <seecref="Microsoft.Azure.Cosmos.CosmosException.RetryAfter"/> time period has
'        '''' elapsed.
'        ''''
'        '''' This property allows the application to set a maximum wait time for all retry attempts.
'        '''' If the cumulative wait time exceeds the this value, the client will stop retrying and return the error to the application.
'        '''' </para>
'        '''' <para>
'        '''' For more information, see <seehref="https://docs.microsoft.com/azure/cosmos-db/performance-tips#throughput">Handle rate limiting/request rate too large</see>.
'        '''' </para>
'        '''' </remarks>
'        '''' <seealsocref="CosmosClientBuilder.WithThrottlingRetryOptions(TimeSpan,Integer)"/>
'        'Public Property MaxRetryWaitTimeOnRateLimitedRequests As TimeSpan?

'        '''' <summary>
'        '''' Gets or sets the boolean to only return the headers and status code in
'        '''' the Cosmos DB response for write item operation like Create, Upsert, Patch and Replace.
'        '''' Setting the option to false will cause the response to have a null resource. This reduces networking and CPU load by not sending
'        '''' the resource back over the network and serializing it on the client.
'        '''' </summary>
'        '''' <remarks>
'        '''' <para>This is optimal for workloads where the returned resource is not used.</para>
'        '''' <para>This option can be overriden by similar property in ItemRequestOptions and TransactionalBatchItemRequestOptions</para>
'        '''' </remarks>
'        '''' <seealsocref="CosmosClientBuilder.WithContentResponseOnWrite(Boolean)"/>
'        '''' <seealsocref="ItemRequestOptions.EnableContentResponseOnWrite"/>
'        '''' <seealsocref="TransactionalBatchItemRequestOptions.EnableContentResponseOnWrite"/>
'        'Public Property EnableContentResponseOnWrite As Boolean?

'        '''' <summary>
'        '''' (Direct/TCP) Controls the amount of idle time after which unused connections are closed.
'        '''' </summary>
'        '''' <value>
'        '''' By default, idle connections are kept open indefinitely. Value must be greater than or equal to 10 minutes. Recommended values are between 20 minutes and 24 hours.
'        '''' </value>
'        '''' <remarks>
'        '''' Mainly useful for sparse infrequent access to a large database account.
'        '''' </remarks>
'        'Public Property IdleTcpConnectionTimeout As TimeSpan?
'        '    Get
'        '        Return idleTcpConnectionTimeoutField
'        '    End Get
'        '    Set(ByVal value As TimeSpan?)
'        '        idleTcpConnectionTimeoutField = value
'        '        ValidateDirectTCPSettings()
'        '    End Set
'        'End Property

'        '''' <summary>
'        '''' (Direct/TCP) Controls the amount of time allowed for trying to establish a connection.
'        '''' </summary>
'        '''' <value>
'        '''' The default timeout is 5 seconds. Recommended values are greater than or equal to 5 seconds.
'        '''' </value>
'        '''' <remarks>
'        '''' When the time elapses, the attempt is cancelled and an error is returned. Longer timeouts will delay retries and failures.
'        '''' </remarks>
'        'Public Property OpenTcpConnectionTimeout As TimeSpan?
'        '    Get
'        '        Return openTcpConnectionTimeoutField
'        '    End Get
'        '    Set(ByVal value As TimeSpan?)
'        '        openTcpConnectionTimeoutField = value
'        '        ValidateDirectTCPSettings()
'        '    End Set
'        'End Property

'        '''' <summary>
'        '''' (Direct/TCP) Controls the number of requests allowed simultaneously over a single TCP connection. When more requests are in flight simultaneously, the direct/TCP client will open additional connections.
'        '''' </summary>
'        '''' <value>
'        '''' The default settings allow 30 simultaneous requests per connection.
'        '''' Do not set this value lower than 4 requests per connection or higher than 50-100 requests per connection.       
'        '''' The former can lead to a large number of connections to be created. 
'        '''' The latter can lead to head of line blocking, high latency and timeouts.
'        '''' </value>
'        '''' <remarks>
'        '''' Applications with a very high degree of parallelism per connection, with large requests or responses, or with very tight latency requirements might get better performance with 8-16 requests per connection.
'        '''' </remarks>
'        'Public Property MaxRequestsPerTcpConnection As Integer?
'        '    Get
'        '        Return maxRequestsPerTcpConnectionField
'        '    End Get
'        '    Set(ByVal value As Integer?)
'        '        maxRequestsPerTcpConnectionField = value
'        '        ValidateDirectTCPSettings()
'        '    End Set
'        'End Property

'        '''' <summary>
'        '''' (Direct/TCP) Controls the maximum number of TCP connections that may be opened to each Cosmos DB back-end.
'        '''' Together with MaxRequestsPerTcpConnection, this setting limits the number of requests that are simultaneously sent to a single Cosmos DB back-end(MaxRequestsPerTcpConnection x MaxTcpConnectionPerEndpoint).
'        '''' </summary>
'        '''' <value>
'        '''' The default value is 65,535. Value must be greater than or equal to 16.
'        '''' </value>
'        'Public Property MaxTcpConnectionsPerEndpoint As Integer?
'        '    Get
'        '        Return maxTcpConnectionsPerEndpointField
'        '    End Get
'        '    Set(ByVal value As Integer?)
'        '        maxTcpConnectionsPerEndpointField = value
'        '        ValidateDirectTCPSettings()
'        '    End Set
'        'End Property

'        '''' <summary>
'        '''' (Direct/TCP) Controls the client port reuse policy used by the transport stack.
'        '''' </summary>
'        '''' <value>
'        '''' The default value is PortReuseMode.ReuseUnicastPort.
'        '''' </value>
'        '''' <remarks>
'        '''' ReuseUnicastPort and PrivatePortPool are not mutually exclusive.
'        '''' When PrivatePortPool is enabled, the client first tries to reuse a port it already has.
'        '''' It falls back to allocating a new port if the initial attempts failed. If this fails, too, the client then falls back to ReuseUnicastPort.
'        '''' </remarks>
'        'Public Property PortReuseMode As PortReuseMode?
'        '    Get
'        '        Return portReuseModeField
'        '    End Get
'        '    Set(ByVal value As PortReuseMode?)
'        '        portReuseModeField = value
'        '        ValidateDirectTCPSettings()
'        '    End Set
'        'End Property

'        '''' <summary>
'        '''' (Gateway/Https) Get or set the proxy information used for web requests.
'        '''' </summary>
'        <JsonIgnore>
'        Public Property WebProxy As IWebProxy
'            Get
'                Return webProxyField
'            End Get
'            Set(ByVal value As IWebProxy)

'                If value IsNot Nothing AndAlso HttpClientFactory IsNot Nothing Then
'                    Throw New ArgumentException($"{NameOf(Me.WebProxy)} cannot be set along {NameOf(Me.HttpClientFactory)}")
'                End If

'                webProxyField = value
'            End Set
'        End Property

'        '''' <summary>
'        '''' Get to set optional serializer options.
'        '''' </summary>
'        '''' <example>
'        '''' An example on how to configure the serialization option to ignore null values
'        '''' <codelanguage="c#">
'        '''' <![CDATA[
'        '''' CosmosClientOptions clientOptions = new CosmosClientOptions()
'        '''' {
'        ''''     SerializerOptions = new CosmosSerializationOptions(){
'        ''''         IgnoreNullValues = true
'        ''''     }
'        '''' };
'        '''' 
'        '''' CosmosClient client = new CosmosClient("endpoint", "key", clientOptions);
'        '''' ]]>
'        '''' </code>
'        '''' </example>
'        'Public Property SerializerOptions As CosmosSerializationOptions
'        '    Get
'        '        Return serializerOptionsField
'        '    End Get
'        '    Set(ByVal value As CosmosSerializationOptions)

'        '        If Serializer IsNot Nothing Then
'        '            Throw New ArgumentException($"{NameOf(Me.SerializerOptions)} is not compatible with {NameOf(Me.Serializer)}. Only one can be set.  ")
'        '        End If

'        '        serializerOptionsField = value
'        '    End Set
'        'End Property

'        '''' <summary>
'        '''' Get to set an optional JSON serializer. The client will use it to serialize or de-serialize user's cosmos request/responses.
'        '''' SDK owned types such as DatabaseProperties and ContainerProperties will always use the SDK default serializer.
'        '''' </summary>
'        '''' <example>
'        '''' An example on how to set a custom serializer. For basic serializer options look at CosmosSerializationOptions
'        '''' <codelanguage="c#">
'        '''' <![CDATA[
'        '''' CosmosSerializer ignoreNullSerializer = new MyCustomIgnoreNullSerializer();
'        ''''         
'        '''' CosmosClientOptions clientOptions = new CosmosClientOptions()
'        '''' {
'        ''''     Serializer = ignoreNullSerializer
'        '''' };
'        '''' 
'        '''' CosmosClient client = new CosmosClient("endpoint", "key", clientOptions);
'        '''' ]]>
'        '''' </code>
'        '''' </example>
'        '<JsonConverter(GetType(ClientOptionJsonConverter))>
'        'Public Property Serializer As CosmosSerializer
'        '    Get
'        '        Return serializerInternal
'        '    End Get
'        '    Set(ByVal value As CosmosSerializer)

'        '        If SerializerOptions IsNot Nothing Then
'        '            Throw New ArgumentException($"{NameOf(Me.Serializer)} is not compatible with {NameOf(Me.SerializerOptions)}. Only one can be set.  ")
'        '        End If

'        '        serializerInternal = value
'        '    End Set
'        'End Property

'        '''' <summary>
'        '''' Limits the operations to the provided endpoint on the CosmosClient.
'        '''' </summary>
'        '''' <value>
'        '''' Default value is false.
'        '''' </value>
'        '''' <remarks>
'        '''' When the value of this property is false, the SDK will automatically discover write and read regions, and use them when the configured application region is not available.
'        '''' When set to true, availability is limited to the endpoint specified on the CosmosClient constructor.
'        '''' Defining the <seecref="ApplicationRegion"/> or <seecref="ApplicationPreferredRegions"/>  is not allowed when setting the value to true.
'        '''' </remarks>
'        '''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/high-availability">High availability</seealso>
'        'Public Property LimitToEndpoint As Boolean = False

'        '''' <summary>
'        '''' Allows optimistic batching of requests to service. Setting this option might impact the latency of the operations. Hence this option is recommended for non-latency sensitive scenarios only.
'        '''' </summary>
'        'Public Property AllowBulkExecution As Boolean

'        '''' <summary>
'        '''' Gets or sets the flag to enable address cache refresh on TCP connection reset notification.
'        '''' </summary>
'        '''' <remarks>
'        '''' Does not apply if <seecref="ConnectionMode.Gateway"/> is used.
'        '''' </remarks>
'        '''' <value>
'        '''' The default value is true
'        '''' </value>
'        'Public Property EnableTcpConnectionEndpointRediscovery As Boolean = True

'        '''' <summary>
'        '''' Gets or sets a delegate to use to obtain an HttpClient instance to be used for HTTPS communication.
'        '''' </summary>
'        '''' <remarks>
'        '''' <para>
'        '''' HTTPS communication is used when <seecref="ConnectionMode"/> is set to <seecref="ConnectionMode.Gateway"/> for all operations and when <seecref="ConnectionMode"/> is <seecref="ConnectionMode.Direct"/> (default) for metadata operations.
'        '''' </para>
'        '''' <para>
'        '''' Useful in scenarios where the application is using a pool of HttpClient instances to be shared, like ASP.NET Core applications with IHttpClientFactory or Blazor WebAssembly applications.
'        '''' </para>
'        '''' <para>
'        '''' For .NET core applications the default GatewayConnectionLimit will be ignored. It must be set on the HttpClientHandler.MaxConnectionsPerServer to limit the number of connections
'        '''' </para>
'        '''' </remarks>
'        <JsonIgnore>
'        Public Property HttpClientFactory As Func(Of HttpClient)
'            Get
'                Return httpClientFactoryField
'            End Get
'            Set(ByVal value As Func(Of HttpClient))

'                If value IsNot Nothing AndAlso WebProxy IsNot Nothing Then
'                    Throw New ArgumentException($"{NameOf(Me.HttpClientFactory)} cannot be set along {NameOf(Me.WebProxy)}")
'                End If

'                If GatewayModeMaxConnectionLimit <> ConnectionPolicy.[Default].MaxConnectionLimit Then
'                    Throw New ArgumentException($"{NameOf(httpClientFactoryField)} can not be set along with {NameOf(Me.GatewayModeMaxConnectionLimit)}. This must be set on the HttpClientHandler.MaxConnectionsPerServer property.")
'                End If

'                httpClientFactoryField = value
'            End Set
'        End Property

'        '''' <summary>
'        '''' Enable partition key level failover
'        '''' </summary>
'        'Friend Property EnablePartitionLevelFailover As Boolean = False

'        '''' <summary>
'        '''' Gets or sets the connection protocol when connecting to the Azure Cosmos service.
'        '''' </summary>
'        '''' <value>
'        '''' Default value is <seecref="Protocol.Tcp"/>.
'        '''' </value>
'        '''' <remarks>
'        '''' This setting is not used when <seecref="ConnectionMode"/> is set to <seecref="Cosmos.ConnectionMode.Gateway"/>.
'        '''' Gateway mode only supports HTTPS.
'        '''' For more information, see <seehref="https://docs.microsoft.com/azure/documentdb/documentdb-performance-tips#use-tcp">Connection policy: Use the TCP protocol</see>.
'        '''' </remarks>
'        Friend Property ConnectionProtocol As Protocol
'            Get
'                Return connectionProtocolField
'            End Get
'            Set(ByVal value As Protocol)
'                ValidateDirectTCPSettings()
'                connectionProtocolField = value
'            End Set
'        End Property

'        '''' <summary>
'        '''' The event handler to be invoked before the request is sent.
'        '''' </summary>
'        Friend Property SendingRequestEventArgs As EventHandler(Of SendingRequestEventArgs)

'        '''' <summary>
'        '''' (Optional) transport interceptor factory
'        '''' </summary>
'        Friend Property TransportClientHandlerFactory As Func(Of TransportClient, TransportClient)

'        '''' <summary>
'        '''' API type for the account
'        '''' </summary>
'        Friend Property _ApiType As ApiType

'        '''' <summary>
'        '''' Optional store client factory instance to use for all transport requests.
'        '''' </summary>
'        Friend Property StoreClientFactory As IStoreClientFactory

'        '''' <summary>
'        '''' Gets or sets the initial delay retry time in milliseconds for the Azure Cosmos DB service
'        '''' for requests that hit RetryWithExceptions. This covers errors that occur due to concurrency errors in the store.
'        '''' </summary>
'        '''' <value>
'        '''' The default value is 1 second. For an example on how to set this value, please refer to <seecref="ConnectionPolicy.RetryOptions"/>.
'        '''' </value>
'        '''' <remarks>
'        '''' <para>
'        '''' When a request fails due to a RetryWith error, the client delays and retries the request. This configures the client
'        '''' to delay the time specified before retrying the request.
'        '''' </para>
'        '''' </remarks>
'        'Friend Property InitialRetryForRetryWithMilliseconds As Integer?

'        '''' <summary>
'        '''' Gets or sets the maximum delay retry time in milliseconds for the Azure Cosmos DB service
'        '''' for requests that hit RetryWithExceptions. This covers errors that occur due to concurrency errors in the store.
'        '''' </summary>
'        '''' <value>
'        '''' The default value is 30 seconds. For an example on how to set this value, please refer to <seecref="ConnectionPolicy.RetryOptions"/>.
'        '''' </value>
'        '''' <remarks>
'        '''' <para>
'        '''' When a request fails due to a RetryWith error, the client delays and retries the request. This configures the maximum time
'        '''' the client should delay before failing the request.
'        '''' </para>
'        '''' </remarks>
'        'Friend Property MaximumRetryForRetryWithMilliseconds As Integer?

'        '''' <summary>
'        '''' Gets or sets the interval to salt retry with value. This will spread the retry values from 1..n from the exponential back-off
'        '''' subscribed.
'        '''' </summary>
'        '''' <value>
'        '''' The default value is to not salt.
'        '''' </value>
'        '''' <remarks>
'        '''' <para>
'        '''' When a request fails due to a RetryWith error, the client delays and retries the request. This configures the jitter on the retry attempted.
'        '''' </para>
'        '''' </remarks>
'        'Friend Property RandomSaltForRetryWithMilliseconds As Integer?

'        '''' <summary>
'        '''' Gets or sets the total time to wait before failing the request for retry with failures.
'        '''' subscribed.
'        '''' </summary>
'        '''' <value>
'        '''' The default value 30 seconds.
'        '''' </value>
'        '''' <remarks>
'        '''' <para>
'        '''' When a request fails due to a RetryWith error, the client delays and retries the request. This configures total time spent waiting on the request.
'        '''' </para>
'        '''' </remarks>
'        'Friend Property TotalWaitTimeForRetryWithMilliseconds As Integer?

'        '''' <summary>
'        '''' Flag that controls whether CPU monitoring thread is created to enrich timeout exceptions with additional diagnostic. Default value is true.
'        '''' </summary>
'        Friend Property EnableCpuMonitor As Boolean?

'        '''' <summary>
'        '''' Flag to enable telemetry
'        '''' </summary>
'        'Friend Property EnableClientTelemetry As Boolean?

'        'Friend Sub SetSerializerIfNotConfigured(ByVal serializer As CosmosSerializer)
'        '    If serializerInternal Is Nothing Then
'        '        serializerInternal = If(serializer, CSharpImpl.__Throw(Of Object)(New ArgumentNullException(NameOf(serializer))))
'        '    End If
'        'End Sub

'        Friend Function Clone() As CosmosClientOptions
'            Dim cloneConfiguration As CosmosClientOptions = CType(MemberwiseClone(), CosmosClientOptions)
'            Return cloneConfiguration
'        End Function

'        Friend Overridable Function GetConnectionPolicy(ByVal clientId As Integer) As ConnectionPolicy
'            ValidateDirectTCPSettings()
'            ValidateLimitToEndpointSettings()
'            Dim connectionPolicy As ConnectionPolicy = New ConnectionPolicy() With {
'                .MaxConnectionLimit = GatewayModeMaxConnectionLimit,
'                .RequestTimeout = RequestTimeout,
'                ._ConnectionMode = ConnectionMode,
'                .ConnectionProtocol = ConnectionProtocol,
'                .UserAgentContainer = CreateUserAgentContainerWithFeatures(clientId),
'                .UseMultipleWriteLocations = True,
'                .IdleTcpConnectionTimeout = IdleTcpConnectionTimeout,
'                .OpenTcpConnectionTimeout = OpenTcpConnectionTimeout,
'                .MaxRequestsPerTcpConnection = MaxRequestsPerTcpConnection,
'                .MaxTcpConnectionsPerEndpoint = MaxTcpConnectionsPerEndpoint,
'                .EnableEndpointDiscovery = Not LimitToEndpoint,
'                .EnablePartitionLevelFailover = EnablePartitionLevelFailover,
'                .PortReuseMode = portReuseModeField,
'                .EnableTcpConnectionEndpointRediscovery = EnableTcpConnectionEndpointRediscovery,
'                .HttpClientFactory = httpClientFactoryField
'            }

'            If EnableClientTelemetry.HasValue Then
'                connectionPolicy.EnableClientTelemetry = EnableClientTelemetry.Value
'            End If

'            If Not Equals(ApplicationRegion, Nothing) Then
'                connectionPolicy.SetCurrentLocation(ApplicationRegion)
'            End If

'            If ApplicationPreferredRegions IsNot Nothing Then
'                connectionPolicy.SetPreferredLocations(ApplicationPreferredRegions)
'            End If

'            If MaxRetryAttemptsOnRateLimitedRequests IsNot Nothing Then
'                connectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = MaxRetryAttemptsOnRateLimitedRequests.Value
'            End If

'            If MaxRetryWaitTimeOnRateLimitedRequests IsNot Nothing Then
'                connectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = MaxRetryWaitTimeOnRateLimitedRequests.Value.TotalSeconds
'            End If

'            If InitialRetryForRetryWithMilliseconds IsNot Nothing Then
'                connectionPolicy.RetryOptions.InitialRetryForRetryWithMilliseconds = InitialRetryForRetryWithMilliseconds
'            End If

'            If MaximumRetryForRetryWithMilliseconds IsNot Nothing Then
'                connectionPolicy.RetryOptions.MaximumRetryForRetryWithMilliseconds = MaximumRetryForRetryWithMilliseconds
'            End If

'            If RandomSaltForRetryWithMilliseconds IsNot Nothing Then
'                connectionPolicy.RetryOptions.RandomSaltForRetryWithMilliseconds = RandomSaltForRetryWithMilliseconds
'            End If

'            If TotalWaitTimeForRetryWithMilliseconds IsNot Nothing Then
'                connectionPolicy.RetryOptions.TotalWaitTimeForRetryWithMilliseconds = TotalWaitTimeForRetryWithMilliseconds
'            End If

'            Return connectionPolicy
'        End Function

'        Friend Function GetDocumentsConsistencyLevel() As Documents.ConsistencyLevel?
'            If Not ConsistencyLevel.HasValue Then
'                Return Nothing
'            End If

'            Return CType(ConsistencyLevel.Value, Documents.ConsistencyLevel)
'        End Function

'        'Friend Shared Function GetAccountEndpoint(ByVal connectionString As String) As String
'        '    Return GetValueFromConnectionString(connectionString, ConnectionStringAccountEndpoint)
'        'End Function

'        'Friend Shared Function GetAccountKey(ByVal connectionString As String) As String
'        '    Return GetValueFromConnectionString(connectionString, ConnectionStringAccountKey)
'        'End Function

'        'Private Shared Function GetValueFromConnectionString(ByVal connectionString As String, ByVal keyName As String) As String
'        '    If Equals(connectionString, Nothing) Then
'        '        Throw New ArgumentNullException(NameOf(connectionString))
'        '    End If

'        '    Dim builder As DbConnectionStringBuilder = New DbConnectionStringBuilder With {
'        '        .ConnectionString = connectionString
'        '    }
'        '    Dim value As Object = Nothing

'        '    If builder.TryGetValue(keyName, value) Then
'        '        Dim keyNameValue As String = TryCast(value, String)

'        '        If Not String.IsNullOrEmpty(keyNameValue) Then
'        '            Return keyNameValue
'        '        End If
'        '    End If

'        '    Throw New ArgumentException("The connection string is missing a required property: " & keyName)
'        'End Function

'        'Private Sub ValidateLimitToEndpointSettings()
'        '    If Not String.IsNullOrEmpty(ApplicationRegion) AndAlso LimitToEndpoint Then
'        '        Throw New ArgumentException($"Cannot specify {NameOf(Me.ApplicationRegion)} and enable {NameOf(Me.LimitToEndpoint)}. Only one can be set.")
'        '    End If

'        '    If ApplicationPreferredRegions?.Count > 0 AndAlso LimitToEndpoint Then
'        '        Throw New ArgumentException($"Cannot specify {NameOf(Me.ApplicationPreferredRegions)} and enable {NameOf(Me.LimitToEndpoint)}. Only one can be set.")
'        '    End If

'        '    If Not String.IsNullOrEmpty(ApplicationRegion) AndAlso ApplicationPreferredRegions?.Count > 0 Then
'        '        Throw New ArgumentException($"Cannot specify {NameOf(Me.ApplicationPreferredRegions)} and {NameOf(Me.ApplicationRegion)}. Only one can be set.")
'        '    End If
'        'End Sub

'        Private Sub ValidateDirectTCPSettings()
'            Dim settingName = String.Empty

'            If _ConnectionMode <> ConnectionMode.Direct Then
'                If IdleTcpConnectionTimeout.HasValue Then
'                    settingName = NameOf(Me.IdleTcpConnectionTimeout)
'                ElseIf OpenTcpConnectionTimeout.HasValue Then
'                    settingName = NameOf(Me.OpenTcpConnectionTimeout)
'                ElseIf MaxRequestsPerTcpConnection.HasValue Then
'                    settingName = NameOf(Me.MaxRequestsPerTcpConnection)
'                ElseIf MaxTcpConnectionsPerEndpoint.HasValue Then
'                    settingName = NameOf(Me.MaxTcpConnectionsPerEndpoint)
'                ElseIf PortReuseMode.HasValue Then
'                    settingName = NameOf(Me.PortReuseMode)
'                End If
'            End If

'            If Not String.IsNullOrEmpty(settingName) Then
'                Throw New ArgumentException($"{settingName} requires {NameOf(_ConnectionMode)} to be set to {NameOf(ConnectionMode.Direct)}")
'            End If
'        End Sub

'        'Friend Function CreateUserAgentContainerWithFeatures(ByVal clientId As Integer) As UserAgentContainer
'        '    Dim features As CosmosClientOptionsFeatures = CosmosClientOptionsFeatures.NoFeatures

'        '    If AllowBulkExecution Then
'        '        features = features Or CosmosClientOptionsFeatures.AllowBulkExecution
'        '    End If

'        '    If HttpClientFactory IsNot Nothing Then
'        '        features = features Or CosmosClientOptionsFeatures.HttpClientFactory
'        '    End If

'        '    Dim featureString As String = Nothing

'        '    If features IsNot CosmosClientOptionsFeatures.NoFeatures Then
'        '        featureString = Convert.ToString(CInt(CInt(features)), CInt(2)).PadLeft(8, "0"c)
'        '    End If

'        '    Dim regionConfiguration As String = GetRegionConfiguration()
'        '    Return New UserAgentContainer(clientId:=clientId, features:=featureString, regionConfiguration:=regionConfiguration, suffix:=ApplicationName)
'        'End Function

'        '''' <summary>
'        '''' This generates a key that added to the user agent to make it 
'        '''' possible to determine if the SDK has region failover enabled.
'        '''' </summary>
'        '''' <returns>Format Reg-{D (Disabled discovery)}-S(application region)|L(List of preferred regions)|N(None, user did not configure it)</returns>
'        'Private Function GetRegionConfiguration() As String
'        '    Dim regionConfig = If(LimitToEndpoint, "D", String.Empty)

'        '    If Not String.IsNullOrEmpty(ApplicationRegion) Then
'        '        Return regionConfig & "S"
'        '    End If

'        '    If ApplicationPreferredRegions IsNot Nothing Then
'        '        Return regionConfig & "L"
'        '    End If

'        '    Return regionConfig & "N"
'        'End Function

'        '''' <summary>
'        '''' Serialize the current configuration into a JSON string
'        '''' </summary>
'        '''' <returns>Returns a JSON string of the current configuration.</returns>
'        'Friend Function GetSerializedConfiguration() As String
'        '    Return JsonConvert.SerializeObject(Me)
'        'End Function

'        '''' <summary>
'        '''' The complex object passed in by the user can contain objects that can not be serialized. Instead just log the types.
'        '''' </summary>
'        'Private Class ClientOptionJsonConverter
'        '    Inherits JsonConverter

'        '    Public Overrides Sub WriteJson(ByVal writer As JsonWriter, ByVal value As Object, ByVal serializer As JsonSerializer)
'        '        Dim handlers As Collection(Of RequestHandler) = Nothing

'        '        If CSharpImpl.__Assign(handlers, TryCast(value, Collection(Of RequestHandler))) IsNot Nothing Then
'        '            writer.WriteValue(String.Join(":", handlers.[Select](Function(x) x.[GetType]())))
'        '            Return
'        '        End If

'        '        Dim cosmosJsonSerializerWrapper As CosmosJsonSerializerWrapper = TryCast(value, CosmosJsonSerializerWrapper)

'        '        If TypeOf value Is CosmosJsonSerializerWrapper Then
'        '            writer.WriteValue(cosmosJsonSerializerWrapper.InternalJsonSerializer.[GetType]().ToString())
'        '        End If

'        '        Dim cosmosSerializer As CosmosSerializer = TryCast(value, CosmosSerializer)

'        '        If TypeOf cosmosSerializer Is CosmosSerializer Then
'        '            writer.WriteValue(cosmosSerializer.[GetType]().ToString())
'        '        End If
'        '    End Sub

'        '    Public Overrides Function ReadJson(ByVal reader As JsonReader, ByVal objectType As Type, ByVal existingValue As Object, ByVal serializer As JsonSerializer) As Object
'        '        Throw New NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.")
'        '    End Function

'        '    Public Overrides ReadOnly Property CanRead As Boolean
'        '        Get
'        '            Return False
'        '        End Get
'        '    End Property

'        '    Public Overrides Function CanConvert(ByVal objectType As Type) As Boolean
'        '        Return objectType Is GetType(Date)
'        '    End Function

'        '    Private Class CSharpImpl
'        '        <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
'        '        Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
'        '            target = value
'        '            Return value
'        '        End Function
'        '    End Class
'        'End Class

'        'Private Class CSharpImpl
'        '    <Obsolete("Please refactor calling code to use normal throw statements")>
'        '    Shared Function __Throw(Of T)(ByVal e As Exception) As T
'        '        Throw e
'        '    End Function
'        'End Class
'    End Class
'End Namespace
