namespace BridgeDealing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    class Program
    {
        static void Main()
        {
            const string nextStep = "Press \"Enter\" to deal again, \"S\" to save, or any other key to exit ... ";
            int dealCount = 0;
            var saveKeyChars = new[] {'s', 'S'};
            var players = new[] { "East", "South", "West", "North" };

            Console.OutputEncoding = Encoding.Unicode;

            while (true)
            {
                var cards = new Cards();

                var dealNumber = $"Deal #{++dealCount}";
                var dealText = $"* {dealNumber} *";
                var dealTextDelineator1 = $"*{new string(' ', dealText.Length - 2)}*";
                var dealTextDelineator2 = new string('*', dealText.Length);

                Console.WriteLine($"\n\n{dealTextDelineator2}");
                Console.WriteLine($"{dealTextDelineator1}");
                Console.WriteLine($"{dealText}");
                Console.WriteLine($"{dealTextDelineator1}");
                Console.WriteLine($"{dealTextDelineator2}\n");

                var cardDeal = cards.Deal();

                DisplayResults(cardDeal, players);

                DisplayShuffledDeck(cards.ShuffledDeck);

                Console.Write(nextStep);

                var key = Console.ReadKey();

                if (saveKeyChars.Contains(key.KeyChar))
                {
                    SaveDealResults(cardDeal, players, dealNumber, cards.ShuffledDeck);
                    Console.Write($"\n{nextStep}");
                    key = Console.ReadKey();
                }
                if (key.Key != ConsoleKey.Enter)
                {
                    break;
                }
            }
        }

        private static void SaveDealResults(List<List<Card>> cardDeal, string[] players, string dealNumber, List<Card> shuffledDeck)
        {
            const string dataFolderName = "DealingData";

            try
            {
                var executingAssemblyName = Assembly.GetEntryAssembly().Location;
                var executingAssemblyPath = Path.GetDirectoryName(executingAssemblyName);
                var now = DateTime.Now;
                var fullDataFolder = Path.Combine(executingAssemblyPath ?? string.Empty, dataFolderName);
                var shuffleDeckLabel = "Shuffled Deck";

                if (!Directory.Exists(fullDataFolder))
                {
                    Directory.CreateDirectory(fullDataFolder);
                }
                var fileName = $"{dealNumber}_{now.Day}-{now.Month}-{now.Year}_{now.Hour}_{now.Minute}_{now.Second}.txt";
                var fullFileName = Path.Combine(fullDataFolder, fileName);

                using (var writer = new StreamWriter(new FileStream(fullFileName, FileMode.Create, FileAccess.Write), Encoding.Unicode))
                {
                    for (var player = 0; player < 4; player++)
                    {
                        var currentPlayer = $"{players[player]} [{cardDeal[player].Sum(p => p.Points)} points]:";
                        writer.WriteLine($"{currentPlayer}");
                        writer.WriteLine(new string('=', currentPlayer.Length));
                        for (var card = 0; card < cardDeal[player].Count; card++)
                        {
                            writer.WriteLine($"{cardDeal[player][card].BridgeCard.DescriptionAttr()}{cardDeal[player][card].Suit.DescriptionAttr()}");
                        }

                        if (player < 3)
                        {
                            writer.WriteLine(string.Empty);
                        }
                    }

                    writer.WriteLine($"\n{shuffleDeckLabel}");
                    writer.WriteLine(new string('=', shuffleDeckLabel.Length));

                    foreach (var card in shuffledDeck)
                    {
                        writer.WriteLine($"{card.BridgeCard.DescriptionAttr()}{card.Suit.DescriptionAttr()}");
                    }

                    Console.WriteLine($"\n\n{dealNumber} saved as \"{fullFileName}\"");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void DisplayResults(List<List<Card>> shuffledDeck, string[] players)
        {
            for (var player = 0; player < 4; player++)
            {
                var currentPlayer = $"{players[player]} [{shuffledDeck[player].Sum(p => p.Points)} points]:";
                Console.WriteLine($"\n{currentPlayer}");
                Console.WriteLine(new string('=', currentPlayer.Length));
                for (var card = 0; card < shuffledDeck[player].Count; card++)
                {
                    WriteColourCard(shuffledDeck[player][card]);
                    Console.Write("\n");
                }
            }
            Console.WriteLine("\n");
        }

        private static void DisplayShuffledDeck(List<Card> shuffledDeck)
        {
            var shuffleDeckLabel = "Shuffled Deck";
            Console.WriteLine($"{shuffleDeckLabel}");
            Console.WriteLine(new string('=', shuffleDeckLabel.Length));

            foreach (var card in shuffledDeck)
            {
                WriteColourCard(card);
                Console.Write("\n");
            }
            Console.Write("\n");
        }

        private static void WriteColourCard(Card card)
        {
            var blackSuits = new List<Suits> {Suits.Clubs, Suits.Spades};

            Console.ForegroundColor = blackSuits.Contains(card.Suit) ? ConsoleColor.Black : ConsoleColor.Red;

            Console.Write($"{card.BridgeCard.DescriptionAttr()}{card.Suit.DescriptionAttr()}");

            Console.ResetColor();
        }
    }
}
