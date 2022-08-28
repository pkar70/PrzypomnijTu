'------------------------------------------------------------
' Copyright (c) Microsoft Corporation.  All rights reserved.
'------------------------------------------------------------

Namespace Microsoft.Azure.Cosmos.Encryption
    Friend Module Constants
        Public Const DiagnosticsCoreDiagnostics As String = "CoreDiagnostics"
        Public Const DiagnosticsDecryptOperation As String = "Decrypt"
        Public Const DiagnosticsDuration As String = "Duration in milliseconds"
        Public Const DiagnosticsEncryptionDiagnostics As String = "EncryptionDiagnostics"
        Public Const DiagnosticsEncryptOperation As String = "Encrypt"
        Public Const DiagnosticsPropertiesEncryptedCount As String = "Properties Encrypted Count"
        Public Const DiagnosticsPropertiesDecryptedCount As String = "Properties Decrypted Count"
        Public Const DiagnosticsStartTime As String = "Start time"
        Public Const DocumentsResourcePropertyName As String = "Documents"
        Public Const IncorrectContainerRidSubStatus As String = "1024"

        ' TODO: Good to have constants available in the Cosmos SDK. Tracked via https://github.com/Azure/azure-cosmos-dotnet-v3/issues/2431
        Public Const IntendedCollectionHeader As String = "x-ms-cosmos-intended-collection-rid"
        Public Const IsClientEncryptedHeader As String = "x-ms-cosmos-is-client-encrypted"
        Public Const AllowCachedReadsHeader As String = "x-ms-cosmos-allow-cachedreads"
        Public Const DatabaseRidHeader As String = "x-ms-cosmos-database-rid"
        Public Const SubStatusHeader As String = "x-ms-substatus"
        Public Const SupportedClientEncryptionPolicyFormatVersion As Integer = 1
    End Module
End Namespace
