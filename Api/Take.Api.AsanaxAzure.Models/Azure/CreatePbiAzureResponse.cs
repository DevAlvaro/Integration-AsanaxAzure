using System;

using Newtonsoft.Json;

namespace Take.Api.AsanaxAzure.Models.Azure
{
    public class CreatePbiAzureResponse
    {
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("resource")]
        public Resource Resource { get; set; }

        public CreatePbiAzureResponse()
        {
            EventType = "";
            Resource = new Resource();
        }
    }

    public class Resource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }

        public Resource()
        {
            Id = 0;
            Fields = new Fields();
        }
    }
    public class Fields
    {
        [JsonProperty("System.Title")]
        public string Title { get; set; }

        [JsonProperty("System.State")]
        public string State { get; set; }

        [JsonProperty("System.WorkItemType")]
        public string ItemType { get; set; }

        [JsonProperty("Custom.ClienteCS")]
        public string ClienteCS { get; set; }

        [JsonProperty("Custom.Asana")]
        public string Asana { get; set; }

        [JsonProperty("System.Description")]
        public string Description { get; set; }

        [JsonProperty("System.History")]
        public string History { get; set; }

        [JsonProperty("System.Tags")]
        public string Tags { get; set; }

        [JsonProperty("System.AssignedTo")]
        public string AssignedTo { get; set; }

        public Fields()
        {
            Title = "";
            State = "";
            ItemType = "";
            ClienteCS = "";
            Description = "";
            Tags = "";
            AssignedTo = "";
            History = "";
        }
    }
}