''------------------------------------------------------------
'' Copyright (c) Microsoft Corporation.  All rights reserved.
''------------------------------------------------------------

'Imports System
'Imports System.Globalization
'Imports System.Threading.Tasks
'Imports Microsoft.Azure.Cosmos.Tracing
'Imports Microsoft.Azure.Documents
'Imports Microsoft.Azure.Documents.Collections

'Namespace Microsoft.Azure.Cosmos
'    Friend MustInherit Class AuthorizationTokenProvider
'        'Inherits ICosmosAuthorizationTokenProvider
'        'Implements IAuthorizationTokenProvider, IDisposable
'        Implements System.IDisposable


'        'Public Async Function AddSystemAuthorizationHeaderAsync(ByVal request As DocumentServiceRequest, ByVal federationId As String, ByVal verb As String, ByVal resourceId As String) As Task
'        '    request.Headers(HttpConstants.HttpHeaders.XDate) = Date.UtcNow.ToString("r", CultureInfo.InvariantCulture)
'        '    request.Headers(HttpConstants.HttpHeaders.Authorization) = (Await Me.GetUserAuthorizationAsync(If(resourceId, request.ResourceAddress), PathsHelper.GetResourcePath(request.ResourceType), verb, request.Headers, request.RequestAuthorizationTokenType)).token
'        'End Function

'        'Public MustOverride Function AddAuthorizationHeaderAsync(ByVal headersCollection As INameValueCollection, ByVal requestAddress As Uri, ByVal verb As String, ByVal tokenType As AuthorizationTokenType) As ValueTask
'        'Public MustOverride Function GetUserAuthorizationAsync(ByVal resourceAddress As String, ByVal resourceType As String, ByVal requestVerb As String, ByVal headers As INameValueCollection, ByVal tokenType As AuthorizationTokenType) As ValueTask(Of (String, String))
'        'Public MustOverride Function GetUserAuthorizationTokenAsync(ByVal resourceAddress As String, ByVal resourceType As String, ByVal requestVerb As String, ByVal headers As INameValueCollection, ByVal tokenType As AuthorizationTokenType, ByVal trace As ITrace) As ValueTask(Of String)
'        'Public MustOverride Sub TraceUnauthorized(ByVal dce As DocumentClientException, ByVal authorizationToken As String, ByVal payload As String)

'        Public Shared Function CreateWithResourceTokenOrAuthKey(ByVal authKeyOrResourceToken As String) As AuthorizationTokenProvider
'            If String.IsNullOrEmpty(authKeyOrResourceToken) Then
'                Throw New ArgumentNullException(NameOf(authKeyOrResourceToken))
'            End If

'            ' PKAR: w moim przypadku to Key, nie ma '&', więc będzie z AuthHelper ret false

'            'If AuthorizationHelper.IsResourceToken(authKeyOrResourceToken) Then
'            '    Return New AuthorizationTokenProviderResourceToken(authKeyOrResourceToken)
'            'Else
'            Return New AuthorizationTokenProviderMasterKey(authKeyOrResourceToken)
'            'End If
'        End Function

'        Public MustOverride Sub Dispose() Implements System.IDisposable.Dispose
'    End Class
'End Namespace
