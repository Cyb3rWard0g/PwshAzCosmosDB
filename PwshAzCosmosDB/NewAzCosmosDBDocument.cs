using System.Collections;
using System.Management.Automation;
using Microsoft.Azure.Cosmos;

namespace PwshAzCosmosDB
{
    [Cmdlet(VerbsCommon.New, "AzCosmosDBDocument")]
    public class NewAzCosmosDBDocument : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Hashtable Document { get; set; }

        [Parameter(Mandatory = true)]
        public string PartitionKeyField { get; set; }  // Field name of the partition key

        [Parameter(Mandatory = true)]
        public string PartitionKeyValue { get; set; }  // Value of the partition key

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

                WriteVerbose($"[+] Creating the new document in the container with PartitionKeyField '{PartitionKeyField}'...");

                // Create the new document in the container
                var documentToCreate = new Hashtable(Document);
                documentToCreate[PartitionKeyField] = PartitionKeyValue;

                var createResponse = container.CreateItemAsync(documentToCreate, new PartitionKey(PartitionKeyValue)).GetAwaiter().GetResult();
                if (createResponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    WriteVerbose("[+] Document created successfully.");
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