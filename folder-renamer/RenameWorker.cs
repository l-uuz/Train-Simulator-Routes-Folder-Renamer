using folder_renamer.Properties;
using System.ComponentModel;
using System.Text;

namespace folder_renamer;

/// <summary>
/// Tast that creates hidden <code>desktop.ini</code> files to “rename” the directories.
/// </summary>
internal class Worker
{
    private const string DocumentPrefix = "[.ShellClassInfo]\r\nConfirmFileOp=0\r\nLocalizedResourceName=";
    private const string DocumentSuffix = "\r\n[ViewState]\r\nMode=\r\nVid=\r\nFolderType=Generic";

    private Task Task { get; }
    public Result Result { get; private set; } = new();

    public Worker(Task task)
    {
        Task = task;
    }

    public void DoWork()
    {
        if (Task.TaskType == TaskType.Rename) Rename();
        if (Task.TaskType == TaskType.Revert) Revert();
    }

    /// <summary>
    /// Creates a desktop.ini file in each route with the name of the route.
    /// </summary>
    public void Rename()
    {
        foreach (var route in Task.RouteDirectories)
        {
            var iniFile = Path.Combine(route, "desktop.ini");
            if (File.Exists(iniFile))
            {
                Result.AddSkippedRoute();
                Task.ReportProgress();
                continue;
            }

            FileStream? file = null;
            try
            {
                var routeName = TrainsimulatorFilesystem.GetRouteName(route);
                file = File.Create(iniFile);
                file.Write(Encoding.Unicode.GetBytes(DocumentPrefix), 0, Encoding.Unicode.GetByteCount(DocumentPrefix));
                file.Write(Encoding.Unicode.GetBytes(routeName), 0, Encoding.Unicode.GetByteCount(routeName));
                file.Write(Encoding.Unicode.GetBytes(DocumentSuffix), 0, Encoding.Unicode.GetByteCount(DocumentSuffix));
                file.Close();
                SetHiddenAndSystemAttribute(iniFile);
                AddSystemAttribute(route);
                Result.AddSuccessfulRoute();
            }
            catch (NoRouteNameException)
            {
                Result.AddErrorRoute(route.Substring(route.LastIndexOf('\\') + 1) + ": " + Resources.noRouteName);
            }
            catch (Exception e)
            {
                Result.AddErrorRoute(route.Substring(route.LastIndexOf('\\') + 1) + ": " + e.Message);
            }
            finally
            {
                file?.Close();
            }
            Task.ReportProgress();
        }
    }

    /// <summary>
    /// Deletes all desktop.ini files in the routes.
    /// </summary>
    public void Revert()
    {
        foreach (var route in Task.RouteDirectories)
        {
            var iniFile = Path.Combine(route, "desktop.ini");
            if (!File.Exists(iniFile))
            {
                Result.AddSkippedRoute();
                Task.ReportProgress();
                continue;
            }
            try
            {
                File.SetAttributes(iniFile, FileAttributes.Hidden);
                File.Delete(iniFile);
                RemoveSystemAttribute(route);
                Result.AddSuccessfulRoute();
            }
            catch (Exception e)
            {
                Result.AddErrorRoute(route.Substring(route.LastIndexOf('\\') + 1) + ":" + e.Message);
            }
            Task.ReportProgress();
        }
    }

    private static void SetHiddenAndSystemAttribute(string path)
    {
        File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System);
    }

    private static void AddSystemAttribute(string path)
    {
        File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.System);
    }

    private static void RemoveSystemAttribute(string path)
    {
        File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.System);
    }
}

internal class Task
{
    private BackgroundWorker Worker { get; }
    public TaskType TaskType { get; }
    public IReadOnlyList<string> RouteDirectories { get; }

    public Task(BackgroundWorker worker, TaskType taskType, IEnumerable<string> routeDirectories)
    {
        this.Worker = worker;
        this.TaskType = taskType;
        this.RouteDirectories = routeDirectories.ToList();
    }

    public void ReportProgress()
    {
        Worker.ReportProgress(1);
    }
}

internal enum TaskType
{
    Rename, Revert
}

internal class Result
{
    private readonly List<string> _errors = new();
    public IReadOnlyList<string> Errors => _errors;
    public int CountSuccessfulRoutes { get; private set; }
    public int CountSkippedRoutes { get; private set; }

    public void AddSuccessfulRoute()
    {
        CountSuccessfulRoutes++;
    }

    public void AddSkippedRoute()
    {
        CountSkippedRoutes++;
    }

    public void AddErrorRoute(string error)
    {
        _errors.Add(error);
    }
}
