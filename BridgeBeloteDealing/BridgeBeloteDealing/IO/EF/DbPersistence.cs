namespace BridgeBeloteDealing.IO.EF
{
    using BridgeBeloteDealing.CardDealing;
    using System;
    using System.Collections.Generic;

    public static class DbPersistence
    {
        public static void SaveAllDealings(List<Dealing> dealings, SortOrders sortOrder)
        {
            try
            {
                var dealingsCount = dealings.Count;

                using (var dbContext = new BridgeBelotEntities())
                {
                    // Initial dealings first
                    for (var dealingNo = 0; dealingNo < dealings.Count; dealingNo++)
                    {
                        SaveDealings(dbContext, dealings[dealingNo].AllCardsDealt, sortOrder, dealingNo + 1, default);
                    }

                    // ... followed by the swapped sides ones
                    for (var dealingNo = 0; dealingNo < dealings.Count; dealingNo++)
                    {
                        SaveDealings(dbContext, dealings[dealingNo].AllCardsDealtRotated, sortOrder, dealingNo + dealingsCount + 1, dealings[dealingNo].ShuffledSequenceNo);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void SaveDealings(BridgeBelotEntities dbContext, List<List<Card>> cards, SortOrders sortOrder, int sequenceNo, int? shuffledSequenceNo)
        {
            var dbDealing = dbContext.DbDealings.Add(new DbDealing { SortOrder = (int)sortOrder, TimeStamp = DateTime.UtcNow });

            foreach (var playerDeck in cards)
            {
                foreach (var card in playerDeck)
                {
                    dbContext.DbCards.Add(new DbCard
                    {
                        DealingId = dbDealing.Id,
                        Suit = (int)card.Suit,
                        Side = (int)card.Side,
                        Stage = (int)card.Stage,
                        BeloteCard = (int)card.BelotCard,
                        SequenceNo = sequenceNo,
                        ShuffledSequenceNo = shuffledSequenceNo
                    });
                }
            }

            dbContext.SaveChanges();
        }
    }
}
