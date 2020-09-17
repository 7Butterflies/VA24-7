using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
                b2CObjectId = claims.Claims.First(claim => claim.Type == "oid").Value,
                email = claims.Claims.First(claim => claim.Type == "emails").Value,
                fullName = claims.Claims.First(claim => claim.Type == "name").Value,
                surname = claims.Claims.First(claim => claim.Type == "family_name").Value,
                country = claims.Claims.First(claim => claim.Type == "country").Value,
                city = claims.Claims.First(claim => claim.Type == "city").Value,
                role = claims.Claims.First(claim => claim.Type == "extension_Role").Value,
            };

            var partitionKey = $"partitionKey-person-useraccounts";
            var documentId = person.b2CObjectId;

            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(partitionKey) };

            var queryableDocument = documentClient.CreateDocumentQuery(collectionUri, feedOptions)
                .Where(t => t.Id == documentId)
                .AsEnumerable()
                .FirstOrDefault();

            JObject document;

            if (queryableDocument == null)
            {
                person.id = documentId;

                document = JObject.FromObject(person);
                document.Add("partitionKey", partitionKey);

                var requestOptions = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };

                await documentClient.UpsertDocumentAsync(collectionUri, document, requestOptions);
            }
            else
            {
                document = JObject.Parse(queryableDocument.ToString());
            }

            return document;
        }

        [FunctionName(nameof(AF_GET_PERSON))]
        public static async Task<dynamic> AF_GET_PERSON(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "person/{id?}")]HttpRequestMessage req, string id,
[CosmosDB(
                databaseName: "VA24-7-DB",
                collectionName: "collection",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
ILogger log)
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("VA24-7-DB", "collection");

            var claims = Security.Security.GetClaims(req);

            var personPartitionKey = $"partitionKey-person-useraccounts";

            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(personPartitionKey) };
            var persons = new List<Person>();
            if (string.IsNullOrEmpty(id))
                persons = documentClient.CreateDocumentQuery<Person>(collectionUri, feedOptions)
                    .AsEnumerable()
                    .ToList();
            else
                persons = documentClient.CreateDocumentQuery<Person>(collectionUri, feedOptions)
                    .Where(x => x.id == id)
                    .AsEnumerable()
                    .ToList();

            return persons;
        }

        [FunctionName(nameof(AF_UPSERT_MEMBERSHIP))]
        public static async Task<dynamic> AF_UPSERT_MEMBERSHIP(
[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "membership/{person1Id}/{person2Id}")]HttpRequestMessage req, string person1Id, string person2Id,
[CosmosDB(
                databaseName: "VA24-7-DB",
                collectionName: "collection",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
ILogger log)
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("VA24-7-DB", "collection");

            var claims = Security.Security.GetClaims(req);

            dynamic str = Task.Run(async () => { return await req.Content.ReadAsStringAsync(); }).Result;
            var json = JsonConvert.DeserializeObject<dynamic>(str as string);

            var membershipPartitionKey = $"partitionKey-person-membership";

            var personIds = new[] { person1Id, person2Id };

            var prescriptions = json.prescriptions as List<Prescription>;

            prescriptions.Select(x => x.id = Guid.NewGuid().ToString());

            var membership = new Membership()
            {
                id = $"membership-{Guid.NewGuid().ToString()}",
                isActive = true,
                createDateTime = DateTime.Now.ToString(),
                prescriptions = prescriptions
            };

            await documentClient.UpsertDocumentAsync(collectionUri, membership, new RequestOptions { PartitionKey = new PartitionKey(membershipPartitionKey) });

            var personPartitionKey = "partitionKey-person-membership";
            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(personPartitionKey) };
            var persons = documentClient.CreateDocumentQuery<Person>(collectionUri, feedOptions)
                .Where(x => personIds.Contains(x.id))
                .AsEnumerable()
                .ToList();

            persons.Where(x => x.role == Role.Patient.ToString()).FirstOrDefault().patientToDoctorMemberships.Add(membership.id);
            persons.Where(x => x.role == Role.Physician.ToString()).FirstOrDefault().patientToDoctorMemberships.Add(membership.id);

            foreach (var person in persons)
            {
                await documentClient.UpsertDocumentAsync(collectionUri, person, new RequestOptions { PartitionKey = new PartitionKey(personPartitionKey) });
            }

            return persons;
        }
    }
}