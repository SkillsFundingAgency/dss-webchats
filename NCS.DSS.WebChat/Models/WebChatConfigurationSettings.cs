namespace NCS.DSS.WebChat.Models
{
    public class WebChatConfigurationSettings
    {
        public string AccessKey { get; set; }
        public string BaseAddress { get; set; }
        public string CollectionId { get; set; }
        public string CustomerCollectionId { get; set; }
        public string CustomerDatabaseId { get; set; }
        public string DatabaseId { get; set; }
        public string CosmosDbEndpoint { get; set; }
        public string EnvironmentName { get; set; }
        public string InteractionCollectionId { get; set; }
        public string InteractionDatabaseId { get; set; }
        public string Key { get; set; }
        public string KeyName { get; set; }
        public string QueueName { get; set; }
    }
}
