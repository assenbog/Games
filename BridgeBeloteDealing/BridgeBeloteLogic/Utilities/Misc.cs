namespace BridgeBeloteLogic.Utilities
{
    using System.Collections.Generic;
    using System;

    public static class Misc
    {
        public static List<int> ShuffledSequenceNos(int maxDealCount)
        {
            var dealingRandomSequence = new Random(Guid.NewGuid().GetHashCode());

            var shuffledSequenceList = new List<int>();

            while (shuffledSequenceList.Count < maxDealCount)
            {
                // Next range doesn't include the max values, i.e. we have an [min, max) interval
                var nextSequenceNo = dealingRandomSequence.Next(1, maxDealCount + 1);

                if (!shuffledSequenceList.Contains(nextSequenceNo))
                {
                    shuffledSequenceList.Add(nextSequenceNo);
                }
            }

            return shuffledSequenceList;
        }
    }
}
