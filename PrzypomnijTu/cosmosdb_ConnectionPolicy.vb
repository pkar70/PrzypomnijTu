'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------
'Imports System
'Imports System.Collections.Generic
'Imports System.Collections.ObjectModel
'Imports System.Collections.Specialized
'Imports System.Net.Http
'Imports Microsoft.Azure.Cosmos.Telemetry
'Imports Microsoft.Azure.Documents
'Imports Microsoft.Azure.Documents.Client

Namespace Microsoft.Azure.Cosmos

    ''' <summary>
    ''' Represents the connection policy associated with a DocumentClient to connect to the Azure Cosmos DB service.
    ''' </summary>
    Friend NotInheritable Class ConnectionPolicy
        'Private Const defaultRequestTimeout As Integer = 10
        '' defaultMediaRequestTimeout is based upon the blob client timeout and the retry policy.
        'Private Const defaultMediaRequestTimeout As Integer = 300
        'Private Const defaultMaxConcurrentFanoutRequests As Integer = 32
        Private Const defaultMaxConcurrentConnectionLimit As Integer = 50
        'Friend UserAgentContainer As UserAgentContainer
        Private Shared defaultPolicy As ConnectionPolicy
        'Private connectionProtocolField As Protocol
        'Private preferredLocationsField As ObservableCollection(Of String)

        '''' <summary>
        '''' Initializes a new instance of the <seecref="ConnectionPolicy"/> class to connect to the Azure Cosmos DB service.
        '''' </summary>
        Public Sub New()
            'connectionProtocolField = Protocol.Https
            'RequestTimeout = TimeSpan.FromSeconds(defaultRequestTimeout)
            'MediaRequestTimeout = TimeSpan.FromSeconds(defaultMediaRequestTimeout)
            'ConnectionMode = ConnectionMode.Gateway
            'MaxConcurrentFanoutRequests = defaultMaxConcurrentFanoutRequests
            'MediaReadMode = MediaReadMode.Buffered
            'UserAgentContainer = New UserAgentContainer(clientId:=0)
            'preferredLocationsField = New ObservableCollection(Of String)()
            'EnableEndpointDiscovery = True
            MaxConnectionLimit = defaultMaxConcurrentConnectionLimit
            'RetryOptions = New RetryOptions()
            'EnableReadRequestsFallback = Nothing
            'EnableClientTelemetry = ClientTelemetryOptions.IsClientTelemetryEnabled()
        End Sub

        '''' <summary>
        '''' Automatically populates the <seecref="PreferredLocations"/> for geo-replicated database accounts in the Azure Cosmos DB service,
        '''' based on the current region that the client is running in.
        '''' </summary>
        '''' <paramname="location">The current region that this client is running in. E.g. "East US" </param>
        'Public Sub SetCurrentLocation(ByVal location As String)
        '    If Not RegionProximityUtil.SourceRegionToTargetRegionsRTTInMs.ContainsKey(location) Then
        '        Throw New ArgumentException($"ApplicationRegion configuration '{location}' is not a valid Azure region or the current SDK version does not recognize it. If the value represents a valid region, make sure you are using the latest SDK version.")
        '    End If

        '    Dim proximityBasedPreferredLocations As List(Of String) = RegionProximityUtil.GeneratePreferredRegionList(location)

        '    If proximityBasedPreferredLocations IsNot Nothing Then
        '        preferredLocationsField.Clear()

        '        For Each preferredLocation In proximityBasedPreferredLocations
        '            preferredLocationsField.Add(preferredLocation)
        '        Next
        '    End If
        'End Sub

        'Public Sub SetPreferredLocations(ByVal regions As IReadOnlyList(Of String))
        '    If regions Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(regions))
        '    End If

        '    preferredLocationsField.Clear()

        '    For Each preferredLocation In regions
        '        preferredLocationsField.Add(preferredLocation)
        '    Next
        'End Sub

        '''' <summary>
        '''' Gets or sets the maximum number of concurrent fanout requests sent to the Azure Cosmos DB service.
        '''' </summary>
        '''' <value>Default value is 32.</value>
        'Friend Property MaxConcurrentFanoutRequests As Integer

        '''' <summary>
        '''' Gets or sets the request timeout in seconds when connecting to the Azure Cosmos DB service.
        '''' The number specifies the time to wait for response to come back from network peer.
        '''' </summary>
        '''' <value>Default value is 10 seconds.</value>
        'Public Property RequestTimeout As TimeSpan

        '''' <summary>
        '''' Gets or sets the media request timeout in seconds when connecting to the Azure Cosmos DB service.
        '''' The number specifies the time to wait for response to come back from network peer for attachment content (a.k.a. media) operations.
        '''' </summary>
        '''' <value>
        '''' Default value is 300 seconds.
        '''' </value>
        'Public Property MediaRequestTimeout As TimeSpan

        '''' <summary>
        '''' Gets or sets the connection mode used by the client when connecting to the Azure Cosmos DB service.
        '''' </summary>
        '''' <value>
        '''' Default value is <seecref="Cosmos.ConnectionMode.Gateway"/>
        '''' </value>
        '''' <remarks>
        '''' For more information, see <seehref="https://docs.microsoft.com/en-us/azure/documentdb/documentdb-performance-tips#direct-connection">Connection policy: Use direct connection mode</see>.
        '''' </remarks>
        'Public Property ConnectionMode As ConnectionMode

        '''' <summary>
        '''' Gets or sets the attachment content (a.k.a. media) download mode when connecting to the Azure Cosmos DB service.
        '''' </summary>
        '''' <value>
        '''' Default value is <seecref="Cosmos.MediaReadMode.Buffered"/>.
        '''' </value>
        'Public Property MediaReadMode As MediaReadMode

        '''' <summary>
        '''' Gets or sets the connection protocol when connecting to the Azure Cosmos DB service.
        '''' </summary>
        '''' <value>
        '''' Default value is <seecref="Protocol.Https"/>.
        '''' </value>
        '''' <remarks>
        '''' This setting is not used when <seecref="ConnectionMode"/> is set to <seecref="Cosmos.ConnectionMode.Gateway"/>.
        '''' Gateway mode only supports HTTPS.
        '''' For more information, see <seehref="https://docs.microsoft.com/en-us/azure/documentdb/documentdb-performance-tips#use-tcp">Connection policy: Use the TCP protocol</see>.
        '''' </remarks>
        'Public Property ConnectionProtocol As Protocol
        '    Get
        '        Return connectionProtocolField
        '    End Get
        '    Set(ByVal value As Protocol)

        '        If value IsNot Protocol.Https AndAlso value IsNot Protocol.Tcp Then
        '            Throw New ArgumentOutOfRangeException("value")
        '        End If

        '        connectionProtocolField = value
        '    End Set
        'End Property

        '''' <summary>
        '''' Gets or sets whether to allow for reads to go to multiple regions configured on an account of Azure Cosmos DB service.
        '''' </summary>
        '''' <value>
        '''' Default value is null.
        '''' </value>
        '''' <remarks>
        '''' If this property is not set, the default is true for all Consistency Levels other than Bounded Staleness,
        '''' The default is false for Bounded Staleness.
        '''' This property only has effect if the following conditions are satisifed:
        '''' 1. <seecref="EnableEndpointDiscovery"/> is true
        '''' 2. the Azure Cosmos DB account has more than one region
        '''' </remarks>
        'Public Property EnableReadRequestsFallback As Boolean?

        '''' <summary>
        '''' Gets or sets the flag to enable address cache refresh on connection reset notification.
        '''' </summary>
        '''' <value>
        '''' The default value is false
        '''' </value>
        'Public Property EnableTcpConnectionEndpointRediscovery As Boolean
        'Friend Property EnableClientTelemetry As Boolean

        '''' <summary>
        '''' Gets the default connection policy used to connect to the Azure Cosmos DB service.
        '''' </summary>
        '''' <value>
        '''' Refer to the default values for the individual properties of <seecref="ConnectionPolicy"/> that determine the default connection policy.
        '''' </value>
        Public Shared ReadOnly Property [Default] As ConnectionPolicy
            Get

                If defaultPolicy Is Nothing Then
                    defaultPolicy = New ConnectionPolicy()
                End If

                Return defaultPolicy
            End Get
        End Property

        '''' <summary>
        '''' A suffix to be added to the default user-agent for the Azure Cosmos DB service.
        '''' </summary>
        '''' <remarks>
        '''' Setting this property after sending any request won't have any effect.
        '''' </remarks>
        'Public Property UserAgentSuffix As String
        '    Get
        '        Return UserAgentContainer.Suffix
        '    End Get
        '    Set(ByVal value As String)
        '        UserAgentContainer.Suffix = value
        '    End Set
        'End Property

        '''' <summary>
        '''' Gets and sets the preferred locations (regions) for geo-replicated database accounts in the Azure Cosmos DB service.
        '''' For example, "East US" as the preferred location.
        '''' </summary>
        '''' <remarks>
        '''' <para>
        '''' When <seecref="EnableEndpointDiscovery"/> is true and the value of this property is non-empty,
        '''' the SDK uses the locations in the collection in the order they are specified to perform operations,
        '''' otherwise if the value of this property is not specified,
        '''' the SDK uses the write region as the preferred location for all operations.
        '''' </para>
        '''' <para>
        '''' If <seecref="EnableEndpointDiscovery"/> is set to false, the value of this property is ignored.
        '''' </para>
        '''' </remarks>
        'Public ReadOnly Property PreferredLocations As Collection(Of String)
        '    Get
        '        Return preferredLocationsField
        '    End Get
        'End Property

        '''' <summary>
        '''' Gets or sets the flag to enable endpoint discovery for geo-replicated database accounts in the Azure Cosmos DB service.
        '''' </summary>
        '''' <remarks>
        '''' When the value of this property is true, the SDK will automatically discover the
        '''' current write and read regions to ensure requests are sent to the correct region
        '''' based on the regions specified in the <seecref="PreferredLocations"/> property.
        '''' <value>Default value is true indicating endpoint discovery is enabled.</value>
        '''' </remarks>
        'Public Property EnableEndpointDiscovery As Boolean
        'Public Property EnablePartitionLevelFailover As Boolean

        '''' <summary>
        '''' Gets or sets the flag to enable writes on any locations (regions) for geo-replicated database accounts in the Azure Cosmos DB service.
        '''' </summary>
        '''' <remarks>
        '''' When the value of this property is true, the SDK will direct write operations to
        '''' available writable locations of geo-replicated database account. Writable locations
        '''' are ordered by <seecref="PreferredLocations"/> property. Setting the property value
        '''' to true has no effect until <seecref="AccountProperties.EnableMultipleWriteLocations"/> 
        '''' is also set to true.
        '''' <value>Default value is false indicating that writes are only directed to
        '''' first region in <seecref="PreferredLocations"/> property.</value>
        '''' </remarks>
        'Public Property UseMultipleWriteLocations As Boolean

        '''' <summary>
        '''' Gets or sets the maximum number of concurrent connections allowed for the target
        '''' service endpoint in the Azure Cosmos DB service.
        '''' </summary>
        '''' <remarks>
        '''' This setting is only applicable in Gateway mode.
        '''' </remarks>
        '''' <value>Default value is 50.</value>
        Public Property MaxConnectionLimit As Integer

        '''' <summary>
        '''' Gets or sets the <seecref="RetryOptions"/> associated
        '''' with the <seecref="DocumentClient"/> in the Azure Cosmos DB service.
        '''' </summary>
        '''' <seealsocref="DocumentClient"/>
        '''' <seealsocref="ConnectionPolicy"/>
        '''' <seealsocref="RetryOptions"/>
        '''' <example>
        '''' The example below creates a new <seecref="DocumentClient"/> and sets the <seecref="ConnectionPolicy"/>
        '''' using the <seecref="RetryOptions"/> property.
        '''' <para>
        '''' <seecref="Cosmos.RetryOptions.MaxRetryAttemptsOnThrottledRequests"/> is set to 3, so in this case, if a request operation is rate limited by exceeding the reserved 
        '''' throughput for the collection, the request operation retries 3 times before throwing the exception to the application.
        '''' <seecref="Cosmos.RetryOptions.MaxRetryWaitTimeInSeconds"/> is set to 60, so in this case if the cumulative retry 
        '''' wait time in seconds since the first request exceeds 60 seconds, the exception is thrown.
        '''' </para>
        '''' <codelanguage="c#">
        '''' <![CDATA[
        '''' ConnectionPolicy connectionPolicy = new ConnectionPolicy();
        '''' connectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 3;
        '''' connectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 60;
        ''''
        '''' DocumentClient client = new DocumentClient(new Uri("service endpoint"), "auth key", connectionPolicy);
        '''' ]]>
        '''' </code>
        '''' </example>
        '''' <value>
        '''' If this property is not set, the SDK uses the default retry policy that has <seecref="Cosmos.RetryOptions.MaxRetryAttemptsOnThrottledRequests"/>
        '''' set to 9 and <seecref="Cosmos.RetryOptions.MaxRetryWaitTimeInSeconds"/> set to 30 seconds.
        '''' </value>
        '''' <remarks>
        '''' For more information, see <seehref="https://docs.microsoft.com/en-us/azure/documentdb/documentdb-performance-tips#429">Handle rate limiting/request rate too large</see>.
        '''' </remarks>
        'Public Property RetryOptions As RetryOptions

        '''' <summary>
        '''' (Direct/TCP) Controls the amount of idle time after which unused connections are closed.
        '''' </summary>
        '''' <value>
        '''' By default, idle connections are kept open indefinitely. Value must be greater than or equal to 10 minutes. Recommended values are between 20 minutes and 24 hours.
        '''' </value>
        '''' <remarks>
        '''' Mainly useful for sparse infrequent access to a large database account.
        '''' </remarks>
        'Public Property IdleTcpConnectionTimeout As TimeSpan?

        '''' <summary>
        '''' (Direct/TCP) Controls the amount of time allowed for trying to establish a connection.
        '''' </summary>
        '''' <value>
        '''' The default timeout is 5 seconds. Recommended values are greater than or equal to 5 seconds.
        '''' </value>
        '''' <remarks>
        '''' When the time elapses, the attempt is cancelled and an error is returned. Longer timeouts will delay retries and failures.
        '''' </remarks>
        'Public Property OpenTcpConnectionTimeout As TimeSpan?

        '''' <summary>
        '''' (Direct/TCP) Controls the number of requests allowed simultaneously over a single TCP connection. When more requests are in flight simultaneously, the direct/TCP client will open additional connections.
        '''' </summary>
        '''' <value>
        '''' The default settings allow 30 simultaneous requests per connection.
        '''' Do not set this value lower than 4 requests per connection or higher than 50-100 requests per connection. 
        '''' The former can lead to a large number of connections to be created. 
        '''' The latter can lead to head of line blocking, high latency and timeouts.
        '''' </value>
        '''' <remarks>
        '''' Applications with a very high degree of parallelism per connection, with large requests or responses, or with very tight latency requirements might get better performance with 8-16 requests per connection.
        '''' </remarks>
        'Public Property MaxRequestsPerTcpConnection As Integer?

        '''' <summary>
        '''' (Direct/TCP) Controls the maximum number of TCP connections that may be opened to each Cosmos DB back-end.
        '''' Together with MaxRequestsPerTcpConnection, this setting limits the number of requests that are simultaneously sent to a single Cosmos DB back-end(MaxRequestsPerTcpConnection x MaxTcpConnectionPerEndpoint).
        '''' </summary>
        '''' <value>
        '''' The default value is 65,535. Value must be greater than or equal to 16.
        '''' </value>
        'Public Property MaxTcpConnectionsPerEndpoint As Integer?

        '''' <summary>
        '''' (Direct/TCP) Controls the client port reuse policy used by the transport stack.
        '''' </summary>
        '''' <value>
        '''' The default value is PortReuseMode.ReuseUnicastPort.
        '''' </value>
        'Public Property PortReuseMode As PortReuseMode?

        '''' <summary>
        '''' Gets or sets a delegate to use to obtain an HttpClient instance to be used for HTTPS communication.
        '''' </summary>
        'Public Property HttpClientFactory As Func(Of HttpClient)

        '''' <summary>
        '''' (Direct/TCP) This is an advanced setting that controls the number of TCP connections that will be opened eagerly to each Cosmos DB back-end.
        '''' </summary>
        '''' <value>
        '''' Default value is 1. Applications with extreme performance requirements can set this value to 2.
        '''' </value>
        '''' <remarks>
        '''' This setting must be used with caution. When used improperly, it can lead to client machine ephemeral port exhaustion <seehref="https://docs.microsoft.com/en-us/azure/load-balancer/load-balancer-outbound-connections">Azure SNAT port exhaustion</see>.
        '''' </remarks>
        'Friend Property MaxTcpPartitionCount As Integer?

        '''' <summary>
        '''' GlobalEndpointManager will subscribe to this event if user updates the preferredLocations list in the Azure Cosmos DB service.
        '''' </summary>
        'Friend Custom Event PreferenceChanged As NotifyCollectionChangedEventHandler
        '    AddHandler(ByVal value As NotifyCollectionChangedEventHandler)
        '        AddHandler preferredLocationsField.CollectionChanged, value
        '    End AddHandler
        '    RemoveHandler(ByVal value As NotifyCollectionChangedEventHandler)
        '        RemoveHandler preferredLocationsField.CollectionChanged, value
        '    End RemoveHandler
        '    RaiseEvent(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
        '        preferredLocationsField?(sender, e)
        '    End RaiseEvent
        'End Event

        'Friend Function GetRetryWithConfiguration() As RetryWithConfiguration
        '    Return RetryOptions?.GetRetryWithConfiguration()
        'End Function
    End Class
End Namespace
