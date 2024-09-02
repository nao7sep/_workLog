namespace _workLog;

class Program
{
    static void Main (string [] args)
    {
        try
        {
        }

        catch (Exception xException)
        {
            Logger.Default.LogAndDisplay (xException.ToString ());
        }

        finally
        {
            Logger.Default.Flush ();
        }
    }
}
