namespace BridgeBeloteAddIn
{
    using ExcelDna.Integration.CustomUI;
    using NetOffice.ExcelApi;
    using System;
    using Application = NetOffice.ExcelApi.Application;

    class ExcelController : IDisposable
    {
        private readonly IRibbonUI _modelingRibbon;
        protected readonly Application _excel;

        public ExcelController(Application excel, IRibbonUI modelingRibbon)
        {
            _modelingRibbon = modelingRibbon;
            _excel = excel;
        }

        public void PressMe()
        {
            var activeSheet = _excel.ActiveSheet as Worksheet;
            activeSheet.Range("A1").Value = "Hello, World!";
        }

        public void Dispose()
        {
            if(_excel != null)
            {
                _excel.Dispose();
            }
        }
    }
}
