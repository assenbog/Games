namespace BridgeBeloteDealing
{
    using BridgeBeloteDealing.CardDealing;
    using BridgeBeloteDealing.IO;
    using BridgeBeloteDealing.IO.EF;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
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
                    // Discard deals containing sequences longer that specified
                    continue;
                }

                dealings.Add(dealing);

                // Note: No shuffled sequence numbers in the initial dealings set
                var formattedOutput = Output.FormattedOutput(initial5CardDealt, additional3CardDealt, dealing.SequenceNo, dealing.ShuffledSequenceNo + maxDealCountValue, dealingSide);

                Console.WriteLine($"\n\nРаздаване #{dealSequence}\n");

                formattedOutput.ForEach(p => Console.WriteLine(p));

                Console.Write("\n\nEsc - Ignore, any other key - Use ... ");

                var keyPress = Console.ReadKey();

                if(keyPress.Key == ConsoleKey.Escape)
                {
                    continue;
                }

                formattedOutput.ForEach(p => outputData.Add(p));

                outputData.Add(string.Empty);

                dealingSide = (Sides)(((int)dealingSide + 1) % 4);

                dealSequence++;
            }

            dealingSide = Sides.North;

            // Now add the rotated and shuffled second dealings set
            foreach (var dealing in dealings.OrderBy(p => p.ShuffledSequenceNo))
            {
                outputData.Add(string.Empty);
                outputData.Add(new string('=', 80));
                outputData.Add(string.Empty);

                var formattedOutput = Output.FormattedOutput(dealing.Initial5CardsDealt, dealing.Additional3CardsDealt, dealing.ShuffledSequenceNo + maxDealCountValue, dealing.SequenceNo, dealingSide);

                dealingSide = (Sides)(((int)dealingSide + 1) % 4);

                formattedOutput.ForEach(p => outputData.Add(p));
            }

            Output.SaveDealResults(outputData);

            // Only save to DB when configured to do so
            if (saveToDatabaseParseSuccess && saveToDatabaseValue)
            {
                DbPersistence.SaveAllDealings(dealings, sortOrders);
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
