using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace SuperSudoku
{
	/// <summary>
	/// GUI Logic goes in this MainWindow class. It also contains the state of the application.
	/// </summary>
	public partial class UserInterface
	{
        // Game elements are implemented in seperate classes and instantiated to properties of this class here:
        private readonly FileHandler fileHandler;
	    private PuzzleSolver puzzleSolver;
	    private PuzzleGrid puzzleGrid;

        // game grid size
	    private const int GameBoardCols = 9;
        private const int GameBoardRows = 9;
	    private const int Max = 9;

        // State properties are defined here
        private bool showHintsOn;
	    private bool showErrorsOn;
	    private bool inPuzzleEnterMode = false;

        // helper enum for moving around the grid
        private enum Direction
        {
            Up, Down, Left, Right
        }

        // GUI elements are defined here
        private readonly SaveFileDialog saveGameDialog;
        private readonly OpenFileDialog openGameDialog;

        /// <summary>
        /// This function instantiates all the game objects and sets up the game window.
        /// </summary>
		public UserInterface()
		{
			InitializeComponent();

            // Initialize game elements
            fileHandler = new FileHandler();

            //puzzleSolver = new PuzzleSolver();
            //var result = puzzleSolver.SolveGrid(puzzleGrid, true);
            //StatusBarText.Text = result ? "This puzzle has a solution solved." : "This puzzle cannot be solved.";

			// Initialize the save game dialog
            saveGameDialog = new SaveFileDialog {Filter = "Sudoku Games | *.sud", DefaultExt = ".sud"};
            openGameDialog = new OpenFileDialog {Filter = "Sudoku Games | *.sud", DefaultExt = ".sud"};

            puzzleGrid = new PuzzleGrid();
            SetPuzzleGrid(puzzleGrid);

            SetHintsBoxToDefault();
		}

        // The following methods are helper methods
        /// <summary>
        /// Enters the puzzle entry state.
        /// </summary>
        private void EnterPuzzleEntryState()
        {
            // clear puzzle grid
            puzzleGrid = new PuzzleGrid();
            SetPuzzleGrid(puzzleGrid);

            // hide buttons
            SolveNow.Visibility = Visibility.Hidden;
            HintsBox.Visibility = Visibility.Hidden;
            HintsBoxTitle.Visibility = Visibility.Hidden;

            // show correct buttons
            EntryComplete.Visibility = Visibility.Visible;

            inPuzzleEnterMode = true;
        }

        /// <summary>
        /// Enters the puzzle entry state.
        /// </summary>
        private void LeavePuzzleEntryState()
        {
            // finish puzzle grid
            puzzleGrid.Finish();
            SetPuzzleGrid(puzzleGrid);

            // hide buttons
            SolveNow.Visibility = Visibility.Visible;
            HintsBox.Visibility = Visibility.Visible;
            HintsBoxTitle.Visibility = Visibility.Visible;

            // show correct buttons
            EntryComplete.Visibility = Visibility.Hidden;

            inPuzzleEnterMode = false;
        }

        /// <summary>
        /// This method shows the save file dialog box. It's called by the save game menu items.
        /// </summary>
        private void SaveFile(bool saveUnsolved = false)
        {
            // Show save file dialog box
            var result = saveGameDialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string fileName = saveGameDialog.FileName;

                bool success;
                success = fileHandler.SaveFile(puzzleGrid, fileName, saveUnsolved);

                if (!success)
                {
                    MessageBox.Show("There was a problem saving your file. Please try again.");
                }
            }
        }

        /// <summary>
        /// This method shows the open file dialog box.
        /// </summary>
        private void OpenFile()
        {
            // Show save file dialog box
            var result = openGameDialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string fileName = openGameDialog.FileName;

                PuzzleGrid grid = fileHandler.OpenFile(fileName);
                puzzleGrid = (PuzzleGrid) grid.Clone();

                if (grid == null)
                {
                    MessageBox.Show("There was a problem opening your file. Please try again.");
                }
                else
                {
                    SetPuzzleGrid(grid);
                }
            }
        }

        /// <summary>
        /// This function is just a fast way of resetting the hints box on the bottom of the window.
        /// </summary>
        private void SetHintsBoxToDefault()
        {
            if (showHintsOn)
            {
                HintsBox.Text = "Click a box to view hints.";
            }
            else
            {
                HintsBox.Text = "Hints are currently turned off. You can turn them on from the options menu.";
            }
        }

        /// <summary>
        /// This function shows the hint text
        /// </summary>
        private void ShowHintText()
        {
            // get the row/column of the text box
            if (Keyboard.FocusedElement != null && Keyboard.FocusedElement is TextBox)
            {
                var name = ((TextBox)Keyboard.FocusedElement).Name;
                var rowCol = GetRowColumnFromTextboxName(name);
                var row = rowCol.Item1;
                var col = rowCol.Item2;

                // get all the possible choices
                var possibleChoices = GetPossibleChoices(row, col);

                // put in text box
                var hints = "Possible choice(s) for current cell: " + string.Join(", ", possibleChoices) + ".";
                HintsBox.Text = hints;
            }
            else
            {
                SetHintsBoxToDefault();
            }
        }

        /// <summary>
        /// The grid GUI elements are labeled by row and column.
        /// 
        /// Elements are in the form "_1x1" where the first number is the row and the second is the column.
        /// Returns itself on fail.
        /// </summary>
        private string GetMovedGridItem(string element, Direction direction)
        {
            var ret = element;

            var rowCol = GetRowColumnFromTextboxName(element);

            if (rowCol != null)
            {
                var row = rowCol.Item1+1; // plus one because they're 0 indexed.
                var col = rowCol.Item2+1;
                var disabled = (Style) Resources["GridElementDisabled"];

                switch (direction)
                {
                    case Direction.Left:
                        // get the new column if we're not on the left edge
                        if (col != 1)
                        {
                            col -= 1;

                            var textbox = (TextBox) FindName(GetTextBoxNameFromRowColumn(row, col));
                            if (textbox != null && textbox.Style == disabled)
                            {
                                while (col != 1 && textbox.Style == disabled)
                                {
                                    col -= 1;

                                    textbox = (TextBox)FindName(GetTextBoxNameFromRowColumn(row, col));
                                }
                            }
                        }
                        break;
                    case Direction.Right:
                        // if we're not on the right edge
                        if (col != GameBoardCols)
                        {
                            col += 1;

                            var textbox = (TextBox)FindName(GetTextBoxNameFromRowColumn(row, col));
                            if (textbox != null && textbox.Style == disabled)
                            {
                                while (col != GameBoardCols && textbox.Style == disabled)
                                {
                                    col += 1;

                                    textbox = (TextBox)FindName(GetTextBoxNameFromRowColumn(row, col));
                                }
                            }
                        }
                        break;
                    case Direction.Up:
                        // not on the top
                        if (row != 1)
                        {
                            row -= 1;

                            var textbox = (TextBox)FindName(GetTextBoxNameFromRowColumn(row, col));
                            if (textbox != null && textbox.Style == disabled)
                            {
                                while (row != 1 && textbox.Style == disabled)
                                {
                                    row -= 1;

                                    textbox = (TextBox)FindName(GetTextBoxNameFromRowColumn(row, col));
                                }
                            }
                        }
                        break;
                    case Direction.Down:
                        // not on bottom
                        if (row != GameBoardRows)
                        {
                            row += 1;

                            var textbox = (TextBox)FindName(GetTextBoxNameFromRowColumn(row, col));
                            if (textbox != null && textbox.Style == disabled)
                            {
                                while (row != GameBoardRows && textbox.Style == disabled)
                                {
                                    row += 1;

                                    textbox = (TextBox)FindName(GetTextBoxNameFromRowColumn(row, col));
                                }
                            }
                        }
                        break;
                }

                ret = GetTextBoxNameFromRowColumn(row, col);
            }

            return ret;
        }

        /// <summary>
        /// This takes a textbox name and returns row and column integers.
        /// </summary>
        /// <param name="textBoxName">The name of the textbox in the format _AxB, where
        /// A is the row and B is the column.</param>
        /// <returns>
        /// A a tuple of two ints in the form of: (Row, Column) if the textBoxName is valid.
        /// Returns null if the textBoxName is invalid.</returns>
        private static Tuple<int,int> GetRowColumnFromTextboxName(string textBoxName)
        {
            Tuple<int,int> ret;

            if (textBoxName.IndexOf('x') != -1 && textBoxName.IndexOf('_') == 0)
            {
                // first chop off the leading underscore and split by x, then put into ints
                var temp = textBoxName.Substring(1).Split('x');
                var row = Convert.ToInt32(temp[0]) - 1; // minus 1 because we want them to be 0 indexed.
                var col = Convert.ToInt32(temp[1]) - 1;

                ret = Tuple.Create(row, col);
            }
            else
            {
                ret = null;
            }

            return ret;
        }

        /// <summary>
        /// This functions does the opposite of the function above; it gets a text box name string given a 
        /// row and a column.
        /// </summary>
        /// <param name="row">Row of the cell.</param>
        /// <param name="col">Column of the cell.</param>
        /// <returns>A string containing the textbox name.</returns>
        private static string GetTextBoxNameFromRowColumn(int row, int col)
        {
            return String.Format("_{0:d}x{1:d}", row, col);
        }

        /// <summary>
        /// This function takes a PuzzleGrid and updates the game board values to the values in the PuzzleGrid.
        /// </summary>
        /// <param name="grid">The puzzle gird.</param>
        private void SetPuzzleGrid(PuzzleGrid grid)
        {
            for (int i = 0; i < GameBoardRows; i++)
            {
                for (int j = 0; j < GameBoardCols; j++)
                {
                    var box = (TextBox) FindName(GetTextBoxNameFromRowColumn(i+1, j+1));
                    if (box != null)
                    {
                        box.Style = (Style)(Resources["GridElement"]);

                        if (grid.Grid[i,j] < 0)
                        {
                            box.Text = ((-1) * grid.Grid[i,j]).ToString();
                            box.Style = (Style)(Resources["GridElementDisabled"]);
                        }
                        else if (grid.Grid[i,j] > 0)
                        {
                            box.Text = grid.Grid[i, j].ToString();
                        }
                        else
                        {
                            box.Text = "";
                        }
                        
                    }
                    
                }
            }
        }

        /// <summary>
        /// Get's all the possible choices for a given row/col
        /// </summary>
        /// <param name="rowIn">The row of the gameboard to check for.</param>
        /// <param name="colIn">The column of the gameboard to check for.</param>
        /// <returns>List of possible values.</returns>
        private IEnumerable<int> GetPossibleChoices(int rowIn, int colIn)
        {
            var sectionRow = rowIn/3;
            var sectionCol = colIn/3;

            var possibilities = new List<int>(); // the possibilities are endless
            for (var i = 1; i < Max; i++)
            {
                var neverSeen = true;

                // if this number is not the value in the current cell
                if (Math.Abs(puzzleGrid.Grid[rowIn, colIn]) != i)
                {
                    // check column
                    for (var row = 0; row < GameBoardRows; row++)
                    {
                        // did we just see the number we're looking for in this column somehwere?
                        if (Math.Abs(puzzleGrid.Grid[row, colIn]) == i)
                        {
                            neverSeen = false;
                        }
                    }
                    // check row
                    for (var col = 0; col < GameBoardCols; col++)
                    {
                        if (Math.Abs(puzzleGrid.Grid[rowIn, col]) == i)
                        {
                            neverSeen = false;
                        }
                    }
                    // check current 9x9 grid 
                    // loop through all the rows and columns
                    for (var r = 0; r < GameBoardRows; r++)
                    {
                        for (var c = 0; c < GameBoardCols; c++)
                        {
                            // if we're in the same section and we found the number we're working on
                            if (sectionRow == r / 3 && sectionCol == c / 3 &&
                                Math.Abs(puzzleGrid.Grid[r, c]) == i)
                            {
                                neverSeen = false;
                            }
                        }
                    }
                }

                // if we've never seen the number, go ahead and add it to the possibilities
                if (neverSeen)
                {
                    possibilities.Add(i);
                }
            }

            return possibilities;
        }

        /// <summary>
        /// Checks if the game is in progress.
        /// </summary>
        /// <returns>True if a user editable field has a value in it, false otherwise.</returns>
        private bool GameInProgress()
        {
            var sum = 0;
            for (int i = 0; i < GameBoardRows; i++)
            {
                for (int j = 0; j < GameBoardCols; j++)
                {
                    sum += puzzleGrid.Grid[i, j];
                }
            }

            // if the sum is greater than 0 that means there is a user editable value
            var inProgress = false;
            if (sum > 0)
            {
                inProgress = true;
            }

            return inProgress;
        }

        /// <summary>
        /// Creates a new game.
        /// </summary>
        private void MakeNewGame(bool noCancel=false)
        {
            var dlg = new NewGameDifficultyDialogBox(noCancel) {Owner = this};
            dlg.ShowDialog();
            var difficulty = dlg.HowHard;

            if (dlg.CreateGame)
            {
                var puzzleGenerator = new PuzzleGenerator(difficulty);
                PuzzleGrid newPuzzleGrid = puzzleGenerator.InitGrid();

                SetPuzzleGrid(newPuzzleGrid);
                puzzleGrid = newPuzzleGrid;
            }
        }

        // The following methods are GUI event handlers. The two arguments are the same for all event handlers.
        /// <summary>
        /// This function is called when the exit menu item is clicked.
        /// </summary>
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// This function sets the show hints option to on, and is called when the show hints menu item is checked.
        /// </summary>
        private void AlwaysShowHintsChecked(object sender, RoutedEventArgs e)
        {
            showHintsOn = true;
            SetHintsBoxToDefault();
            ShowHintText();
        }

        /// <summary>
        /// This sets the hints option to off, and is called when the show hints menu item is unchecked.
        /// </summary>
        private void AlwaysShowHintsUnchecked(object sender, RoutedEventArgs e)
        {
            showHintsOn = false;
            SetHintsBoxToDefault();
        }

        /// <summary>
        /// This function sets the show errors option to on and is called when the show errors menu option is checked.
        /// </summary>
        private void ShowErrorsChecked(object sender, RoutedEventArgs e)
        {
            showErrorsOn = true;
        }

        /// <summary>
        /// This function sets the show errors option to off and is called when the show errors menu option is unchecked.
        /// </summary>
        private void ShowErrorsUnchecked(object sender, RoutedEventArgs e)
        {
            showErrorsOn = false;
        }

        /// <summary>
        /// This function is called when the save game menu item is clicked.
        /// </summary>
        private void SaveGameClick(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        /// <summary>
        /// This function is called when the save game unsolved menu item is clicked.
        /// </summary>
        private void SaveGameUnsolvedClick(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        /// <summary>
        /// This function is called when the load game menu item is clicked.
        /// </summary>
        private void LoadGameClick(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        /// <summary>
        /// This function is called when a grid element is clicked or gets focused.
        /// </summary>
        private void GridElementGotFocus(object sender, RoutedEventArgs e)
        {
            if (showHintsOn)
            {
                ShowHintText();
            }
            else
            {
                SetHintsBoxToDefault();
            }

            ((TextBox)sender).Style = (Style)(Resources["GridElementFocused"]);
        }

	    /// <summary>
        /// This function is called when a grid element no longer has focus.
        /// </summary>
        private void GridElementLostFocus(object sender, RoutedEventArgs e)
        {
            if (showHintsOn)
            {
                SetHintsBoxToDefault();
            }
            ((TextBox)sender).Style = (Style)(Resources["GridElement"]);
        }

        /// <summary>
        /// This function is called when a grid item is selected and a key is pressed. It enforces 
        /// all the rules about how keys can manipulate the game grid.
        /// </summary>
        private void GridElementKeyDown(object sender, KeyEventArgs e)
        {
            var name = ((TextBox)sender).Name;

            // get the row/column of the text box
            var rowCol = GetRowColumnFromTextboxName(name);

            switch (e.Key)
            {
                    // When we're setting the boxes we also keep the game grid up to date.
                case Key.D1:
                case Key.NumPad1:
                    ((TextBox)sender).Text = "1";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 1);
                    break;
                case Key.D2:
                case Key.NumPad2:
                    ((TextBox)sender).Text = "2";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 2);
                    break;
                case Key.D3:
                case Key.NumPad3:
                    ((TextBox)sender).Text = "3";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 3);
                    break;
                case Key.D4:
                case Key.NumPad4:
                    ((TextBox)sender).Text = "4";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 4);
                    break;
                case Key.D5:
                case Key.NumPad5:
                    ((TextBox)sender).Text = "5";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 5);
                    break;
                case Key.D6:
                case Key.NumPad6:
                    ((TextBox)sender).Text = "6";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 6);
                    break;
                case Key.D7:
                case Key.NumPad7:
                    ((TextBox)sender).Text = "7";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 7);
                    break;
                case Key.D8:
                case Key.NumPad8:
                    ((TextBox)sender).Text = "8";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 8);
                    break;
                case Key.D9:
                case Key.NumPad9:
                    ((TextBox)sender).Text = "9";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 9);
                    break;
                case Key.Delete:
                case Key.Back:
                case Key.D0:
                case Key.NumPad0:
                case Key.Space:
                    ((TextBox)sender).Text = "";
                    puzzleGrid.InitSetCell(rowCol.Item1, rowCol.Item2, 0);
                    break;
                case Key.Left:
                    // Calc left item and set it to be focused
                    var left = (TextBox) FindName(GetMovedGridItem(name, Direction.Left));
                    if (left != null)
                    {
                        left.Focus();
                    }
                    break;
                case Key.Right:
                    var right = (TextBox) FindName(GetMovedGridItem(name, Direction.Right));
                    if (right != null)
                    {
                        right.Focus();
                    }
                    break;
                case Key.Up:
                    var above = (TextBox) FindName(GetMovedGridItem(name, Direction.Up));
                    if (above != null)
                    {
                        above.Focus();
                    }
                    break;
                case Key.Down:
                    var below = (TextBox) FindName(GetMovedGridItem(name, Direction.Down));
                    if (below != null)
                    {
                        below.Focus();
                    }
                    break;
            }
        }

        /// <summary>
        /// This function is also called when buttons are pressed, but supresses some keys like the spacebar. 
        /// I modified it so that it passes all key signals down to the function above this.
        /// </summary>
        private void GridElementPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // We need to send these key presses down the line, otherwise
            // they're blocked by this method.
            GridElementKeyDown(sender, e);
        }

        /// <summary>
        /// This function is called when we need to solve the puzzle.
        /// </summary>
        private void SolveNowClick(object sender, RoutedEventArgs e)
        {
            puzzleSolver = new PuzzleSolver();
            var result = puzzleSolver.SolveGrid(puzzleGrid, true);
            if (result == true)
            {
                SetPuzzleGrid(puzzleSolver.SolutionGrid);
                MessageBox.Show("The current puzzle has been solved.");
            }
            else
            {
                MessageBox.Show("The current puzzle cannot be solved.");
            }
        }

        /// <summary>
        /// This function is called when we need a new puzzle.
        /// </summary>
	    private void NewGameClick(object sender, RoutedEventArgs e)
        {
            // save game if we need to))
            if (GameInProgress())
            {
                var save = MessageBox.Show("Save current game before creating a new one?", 
                    "Save current game?", MessageBoxButton.YesNo);
                if (save == MessageBoxResult.Yes)
                {
                    SaveGameClick(sender, e);
                }
            }

            // create new game
            MakeNewGame();
        }

        private void ShowHintClick(object sender, RoutedEventArgs e)
        {
            ShowHintText();
        }

	    private void FillInAnswerClick(object sender, RoutedEventArgs e)
	    {
	        throw new NotImplementedException();
	    }

        private void GameWindowLoaded(object sender, RoutedEventArgs e)
        {
            MakeNewGame(true);
        }

        private void EnterPuzzleClick(object sender, RoutedEventArgs e)
        {
            // save game if we need to
            if (GameInProgress())
            {
                var save = MessageBox.Show("Save current game before creating a new one?",
                    "Save current game?", MessageBoxButton.YesNo);
                if (save == MessageBoxResult.Yes)
                {
                    SaveGameClick(sender, e);
                }
            }

            EnterPuzzleEntryState();
        }

	    private void EntryCompleteClick(object sender, RoutedEventArgs e)
	    {
            puzzleSolver = new PuzzleSolver();
            var result = puzzleSolver.SolveGrid((PuzzleGrid)puzzleGrid.Clone(), true);
            if (result == true)
            {
                LeavePuzzleEntryState();
            }
            else
            {
                MessageBox.Show("That puzzle is not solvable, please modify it so that it is.");
            }
	    }
	}
}