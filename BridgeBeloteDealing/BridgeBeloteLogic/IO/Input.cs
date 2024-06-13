namespace BridgeBeloteLogic.IO
{
    using System.IO;
    using System.Xml.Serialization;
    using BridgeBeloteLogic.CardDealing;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System;

    public class Input
    {
        public List<Dealing> DeserialiseFromXml(string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open))
                {
                    var serialiser = new XmlSerializer(typeof(List<Dealing>));

                    return serialiser.Deserialize(fs) as List<Dealing>;
                }
            }
            catch
            {
                return null;
            }
        }


        public bool StarDealingsPresent()
        {
            var starDealingsFolder = GetStarDealingsFolder();

            return Directory.Exists(starDealingsFolder);
        }

        public string GetStarDealingsFolder()
        {
            var currentFolder = GetCurrentFolder();

            return Path.Combine(currentFolder, "StarDealings");
        }

        public List<Dealing> GetStarDealings()
        {
            var starDealings = new List<Dealing>();

            var starDealingsFolder = GetStarDealingsFolder();

            var files = Directory.GetFiles(starDealingsFolder, "*.xml");

            var input = new Input();

            foreach (var file in files)
            {
                var dealings = input.DeserialiseFromXml(file);

                starDealings.AddRange(dealings.Where(p => p.StarDealing));
            }

            return starDealings;
        }

        public List<Dealing> GetRandomDealings(List<Dealing> dealings, int randomCount)
        {
            var randomDealings = dealings.OrderBy(p => Guid.NewGuid()).Take(randomCount).ToList();

            for (var i = 0; i < randomCount; i++)
            {
                randomDealings[i].DealingSide = (Sides)(i % 4);
            }

            return randomDealings;
        }

        private string GetCurrentFolder()
        {
            var codeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

            var uri = new UriBuilder(codeBase);

            return Uri.UnescapeDataString(uri.Path);
        }
    }
}
