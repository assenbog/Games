﻿namespace BridgeBeloteLogic.IO
{
    using BridgeBeloteLogic.CardDealing;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    public class Input
    {
        public List<Dealing> DeserialiseFromXml(string fileName)
        {
            try
            {
                var fs = new FileStream(fileName, FileMode.Open);

                var serialiser = new XmlSerializer(typeof(List<Dealing>));

                return serialiser.Deserialize(fs) as List<Dealing>;
            }
            catch
            {
                return null;
            }
        }
    }
}
