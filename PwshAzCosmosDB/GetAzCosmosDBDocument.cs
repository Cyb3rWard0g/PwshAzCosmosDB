using System.Management.Automation;
using Microsoft.Azure.Cosmos;

namespace PwshAzCosmosDB
{
    [Cmdlet(VerbsCommon.Get, "AzCosmosDBDocument")]
    [OutputType(typeof(object))] // Change object to your document type
    public class GetAzCosmosDBDocument : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string? DocumentId { get; set; }

        [Parameter(Mandatory = false)]
        public string? PartitionKey { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the Cosmos container from session state
            var container = SessionState.PSVariable.Get("AzCosmosDBContainer").Value as Container;
            if (container == null)
            {
                ThrowTerminatingError(new ErrorRecord(new PSInvalidOperationException("Container not found in session state."),
                    "ContainerNotFound", ErrorCategory.ResourceUnavailable, null));
            }

            try
            {
                // Create a PartitionKey object based on the provided or default value
                var partitionKeyValue = string.IsNullOrEmpty(PartitionKey) ? DocumentId : PartitionKey;
                var partitionKey = new PartitionKey(partitionKeyValue);

                // Retrieve the document by its ID and partition key
                var response = container.ReadItemAsync<object>(DocumentId, partitionKey);
                var document = response.Result;

                // Return the retrieved document
                WriteObject(document);
            }
            catch (CosmosException ex)
            {
                WriteError(new ErrorRecord(ex, "CosmosGetDocumentError", ErrorCategory.ReadError, this));
            }
        }
    }
}