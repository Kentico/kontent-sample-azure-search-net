using Kontent_Azure_Search_Demo.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Index = Microsoft.Azure.Search.Models.Index;

namespace Kontent_Azure_Search_Demo.Helpers
{
    public class SearchHelper
    {
        public static void AddToIndex<T>(ISearchIndexClient indexClient, IEnumerable<T> documents)
        {
            var actions = documents.Select(a => IndexAction.Upload(a));
            var batch = IndexBatch.New(actions);
            indexClient.Documents.Index(batch);
        }

        public static void CreateIndex<T>(string indexName, SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<T>()
            };

            serviceClient.Indexes.Create(definition);
        }

        public static SearchServiceClient CreateSearchServiceClient(IConfiguration configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            return serviceClient;
        }

        public static void DeleteIndexIfExists(string indexName, SearchServiceClient serviceClient)
        {
            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }
        }

        public static DocumentSearchResult<T> QueryIndex<T>(ISearchIndexClient indexClient, string searchText)
        {
            var parameters = new SearchParameters();
            return indexClient.Documents.Search<T>(searchText, parameters);
        }
        
        public static void RemoveFromIndex<T>(ISearchIndexClient indexClient, IEnumerable<T> documents)
        {
            var actions = documents.Select(a => IndexAction.Delete(a));
            var batch = IndexBatch.New(actions);
            indexClient.Documents.Index(batch);
        }
    }
}
