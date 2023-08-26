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

## Publish Project

```
dotnet publish --configuration Release
```

## Import Module Locally

```
Import-Module <Path-to-Project>\bin\Release\net7.0\publish\PwshAzCosmosDB.dll
```