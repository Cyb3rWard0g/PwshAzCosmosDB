# PwshAzCosmosDB

PwshAzCosmosDB is a PowerShell binary module built using .NET Core and designed to work with PowerShell Core. It provides cmdlets, written in C#, for performing document-level operations against Azure Cosmos DB. This module is designed to fill the gap left by the lack of document-level operations in the existing [Az.CosmosDB module](https://learn.microsoft.com/en-us/powershell/module/az.cosmosdb/?view=azps-10.2.0).

## Dependencies

`PwshAzCosmosDB` depends on the following NuGet packages:

- [Azure.Identity](https://www.nuget.org/packages/Azure.Identity/) (Version 1.10.0)
- [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos/) (Version 3.35.3)
- [PowerShellStandard.Library](https://www.nuget.org/packages/PowerShellStandard.Library/) (Version 7.0.0-preview.1)
- [dnMerge](https://www.nuget.org/packages/dnMerge/) (Version 0.5.15)

Note that the `dnMerge` package is used to merge multiple NuGet packages into a single assembly to improve module compatibility.

## Installation

To install the [PwshAzCosmosDB module](https://www.powershellgallery.com/packages/PwshAzCosmosDB), you can use the PowerShell Gallery:

```powershell
Install-Module -Name PwshAzCosmosDB -Scope CurrentUser -verbose
```

## Usage

`PwshAzCosmosDB` provides cmdlets for various document-level operations, including creating, reading, updating, and deleting documents in Azure Cosmos DB.

### Initialize Cosmos Client

first you need to connect to the Azure CosmosDB

#### Master Key

```powershell
$params = @{
    "Endpoint" = "https://<CosmosDB-Name>.documents.azure.com:443/"
    "DatabaseName" = "<DB-Name>"
    "ContainerName" = "<Container-Name>"
    "MasterKey" = "<Master-Key>"
    "Verbose" = $true
}
Connect-AzCosmosDB @params
```

#### User Assigned Managed Identity

```powershell
$env:MANAGED_IDENTITY_CLIENT_ID = '<user-assigned-managed-identity-client-id'

$params = @{
    "Endpoint" = "https://<CosmosDB-Name>.documents.azure.com:443/"
    "DatabaseName" = "<DB-Name>"
    "ContainerName" = "<Container-Name>"
    "Verbose" = $true
}
Connect-AzCosmosDB @params
```

### Retrieve an existing document from Azure Cosmos DB

```powershell
Get-AzCosmosDBDocument -DocumentId "document-id" -PartitionKey "<PartitionKeyValue>" -verbose
```

### Create a new document in Azure Cosmos DB

```powershell
$pkField = '<partition-key-field-name>'
$pkValue = '<partition-key-value>'
$documentHashtable = @{
    "title" = "New Title"
    "description" = "This is a new document"
    "author" = "Roberto Rodriguez"
}
New-AzCosmosDBDocument -Document $documentHashTable -PartitionKeyField $pkField -PartitionKeyValue $pkValue -verbose
```

### Update an existing document in Azure Cosmos DB

```powershell
$documentId = '<document-id>'
$pkField = '<partition-key-field-name>'
$pkValue = '<partition-key-value>'
$updatesHashtable = @{
    "testField" = "testvalue"
}
Update-AzCosmosDBDocument -DocumentId $documentId -Updates $updatesHashtable -PartitionKeyField $pkField -PartitionKeyValue $pkValue -verbose
```

### Delete a document from Azure Cosmos DB

```powershell
Remove-AzCosmosDBDocument -DocumentId "document-id" -PartitionKey "<PartitionKeyValue>" -verbose
```

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License.