namespace BridgeBeloteAddIn
{
    using System;
    using System.IO;
    using System.Resources;
    using System.Runtime.InteropServices;
    using Application = NetOffice.ExcelApi.Application;
    using ExcelDna.Integration.CustomUI;

    [ComVisible(true)]
    public class CustomRibbon : ExcelRibbon
    {
        private Application _excel;
        private IRibbonUI _thisRibbon;
        private ExcelController _excelController;

        public override string GetCustomUI(string ribbonId)
        {
            _excel = new Application(null, ExcelDna.Integration.ExcelDnaUtil.Application);
            string ribbonXml = GetCustomRibbonXML();
            return ribbonXml;
        }

        private string GetCustomRibbonXML()
        {
            string ribbonXml;
            var thisAssembly = typeof(CustomRibbon).Assembly;
            var resourceName = typeof(CustomRibbon).Namespace + ".CustomRibbon.xml";

            using (Stream stream = thisAssembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                ribbonXml = reader.ReadToEnd();
            }

            if (ribbonXml == null)
            {
                throw new MissingManifestResourceException(resourceName);
            }
            return ribbonXml;
        }

        public void OnLoad(IRibbonUI ribbon)
        {
            _thisRibbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));

            _excel.WorkbookActivateEvent += OnInvalidateRibbon;
            _excel.WorkbookDeactivateEvent += OnInvalidateRibbon;
            _excel.SheetActivateEvent += OnInvalidateRibbon;
            _excel.SheetDeactivateEvent += OnInvalidateRibbon;

            if (_excel.ActiveWorkbook == null)
            {
                _excel.Workbooks.Add();
            }

            _excelController = ExcelController.Instance;
            _excelController.RibbonUI = ribbon;
            _excelController.ExcelApplication = _excel;
        }

        public void OnLoadDealings(IRibbonControl control)
        {
            _excelController.LoadDealings();
        }

        public void OnResultsComparison(IRibbonControl control)
        {
            _excelController.ResultsComparison();
        }

        private void OnInvalidateRibbon(object obj)
        {
            _thisRibbon.Invalidate();
        }
    }
}
