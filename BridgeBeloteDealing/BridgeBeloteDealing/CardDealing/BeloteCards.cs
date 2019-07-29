namespace BridgeBeloteDealing.CardDealing
{
    using System;
    using System.ComponentModel;

    [Serializable]
    public enum BeloteCards
    {
        [Description("  7")]
        Seven,
        [Description("  8")]
        Eight,
        [Description("  9")]
        Nine,
        [Description(" 10")]
        Ten,
        [Description("  J")]
        Jack,
        [Description("  Q")]
        Queen,
        [Description("  K")]
        King,
        [Description("  A")]
        Ace
    }
}
