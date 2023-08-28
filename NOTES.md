# Developer Notes

## Create Project Directory

```
$module = 'PwshAzCosmosDB'

New-Item -Path $module -Type Directory  

Set-Location $module
```

## Create .NET Project

```
dotnet new classlib -n $module
```

## Set SDK Version

```
dotnet new globaljson --sdk-version 7.0.400
```

## Add NuGet packages

```
dotnet add package PowerShellStandard.Library --version 7.0.0-preview.1
dotnet add package Microsoft.Azure.Cosmos
dotnet add package Microsoft.PowerShell.SDK
```

## Create Solution File

```
dotnet new sln

dotnet sln add "$($module).csproj"
```

## Add dnMerge Reference and 

```xml
<Project Sdk="Microsoft.NET.Sdk">

....

  <ItemGroup>
    <PackageReference Include="dnMerge" Version="0.5.15">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>    
  </ItemGroup>

.....

</Project>
```

## Make Sure Dependency DLLs are also Built

Add `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` to your property group.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Azure.Identity" Version="1.10.0" />
    <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.35.3" />
    <PackageReference Include="PowerShellStandard.Library" Version="7.0.0-preview.1" />
  </ItemGroup>

</Project>

```

## Build Project

Make sure you build `Release` so that `dnMerge` can pack all references to one DLL.

```
dotnet build --configuration Release
```

## Import Module Locally

```powershell
Import-Module PwshAzCosmosDB\bin\Release\net7.0\PwshAzCosmosDB.dll
```

## Publish to PS Gallery

* Create `output\PwshAzCosmosDB` directory
* Copy `PwshAzCosmosDB.dll` and `PwshAzCosmosDB.psd1` files to `output\PwshAzCosmosDB`
* Switch directory tp `output` directory
* Publish module to PSGallery with the following commands:

```powershell
Publish-Module -Name .\output\PwshAzCosmosDB\ -NuGetApiKey XXXXXX -verbose -Debug
```