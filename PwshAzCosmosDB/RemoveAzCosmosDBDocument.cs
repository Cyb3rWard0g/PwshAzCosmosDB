using System;
using System.Management.Automation;
using Microsoft.Azure.Cosmos;

namespace PwshAzCosmosDB
{
    [Cmdlet(VerbsCommon.Remove, "AzCosmosDBDocument")]
    public class RemoveAzCosmosDBDocument : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string DocumentId { get; set; }

        [Parameter(Mandatory = false)]
        public string PartitionKey { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                WriteVerbose("[+] Retrieving the Cosmos container from session state...");
                var container = SessionState.PSVariable.Get("AzCosmosDBContainer").Value as Container;
                if (container == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new PSInvalidOperationException("Container not found in session state."),
                        "ContainerNotFound", ErrorCategory.ResourceUnavailable, null));
                }

                // Create a PartitionKey object based on the provided or default value
                var partitionKeyValue = string.IsNullOrEmpty(PartitionKey) ? DocumentId : PartitionKey;
                var partitionKey = new PartitionKey(partitionKeyValue);

                WriteVerbose("[+] Deleting the document from the container...");

                // Delete the document from the container
                var deleteResponse = container.DeleteItemAsync<object>(DocumentId, partitionKey).GetAwaiter().GetResult();
                if (deleteResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    WriteVerbose("[+] Document deleted successfully.");
                }
                else
                {
                    WriteWarning("Failed to delete document.");
                }
            }
            catch (CosmosException ex)
            {
                WriteError(new ErrorRecord(ex, "CosmosDeleteDocumentError", ErrorCategory.WriteError, this));
            }
        }
    }
}