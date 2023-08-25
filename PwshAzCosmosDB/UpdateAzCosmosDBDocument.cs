using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Cosmos;

namespace PwshAzCosmosDB
{
    [Cmdlet(VerbsData.Update, "AzCosmosDBDocument")]
    [OutputType(typeof(object))] // Change object to your document type
    public class UpdateAzCosmosDBDocument : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string? DocumentId { get; set; }

        [Parameter(Mandatory = true)]
        public Hashtable? Updates { get; set; } // Hashtable of property updates

        [Parameter(Mandatory = false)]
        public string? PartitionKey { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                // Retrieve the existing document using Get-AzCosmosDBDocument cmdlet
                var getDocumentCmdlet = new GetAzCosmosDBDocument
                {
                    DocumentId = DocumentId,
                    PartitionKey = PartitionKey
                };
                var existingDocuments = getDocumentCmdlet.Invoke<object>();

                var existingDocument = existingDocuments.SingleOrDefault();

                if (existingDocument == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new PSInvalidOperationException("Document not found."),
                        "DocumentNotFound", ErrorCategory.ObjectNotFound, null));
                }

                // Apply updates to the existing document
                if (Updates != null)
                {
                    foreach (var key in Updates.Keys)
                    {
                        var propertyName = key.ToString();
                        
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            var property = existingDocument.GetType().GetProperty(propertyName);
                            if (property != null)
                            {
                                property.SetValue(existingDocument, Updates[key]);
                            }
                            else
                            {
                                // Handle property not found case
                                WriteWarning($"Property '{propertyName}' not found in the document.");
                            }
                        }
                    }
                }

                // Retrieve the Cosmos container from session state
                var container = SessionState.PSVariable.Get("AzCosmosDBContainer").Value as Container;
                if (container == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new PSInvalidOperationException("Container not found in session state."),
                        "ContainerNotFound", ErrorCategory.ResourceUnavailable, null));
                }

                // Create a PartitionKey object based on the provided or default value
                var partitionKeyValue = string.IsNullOrEmpty(PartitionKey) ? DocumentId : PartitionKey;
                var partitionKey = new PartitionKey(partitionKeyValue);

                // Update the document in the container
                var updateResponse = container.ReplaceItemAsync(existingDocument, DocumentId, partitionKey);
                var updatedDocument = updateResponse.Result.Resource;

                // Return the updated document
                WriteObject(updatedDocument);
            }
            catch (CosmosException ex)
            {
                WriteError(new ErrorRecord(ex, "CosmosUpdateDocumentError", ErrorCategory.WriteError, this));
            }
        }
    }
}