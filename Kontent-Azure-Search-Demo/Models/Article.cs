using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Kontent_Azure_Search_Demo.Models
{
    public class Article
    {
        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnMicrosoft), IsRetrievable(false)]
        public string Body { get; set; }

        [Key]
        public string ID { get; set; }

        public DateTime? PostDate { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnMicrosoft)]
        public string Summary { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnMicrosoft)]
        public string Title { get; set; }

        public string UrlPattern { get; set; }
    }
}
