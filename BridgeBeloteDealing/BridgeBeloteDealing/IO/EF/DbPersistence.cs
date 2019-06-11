namespace BridgeBeloteDealing.IO.EF
{
    using BridgeBeloteDealing.CardDealing;
    using CardDealing;
    using System;
    using System.Collections.Generic;

    public static class DbPersistence
    {
        public static void SaveDealings(List<List<Card>> cards, SortOrders sortOrder)
        {
            try
            {
                using (var dbContext = new BridgeBelotEntities())
                {
                    var dbDealing = dbContext.DbDealings.Add(new DbDealing { SortOrder = (int)sortOrder, TimeStamp = DateTime.UtcNow });

                    foreach (var playerDeck in cards)
                    {
                        foreach (var card in playerDeck)
                        {
                            dbContext.DbCards.Add(new DbCard { DealingId = dbDealing.Id, Suit = (int)card.Suit, Side = (int)card.Side, Stage = (int)card.Stage, BeloteCard = (int)card.BelotCard });
                        }
                    }
                    dbContext.SaveChanges();
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
