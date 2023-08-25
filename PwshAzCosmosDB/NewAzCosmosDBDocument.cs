using System.Collections;
using System.Management.Automation;
using Microsoft.Azure.Cosmos;

namespace PwshAzCosmosDB
{
    [Cmdlet(VerbsCommon.New, "AzCosmosDBDocument")]
    public class NewAzCosmosDBDocument : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Hashtable? Document { get; set; }

        [Parameter(Mandatory = false)]
        public string? PartitionKey { get; set; }

        [Parameter(Mandatory = false)]
        public string? DocumentId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                // Retrieve the Cosmos container from session state
                var container = SessionState.PSVariable.Get("AzCosmosDBContainer").Value as Container;
                if (container == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new PSInvalidOperationException("Container not found in session state."),
                        "ContainerNotFound", ErrorCategory.ResourceUnavailable, null));
                }

                // Create a PartitionKey object based on the provided or default value
                var partitionKeyValue = string.IsNullOrEmpty(PartitionKey) ? Document?.ToString() : PartitionKey;
                var partitionKey = new PartitionKey(partitionKeyValue);

                // Create the new document in the container
                Hashtable? documentToCreate = null;
                if (Document != null)
                {
                    documentToCreate = new Hashtable(Document);
                    if (!string.IsNullOrEmpty(DocumentId))
                    {
                        documentToCreate["id"] = DocumentId;
                    }
                }

                var createResponse = container.CreateItemAsync(documentToCreate, partitionKey);
                if (createResponse.Result.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    WriteVerbose("Document created successfully.");
                }
                else
                {
                    WriteWarning("Failed to create document.");
                }
            }
            catch (CosmosException ex)
            {
                WriteError(new ErrorRecord(ex, "CosmosCreateDocumentError", ErrorCategory.WriteError, this));
            }
        }
    }
}