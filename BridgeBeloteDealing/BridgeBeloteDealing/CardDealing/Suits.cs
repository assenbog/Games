namespace BridgeBeloteDealing.CardDealing
{
    using System;
    using System.ComponentModel;

    [Serializable]
    public enum Suits
    {
        [Description("\u2660")]
        Spades,
        [Description("\u2665")]
        Hearts,
        [Description("\u2663")]
        Clubs,
        [Description("\u2666")]
        Diamonds
    }
}
