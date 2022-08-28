''------------------------------------------------------------
'' Copyright (c) Microsoft Corporation.  All rights reserved.
''------------------------------------------------------------

''Imports System
''Imports System.Globalization
''Imports System.Net
''Imports System.Security
''Imports System.Text
''Imports System.Threading.Tasks
''Imports Microsoft.Azure.Cosmos.Core.Trace
''Imports Microsoft.Azure.Cosmos.Tracing
''Imports Microsoft.Azure.Documents
''Imports Microsoft.Azure.Documents.Collections

'Namespace Microsoft.Azure.Cosmos
'    Friend NotInheritable Class AuthorizationTokenProviderMasterKey
'        Inherits AuthorizationTokenProvider
'        '''The MAC signature found in the HTTP request is not the same as the computed signature.Server used following string to sign
'        '''The input authorization token can't serve the request. Please check that the expected payload is built as per the protocol, and check the key being used. Server used the following payload to sign
'        Private Const MacSignatureString As String = "to sign"
'        Private Const EnableAuthFailureTracesConfig As String = "enableAuthFailureTraces"
'        'Private ReadOnly enableAuthFailureTraces As Lazy(Of Boolean) nie trzeba Traces
'        Private ReadOnly authKeyHashFunction As IComputeHash
'        Private isDisposed As Boolean = False

'        Public Sub New(ByVal computeHash As IComputeHash)
'            If computeHash Is Nothing Then
'                Throw New ArgumentNullException(NameOf(computeHash))
'            End If
'            authKeyHashFunction = computeHash
'            Dim enableAuthFailureTracesFlag As Boolean = Nothing
'            '            enableAuthFailureTraces = New Lazy(Of Boolean)(Function()
'            '#If NETSTANDARD20 Then
'            '                // GetEntryAssembly returns null when loaded from native netstandard2.0
'            '                if (System.Reflection.Assembly.GetEntryAssembly() == null)
'            '                {
'            '                        return false;
'            '                }
'            '#End If
'            '                                                               Dim enableAuthFailureTracesString As String = Configuration.ConfigurationManager.AppSettings(EnableAuthFailureTracesConfig)
'            '                                                               Dim enableAuthFailureTracesFlag As Boolean = Nothing

'            '                                                               If String.IsNullOrEmpty(enableAuthFailureTracesString) OrElse Not Boolean.TryParse(enableAuthFailureTracesString, enableAuthFailureTracesFlag) Then
'            '                                                                   Return False
'            '                                                               End If

'            '                                                               Return enableAuthFailureTracesFlag
'            '                                                           End Function)
'        End Sub

'        'Public Sub New(ByVal authKey As SecureString)
'        '    Me.New(New SecureStringHMACSHA256Helper(authKey))
'        'End Sub

'        Public Sub New(ByVal authKey As String)
'            Me.New(New StringHMACSHA256Hash(authKey))
'        End Sub

'        'Public Overrides Function GetUserAuthorizationAsync(ByVal resourceAddress As String, ByVal resourceType As String, ByVal requestVerb As String, ByVal headers As INameValueCollection, ByVal tokenType As AuthorizationTokenType) As ValueTask(Of (String, String))
'        '    ' this is masterkey authZ
'        '    headers(HttpConstants.HttpHeaders.XDate) = Date.UtcNow.ToString("r", CultureInfo.InvariantCulture)
'        '    Dim arrayOwner As AuthorizationHelper.ArrayOwner = Nothing
'        '    Dim authorizationToken As String = AuthorizationHelper.GenerateKeyAuthorizationSignature(requestVerb, resourceAddress, resourceType, headers, authKeyHashFunction, arrayOwner)

'        '    Using arrayOwner
'        '        Dim payload As String = Nothing

'        '        If arrayOwner.Buffer.Count > 0 Then
'        '            payload = Encoding.UTF8.GetString(arrayOwner.Buffer.Array, arrayOwner.Buffer.Offset, CInt(arrayOwner.Buffer.Count))
'        '        End If

'        '        Return New ValueTask(Of (String, String))((authorizationToken, payload))
'        '    End Using
'        'End Function

'        'Public Overrides Function GetUserAuthorizationTokenAsync(ByVal resourceAddress As String, ByVal resourceType As String, ByVal requestVerb As String, ByVal headers As INameValueCollection, ByVal tokenType As AuthorizationTokenType, ByVal trace As ITrace) As ValueTask(Of String)
'        '    ' this is masterkey authZ
'        '    headers(HttpConstants.HttpHeaders.XDate) = Date.UtcNow.ToString("r", CultureInfo.InvariantCulture)
'        '    Dim arrayOwner As AuthorizationHelper.ArrayOwner = Nothing
'        '    Dim authorizationToken As String = AuthorizationHelper.GenerateKeyAuthorizationSignature(requestVerb, resourceAddress, resourceType, headers, authKeyHashFunction, arrayOwner)

'        '    Using arrayOwner
'        '        Return New ValueTask(Of String)(authorizationToken)
'        '    End Using
'        'End Function

'        'Public Overrides Function AddAuthorizationHeaderAsync(ByVal headersCollection As INameValueCollection, ByVal requestAddress As Uri, ByVal verb As String, ByVal tokenType As AuthorizationTokenType) As ValueTask
'        '    Dim dateTime = Date.UtcNow.ToString("r", CultureInfo.InvariantCulture)
'        '    headersCollection(HttpConstants.HttpHeaders.XDate) = dateTime
'        '    Dim token As String = AuthorizationHelper.GenerateKeyAuthorizationSignature(verb, requestAddress, headersCollection, authKeyHashFunction)
'        '    headersCollection.Add(HttpConstants.HttpHeaders.Authorization, token)
'        '    Return Nothing
'        'End Function

'        'Public Overrides Sub TraceUnauthorized(ByVal dce As DocumentClientException, ByVal authorizationToken As String, ByVal payload As String)
'        '    If Not Equals(payload, Nothing) AndAlso dce.Message IsNot Nothing AndAlso dce.StatusCode.HasValue AndAlso dce.StatusCode.Value = HttpStatusCode.Unauthorized AndAlso dce.Message.Contains(MacSignatureString) Then
'        '        ' The following code is added such that we get trace data on unexpected 401/HMAC errors and it is
'        '        '   disabled by default. The trace will be trigger only when "enableAuthFailureTraces" named configuration 
'        '        '   is set to true (currently true for CTL runs).
'        '        '   For production we will work directly with specific customers in order to enable this configuration.
'        '        Dim normalizedPayload = NormalizeAuthorizationPayload(payload)

'        '        If enableAuthFailureTraces.Value Then
'        '            Dim tokenFirst5 As String = HttpUtility.UrlDecode(authorizationToken).Split("&"c)(2).Split("="c)(1).Substring(0, 5)
'        '            Dim authHash As ULong = 0

'        '            If authKeyHashFunction?.Key IsNot Nothing Then
'        '                Dim bytes As Byte() = Encoding.UTF8.GetBytes(authKeyHashFunction?.Key?.ToString())
'        '                authHash = Documents.Routing.MurmurHash3.Hash64(bytes, bytes.Length)
'        '            End If

'        '            DefaultTrace.TraceError("Un-expected authorization payload mis-match. Actual payload={0}, token={1}..., hash={2:X}..., error={3}", normalizedPayload, tokenFirst5, authHash, dce.Message)
'        '        Else
'        '            DefaultTrace.TraceError("Un-expected authorization payload mis-match. Actual {0} service expected {1}", normalizedPayload, dce.Message)
'        '        End If
'        '    End If
'        'End Sub

'        Public Overrides Sub Dispose()
'            If Not isDisposed Then
'                authKeyHashFunction.Dispose()
'                isDisposed = True
'            End If
'        End Sub

'        'Private Shared Function NormalizeAuthorizationPayload(ByVal input As String) As String
'        '    Const expansionBuffer = 12
'        '    Dim builder As StringBuilder = New StringBuilder(input.Length + expansionBuffer)

'        '    For i = 0 To input.Length - 1

'        '        Select Case input(i)
'        '            Case ChrW(10)
'        '                builder.Append("\n")
'        '            Case "/"c
'        '                builder.Append("\/")
'        '            Case Else
'        '                builder.Append(input(i))
'        '        End Select
'        '    Next

'        '    Return builder.ToString()
'        'End Function

'        'Private Class CSharpImpl
'        '    <Obsolete("Please refactor calling code to use normal throw statements")>
'        '    Shared Function __Throw(Of T)(ByVal e As Exception) As T
'        '        Throw e
'        '    End Function
'        'End Class
'    End Class
'End Namespace
