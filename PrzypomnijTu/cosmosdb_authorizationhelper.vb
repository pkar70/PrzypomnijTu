'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------
'Imports System
'Imports System.Buffers
'Imports System.Collections.Generic
'Imports System.Diagnostics.CodeAnalysis
'Imports System.Globalization
'Imports System.Security.Cryptography
'Imports System.Text
'Imports Microsoft.Azure.Cosmos.Core.Trace
'Imports Microsoft.Azure.Documents
'Imports Microsoft.Azure.Documents.Collections
'Imports System.Runtime.InteropServices
'Imports System.Runtime.CompilerServices

Namespace Microsoft.Azure.Cosmos

    ' This class is used by both client (for generating the auth header with master/system key) and 
    ' by the G/W when verifying the auth header. Some additional logic is also used by management service.
    Friend Module AuthorizationHelper
        'Public Const MaxAuthorizationHeaderSize As Integer = 1024
        'Public Const DefaultAllowedClockSkewInSeconds As Integer = 900
        'Public Const DefaultMasterTokenExpiryInSeconds As Integer = 900
        'Private Const MaxAadAuthorizationHeaderSize As Integer = 16 * 1024
        'Private Const MaxResourceTokenAuthorizationHeaderSize As Integer = 8 * 1024
        'Private ReadOnly AuthorizationFormatPrefixUrlEncoded As String = HttpUtility.UrlEncode(String.Format(CultureInfo.InvariantCulture, Constants.Properties.AuthorizationFormat, Constants.Properties.MasterToken, Constants.Properties.TokenVersion, String.Empty))
        'Private ReadOnly AuthorizationEncoding As Encoding = New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        '' This API is a helper method to create auth header based on client request.
        '' Uri is split into resourceType/resourceId - 
        '' For feed/post/put requests, resourceId = parentId,
        '' For point get requests,     resourceId = last segment in URI
        'Public Function GenerateGatewayAuthSignatureWithAddressResolution(ByVal verb As String, ByVal uri As Uri, ByVal headers As INameValueCollection, ByVal stringHMACSHA256Helper As IComputeHash, ByVal Optional clientVersion As String = "") As String
        '    If uri Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(uri))
        '    End If

        '    ' Address request has the URI fragment (dbs/dbid/colls/colId...) as part of
        '    ' either $resolveFor 'or' $generate queries of the context.RequestUri.
        '    ' Extracting out the URI in the form https://localhost/dbs/dbid/colls/colId/docs to generate the signature.
        '    ' Authorizer uses the same URI to verify signature.
        '    If uri.AbsolutePath.Equals(Paths.Address_Root, StringComparison.OrdinalIgnoreCase) Then
        '        uri = GenerateUriFromAddressRequestUri(uri)
        '    End If

        '    Return AuthorizationHelper.GenerateKeyAuthorizationSignature(verb, uri, headers, stringHMACSHA256Helper, clientVersion)
        'End Function

        '' This API is a helper method to create auth header based on client request.
        '' Uri is split into resourceType/resourceId - 
        '' For feed/post/put requests, resourceId = parentId,
        '' For point get requests,     resourceId = last segment in URI
        'Public Function GenerateKeyAuthorizationSignature(ByVal verb As String, ByVal uri As Uri, ByVal headers As INameValueCollection, ByVal stringHMACSHA256Helper As IComputeHash, ByVal Optional clientVersion As String = "") As String
        '    If String.IsNullOrEmpty(verb) Then
        '        Throw New ArgumentException(RMResources.StringArgumentNullOrEmpty, NameOf(verb))
        '    End If

        '    If uri Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(uri))
        '    End If

        '    If stringHMACSHA256Helper Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(stringHMACSHA256Helper))
        '    End If

        '    If headers Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(headers))
        '    End If

        '    Dim resourceType As String = Nothing, resourceIdValue As String = Nothing
        '    AuthorizationHelper.GetResourceTypeAndIdOrFullName(uri, __, resourceType, resourceIdValue, clientVersion)
        '    Dim arrayOwner As ArrayOwner = Nothing
        '    Dim authorizationToken = AuthorizationHelper.GenerateKeyAuthorizationSignature(verb, resourceIdValue, resourceType, headers, stringHMACSHA256Helper, arrayOwner)

        '    Using arrayOwner
        '        Return authorizationToken
        '    End Using
        'End Function

        '' This is a helper for both system and master keys
        '<SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification:="HTTP Headers are ASCII")>
        'Public Function GenerateKeyAuthorizationSignature(ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal key As String, ByVal Optional bUseUtcNowForMissingXDate As Boolean = False) As String
        '    Dim authorizationToken = AuthorizationHelper.GenerateKeyAuthorizationCore(verb, resourceId, resourceType, headers, key)
        '    Return AuthorizationFormatPrefixUrlEncoded & HttpUtility.UrlEncode(authorizationToken)
        'End Function

        '' This is a helper for both system and master keys
        '<SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification:="HTTP Headers are ASCII")>
        'Public Function GenerateKeyAuthorizationSignature(ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal stringHMACSHA256Helper As IComputeHash) As String
        '    Dim payloadStream As ArrayOwner = Nothing
        '    Dim authorizationToken = AuthorizationHelper.GenerateUrlEncodedAuthorizationTokenWithHashCore(verb, resourceId, resourceType, headers, stringHMACSHA256Helper, payloadStream)

        '    Using payloadStream
        '        Return AuthorizationFormatPrefixUrlEncoded & authorizationToken
        '    End Using
        'End Function

        '' This is a helper for both system and master keys
        '<SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification:="HTTP Headers are ASCII")>
        'Public Function GenerateKeyAuthorizationSignature(ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal stringHMACSHA256Helper As IComputeHash, <Out> ByRef payload As String) As String
        '    Dim payloadStream As ArrayOwner = Nothing
        '    Dim authorizationToken = AuthorizationHelper.GenerateUrlEncodedAuthorizationTokenWithHashCore(verb, resourceId, resourceType, headers, stringHMACSHA256Helper, payloadStream)

        '    Using payloadStream
        '        payload = AuthorizationEncoding.GetString(payloadStream.Buffer.Array, payloadStream.Buffer.Offset, payloadStream.Buffer.Count)
        '        Return AuthorizationFormatPrefixUrlEncoded & authorizationToken
        '    End Using
        'End Function

        '' This is a helper for both system and master keys
        '<SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification:="HTTP Headers are ASCII")>
        'Public Function GenerateKeyAuthorizationSignature(ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal stringHMACSHA256Helper As IComputeHash, <Out> ByRef payload As ArrayOwner) As String
        '    Dim authorizationToken = AuthorizationHelper.GenerateUrlEncodedAuthorizationTokenWithHashCore(verb:=verb, resourceId:=resourceId, resourceType:=resourceType, headers:=headers, stringHMACSHA256Helper:=stringHMACSHA256Helper, payload:=payload)

        '    Try
        '        Return AuthorizationFormatPrefixUrlEncoded & authorizationToken
        '    Catch
        '        payload.Dispose()
        '        Throw
        '    End Try
        'End Function

        '' used in Compute
        'Public Sub ParseAuthorizationToken(ByVal authorizationTokenString As String, <Out> ByRef typeOutput As ReadOnlyMemory(Of Char), <Out> ByRef versionOutput As ReadOnlyMemory(Of Char), <Out> ByRef tokenOutput As ReadOnlyMemory(Of Char))
        '    typeOutput = Nothing
        '    versionOutput = Nothing
        '    tokenOutput = Nothing

        '    If String.IsNullOrEmpty(authorizationTokenString) Then
        '        DefaultTrace.TraceError("Auth token missing")
        '        Throw New UnauthorizedException(RMResources.MissingAuthHeader)
        '    End If

        '    Dim authorizationTokenLength = authorizationTokenString.Length
        '    authorizationTokenString = HttpUtility.UrlDecode(authorizationTokenString)

        '    ' Format of the token being deciphered is 
        '    ' type=<master/resource/system>&ver=<version>&sig=<base64encodedstring>

        '    ' Step 1. split the tokens into type/ver/token.
        '    ' when parsing for the last token, I use , as a separator to skip any redundant authorization headers

        '    Dim authorizationToken As ReadOnlyMemory(Of Char) = authorizationTokenString.AsMemory()
        '    Dim typeSeparatorPosition = authorizationToken.Span.IndexOf("&"c)

        '    If typeSeparatorPosition = -1 Then
        '        Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat)
        '    End If

        '    Dim authType = authorizationToken.Slice(0, typeSeparatorPosition)
        '    authorizationToken = authorizationToken.Slice(typeSeparatorPosition + 1, authorizationToken.Length - typeSeparatorPosition - 1)
        '    Dim versionSepartorPosition = authorizationToken.Span.IndexOf("&"c)

        '    If versionSepartorPosition = -1 Then
        '        Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat)
        '    End If

        '    Dim version = authorizationToken.Slice(0, versionSepartorPosition)
        '    authorizationToken = authorizationToken.Slice(versionSepartorPosition + 1, authorizationToken.Length - versionSepartorPosition - 1)
        '    Dim token = authorizationToken
        '    Dim tokenSeparatorPosition = authorizationToken.Span.IndexOf(","c)

        '    If tokenSeparatorPosition <> -1 Then
        '        token = authorizationToken.Slice(0, tokenSeparatorPosition)
        '    End If

        '    ' Step 2. For each token, split to get the right half of '='
        '    ' Additionally check for the left half to be the expected scheme type
        '    Dim typeKeyValueSepartorPosition = authType.Span.IndexOf("="c)

        '    If typeKeyValueSepartorPosition = -1 OrElse Not authType.Span.Slice(0, typeKeyValueSepartorPosition).SequenceEqual(Constants.Properties.AuthSchemaType.AsSpan()) OrElse Not authType.Span.Slice(0, typeKeyValueSepartorPosition).ToString().Equals(Constants.Properties.AuthSchemaType, StringComparison.OrdinalIgnoreCase) Then
        '        Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat)
        '    End If

        '    Dim authTypeValue = authType.Slice(typeKeyValueSepartorPosition + 1)

        '    If MemoryExtensions.Equals(authTypeValue.Span, Constants.Properties.AadToken.AsSpan(), StringComparison.OrdinalIgnoreCase) Then
        '        If authorizationTokenLength > MaxAadAuthorizationHeaderSize Then
        '            DefaultTrace.TraceError($"Token of type [{authTypeValue.Span.ToString()}] was of size [{authorizationTokenLength}] while the max allowed size is [{MaxAadAuthorizationHeaderSize}].")
        '            Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat, SubStatusCodes.InvalidAuthHeaderFormat)
        '        End If
        '    ElseIf MemoryExtensions.Equals(authTypeValue.Span, Constants.Properties.ResourceToken.AsSpan(), StringComparison.OrdinalIgnoreCase) Then

        '        If authorizationTokenLength > MaxResourceTokenAuthorizationHeaderSize Then
        '            DefaultTrace.TraceError($"Token of type [{authTypeValue.Span.ToString()}] was of size [{authorizationTokenLength}] while the max allowed size is [{MaxResourceTokenAuthorizationHeaderSize}].")
        '            Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat, SubStatusCodes.InvalidAuthHeaderFormat)
        '        End If
        '    ElseIf authorizationTokenLength > MaxAuthorizationHeaderSize Then
        '        DefaultTrace.TraceError($"Token of type [{authTypeValue.Span.ToString()}] was of size [{authorizationTokenLength}] while the max allowed size is [{MaxAuthorizationHeaderSize}].")
        '        Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat, SubStatusCodes.InvalidAuthHeaderFormat)
        '    End If

        '    Dim versionKeyValueSeparatorPosition = version.Span.IndexOf("="c)

        '    If versionKeyValueSeparatorPosition = -1 OrElse Not version.Span.Slice(0, versionKeyValueSeparatorPosition).SequenceEqual(Constants.Properties.AuthVersion.AsSpan()) OrElse Not version.Slice(0, versionKeyValueSeparatorPosition).ToString().Equals(Constants.Properties.AuthVersion, StringComparison.OrdinalIgnoreCase) Then
        '        Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat)
        '    End If

        '    Dim versionValue = version.Slice(versionKeyValueSeparatorPosition + 1)
        '    Dim tokenKeyValueSeparatorPosition = token.Span.IndexOf("="c)

        '    If tokenKeyValueSeparatorPosition = -1 OrElse Not token.Slice(0, tokenKeyValueSeparatorPosition).Span.SequenceEqual(Constants.Properties.AuthSignature.AsSpan()) OrElse Not token.Slice(0, tokenKeyValueSeparatorPosition).ToString().Equals(Constants.Properties.AuthSignature, StringComparison.OrdinalIgnoreCase) Then
        '        Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat)
        '    End If

        '    Dim tokenValue = token.Slice(tokenKeyValueSeparatorPosition + 1)

        '    If authTypeValue.IsEmpty OrElse versionValue.IsEmpty OrElse tokenValue.IsEmpty Then
        '        Throw New UnauthorizedException(RMResources.InvalidAuthHeaderFormat)
        '    End If

        '    typeOutput = authTypeValue
        '    versionOutput = versionValue
        '    tokenOutput = tokenValue
        'End Sub

        '' used in Compute
        'Public Function CheckPayloadUsingKey(ByVal inputToken As ReadOnlyMemory(Of Char), ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal key As String) As Boolean
        '    Dim requestBasedToken = AuthorizationHelper.GenerateKeyAuthorizationCore(verb, resourceId, resourceType, headers, key)
        '    Return inputToken.Span.SequenceEqual(requestBasedToken.AsSpan()) OrElse inputToken.ToString().Equals(requestBasedToken, StringComparison.OrdinalIgnoreCase)
        'End Function

        '' used by Compute
        'Public Sub ValidateInputRequestTime(ByVal requestHeaders As INameValueCollection, ByVal masterTokenExpiryInSeconds As Integer, ByVal allowedClockSkewInSeconds As Integer)
        '    ValidateInputRequestTime(requestHeaders, Function(headers, field) GetHeaderValue(headers, field), masterTokenExpiryInSeconds, allowedClockSkewInSeconds)
        'End Sub

        'Public Sub ValidateInputRequestTime(Of T)(ByVal requestHeaders As T, ByVal headerGetter As Func(Of T, String, String), ByVal masterTokenExpiryInSeconds As Integer, ByVal allowedClockSkewInSeconds As Integer)
        '    If requestHeaders Is Nothing Then
        '        DefaultTrace.TraceError("Null request headers for validating auth time")
        '        Throw New UnauthorizedException(RMResources.MissingDateForAuthorization)
        '    End If

        '    ' Fetch the date in the headers to compare against the correct time.
        '    ' Since Date header is overridden by some proxies/http client libraries, we support
        '    ' an additional date header 'x-ms-date' and prefer that to the regular 'date' header.
        '    Dim dateToCompare = headerGetter(requestHeaders, HttpConstants.HttpHeaders.XDate)

        '    If String.IsNullOrEmpty(dateToCompare) Then
        '        dateToCompare = headerGetter(requestHeaders, HttpConstants.HttpHeaders.HttpDate)
        '    End If

        '    ValidateInputRequestTime(dateToCompare, masterTokenExpiryInSeconds, allowedClockSkewInSeconds)
        'End Sub

        'Public Sub CheckTimeRangeIsCurrent(ByVal allowedClockSkewInSeconds As Integer, ByVal startDateTime As Date, ByVal expiryDateTime As Date)
        '    ' Check if time ranges provided are beyond DateTime.MinValue or DateTime.MaxValue
        '    Dim outOfRange = startDateTime <= Date.MinValue.AddSeconds(allowedClockSkewInSeconds) OrElse expiryDateTime >= Date.MaxValue.AddSeconds(-allowedClockSkewInSeconds)

        '    ' Adjust for a time lag between various instances upto 5 minutes i.e. allow [start-5, end+5]
        '    If outOfRange OrElse startDateTime.AddSeconds(-allowedClockSkewInSeconds) > Date.UtcNow OrElse expiryDateTime.AddSeconds(allowedClockSkewInSeconds) < Date.UtcNow Then
        '        Dim message = String.Format(CultureInfo.InvariantCulture, RMResources.InvalidTokenTimeRange, startDateTime.ToString("r", CultureInfo.InvariantCulture), expiryDateTime.ToString("r", CultureInfo.InvariantCulture), Date.UtcNow.ToString("r", CultureInfo.InvariantCulture))
        '        DefaultTrace.TraceError(message)
        '        Throw New ForbiddenException(message)
        '    End If
        'End Sub

        'Friend Sub GetResourceTypeAndIdOrFullName(ByVal uri As Uri, <Out> ByRef isNameBased As Boolean, <Out> ByRef resourceType As String, <Out> ByRef resourceId As String, ByVal Optional clientVersion As String = "")
        '    If uri Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(uri))
        '    End If

        '    resourceType = String.Empty
        '    resourceId = String.Empty
        '    Dim uriSegmentsCount = uri.Segments.Length

        '    If uriSegmentsCount < 1 Then
        '        Throw New ArgumentException(RMResources.InvalidUrl)
        '    End If

        '    ' Authorization code is fine with Uri not having resource id and path. 
        '    ' We will just return empty in that case
        '    If Not PathsHelper.TryParsePathSegments(uri.PathAndQuery, __, resourceType, resourceId, isNameBased, clientVersion) Then
        '        resourceType = String.Empty
        '        resourceId = String.Empty
        '    End If
        'End Sub

        'Public Function IsUserRequest(ByVal resourceType As String) As Boolean
        '    If String.Compare(resourceType, Paths.Root, StringComparison.OrdinalIgnoreCase) = 0 OrElse String.Compare(resourceType, Paths.PartitionKeyRangePreSplitSegment, StringComparison.OrdinalIgnoreCase) = 0 OrElse String.Compare(resourceType, Paths.PartitionKeyRangePostSplitSegment, StringComparison.OrdinalIgnoreCase) = 0 OrElse String.Compare(resourceType, Paths.ControllerOperations_BatchGetOutput, StringComparison.OrdinalIgnoreCase) = 0 OrElse String.Compare(resourceType, Paths.ControllerOperations_BatchReportCharges, StringComparison.OrdinalIgnoreCase) = 0 OrElse String.Compare(resourceType, Paths.Operations_GetStorageAccountKey, StringComparison.OrdinalIgnoreCase) = 0 Then
        '        Return False
        '    End If

        '    Return True
        'End Function

        'Public Function GetSystemOperationType(ByVal readOnlyRequest As Boolean, ByVal resourceType As String) As AuthorizationTokenType
        '    If Not IsUserRequest(resourceType) Then
        '        If readOnlyRequest Then
        '            Return AuthorizationTokenType.SystemReadOnly
        '        Else
        '            Return AuthorizationTokenType.SystemAll
        '        End If
        '    End If

        '    ' operations on user resources
        '    If readOnlyRequest Then
        '        Return AuthorizationTokenType.SystemReadOnly
        '    Else
        '        Return AuthorizationTokenType.SystemReadWrite
        '    End If
        'End Function

        'Public Function SerializeMessagePayload(ByVal stream As Span(Of Byte), ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal Optional bUseUtcNowForMissingXDate As Boolean = False) As Integer
        '    Dim xDate = AuthorizationHelper.GetHeaderValue(headers, HttpConstants.HttpHeaders.XDate)
        '    Dim [date] = AuthorizationHelper.GetHeaderValue(headers, HttpConstants.HttpHeaders.HttpDate)

        '    ' At-least one of date header should present
        '    ' https://docs.microsoft.com/en-us/rest/api/documentdb/access-control-on-documentdb-resources 
        '    If String.IsNullOrEmpty(xDate) AndAlso String.IsNullOrWhiteSpace([date]) Then
        '        If Not bUseUtcNowForMissingXDate Then
        '            Throw New UnauthorizedException(RMResources.InvalidDateHeader)
        '        End If

        '        headers(HttpConstants.HttpHeaders.XDate) = Date.UtcNow.ToString("r", CultureInfo.InvariantCulture)
        '        xDate = AuthorizationHelper.GetHeaderValue(headers, HttpConstants.HttpHeaders.XDate)
        '    End If

        '    ' for name based, it is case sensitive, we won't use the lower case
        '    If Not PathsHelper.IsNameBased(resourceId) Then
        '        resourceId = resourceId.ToLowerInvariant()
        '    End If

        '    Dim totalLength = 0
        '    Dim length As Integer = stream.Write(verb.ToLowerInvariant())
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(vbLf)
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(resourceType.ToLowerInvariant())
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(vbLf)
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(resourceId)
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(vbLf)
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(xDate.ToLowerInvariant())
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(vbLf)
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(If(xDate.Equals(String.Empty, StringComparison.OrdinalIgnoreCase), [date].ToLowerInvariant(), String.Empty))
        '    totalLength += length
        '    stream = stream.Slice(length)
        '    length = stream.Write(vbLf)
        '    totalLength += length
        '    Return totalLength
        'End Function

        'Public Function IsResourceToken(ByVal token As String) As Boolean
        '    Dim typeSeparatorPosition = token.IndexOf("&"c)

        '    If typeSeparatorPosition = -1 Then
        '        Return False
        '    End If

        'Dim authType = token.Substring(0, typeSeparatorPosition)
        'Dim typeKeyValueSepartorPosition = authType.IndexOf("="c)

        'If typeKeyValueSepartorPosition = -1 OrElse Not authType.Substring(0, typeKeyValueSepartorPosition).Equals(Constants.Properties.AuthSchemaType, StringComparison.OrdinalIgnoreCase) Then
        '    Return False
        'End If

        'Dim authTypeValue = authType.Substring(typeKeyValueSepartorPosition + 1)
        'Return authTypeValue.Equals(Constants.Properties.ResourceToken, StringComparison.OrdinalIgnoreCase)
        'End Function

        'Friend Function GetHeaderValue(ByVal headerValues As INameValueCollection, ByVal key As String) As String
        '    If headerValues Is Nothing Then
        '        Return String.Empty
        '    End If

        '    Return If(headerValues(key), String.Empty)
        'End Function

        'Friend Function GetHeaderValue(ByVal headerValues As IDictionary(Of String, String), ByVal key As String) As String
        '    If headerValues Is Nothing Then
        '        Return String.Empty
        '    End If

        '    Dim value As String = Nothing
        '    headerValues.TryGetValue(key, value)
        '    Return value
        'End Function

        'Friend Function GetAuthorizationResourceIdOrFullName(ByVal resourceType As String, ByVal resourceIdOrFullName As String) As String
        '    If String.IsNullOrEmpty(resourceType) OrElse String.IsNullOrEmpty(resourceIdOrFullName) Then
        '        Return resourceIdOrFullName
        '    End If

        '    If PathsHelper.IsNameBased(resourceIdOrFullName) Then
        '        ' resource fullname is always end with name (not type segment like docs/colls).
        '        Return resourceIdOrFullName
        '    End If

        '    If resourceType.Equals(Paths.OffersPathSegment, StringComparison.OrdinalIgnoreCase) OrElse resourceType.Equals(Paths.PartitionsPathSegment, StringComparison.OrdinalIgnoreCase) OrElse resourceType.Equals(Paths.TopologyPathSegment, StringComparison.OrdinalIgnoreCase) OrElse resourceType.Equals(Paths.RidRangePathSegment, StringComparison.OrdinalIgnoreCase) OrElse resourceType.Equals(Paths.SnapshotsPathSegment, StringComparison.OrdinalIgnoreCase) Then
        '        Return resourceIdOrFullName
        '    End If

        '    Dim parsedRId As ResourceId = ResourceId.Parse(resourceIdOrFullName)

        '    If resourceType.Equals(Paths.DatabasesPathSegment, StringComparison.OrdinalIgnoreCase) Then
        '        Return parsedRId.DatabaseId.ToString()
        '    ElseIf resourceType.Equals(Paths.UsersPathSegment, StringComparison.OrdinalIgnoreCase) Then
        '        Return parsedRId.UserId.ToString()
        '    ElseIf resourceType.Equals(Paths.UserDefinedTypesPathSegment, StringComparison.OrdinalIgnoreCase) Then
        '        Return parsedRId.UserDefinedTypeId.ToString()
        '    ElseIf resourceType.Equals(Paths.CollectionsPathSegment, StringComparison.OrdinalIgnoreCase) Then
        '        Return parsedRId.DocumentCollectionId.ToString()
        '    ElseIf resourceType.Equals(Paths.ClientEncryptionKeysPathSegment, StringComparison.OrdinalIgnoreCase) Then
        '        Return parsedRId.ClientEncryptionKeyId.ToString()
        '    ElseIf resourceType.Equals(Paths.DocumentsPathSegment, StringComparison.OrdinalIgnoreCase) Then
        '        Return parsedRId.DocumentId.ToString()
        '    Else
        '        ' leaf node 
        '        Return resourceIdOrFullName
        '    End If
        'End Function

        'Public Function GenerateUriFromAddressRequestUri(ByVal uri As Uri) As Uri
        '    ' Address request has the URI fragment (dbs/dbid/colls/colId...) as part of
        '    ' either $resolveFor 'or' $generate queries of the context.RequestUri.
        '    ' Extracting out the URI in the form https://localhost/dbs/dbid/colls/colId/docs to generate the signature.
        '    ' Authorizer uses the same URI to verify signature.
        '    Dim addressFeedUri As String = If(UrlUtility.ParseQuery(uri.Query)(HttpConstants.QueryStrings.Url), If(UrlUtility.ParseQuery(uri.Query)(HttpConstants.QueryStrings.GenerateId), UrlUtility.ParseQuery(uri.Query)(HttpConstants.QueryStrings.GetChildResourcePartitions)))

        '    If String.IsNullOrEmpty(addressFeedUri) Then
        '        Throw New BadRequestException(RMResources.BadUrl)
        '    End If

        '    Return New Uri(uri.Scheme & "://" & uri.Host & "/" & HttpUtility.UrlDecode(addressFeedUri).Trim("/"c))
        'End Function

        'Private Sub ValidateInputRequestTime(ByVal dateToCompare As String, ByVal masterTokenExpiryInSeconds As Integer, ByVal allowedClockSkewInSeconds As Integer)
        '    If String.IsNullOrEmpty(dateToCompare) Then
        '        Throw New UnauthorizedException(RMResources.MissingDateForAuthorization)
        '    End If

        '    Dim utcStartTime As Date = Nothing

        '    If Not Date.TryParse(dateToCompare, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal Or DateTimeStyles.AdjustToUniversal Or DateTimeStyles.AllowWhiteSpaces, utcStartTime) Then
        '        Throw New UnauthorizedException(RMResources.InvalidDateHeader)
        '    End If

        '    ' Check if time range is beyond DateTime.MaxValue
        '    Dim outOfRange = utcStartTime >= Date.MaxValue.AddSeconds(-masterTokenExpiryInSeconds)

        '    If outOfRange Then
        '        Dim message = String.Format(CultureInfo.InvariantCulture, RMResources.InvalidTokenTimeRange, utcStartTime.ToString("r", CultureInfo.InvariantCulture), Date.MaxValue.ToString("r", CultureInfo.InvariantCulture), Date.UtcNow.ToString("r", CultureInfo.InvariantCulture))
        '        DefaultTrace.TraceError(message)
        '        Throw New ForbiddenException(message)
        '    End If

        '    Dim utcEndTime = utcStartTime + TimeSpan.FromSeconds(masterTokenExpiryInSeconds)
        '    CheckTimeRangeIsCurrent(allowedClockSkewInSeconds, utcStartTime, utcEndTime)
        'End Sub

        '' This function is used by Compute
        'Friend Function GenerateAuthorizationTokenWithHashCore(ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal stringHMACSHA256Helper As IComputeHash, <Out> ByRef payload As ArrayOwner) As String
        '    Return AuthorizationHelper.GenerateAuthorizationTokenWithHashCore(verb, resourceId, resourceType, headers, stringHMACSHA256Helper, urlEncode:=False, payload)
        'End Function

        'Private Function GenerateUrlEncodedAuthorizationTokenWithHashCore(ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal stringHMACSHA256Helper As IComputeHash, <Out> ByRef payload As ArrayOwner) As String
        '    Return AuthorizationHelper.GenerateAuthorizationTokenWithHashCore(verb, resourceId, resourceType, headers, stringHMACSHA256Helper, urlEncode:=True, payload)
        'End Function

        'Private Function GenerateAuthorizationTokenWithHashCore(ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal stringHMACSHA256Helper As IComputeHash, ByVal urlEncode As Boolean, <Out> ByRef payload As ArrayOwner) As String
        '    ' resourceId can be null for feed-read of /dbs
        '    If String.IsNullOrEmpty(verb) Then
        '        Throw New ArgumentException(RMResources.StringArgumentNullOrEmpty, NameOf(verb))
        '    End If

        '    If Equals(resourceType, Nothing) Then
        '        Throw New ArgumentNullException(NameOf(resourceType)) ' can be empty
        '    End If

        '    If stringHMACSHA256Helper Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(stringHMACSHA256Helper))
        '    End If

        '    If headers Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(headers))
        '    End If

        '    ' Order of the values included in the message payload is a protocol that clients/BE need to follow exactly.
        '    ' More headers can be added in the future.
        '    ' If any of the value is optional, it should still have the placeholder value of ""
        '    ' OperationType -> ResourceType -> ResourceId/OwnerId -> XDate -> Date
        '    Dim verbInput = If(verb, String.Empty)
        '    Dim resourceIdInput = If(resourceId, String.Empty)
        '    Dim resourceTypeInput = If(resourceType, String.Empty)
        '    Dim authResourceId = GetAuthorizationResourceIdOrFullName(resourceTypeInput, resourceIdInput)
        '    Dim capacity = ComputeMemoryCapacity(verbInput, authResourceId, resourceTypeInput)
        '    Dim buffer = ArrayPool(Of Byte).Shared.Rent(capacity)

        '    Try
        '        Dim payloadBytes As Span(Of Byte) = buffer
        '        Dim length = AuthorizationHelper.SerializeMessagePayload(payloadBytes, verbInput, authResourceId, resourceTypeInput, headers)
        '        payload = New ArrayOwner(ArrayPool(Of Byte).Shared, New ArraySegment(Of Byte)(buffer, 0, length))
        '        Dim hashPayLoad As Byte() = stringHMACSHA256Helper.ComputeHash(payload.Buffer)
        '        Return AuthorizationHelper.OptimizedConvertToBase64string(hashPayLoad, urlEncode)
        '    Catch
        '        ArrayPool(Of Byte).Shared.Return(buffer)
        '        Throw
        '    End Try

        '    ''' <summary>
        '    ''' This an optimized version of doing Convert.ToBase64String(hashPayLoad) with an optional wrapping HttpUtility.UrlEncode.
        '    ''' This avoids the over head of converting it to a string and back to a byte[].
        '    ''' </summary>
        '    ' Create a large enough buffer that URL encode can use it.
        '    ' Increase the buffer by 3x so it can be used for the URL encoding
        '    ' This replaces the Convert.ToBase64String
        'End Function

        '''' Cannot convert MethodDeclarationSyntax, System.NotSupportedException: UnsafeKeyword is not supported!
        ''''    at ICSharpCode.CodeConverter.VB.SyntaxKindExtensions.ConvertToken(SyntaxKind t, TokenContext context) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\SyntaxKindExtensions.cs:line 190
        ''''    at ICSharpCode.CodeConverter.VB.CommonConversions.ConvertModifier(SyntaxToken m, TokenContext context) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommonConversions.cs:line 472
        ''''    at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext()
        ''''    at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.ToList()
        ''''    at ICSharpCode.CodeConverter.VB.CommonConversions.ConvertModifiersCore(IReadOnlyCollection`1 modifiers, TokenContext context, Boolean isConstructor) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommonConversions.cs:line 425
        ''''    at ICSharpCode.CodeConverter.VB.NodesVisitor.VisitMethodDeclaration(MethodDeclarationSyntax node) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\NodesVisitor.cs:line 429
        ''''    at Microsoft.CodeAnalysis.CSharp.CSharpSyntaxVisitor`1.Visit(SyntaxNode node)
        ''''    at ICSharpCode.CodeConverter.VB.CommentConvertingVisitorWrapper`1.Accept(SyntaxNode csNode, Boolean addSourceMapping) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommentConvertingVisitorWrapper.cs:line 26
        '''' 
        '''' Input:
        '''' 
        ''''         /// <summary>
        ''''         /// This an optimized version of doing Convert.ToBase64String(hashPayLoad) with an optional wrapping HttpUtility.UrlEncode.
        ''''         /// This avoids the over head of converting it to a string and back to a byte[].
        ''''         /// </summary>
        ''''         private static unsafe string OptimizedConvertToBase64string(byte[] hashPayLoad, bool urlEncode)
        ''''         {
        ''''             // Create a large enough buffer that URL encode can use it.
        ''''             // Increase the buffer by 3x so it can be used for the URL encoding
        ''''             int capacity = System.Buffers.Text.Base64.GetMaxEncodedToUtf8Length(hashPayLoad.Length) * 3;
        ''''             byte[] rentedBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(capacity);
        '''' 
        ''''             try
        ''''             {
        ''''                 System.Span<byte> encodingBuffer = rentedBuffer;
        ''''                 // This replaces the Convert.ToBase64String
        ''''                 System.Buffers.OperationStatus status = System.Buffers.Text.Base64.EncodeToUtf8(
        ''''                     hashPayLoad,
        ''''                     encodingBuffer,
        ''''                     out int _,
        ''''                     out int bytesWritten);
        '''' 
        ''''                 if (status != System.Buffers.OperationStatus.Done)
        ''''                 {
        ''''                     throw new System.ArgumentException($"Authorization key payload is invalid. {status}");
        ''''                 }
        '''' 
        ''''                 return urlEncode 
        ''''                     ? Microsoft.Azure.Cosmos.AuthorizationHelper.UrlEncodeBase64SpanInPlace(encodingBuffer, bytesWritten)
        ''''                     : System.Text.Encoding.UTF8.GetString(encodingBuffer.Slice(0, bytesWritten));
        ''''             }
        ''''             finally
        ''''             {
        ''''                 if (rentedBuffer != null)
        ''''                 {
        ''''                     System.Buffers.ArrayPool<byte>.Shared.Return(rentedBuffer);
        ''''                 }
        ''''             }
        ''''         }
        '''' 
        '''' 

        '' This function is used by Compute
        'Friend Function ComputeMemoryCapacity(ByVal verbInput As String, ByVal authResourceId As String, ByVal resourceTypeInput As String) As Integer
        '    Return verbInput.Length + AuthorizationEncoding.GetMaxByteCount(authResourceId.Length) + resourceTypeInput.Length + 5 + 30 ' new line characters
        '    ' date header length;
        'End Function

        'Private Function GenerateKeyAuthorizationCore(ByVal verb As String, ByVal resourceId As String, ByVal resourceType As String, ByVal headers As INameValueCollection, ByVal key As String) As String
        '    ' resourceId can be null for feed-read of /dbs
        '    If String.IsNullOrEmpty(verb) Then
        '        Throw New ArgumentException(RMResources.StringArgumentNullOrEmpty, NameOf(verb))
        '    End If

        '    If Equals(resourceType, Nothing) Then
        '        Throw New ArgumentNullException(NameOf(resourceType)) ' can be empty
        '    End If

        '    If String.IsNullOrEmpty(key) Then
        '        Throw New ArgumentException(RMResources.StringArgumentNullOrEmpty, NameOf(key))
        '    End If

        '    If headers Is Nothing Then
        '        Throw New ArgumentNullException(NameOf(headers))
        '    End If

        '    Dim keyBytes = Convert.FromBase64String(key)

        '    Using hmacSha256 As HMACSHA256 = New HMACSHA256(keyBytes)
        '        ' Order of the values included in the message payload is a protocol that clients/BE need to follow exactly.
        '        ' More headers can be added in the future.
        '        ' If any of the value is optional, it should still have the placeholder value of ""
        '        ' OperationType -> ResourceType -> ResourceId/OwnerId -> XDate -> Date
        '        Dim verbInput = If(verb, String.Empty)
        '        Dim resourceIdInput = If(resourceId, String.Empty)
        '        Dim resourceTypeInput = If(resourceType, String.Empty)
        '        Dim authResourceId = GetAuthorizationResourceIdOrFullName(resourceTypeInput, resourceIdInput)
        '        Dim memoryStreamCapacity = ComputeMemoryCapacity(verbInput, authResourceId, resourceTypeInput)
        '        Dim arrayPoolBuffer = ArrayPool(Of Byte).Shared.Rent(memoryStreamCapacity)

        '        Try
        '            Dim length = AuthorizationHelper.SerializeMessagePayload(arrayPoolBuffer, verbInput, authResourceId, resourceTypeInput, headers)
        '            Dim hashPayLoad = hmacSha256.ComputeHash(arrayPoolBuffer, 0, length)
        '            Return Convert.ToBase64String(hashPayLoad)
        '        Finally
        '            ArrayPool(Of Byte).Shared.Return(arrayPoolBuffer)
        '        End Try
        '    End Using

        '    ''' <summary>
        '    ''' This does HttpUtility.UrlEncode functionality with Span buffer. It does an in place update to avoid
        '    ''' creating the new buffer.
        '    ''' </summary>
        '    ''' <paramname="base64Bytes">The buffer that include the bytes to url encode.</param>
        '    ''' <paramname="length">The length of bytes used in the buffer</param>
        '    ''' <returns>The URLEncoded string of the bytes in the buffer</returns>
        '    ' Base64 is limited to Alphanumeric characters and '/' '=' '+'
        'End Function

        '''' Cannot convert MethodDeclarationSyntax, System.NotSupportedException: UnsafeKeyword is not supported!
        ''''    at ICSharpCode.CodeConverter.VB.SyntaxKindExtensions.ConvertToken(SyntaxKind t, TokenContext context) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\SyntaxKindExtensions.cs:line 190
        ''''    at ICSharpCode.CodeConverter.VB.CommonConversions.ConvertModifier(SyntaxToken m, TokenContext context) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommonConversions.cs:line 472
        ''''    at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext()
        ''''    at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.ToList()
        ''''    at ICSharpCode.CodeConverter.VB.CommonConversions.ConvertModifiersCore(IReadOnlyCollection`1 modifiers, TokenContext context, Boolean isConstructor) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommonConversions.cs:line 425
        ''''    at ICSharpCode.CodeConverter.VB.NodesVisitor.VisitMethodDeclaration(MethodDeclarationSyntax node) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\NodesVisitor.cs:line 429
        ''''    at Microsoft.CodeAnalysis.CSharp.CSharpSyntaxVisitor`1.Visit(SyntaxNode node)
        ''''    at ICSharpCode.CodeConverter.VB.CommentConvertingVisitorWrapper`1.Accept(SyntaxNode csNode, Boolean addSourceMapping) in D:\GitWorkspace\CodeConverter\CodeConverter\VB\CommentConvertingVisitorWrapper.cs:line 26
        '''' 
        '''' Input:
        '''' 
        ''''         /// <summary>
        ''''         /// This does HttpUtility.UrlEncode functionality with Span buffer. It does an in place update to avoid
        ''''         /// creating the new buffer.
        ''''         /// </summary>
        ''''         /// <param name="base64Bytes">The buffer that include the bytes to url encode.</param>
        ''''         /// <param name="length">The length of bytes used in the buffer</param>
        ''''         /// <returns>The URLEncoded string of the bytes in the buffer</returns>
        ''''         public unsafe static string UrlEncodeBase64SpanInPlace(System.Span<byte> base64Bytes, int length)
        ''''         {
        ''''             if (base64Bytes == default)
        ''''             {
        ''''                 throw new System.ArgumentNullException(nameof(base64Bytes));
        ''''             }
        '''' 
        ''''             if (base64Bytes.Length < length * 3)
        ''''             {
        ''''                 throw new System.ArgumentException($"{nameof(base64Bytes)} should be 3x to avoid running out of space in worst case scenario where all characters are special");
        ''''             }
        '''' 
        ''''             if (length == 0)
        ''''             {
        ''''                 return string.Empty;
        ''''             }
        '''' 
        ''''             int escapeBufferPosition = base64Bytes.Length - 1;
        ''''             for (int i = length - 1; i >= 0; i--)
        ''''             { 
        ''''                 byte curr = base64Bytes[i];
        ''''                 // Base64 is limited to Alphanumeric characters and '/' '=' '+'
        ''''                 switch (curr)
        ''''                 {
        ''''                     case (byte)'/':
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'f';
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'2';
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'%';
        ''''                         break;
        ''''                     case (byte)'=':
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'d';
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'3';
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'%';
        ''''                         break;
        ''''                     case (byte)'+':
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'b';
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'2';
        ''''                         base64Bytes[escapeBufferPosition--] = (byte)'%';
        ''''                         break;
        ''''                     default:
        ''''                         base64Bytes[escapeBufferPosition--] = curr;
        ''''                         break;
        ''''                 }
        ''''             }
        '''' 
        ''''             System.Span<byte> endSlice = base64Bytes.Slice(escapeBufferPosition + 1);
        ''''             fixed (byte* bp = endSlice)
        ''''             {
        ''''                 return System.Text.Encoding.UTF8.GetString(bp, endSlice.Length);
        ''''             }
        ''''         }
        '''' 
        '''' 
        '<Extension()>
        'Private Function Write(ByVal stream As Span(Of Byte), ByVal contentToWrite As String) As Integer
        '    Dim actualByteCount = AuthorizationEncoding.GetBytes(contentToWrite, stream)
        '    Return actualByteCount
        'End Function

        'Public Structure ArrayOwner
        '    Implements IDisposable

        '    Private _Buffer As System.ArraySegment(Of Byte)
        '    Private ReadOnly pool As ArrayPool(Of Byte)

        '    Public Sub New(ByVal pool As ArrayPool(Of Byte), ByVal buffer As ArraySegment(Of Byte))
        '        Me.pool = pool
        '        Me.Buffer = buffer
        '    End Sub

        '    Public Property Buffer As ArraySegment(Of Byte)
        '        Get
        '            Return _Buffer
        '        End Get
        '        Private Set(ByVal value As ArraySegment(Of Byte))
        '            _Buffer = value
        '        End Set
        '    End Property

        '    Public Sub Dispose() Implements IDisposable.Dispose
        '        If Buffer.Array IsNot Nothing Then
        '            pool?.Return(Buffer.Array)
        '            Buffer = Nothing
        '        End If
        '    End Sub
        'End Structure
    End Module
End Namespace
