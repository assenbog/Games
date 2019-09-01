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

        private int secondSetStartIndex = 0;

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

        public List<Dealing> Dealings1 { get; private set; }

        public List<Dealing> Dealings2 { get; private set; }

        public Application ExcelApplication { get; set; }

        public IRibbonUI RibbonUI { get; set; }

        // Note: set is either 1 or 2
        public void LoadDealings(int set)
        {
            if(set == 1 && Dealings1 != null)
            {
                var reply = MessageBox.Show("Dealings1 have already been loaded. Would you like to reload them?", "Load Dealings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if(reply != DialogResult.Yes)
                {
                    return;
                }
            }

            if (set == 2 && Dealings2 != null)
            {
                var reply = MessageBox.Show("Dealings2 have already been loaded. Would you like to reload them?", "Load Dealings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if (reply != DialogResult.Yes)
                {
                    return;
                }
            }

            try
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

                        if (set == 1)
                        {
                            Dealings1 = input.DeserialiseFromXml(filePath);
                        }
                        else if (set == 2)
                        {
                            Dealings2 = input.DeserialiseFromXml(filePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ResultsComparison()
        {
            const string results1 = "BridgeBelote1";
            const string results2 = "BridgeBelote2";
            const string resultsComparison = "Results Comparison";
            const string dropDowns = "DropDowns";

            ExcelApplication.EnableEvents = true;

            if (Dealings1 == null)
            {
                // No Dealings loaded
                MessageBox.Show("Please load Dealings1 before proceeding", "Bridge Belote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ExcelApplication.ActiveWorkbook.Worksheets.Any(p => ((Worksheet)p).Name.Equals(results1, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Wrong workbook
                MessageBox.Show("Document doesn't appear to hold valid Bridge Belote results", $"{results1}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var reply = MessageBox.Show("Include BridgeBelote2 results in the analysis?", "Analysis", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            var inludeBridgeBelote2 = reply == DialogResult.Yes;

            if(inludeBridgeBelote2)
            {
                // Check the Dealings2 related data
                if (Dealings2 == null)
                {
                    // No Dealings loaded
                    MessageBox.Show("Please load Dealings2 before proceeding", "Bridge Belote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ExcelApplication.ActiveWorkbook.Worksheets.Any(p => ((Worksheet)p).Name.Equals(results2, StringComparison.InvariantCultureIgnoreCase)))
                {
                    // Wrong workbook
                    MessageBox.Show("Document doesn't appear to hold valid Bridge Belote results", $"{results2}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Get the BridgeBelote [results] Worksheet and DropDowns. We'd like to add the new Worksheet in between the two
            var resultsWorksheet1 = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(results1, StringComparison.InvariantCultureIgnoreCase));
            var resultsWorksheet2 = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(results2, StringComparison.InvariantCultureIgnoreCase));
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
            var sourceHeadingsRange = resultsWorksheet1.Range("A1:O2") as Range;
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
                ResultsComparisonWorksheet.Columns[i].ColumnWidth = resultsWorksheet1.Columns[i].ColumnWidth;
            }

            var targetRowIndex = 3;

            secondSetStartIndex = MergeDealingResults(resultsWorksheet1, Dealings1, targetRowIndex);

            if(inludeBridgeBelote2)
            {
                // Note: We have no use for the return value at this stage
                MergeDealingResults(resultsWorksheet2, Dealings2, secondSetStartIndex);
            }
        }

        private int MergeDealingResults(Worksheet resultsWorksheet, List<Dealing> dealings, int targetRowIndex)
        {
            const int redDifferenceColumnIndex = 12;
            const int blueDifferenceColumnIndex = 13;
            const int redWinningPointsColumnIndex = 14;
            const int blueWinningPointsColumnIndex = 15;

            foreach (var dealing in Dealings1)
            {
                var firstRowSourceIndex = dealing.SequenceNo + 2;
                var secondRowSourceIndex = dealing.ShuffledSequenceNo + 3;

                var firstRowSourceRange = resultsWorksheet.Range($"A{firstRowSourceIndex}:O{firstRowSourceIndex}");
                var secondRowSourceRange = resultsWorksheet.Range($"A{secondRowSourceIndex}:O{secondRowSourceIndex}");

                var firstRowTargetRange = ResultsComparisonWorksheet.Range($"A{targetRowIndex}:O{targetRowIndex}");
                var secondRowTargetRange = ResultsComparisonWorksheet.Range($"A{targetRowIndex + 1}:O{targetRowIndex + 1}");

                firstRowTargetRange.Value = firstRowSourceRange.Value;
                secondRowTargetRange.Value = secondRowSourceRange.Value;

                // Победни точки
                int.TryParse(((object[,])firstRowTargetRange.Value)[1, redDifferenceColumnIndex]?.ToString() ?? "0", out int firstRowRedDifference);
                int.TryParse(((object[,])firstRowTargetRange.Value)[1, blueDifferenceColumnIndex]?.ToString() ?? "0", out int firstRowBlueDifference);
                int.TryParse(((object[,])secondRowTargetRange.Value)[1, redDifferenceColumnIndex]?.ToString() ?? "0", out int secondRowRedDifference);
                int.TryParse(((object[,])secondRowTargetRange.Value)[1, blueDifferenceColumnIndex]?.ToString() ?? "0", out int secondRowBlueDifference);

                var redDifference = firstRowRedDifference + secondRowRedDifference;
                var blueDifference = firstRowBlueDifference + secondRowBlueDifference;

                if (redDifference > blueDifference)
                {
                    ((object[,])firstRowTargetRange.Value)[1, redWinningPointsColumnIndex] = redDifference - blueDifference;
                }

                else if (redDifference < blueDifference)
                {
                    ((object[,])secondRowTargetRange.Value)[1, blueWinningPointsColumnIndex] = blueDifference - redDifference;
                }

                // Space out each 2 rows with an empty one
                targetRowIndex += 3;
            }

            return targetRowIndex;
        }

        private void OnSelectionChange(Range Target)
        {
            Range sequenceRange = null;

            // Note: When an entire row is selected, e.g. row 15, the Address is of the format "$15:$15"
            var match = Regex.Match(Target.Address, @"\$(\d+):\$(\d+)");

            if(match.Success && match.Groups[1].Value == match.Groups[2].Value)
            {
                // Both group values should provide the row number which we'll use to get the "A" column only range 
                sequenceRange = ResultsComparisonWorksheet.Range($"A{match.Groups[1].Value}:A{match.Groups[1].Value}");
            }

            if (sequenceRange != null && sequenceRange.Value != null && int.TryParse(sequenceRange.Value.ToString(), out int index))
            {
                // We are interested in the double clicking where we have either the original or the shuffled sequence number
                var dealing = index < secondSetStartIndex 
                            ? Dealings1.FirstOrDefault(p => p.SequenceNo == index || p.ShuffledSequenceNo == index) 
                            : Dealings2.FirstOrDefault(p => p.SequenceNo == index || p.ShuffledSequenceNo == index);

                if (dealing != null)
                {
                    var output = new Output(false);
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
