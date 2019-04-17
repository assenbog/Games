namespace BridgeBeloteDealing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;

    class Program
    {
        static void Main()
        {
            const int MaxDealCount = 32;

            var dealCount = 1;

            var dealingSide = Sides.North;

            var outputData = new List<string>();

            var sortOrderString = ConfigurationManager.AppSettings["SortOrder"];

            Console.OutputEncoding = Encoding.Unicode;

            if (string.IsNullOrEmpty(sortOrderString) || !Enum.TryParse<SortOrders>(sortOrderString, out var sortOrders))
            {
                sortOrders = SortOrders.Suit;
            }

            Console.WriteLine("Карти за Бридж Белот");
            Console.WriteLine("====================");

            while (dealCount <= MaxDealCount)
            {
                if(outputData.Count > 0)
                {
                    outputData.Add(string.Empty);
                    outputData.Add(new string('=', 80));
                    outputData.Add(string.Empty);
                }

                var cards = new Cards(sortOrders);

                var cardsDealt = cards.CardsDealt();
                var initial5CardDealt = cards.Initial5CardsDealt;
                var additional3CardDealt = cards.Additional3CardsDealt;

                if (Rules.Discard(cardsDealt, 5))
                {
                    // Discard cards dealt where there are either 4 of a kind, e.g. 4 Jacks, 
                    // or a player suite where there are 5 or more long sequences, e.g. 9 10 J Q K
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
