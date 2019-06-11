namespace BridgeBeloteDealing
{
    public static class DbOutput
    {
        public static void Save(Dealing cards)
        {
            using(var dbContext = new BridgeBelotEntities())
            {
                dbContext.Dealings.Add(cards);
                cards.CardsDealt.ForEach(side => side.ForEach(card => dbContext.Cards.Add(card)));
                dbContext.SaveChanges();
            }
        }
    }
}
