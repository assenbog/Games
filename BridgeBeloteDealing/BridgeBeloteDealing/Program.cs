namespace BridgeBeloteDealing
{
    using BridgeBeloteDealing.Dealing;
    using BridgeBeloteDealing.IO;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;

    class Program
    {
        static void Main()
        {
            const int DefaultMaxDealCount = 32;

            var sortOrders = SortOrders.Suit;
            var allow4OfAKind = true;
            var maxSequenceLength = 0;

            var dealCount = 1;

            var dealingSide = Sides.North;

            var outputData = new List<string>();

            var sortBySuitSetting = ConfigurationManager.AppSettings["SortBySuit"];
            var allow4OfAKindSetting = ConfigurationManager.AppSettings["Allow4OfAKind"];
            var maxSequenceLengthSetting = ConfigurationManager.AppSettings["MaxSequenceLength"];
            var maxDealCountSetting = ConfigurationManager.AppSettings["MaxDealCount"];
            var sortBySuitParamParseSuccess = bool.TryParse(sortBySuitSetting, out var sortBySuitValue);
            var allow4OfAKindParamParseSuccess = bool.TryParse(allow4OfAKindSetting, out var allow4OfAKindValue);
            var maxSequenceLengthParamParseSuccess = int.TryParse(maxSequenceLengthSetting, out var maxSequenceLengthValue);
            var maxDealCountParamParseSuccess = int.TryParse(maxDealCountSetting, out var maxDealCountValue);

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

            Console.WriteLine("Карти за Бридж Белот");
            Console.WriteLine("====================");

            while (dealCount <= maxDealCountValue)
            {
                if(outputData.Count > 0)
                {
                    outputData.Add(string.Empty);
                    outputData.Add(new string('=', 80));
                    outputData.Add(string.Empty);
                }

                var cards = new Dealing.Dealing(sortOrders);

                var cardsDealt = cards.CardsDealt();
                var initial5CardDealt = cards.Initial5CardsDealt;
                var additional3CardDealt = cards.Additional3CardsDealt;

                if (!allow4OfAKind && Rules.FourOfAKindCheck(cardsDealt))
                {
                    // Discard deals containing 4 of a kind
                    continue;
                }

                if (maxSequenceLength > 0 && Rules.LongSequencesCheck(cardsDealt, maxSequenceLength))
                {
                    // Discard deals containing sequences longer that specified
                    continue;
                }

                var formattedOutput = Output.FormattedOutput(initial5CardDealt, additional3CardDealt, dealCount, dealingSide);

                Console.WriteLine($"\n\nРаздаване #{dealCount}\n");

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

                dealCount++;
            }

            Output.SaveDealResults(outputData);

            Console.Write("\nPress any key to exit ... ");

            Console.ReadKey();
        }
    }
}
