using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace ConsoleApp1
{
    public class Submission
    {
        // Using local files for testing
        public static string xmlURL = "NationalParks.xml";
        public static string xmlErrorURL = "NationalParksErrors.xml";
        public static string xsdURL = "NationalParks.xsd";

        public static void Main(string[] args)
        {
            Console.WriteLine("=== Testing Valid XML ===\n");
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            Console.WriteLine("\n=== Testing Invalid XML ===\n");
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            Console.WriteLine("\n=== Converting XML to JSON ===\n");
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                string xmlContent = DownloadContent(xmlUrl);
                string xsdContent = DownloadContent(xsdUrl);
                
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                
                XmlSchemaSet schemas = new XmlSchemaSet();
                using (StringReader xsdReader = new StringReader(xsdContent))
                {
                    XmlSchema schema = XmlSchema.Read(xsdReader, null);
                    schemas.Add(schema);
                }
                settings.Schemas = schemas;
                
                StringBuilder errorMessages = new StringBuilder();
                bool hasErrors = false;
                
                settings.ValidationEventHandler += (sender, e) =>
                {
                    hasErrors = true;
                    if (errorMessages.Length > 0)
                        errorMessages.Append("; ");
                    errorMessages.Append(e.Message);
                };
                
                using (StringReader xmlReader = new StringReader(xmlContent))
                using (XmlReader reader = XmlReader.Create(xmlReader, settings))
                {
                    while (reader.Read()) { }
                }
                
                if (hasErrors)
                    return errorMessages.ToString();
                else
                    return "No errors are found";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                string xmlContent = DownloadContent(xmlUrl);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                string jsonText = JsonConvert.SerializeXmlNode(doc);
                return jsonText;
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        private static string DownloadContent(string url)
        {
            // Check if it's a local file path
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                return File.ReadAllText(url);
            }
            
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
