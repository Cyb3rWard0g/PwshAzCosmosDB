# Developer Notes

## Create Project

```
 dotnet new classlib -n PwshAzCosmosDB
```

## Add NuGet packages

Switch directory to the new project and run the following commands:

```
dotnet add package Microsoft.Azure.Cosmos
dotnet add package Microsoft.PowerShell.SDK
dotnet add package Azure.Identity
```

## Create Solution File

```
dotnet new sln

dotnet sln add .\PwshAzCosmosDB\PwshAzCosmosDB.csproj
```

## Build Project

```
dotnet build --configuration Release
```

## Publish Project (Optional)

Without `dnMerge` you can run the following command and then import the DLL created in the puslish folder.

```
dotnet publish --configuration Release

Import-Module <Path-to-Project>\bin\Release\net48\publish\PwshAzCosmosDB.dll
```

## Import Module Locally

```
Import-Module <Path-to-Project>\bin\Release\net48\PwshAzCosmosDB.dll
```

## Publish to PS Gallery

* Create `output\PwshAzCosmosDB` directory
* Copy DLL and .psd1 file to `output\PwshAzCosmosDB`
* Switch directory tp `output` directory
* Publish module to PSGallery with the following commands:

```powershell
Publish-Module -Name .\PwshAzCosmosDB\ -NuGetApiKey XXXXXX -verbose -Debug
```