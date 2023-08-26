using System.Management.Automation;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace PwshAzCosmosDB
{
    [Cmdlet(VerbsCommon.Get, "AzCosmosDBDocument")]
    [OutputType(typeof(JObject))]
    public class GetAzCosmosDBDocument : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string DocumentId { get; set; }

        [Parameter(Mandatory = false)]
        public string PartitionKey { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the Cosmos container from session state
            var container = SessionState.PSVariable.Get("AzCosmosDBContainer").Value as Container;
            if (container == null)
            {
                WriteVerbose("[+] Container not found in session state.");
                ThrowTerminatingError(new ErrorRecord(
                    new PSInvalidOperationException("Container not found in session state."),
                    "ContainerNotFound", ErrorCategory.ResourceUnavailable, null));
            }

            WriteVerbose("[+] Successfully retrieved the Cosmos container from session state.");
            WriteVerbose($"[+] Container Name: {container.Id}");
            WriteVerbose($"[+] Database Name: {container.Database.Id}");

            try
            {
                var partitionKeyValue = string.IsNullOrEmpty(PartitionKey) ? DocumentId : PartitionKey;
                var partitionKey = new PartitionKey(partitionKeyValue);

                WriteVerbose("[+] Retrieving the document...");

                // Retrieve the document by its ID and partition key
                var response = container.ReadItemAsync<object>(DocumentId, partitionKey).GetAwaiter().GetResult();
                var documentResponse = response;

                WriteVerbose("[+] Successfully retrieved the document.");

                // Now you can safely write to the pipeline
                // https://github.com/PowerShell/PowerShell/issues/10650
                WriteObject(documentResponse.Resource.ToString());
            }
            catch (CosmosException ex)
            {
                WriteError(new ErrorRecord(ex, "CosmosGetDocumentError", ErrorCategory.ReadError, this));
            }
        }
    }
}