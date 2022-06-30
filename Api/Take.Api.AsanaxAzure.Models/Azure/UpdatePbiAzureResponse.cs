using System;

using Newtonsoft.Json;

namespace Take.Api.AsanaxAzure.Models.Azure
{
    public class UpdatePbiAzureResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("resource")]
        public ResourceUpdate ResourceUpdate { get; set; }
    }

    public class ResourceUpdate
    {
        [JsonProperty("fields")]
        public RevisionFields RevisionFields { get; set; }

        [JsonProperty("relations")]
        public Relations Relations { get; set; }

        [JsonProperty("revision")]
        public Revision Revision { get; set; }

        public ResourceUpdate()
        {
            RevisionFields = new RevisionFields();
            Relations = new Relations();
            Revision = new Revision();
        }
    }

    public class RevisionFields
    {
        [JsonProperty("System.WorkItemType")]
        public RevisionWorkItemType RevisionWorkItemType { get; set; }

        public RevisionFields()
        {
            RevisionWorkItemType = new RevisionWorkItemType();
        }
    }

    public class Relations
    {
        [JsonProperty("added")]
        public AddedRelations[] AddedRelations { get; set; }
    }

    public class AddedRelations
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        public AddedRelations()
        {
            Url = "";
        }
    }

    public class RevisionWorkItemType
    {
        [JsonProperty("oldValue")]
        public string oldValue { get; set; }

        [JsonProperty("newValue")]
        public string newValue { get; set; }

        public RevisionWorkItemType()
        {
            oldValue = "";
            newValue = "";
        }
    }

    public class Revision
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }

        [JsonProperty("commentVersionRef")]
        public Comment Comment { get; set; }
    }

    public class Comment
    {
        [JsonProperty("commentId")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        public Comment()
        {
            Id = "";
            Url = "";
        }
    }

}