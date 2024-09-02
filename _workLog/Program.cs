namespace _workLog;

class Program
{
    static void Main (string [] args)
    {
        // todo

        foreach (string xArg in args)
        {
            Attachment xAttachment = Environment.Attach (Guid.NewGuid (), xArg);
            xAttachment.DisplayAllInfo ();
        }

        Console.WriteLine ("Press any key to exit: ");
        Console.ReadKey (true);
        Console.WriteLine ();
    }
}
