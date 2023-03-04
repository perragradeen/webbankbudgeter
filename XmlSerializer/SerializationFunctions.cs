using System.Xml;
using System.Xml.Serialization;

namespace Serializers
{
    /// <summary>
    /// Serialisera eller Deserialisera något till/från fil.
    /// 
    /// Serialisera ex.
    /// SerializeObject(xmlDocFileName, typeof(Product), selectedProduct);
    /// 
    /// Ladda XML-filen till minnet med serialisering
    /// OneAccessoriesCatalog = DeserializeObject(accessoryCatalogIn, typeof(Accessories)) as Accessories;
    /// </summary>
    public static class SerializationFunctions
    {
        /// <summary>
        /// Lägg innehållet i en xml-fil till minnet i den statiska vaiabeln AllCategories
        /// </summary>
        /// <param name="filename">xml-fil som ska läsas in</param>
        public static T DeserializeObject<T>(string filename)
        {
            FileStream fileStream = null;
            try
            {
                // Initiera variabler som behövs
                // Gör en serializer som matchar mot klassen accessories
                var serializer = new XmlSerializer(typeof(T)); // TODO: kolla varför och fixa:
                /*
                 * System.IO.FileNotFoundException: 'Could not load file or assembly '


GeneralSettingsHandler.XmlSerializers


, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null, processorArchitecture=MSIL'. Det går inte att hitta filen.'
*/

                // Öppna filen
                fileStream = new FileStream(filename, FileMode.Open);
                var reader = new XmlTextReader(fileStream);

                // Utför själva serialiseringen, spara resultatet i objectSerialized
                var objectSerialized = serializer.Deserialize(reader);

                // Stäng filen, nu när läsningen är färdig
                fileStream.Close();

                return (T)objectSerialized;
            }
            catch (Exception serExcp)
            {
                ErrorCatchForSerialze(filename, fileStream, serExcp);
                throw;
            }
        }

        /// <summary>
        /// Lägg innehållet i en xml-fil till minnet i den statiska vaiabeln AllCategories
        /// </summary>
        /// <param name="filename">xml-fil som ska läsas in</param>
        /// <param name="serializeType">the return type, ie Accessories</param>
        /// <param name="selectedProduct">the object to load file to</param>
        public static void SerializeObject(string filename, Type serializeType, object selectedProduct)
        {
            StreamWriter myWriter = null;
            try
            {
                // Gör en serializer som matchar mot klassen accessories

                #region Serializera ner till fil

                var serializer = new XmlSerializer(serializeType);

                myWriter = new StreamWriter(filename);

                // these lines do the actual serialization
                serializer.Serialize(myWriter, selectedProduct);

                myWriter.Close();

                #endregion
            }
            catch (Exception deserExcp)
            {
                ErrorCatchForSerialze(filename, myWriter, deserExcp);
            }
        }

        #region Error handling

        // Done: Fånga fel här och stäng filen etc...
        private static void ErrorCatchForSerialze(string filename, Stream fileStream, Exception serExcp)
        {
            ErrorCatchForSerialze(filename, "deserializing", fileStream, null, serExcp);
        }

        private static void ErrorCatchForSerialze(string filename, TextWriter fileWriteStream, Exception serExcp)
        {
            ErrorCatchForSerialze(filename, "serializing", null, fileWriteStream, serExcp);
        }

        private static void ErrorCatchForSerialze(
            string filename, string deserializingOrSer, Stream fileStream, TextWriter fileWriteStream,
            Exception serExcp)
        {
            Console.WriteLine(
                "Error while " + deserializingOrSer + " object: " + filename + "- Sys err: " + serExcp.Message);

            try
            {
                // Stäng filen, även om den redan är stängd
                if (fileStream != null)
                {
                    fileStream.Close();
                }

                if (fileWriteStream != null)
                {
                    fileWriteStream.Close();
                }
            }
            catch (Exception closeExcp)
            {
                Console.WriteLine("File; " + filename + " already closed or other error: " + closeExcp.Message);
                throw;
            }
        }

        #endregion
    }
}