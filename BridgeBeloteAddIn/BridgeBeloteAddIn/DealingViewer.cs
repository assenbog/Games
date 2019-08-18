namespace BridgeBeloteAddIn
{
    using System.Windows.Forms;

    public partial class DealingViewer : Form
    {
        public DealingViewer(string html)
        {
            InitializeComponent();

            webBrowser.DocumentText = html;
        }
    }
}
