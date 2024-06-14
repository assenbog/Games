namespace BridgeBeloteDealing
{
    using BridgeBeloteLogic;
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Text;

    class Program
    {
        static void Main()
        {
            var allow4OfAKindValue = true;
            var maxSequenceLengthValue = 0;
            var maxDealCountValue = 16;
            var saveToDatabaseValue = false;

            try
            {
                var allow4OfAKindSetting = ConfigurationManager.AppSettings["Allow4OfAKind"];
                var maxSequenceLengthSetting = ConfigurationManager.AppSettings["MaxSequenceLength"];
                var maxDealCountSetting = ConfigurationManager.AppSettings["MaxDealCount"];
                var saveToDatabaseSetting = ConfigurationManager.AppSettings["SaveToDatabase"];

                bool.TryParse(allow4OfAKindSetting, out allow4OfAKindValue);
                int.TryParse(maxSequenceLengthSetting, out maxSequenceLengthValue);
                int.TryParse(maxDealCountSetting, out maxDealCountValue);
                bool.TryParse(saveToDatabaseSetting, out saveToDatabaseValue);

                Console.OutputEncoding = Encoding.Unicode;

                DealingProcessFlow.Go(allow4OfAKindValue, maxSequenceLengthValue, maxDealCountValue, saveToDatabaseValue);
            }
            catch (Exception ex) 
            { 
                var exceptionDetails = ex.ToString();

                EventLog.WriteEntry("Application", exceptionDetails, EventLogEntryType.Error);

                Console.Write($"\nThe following exception occured:\n {exceptionDetails}");
            }

            Console.Write("\nPress any key to exit ... ");

            Console.ReadKey();
        }
    }
}
