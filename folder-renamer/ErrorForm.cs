using System.Media;
using folder_renamer.Properties;

namespace folder_renamer
{
    internal partial class ErrorForm : Form
    {
        public ErrorForm(Result result)
        {
            InitializeComponent();
            label.Text = string.Format(Resources.notSuccessfulMsg, result.CountSuccessfulRoutes, result.CountSkippedRoutes, result.Errors.Count);
            textBox.Text = string.Join("\r\n", result.Errors);
        }

        private void OnClipboardClick(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox.Text);
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            SystemSounds.Hand.Play();
        }
    }
}
