'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------

Imports System
Imports System.Security

Namespace Microsoft.Azure.Cosmos
    Friend Interface IComputeHash
        Inherits IDisposable

        Function ComputeHash(ByVal bytesToHash As ArraySegment(Of Byte)) As Byte()
        ReadOnly Property Key As SecureString
    End Interface
End Namespace

' jako ze System.Security.SecureString nie widać, to robie to sam (choć bez czyszczenia pamięci etc.)
Public Class SecureString
    Public _string As String
End Class