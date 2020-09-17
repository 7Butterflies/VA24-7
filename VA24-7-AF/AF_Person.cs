using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using VA24_7_Shared.Model;

namespace VA24_7_AF
{
    public static class AF_Person
    {
        [FunctionName(nameof(AF_ADD_PERSON))]
        public static async Task<dynamic> AF_ADD_PERSON(
[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "person")]HttpRequestMessage req,
[CosmosDB(
                databaseName: "VA24-7-DB",
                collectionName: "collection",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
ILogger log)
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("VA24-7-DB", "collection");

            var claims = Security.Security.GetClaims(req);

            var person = new Person()
            {
                B2CObjectId = claims.Claims.First(claim => claim.Type == "oid").Value,
                Email = claims.Claims.First(claim => claim.Type == "emails").Value,
                FullName = claims.Claims.First(claim => claim.Type == "name").Value,
                Surname = claims.Claims.First(claim => claim.Type == "family_name").Value,
                Country = claims.Claims.First(claim => claim.Type == "country").Value,
                City = claims.Claims.First(claim => claim.Type == "city").Value,
                Role = claims.Claims.First(claim => claim.Type == "extension_Role").Value,
                PersonId = Guid.NewGuid()
            };

            var partitionKey = $"partitionKey-person-useraccounts";
            var documentId = $"person-{ person.Email }-{ person.B2CObjectId }";

            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(partitionKey) };

            var queryableDocument = documentClient.CreateDocumentQuery(collectionUri, feedOptions)
                .Where(t => t.Id == documentId)
                .AsEnumerable()
                .FirstOrDefault();

            JObject document;

            if (queryableDocument == null)
            {
                document = new JObject
                {
                    ["person"] = JObject.FromObject(person),
                    ["partitionKey"] = partitionKey,
                    ["id"] = documentId
                };

                var requestOptions = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };

                await documentClient.UpsertDocumentAsync(collectionUri, document, requestOptions);
            }
            else
            {
                document = JObject.Parse(queryableDocument.ToString());
            }

            return document;
        }
    }
}