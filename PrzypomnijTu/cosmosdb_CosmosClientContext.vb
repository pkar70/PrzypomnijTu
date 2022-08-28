'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------

'Imports System
'Imports System.IO
'Imports System.Threading
'Imports System.Threading.Tasks
'Imports Microsoft.Azure.Cosmos.Handlers
'Imports Microsoft.Azure.Cosmos.Tracing
'Imports Microsoft.Azure.Documents

Namespace Microsoft.Azure.Cosmos

    ''' <summary>
    ''' This class is used to get access to different client level operations without directly referencing the client object.
    ''' This makes it easy to pass a reference to the client, and it makes it easy to mock for unit tests.
    ''' </summary>
    Friend MustInherit Class CosmosClientContext
        Implements IDisposable
        '''' <summary>
        '''' The Cosmos client that is used for the request
        '''' </summary>
        'Friend MustOverride ReadOnly Property Client As CosmosClient
        'Friend MustOverride ReadOnly Property DocumentClient As DocumentClient
        'Friend MustOverride ReadOnly Property SerializerCore As CosmosSerializerCore
        'Friend MustOverride ReadOnly Property ResponseFactory As CosmosResponseFactoryInternal
        'Friend MustOverride ReadOnly Property RequestHandler As RequestInvokerHandler
        'Friend MustOverride ReadOnly Property ClientOptions As CosmosClientOptions
        'Friend MustOverride ReadOnly Property UserAgent As String
        'Friend MustOverride Function GetExecutorForContainer(ByVal container As ContainerInternal) As BatchAsyncContainerExecutor

        '''' <summary>
        '''' Generates the URI link for the resource
        '''' </summary>
        '''' <paramname="parentLink">The parent link URI (/dbs/mydbId) </param>
        '''' <paramname="uriPathSegment">The URI path segment</param>
        '''' <paramname="id">The id of the resource</param>
        '''' <returns>A resource link in the format of {parentLink}/this.UriPathSegment/this.Name with this.Name being a Uri escaped version</returns>
        'Friend MustOverride Function CreateLink(ByVal parentLink As String, ByVal uriPathSegment As String, ByVal id As String) As String
        'Friend MustOverride Sub ValidateResource(ByVal id As String)
        'Friend MustOverride Function GetCachedContainerPropertiesAsync(ByVal containerUri As String, ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of ContainerProperties)
        'Friend MustOverride Function OperationHelperAsync(Of TResult)(ByVal operationName As String, ByVal requestOptions As RequestOptions, ByVal task As Func(Of ITrace, Task(Of TResult)), ByVal Optional traceComponent As TraceComponent = TraceComponent.Transport, ByVal Optional traceLevel As TraceLevel = TraceLevel.Info) As Task(Of TResult)

        '''' <summary>
        '''' This is a wrapper around ExecUtil method. This allows the calls to be mocked so logic done 
        '''' in a resource can be unit tested.
        '''' </summary>
        'Friend MustOverride Function ProcessResourceOperationStreamAsync(ByVal resourceUri As String, ByVal resourceType As ResourceType, ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal cosmosContainerCore As ContainerInternal, ByVal partitionKey As PartitionKey?, ByVal itemId As String, ByVal streamPayload As Stream, ByVal requestEnricher As Action(Of RequestMessage), ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of ResponseMessage)

        '''' <summary>
        '''' This is a wrapper around ExecUtil method. This allows the calls to be mocked so logic done 
        '''' in a resource can be unit tested.
        '''' </summary>
        'Friend MustOverride Function ProcessResourceOperationStreamAsync(ByVal resourceUri As String, ByVal resourceType As ResourceType, ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal cosmosContainerCore As ContainerInternal, ByVal feedRange As FeedRange, ByVal streamPayload As Stream, ByVal requestEnricher As Action(Of RequestMessage), ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of ResponseMessage)

        '''' <summary>
        '''' This is a wrapper around request invoker method. This allows the calls to be mocked so logic done 
        '''' in a resource can be unit tested.
        '''' </summary>
        'Friend MustOverride Function ProcessResourceOperationAsync(Of T)(ByVal resourceUri As String, ByVal resourceType As ResourceType, ByVal operationType As OperationType, ByVal requestOptions As RequestOptions, ByVal containerInternal As ContainerInternal, ByVal feedRange As FeedRange, ByVal streamPayload As Stream, ByVal requestEnricher As Action(Of RequestMessage), ByVal responseCreator As Func(Of ResponseMessage, T), ByVal trace As ITrace, ByVal cancellationToken As CancellationToken) As Task(Of T)
        Public MustOverride Sub Dispose() Implements IDisposable.Dispose
    End Class
End Namespace
