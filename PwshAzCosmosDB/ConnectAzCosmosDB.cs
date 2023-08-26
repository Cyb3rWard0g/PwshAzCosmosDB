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
        public string MasterKey { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                WriteVerbose("[+] Attempting to connect to Azure Cosmos DB container...");

                CosmosClient cosmosClient;

                if (string.IsNullOrEmpty(MasterKey))
                {
                    // Use managed identity authentication
                    WriteVerbose("[+] Using managed identity authentication...");
                    var managedIdentityCredential = new DefaultAzureCredential();
                    cosmosClient = new CosmosClient(Endpoint, managedIdentityCredential);
                }
                else
                {
                    // Use master key authentication
                    WriteVerbose("[+] Using master key authentication...");
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