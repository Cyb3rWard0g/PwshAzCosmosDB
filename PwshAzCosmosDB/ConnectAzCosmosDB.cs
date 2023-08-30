using System.Management.Automation;
using Microsoft.Azure.Cosmos;
using Azure.Identity;


namespace PwshAzCosmosDB
{
    [Cmdlet(VerbsCommunications.Connect, "AzCosmosDB")]
    [OutputType(typeof(Container))]
    public class ConnectAzCosmosDB : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Endpoint { get; set; }

        [Parameter(Mandatory = true)]
        public string DatabaseName { get; set; }

        [Parameter(Mandatory = true)]
        public string ContainerName { get; set; }

        [Parameter(Mandatory = false)]
        public string? MasterKey { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                WriteVerbose("[+] Attempting to connect to Azure Cosmos DB container...");

                CosmosClient cosmosClient;

                if (string.IsNullOrEmpty(MasterKey))
                {
                    // Check for master key environment variable
                    var environmentMasterKey = Environment.GetEnvironmentVariable("COSMOSDB_MASTER_KEY");
                    
                    if (!string.IsNullOrEmpty(environmentMasterKey))
                    {
                        WriteVerbose("[+] Using master key from environment variable...");
                        cosmosClient = new CosmosClient(Endpoint, environmentMasterKey);
                    }
                    else
                    {
                        var managedIdentityClientId = Environment.GetEnvironmentVariable("MANAGED_IDENTITY_CLIENT_ID");

                        if (string.IsNullOrEmpty(managedIdentityClientId))
                        {
                            // https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet&preserve-view=true#defaultazurecredential
                            WriteVerbose("[+] Using ChainedTokenCredential: AzurePowerShellCredential -> AzureCliCredential -> ManagedIdentityCredential");
                            cosmosClient = new CosmosClient(Endpoint, new ChainedTokenCredential(new AzurePowerShellCredential(), new AzureCliCredential(), new ManagedIdentityCredential()));
                        }
                        else
                        {
                            // Use ManagedIdentityCredential with the provided client ID
                            WriteVerbose($"[+] Using ManagedIdentityCredential with identity: {managedIdentityClientId}");
                            cosmosClient = new CosmosClient(Endpoint, new ManagedIdentityCredential(managedIdentityClientId));
                        }
                    }
                }
                else
                {
                    // Use provided master key authentication
                    WriteVerbose("[+] Using provided master key...");
                    cosmosClient = new CosmosClient(Endpoint, MasterKey);
                }

                // Get the specific container
                WriteVerbose("[+] Retrieving the specific container...");
                var container = cosmosClient.GetContainer(DatabaseName, ContainerName);

                if (container == null)
                {
                    WriteVerbose("[+] No Cosmos container found.");
                }
                else
                {
                    // Print information about the retrieved container
                    WriteVerbose("[+] Retrieved Cosmos container:");
                    WriteVerbose($"[+] Container Name: {container.Id}");
                    WriteVerbose($"[+] Database Name: {container.Database.Id}");

                    // Check if the container is already stored in the session state
                    if (SessionState.PSVariable.Get("AzCosmosDBContainer") == null)
                    {
                        // Store the container in session state for later cmdlets to access
                        WriteVerbose("[+] Storing the container in session state...");
                        SessionState.PSVariable.Set("AzCosmosDBContainer", container);

                        WriteVerbose("[+] Successfully connected to the Azure Cosmos DB container.");
                    }
                    else
                    {
                        WriteVerbose("[+] Container is already stored in session state.");
                    }
                }
            }
            catch (CosmosException ex)
            {
                WriteError(new ErrorRecord(ex, "CosmosConnectionError", ErrorCategory.ConnectionError, this));
            }
        }
    }
}