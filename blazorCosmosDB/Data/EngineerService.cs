using Azure;
using Microsoft.Azure.Cosmos;
using System.Configuration;

namespace blazorCosmosDB.Data
{
    public class EngineerService : IEngineerService
    {

        private readonly string _connectionString;
        private readonly string CosmosDbConnectionString;
        private readonly string CosmosDbName;
        private readonly string CosmosDbContainerName;


        public EngineerService(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString"];

            // Assign the rest here, using the available configuration or constants
            CosmosDbConnectionString = _connectionString;
            CosmosDbName = "Contractors";
            CosmosDbContainerName = "Engineers";

        }
        
        private Container GetContainerClient()
        {
            var cosmosDbClient = new CosmosClient(CosmosDbConnectionString);
            var container = cosmosDbClient.GetContainer(CosmosDbName, CosmosDbContainerName);
            return container;
        }

        public async Task UpsertEngineer(Engineer engineer)
        {
            try
            {
                if(engineer.id == null)
                {
                    engineer.id = Guid.NewGuid();
                }
                var container = GetContainerClient();
                var updateRes = await container.UpsertItemAsync(engineer, new PartitionKey(engineer.id.ToString()));
                Console.Write(updateRes.StatusCode);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception", ex);
            }
        }

        public async Task DeleteEngineer(string? id, string? partitionKey)
        {
            try
            {
                var container = GetContainerClient();
                var response = await container.DeleteItemAsync<Engineer>(id, new PartitionKey(partitionKey));
            }
            catch (Exception ex)
            {
                throw new Exception("Exception", ex);
            }
        }

        public async Task<List<Engineer>> GetEngineerDetails()
        {
            List<Engineer> engineers = new List<Engineer>();
            try
            {
                var container = GetContainerClient();
                var sqlQuery = "SELECT * FROM c";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Engineer> queryResultSetIterator = container.GetItemQueryIterator<Engineer>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Engineer> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Engineer engineer in currentResultSet)
                    {
                        engineers.Add(engineer);
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ToString();
            }
            return engineers;
        }

        public async Task<Engineer> GetEngineerDetailsById(string? id, string? partitionKey)
        {
            try
            {
                var container = GetContainerClient();
                ItemResponse<Engineer> response = await container.ReadItemAsync<Engineer>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (Exception ex)
            {

                throw new Exception("Exception ", ex);
            }
        }
    }
}
