namespace BridgeBeloteDealing
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Rules
    {
        private static readonly List<BeloteCards> CardsThatCount;
        private static readonly List<BeloteCards> AllBelotCards;

        static Rules()
        {
            CardsThatCount = new List<BeloteCards> { BeloteCards.Nine, BeloteCards.Ten, BeloteCards.Jack, BeloteCards.Queen, BeloteCards.King, BeloteCards.Ace };
            AllBelotCards = new List<BeloteCards> { BeloteCards.Seven, BeloteCards.Eight, BeloteCards.Nine, BeloteCards.Ten, BeloteCards.Jack, BeloteCards.Queen, BeloteCards.King, BeloteCards.Ace };
        }

        public static bool Discard(List<List<Card>> allCardsDealt, int maxLengthDiscard)
        {
            return FourOfAKindCheck(allCardsDealt) || LongSequencesCheck(allCardsDealt, maxLengthDiscard);
        }

        private static bool FourOfAKindCheck(List<List<Card>> allCardsDealt)
        {
            foreach (var cardThatCounts in CardsThatCount)
            {
                if (allCardsDealt.Any(p => p.All(q => q.BelotCard == cardThatCounts)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool LongSequencesCheck(List<List<Card>> allCardsDealt, int maxLengthDiscard)
        {
            var skip = 0;

            while (true)
            {
                var sequence = AllBelotCards.Skip(skip++).Take(maxLengthDiscard).ToList();

                if (sequence.Count < maxLengthDiscard)
                {
                    return false;
                }

                foreach (var cardsDealt in allCardsDealt)
                {
                    var sameSuitCards = cardsDealt.GroupBy(p => p.Suit, p => p.BelotCard, (key, value) => new { Key = key, SuitCards = value.ToList()});

                    foreach (var suitCardsDealt in sameSuitCards)
                    {
                        if (suitCardsDealt.SuitCards.Intersect(sequence).Count() == maxLengthDiscard)
                        {
                            return true;
                        }
                    }
                }
            }
        }
    }
}
