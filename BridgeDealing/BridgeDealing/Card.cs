namespace BridgeDealing
{
    public class Card
    {
        public Suits Suit { get; set; }

        public BridgeCards BridgeCard { get; set; }

        public int Points
        {
            get
            {
                switch (BridgeCard)
                {
                    case BridgeCards.Ace:
                        return 4;
                    case BridgeCards.King:
                        return 3;
                    case BridgeCards.Queen:
                        return 2;
                    case BridgeCards.Jack:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        public int RandomSequence { get; set; }
    }
}
