namespace BridgeBeloteDealing.CardDealing
{
    using System;

    [Serializable]
    public class Card
    {
        public Suits Suit { get; set; }

        public Sides Side { get; set; }

        public Stages Stage { get; set; }

        public BeloteCards BelotCard { get; set; }

        public int RandomSequence { get; set; }
    }
}
