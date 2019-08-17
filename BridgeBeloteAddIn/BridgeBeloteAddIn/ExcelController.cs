namespace BridgeBeloteAddIn
{
    using BridgeBeloteLogic.CardDealing;
    using BridgeBeloteLogic.IO;
    using ExcelDna.Integration.CustomUI;
    using NetOffice.ExcelApi;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Application = NetOffice.ExcelApi.Application;

    public sealed class ExcelController : IDisposable
    {
        private static readonly object _padlock = new object();
        private static ExcelController _instance = null;

        private List<Dealing> _dealings = null;

        private ExcelController()
        {
        }

        public static ExcelController Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ExcelController();
                        }
                    }
                }
                return _instance;
            }
        }
        public Application ExcelApplication { get; set; }

        public IRibbonUI RibbonUI { get; set; }

        public void LoadDealings()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "XML files (*.xml)|*.xml";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    var input = new Input();

                    _dealings = input.DeserialiseFromXml(filePath);
                }
            }
        }

        public void PressMe()
        {
            var activeSheet = ExcelApplication.ActiveSheet as Worksheet;
            activeSheet.Range("A1").Value = "Hello, World!";
        }

        public void Dispose()
        {
            if(ExcelApplication != null)
            {
                ExcelApplication.Dispose();
            }
        }
    }
}
