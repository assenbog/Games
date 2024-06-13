namespace BridgeBeloteDealing
{
    using BridgeBeloteLogic;
    using System;
    using System.Configuration;
    using System.Text;

    class Program
    {
        static void Main()
        {
            const int DefaultMaxDealCount = 16;
            const int MaxSequenceLength = 0;

            var allow4OfAKindSetting = ConfigurationManager.AppSettings["Allow4OfAKind"];
            var maxSequenceLengthSetting = ConfigurationManager.AppSettings["MaxSequenceLength"];
            var maxDealCountSetting = ConfigurationManager.AppSettings["MaxDealCount"];
            var saveToDatabaseSetting = ConfigurationManager.AppSettings["SaveToDatabase"];

            var allow4OfAKindParamParseSuccess = bool.TryParse(allow4OfAKindSetting, out var allow4OfAKindValue);
            var maxSequenceLengthParamParseSuccess = int.TryParse(maxSequenceLengthSetting, out var maxSequenceLengthValue);
            var maxDealCountParamParseSuccess = int.TryParse(maxDealCountSetting, out var maxDealCountValue);
            var saveToDatabaseParseSuccess = bool.TryParse(saveToDatabaseSetting, out var saveToDatabaseValue);

            Console.OutputEncoding = Encoding.Unicode;

            if (!allow4OfAKindParamParseSuccess)
            {
                allow4OfAKindValue = false;
            }

            if (!maxSequenceLengthParamParseSuccess || maxSequenceLengthValue > 0 || maxSequenceLengthValue < 8)
            {
                maxSequenceLengthValue = MaxSequenceLength;
            }

            if (!maxDealCountParamParseSuccess)
            {
                maxDealCountValue = DefaultMaxDealCount;
            }

            if (!saveToDatabaseParseSuccess)
            {
                saveToDatabaseValue = false;
            }

            DealingProcessFlow.Go(allow4OfAKindValue, maxSequenceLengthValue, maxDealCountValue, saveToDatabaseValue);

            Console.Write("\nPress any key to exit ... ");

            Console.ReadKey();
        }
    }
}
