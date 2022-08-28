'' klon z https://github.com/Azure/azure-cosmos-dotnet-v3/tree/master/Microsoft.Azure.Cosmos/src
'' przerobienie na VB
'' bo microsoft.azure.documentDB, który miał działać na 'niskim' MinSDK nie działa (nie widać namespace)


'' potrzebne rzeczy:
''Private gmCosmosClient As Microsoft.Azure.Cosmos.CosmosClient = Nothing
''Private gmCosmosDatabase As Microsoft.Azure.Cosmos.Database = Nothing
''Private mCosmosLoginContainer As Microsoft.Azure.Cosmos.Container = Nothing
''    gmCosmosClient = New Microsoft.Azure.Cosmos.CosmosClient(COSMOS_ENDPOINT_URI, COSMOS_PRIMARY_KEY_RW)
''    gmCosmosDatabase = gmCosmosClient.GetDatabase(COSMOS_DB_NAME)
''    mCosmosContainer = gmCosmosDatabase.GetContainer("przyptu")
''Dim sqlQueryDef As New Microsoft.Azure.Cosmos.QueryDefinition(sQry)
''Using oIterator As Microsoft.Azure.Cosmos.FeedIterator(Of Long) = oCont.GetItemQueryIterator(Of Long)(sqlQueryDef)

''    While oIterator.HasMoreResults
''        Dim currentResultSet As Microsoft.Azure.Cosmos.FeedResponse(Of Long) = Await oIterator.ReadNextAsync()
''        For Each oItem As Integer In currentResultSet

''Imports System
''Imports System.Collections.Generic
''Imports System.Diagnostics
''Imports System.Net
''Imports System.Text
''Imports System.Threading
''Imports System.Threading.Tasks
''Imports Global.Azure.Core
''Imports Microsoft.Azure.Cosmos.Handlers
''Imports Microsoft.Azure.Cosmos.Telemetry
''Imports Microsoft.Azure.Cosmos.Tracing
''Imports Microsoft.Azure.Cosmos.Tracing.TraceData
''Imports Microsoft.Azure.Documents



'Namespace Microsoft.Azure.Cosmos

'    Public Class CosmosClient
'        Implements System.IDisposable

'        Private _DisposedDateTimeUtc As System.DateTime? = Nothing
'        'Private ReadOnly DatabaseRootUri As String = Paths.Databases_Root
'        'Private accountConsistencyLevel As ConsistencyLevel?
'        Private isDisposed As Boolean = False
'        Friend Shared numberOfClientsCreated As Integer

'        'Friend Property DisposedDateTimeUtc As System.DateTime? = Nothing
'        '    Get
'        '        Return _DisposedDateTimeUtc
'        '    End Get
'        '    Private Set(ByVal value As System.DateTime?)
'        '        _DisposedDateTimeUtc = value
'        '    End Set
'        'End Property

'        Shared Sub New()
'#If PREVIEW Then
'            HttpConstants.Versions.CurrentVersion = HttpConstants.Versions.v2020_07_15;
'#Else
'            HttpConstants.Versions.CurrentVersion = HttpConstants.Versions.v2018_12_31
'#End If
'            HttpConstants.Versions.CurrentVersionUTF8 = System.Text.Encoding.UTF8.GetBytes(HttpConstants.Versions.CurrentVersion)

'            ' V3 always assumes assemblies exists
'            ' Shall revisit on feedback
'            ' NOTE: Native ServiceInteropWrapper.AssembliesExist has appsettings dependency which are proofed for CTL (native dll entry) scenarios.
'            ' Revert of this depends on handling such in direct assembly
'            ServiceInteropWrapper.AssembliesExist = New System.Lazy(Of Boolean)(Function() True)
'            Microsoft.Azure.Cosmos.Core.Trace.DefaultTrace.InitEventListener()

'            ' If a debugger is not attached remove the DefaultTraceListener. 
'            ' DefaultTraceListener can cause lock contention leading to availability issues
'            If Not System.Diagnostics.Debugger.IsAttached Then
'                Call Microsoft.Azure.Cosmos.CosmosClient.RemoveDefaultTraceListener()
'            End If
'        End Sub


'        '''' <summary>
'        '''' Creates a new CosmosClient with the connection string.
'        'Public Sub New(ByVal connectionString As String, ByVal Optional clientOptions As CosmosClientOptions = Nothing)
'        '    Me.New(CosmosClientOptions.GetAccountEndpoint(connectionString), CosmosClientOptions.GetAccountKey(connectionString), clientOptions)
'        'End Sub

'        Public Sub New(ByVal accountEndpoint As String, ByVal authKeyOrResourceToken As String, ByVal Optional clientOptions As CosmosClientOptions = Nothing)
'            Me.New(accountEndpoint, AuthorizationTokenProvider.CreateWithResourceTokenOrAuthKey(authKeyOrResourceToken), clientOptions)
'        End Sub

'        'Public Sub New(ByVal accountEndpoint As String, ByVal tokenCredential As TokenCredential, ByVal Optional clientOptions As CosmosClientOptions = Nothing)
'        '    Me.New(accountEndpoint, New AuthorizationTokenProviderTokenCredential(tokenCredential, New System.Uri(accountEndpoint), clientOptions?.TokenCredentialBackgroundRefreshInterval), clientOptions)
'        'End Sub

'        ''' <summary>
'        ''' Used by Compute
'        ''' Creates a new CosmosClient with the AuthorizationTokenProvider
'        ''' </summary>
'        Friend Sub New(ByVal accountEndpoint As String, ByVal authorizationTokenProvider As AuthorizationTokenProvider, ByVal clientOptions As CosmosClientOptions)
'            If String.IsNullOrEmpty(accountEndpoint) Then
'                Throw New System.ArgumentNullException(NameOf(accountEndpoint))
'            End If

'            Me.Endpoint = New System.Uri(accountEndpoint)
'            If authorizationTokenProvider Is Nothing Then
'                Throw New System.ArgumentNullException(NameOf(authorizationTokenProvider))
'            End If
'            Me.AuthorizationTokenProvider = authorizationTokenProvider

'            clientOptions = New CosmosClientOptions()

'            Me.ClientId = Me.IncrementNumberOfClientsCreated()
'            Me.ClientContext = ClientContextCore.Create(Me, clientOptions)
'            'Me.ClientConfigurationTraceDatum = New ClientConfigurationTraceDatum(Me.ClientContext, System.DateTime.UtcNow)
'        End Sub

'        '        ''' <summary>
'        '        ''' Creates a new CosmosClient with the account endpoint URI string and TokenCredential.
'        '        ''' In addition to that it initializes the client with containers provided i.e The SDK warms up the caches and 
'        '        ''' connections before the first call to the service is made. Use this to obtain lower latency while startup of your application.
'        '        ''' CosmosClient is thread-safe. Its recommended to maintain a single instance of CosmosClient per lifetime 
'        '        ''' of the application which enables efficient connection management and performance. Please refer to the
'        '        ''' <seehref="https://docs.microsoft.com/azure/cosmos-db/performance-tips">performance guide</see>.
'        '        ''' </summary>
'        '        ''' <paramname="accountEndpoint">The cosmos service endpoint to use</param>
'        '        ''' <paramname="authKeyOrResourceToken">The cosmos account key or resource token to use to create the client.</param>
'        '        ''' <paramname="containers">Containers to be initialized identified by it's database name and container name.</param>
'        '        ''' <paramname="cosmosClientOptions">(Optional) client options</param>
'        '        ''' <paramname="cancellationToken">(Optional) Cancellation Token</param>
'        '        ''' <returns>
'        '        ''' A CosmosClient object.
'        '        ''' </returns>
'        '        ''' <example>
'        '        ''' The CosmosClient is created with the AccountEndpoint, AccountKey or ResourceToken and 2 containers in the account are initialized
'        '        ''' <codelanguage="c#">
'        '        ''' <![CDATA[
'        '        ''' using Microsoft.Azure.Cosmos;
'        '        ''' List<(string, string)> containersToInitialize = new List<(string, string)>
'        '        ''' { ("DatabaseName1", "ContainerName1"), ("DatabaseName2", "ContainerName2") };
'        '        ''' 
'        '        ''' CosmosClient cosmosClient = await CosmosClient.CreateAndInitializeAsync("account-endpoint-from-portal", 
'        '        '''                                                                         "account-key-from-portal",
'        '        '''                                                                         containersToInitialize)
'        '        ''' 
'        '        ''' // Dispose cosmosClient at application exit
'        '        ''' ]]>
'        '        ''' </code>
'        '        ''' </example>
'        '        Public Shared Async Function CreateAndInitializeAsync(ByVal accountEndpoint As String, ByVal authKeyOrResourceToken As String, ByVal containers As System.Collections.Generic.IReadOnlyList(Of (String, String)), ByVal Optional cosmosClientOptions As CosmosClientOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of Microsoft.Azure.Cosmos.CosmosClient)
'        '            If containers Is Nothing Then
'        '                Throw New System.ArgumentNullException(NameOf(containers))
'        '            End If

'        '            Dim cosmosClient As Microsoft.Azure.Cosmos.CosmosClient = New Microsoft.Azure.Cosmos.CosmosClient(accountEndpoint, authKeyOrResourceToken, cosmosClientOptions)
'        '            Await cosmosClient.InitializeContainersAsync(containers, cancellationToken)
'        '            Return cosmosClient
'        '        End Function

'        '        ''' <summary>
'        '        ''' Creates a new CosmosClient with the account endpoint URI string and TokenCredential.
'        '        ''' In addition to that it initializes the client with containers provided i.e The SDK warms up the caches and 
'        '        ''' connections before the first call to the service is made. Use this to obtain lower latency while startup of your application.
'        '        ''' CosmosClient is thread-safe. Its recommended to maintain a single instance of CosmosClient per lifetime 
'        '        ''' of the application which enables efficient connection management and performance. Please refer to the
'        '        ''' <seehref="https://docs.microsoft.com/azure/cosmos-db/performance-tips">performance guide</see>.
'        '        ''' </summary>
'        '        ''' <paramname="connectionString">The connection string to the cosmos account. ex: https://mycosmosaccount.documents.azure.com:443/;AccountKey=SuperSecretKey; </param>
'        '        ''' <paramname="containers">Containers to be initialized identified by it's database name and container name.</param>
'        '        ''' <paramname="cosmosClientOptions">(Optional) client options</param>
'        '        ''' <paramname="cancellationToken">(Optional) Cancellation Token</param>
'        '        ''' <returns>
'        '        ''' A CosmosClient object.
'        '        ''' </returns>
'        '        ''' <example>
'        '        ''' The CosmosClient is created with the ConnectionString and 2 containers in the account are initialized
'        '        ''' <codelanguage="c#">
'        '        ''' <![CDATA[
'        '        ''' using Microsoft.Azure.Cosmos;
'        '        ''' List<(string, string)> containersToInitialize = new List<(string, string)>
'        '        ''' { ("DatabaseName1", "ContainerName1"), ("DatabaseName2", "ContainerName2") };
'        '        ''' 
'        '        ''' CosmosClient cosmosClient = await CosmosClient.CreateAndInitializeAsync("connection-string-from-portal",
'        '        '''                                                                         containersToInitialize)
'        '        ''' 
'        '        ''' // Dispose cosmosClient at application exit
'        '        ''' ]]>
'        '        ''' </code>
'        '        ''' </example>
'        '        Public Shared Async Function CreateAndInitializeAsync(ByVal connectionString As String, ByVal containers As System.Collections.Generic.IReadOnlyList(Of (String, String)), ByVal Optional cosmosClientOptions As CosmosClientOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of Microsoft.Azure.Cosmos.CosmosClient)
'        '            If containers Is Nothing Then
'        '                Throw New System.ArgumentNullException(NameOf(containers))
'        '            End If

'        '            Dim cosmosClient As Microsoft.Azure.Cosmos.CosmosClient = New Microsoft.Azure.Cosmos.CosmosClient(connectionString, cosmosClientOptions)
'        '            Await cosmosClient.InitializeContainersAsync(containers, cancellationToken)
'        '            Return cosmosClient
'        '        End Function

'        '        ''' <summary>
'        '        ''' Creates a new CosmosClient with the account endpoint URI string and TokenCredential.
'        '        ''' In addition to that it initializes the client with containers provided i.e The SDK warms up the caches and 
'        '        ''' connections before the first call to the service is made. Use this to obtain lower latency while startup of your application.
'        '        ''' CosmosClient is thread-safe. Its recommended to maintain a single instance of CosmosClient per lifetime 
'        '        ''' of the application which enables efficient connection management and performance. Please refer to the
'        '        ''' <seehref="https://docs.microsoft.com/azure/cosmos-db/performance-tips">performance guide</see>.
'        '        ''' </summary>
'        '        ''' <paramname="accountEndpoint">The cosmos service endpoint to use.</param>
'        '        ''' <paramname="tokenCredential"><seecref="TokenCredential"/>The token to provide AAD token for authorization.</param>
'        '        ''' <paramname="containers">Containers to be initialized identified by it's database name and container name.</param>
'        '        ''' <paramname="cosmosClientOptions">(Optional) client options</param>
'        '        ''' <paramname="cancellationToken">(Optional) Cancellation Token</param>
'        '        ''' <returns>
'        '        ''' A CosmosClient object.
'        '        ''' </returns>
'        '        Public Shared Async Function CreateAndInitializeAsync(ByVal accountEndpoint As String, ByVal tokenCredential As TokenCredential, ByVal containers As System.Collections.Generic.IReadOnlyList(Of (String, String)), ByVal Optional cosmosClientOptions As CosmosClientOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of Microsoft.Azure.Cosmos.CosmosClient)
'        '            If containers Is Nothing Then
'        '                Throw New System.ArgumentNullException(NameOf(containers))
'        '            End If

'        '            Dim cosmosClient As Microsoft.Azure.Cosmos.CosmosClient = New Microsoft.Azure.Cosmos.CosmosClient(accountEndpoint, tokenCredential, cosmosClientOptions)
'        '            Await cosmosClient.InitializeContainersAsync(containers, cancellationToken)
'        '            Return cosmosClient
'        '        End Function

'        '        ''' <summary>
'        '        ''' Used for unit testing only.
'        '        ''' </summary>
'        '        ''' <remarks>This constructor should be removed at some point. The mocking should happen in a derived class.</remarks>
'        '        Friend Sub New(ByVal accountEndpoint As String, ByVal authKeyOrResourceToken As String, ByVal cosmosClientOptions As CosmosClientOptions, ByVal documentClient As DocumentClient)
'        '            If String.IsNullOrEmpty(accountEndpoint) Then
'        '                Throw New System.ArgumentNullException(NameOf(accountEndpoint))
'        '            End If

'        '            If String.IsNullOrEmpty(authKeyOrResourceToken) Then
'        '                Throw New System.ArgumentNullException(NameOf(authKeyOrResourceToken))
'        '            End If

'        '            If cosmosClientOptions Is Nothing Then
'        '                Throw New System.ArgumentNullException(NameOf(cosmosClientOptions))
'        '            End If

'        '            If documentClient Is Nothing Then
'        '                Throw New System.ArgumentNullException(NameOf(documentClient))
'        '            End If

'        '            Me.Endpoint = New System.Uri(accountEndpoint)
'        '            Me.AccountKey = authKeyOrResourceToken
'        '            Me.AuthorizationTokenProvider = Me.AuthorizationTokenProvider.CreateWithResourceTokenOrAuthKey(authKeyOrResourceToken)
'        '            Me.ClientContext = ClientContextCore.Create(Me, documentClient, cosmosClientOptions)
'        '            Me.ClientConfigurationTraceDatum = New ClientConfigurationTraceDatum(Me.ClientContext, System.DateTime.UtcNow)
'        '        End Sub

'        '        ''' <summary>
'        '        ''' The <seecref="Cosmos.CosmosClientOptions"/> used initialize CosmosClient.
'        '        ''' </summary>
'        '        Public Overridable ReadOnly Property ClientOptions As CosmosClientOptions
'        '            Get
'        '                Return Me.ClientContext.ClientOptions
'        '            End Get
'        '        End Property

'        '        ''' <summary>
'        '        ''' The response factory used to create CosmosClient response types.
'        '        ''' </summary>
'        '        ''' <remarks>
'        '        ''' This can be used for generating responses for tests, and allows users to create
'        '        ''' a custom container that modifies the response. For example the client encryption
'        '        ''' uses this to decrypt responses before returning to the caller.
'        '        ''' </remarks>
'        '#If PREVIEW Then
'        '        public
'        '#Else
'        '#End If
'        '        Friend Overridable ReadOnly Property ResponseFactory As CosmosResponseFactory
'        '            Get
'        '                Return Me.ClientContext.ResponseFactory
'        '            End Get
'        '        End Property

'        '        ''' <summary>
'        '        ''' Gets the endpoint Uri for the Azure Cosmos DB service.
'        '        ''' </summary>
'        '        ''' <value>
'        '        ''' The Uri for the account endpoint.
'        '        ''' </value>
'        '        ''' <seealsocref="System.Uri"/>
'        Public Overridable ReadOnly Property Endpoint As System.Uri

'        '        ''' <summary>
'        '        ''' Gets the AuthKey or resource token used by the client from the Azure Cosmos DB service.
'        '        ''' </summary>
'        '        ''' <value>
'        '        ''' The AuthKey used by the client.
'        '        ''' </value>
'        '        Friend ReadOnly Property AccountKey As String

'        '        ''' <summary>
'        '        ''' Gets the AuthorizationTokenProvider used to generate the authorization token
'        '        ''' </summary>
'        Friend ReadOnly Property AuthorizationTokenProvider As AuthorizationTokenProvider

'        '        Friend ReadOnly Property DocumentClient As DocumentClient
'        '            Get
'        '                Return Me.ClientContext.DocumentClient
'        '            End Get
'        '        End Property

'        '        Friend ReadOnly Property RequestHandler As RequestInvokerHandler
'        '            Get
'        '                Return Me.ClientContext.RequestHandler
'        '            End Get
'        '        End Property

'        Friend ReadOnly Property ClientContext As CosmosClientContext
'        '        Friend ReadOnly Property ClientConfigurationTraceDatum As ClientConfigurationTraceDatum
'        Friend ReadOnly Property ClientId As Integer

'        '        ''' <summary>
'        '        ''' Reads the <seecref="Microsoft.Azure.Cosmos.AccountProperties"/> for the Azure Cosmos DB account.
'        '        ''' </summary>
'        '        ''' <returns>
'        '        ''' A <seecref="AccountProperties"/> wrapped in a <seecref="System.Threading.Tasks.Task"/> object.
'        '        ''' </returns>
'        '        Public Overridable Function ReadAccountAsync() As System.Threading.Tasks.Task(Of AccountProperties)
'        '            Return Me.ClientContext.OperationHelperAsync(NameOf(ReadAccountAsync), Nothing, Function(trace) CType(Me.DocumentClient, IDocumentClientInternal).GetDatabaseAccountInternalAsync(Me.Endpoint))
'        '        End Function

'        '        ''' <summary>
'        '        ''' Returns a proxy reference to a database. 
'        '        ''' </summary>
'        '        ''' <paramname="id">The Cosmos database id</param>
'        '        ''' <remarks>
'        '        ''' <seecref="Database"/> proxy reference doesn't guarantee existence.
'        '        ''' Please ensure database exists through <seecref="CosmosClient.CreateDatabaseAsync(String,,RequestOptions,CancellationToken)"/> 
'        '        ''' or <seecref="CosmosClient.CreateDatabaseIfNotExistsAsync(String,,RequestOptions,CancellationToken)"/>, before
'        '        ''' operating on it.
'        '        ''' </remarks>
'        '        ''' <example>
'        '        ''' <codelanguage="c#">
'        '        ''' <![CDATA[
'        '        ''' Database db = cosmosClient.GetDatabase("myDatabaseId");
'        '        ''' DatabaseResponse response = await db.ReadAsync();
'        '        ''' ]]>
'        '        ''' </code>
'        '        ''' </example>
'        '        ''' <returns>Cosmos database proxy</returns>
'        '        Public Overridable Function GetDatabase(ByVal id As String) As Database
'        '            Return New DatabaseInlineCore(Me.ClientContext, id)
'        '        End Function

'        '        ''' <summary>
'        '        ''' Returns a proxy reference to a container. 
'        '        ''' </summary>
'        '        ''' <remarks>
'        '        ''' <seecref="Container"/> proxy reference doesn't guarantee existence.
'        '        ''' Please ensure container exists through <seecref="Database.CreateContainerAsync(ContainerProperties,,RequestOptions,CancellationToken)"/> 
'        '        ''' or <seecref="Database.CreateContainerIfNotExistsAsync(ContainerProperties,,RequestOptions,CancellationToken)"/>, before
'        '        ''' operating on it.
'        '        ''' </remarks>
'        '        ''' <paramname="databaseId">Cosmos database name</param>
'        '        ''' <paramname="containerId">Cosmos container name</param>
'        '        ''' <returns>Cosmos container proxy</returns>
'        '        Public Overridable Function GetContainer(ByVal databaseId As String, ByVal containerId As String) As Container
'        '            If String.IsNullOrEmpty(databaseId) Then
'        '                Throw New System.ArgumentNullException(NameOf(databaseId))
'        '            End If

'        '            If String.IsNullOrEmpty(containerId) Then
'        '                Throw New System.ArgumentNullException(NameOf(containerId))
'        '            End If

'        '            Return Me.GetDatabase(CStr((databaseId))).GetContainer(containerId)
'        '        End Function

'        '        ''' <summary>
'        '        ''' Sends a request for creating a database.
'        '        '''
'        '        ''' A database manages users, permissions and a set of containers.
'        '        ''' Each Azure Cosmos DB Database Account is able to support multiple independent named databases,
'        '        ''' with the database being the logical container for data.
'        '        '''
'        '        ''' Each Database consists of one or more containers, each of which in turn contain one or more
'        '        ''' documents. Since databases are an administrative resource, the Service Master Key will be
'        '        ''' required in order to access and successfully complete any action using the User APIs.
'        '        ''' </summary>
'        '        ''' <paramname="id">The database id.</param>
'        '        ''' <paramname="throughput">(Optional) The throughput provisioned for a database in measurement of Request Units per second in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) A set of options that can be set.</param>
'        '        ''' <paramname="cancellationToken">(Optional) <seecref="CancellationToken"/> representing request cancellation.</param>
'        '        ''' <returns>A <seecref="Task"/> containing a <seecref="DatabaseResponse"/> which wraps a <seecref="DatabaseProperties"/> containing the resource record.</returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/request-units">Request Units</seealso>
'        '        Public Overridable Function CreateDatabaseAsync(ByVal id As String, ByVal Optional throughput As Integer? = Nothing, ByVal Optional requestOptions As RequestOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of DatabaseResponse)
'        '            If String.IsNullOrEmpty(id) Then
'        '                Throw New System.ArgumentNullException(NameOf(id))
'        '            End If

'        '            Return Me.ClientContext.OperationHelperAsync(NameOf(CreateDatabaseAsync), requestOptions, Function(trace)
'        '                                                                                                          Dim databaseProperties As DatabaseProperties = Me.PrepareDatabaseProperties(id)
'        '                                                                                                          Dim throughputProperties As ThroughputProperties = throughputProperties.CreateManualThroughput(throughput)
'        '                                                                                                          Return Me.CreateDatabaseInternalAsync(databaseProperties:=databaseProperties, throughputProperties:=throughputProperties, requestOptions:=requestOptions, trace:=trace, cancellationToken:=cancellationToken)
'        '                                                                                                      End Function)
'        '        End Function

'        '        ''' <summary>
'        '        ''' Sends a request for creating a database.
'        '        '''
'        '        ''' A database manages users, permissions and a set of containers.
'        '        ''' Each Azure Cosmos DB Database Account is able to support multiple independent named databases,
'        '        ''' with the database being the logical container for data.
'        '        '''
'        '        ''' Each Database consists of one or more containers, each of which in turn contain one or more
'        '        ''' documents. Since databases are an administrative resource, the Service Master Key will be
'        '        ''' required in order to access and successfully complete any action using the User APIs.
'        '        ''' </summary>
'        '        ''' <paramname="id">The database id.</param>
'        '        ''' <paramname="throughputProperties">(Optional) The throughput provisioned for a database in measurement of Request Units per second in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) A set of options that can be set.</param>
'        '        ''' <paramname="cancellationToken">(Optional) <seecref="CancellationToken"/> representing request cancellation.</param>
'        '        ''' <returns>A <seecref="Task"/> containing a <seecref="DatabaseResponse"/> which wraps a <seecref="DatabaseProperties"/> containing the resource record.</returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/request-units">Request Units</seealso>
'        '        Public Overridable Function CreateDatabaseAsync(ByVal id As String, ByVal throughputProperties As ThroughputProperties, ByVal Optional requestOptions As RequestOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of DatabaseResponse)
'        '            If String.IsNullOrEmpty(id) Then
'        '                Throw New System.ArgumentNullException(NameOf(id))
'        '            End If

'        '            Return Me.ClientContext.OperationHelperAsync(NameOf(CreateDatabaseAsync), requestOptions, Function(trace)
'        '                                                                                                          Dim databaseProperties As DatabaseProperties = Me.PrepareDatabaseProperties(id)
'        '                                                                                                          Return Me.CreateDatabaseInternalAsync(databaseProperties:=databaseProperties, throughputProperties:=throughputProperties, requestOptions:=requestOptions, trace:=trace, cancellationToken:=cancellationToken)
'        '                                                                                                      End Function)
'        '        End Function

'        '        ''' <summary>
'        '        ''' <para>Check if a database exists, and if it doesn't, create it.
'        '        ''' Only the database id is used to verify if there is an existing database. Other database properties 
'        '        ''' such as throughput are not validated and can be different then the passed properties.</para>
'        '        ''' 
'        '        ''' <para>A database manages users, permissions and a set of containers.
'        '        ''' Each Azure Cosmos DB Database Account is able to support multiple independent named databases,
'        '        ''' with the database being the logical container for data.</para>
'        '        '''
'        '        ''' <para>Each Database consists of one or more containers, each of which in turn contain one or more
'        '        ''' documents. Since databases are an administrative resource, the Service Master Key will be
'        '        ''' required in order to access and successfully complete any action using the User APIs.</para>
'        '        ''' </summary>
'        '        ''' <paramname="id">The database id.</param>
'        '        ''' <paramname="throughputProperties">The throughput provisioned for a database in measurement of Request Units per second in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) A set of additional options that can be set.</param>
'        '        ''' <paramname="cancellationToken">(Optional) <seecref="CancellationToken"/> representing request cancellation.</param>
'        '        ''' <returns>A <seecref="Task"/> containing a <seecref="DatabaseResponse"/> which wraps a <seecref="DatabaseProperties"/> containing the resource record.
'        '        ''' <listtype="table">
'        '        '''     <listheader>
'        '        '''         <term>StatusCode</term><description>Common success StatusCodes for the CreateDatabaseIfNotExistsAsync operation</description>
'        '        '''     </listheader>
'        '        '''     <item>
'        '        '''         <term>201</term><description>Created - New database is created.</description>
'        '        '''     </item>
'        '        '''     <item>
'        '        '''         <term>200</term><description>OK - This means the database already exists.</description>
'        '        '''     </item>
'        '        ''' </list>
'        '        ''' </returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/request-units">Request Units</seealso>
'        '        Public Overridable Function CreateDatabaseIfNotExistsAsync(ByVal id As String, ByVal throughputProperties As ThroughputProperties, ByVal Optional requestOptions As RequestOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of DatabaseResponse)
'        '            Return If(String.IsNullOrEmpty(id), CSharpImpl.__Throw(Of System.Object)(New System.ArgumentNullException(NameOf(id))), Me.ClientContext.OperationHelperAsync(NameOf(CreateDatabaseIfNotExistsAsync), requestOptions, Async Function(trace)
'        '                                                                                                                                                                                                                                      Dim totalRequestCharge As Double = 0
'        '                                                                                                                                                                                                                                      ' Doing a Read before Create will give us better latency for existing databases
'        '                                                                                                                                                                                                                                      Dim databaseProperties As DatabaseProperties = Me.PrepareDatabaseProperties(id)
'        '                                                                                                                                                                                                                                      Dim database As DatabaseCore = CType(Me.GetDatabase(id), DatabaseCore)

'        '                                                                                                                                                                                                                                      Using readResponse As ResponseMessage = Await database.ReadStreamAsync(requestOptions:=requestOptions, trace:=trace, cancellationToken:=cancellationToken)
'        '                                                                                                                                                                                                                                          totalRequestCharge = readResponse.Headers.RequestCharge

'        '                                                                                                                                                                                                                                          If readResponse.StatusCode <> System.Net.HttpStatusCode.NotFound Then
'        '                                                                                                                                                                                                                                              Return Me.ClientContext.ResponseFactory.CreateDatabaseResponse(database, readResponse)
'        '                                                                                                                                                                                                                                          End If
'        '                                                                                                                                                                                                                                      End Using

'        '                                                                                                                                                                                                                                      Using createResponse As ResponseMessage = Await Me.CreateDatabaseStreamInternalAsync(databaseProperties, throughputProperties, requestOptions, trace, cancellationToken)
'        '                                                                                                                                                                                                                                          totalRequestCharge += createResponse.Headers.RequestCharge
'        '                                                                                                                                                                                                                                          createResponse.Headers.RequestCharge = totalRequestCharge

'        '                                                                                                                                                                                                                                          If createResponse.StatusCode <> System.Net.HttpStatusCode.Conflict Then
'        '                                                                                                                                                                                                                                              Return Me.ClientContext.ResponseFactory.CreateDatabaseResponse(Me.GetDatabase(databaseProperties.Id), createResponse)
'        '                                                                                                                                                                                                                                          End If
'        '                                                                                                                                                                                                                                      End Using

'        '                                                                                                                                                                                                                                      ' This second Read is to handle the race condition when 2 or more threads have Read the database and only one succeeds with Create
'        '                                                                                                                                                                                                                                      ' so for the remaining ones we should do a Read instead of throwing Conflict exception
'        '                                                                                                                                                                                                                                      Using readResponseAfterConflict As ResponseMessage = Await database.ReadStreamAsync(requestOptions:=requestOptions, trace:=trace, cancellationToken:=cancellationToken)
'        '                                                                                                                                                                                                                                          totalRequestCharge += readResponseAfterConflict.Headers.RequestCharge
'        '                                                                                                                                                                                                                                          readResponseAfterConflict.Headers.RequestCharge = totalRequestCharge
'        '                                                                                                                                                                                                                                          Return Me.ClientContext.ResponseFactory.CreateDatabaseResponse(Me.GetDatabase(databaseProperties.Id), readResponseAfterConflict)
'        '                                                                                                                                                                                                                                      End Using
'        '                                                                                                                                                                                                                                  End Function))
'        '        End Function

'        '        ''' <summary>
'        '        ''' <para>Check if a database exists, and if it doesn't, create it.
'        '        ''' Only the database id is used to verify if there is an existing database. Other database properties 
'        '        ''' such as throughput are not validated and can be different then the passed properties.</para>
'        '        ''' 
'        '        ''' <para>A database manages users, permissions and a set of containers.
'        '        ''' Each Azure Cosmos DB Database Account is able to support multiple independent named databases,
'        '        ''' with the database being the logical container for data.</para>
'        '        '''
'        '        ''' <para>Each Database consists of one or more containers, each of which in turn contain one or more
'        '        ''' documents. Since databases are an administrative resource, the Service Master Key will be
'        '        ''' required in order to access and successfully complete any action using the User APIs.</para>
'        '        ''' </summary>
'        '        ''' <paramname="id">The database id.</param>
'        '        ''' <paramname="throughput">(Optional) The throughput provisioned for a database in measurement of Request Units per second in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) A set of additional options that can be set.</param>
'        '        ''' <paramname="cancellationToken">(Optional) <seecref="CancellationToken"/> representing request cancellation.</param>
'        '        ''' <returns>A <seecref="Task"/> containing a <seecref="DatabaseResponse"/> which wraps a <seecref="DatabaseProperties"/> containing the resource record.
'        '        ''' <listtype="table">
'        '        '''     <listheader>
'        '        '''         <term>StatusCode</term><description>Common success StatusCodes for the CreateDatabaseIfNotExistsAsync operation</description>
'        '        '''     </listheader>
'        '        '''     <item>
'        '        '''         <term>201</term><description>Created - New database is created.</description>
'        '        '''     </item>
'        '        '''     <item>
'        '        '''         <term>200</term><description>OK- This means the database already exists.</description>
'        '        '''     </item>
'        '        ''' </list>
'        '        ''' </returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/request-units">Request Units</seealso>
'        '        Public Overridable Function CreateDatabaseIfNotExistsAsync(ByVal id As String, ByVal Optional throughput As Integer? = Nothing, ByVal Optional requestOptions As RequestOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of DatabaseResponse)
'        '            Dim throughputProperties As ThroughputProperties = throughputProperties.CreateManualThroughput(throughput)
'        '            Return Me.CreateDatabaseIfNotExistsAsync(id, throughputProperties, requestOptions, cancellationToken)
'        '        End Function

'        '        ''' <summary>
'        '        ''' This method creates a query for databases under an Cosmos DB Account using a SQL statement with parameterized values. It returns a FeedIterator.
'        '        ''' For more information on preparing SQL statements with parameterized values, please see <seecref="QueryDefinition"/>.
'        '        ''' </summary>
'        '        ''' <paramname="queryDefinition">The cosmos SQL query definition.</param>
'        '        ''' <paramname="continuationToken">The continuation token in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) The options for the item query request.</param>
'        '        ''' <returns>An iterator to go through the databases.</returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <remarks>
'        '        ''' Refer to https://docs.microsoft.com/azure/cosmos-db/sql-query-getting-started for syntax and examples.
'        '        ''' <para>
'        '        ''' <seecref="Database.ReadAsync(RequestOptions,CancellationToken)"/> is recommended for single database look-up.
'        '        ''' </para>
'        '        ''' </remarks>
'        '        ''' <example>
'        '        ''' This create the type feed iterator for database with queryText as input,
'        '        ''' <codelanguage="c#">
'        '        ''' <![CDATA[
'        '        ''' QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c where c.status like @status")
'        '        '''     .WithParameter("@status", "start%");
'        '        ''' using (FeedIterator<DatabaseProperties> feedIterator = this.users.GetDatabaseQueryIterator<DatabaseProperties>(queryDefinition))
'        '        ''' {
'        '        '''     while (feedIterator.HasMoreResults)
'        '        '''     {
'        '        '''         FeedResponse<DatabaseProperties> response = await feedIterator.ReadNextAsync();
'        '        '''         foreach (var database in response)
'        '        '''         {
'        '        '''             Console.WriteLine(database);
'        '        '''         }
'        '        '''     }
'        '        ''' }
'        '        ''' ]]>
'        '        ''' </code>
'        '        ''' </example>
'        '        Public Overridable Function GetDatabaseQueryIterator(Of T)(ByVal queryDefinition As QueryDefinition, ByVal Optional continuationToken As String = Nothing, ByVal Optional requestOptions As QueryRequestOptions = Nothing) As FeedIterator(Of T)
'        '            Return New FeedIteratorInlineCore(Of T)(Me.GetDatabaseQueryIteratorHelper(Of T)(queryDefinition, continuationToken, requestOptions), Me.ClientContext)
'        '        End Function

'        '        ''' <summary>
'        '        ''' This method creates a query for databases under an Cosmos DB Account using a SQL statement with parameterized values. It returns a FeedIterator.
'        '        ''' For more information on preparing SQL statements with parameterized values, please see <seecref="QueryDefinition"/>.
'        '        ''' </summary>
'        '        ''' <paramname="queryDefinition">The cosmos SQL query definition.</param>
'        '        ''' <paramname="continuationToken">The continuation token in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) The options for the query request.</param>
'        '        ''' <returns>An iterator to go through the databases</returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <remarks>
'        '        ''' Refer to https://docs.microsoft.com/azure/cosmos-db/sql-query-getting-started for syntax and examples.
'        '        ''' <para>
'        '        ''' <seecref="Database.ReadStreamAsync(RequestOptions,CancellationToken)"/> is recommended for single database look-up.
'        '        ''' </para>
'        '        ''' </remarks>
'        '        ''' <example>
'        '        ''' Example on how to fully drain the query results.
'        '        ''' <codelanguage="c#">
'        '        ''' <![CDATA[
'        '        ''' QueryDefinition queryDefinition = new QueryDefinition("select * From c where c._rid = @rid")
'        '        '''               .WithParameter("@rid", "TheRidValue");
'        '        ''' using (FeedIterator feedIterator = this.CosmosClient.GetDatabaseQueryStreamIterator(
'        '        '''     queryDefinition)
'        '        ''' {
'        '        '''     while (feedIterator.HasMoreResults)
'        '        '''     {
'        '        '''         // Stream iterator returns a response with status for errors
'        '        '''         using(ResponseMessage response = await feedIterator.ReadNextAsync())
'        '        '''         {
'        '        '''             // Handle failure scenario. 
'        '        '''             if(!response.IsSuccessStatusCode)
'        '        '''             {
'        '        '''                 // Log the response.Diagnostics and handle the error
'        '        '''             }
'        '        '''         }
'        '        '''     }
'        '        ''' }
'        '        ''' ]]>
'        '        ''' </code>
'        '        ''' </example>
'        '        Public Overridable Function GetDatabaseQueryStreamIterator(ByVal queryDefinition As QueryDefinition, ByVal Optional continuationToken As String = Nothing, ByVal Optional requestOptions As QueryRequestOptions = Nothing) As FeedIterator
'        '            Return New FeedIteratorInlineCore(Me.GetDatabaseQueryStreamIteratorHelper(queryDefinition, continuationToken, requestOptions), Me.ClientContext)
'        '        End Function

'        '        ''' <summary>
'        '        ''' This method creates a query for databases under an Cosmos DB Account using a SQL statement. It returns a FeedIterator.
'        '        ''' For more information on preparing SQL statements with parameterized values, please see <seecref="QueryDefinition"/> overload.
'        '        ''' </summary>
'        '        ''' <paramname="queryText">The cosmos SQL query text.</param>
'        '        ''' <paramname="continuationToken">The continuation token in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) The options for the item query request.</param>
'        '        ''' <returns>An iterator to go through the databases.</returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <remarks>
'        '        ''' Refer to https://docs.microsoft.com/azure/cosmos-db/sql-query-getting-started for syntax and examples.
'        '        ''' <para>
'        '        ''' <seecref="Database.ReadAsync(RequestOptions,CancellationToken)"/> is recommended for single database look-up.
'        '        ''' </para>
'        '        ''' </remarks>
'        '        ''' <example>
'        '        ''' This create the type feed iterator for database with queryText as input,
'        '        ''' <codelanguage="c#">
'        '        ''' <![CDATA[
'        '        ''' string queryText = "SELECT * FROM c where c.status like 'start%'";
'        '        ''' using (FeedIterator<DatabaseProperties> feedIterator = this.users.GetDatabaseQueryIterator<DatabaseProperties>(queryText)
'        '        ''' {
'        '        '''     while (feedIterator.HasMoreResults)
'        '        '''     {
'        '        '''         FeedResponse<DatabaseProperties> response = await feedIterator.ReadNextAsync();
'        '        '''         foreach (var database in response)
'        '        '''         {
'        '        '''             Console.WriteLine(database);
'        '        '''         }
'        '        '''     }
'        '        ''' }
'        '        ''' ]]>
'        '        ''' </code>
'        '        ''' </example>
'        '        Public Overridable Function GetDatabaseQueryIterator(Of T)(ByVal Optional queryText As String = Nothing, ByVal Optional continuationToken As String = Nothing, ByVal Optional requestOptions As QueryRequestOptions = Nothing) As FeedIterator(Of T)
'        '            Dim queryDefinition As QueryDefinition = Nothing

'        '            If Not Equals(queryText, Nothing) Then
'        '                queryDefinition = New QueryDefinition(queryText)
'        '            End If

'        '            Return New FeedIteratorInlineCore(Of T)(Me.GetDatabaseQueryIteratorHelper(Of T)(queryDefinition, continuationToken, requestOptions), Me.ClientContext)
'        '        End Function

'        '        ''' <summary>
'        '        ''' This method creates a query for databases under an Cosmos DB Account using a SQL statement. It returns a FeedIterator.
'        '        ''' For more information on preparing SQL statements with parameterized values, please see <seecref="QueryDefinition"/> overload.
'        '        ''' </summary>
'        '        ''' <paramname="queryText">The cosmos SQL query text.</param>
'        '        ''' <paramname="continuationToken">The continuation token in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) The options for the query request.</param>
'        '        ''' <returns>An iterator to go through the databases</returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <remarks>
'        '        ''' Refer to https://docs.microsoft.com/azure/cosmos-db/sql-query-getting-started for syntax and examples.
'        '        ''' <para>
'        '        ''' <seecref="Database.ReadStreamAsync(RequestOptions,CancellationToken)"/> is recommended for single database look-up.
'        '        ''' </para>
'        '        ''' </remarks>
'        '        ''' <example>
'        '        ''' Example on how to fully drain the query results.
'        '        ''' <codelanguage="c#">
'        '        ''' <![CDATA[
'        '        ''' using (FeedIterator feedIterator = this.CosmosClient.GetDatabaseQueryStreamIterator(
'        '        '''     ("select * From c where c._rid = 'TheRidValue'")
'        '        ''' {
'        '        '''     while (feedIterator.HasMoreResults)
'        '        '''     {
'        '        '''         // Stream iterator returns a response with status for errors
'        '        '''         using(ResponseMessage response = await feedIterator.ReadNextAsync())
'        '        '''         {
'        '        '''             // Handle failure scenario. 
'        '        '''             if(!response.IsSuccessStatusCode)
'        '        '''             {
'        '        '''                 // Log the response.Diagnostics and handle the error
'        '        '''             }
'        '        '''         }
'        '        '''     }
'        '        ''' }
'        '        ''' ]]>
'        '        ''' </code>
'        '        ''' </example>
'        '        Public Overridable Function GetDatabaseQueryStreamIterator(ByVal Optional queryText As String = Nothing, ByVal Optional continuationToken As String = Nothing, ByVal Optional requestOptions As QueryRequestOptions = Nothing) As FeedIterator
'        '            Dim queryDefinition As QueryDefinition = Nothing

'        '            If Not Equals(queryText, Nothing) Then
'        '                queryDefinition = New QueryDefinition(queryText)
'        '            End If

'        '            Return New FeedIteratorInlineCore(Me.GetDatabaseQueryStreamIterator(queryDefinition, continuationToken, requestOptions), Me.ClientContext)
'        '        End Function

'        '        ''' <summary>
'        '        ''' Send a request for creating a database.
'        '        '''
'        '        ''' A database manages users, permissions and a set of containers.
'        '        ''' Each Azure Cosmos DB Database Account is able to support multiple independent named databases,
'        '        ''' with the database being the logical container for data.
'        '        '''
'        '        ''' Each Database consists of one or more containers, each of which in turn contain one or more
'        '        ''' documents. Since databases are an administrative resource, the Service Master Key will be
'        '        ''' required in order to access and successfully complete any action using the User APIs.
'        '        ''' </summary>
'        '        ''' <paramname="databaseProperties">The database properties</param>
'        '        ''' <paramname="throughput">(Optional) The throughput provisioned for a database in measurement of Request Units per second in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) A set of options that can be set.</param>
'        '        ''' <paramname="cancellationToken">(Optional) <seecref="CancellationToken"/> representing request cancellation.</param>
'        '        ''' <returns>A <seecref="Task"/> containing a <seecref="DatabaseResponse"/> which wraps a <seecref="DatabaseProperties"/> containing the resource record.</returns>
'        '        ''' <exception>https://aka.ms/cosmosdb-dot-net-exceptions</exception>
'        '        ''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/request-units">Request Units</seealso>
'        '        Public Overridable Function CreateDatabaseStreamAsync(ByVal databaseProperties As DatabaseProperties, ByVal Optional throughput As Integer? = Nothing, ByVal Optional requestOptions As RequestOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of ResponseMessage)
'        '            If databaseProperties Is Nothing Then
'        '                Throw New System.ArgumentNullException(NameOf(databaseProperties))
'        '            End If

'        '            Return Me.ClientContext.OperationHelperAsync(NameOf(CreateDatabaseStreamAsync), requestOptions, Function(trace)
'        '                                                                                                                Me.ClientContext.ValidateResource(databaseProperties.Id)
'        '                                                                                                                Return Me.CreateDatabaseStreamInternalAsync(databaseProperties, ThroughputProperties.CreateManualThroughput(throughput), requestOptions, trace, cancellationToken)
'        '                                                                                                            End Function)
'        '        End Function

'        '        ''' <summary>
'        '        ''' Removes the DefaultTraceListener which causes locking issues which leads to avability problems. 
'        '        ''' </summary>
'        '        Private Shared Sub RemoveDefaultTraceListener()
'        '            ' The TraceSource already has the default trace listener
'        '            Dim defaultTraceListener As System.Diagnostics.DefaultTraceListener = Nothing

'        '            If Core.Trace.DefaultTrace.TraceSource.Listeners.Count > 0 Then
'        '                Dim removeDefaultTraceListeners As System.Collections.Generic.List(Of System.Diagnostics.DefaultTraceListener) = New System.Collections.Generic.List(Of System.Diagnostics.DefaultTraceListener)()

'        '                For Each traceListnerObject As Object In Core.Trace.DefaultTrace.TraceSource.Listeners

'        '                    If CSharpImpl.__Assign(defaultTraceListener, TryCast(traceListnerObject, System.Diagnostics.DefaultTraceListener)) IsNot Nothing Then
'        '                        removeDefaultTraceListeners.Add(defaultTraceListener)
'        '                    End If
'        '                Next

'        '                ' Remove all the default trace listeners
'        '                For Each defaultTraceListener As System.Diagnostics.DefaultTraceListener In removeDefaultTraceListeners
'        '                    Core.Trace.DefaultTrace.TraceSource.Listeners.Remove(defaultTraceListener)
'        '                Next
'        '            End If
'        '        End Sub

'        '        Friend Overridable Async Function GetAccountConsistencyLevelAsync() As System.Threading.Tasks.Task(Of ConsistencyLevel)
'        '            If Not Me.accountConsistencyLevel.HasValue Then
'        '                Me.accountConsistencyLevel = Await Me.DocumentClient.GetDefaultConsistencyLevelAsync()
'        '            End If

'        '            Return Me.accountConsistencyLevel.Value
'        '        End Function

'        '        Friend Function PrepareDatabaseProperties(ByVal id As String) As DatabaseProperties
'        '            If String.IsNullOrWhiteSpace(id) Then
'        '                Throw New System.ArgumentNullException(NameOf(id))
'        '            End If

'        '            Dim databaseProperties As DatabaseProperties = New DatabaseProperties() With {
'        '                .id = id
'        '            }
'        '            Me.ClientContext.ValidateResource(databaseProperties.Id)
'        '            Return databaseProperties
'        '        End Function

'        '        ''' <summary>
'        '        ''' Send a request for creating a database.
'        '        '''
'        '        ''' A database manages users, permissions and a set of containers.
'        '        ''' Each Azure Cosmos DB Database Account is able to support multiple independent named databases,
'        '        ''' with the database being the logical container for data.
'        '        '''
'        '        ''' Each Database consists of one or more containers, each of which in turn contain one or more
'        '        ''' documents. Since databases are an administrative resource, the Service Master Key will be
'        '        ''' required in order to access and successfully complete any action using the User APIs.
'        '        ''' </summary>
'        '        ''' <paramname="databaseProperties">The database properties</param>
'        '        ''' <paramname="throughputProperties">(Optional) The throughput provisioned for a database in measurement of Request Units per second in the Azure Cosmos DB service.</param>
'        '        ''' <paramname="requestOptions">(Optional) A set of options that can be set.</param>
'        '        ''' <paramname="cancellationToken">(Optional) <seecref="CancellationToken"/> representing request cancellation.</param>
'        '        ''' <returns>A <seecref="Task"/> containing a <seecref="DatabaseResponse"/> which wraps a <seecref="DatabaseProperties"/> containing the resource record.</returns>
'        '        ''' <seealsohref="https://docs.microsoft.com/azure/cosmos-db/request-units">Request Units</seealso>
'        '        Friend Overridable Function CreateDatabaseStreamAsync(ByVal databaseProperties As DatabaseProperties, ByVal throughputProperties As ThroughputProperties, ByVal Optional requestOptions As RequestOptions = Nothing, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task(Of ResponseMessage)
'        '            If databaseProperties Is Nothing Then
'        '                Throw New System.ArgumentNullException(NameOf(databaseProperties))
'        '            End If

'        '            Return Me.ClientContext.OperationHelperAsync(NameOf(CreateDatabaseIfNotExistsAsync), requestOptions, Function(trace)
'        '                                                                                                                     Me.ClientContext.ValidateResource(databaseProperties.Id)
'        '                                                                                                                     Return Me.CreateDatabaseStreamInternalAsync(databaseProperties, throughputProperties, requestOptions, trace, cancellationToken)
'        '                                                                                                                 End Function)
'        '        End Function

'        '        Private Async Function CreateDatabaseInternalAsync(ByVal databaseProperties As DatabaseProperties, ByVal throughputProperties As ThroughputProperties, ByVal requestOptions As RequestOptions, ByVal trace As ITrace, ByVal cancellationToken As System.Threading.CancellationToken) As System.Threading.Tasks.Task(Of DatabaseResponse)
'        '            Dim response As ResponseMessage = Await Me.ClientContext.ProcessResourceOperationStreamAsync(resourceUri:=Me.DatabaseRootUri, resourceType:=ResourceType.Database, operationType:=OperationType.Create, requestOptions:=requestOptions, cosmosContainerCore:=Nothing, feedRange:=Nothing, streamPayload:=Me.ClientContext.SerializerCore.ToStream(Of DatabaseProperties)(databaseProperties), requestEnricher:=Function(httpRequestMessage) httpRequestMessage.AddThroughputPropertiesHeader(throughputProperties), trace, cancellationToken:=cancellationToken)
'        '            Return Me.ClientContext.ResponseFactory.CreateDatabaseResponse(Me.GetDatabase(databaseProperties.Id), response)
'        '        End Function

'        '        Private Function CreateDatabaseStreamInternalAsync(ByVal databaseProperties As DatabaseProperties, ByVal throughputProperties As ThroughputProperties, ByVal requestOptions As RequestOptions, ByVal trace As ITrace, ByVal cancellationToken As System.Threading.CancellationToken) As System.Threading.Tasks.Task(Of ResponseMessage)
'        '            Return Me.ClientContext.ProcessResourceOperationAsync(resourceUri:=Me.DatabaseRootUri, resourceType:=ResourceType.Database, operationType:=OperationType.Create, requestOptions:=requestOptions, containerInternal:=Nothing, feedRange:=Nothing, streamPayload:=Me.ClientContext.SerializerCore.ToStream(Of DatabaseProperties)(databaseProperties), requestEnricher:=Function(httpRequestMessage) httpRequestMessage.AddThroughputPropertiesHeader(throughputProperties), responseCreator:=Function(response) response, trace:=trace, cancellationToken:=cancellationToken)
'        '        End Function

'        '        Private Function GetDatabaseQueryIteratorHelper(Of T)(ByVal queryDefinition As QueryDefinition, ByVal Optional continuationToken As String = Nothing, ByVal Optional requestOptions As QueryRequestOptions = Nothing) As FeedIteratorInternal(Of T)
'        '            Dim databaseStreamIterator As FeedIteratorInternal = Nothing

'        '            If Not (CSharpImpl.__Assign(databaseStreamIterator, TryCast(Me.GetDatabaseQueryStreamIteratorHelper(queryDefinition, continuationToken, requestOptions), FeedIteratorInternal)) IsNot Nothing) Then
'        '                Throw New System.InvalidOperationException($"Expected a FeedIteratorInternal.")
'        '            End If

'        '            Return New FeedIteratorCore(Of T)(databaseStreamIterator, Function(response) Me.ClientContext.ResponseFactory.CreateQueryFeedResponse(Of T)(responseMessage:=response, resourceType:=ResourceType.Database))
'        '        End Function

'        '        Private Function GetDatabaseQueryStreamIteratorHelper(ByVal queryDefinition As QueryDefinition, ByVal Optional continuationToken As String = Nothing, ByVal Optional requestOptions As QueryRequestOptions = Nothing) As FeedIteratorInternal
'        '            Return New FeedIteratorCore(clientContext:=Me.ClientContext, resourceLink:=Me.DatabaseRootUri, resourceType:=ResourceType.Database, queryDefinition:=queryDefinition, continuationToken:=continuationToken, options:=requestOptions)
'        '        End Function

'        '        Private Function InitializeContainersAsync(ByVal containers As System.Collections.Generic.IReadOnlyList(Of (String, String)), ByVal cancellationToken As System.Threading.CancellationToken) As System.Threading.Tasks.Task
'        '            Dim databaseId As String = Nothing, containerId As String = Nothing

'        '            Try
'        '                Dim tasks As System.Collections.Generic.List(Of System.Threading.Tasks.Task) = New System.Collections.Generic.List(Of System.Threading.Tasks.Task)()

'        '                For Each (databaseId, containerId) In containers
'        '                    tasks.Add(Me.InitializeContainerAsync(databaseId, containerId, cancellationToken))
'        '                Next

'        '                Return System.Threading.Tasks.Task.WhenAll(tasks)
'        '            Catch
'        '                Me.Dispose()
'        '                Throw
'        '            End Try
'        '        End Function

'        Private Function IncrementNumberOfClientsCreated() As Integer
'            Return System.Threading.Interlocked.Increment(Microsoft.Azure.Cosmos.CosmosClient.numberOfClientsCreated)
'        End Function

'        '        Private Async Function InitializeContainerAsync(ByVal databaseId As String, ByVal containerId As String, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task
'        '            Dim container As ContainerInternal = CType(Me.GetContainer(databaseId, containerId), ContainerInternal)
'        '            Dim feedRanges As System.Collections.Generic.IReadOnlyList(Of FeedRange) = Await container.GetFeedRangesAsync(cancellationToken)
'        '            Dim tasks As System.Collections.Generic.List(Of System.Threading.Tasks.Task) = New System.Collections.Generic.List(Of System.Threading.Tasks.Task)()

'        '            For Each feedRange As FeedRange In feedRanges
'        '                tasks.Add(Microsoft.Azure.Cosmos.CosmosClient.InitializeFeedRangeAsync(container, feedRange, cancellationToken))
'        '            Next

'        '            Await System.Threading.Tasks.Task.WhenAll(tasks)
'        '        End Function

'        '        Private Shared Async Function InitializeFeedRangeAsync(ByVal container As ContainerInternal, ByVal feedRange As FeedRange, ByVal Optional cancellationToken As System.Threading.CancellationToken = DirectCast(Nothing, Global.System.Threading.CancellationToken)) As System.Threading.Tasks.Task
'        '            ' Do a dummy querry for each Partition Key Range to warm up the caches and connections
'        '            Dim guidToCheck As String = System.Guid.NewGuid().ToString()
'        '            Dim queryDefinition As QueryDefinition = New QueryDefinition($"select * from c where c.id = '{guidToCheck}'")
'        '            ''' Cannot convert UsingStatementSyntax, System.InvalidCastException: Unable to cast object of type 'Microsoft.CodeAnalysis.VisualBasic.Syntax.EmptyStatementSyntax' to type 'Microsoft.CodeAnalysis.VisualBasic.Syntax.ExpressionSyntax'.
'        '            '''    at ICSharpCode.CodeConverter.VB.CommonConversions.RemodelVariableDeclaration(VariableDeclarationSyntax declaration) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommonConversions.cs:line 478
'        '            '''    at ICSharpCode.CodeConverter.VB.MethodBodyExecutableStatementVisitor.VisitUsingStatement(UsingStatementSyntax node) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\MethodBodyExecutableStatementVisitor.cs:line 494
'        '            '''    at Microsoft.CodeAnalysis.CSharp.CSharpSyntaxVisitor`1.Visit(SyntaxNode node)
'        '            '''    at ICSharpCode.CodeConverter.VB.CommentConvertingMethodBodyVisitor.DefaultVisit(SyntaxNode node) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommentConvertingMethodBodyVisitor.cs:line 24
'        '            ''' 
'        '            ''' Input:
'        '            '''             using (FeedIterator feedIterator = container.GetItemQueryStreamIterator(feedRange,
'        '            '''                                                                                     queryDefinition,
'        '            '''                                                                                     continuationToken: null,
'        '            '''                                                                                     requestOptions: new QueryRequestOptions() { }))
'        '            '''             {
'        '            '''                 while (feedIterator.HasMoreResults)
'        '            '''                 {
'        '            '''                     using ResponseMessage response = await feedIterator.ReadNextAsync(cancellationToken);
'        '            '''                     response.EnsureSuccessStatusCode();
'        '            '''                 }
'        '            '''             }
'        '            ''' 
'        '            ''' 
'        '        End Function

'        ''' <summary>
'        ''' Dispose of cosmos client
'        ''' </summary>
'        Public Sub Dispose() Implements Global.System.IDisposable.Dispose
'            Me.Dispose(True)
'        End Sub

'        ''' <summary>
'        ''' Dispose of cosmos client
'        ''' </summary>
'        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
'            If Not Me.isDisposed Then
'                _DisposedDateTimeUtc = System.DateTime.UtcNow

'                If disposing Then
'                    Me.ClientContext.Dispose()
'                End If

'                Me.isDisposed = True
'            End If
'        End Sub

'        '        Private Class CSharpImpl
'        '            <System.Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
'        '            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
'        '                target = value
'        '                Return value
'        '            End Function <System.Obsolete("Please refactor calling code to use normal throw statements")>
'        '            Shared Function __Throw(Of T)(ByVal e As System.Exception) As T
'        '                Throw e
'        '            End Function
'        '        End Class
'    End Class

'End Namespace

