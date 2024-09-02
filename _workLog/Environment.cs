using System.Reflection;

namespace _workLog
{
    public static class Environment
    {
        private static string? _appDirectoryPath;

        public static string AppDirectoryPath => _appDirectoryPath ??= Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location)!;

        public static string MapPath (string relativePath) => Path.Join (AppDirectoryPath, relativePath);
    }
}
