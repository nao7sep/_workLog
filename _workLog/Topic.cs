using System.Text.Json.Serialization;

namespace _workLog
{
    public class Topic
    {
        public Guid Id { get; init; }

        public DateTime CreatedAtUtc { get; init; }

        public required string Content { get; set; } // May be updated later.

        [JsonIgnore]
        // Messages wont be serialized into topics' JSON files.
        public List <Message> Messages { get; init; } = [];
    }
}
