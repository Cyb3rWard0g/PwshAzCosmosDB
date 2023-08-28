using System.Collections;
using System.Management.Automation;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace PwshAzCosmosDB
{
    [Cmdlet(VerbsData.Update, "AzCosmosDBDocument")]
    [OutputType(typeof(JObject))]
    public class UpdateAzCosmosDBDocument : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string DocumentId { get; set; }

        [Parameter(Mandatory = true)]
        public Hashtable Updates { get; set; }

        [Parameter(Mandatory = true)]
        public string PartitionKeyField { get; set; }

        [Parameter(Mandatory = true)]
        public string PartitionKeyValue { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("[+] Updating input updates with Id and partition context...");
            // Add the "id" property to Updates hashtable if it doesn't exist
            if (!Updates.ContainsKey("id"))
            {
                Updates.Add("id", DocumentId);
            }

            // Add partition key field and value to Updates hashtable if provided
            if (!Updates.ContainsKey(PartitionKeyField)){
                Updates.Add(PartitionKeyField, PartitionKeyValue);
            }

            // Retrieve the Cosmos container from session state
            WriteVerbose("[+] Retrieving the Cosmos container from session state...");
            if (SessionState.PSVariable.Get("AzCosmosDBContainer").Value is Container container)
            {
                // Create a PartitionKey object based on the provided or default value
                var partitionKey = new PartitionKey(PartitionKeyValue);

                // Retrieve the document by its ID and partition key
                var response = container.ReadItemAsync<object>(DocumentId, partitionKey).GetAwaiter().GetResult();
                var documentResponse = response;

                WriteVerbose("[+] Successfully retrieved existing document.");
                // Parse the JSON string into a JObject
                // https://github.com/PowerShell/PowerShell/issues/10650
                var documentJObject = JObject.Parse(documentResponse.Resource.ToString());

                // Convert the JObject to a dictionary
                var documentDictionary = documentJObject.ToObject<Dictionary<string, object>>();

                WriteVerbose("[+] Updating document locally ..");
                foreach (var key in Updates.Keys)
                {
                    var propertyName = key.ToString();
                    
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        if (documentDictionary.ContainsKey(propertyName))
                        {
                            documentDictionary[propertyName] = Updates[key];
                        }
                        else
                        {
                            documentDictionary.Add(propertyName, Updates[key]);
                        }
                    }
                }

                WriteVerbose($"[+] Updating document with ID: {DocumentId}...");
                try
                {
                    var updateResponse = container.ReplaceItemAsync(documentDictionary, DocumentId, partitionKey).GetAwaiter().GetResult();
                    var updatedDocument = updateResponse.Resource;

                    WriteVerbose("[+] Document updated successfully.");

                    // Return the updated document
                    WriteObject(updatedDocument);
                }
                catch (CosmosException ex)
                {
                    WriteError(new ErrorRecord(ex, "CosmosUpdateDocumentError", ErrorCategory.WriteError, this));
                }
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new PSInvalidOperationException("Container not found in session state."),
                    "ContainerNotFound", ErrorCategory.ResourceUnavailable, null));
            }
        }
    }
}