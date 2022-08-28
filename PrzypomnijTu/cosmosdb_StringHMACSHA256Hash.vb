'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------

Imports System
Imports System.Collections.Concurrent
Imports System.Security
Imports System.Security.Cryptography

Namespace Microsoft.Azure.Cosmos
    Friend NotInheritable Class StringHMACSHA256Hash
        Implements IComputeHash

        Private ReadOnly _base64EncodedKey As String
        Private ReadOnly keyBytes As Byte()
        Private _secureString As SecureString
        Private hmacPool As ConcurrentQueue(Of HMACSHA256)

        Public Sub New(ByVal base64EncodedKey As String)
            _base64EncodedKey = base64EncodedKey
            keyBytes = Convert.FromBase64String(base64EncodedKey)
            hmacPool = New ConcurrentQueue(Of HMACSHA256)()
        End Sub

        Public Function ComputeHash(ByVal bytesToHash As ArraySegment(Of Byte)) As Byte() Implements IComputeHash.ComputeHash
            Dim hmacSha256 As HMACSHA256 = Nothing

            If hmacPool.TryDequeue(hmacSha256) Then
                hmacSha256.Initialize()
            Else
                hmacSha256 = New HMACSHA256(keyBytes)
            End If

            Try
                Return hmacSha256.ComputeHash(bytesToHash.Array, 0, bytesToHash.Count)
            Finally
                hmacPool.Enqueue(hmacSha256)
            End Try
        End Function

        Public ReadOnly Property Key As SecureString Implements IComputeHash.Key
            Get
                If _secureString IsNot Nothing Then Return _secureString
                _secureString = New SecureString
                _secureString._string = _base64EncodedKey
                '_secureString = SecureStringUtility.ConvertToSecureString(base64EncodedKey)
                Return _secureString
            End Get
        End Property


        Public Sub Dispose() Implements IDisposable.Dispose
            Dim hmacsha256 As HMACSHA256 = Nothing

            While hmacPool.TryDequeue(hmacsha256)
                hmacsha256.Dispose()
            End While

            'If secureString IsNot Nothing Then
            '    secureString.Dispose()
            '    secureString = Nothing
            'End If
        End Sub


    End Class
End Namespace
