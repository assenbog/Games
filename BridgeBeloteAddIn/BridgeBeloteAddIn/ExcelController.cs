namespace BridgeBeloteAddIn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using System.Text.RegularExpressions;
    using BridgeBeloteLogic.CardDealing;
    using BridgeBeloteLogic.IO;
    using ExcelDna.Integration.CustomUI;
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using Application = NetOffice.ExcelApi.Application;
    using System.IO;

    public sealed class ExcelController : IDisposable
    {
        private static readonly object _padlock = new object();
        private static ExcelController _instance = null;

        private int secondSetStartIndex = 0;
        private Worksheet _resultsComparisonWorksheet;
        private List<Dealing> _dealings1;
        private List<Dealing> _dealings2;
        private string _dealings1FileName;
        private string _dealings2FileName;
        private string _dealings1FileNameRange;

        public Application ExcelApplication { get; set; }

        public IRibbonUI RibbonUI { get; set; }

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

        // Note: set is either 1 or 2
        public void LoadDealings(int set)
        {
            if(set == 1 && _dealings1 != null)
            {
                var reply = MessageBox.Show("Dealings1 have already been loaded. Would you like to reload them?", "Load Dealings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if(reply != DialogResult.Yes)
                {
                    return;
                }
            }

            if (set == 2 && _dealings2 != null)
            {
                var reply = MessageBox.Show("Dealings2 have already been loaded. Would you like to reload them?", "Load Dealings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if (reply != DialogResult.Yes)
                {
                    return;
                }
            }

            try
            {
                using (var openFileDialog = new OpenFileDialog())
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
                            _dealings1 = input.DeserialiseFromXml(filePath);
                            _dealings1FileName = filePath;
                        }
                        else if (set == 2)
                        {
                            _dealings2 = input.DeserialiseFromXml(filePath);
                            _dealings2FileName = filePath;
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

            // Get the workbook name
            var activeWorkbookFileName = ExcelApplication.ActiveWorkbook.FullName;

            // If we haven't yet saved to xlsx but are still working with the xltx - prompt and exit
            if (!Path.GetExtension(activeWorkbookFileName).Equals(".xlsx"))
            {
                MessageBox.Show("Please save this file to XLSX format before proceeding", "Bridge Belote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_dealings1 == null)
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
                if (_dealings2 == null)
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
            _resultsComparisonWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(resultsComparison, StringComparison.InvariantCultureIgnoreCase));
            if (_resultsComparisonWorksheet != null)
            {
                _resultsComparisonWorksheet.Delete();
            }

            // Add the comparison Worksheet after the BridgeBelote one
            _resultsComparisonWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.Add(dropDownsWorksheet);
            _resultsComparisonWorksheet.Name = resultsComparison;

            _resultsComparisonWorksheet.SelectionChangeEvent += OnSelectionChange;

            // Copy the headings rows
            var sourceHeadingsRange = resultsWorksheet1.Range("A1:O2") as Range;
            sourceHeadingsRange.Copy();

            // Target headings range
            var targetHeadingsRange = _resultsComparisonWorksheet.Range("A1:O2");

            // Copy the formatting
            targetHeadingsRange.PasteSpecial(XlPasteType.xlPasteFormats);
            // ... and the values
            targetHeadingsRange.Value = sourceHeadingsRange.Value;

            // Make sure columns in source and target are of the same width
            for (var i = 1; i <= 15; i++)
            {
                _resultsComparisonWorksheet.Columns[i].ColumnWidth = resultsWorksheet1.Columns[i].ColumnWidth;
            }

            var targetRowIndex = 3;

            secondSetStartIndex = MergeDealingResults(resultsWorksheet1, _dealings1, targetRowIndex);

            var dataFileRow = secondSetStartIndex;

            if (inludeBridgeBelote2)
            {
                // Note: We have no use for the return value at this stage
                dataFileRow = MergeDealingResults(resultsWorksheet2, _dealings2, secondSetStartIndex);
            }

            // We'll use this for comparisons with other games
            _dealings1FileNameRange = $"A{dataFileRow}";

            _resultsComparisonWorksheet.Range(_dealings1FileNameRange).Value = $"Dealing1 File: {_dealings1FileName}";
            if(!string.IsNullOrEmpty(_dealings2FileName))
            {
                _resultsComparisonWorksheet.Range($"A{dataFileRow + 1}").Value = $"Dealing2 File: {_dealings2FileName}";
            }

            // Freeze the top 2 rows in the ResultsComparisonWorksheet
            var activeWindow = ExcelApplication.ActiveWindow;
            ExcelApplication.Goto(_resultsComparisonWorksheet.Cells[2]);
            activeWindow.SplitRow = 2;
            activeWindow.FreezePanes = true;

            // Both BridgeBelote1 & 2 worksheets have columns N & O hidden, as they are only relevant to the Comparison worksheet and need to be unhidden there
            var hiddenColumnRange = _resultsComparisonWorksheet.Range("N:O");
            hiddenColumnRange.Columns.Hidden = false;
        }

        public void CompareWithOtherGames()
        {
            const string bridgeBelote1 = "BridgeBelote1";
            const string compareWithOtherGames = "Compare With Other Games";
            const string dropDowns = "DropDowns";
            const string resultsComparison = "Results Comparison";
            var resultsRange = "D37:M39";

            Worksheet compareWithOtherGamesWorksheet;

            ExcelApplication.EnableEvents = true;

            if (_resultsComparisonWorksheet == null)
            {
                // No Dealings loaded
                MessageBox.Show("Please perform Result Comparison before proceeding", "Bridge Belote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // [Re]Create the "Compare With Other Games" worksheet
            compareWithOtherGamesWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(compareWithOtherGames, StringComparison.InvariantCultureIgnoreCase));
            if (compareWithOtherGamesWorksheet != null)
            {
                compareWithOtherGamesWorksheet.Delete();
            }

            var bridgeBelote1Worksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(bridgeBelote1, StringComparison.InvariantCultureIgnoreCase));
            var dropDownsWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(dropDowns, StringComparison.InvariantCultureIgnoreCase));

            // Add the comparison Worksheet before the dropdowns one
            compareWithOtherGamesWorksheet = (Worksheet)ExcelApplication.ActiveWorkbook.Worksheets.Add(dropDownsWorksheet);
            compareWithOtherGamesWorksheet.Name = compareWithOtherGames;

            var activeWorkbookFileName = ExcelApplication.ActiveWorkbook.FullName;

            var activeWorkbookFilePath = Path.GetDirectoryName(activeWorkbookFileName);

            var currentRow = 1;

            compareWithOtherGamesWorksheet.Range($"A{currentRow++}").Value = $"Filename: {activeWorkbookFileName}";
            bridgeBelote1Worksheet.Range(resultsRange).Copy();
            compareWithOtherGamesWorksheet.Range($"A{currentRow}").PasteSpecial(XlPasteType.xlPasteValues);
            compareWithOtherGamesWorksheet.Range($"A{currentRow}").PasteSpecial(XlPasteType.xlPasteFormats);
            currentRow += 4;

            // Exclude the current one to avoid duplication
            var excelFilesInActiveWorkbookFolder = Directory.GetFiles(activeWorkbookFilePath, "BridgeBelotResults*.xlsx").Where(p => !p.Equals(activeWorkbookFileName, StringComparison.InvariantCultureIgnoreCase));

            foreach (var excelFile in excelFilesInActiveWorkbookFolder)
            {
                try
                {
                    using (var workbook = ExcelApplication.Workbooks.Open(excelFile))
                    {
                        bridgeBelote1Worksheet = (Worksheet)workbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(bridgeBelote1, StringComparison.InvariantCultureIgnoreCase));
                        var resultsComparisonWorksheet = (Worksheet)workbook.Worksheets.FirstOrDefault(p => ((Worksheet)p).Name.Equals(resultsComparison, StringComparison.InvariantCultureIgnoreCase));

                        // Check if same dealings file was used before proceeding
                        var dealings1FileName = resultsComparisonWorksheet.Range(_dealings1FileNameRange).Value;
                        // A null check to exclude files where the xml file info is missing, as we can't compare without it
                        if (dealings1FileName != null && dealings1FileName.ToString().EndsWith(_dealings1FileName))
                        {
                            compareWithOtherGamesWorksheet.Range($"A{currentRow++}").Value = $"Filename: {workbook.FullName}";
                            bridgeBelote1Worksheet.Range(resultsRange).Copy();
                            compareWithOtherGamesWorksheet.Range($"A{currentRow}").PasteSpecial(XlPasteType.xlPasteValues);
                            compareWithOtherGamesWorksheet.Range($"A{currentRow}").PasteSpecial(XlPasteType.xlPasteFormats);
                            currentRow += 4;
                        }
                        workbook.Close(false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to process \"{excelFile}\": {ex.Message}");
                }
            }
        }

        private int MergeDealingResults(Worksheet resultsWorksheet, List<Dealing> dealings, int targetRowIndex)
        {
            const int redDifferenceColumnIndex = 12;
            const int blueDifferenceColumnIndex = 13;
            const int redWinningPointsColumnIndex = 14;
            const int blueWinningPointsColumnIndex = 15;

            foreach (var dealing in dealings)
            {
                var firstRowSourceIndex = dealing.SequenceNo + 2;
                var secondRowSourceIndex = dealing.ShuffledSequenceNo + 3;

                var firstRowSourceRange = resultsWorksheet.Range($"A{firstRowSourceIndex}:O{firstRowSourceIndex}");
                var secondRowSourceRange = resultsWorksheet.Range($"A{secondRowSourceIndex}:O{secondRowSourceIndex}");

                var firstRowTargetRange = _resultsComparisonWorksheet.Range($"A{targetRowIndex}:O{targetRowIndex}");
                var secondRowTargetRange = _resultsComparisonWorksheet.Range($"A{targetRowIndex + 1}:O{targetRowIndex + 1}");

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
                    var winningPoints = redDifference - blueDifference;
                    firstRowTargetRange[1, redWinningPointsColumnIndex].Value = winningPoints;
                }

                else if (redDifference < blueDifference)
                {
                    var winningPoints = blueDifference - redDifference;
                    secondRowTargetRange[1, blueWinningPointsColumnIndex].Value = winningPoints;
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
                sequenceRange = _resultsComparisonWorksheet.Range($"A{match.Groups[1].Value}:A{match.Groups[1].Value}");
            }

            if (sequenceRange != null && sequenceRange.Value != null && int.TryParse(sequenceRange.Value.ToString(), out int index))
            {
                // We are interested in the double clicking where we have either the original or the shuffled sequence number
                var dealing = index < secondSetStartIndex 
                            ? _dealings1.FirstOrDefault(p => p.SequenceNo == index || p.ShuffledSequenceNo == index) 
                            : _dealings2.FirstOrDefault(p => p.SequenceNo == index || p.ShuffledSequenceNo == index);

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
