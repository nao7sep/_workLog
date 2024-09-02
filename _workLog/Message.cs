namespace _workLog
{
    public class Message
    {
        public Guid Id { get; init; }

        public Guid TopicId { get; init; }

        public DateTime CreatedAtUtc { get; init; }

        public required string Content { get; set; } // May be updated later.

        // Attachments will be serialized into messages' JSON files.
        public List <Attachment> Attachments { get; init; } = [];
    }
}
