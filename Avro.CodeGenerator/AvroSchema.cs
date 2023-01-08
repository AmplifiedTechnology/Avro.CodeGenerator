using System.Text.Json.Serialization;

namespace CodeGenerator
{
    public class AvroSchema
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")] 
        public string Name { get;set; }

        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }
    }
}
