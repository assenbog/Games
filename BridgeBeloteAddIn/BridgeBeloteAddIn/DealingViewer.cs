namespace BridgeBeloteAddIn
{
    using System.Windows.Forms;
    using BridgeBeloteAddIn.Properties;

    public partial class DealingViewer : Form
    {
        public DealingViewer(string html)
        {
            InitializeComponent();

            webBrowser.DocumentText = html;
        }

        private void OnFormLoad(object sender, System.EventArgs e)
        {
            // Set window location
            if (Settings.Default.WindowLocation != null)
            {
                this.Location = Settings.Default.WindowLocation;
            }

            // Set window size
            if (Settings.Default.WindowSize != null)
            {
                this.Size = Settings.Default.WindowSize;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Copy window location to app settings
            Settings.Default.WindowLocation = this.Location;

            // Copy window size to app settings
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowSize = this.Size;
            }
            else
            {
                Settings.Default.WindowSize = this.RestoreBounds.Size;
            }

            // Save settings
            Settings.Default.Save();
        }
    }
}
