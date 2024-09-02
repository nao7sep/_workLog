using System.Reflection;

namespace _workLog
{
    public static class Environment
    {
        private static string? _appDirectoryPath;

        public static string AppDirectoryPath => _appDirectoryPath ??= Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location)!;

        public static string MapPath (string relativePath) => Path.Join (AppDirectoryPath, relativePath);

        private static string? _topicsDirectoryPath;

        public static string TopicsDirectoryPath => _topicsDirectoryPath ??= MapPath ("Topics");

        private static string? _messagesDirectoryPath;

        public static string MessagesDirectoryPath => _messagesDirectoryPath ??= MapPath ("Messages");

        private static string? _attachmentsDirectoryPath;

        public static string AttachmentsDirectoryPath => _attachmentsDirectoryPath ??= MapPath ("Attachments");

        public static Attachment Attach (Guid messageId, string path)
        {
            for (int temp = 0; ; temp ++)
            {
                string xRelativePath =
                    temp == 0 ?
                        Path.Join ("Attachments", Path.GetFileName (path)) :
                        Path.Join ("Attachments", temp.ToString (), Path.GetFileName (path));

                string xPath = MapPath (xRelativePath);

                if (File.Exists (xPath) == false)
                {
                    Directory.CreateDirectory (Path.GetDirectoryName (xPath)!);
                    File.Copy (path, xPath);

                    return new Attachment
                    {
                        Id = Guid.NewGuid (),
                        MessageId = messageId,
                        CreatedAtUtc = DateTime.UtcNow,
                        RelativePath = xRelativePath
                    };
                }
            }
        }
    }
}
