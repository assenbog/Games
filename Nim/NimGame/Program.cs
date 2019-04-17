using System;
using System.Collections.Generic;
using System.Linq;

namespace NimGame
{
    class Program
    {
        static void Main()
        {
            var nimItems = InitialiseWithDefaults();
            bool computersTurn = false;
            bool playAgain = true;
            bool givePlayerAChance = false;
            int givePlayerAChanceMoves = 2;
            int computerMoveCount;
            var nim = new Nim();

            Console.WriteLine("**************************");
            Console.WriteLine("****      N I M       ****");
            Console.WriteLine("**************************");
            Console.WriteLine("Computer and player take turns to delete any number of items from a selected row. The winner is the one who removes the last remaining item from the last remaining row.");

            while (playAgain)
            {
                computerMoveCount = 0;

                Console.WriteLine(computersTurn ? "\n\nComputer plays first." : "\n\nUser plays first.");

                while (nimItems.Any(p => p > 0))
                {
                    // Remove empty items
                    nimItems.Where(p => p == 0).ToList().ForEach(p => nimItems.Remove(p));
                    DisplayNumbers(nimItems);

                    if (computersTurn)
                    {
                        int row;
                        int items;
                        if(givePlayerAChance && ((computerMoveCount % givePlayerAChanceMoves) == 0))
                        {
                            // Delete one item from the longest row (probably not the right move)
                            row = nimItems.IndexOf(nimItems.Max());
                            items = 1;
                        }
                        else
                            {
                                var rowItems = nim.Move(nimItems);
                                row = rowItems.Row;
                                items = rowItems.Items;
                            }
                        nimItems[row] -= items;
                        Console.WriteLine("Computer move - remove {0} item{1} from row {2}", items, items > 1 ? "s" : string.Empty, row);
                        computersTurn = false;
                        computerMoveCount++;
                    }
                    else
                    {
                        var validMove = false;
                        var playerMove = new int[2];
                        while (!validMove)
                        {
                            Console.Write("\nPlayer move - please provide the row and items to delete from it in the format (row, items): ");
                            var line = Console.ReadLine();
                            try 
                            {
                                playerMove = line.Split(',').Select(p => Convert.ToInt32(p)).ToArray();
                                validMove = playerMove.Count() == 2 && playerMove[0] <= nimItems.Count() && nimItems[playerMove[0]] >= playerMove[1];
                            }
                            catch
                            {
                                validMove = false;
                            }
                        }
                        nimItems[playerMove[0]] -= playerMove[1];
                        computersTurn = true;
                    }
                }

                Console.WriteLine(computersTurn ? "\nPlayer wins!" : "\nComputer wins!!!");

                Console.Write("\n\nPlay again (Y/N)? ");
                var playAgainAnswer = Console.ReadKey();
                if (!playAgainAnswer.KeyChar.Equals('Y') && !playAgainAnswer.KeyChar.Equals('y'))
                {
                    Console.WriteLine("\n\nPress any key to exit ...");
                    Console.ReadKey();
                    playAgain = false;
                }
                else
                {
                    Console.Write("\n\nGive player a chance (Y/N)? ");
                    var givePlayerAChanceAnswer = Console.ReadKey();
                    if (givePlayerAChanceAnswer.KeyChar.Equals('Y') || givePlayerAChanceAnswer.KeyChar.Equals('y'))
                    {
                        givePlayerAChance = true;
                    }
                    else
                    {
                        givePlayerAChance = false;
                    }
    
                    Console.Write("\n\nWould you like to set stack sizes (Y/N)? ");
                    var setStackSizeAnswer = Console.ReadKey();
                    if (setStackSizeAnswer.KeyChar.Equals('Y') || setStackSizeAnswer.KeyChar.Equals('y'))
                    {
                        Console.Write("\n\nPlease enter a comma separated list of numbers: ");
                        var lineInput = Console.ReadLine();
                        try
                        {
                            var numbers = lineInput.Split(',');
                            nimItems = new List<int>(numbers.Select(p => Convert.ToInt32(p)));
                        }
                        catch
                        {
                            Console.WriteLine("Invalid user input. Stack numbers remain unchanged");
                        }
                    }
                    else
                    {
                        nimItems = InitialiseWithDefaults();
                    }
                    Console.Write("\n\nWould you like to play first (Y/N)? ");
                    var playFirstAnswer = Console.ReadKey();
                    if (playFirstAnswer.KeyChar.Equals('Y') || playFirstAnswer.KeyChar.Equals('y'))
                    {
                        computersTurn = false;
                    }
                    else
                    {
                        computersTurn = true;
                    }
                }
            }
        }

        static List<int> InitialiseWithDefaults()
        {
            return new List<int> {1, 3, 5, 7};
        }

        static void DisplayNumbers(List<int> items)
        {
            Console.WriteLine("\n");

            var maxItems = items.Max();

            for (var i = 0; i < items.Count; i++)
            {
                var leftPadding = (maxItems - items[i]) / 2;
                Console.WriteLine(string.Format("({0},{1})", i, items[i]) + "\t\t" + new String(' ', leftPadding) + new String('O', items[i]));
            }
        }
    }


}
