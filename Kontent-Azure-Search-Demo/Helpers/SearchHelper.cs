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
        private readonly string indexName;
        private readonly ISearchIndexClient searchIndexClient;
        private readonly SearchServiceClient searchServiceClient;
        
        public SearchHelper(IConfiguration configuration)
        {
            indexName = configuration["SearchIndexName"];
            searchServiceClient = CreateSearchServiceClient(configuration);
            searchIndexClient = searchServiceClient.Indexes.GetClient(indexName);
        }

        public void AddToIndex<T>(IEnumerable<T> documents)
        {
            var actions = documents.Select(a => IndexAction.Upload(a));
            var batch = IndexBatch.New(actions);
            searchIndexClient.Documents.Index(batch);
        }

        public void CreateIndex<T>()
        {
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<T>()
            };

            searchServiceClient.Indexes.Create(definition);
        }

        public void DeleteIndexIfExists()
        {
            if (searchServiceClient.Indexes.Exists(indexName))
            {
                searchServiceClient.Indexes.Delete(indexName);
            }
        }

        public IEnumerable<T> QueryIndex<T>(string searchText)
        {
            var parameters = new SearchParameters();
            var results = searchIndexClient.Documents.Search<T>(searchText, parameters);
            return results.Results.Select(r=>r.Document);
        }

        public void RemoveFromIndex<T>(IEnumerable<T> documents)
        {
            var actions = documents.Select(a => IndexAction.Delete(a));
            var batch = IndexBatch.New(actions);
            searchIndexClient.Documents.Index(batch);
        }

        private SearchServiceClient CreateSearchServiceClient(IConfiguration configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            return new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
        }
    }
}
