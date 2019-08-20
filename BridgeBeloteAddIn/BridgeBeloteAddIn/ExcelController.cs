namespace BridgeBeloteAddIn
{
    using BridgeBeloteLogic.CardDealing;
    using BridgeBeloteLogic.IO;
    using ExcelDna.Integration.CustomUI;
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Application = NetOffice.ExcelApi.Application;
    using System.Text.RegularExpressions;

    public sealed class ExcelController : IDisposable
    {
        private static readonly object _padlock = new object();
        private static ExcelController _instance = null;

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

        public Worksheet ResultsComparisonWorksheet { get; private set; }

        public List<Dealing> Dealings { get; private set; }

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

                    Dealings = input.DeserialiseFromXml(filePath);
                }
            }
        }

        public void ResultsComparison()
        {
            const string results = "BridgeBelote";
            const string resultsComparison = "Results Comparison";
            const string dropDowns = "DropDowns";

            if(Dealings == null)
            {
                // No Dealings loaded
                MessageBox.Show("Please load Dealings before proceeding", "Bridge Belote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ExcelApplication.ActiveWorkbook.Worksheets.Any(p => ((Worksheet)p).Name.Equals(results, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Wrong workbook
                MessageBox.Show("Document doesn't appear to hold valid Bridge Belote results", "Bridge Belote", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the BridgeBelote [results] Worksheet and DropDowns. We'd like to add the new Worksheet in between the two
            var resultsWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(results, StringComparison.InvariantCultureIgnoreCase));
            var dropDownsWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(dropDowns, StringComparison.InvariantCultureIgnoreCase));

            // [Re]Create the "Results Comparison" worksheet
            ResultsComparisonWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(resultsComparison, StringComparison.InvariantCultureIgnoreCase));
            if (ResultsComparisonWorksheet != null)
            {
                ResultsComparisonWorksheet.Delete();
            }

            // Add the comparison Worksheet after the BridgeBelote one
            ResultsComparisonWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.Add(dropDownsWorksheet);
            ResultsComparisonWorksheet.Name = resultsComparison;

            ResultsComparisonWorksheet.SelectionChangeEvent += OnSelectionChange;

            // Copy the headings rows
            var sourceHeadingsRange = resultsWorksheet.Range("A1:O2") as Range;
            sourceHeadingsRange.Copy();

            // Target headings range
            var targetHeadingsRange = ResultsComparisonWorksheet.Range("A1:O2");

            // Copy the formatting
            targetHeadingsRange.PasteSpecial(XlPasteType.xlPasteFormats);
            // ... and the values
            targetHeadingsRange.Value = sourceHeadingsRange.Value;

            // Make sure columns in source and target are of the same width
            for (var i = 1; i <= 15; i++)
            {
                ResultsComparisonWorksheet.Columns[i].ColumnWidth = resultsWorksheet.Columns[i].ColumnWidth;
            }

            var targetRowIndex = 3;

            foreach (var dealing in Dealings)
            {
                var firstRowSourceIndex = dealing.SequenceNo + 2;
                var secondRowSourceIndex = dealing.ShuffledSequenceNo + 3;

                var firstRowSourceRange = resultsWorksheet.Range($"A{firstRowSourceIndex}:O{firstRowSourceIndex}");
                var secondRowSourceRange = resultsWorksheet.Range($"A{secondRowSourceIndex}:O{secondRowSourceIndex}");

                var firstRowTargetRange = ResultsComparisonWorksheet.Range($"A{targetRowIndex}:O{targetRowIndex}");
                var secondRowTargetRange = ResultsComparisonWorksheet.Range($"A{targetRowIndex + 1}:O{targetRowIndex + 1}");

                firstRowTargetRange.Value = firstRowSourceRange.Value;
                secondRowTargetRange.Value = secondRowSourceRange.Value;

                // Space out each 2 rows with an empty one
                targetRowIndex += 3;
            }
        }

        private void OnSelectionChange(Range Target)
        {
            Range sequenceRange = null;

            // Note: When an entire row is selected, e.g. row 15, the Address is of the format "$15:$15"
            var match = Regex.Match(Target.Address, @"\$(\d+):\$(\d+)");

            if(match.Success && match.Groups[1].Value == match.Groups[2].Value)
            {
                // Both group values should provide the row number which we'll use to get the "A" row only range 
                sequenceRange = ResultsComparisonWorksheet.Range($"A{match.Groups[1].Value}:A{match.Groups[1].Value}");
            }

            if (sequenceRange != null && sequenceRange.Value != null && int.TryParse(sequenceRange.Value.ToString(), out int index))
            {
                // We are interested in the double clicking where we have either the original or the shuffled sequence number
                var dealing = Dealings.FirstOrDefault(p => p.SequenceNo == index || p.ShuffledSequenceNo == index);
                if (dealing != null)
                {
                    var output = new Output();
                    var formattedOutput = output.FormattedOutput(dealing.Initial5CardsDealt, dealing.Additional3CardsDealt, dealing.SequenceNo, dealing.ShuffledSequenceNo, dealing.DealingSide);
                    var htmlOutput = output.FormattedHtmlOutput(formattedOutput);

                    var dealingViewer = new DealingViewer(htmlOutput);
                    dealingViewer.ShowDialog();
                }
            }
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
