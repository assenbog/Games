namespace BridgeBeloteDealing.IO
{
    using BridgeBeloteDealing.CardDealing;
    using BridgeBeloteDealing.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class Output
    {
        public static void SaveDealResults(List<string> output)
        {
            const string dataFolderName = "BelotCardDealing";

            try
            {
                var executingAssemblyName = Assembly.GetEntryAssembly().Location;
                var executingAssemblyPath = Path.GetDirectoryName(executingAssemblyName);
                var now = DateTime.Now;
                var fullDataFolder = Path.Combine(executingAssemblyPath ?? string.Empty, dataFolderName);
                if (!Directory.Exists(fullDataFolder))
                {
                    Directory.CreateDirectory(fullDataFolder);
                }
                var dateTimeStamp = now.ToString("dd MMM yyyy HH_mm");
                var fileName = $"{dataFolderName} {dateTimeStamp}.txt";
                var fullFileName = Path.Combine(fullDataFolder, fileName);

                using (var writer = new StreamWriter(new FileStream(fullFileName, FileMode.OpenOrCreate, FileAccess.Write), Encoding.Unicode))
                {
                    output.ForEach(p => writer.WriteLine(p));
                }

                Console.WriteLine($"\n\nDealing data saved as \"{fullFileName}\"");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static List<string> FormattedOutput(List<List<Card>> initial5CardDealt, List<List<Card>> additional3CardDealt, int sequenceNo, int? shuffledSequenceNo, Sides dealer)
        {
            const int eastWestSpaceAdjuster = 56;
            const int leadingSpaceAdjuster = 25;

            var halfEastWestSpaceAdjuster = eastWestSpaceAdjuster / 2;

            var northCards = SideOutput(initial5CardDealt[0], additional3CardDealt[0], dealer == Sides.North);
            var westCards = SideOutput(initial5CardDealt[1], additional3CardDealt[1], dealer == Sides.West);
            var southCards = SideOutput(initial5CardDealt[2], additional3CardDealt[2], dealer == Sides.South);
            var eastCards = SideOutput(initial5CardDealt[3], additional3CardDealt[3], dealer == Sides.East);

            var shuffledSequenceNoAsString = shuffledSequenceNo.HasValue ? $" -> {shuffledSequenceNo.Value.ToString("##")}" : string.Empty;
            var prefix = shuffledSequenceNo.HasValue ? string.Empty : new string(' ', 3);
            var dealNumber = $"{prefix}# {sequenceNo:##}{shuffledSequenceNoAsString}";
            var currentDate = DateTime.Now.ToString("dd/MM/yy");

            return new List<string>
            {
                $"{new string(' ', leadingSpaceAdjuster)}{northCards[0]}",
                $"{new string(' ', leadingSpaceAdjuster)}{northCards[1]}",
                $"{new string(' ', leadingSpaceAdjuster)}{northCards[2]}",
                $"{new string(' ', leadingSpaceAdjuster)}{northCards[3]}",
                string.Empty,
                $"{westCards[0]}{new string(' ', eastWestSpaceAdjuster - westCards[0].Length)}{eastCards[0]}",
                $"{westCards[1]}{new string(' ', halfEastWestSpaceAdjuster - westCards[1].Length)}{dealNumber}{new string(' ', halfEastWestSpaceAdjuster - dealNumber.Length)}{eastCards[1]}",
                $"{westCards[2]}{new string(' ', halfEastWestSpaceAdjuster - westCards[2].Length)}{currentDate}{new string(' ', halfEastWestSpaceAdjuster - currentDate.Length)}{eastCards[2]}",
                $"{westCards[3]}{new string(' ', eastWestSpaceAdjuster - westCards[3].Length)}{eastCards[3]}",
                string.Empty,
                $"{new string(' ', leadingSpaceAdjuster)}{southCards[0]}",
                $"{new string(' ', leadingSpaceAdjuster)}{southCards[1]}",
                $"{new string(' ', leadingSpaceAdjuster)}{southCards[2]}",
                $"{new string(' ', leadingSpaceAdjuster)}{southCards[3]}"
            };
        }

        private static List<string> SideOutput(List<Card> initial5CardDealt, List<Card> additional3CardDealt, bool isDealing)
        {
            const int fiveThreesOffset = 3;

            var prefix = isDealing ? "| " : string.Empty;

            var clubsStringBuilder = new StringBuilder($"{prefix}\u2663");
            var diamondsStringBuilder = new StringBuilder($"{prefix}\u2666");
            var heartsStringBuilder = new StringBuilder($"{prefix}\u2665");
            var spadesStringBuilder = new StringBuilder($"{prefix}\u2660");

            var initial5CardSuits = initial5CardDealt.GroupBy(p => p.Suit).Select(g => new { Suit = g.Key, CardsInSuit = g.ToList() }).ToArray();

            var additional3CardSuits = additional3CardDealt.GroupBy(p => p.Suit).Select(g => new { Suit = g.Key, CardsInSuit = g.ToList() }).ToArray();

            var initialClubsCards = initial5CardSuits.FirstOrDefault(p => p.Suit == Suits.Clubs)?.CardsInSuit;
            var initialDiamondsCards = initial5CardSuits.FirstOrDefault(p => p.Suit == Suits.Diamonds)?.CardsInSuit;
            var initialHeartsCards = initial5CardSuits.FirstOrDefault(p => p.Suit == Suits.Hearts)?.CardsInSuit;
            var initialSpadesCards = initial5CardSuits.FirstOrDefault(p => p.Suit == Suits.Spades)?.CardsInSuit;

            var additionalClubsCards = additional3CardSuits.FirstOrDefault(p => p.Suit == Suits.Clubs)?.CardsInSuit;
            var additionalDiamondsCards = additional3CardSuits.FirstOrDefault(p => p.Suit == Suits.Diamonds)?.CardsInSuit;
            var additionalHeartsCards = additional3CardSuits.FirstOrDefault(p => p.Suit == Suits.Hearts)?.CardsInSuit;
            var additionalSpadesCards = additional3CardSuits.FirstOrDefault(p => p.Suit == Suits.Spades)?.CardsInSuit;

            if (initialSpadesCards != null && initialSpadesCards.Any())
            {
                // Spades
                initialSpadesCards.ForEach(p => spadesStringBuilder.Append(p.BelotCard.DescriptionAttr()));
            }

            if (initialHeartsCards != null && initialHeartsCards.Any())
            {
                // Hearts
                initialHeartsCards.ForEach(p => heartsStringBuilder.Append(p.BelotCard.DescriptionAttr()));
            }

            if (initialClubsCards != null && initialClubsCards.Any())
            {
                // Clubs
                initialClubsCards.ForEach(p => clubsStringBuilder.Append(p.BelotCard.DescriptionAttr()));
            }

            if (initialDiamondsCards != null && initialDiamondsCards.Any())
            {
                // Diamonds
                initialDiamondsCards.ForEach(p => diamondsStringBuilder.Append(p.BelotCard.DescriptionAttr()));
            }

            var initialMaxLength = new[] { spadesStringBuilder.Length, heartsStringBuilder.Length, clubsStringBuilder.Length, diamondsStringBuilder.Length }.Max() + fiveThreesOffset;

            AddAdditionalCards(additionalSpadesCards, spadesStringBuilder, initialMaxLength - spadesStringBuilder.Length);
            AddAdditionalCards(additionalHeartsCards, heartsStringBuilder, initialMaxLength - heartsStringBuilder.Length);
            AddAdditionalCards(additionalClubsCards, clubsStringBuilder, initialMaxLength - clubsStringBuilder.Length);
            AddAdditionalCards(additionalDiamondsCards, diamondsStringBuilder, initialMaxLength - diamondsStringBuilder.Length);

            return new List<string> { spadesStringBuilder.ToString(), heartsStringBuilder.ToString(), clubsStringBuilder.ToString(), diamondsStringBuilder.ToString() }; ;
        }

        private static void AddAdditionalCards(List<Card> additionalCards, StringBuilder sb, int initialAdditionalSeparator)
        {
            if (additionalCards != null && additionalCards.Any())
            {
                sb.Append(new string(' ', initialAdditionalSeparator));
                additionalCards.ForEach(p => sb.Append(p.BelotCard.DescriptionAttr()));
            }
        }
    }
}
