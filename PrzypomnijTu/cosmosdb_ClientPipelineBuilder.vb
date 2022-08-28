'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------

'Imports System
'Imports System.Collections.Generic
'Imports System.Diagnostics
'Imports System.Linq
'Imports Microsoft.Azure.Cosmos.Handlers
'Imports Microsoft.Azure.Cosmos.Telemetry

Namespace Microsoft.Azure.Cosmos
    Friend Class ClientPipelineBuilder
        '        Private ReadOnly client As CosmosClient
        '        Private ReadOnly requestedClientConsistencyLevel As ConsistencyLevel?
        '        Private ReadOnly diagnosticsHandler As DiagnosticsHandler
        '        Private ReadOnly invalidPartitionExceptionRetryHandler As RequestHandler
        '        Private ReadOnly transportHandler As RequestHandler
        '        Private ReadOnly telemetryHandler As TelemetryHandler
        '        Private customHandlersField As IReadOnlyCollection(Of RequestHandler)
        '        Private retryHandler As RequestHandler

        '        Public Sub New(ByVal client As CosmosClient, ByVal requestedClientConsistencyLevel As ConsistencyLevel?, ByVal customHandlers As IReadOnlyCollection(Of RequestHandler), ByVal telemetry As ClientTelemetry)
        '            Me.client = If(client, CSharpImpl.__Throw(Of Object)(New ArgumentNullException(NameOf(client))))
        '            Me.requestedClientConsistencyLevel = requestedClientConsistencyLevel
        '            transportHandler = New TransportHandler(client)
        '            Debug.Assert(transportHandler.InnerHandler Is Nothing, NameOf(transportHandler))
        '            invalidPartitionExceptionRetryHandler = New NamedCacheRetryHandler()
        '            Debug.Assert(invalidPartitionExceptionRetryHandler.InnerHandler Is Nothing, "The invalidPartitionExceptionRetryHandler.InnerHandler must be null to allow other handlers to be linked.")
        '            PartitionKeyRangeHandler = New PartitionKeyRangeHandler(client)
        '            Debug.Assert(PartitionKeyRangeHandler.InnerHandler Is Nothing, "The PartitionKeyRangeHandler.InnerHandler must be null to allow other handlers to be linked.")

        '            ' Disable system usage for internal builds. Cosmos DB owns the VMs and already logs
        '            ' the system information so no need to track it.
        '#If Not INTERNAL Then
        '            diagnosticsHandler = New DiagnosticsHandler()
        '            Debug.Assert(diagnosticsHandler.InnerHandler Is Nothing, NameOf(diagnosticsHandler))

        '            If telemetry IsNot Nothing Then
        '                telemetryHandler = New TelemetryHandler(telemetry)
        '                Debug.Assert(telemetryHandler.InnerHandler Is Nothing, NameOf(telemetryHandler))
        '            End If
        '#Else
        '            this.diagnosticsHandler = null;
        '            this.telemetryHandler = null;
        '#End If

        '            UseRetryPolicy()
        '            AddCustomHandlers(customHandlers)
        '        End Sub

        '        Friend Property CustomHandlers As IReadOnlyCollection(Of RequestHandler)
        '            Get
        '                Return customHandlersField
        '            End Get
        '            Private Set(ByVal value As IReadOnlyCollection(Of RequestHandler))

        '                If value IsNot Nothing AndAlso value.Any(Function(x) x?.InnerHandler IsNot Nothing) Then
        '                    Throw New ArgumentOutOfRangeException(NameOf(Me.CustomHandlers))
        '                End If

        '                customHandlersField = value
        '            End Set
        '        End Property

        '        Friend Property PartitionKeyRangeHandler As RequestHandler

        '        ''' <summary>
        '        ''' This is the cosmos pipeline logic for the operations. 
        '        ''' 
        '        '''                                    +-----------------------------+
        '        '''                                    |                             |
        '        '''                                    |    RequestInvokerHandler    |
        '        '''                                    |                             |
        '        '''                                    +-----------------------------+
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                    +-----------------------------+
        '        '''                                    |                             |
        '        '''                                    |       UserHandlers          |
        '        '''                                    |                             |
        '        '''                                    +-----------------------------+
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                    +-----------------------------+
        '        '''                                    |                             |
        '        '''                                    |       DiagnosticHandler     |
        '        '''                                    |                             |
        '        '''                                    +-----------------------------+
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                    +-----------------------------+
        '        '''                                    |                             |
        '        '''                                    |       TelemetryHandler      |-> Trigger a thread to monitor system usage/operation information and sends to an API
        '        '''                                    |                             |
        '        '''                                    +-----------------------------+
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                    +-----------------------------+
        '        '''                                    |                             |
        '        '''                                    |       RetryHandler          |-> RetryPolicy -> ResetSessionTokenRetryPolicyFactory -> ClientRetryPolicy -> ResourceThrottleRetryPolicy
        '        '''                                    |                             |
        '        '''                                    +-----------------------------+
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                                 |
        '        '''                                    +-----------------------------+
        '        '''                                    |                             |
        '        '''                                    |       RouteHandler          | 
        '        '''                                    |                             |
        '        '''                                    +-----------------------------+
        '        '''                                    |                             |
        '        '''                                    |                             |
        '        '''                                    |                             |
        '        '''                  +-----------------------------+         +---------------------------------------+
        '        '''                  | !IsPartitionedFeedOperation |         |    IsPartitionedFeedOperation         |
        '        '''                  |      TransportHandler       |         | invalidPartitionExceptionRetryHandler |
        '        '''                  |                             |         |                                       |
        '        '''                  +-----------------------------+         +---------------------------------------+
        '        '''                                                                          |
        '        '''                                                                          |
        '        '''                                                                          |
        '        '''                                                          +---------------------------------------+
        '        '''                                                          |                                       |
        '        '''                                                          |     PartitionKeyRangeHandler          |
        '        '''                                                          |                                       |
        '        '''                                                          +---------------------------------------+
        '        '''                                                                          |
        '        '''                                                                          |
        '        '''                                                                          |
        '        '''                                                          +---------------------------------------+
        '        '''                                                          |                                       |
        '        '''                                                          |         TransportHandler              |
        '        '''                                                          |                                       |
        '        '''                                                          +---------------------------------------+
        '        ''' </summary>
        '        ''' <returns>The request invoker handler used to do calls to Cosmos DB</returns>
        '        Public Function Build() As RequestInvokerHandler
        '            Dim root As RequestInvokerHandler = New RequestInvokerHandler(client, requestedClientConsistencyLevel)
        '            Dim current As RequestHandler = root

        '            If CustomHandlers IsNot Nothing AndAlso CustomHandlers.Any() Then
        '                For Each handler As RequestHandler In CustomHandlers
        '                    current.InnerHandler = handler
        '                    current = current.InnerHandler
        '                Next
        '            End If

        '            ' Public SDK should always have the diagnostics handler
        '#If Not INTERNAL Then
        '            Debug.Assert(diagnosticsHandler IsNot Nothing, NameOf(diagnosticsHandler))
        '#End If
        '            If diagnosticsHandler IsNot Nothing Then
        '                current.InnerHandler = diagnosticsHandler
        '                current = current.InnerHandler
        '            End If

        '            If telemetryHandler IsNot Nothing Then
        '                current.InnerHandler = telemetryHandler
        '                current = current.InnerHandler
        '            End If

        '            Debug.Assert(retryHandler IsNot Nothing, NameOf(retryHandler))
        '            current.InnerHandler = retryHandler
        '            current = current.InnerHandler

        '            ' Have a router handler
        '            Dim feedHandler As RequestHandler = CreateDocumentFeedPipeline()
        '            Debug.Assert(feedHandler IsNot Nothing, NameOf(feedHandler))
        '            Debug.Assert(transportHandler.InnerHandler Is Nothing, NameOf(transportHandler))
        '            Dim routerHandler As RequestHandler = New RouterHandler(documentFeedHandler:=feedHandler, pointOperationHandler:=transportHandler)
        '            current.InnerHandler = routerHandler
        '            current = current.InnerHandler
        '            Return root
        '        End Function

        '        Friend Shared Function CreatePipeline(ParamArray requestHandlers As RequestHandler()) As RequestHandler
        '            Dim head As RequestHandler = Nothing
        '            Dim handlerCount = requestHandlers.Length

        '            For i = handlerCount - 1 To 0 Step -1
        '                Dim indexHandler As RequestHandler = requestHandlers(i)

        '                If indexHandler.InnerHandler IsNot Nothing Then
        '                    Throw New ArgumentOutOfRangeException($"The requestHandlers[{i}].InnerHandler is required to be null to allow the pipeline to chain the handlers.")
        '                End If

        '                If head IsNot Nothing Then
        '                    indexHandler.InnerHandler = head
        '                End If

        '                head = indexHandler
        '            Next

        '            Return head
        '        End Function

        '        Private Function UseRetryPolicy() As ClientPipelineBuilder
        '            retryHandler = New RetryHandler(client)
        '            Debug.Assert(retryHandler.InnerHandler Is Nothing, "The retryHandler.InnerHandler must be null to allow other handlers to be linked.")
        '            Return Me
        '        End Function

        '        Private Function AddCustomHandlers(ByVal customHandlers As IReadOnlyCollection(Of RequestHandler)) As ClientPipelineBuilder
        '            Me.CustomHandlers = customHandlers
        '            Return Me
        '        End Function

        '        Private Function CreateDocumentFeedPipeline() As RequestHandler
        '            Dim feedPipeline = New RequestHandler() {invalidPartitionExceptionRetryHandler, PartitionKeyRangeHandler, transportHandler}
        '            Return CreatePipeline(feedPipeline)
        '        End Function

        '        Private Class CSharpImpl
        '            <Obsolete("Please refactor calling code to use normal throw statements")>
        '            Shared Function __Throw(Of T)(ByVal e As Exception) As T
        '                Throw e
        '            End Function
        '        End Class
    End Class
End Namespace
