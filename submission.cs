using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Text;

/**
 * ASU CSE445 - Assignment 4
 * Student: Farida Hazeq
 * 
 * This program demonstrates XML validation against XSD schema
 * and XML to JSON conversion using Newtonsoft.Json.
 * 
 * The program:
 * 1. Validates a correct XML file (NationalParks.xml) against its XSD
 * 2. Validates an error XML file (NationalParksErrors.xml) against the same XSD
 * 3. Converts the valid XML to JSON format
 */

namespace ConsoleApp1
{
    public class Submission
    {
        // ============================================================
        // Q1.1, Q1.2, Q1.3 - URLs for XML and XSD files
        // These URLs are hosted on GitHub Pages and are accessible publicly
        // The autograder reads these URLs to access the files
        // ============================================================
        
        /// <summary>
        /// URL to the valid XML file containing 11 national parks
        /// Each park has Name, Phone(s), Address, and Rating attribute
        /// </summary>
        public static string xmlURL = "https://faridahazeq.github.io/cse445-assignment4/NationalParks.xml";
        
        /// <summary>
        /// URL to the error XML file containing 5 intentional errors:
        /// 1. Wrong root element name (NationalPark instead of NationalParks)
        /// 2. Missing required Rating attribute
        /// 3. Missing Phone element
        /// 4. Unclosed Address tag
        /// 5. Two Name elements for one park
        /// </summary>
        public static string xmlErrorURL = "https://faridahazeq.github.io/cse445-assignment4/NationalParksErrors.xml";
        
        /// <summary>
        /// URL to the XSD schema file that defines the structure of NationalParks
        /// Defines: NationalParks root, NationalPark elements, Name, Phone, Address,
        /// and the Rating (required) and NearestAirport (optional) attributes
        /// </summary>
        public static string xsdURL = "https://faridahazeq.github.io/cse445-assignment4/NationalParks.xsd";

        // ============================================================
        // Q3 - Main method
        // Calls verification for both valid and error XML files,
        // then converts valid XML to JSON
        // ============================================================
        
        /// <summary>
        /// Main entry point of the program
        /// Executes three operations as required by the assignment:
        /// 1. Validates the correct XML file against XSD
        /// 2. Validates the error XML file against XSD (should produce errors)
        /// 3. Converts the valid XML to JSON format
        /// </summary>
        /// <param name="args">Command line arguments (not used)</param>
        public static void Main(string[] args)
        {
            // Q3.1 - Validate the correct XML file
            // Expected output: "No errors are found"
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            // Q3.2 - Validate the error XML file
            // Expected output: Error messages describing the validation failures
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            // Q3.3 - Convert valid XML to JSON
            // Expected output: JSON representation of the national parks data
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // ============================================================
        // Q2.1 - Verification Method
        // Validates an XML file against an XSD schema
        // ============================================================
        
        /// <summary>
        /// Validates an XML file against its corresponding XSD schema
        /// 
        /// Process:
        /// 1. Downloads the XML and XSD content from the provided URLs
        /// 2. Creates an XmlSchemaSet and adds the downloaded schema
        /// 3. Configures XmlReaderSettings for validation
        /// 4. Reads through the XML, triggering validation events
        /// 5. Returns "No errors are found" if validation passes
        /// 6. Returns error messages if validation fails
        /// 
        /// This method uses the built-in .NET XML validation capabilities
        /// with XmlReader and XmlSchemaSet to perform full schema validation.
        /// </summary>
        /// <param name="xmlUrl">URL of the XML file to validate</param>
        /// <param name="xsdUrl">URL of the XSD schema file</param>
        /// <returns>
        /// "No errors are found" if XML is valid,
        /// otherwise returns the validation error message
        /// </returns>
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                // Download the XML and XSD content from the URLs
                string xmlContent = DownloadContent(xmlUrl);
                string xsdContent = DownloadContent(xsdUrl);
                
                // Configure XML reader settings for schema validation
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                
                // Create and load the schema set from the downloaded XSD content
                XmlSchemaSet schemas = new XmlSchemaSet();
                using (StringReader xsdReader = new StringReader(xsdContent))
                {
                    XmlSchema schema = XmlSchema.Read(xsdReader, null);
                    schemas.Add(schema);
                }
                settings.Schemas = schemas;
                
                // StringBuilder to collect all validation error messages
                StringBuilder errorMessages = new StringBuilder();
                bool hasErrors = false;
                
                // Event handler for validation errors and warnings
                // This captures any validation issues during XML reading
                settings.ValidationEventHandler += (sender, e) =>
                {
                    hasErrors = true;
                    if (errorMessages.Length > 0)
                        errorMessages.Append("; ");
                    errorMessages.Append(e.Message);
                };
                
                // Read through the XML document - validation occurs during reading
                using (StringReader xmlReader = new StringReader(xmlContent))
                using (XmlReader reader = XmlReader.Create(xmlReader, settings))
                {
                    while (reader.Read()) { } // Consume all nodes to trigger validation
                }
                
                // Return appropriate message based on validation result
                if (hasErrors)
                    return errorMessages.ToString();
                else
                    return "No errors are found";
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., network issues, malformed XML)
                return $"Error: {ex.Message}";
            }
        }

        // ============================================================
        // Q2.2 - Xml2Json Method
        // Converts an XML document to JSON format using Newtonsoft.Json
        // ============================================================
        
        /// <summary>
        /// Converts an XML file to JSON format using Newtonsoft.Json
        /// 
        /// Process:
        /// 1. Downloads the XML content from the provided URL
        /// 2. Loads the XML into an XmlDocument
        /// 3. Uses JsonConvert.SerializeXmlNode to convert to JSON
        /// 4. Returns the JSON string
        /// 
        /// The resulting JSON is compatible with JsonConvert.DeserializeXmlNode
        /// as required by the assignment.
        /// </summary>
        /// <param name="xmlUrl">URL of the XML file to convert</param>
        /// <returns>
        /// JSON string representation of the XML data,
        /// or an error message if conversion fails
        /// </returns>
        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                // Download the XML content from the URL
                string xmlContent = DownloadContent(xmlUrl);
                
                // Load XML into an XmlDocument for processing
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                
                // Convert the XML document to JSON using Newtonsoft.Json
                // The output is formatted for readability (Indented)
                string jsonText = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
                return jsonText;
            }
            catch (Exception ex)
            {
                // Return error message if conversion fails
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        // ============================================================
        // Helper Method - DownloadContent
        // Downloads content from a URL or reads from local file
        // ============================================================
        
        /// <summary>
        /// Downloads content from a URL or reads from a local file path
        /// 
        /// This helper method is used by both Verification and Xml2Json
        /// to retrieve the XML and XSD content.
        /// </summary>
        /// <param name="url">URL or local file path</param>
        /// <returns>Content as a string</returns>
        private static string DownloadContent(string url)
        {
            // Check if the input is a local file path (not HTTP/HTTPS)
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                // Read from local file system
                return File.ReadAllText(url);
            }
            
            // Download from remote URL using WebClient
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}