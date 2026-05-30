namespace folder_renamer;

/// <summary>
/// Indicates that the name of the route could not be determined.
/// </summary>
internal class NoRouteNameException : Exception
{
    public NoRouteNameException() : base("Route name could not be determined") { }

    public NoRouteNameException(string routeName) : base($"Route name \"{routeName}\" could not be determined") { }
}
