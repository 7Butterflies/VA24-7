using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
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

        [FunctionName("AF_UPSERT_PERSON_TO_IOTDEVICE_MEMBERSHIP")]
        public static async Task<IActionResult> AF_UPSERT_PERSON_TO_IOTDEVICE_MEMBERSHIP(
 [HttpTrigger(AuthorizationLevel.Function, "post", Route = "person/{personId}/device/{deviceId}")] HttpRequestMessage req, string personId, string deviceId,
 [CosmosDB(
                databaseName: "VA24-7-DB",
                collectionName: "collection",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
 ILogger log)
        {
            try
            {
                var registryManager = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTHubOwnerConnectionString"));
                var query = registryManager.CreateQuery($"select * from devices where deviceId = '{deviceId}'");
                var deviceTwins = await query.GetNextAsTwinAsync();


                if (deviceTwins.Any())
                {
                    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("VA24-7-DB", "collection");

                    var personPartitionKey = $"partitionKey-person-useraccounts";

                    var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new PartitionKey(personPartitionKey) };

                    var existingDeviceAssociations = documentClient.CreateDocumentQuery<Person>(collectionUri, feedOptions)
                      .Where(x => x.device.deviceId == deviceId && x.device.associationStatus == RecordStatus.Active.ToString())
                     .AsEnumerable()
                     .ToList();

                    if (!existingDeviceAssociations.Any())
                    {
                        var person = documentClient.CreateDocumentQuery<Document>(collectionUri, feedOptions)
                            .Where(x => x.Id == personId)
                           .AsEnumerable()
                           .FirstOrDefault();

                        var device = new IoTDevice();

                        device = new IoTDevice() { deviceId = deviceId, associationStatus = RecordStatus.Active.ToString() };

                        person.SetPropertyValue("device", device);

                        await documentClient.ReplaceDocumentAsync(person, new RequestOptions { PartitionKey = new PartitionKey(personPartitionKey) });

                        return new OkObjectResult(person);
                    }
                    else
                    {
                        return new BadRequestObjectResult("This device has already been registered.");
                    }
                }
                else
                {
                    return new NotFoundObjectResult("No devices found");
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}