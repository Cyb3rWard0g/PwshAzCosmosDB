using System;
using System.Management.Automation;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    // Check for master key environment variable
                    var environmentMasterKey = Environment.GetEnvironmentVariable("COSMOSDB_MASTER_KEY");
                    
                    if (!string.IsNullOrEmpty(environmentMasterKey))
                    {
                        WriteVerbose("[+] Using master key from environment variable...");
                        cosmosClient = new CosmosClient(Endpoint, environmentMasterKey);
                    }
                    else
                    {

                        // Use managed identity authentication
                        WriteVerbose("[+] Using managed identity authentication...");

                        string primaryKey;
                        string accessToken;
                        string resource = "https://management.azure.com";
                        string apiVersion = "2019-08-01";

                        var azcosmosDBResourceId = Environment.GetEnvironmentVariable("COSMOS_RESOURCE_ID");
                        var managedIdentityPrincipalId = Environment.GetEnvironmentVariable("MANAGED_IDENTITY_PRINCIPAL_ID");
                        var identityEndpoint = Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT");
                        var identityHeader = Environment.GetEnvironmentVariable("IDENTITY_HEADER");

                        WriteVerbose($"[+] User Assigned Managed Identity Principal Id: {managedIdentityPrincipalId}");

                        try
                        {
                            // Retrieve access token
                            var requestURI = $"{identityEndpoint}?resource={resource}&api-version={apiVersion}&principal_id={managedIdentityPrincipalId}";
                            WebRequest request = WebRequest.Create(requestURI);
                            request.Headers["X-IDENTITY-HEADER"] = identityHeader;
                            request.Method = "GET";

                            WriteVerbose("[+] Retrieving access token...");
                            using (WebResponse response = request.GetResponse())
                            {
                                using (StreamReader streamResponse = new StreamReader(response.GetResponseStream()))
                                {
                                    string stringResponse = streamResponse.ReadToEnd();

                                    if ((int)((HttpWebResponse)response).StatusCode >= 400)
                                    {
                                        throw new Exception(stringResponse);
                                    }

                                    Dictionary<string, string> oauthResults = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringResponse);
                                    accessToken = oauthResults["access_token"];
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Acquire token failed: {e.Message}");
                        }

                        try
                        {
                            // Now use the access token to retrieve keys
                            string url = $"https://management.azure.com/{azcosmosDBResourceId}/listKeys?api-version=2020-04-01";

                            WebRequest request = WebRequest.Create(url);
                            request.Method = "POST";
                            request.Headers["Authorization"] = $"Bearer {accessToken}";
                            request.Headers["Accept"] = "application/json";

                            WriteVerbose("[+] Retrieving Cosmos Primary Master Key...");
                            using (WebResponse response = request.GetResponse())
                            {
                                using (StreamReader streamResponse = new StreamReader(response.GetResponseStream()))
                                {
                                    string stringResponse = streamResponse.ReadToEnd();

                                    if ((int)((HttpWebResponse)response).StatusCode == 200)
                                    {
                                        JObject responseObject = JObject.Parse(stringResponse);
                                        primaryKey = responseObject["primaryMasterKey"].ToString();
                                    }
                                    else
                                    {
                                        throw new Exception($"Failed to retrieve Cosmos DB keys: {((HttpWebResponse)response).StatusCode}");
                                    }
                                }
                            }
                            
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Master Key Retrieval failed: {e.Message}");
                        }

                        WriteVerbose("[+] Initializing CosmosClient...");
                        cosmosClient = new CosmosClient(Endpoint, primaryKey);
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