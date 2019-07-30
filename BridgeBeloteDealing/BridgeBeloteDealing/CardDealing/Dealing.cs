namespace BridgeBeloteDealing.CardDealing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    [Serializable]
    public class Dealing
    {
        private readonly List<Card> _alreadyDealt;
        private readonly List<Card> _deck;
        private Dictionary<BeloteCards, int> _belotCardsSuitOrder;
        private Dictionary<BeloteCards, int> _belotCardsNoTrumpsOrder;
        private readonly SortOrders _sortOrders;

        public Dealing()
        {
            // Note: required for serialisation
        }

        public Dealing(SortOrders sortOrders, int sequenceNo, int shuffledSequenceNo, Sides dealingSide)
        {
            _sortOrders = sortOrders;

            SequenceNo = sequenceNo;
            ShuffledSequenceNo = shuffledSequenceNo;
            DealingSide = dealingSide;

            _deck = new List<Card>();
            _alreadyDealt = new List<Card>();

            InitialiseDeck();

            DealTheCards();
        }

        public Sides DealingSide { get; set; }

        public int SequenceNo { get; set; }

        public int ShuffledSequenceNo { get; set; }

        [XmlIgnore]
        public List<List<Card>> Initial5CardsDealt { get; set; }

        [XmlIgnore]
        public List<List<Card>> Additional3CardsDealt { get; set; }

        public List<List<Card>> AllCardsDealt { get; set; }

        private void DealTheCards()
        {
            Initial5CardsDealt = GetInitial5CardsDealt();
            Additional3CardsDealt = GetAdditional3CardsDealt();

            var cardsDealt = new List<List<Card>> { new List<Card>(Initial5CardsDealt[0]), new List<Card>(Initial5CardsDealt[1]), new List<Card>(Initial5CardsDealt[2]), new List<Card>(Initial5CardsDealt[3]) };

            for (var i = 0; i < 4; i++)
            {
                cardsDealt[i].AddRange(Additional3CardsDealt[i]);
            }

            AllCardsDealt = new List<List<Card>>
            {
                cardsDealt[0].OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList(),
                cardsDealt[1].OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList(),
                cardsDealt[2].OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList(),
                cardsDealt[3].OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList()
            };
        }

        private List<List<Card>> GetInitial5CardsDealt()
        {
            var playerNorthThreeCards = _deck.OrderBy(p => p.RandomSequence).Take(3).ToList();
            _alreadyDealt.AddRange(playerNorthThreeCards);
            var playerWestThreeCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(3).ToList();
            _alreadyDealt.AddRange(playerWestThreeCards);
            var playerSouthThreeCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(3).ToList();
            _alreadyDealt.AddRange(playerSouthThreeCards);
            var playerEastThreeCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(3).ToList();
            _alreadyDealt.AddRange(playerEastThreeCards);

            var playerNorthTwoCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(2).ToList();
            _alreadyDealt.AddRange(playerNorthTwoCards);
            var playerWestTwoCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(2).ToList();
            _alreadyDealt.AddRange(playerWestTwoCards);
            var playerSouthTwoCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(2).ToList();
            _alreadyDealt.AddRange(playerSouthTwoCards);
            var playerEastTwoCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(2).ToList();
            _alreadyDealt.AddRange(playerEastTwoCards);

            var playerNorthCards = new List<Card>(playerNorthThreeCards);
            playerNorthCards.AddRange(playerNorthTwoCards);
            var playerWestCards = new List<Card>(playerWestThreeCards);
            playerWestCards.AddRange(playerWestTwoCards);
            var playerSouthCards = new List<Card>(playerSouthThreeCards);
            playerSouthCards.AddRange(playerSouthTwoCards);
            var playerEastCards = new List<Card>(playerEastThreeCards);
            playerEastCards.AddRange(playerEastTwoCards);

            playerNorthCards.ForEach(p => { p.Side = Sides.North; p.Stage = Stages.FiveCards; });
            playerWestCards.ForEach(p => { p.Side = Sides.West; p.Stage = Stages.FiveCards; });
            playerSouthCards.ForEach(p => { p.Side = Sides.South; p.Stage = Stages.FiveCards; });
            playerEastCards.ForEach(p => { p.Side = Sides.East; p.Stage = Stages.FiveCards; });

            return new List<List<Card>>
            {
                playerNorthCards.OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList(),
                playerWestCards.OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList(),
                playerSouthCards.OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList(),
                playerEastCards.OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList()
            };
        }

        private List<List<Card>> GetAdditional3CardsDealt()
        {
            var playerNorthThreeCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(3).ToList();
            var playerNorthOrderedThreeCards = playerNorthThreeCards.OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList();
            _alreadyDealt.AddRange(playerNorthOrderedThreeCards);
            var playerWestThreeCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(3).ToList();
            var playerWestOrderedThreeCards = playerWestThreeCards.OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList();
            _alreadyDealt.AddRange(playerWestOrderedThreeCards);
            var playerSouthThreeCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(3).ToList();
            var playerSouthOrderedThreeCards = playerSouthThreeCards.OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList();
            _alreadyDealt.AddRange(playerSouthOrderedThreeCards);
            var playerEastThreeCards = _deck.Where(p => !_alreadyDealt.Contains(p)).OrderBy(p => p.RandomSequence).Take(3).ToList();
            var playerEastOrderedThreeCards = playerEastThreeCards.OrderBy(p => p.Suit).ThenBy(p => _sortOrders == SortOrders.Suit ? _belotCardsSuitOrder[p.BelotCard] : _belotCardsNoTrumpsOrder[p.BelotCard]).ToList();
            _alreadyDealt.AddRange(playerEastOrderedThreeCards);

            var playerNorthCards = new List<Card>(playerNorthOrderedThreeCards);
            var playerWestCards = new List<Card>(playerWestOrderedThreeCards);
            var playerSouthCards = new List<Card>(playerSouthOrderedThreeCards);
            var playerEastCards = new List<Card>(playerEastOrderedThreeCards);

            playerNorthCards.ForEach(p => { p.Side = Sides.North; p.Stage = Stages.ThreeCards; });
            playerWestCards.ForEach(p => { p.Side = Sides.West; p.Stage = Stages.ThreeCards; });
            playerSouthCards.ForEach(p => { p.Side = Sides.South; p.Stage = Stages.ThreeCards; });
            playerEastCards.ForEach(p => { p.Side = Sides.East; p.Stage = Stages.ThreeCards; });

            return new List<List<Card>> { playerNorthCards, playerWestCards, playerSouthCards, playerEastCards };
        }

        private void InitialiseDeck()
        {
            var cardRandomSequence = new Random(Guid.NewGuid().GetHashCode());

            foreach (var suit in Enum.GetValues(typeof(Suits)).Cast<Suits>())
            {
                foreach (var belotCard in Enum.GetValues(typeof(BeloteCards)).Cast<BeloteCards>())
                {
                    _deck.Add(new Card { BelotCard = belotCard, Suit = suit, RandomSequence = cardRandomSequence.Next() });
                }
            }

            _belotCardsSuitOrder = new Dictionary<BeloteCards, int>
            {
                { BeloteCards.Jack, 1},
                { BeloteCards.Nine, 2},
                { BeloteCards.Ace, 3},
                { BeloteCards.Ten, 4},
                { BeloteCards.King, 5},
                { BeloteCards.Queen, 6},
                { BeloteCards.Eight, 7},
                { BeloteCards.Seven, 8}
            };

            _belotCardsNoTrumpsOrder = new Dictionary<BeloteCards, int>
            {
                { BeloteCards.Ace, 1},
                { BeloteCards.Ten, 2},
                { BeloteCards.King, 3},
                { BeloteCards.Queen, 4},
                { BeloteCards.Jack, 5},
                { BeloteCards.Nine, 6},
                { BeloteCards.Eight, 7},
                { BeloteCards.Seven, 8}
            };
        }
    }
}
