namespace BridgeBeloteDealing
{
    using BridgeBeloteLogic.CardDealing;
    using BridgeBeloteLogic.IO;
    using BridgeBeloteLogic.IO.EF;
    using BridgeBeloteLogic.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;

    class Program
    {
        static void Main()
        {
            const int DefaultMaxDealCount = 32;

            var sortOrders = SortOrders.Suit;
            var allow4OfAKind = true;
            var maxSequenceLength = 0;

            var dealings = new List<Dealing>();

            var dealSequence = 1;

            var dealingSide = Sides.North;

            var outputData = new List<string>();

            var sortBySuitSetting = ConfigurationManager.AppSettings["SortBySuit"];
            var allow4OfAKindSetting = ConfigurationManager.AppSettings["Allow4OfAKind"];
            var maxSequenceLengthSetting = ConfigurationManager.AppSettings["MaxSequenceLength"];
            var maxDealCountSetting = ConfigurationManager.AppSettings["MaxDealCount"];
            var saveToDatabaseSetting = ConfigurationManager.AppSettings["SaveToDatabase"];

            var sortBySuitParamParseSuccess = bool.TryParse(sortBySuitSetting, out var sortBySuitValue);
            var allow4OfAKindParamParseSuccess = bool.TryParse(allow4OfAKindSetting, out var allow4OfAKindValue);
            var maxSequenceLengthParamParseSuccess = int.TryParse(maxSequenceLengthSetting, out var maxSequenceLengthValue);
            var maxDealCountParamParseSuccess = int.TryParse(maxDealCountSetting, out var maxDealCountValue);
            var saveToDatabaseParseSuccess = bool.TryParse(saveToDatabaseSetting, out var saveToDatabaseValue);

            Console.OutputEncoding = Encoding.Unicode;

            if (sortBySuitParamParseSuccess && !sortBySuitValue)
            {
                sortOrders = SortOrders.NoTrumps;
            }

            if(allow4OfAKindParamParseSuccess && !allow4OfAKindValue)
            {
                allow4OfAKind = false;
            }

            if (maxSequenceLengthParamParseSuccess && maxSequenceLengthValue > 0 && maxSequenceLengthValue < 8)
            {
                maxSequenceLength = maxSequenceLengthValue;
            }

            if(!maxDealCountParamParseSuccess)
            {
                maxDealCountValue = DefaultMaxDealCount;
            }

            var shuffledSequenceNos = ShuffledSequenceNos(maxDealCountValue);

            var output = new Output();
            var input = new Input();

            var keyPress = new ConsoleKeyInfo();

            var tempFileName = output.TempFileNameWithoutExt + "xml";

            if (File.Exists(tempFileName))
            {
                Console.WriteLine("\nНаличен частичен файл с резултати. Искате ли да го използвате (Y/N)?");
                keyPress = Console.ReadKey();
                if(keyPress.Key == ConsoleKey.Y)
                {
                    var tempDealings = input.DeserialiseFromXml(tempFileName);
                    dealings.AddRange(tempDealings);
                    dealSequence = dealings.Count + 1;
                    var lastDealingSide = (int)tempDealings.Last().DealingSide;
                    dealingSide = (Sides)(((int)lastDealingSide + 1) % 4);
                }
            }

            Console.WriteLine("Карти за Бридж Белот");
            Console.WriteLine("====================");

            while (dealSequence <= maxDealCountValue)
            {
                if(outputData.Count > 0)
                {
                    outputData.Add(string.Empty);
                    outputData.Add(new string('=', 80));
                    outputData.Add(string.Empty);
                }

                var dealing = new Dealing(sortOrders, dealSequence, shuffledSequenceNos[dealSequence - 1], dealingSide);

                var allCardsDealt = dealing.AllCardsDealt;
                var initial5CardDealt = dealing.Initial5CardsDealt;
                var additional3CardDealt = dealing.Additional3CardsDealt;

                if (!allow4OfAKind && Rules.FourOfAKindCheck(allCardsDealt))
                {
                    // Discard deals containing 4 of a kind
                    continue;
                }

                if (maxSequenceLength > 0 && Rules.LongSequencesCheck(allCardsDealt, maxSequenceLength))
                {
                    // Discard deals containing sequences longer than specified
                    continue;
                }

                do
                {
                    // Note: No shuffled sequence numbers in the initial dealings set
                    var formattedOutput = output.FormattedOutput(initial5CardDealt, additional3CardDealt, dealSequence, default, dealingSide);

                    Console.WriteLine($"\n\nРаздаване #{dealSequence}\n");

                    formattedOutput.ForEach(p => Console.WriteLine(p));

                    var discardLast = dealSequence > 1 ? "D - Discard last, " : string.Empty;

                    Console.Write($"\n\nEsc - Ignore, R - Rotate, {discardLast}P - Pause and save to temp file, any other key - Use ... ");

                    keyPress = Console.ReadKey();

                    switch (keyPress.Key)
                    {
                        case ConsoleKey.D:
                            dealings.RemoveAt(dealings.Count - 1);
                            dealSequence--;
                            var perviousDealingSide = (int)dealingSide - 1;
                            if(perviousDealingSide < 0)
                            {
                                perviousDealingSide = 3;
                            }
                            dealingSide = (Sides)(perviousDealingSide % 4);
                            break;
                        case ConsoleKey.Escape:
                            break;
                        case ConsoleKey.P:
                            output.SerialiseToXml(dealings, true);
                            Console.Write("\nPress any key to exit ... ");
                            Console.ReadKey();
                            return;
                        case ConsoleKey.R:
                            allCardsDealt = allCardsDealt.MoveFirstItemToEndOfList();
                            initial5CardDealt = initial5CardDealt.MoveFirstItemToEndOfList();
                            additional3CardDealt = additional3CardDealt.MoveFirstItemToEndOfList();
                            continue;
                        default: 
                            break; 
                    }
                }
                while (keyPress.Key == ConsoleKey.R || keyPress.Key == ConsoleKey.D);

                if (keyPress.Key == ConsoleKey.Escape)
                {
                    continue;
                }

                dealings.Add(dealing);

                dealingSide = (Sides)(((int)dealingSide + 1) % 4);

                dealSequence++;
            }

            // Shuffled dealings within each side
            var northShuffledDealings = dealings.Where(p => p.DealingSide == Sides.North).OrderBy(p => p.ShuffledSequenceNo).ToList();
            var westShuffledDealings = dealings.Where(p => p.DealingSide == Sides.West).OrderBy(p => p.ShuffledSequenceNo).ToList();
            var southShuffledDealings = dealings.Where(p => p.DealingSide == Sides.South).OrderBy(p => p.ShuffledSequenceNo).ToList();
            var eastShuffledDealings = dealings.Where(p => p.DealingSide == Sides.East).OrderBy(p => p.ShuffledSequenceNo).ToList();

            // Shuffled dealings: Combined random ordered sides in a sequence, i.e. North, West, South and last  - East
            var shuffledDealings = new List<Dealing>();

            // Each side will have the same number of dealings as the rest, so iterate over North to combine into a single list
            for (var i = 0; i < northShuffledDealings.Count; i++)
            {
                shuffledDealings.Add(northShuffledDealings[i]);
                shuffledDealings.Add(westShuffledDealings[i]);
                shuffledDealings.Add(southShuffledDealings[i]);
                shuffledDealings.Add(eastShuffledDealings[i]);
            }

            // Update the custom order sequencing
            for (var i = 0; i < shuffledDealings.Count; i++)
            {
                shuffledDealings[i].ShuffledSequenceNo = maxDealCountValue + i + 1;
            }

            dealingSide = Sides.North;

            // The original set
            foreach (var dealing in dealings)
            {
                outputData.Add(string.Empty);
                outputData.Add(new string('=', 80));
                outputData.Add(string.Empty);

                var formattedOutput = output.FormattedOutput(dealing.Initial5CardsDealt, dealing.Additional3CardsDealt, dealing.SequenceNo, dealing.ShuffledSequenceNo, dealingSide);

                dealingSide = (Sides)(((int)dealingSide + 1) % 4);

                formattedOutput.ForEach(p => outputData.Add(p));
            }

            dealingSide = Sides.North;

            // Now add the shuffled second dealings set
            foreach (var dealing in shuffledDealings)
            {
                outputData.Add(string.Empty);
                outputData.Add(new string('=', 80));
                outputData.Add(string.Empty);

                var formattedOutput = output.FormattedOutput(dealing.Initial5CardsDealt, dealing.Additional3CardsDealt, dealing.ShuffledSequenceNo, dealing.SequenceNo, dealingSide);

                dealingSide = (Sides)(((int)dealingSide + 1) % 4);

                formattedOutput.ForEach(p => outputData.Add(p));
            }

            output.SaveDealResults(outputData);
            output.SerialiseToXml(dealings);

            // Only save to DB when configured to do so
            if (saveToDatabaseParseSuccess && saveToDatabaseValue)
            {
                DbPersistence.SaveAllDealings(dealings, sortOrders);
            }

            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }

            Console.Write("\nPress any key to exit ... ");

            Console.ReadKey();
        }

        private static List<int> ShuffledSequenceNos(int maxDealCount)
        {
            var dealingRandomSequence = new Random(Guid.NewGuid().GetHashCode());

            var shuffledSequenceList = new List<int>();

            while(shuffledSequenceList.Count < maxDealCount)
            {
                // Next range doesn't include the max values, i.e. we have an [min, max) interval
                var nextSequenceNo = dealingRandomSequence.Next(1, maxDealCount + 1);

                if(!shuffledSequenceList.Contains(nextSequenceNo))
                {
                    shuffledSequenceList.Add(nextSequenceNo);
                }
            }

            return shuffledSequenceList;
        }
    }
}
