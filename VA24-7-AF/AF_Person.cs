using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using VA24_7_Shared.Model;

namespace VA24_7_AF
{
    public static class AF_Person
    {
        [FunctionName(nameof(AF_ADD_PERSON))]
        public static dynamic AF_ADD_PERSON(
[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "person")]HttpRequestMessage req,
[CosmosDB(
                databaseName: "VA24-7-DB",
                collectionName: "collection",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
ILogger log)
        {
            dynamic body = Task.Run(async () => { return await req.Content.ReadAsStringAsync(); }).Result;
            var person = JsonConvert.DeserializeObject<Person>(body as string);

            return null;
        }
    }
}
