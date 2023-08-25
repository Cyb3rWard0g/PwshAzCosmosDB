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
        public string? Endpoint { get; set; }

        [Parameter(Mandatory = true)]
        public string? DatabaseName { get; set; }

        [Parameter(Mandatory = true)]
        public string? ContainerName { get; set; }

        [Parameter(Mandatory = false)]
        public string? MasterKey { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                CosmosClient cosmosClient;

                if (string.IsNullOrEmpty(MasterKey))
                {
                    // Use managed identity authentication
                    var managedIdentityCredential = new DefaultAzureCredential();
                    cosmosClient = new CosmosClient(Endpoint, managedIdentityCredential);
                }
                else
                {
                    // Use master key authentication
                    cosmosClient = new CosmosClient(Endpoint, MasterKey);
                }

                // Get the specific container
                var container = cosmosClient.GetContainer(DatabaseName, ContainerName);

                // Store the container in session state for later cmdlets to access
                SessionState.PSVariable.Set("AzCosmosDBContainer", container);

                // Provide feedback to the user
                WriteInformation("Successfully connected to the Azure Cosmos DB container.", 
                    new string[] { "AzCosmosDBContainerConnected" });
            }
            catch (CosmosException ex)
            {
                WriteError(new ErrorRecord(ex, "CosmosConnectionError", ErrorCategory.ConnectionError, this));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ConnectAzCosmosDBError", ErrorCategory.NotSpecified, this));
            }
        }
    }
}