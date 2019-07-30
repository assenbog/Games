namespace BridgeBeloteDealing.CardDealing
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class Card
    {
        public Suits Suit { get; set; }

        public Sides Side { get; set; }

        public Stages Stage { get; set; }

        public BeloteCards BelotCard { get; set; }

        [XmlIgnore]
        public int RandomSequence { get; set; }
    }
}
