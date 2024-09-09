namespace _workLog;

class Program
{
    static void Main (string [] args)
    {
        try
        {
            // todo
        }

        catch (Exception xException)
        {
            Logger.Default.LogAndDisplay (xException.ToString ());
        }

        finally
        {
            if (Logger.Default.Messages.Any ())
                Logger.Default.Flush ();
        }
    }
}
