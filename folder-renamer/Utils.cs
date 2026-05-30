using System.Diagnostics;
using System.Media;

namespace folder_renamer;

internal static class Utils
{
    /// <summary>
    /// Lookups the product version of the running application.
    /// </summary>
    public static string? GetProductVersion()
    {
        var location = Environment.ProcessPath;
        if (File.Exists(location))
            return FileVersionInfo.GetVersionInfo(location).ProductVersion;

        return null;
    }

    /// <summary>
    /// Opens the passed filename. Can also be a URL.
    /// </summary>
    public static void OpenFile(string filename)
    {
        try
        {
            Process.Start("explorer.exe", filename);
        }
        catch (Exception)
        {
            SystemSounds.Beep.Play();
        }
    }
}
