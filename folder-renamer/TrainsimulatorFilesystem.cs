using folder_renamer.Properties;
using Microsoft.Win32;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace folder_renamer;

/// <summary>
/// Utilities for dealing with the file system of Train Simulator Classic
/// </summary>
internal static class TrainsimulatorFilesystem
{

    /// <summary>
    /// Tries to determine the path where Dovetail's Train Simulator Classic is installed.
    /// </summary>
    /// <returns>Absolute path to RailWorks installation or <see langword="null"/> if not found.</returns>
    public static string? GuessTrainsimulatorPath()
    {
        // Try one of the guessed paths (default cases, registry)
        return GetPossiblePaths().FirstOrDefault(IsPathValid);
    }

    /// <summary>
    /// Returns paths where Train Simulator may be installed.
    /// </summary>
    private static IEnumerable<string> GetPossiblePaths()
    {
        // Default cases for most users
        yield return @"C:\Program Files\Steam\steamapps\common\RailWorks";
        yield return @"C:\Program Files (x86)\Steam\steamapps\common\RailWorks";
        yield return @"D:\Program Files\Steam\steamapps\common\RailWorks";
        yield return @"D:\Program Files (x86)\Steam\steamapps\common\RailWorks";
        yield return Environment.SpecialFolder.ProgramFiles + @"\Steam\steamapps\common\RailWorks";
        yield return Environment.SpecialFolder.ProgramFilesX86 + @"\Steam\steamapps\common\RailWorks";

        // Try also to load the path from registry
        var pathFromRegistry = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\railsimulator.com\RailWorks", "Install_Path", "")?.ToString();
        if (pathFromRegistry != null) yield return pathFromRegistry;
        pathFromRegistry = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\railsimulator.com\RailWorks", "Install_Path", "")?.ToString();
        if (pathFromRegistry != null) yield return pathFromRegistry;
    }

    /// <summary>
    /// Checks whether the given path contains a "RailWorks.exe" and a route folder.
    /// </summary>
    public static bool IsPathValid(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path) && File.Exists(path + @"\RailWorks.exe") && Directory.Exists(path + @"\Content\Routes");
    }

    /// <summary>
    /// Returns all route directories in the Train Simulator directory.
    /// </summary>
    public static string[] GetAllRouteDirectories()
    {
        return Directory.GetDirectories(Settings.Default.RailWorksPath + @"\Content\Routes");
    }

    /// <summary>
    /// Tries to determine the English or German name for a route.
    /// </summary>
    /// <param name="routePath">Absolute path of the route</param>
    /// <returns>The name depends on the name availability and the current UI language</returns>
    /// <exception cref="NoRouteNameException">If the route name could not be determined.</exception>
    public static string GetRouteName(string routePath)
    {
        string routeProperties = routePath + @"\RouteProperties.xml";
        if (!File.Exists(routeProperties)) throw new NoRouteNameException();

        XDocument doc = XDocument.Load(new StreamReader(routeProperties, Encoding.UTF8));

        if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase))
        {
            // Load the German name first
            var name = doc.XPathSelectElement("cRouteProperties/DisplayName/Localisation-cUserLocalisedString/German")?.Value;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            name = doc.XPathSelectElement("cRouteProperties/DisplayName/Localisation-cUserLocalisedString/English")?.Value;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            throw new NoRouteNameException();
        }
        else
        {
            // Load the English name first
            var name = doc.XPathSelectElement("cRouteProperties/DisplayName/Localisation-cUserLocalisedString/English")?.Value;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            name = doc.XPathSelectElement("cRouteProperties/DisplayName/Localisation-cUserLocalisedString/German")?.Value;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            throw new NoRouteNameException();
        }
    }
}
