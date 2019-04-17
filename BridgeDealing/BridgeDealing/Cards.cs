namespace BridgeDealing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Cards
    {
        private readonly List<Card> _deck;

        public Cards()
        {
            _deck = new List<Card>();
            ShuffledDeck = new List<Card>();

            InitialiseDeck();
        }
        public List<List<Card>> Deal()
        {
            var player1Cards = new List<Card>();
            var player2Cards = new List<Card>();
            var player3Cards = new List<Card>();
            var player4Cards = new List<Card>();

            for (var cardNo = 0; cardNo < 13; cardNo++)
            {
                var player1Card = _deck.Where(p => !ShuffledDeck.Contains(p)).OrderBy(p => p.RandomSequence).Take(1).First();
                ShuffledDeck.Add(player1Card);
                player1Cards.Add(player1Card);
                var player2Card = _deck.Where(p => !ShuffledDeck.Contains(p)).OrderBy(p => p.RandomSequence).Take(1).First();
                ShuffledDeck.Add(player2Card);
                player2Cards.Add(player2Card);
                var player3Card = _deck.Where(p => !ShuffledDeck.Contains(p)).OrderBy(p => p.RandomSequence).Take(1).First();
                ShuffledDeck.Add(player3Card);
                player3Cards.Add(player3Card);
                var player4Card = _deck.Where(p => !ShuffledDeck.Contains(p)).OrderBy(p => p.RandomSequence).Take(1).First();
                ShuffledDeck.Add(player4Card);
                player4Cards.Add(player4Card);
            }

            return new List<List<Card>>
            {
                player1Cards.OrderBy(p => GetSuitSequence(player1Cards)[p.Suit]).ThenBy(p => p.BridgeCard).ToList(),
                player2Cards.OrderBy(p => GetSuitSequence(player2Cards)[p.Suit]).ThenBy(p => p.BridgeCard).ToList(),
                player3Cards.OrderBy(p => GetSuitSequence(player3Cards)[p.Suit]).ThenBy(p => p.BridgeCard).ToList(),
                player4Cards.OrderBy(p => GetSuitSequence(player4Cards)[p.Suit]).ThenBy(p => p.BridgeCard).ToList()
            };
        }

        public List<Card> ShuffledDeck { get; }

        private void InitialiseDeck()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            foreach (var suit in Enum.GetValues(typeof(Suits)).Cast<Suits>())
            {
                foreach (var bridgeCard in Enum.GetValues(typeof(BridgeCards)).Cast<BridgeCards>())
                {
                    _deck.Add(new Card {BridgeCard = bridgeCard, Suit = suit, RandomSequence = random.Next()});
                }
            }
        }

        private static Dictionary<Suits, int> GetSuitSequence(List<Card> cards)
        {
            var redSuits = new[] { Suits.Diamonds, Suits.Hearts };

            var distinctSuits = cards.Select(p => p.Suit).Distinct().ToList();

            if (distinctSuits.Count == 3)
            {
                if (distinctSuits.Intersect(redSuits).Count() == 2)
                {
                    return new Dictionary<Suits, int> { { Suits.Diamonds, 1 }, { Suits.Clubs, 2 }, { Suits.Spades, 3 }, { Suits.Hearts, 4 } };
                }
                return new Dictionary<Suits, int> { { Suits.Clubs, 1 }, { Suits.Diamonds, 2 }, { Suits.Hearts, 3 }, { Suits.Spades, 4 } };
            }
            if (distinctSuits.Count == 2)
            {
                return new Dictionary<Suits, int> { { Suits.Clubs, 1 }, { Suits.Diamonds, 2 }, { Suits.Hearts, 3 }, { Suits.Spades, 4 } };
            }
            return new Dictionary<Suits, int> { { Suits.Clubs, 1 }, { Suits.Diamonds, 2 }, { Suits.Spades, 3 }, { Suits.Hearts, 4 } };
        }
    }
}
