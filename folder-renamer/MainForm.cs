using System.ComponentModel;
using folder_renamer.Properties;

namespace folder_renamer
{
    internal partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Show product version in window header
            var versionInfo = Utils.GetProductVersion();
            if (versionInfo != null) Text += @" " + versionInfo;

            // Check if saved path is still valid
            if (!TrainsimulatorFilesystem.IsPathValid(Settings.Default.RailWorksPath))
            {
                // Try to guess the path, otherwise reset settings
                var guessedPath = TrainsimulatorFilesystem.GuessTrainsimulatorPath();
                if (guessedPath != null)
                {
                    Settings.Default.RailWorksPath = guessedPath;
                    Settings.Default.Save();
                    Settings.Default.Reload();
                    renameButton.Enabled = true;
                    revertButton.Enabled = true;
                }
                else if (!string.IsNullOrWhiteSpace(Settings.Default.RailWorksPath))
                {
                    Settings.Default.Reset();
                }
            }
            else
            {
                renameButton.Enabled = true;
                revertButton.Enabled = true;
            }

            // Link textbox to the path
            pathTextBox.DataBindings.Add(nameof(pathTextBox.Text), Settings.Default, nameof(Settings.Default.RailWorksPath), true, DataSourceUpdateMode.OnPropertyChanged);
            pathTextBox.Text = Settings.Default.RailWorksPath;
        }

        private void OnSelectPathButtonClick(object sender, EventArgs e)
        {
            bool pathCorrect;
            // Ask the user for the path until the path is valid or the user aborted
            do
            {
                if (folderBrowserDialog.ShowDialog() != DialogResult.OK) return;
                pathCorrect = TrainsimulatorFilesystem.IsPathValid(folderBrowserDialog.SelectedPath);
                if (!pathCorrect && MessageBox.Show(Resources.pathNotValid, Resources.error, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) != DialogResult.Retry) return;
            } while (!pathCorrect);

            Settings.Default.RailWorksPath = folderBrowserDialog.SelectedPath;
            Settings.Default.Save();
            Settings.Default.Reload();
            renameButton.Enabled = true;
            revertButton.Enabled = true;
        }

        private void OnRenameButtonClick(object sender, EventArgs e)
        {
            renameButton.Enabled = false;
            revertButton.Enabled = false;
            if (!TrainsimulatorFilesystem.IsPathValid(Settings.Default.RailWorksPath))
            {
                MessageBox.Show(Resources.pathNotValid, Resources.error, MessageBoxButtons.OK);
                return;
            }

            var routes = TrainsimulatorFilesystem.GetAllRouteDirectories();
            progressBar.Value = 0;
            progressBar.Maximum = routes.Length;
            backgroundWorker.RunWorkerAsync(new Task(backgroundWorker, TaskType.Rename, routes));
        }

        private void OnRevertButtonClick(object sender, EventArgs e)
        {
            renameButton.Enabled = false;
            revertButton.Enabled = false;
            if (!TrainsimulatorFilesystem.IsPathValid(Settings.Default.RailWorksPath))
            {
                MessageBox.Show(Resources.pathNotValid, Resources.error, MessageBoxButtons.OK);
                return;
            }

            var routes = TrainsimulatorFilesystem.GetAllRouteDirectories();
            progressBar.Value = 0;
            progressBar.Maximum = routes.Length;
            backgroundWorker.RunWorkerAsync(new Task(backgroundWorker, TaskType.Revert, routes));
        }

        private void OnBackgroundWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            renameButton.Enabled = true;
            revertButton.Enabled = true;
            if (e.Result is Result r)
            {
                if (r.Errors.Count == 0)
                {
                    MessageBox.Show(String.Format(Resources.successfulMsg, r.CountSuccessfulRoutes, r.CountSkippedRoutes), Resources.successful);
                }
                else
                {
                    new ErrorForm(r).ShowDialog();
                }
            }
        }

        private void OnBackgroundWorkerProgress(object sender, ProgressChangedEventArgs e)
        {
            progressBar.PerformStep();
        }

        private void DoBackgroundWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is Task t)
            {
                var w = new Worker(t);
                w.DoWork();
                e.Result = w.Result;
            }
        }
    }
}
