'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------
'Imports System
'Imports System.Collections.Generic
'Imports System.Diagnostics
'Imports System.Linq
Imports System.Net
Imports System.Net.Http
'Imports System.Net.Http.Headers
'Imports System.Threading
'Imports System.Threading.Tasks
'Imports Microsoft.Azure.Cosmos.Resource.CosmosExceptions
'Imports Microsoft.Azure.Cosmos.Tracing
'Imports Microsoft.Azure.Cosmos.Tracing.TraceData
'Imports Microsoft.Azure.Documents
'Imports Microsoft.Azure.Documents.Collections

Namespace Microsoft.Azure.Cosmos
    Friend NotInheritable Class CosmosHttpClientCore
        '    Inherits CosmosHttpClient

        '    Private ReadOnly httpClient As HttpClient
        '    Private ReadOnly eventSource As ICommunicationEventSource
        '    Private disposedValue As Boolean

        '    Private Sub New(ByVal httpClient As HttpClient, ByVal httpMessageHandler As HttpMessageHandler, ByVal eventSource As ICommunicationEventSource)
        '        Me.httpClient = If(httpClient, CSharpImpl.__Throw(Of HttpClient)(New ArgumentNullException(NameOf(httpClient))))
        '        Me.eventSource = If(eventSource, CSharpImpl.__Throw(Of Object)(New ArgumentNullException(NameOf(eventSource))))
        '        Me.HttpMessageHandler = httpMessageHandler
        '    End Sub

        '    Public Overrides ReadOnly Property HttpMessageHandler As HttpMessageHandler

        '    Public Shared Function CreateWithConnectionPolicy(ByVal apiType As ApiType, ByVal eventSource As ICommunicationEventSource, ByVal connectionPolicy As ConnectionPolicy, ByVal httpMessageHandler As HttpMessageHandler, ByVal sendingRequestEventArgs As EventHandler(Of SendingRequestEventArgs), ByVal receivedResponseEventArgs As EventHandler(Of ReceivedResponseEventArgs)) As CosmosHttpClient
        '        If connectionPolicy Is Nothing Then
        '            Throw New ArgumentNullException(NameOf(connectionPolicy))
        '        End If

        '        Dim httpClientFactory As Func(Of HttpClient) = connectionPolicy.HttpClientFactory

        '        If httpClientFactory IsNot Nothing Then
        '            If sendingRequestEventArgs IsNot Nothing AndAlso receivedResponseEventArgs IsNot Nothing Then
        '                Throw New InvalidOperationException($"{NameOf(connectionPolicy.HttpClientFactory)} can not be set at the same time as {NameOf(sendingRequestEventArgs)} or {NameOf(receivedResponseEventArgs)}")
        '            End If

        '            Dim userHttpClient As HttpClient = If(httpClientFactory.Invoke(), CSharpImpl.__Throw(Of HttpClient)(New ArgumentNullException($"{NameOf(httpClientFactory)} returned null. {NameOf(httpClientFactory)} must return a HttpClient instance.")))
        '            Return CosmosHttpClientCore.CreateHelper(httpClient:=userHttpClient, httpMessageHandler:=httpMessageHandler, requestTimeout:=connectionPolicy.RequestTimeout, userAgentContainer:=connectionPolicy.UserAgentContainer, apiType:=apiType, eventSource:=eventSource)
        '        End If

        '        If httpMessageHandler Is Nothing Then
        '            httpMessageHandler = CosmosHttpClientCore.CreateHttpClientHandler(gatewayModeMaxConnectionLimit:=connectionPolicy.MaxConnectionLimit, webProxy:=Nothing)
        '        End If

        '        If sendingRequestEventArgs IsNot Nothing OrElse receivedResponseEventArgs IsNot Nothing Then
        '            httpMessageHandler = CreateHttpMessageHandler(httpMessageHandler, sendingRequestEventArgs, receivedResponseEventArgs)
        '        End If

        '        Dim httpClient As HttpClient = New HttpClient(httpMessageHandler)
        '        Return CosmosHttpClientCore.CreateHelper(httpClient:=httpClient, httpMessageHandler:=httpMessageHandler, requestTimeout:=connectionPolicy.RequestTimeout, userAgentContainer:=connectionPolicy.UserAgentContainer, apiType:=apiType, eventSource:=eventSource)
        '    End Function

        Public Shared Function CreateHttpClientHandler(ByVal gatewayModeMaxConnectionLimit As Integer, ByVal webProxy As IWebProxy) As HttpMessageHandler
            Dim httpClientHandler As HttpClientHandler = New HttpClientHandler()

            ' Proxy is only set by users and can cause not supported exception on some platforms
            If webProxy IsNot Nothing Then
                httpClientHandler.Proxy = webProxy
            End If

            ' https://docs.microsoft.com/en-us/archive/blogs/timomta/controlling-the-number-of-outgoing-connections-from-httpclient-net-core-or-full-framework
            Try
                httpClientHandler.MaxConnectionsPerServer = gatewayModeMaxConnectionLimit
                ' MaxConnectionsPerServer is not supported on some platforms.
            Catch __unusedPlatformNotSupportedException1__ As PlatformNotSupportedException
            End Try

            Return httpClientHandler
        End Function

        '    Private Shared Function CreateHttpMessageHandler(ByVal innerHandler As HttpMessageHandler, ByVal sendingRequestEventArgs As EventHandler(Of SendingRequestEventArgs), ByVal receivedResponseEventArgs As EventHandler(Of ReceivedResponseEventArgs)) As HttpMessageHandler
        '        Return New HttpRequestMessageHandler(sendingRequestEventArgs, receivedResponseEventArgs, innerHandler)
        '    End Function

        '    Private Shared Function CreateHelper(ByVal httpClient As HttpClient, ByVal httpMessageHandler As HttpMessageHandler, ByVal requestTimeout As TimeSpan, ByVal userAgentContainer As UserAgentContainer, ByVal apiType As ApiType, ByVal eventSource As ICommunicationEventSource) As CosmosHttpClient
        '        If httpClient Is Nothing Then
        '            Throw New ArgumentNullException(NameOf(httpClient))
        '        End If

        '        httpClient.Timeout = If(requestTimeout > CosmosHttpClientCore.GatewayRequestTimeout, requestTimeout, CosmosHttpClientCore.GatewayRequestTimeout)
        '        httpClient.DefaultRequestHeaders.CacheControl = New CacheControlHeaderValue With {
        '            .NoCache = True
        '        }
        '        httpClient.AddUserAgentHeader(userAgentContainer)
        '        httpClient.AddApiTypeHeader(apiType)

        '        ' Set requested API version header that can be used for
        '        ' version enforcement.
        '        httpClient.DefaultRequestHeaders.Add(HttpConstants.HttpHeaders.Version, HttpConstants.Versions.CurrentVersion)
        '        httpClient.DefaultRequestHeaders.Add(HttpConstants.HttpHeaders.Accept, RuntimeConstants.MediaTypes.Json)
        '        Return New CosmosHttpClientCore(httpClient, httpMessageHandler, eventSource)
        '    End Function

        '    Public Overrides Function GetAsync(ByVal uri As Uri, ByVal additionalHeaders As INameValueCollection, ByVal resourceType As ResourceType, ByVal timeoutPolicy As HttpTimeoutPolicy, ByVal clientSideRequestStatistics As IClientSideRequestStatistics, ByVal cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        '        If uri Is Nothing Then
        '            Throw New ArgumentNullException(NameOf(uri))

        '            ' GetAsync doesn't let clients to pass in additional headers. So, we are
        '            ' internally using SendAsync and add the additional headers to requestMessage. 
        '        End If

        '        ''' Cannot convert LocalFunctionStatementSyntax, CONVERSION ERROR: Conversion for LocalFunctionStatement not implemented, please report this issue in 'System.Threading.Tasks.Valu...' at character 8480
        '        ''' 
        '        ''' 
        '        ''' Input:
        '        ''' 
        '        '''             // GetAsync doesn't let clients to pass in additional headers. So, we are
        '        '''             // internally using SendAsync and add the additional headers to requestMessage. 
        '        '''             System.Threading.Tasks.ValueTask<System.Net.Http.HttpRequestMessage> CreateRequestMessage()
        '        '''             {
        '        '''                 System.Net.Http.HttpRequestMessage requestMessage = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, uri);
        '        '''                 if (additionalHeaders != null)
        '        '''                 {
        '        '''                     foreach (string header in additionalHeaders)
        '        '''                     {
        '        '''                         if (GatewayStoreClient.IsAllowedRequestHeader(header))
        '        '''                         {
        '        '''                             requestMessage.Headers.TryAddWithoutValidation(header, additionalHeaders[header]);
        '        '''                         }
        '        '''                     }
        '        '''                 }
        '        ''' 
        '        '''                 return new System.Threading.Tasks.ValueTask<System.Net.Http.HttpRequestMessage>(requestMessage);
        '        '''             }
        '        ''' 
        '        ''' 
        '        Return Me.SendHttpAsync(New Func(Of ValueTask(Of HttpRequestMessage))(AddressOf CreateRequestMessage), resourceType, timeoutPolicy, clientSideRequestStatistics, cancellationToken)
        '    End Function

        '    Public Overrides Function SendHttpAsync(ByVal createRequestMessageAsync As Func(Of ValueTask(Of HttpRequestMessage)), ByVal resourceType As ResourceType, ByVal timeoutPolicy As HttpTimeoutPolicy, ByVal clientSideRequestStatistics As IClientSideRequestStatistics, ByVal cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        '        If createRequestMessageAsync Is Nothing Then
        '            Throw New ArgumentNullException(NameOf(createRequestMessageAsync))
        '        End If

        '        Return Me.SendHttpHelperAsync(createRequestMessageAsync, resourceType, timeoutPolicy, clientSideRequestStatistics, cancellationToken)
        '    End Function

        '    Private Async Function SendHttpHelperAsync(ByVal createRequestMessageAsync As Func(Of ValueTask(Of HttpRequestMessage)), ByVal resourceType As ResourceType, ByVal timeoutPolicy As HttpTimeoutPolicy, ByVal clientSideRequestStatistics As IClientSideRequestStatistics, ByVal cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        '        Dim startDateTimeUtc = Date.UtcNow
        '        Dim timeoutEnumerator As IEnumerator(Of (TimeSpan, TimeSpan)) = timeoutPolicy.GetTimeoutEnumerator()
        '        timeoutEnumerator.MoveNext()
        '        Dim requestTimeout As TimeSpan = Nothing, delayForNextRequest As TimeSpan = Nothing, datum As ClientSideRequestStatisticsTraceDatum = Nothing, datum As ClientSideRequestStatisticsTraceDatum = Nothing

        '        While True
        '            cancellationToken.ThrowIfCancellationRequested()
        '            (requestTimeout, delayForNextRequest) = timeoutEnumerator.Current

        '            Using requestMessage As HttpRequestMessage = Await createRequestMessageAsync()
        '                ' If the default cancellation token is passed then use the timeout policy
        '                Dim cancellationTokenSource = cancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
        '                cancellationTokenSource.CancelAfter(requestTimeout)
        '                Dim requestStartTime = Date.UtcNow

        '                Try
        '                    Dim responseMessage = Await Me.ExecuteHttpHelperAsync(requestMessage, resourceType, cancellationTokenSource.Token)

        '                    If CSharpImpl.__Assign(datum, TryCast(clientSideRequestStatistics, ClientSideRequestStatisticsTraceDatum)) IsNot Nothing Then
        '                        datum.RecordHttpResponse(requestMessage, responseMessage, resourceType, requestStartTime)
        '                    End If

        '                    Return responseMessage
        '                Catch e As Exception

        '                    If CSharpImpl.__Assign(datum, TryCast(clientSideRequestStatistics, ClientSideRequestStatisticsTraceDatum)) IsNot Nothing Then
        '                        datum.RecordHttpException(requestMessage, e, resourceType, requestStartTime)
        '                    End If
        '                    ' Throw if the user passed in cancellation was requested

        '                    ' Convert OperationCanceledException to 408 when the HTTP client throws it. This makes it clear that the 
        '                    ' the request timed out and was not user canceled operation.
        '                    ' throw current exception (caught in transport handler)
        '                    Dim isOutOfRetries As Boolean = Date.UtcNow - startDateTimeUtc > timeoutPolicy.MaximumRetryTimeLimit OrElse Not timeoutEnumerator.MoveNext() ' Maximum of time for all retries
        '                    ' No more retries are configured
        '                    ''' Cannot convert SwitchStatementSyntax, System.InvalidCastException: Unable to cast object of type 'Microsoft.CodeAnalysis.VisualBasic.Syntax.EmptyStatementSyntax' to type 'Microsoft.CodeAnalysis.VisualBasic.Syntax.CaseClauseSyntax'.
        '                    '''    at System.Linq.Enumerable.CastIterator[TResult](IEnumerable source)+MoveNext()
        '                    '''    at Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.SeparatedList[TNode](IEnumerable`1 nodes)
        '                    '''    at ICSharpCode.CodeConverter.VB.MethodBodyExecutableStatementVisitor.ConvertSwitchSection(SwitchSectionSyntax section) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\MethodBodyExecutableStatementVisitor.cs:line 232
        '                    '''    at System.Linq.Enumerable.SelectIPartitionIterator`2.LazyToArray()
        '                    '''    at System.Linq.Enumerable.SelectIPartitionIterator`2.ToArray()
        '                    '''    at ICSharpCode.CodeConverter.VB.MethodBodyExecutableStatementVisitor.VisitSwitchStatement(SwitchStatementSyntax node) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\MethodBodyExecutableStatementVisitor.cs:line 205
        '                    '''    at Microsoft.CodeAnalysis.CSharp.CSharpSyntaxVisitor`1.Visit(SyntaxNode node)
        '                    '''    at ICSharpCode.CodeConverter.VB.CommentConvertingMethodBodyVisitor.DefaultVisit(SyntaxNode node) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommentConvertingMethodBodyVisitor.cs:line 24
        '                    ''' 
        '                    ''' Input:
        '                    ''' 
        '                    '''                         switch (e)
        '                    '''                         {
        '                    '''                             case System.OperationCanceledException operationCanceledException:
        '                    '''                                 // Throw if the user passed in cancellation was requested
        '                    '''                                 if (cancellationToken.IsCancellationRequested)
        '                    '''                                 {
        '                    '''                                     throw;
        '                    '''                                 }
        '                    ''' 
        '                    '''                                 // Convert OperationCanceledException to 408 when the HTTP client throws it. This makes it clear that the 
        '                    '''                                 // the request timed out and was not user canceled operation.
        '                    '''                                 if (isOutOfRetries || !timeoutPolicy.IsSafeToRetry(requestMessage.Method))
        '                    '''                                 {
        '                    '''                                     // throw current exception (caught in transport handler)
        '                    '''                                     string message =
        '                    '''                                             $"GatewayStoreClient Request Timeout. Start Time UTC:{startDateTimeUtc}; Total Duration:{(System.DateTime.UtcNow - startDateTimeUtc).TotalMilliseconds} Ms; Request Timeout {requestTimeout.TotalMilliseconds} Ms; Http Client Timeout:{this.httpClient.Timeout.TotalMilliseconds} Ms; Activity id: {System.Diagnostics.Trace.CorrelationManager.ActivityId};";
        '                    '''                                     e.Data.Add("Message", message);
        '                    '''                                     throw;
        '                    '''                                 }
        '                    ''' 
        '                    '''                                 break;
        '                    '''                             case WebException webException:
        '                    '''                                 if (isOutOfRetries || (!timeoutPolicy.IsSafeToRetry(requestMessage.Method) && !WebExceptionUtility.IsWebExceptionRetriable(webException)))
        '                    '''                                 {
        '                    '''                                     throw;
        '                    '''                                 }
        '                    ''' 
        '                    '''                                 break;
        '                    '''                             case System.Net.Http.HttpRequestException httpRequestException:
        '                    '''                                 if (isOutOfRetries || !timeoutPolicy.IsSafeToRetry(requestMessage.Method))
        '                    '''                                 {
        '                    '''                                     throw;
        '                    '''                                 }
        '                    ''' 
        '                    '''                                 break;
        '                    '''                             default:
        '                    '''                                 throw;
        '                    '''                         }
        '                    ''' 
        '                    ''' 
        '                End Try
        '            End Using

        '            If delayForNextRequest <> TimeSpan.Zero Then
        '                Await Task.Delay(delayForNextRequest)
        '            End If
        '        End While
        '    End Function

        '    Private Async Function ExecuteHttpHelperAsync(ByVal requestMessage As HttpRequestMessage, ByVal resourceType As ResourceType, ByVal cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        '        Dim sendTimeUtc = Date.UtcNow
        '        Dim localGuid As Guid = Guid.NewGuid() ' For correlating HttpRequest and HttpResponse Traces
        '        Dim requestedActivityId = Trace.CorrelationManager.ActivityId
        '        eventSource.Request(requestedActivityId, localGuid, requestMessage.RequestUri.ToString(), resourceType.ToResourceTypeString(), requestMessage.Headers)

        '        ' Only read the header initially. The content gets copied into a memory stream later
        '        ' if we read the content HTTP client will buffer the message and then it will get buffered
        '        ' again when it is copied to the memory stream.
        '        Dim responseMessage = Await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)

        '        ' WebAssembly HttpClient does not set the RequestMessage property on SendAsync
        '        If responseMessage.RequestMessage Is Nothing Then
        '            responseMessage.RequestMessage = requestMessage
        '        End If

        '        Dim receivedTimeUtc = Date.UtcNow
        '        Dim durationTimeSpan = receivedTimeUtc - sendTimeUtc
        '        Dim activityId = Guid.Empty
        '        Dim headerValues As IEnumerable(Of String) = Nothing

        '        If responseMessage.Headers.TryGetValues(HttpConstants.HttpHeaders.ActivityId, headerValues) AndAlso headerValues.Any() Then
        '            activityId = New Guid(headerValues.First())
        '        End If

        '        eventSource.Response(activityId, localGuid, responseMessage.StatusCode, durationTimeSpan.TotalMilliseconds, responseMessage.Headers)
        '        Return responseMessage
        '    End Function

        '    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        '        If Not disposedValue Then
        '            If disposing Then
        '                httpClient.Dispose()
        '            End If

        '            disposedValue = True
        '        End If
        '    End Sub

        '    Public Overloads Overrides Sub Dispose()
        '        Dispose(True)
        '    End Sub

        '    Private Class HttpRequestMessageHandler
        '        Inherits DelegatingHandler

        '        Private ReadOnly sendingRequest As EventHandler(Of SendingRequestEventArgs)
        '        Private ReadOnly receivedResponse As EventHandler(Of ReceivedResponseEventArgs)

        '        Public Sub New(ByVal sendingRequest As EventHandler(Of SendingRequestEventArgs), ByVal receivedResponse As EventHandler(Of ReceivedResponseEventArgs), ByVal innerHandler As HttpMessageHandler)
        '            Me.sendingRequest = sendingRequest
        '            Me.receivedResponse = receivedResponse
        '            Me.InnerHandler = If(innerHandler, CSharpImpl.__Throw(Of HttpMessageHandler)(New ArgumentNullException($"innerHandler is null. This required for .NET core to limit the http connection. See {NameOf(CreateHttpClientHandler)} ")))
        '        End Sub

        '        Protected Overrides Async Function SendAsync(ByVal request As HttpRequestMessage, ByVal cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        '            sendingRequest?.Invoke(Me, New SendingRequestEventArgs(request))
        '            Dim response = Await MyBase.SendAsync(request, cancellationToken)
        '            receivedResponse?.Invoke(Me, New ReceivedResponseEventArgs(request, response))
        '            Return response
        '        End Function

        '        Private Class CSharpImpl
        '            <Obsolete("Please refactor calling code to use normal throw statements")>
        '            Shared Function __Throw(Of T)(ByVal e As Exception) As T
        '                Throw e
        '            End Function
        '        End Class
        '    End Class

        '    Private Class CSharpImpl
        '        <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
        '        Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
        '            target = value
        '            Return value
        '        End Function <Obsolete("Please refactor calling code to use normal throw statements")>
        '        Shared Function __Throw(Of T)(ByVal e As Exception) As T
        '            Throw e
        '        End Function
        '    End Class
    End Class
End Namespace
