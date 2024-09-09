using System.Text;

namespace _workLog
{
    public class Logger
    {
        public DateTime CreatedAtUtc { get; init; }

        public string Path { get; init; }

        public List <string> Messages { get; } = new ();

        public Logger ()
        {
            CreatedAtUtc = DateTime.UtcNow;
            Path = Environment.MapPath (@$"Logs\Log-{CreatedAtUtc:yyyyMMdd'T'HHmmss'Z'}.log");
        }

        public void LogAndDisplay (string message, bool flush = false)
        {
            Messages.Add (message);

            if (flush)
                Flush ();

            Console.WriteLine (message);
        }

        public void Flush ()
        {
            Directory.CreateDirectory (Environment.MapPath ("Logs"));
            File.AppendAllLines (Path, Messages, Encoding.UTF8);
            Messages.Clear ();
        }

        private static Logger? _default;

        public static Logger Default => _default ??= new ();
    }
}
