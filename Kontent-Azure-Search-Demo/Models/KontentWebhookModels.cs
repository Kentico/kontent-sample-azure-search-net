using Newtonsoft.Json;
using System;

namespace Kontent_Azure_Search_Demo.KontentWebhookModels
{
    public class Data
    {
        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("taxonomies")]
        public Taxonomy[] Taxonomies { get; set; }
    }

    public class Item
    {
        [JsonProperty("codename")]
        public string Codename { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Message
    {
        [JsonProperty("api_name")]
        public string ApiName { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("project_id")]
        public Guid ProjectId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Taxonomy
    {
        [JsonProperty("codename")]
        public string Codename { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class Webhook
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }
    }
}
