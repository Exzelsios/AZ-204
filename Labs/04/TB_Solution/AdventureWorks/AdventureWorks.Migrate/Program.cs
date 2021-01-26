using System;
using AdventureWorks.Context;
using AdventureWorks.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureWorks.Migrate
{
    class Program
    {
        private const string sqlDBConnectionString = "Server=tcp:polysqlsrvrtb.database.windows.net,1433;Initial Catalog=AdventureWorks;Persist Security Info=False;User ID=testuser;Password=TestPa55w.rd.;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private const string cosmosDBConnectionString = "AccountEndpoint=https://tbcosmosdb.documents.azure.com:443/;AccountKey=6PtCOnIyDdyXN3G6ishyKPadroksK7uohxh45C2LRshivGMStQvnvLEtOGuRMby8veTkUHd3shuPPJHorc5WOA==;";
        static async Task Main(string[] args)
        {
            await Console.Out.WriteLineAsync("Hello World!");

            using AdventureWorksSqlContext context = new AdventureWorksSqlContext(sqlDBConnectionString);

            List<Model> items = await context.Models.Include(m => m.Products).ToListAsync<Model>();

            await Console.Out.WriteLineAsync($"Total: {items.Count}");

            using CosmosClient client = new CosmosClient(cosmosDBConnectionString);

            Database database = await client.CreateDatabaseIfNotExistsAsync("Retail");

            Container container = await database.CreateContainerIfNotExistsAsync("Online", partitionKeyPath: $"/{nameof(Model.Category)}");

            int count = 0; 
            foreach (var item in items)
            {
    ItemResponse<Model> document = await container.UpsertItemAsync<Model>(item);
    await Console.Out.WriteLineAsync($"Upserted document #{++count:000} [Activity Id: {document.ActivityId}]");
}

await Console.Out.WriteLineAsync($"Total Azure Cosmos DB Documents: {count}");
        }
    }
}
