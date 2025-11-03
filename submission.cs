using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;


/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/

namespace ConsoleApp1
{
    public class Program
    {
        public static string xmlURL = "https://zohernandez777.github.io/CSE445_A4/Hotels.xml";
        public static string xmlErrorURL = "https://zohernandez777.github.io/CSE445_A4/HotelsErrors.xml";
        public static string xsdURL = "https://zohernandez777.github.io/CSE445_A4/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        // FIXME: want to try something different, I never used explioctlty used the XmlSchemaSet from the notes only the .Add onto the settings, so 
        // I will try to implement it that way just in case for manual testing 
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                //cretes a schema set and add xsd schema to it 
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(null, xsdUrl); // null = no namespac

                // xmlreader for validation
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;  // Validate against schema
                settings.Schemas = schemaSet;               // Assign the schema set

                string errorMessage = "";

                //validation event handler
                settings.ValidationEventHandler += (sender, e) =>
                {
                    //record the first message ignore the rest
                    if (errorMessage == "")
                        errorMessage = e.Message;
                };

                // create XmlTextReader to read from the XML URL
                XmlTextReader textReader = new XmlTextReader(xmlUrl);

                // wrap XmlTextReader with XmlReader to enable validation
                using (XmlReader reader = XmlReader.Create(textReader, settings))
                {
                    // reads through the XML document
                    while (reader.Read()) { }
                }

                // return result message
                if (errorMessage == "")
                    return "No Error";
                else
                    return errorMessage;
            }
            catch (Exception ex)
            {
                // just print out any exceptions
                return "Exception: " + ex.Message;
            }
        }


        //FIXME: realized my that the customized test cases isnt showing the phone numbers 
        public static string Xml2Json(string xmlUrl)
        {
            //need to get the xml from the url
            try
            {
                //downmload the XML into a string
                using (var webClient = new System.Net.WebClient()) //webclient to download the xml
                {
                    string xmlContent = webClient.DownloadString(xmlUrl); //download the xml content as a string
                    //loads the xml string into a xmlDocument for DOM traversal
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlContent); //load the xml string


                    //make a list to store the hotels 
                    var hotelsList = new List<Dictionary<string, object>>();

                    //need to loop through each <Hotel> element 
                    XmlNodeList hotels = doc.GetElementsByTagName("Hotel");
                    foreach (XmlNode hotelNode in hotels)
                    {
                        //put hotels into a dictionary 
                        var hotel = new Dictionary<string, object>();

                        //extract the hotel name
                        hotel["Name"] = hotelNode["Name"]?.InnerText.Trim();

                        // get all the <phone> eleemnts in a striong list
                        var phoneList = new List<string>();
                        //FIXME: dfoesnt showe the phone numbers in gradescope autopgrader
                        foreach (XmlNode phoneNode in hotelNode.ChildNodes) // for each node in hotel
                        {
                            if (phoneNode.LocalName == "Phone") //if the node is a phone
                                phoneList.Add(phoneNode.InnerText.Trim()); //pull the inner text without any xml markup
                        }

                        hotel["Phone"] = phoneList; //add the phone list to the hotel dictionary

                        //next thing to get is the address
                        //extracts the <address> details 
                        XmlNode addr = hotelNode["Address"];
                        if (addr != null) //if the address exists
                        {
                            var address = new Dictionary<string, string>
                            {


                                //All of these use InnerText to get the plain text of the node, without xml markup
                                //left side is the key, right side is the value
                                ["Number"] = addr["Number"].InnerText.Trim(),  
                                ["Street"] = addr["Street"].InnerText.Trim(),
                                ["City"] = addr["City"].InnerText.Trim(),
                                ["State"] = addr["State"].InnerText.Trim(),
                                ["Zip"] = addr["Zip"].InnerText.Trim(),
                                //left hand side is the key, addr.Attributes gets the attribute from  ["_NearestAirport"]
                                //?.Value: if the attribute is null, the whole expression becomes null else it yields the attribute’s string value
                                //?.trim(): if the value is null, the whole expression becomes null else it trims whitespace
                                //?? "": if the expression is null, it yields an empty string
                                ["_NearestAirport"] = addr.Attributes["_NearestAirport"]?.Value?.Trim() ?? ""

                            };
                            hotel["Address"] = address; // add the address dictionary to the hotel dictionary
                        }

                        //i went with the optional rating so we can add it here 
                        XmlAttribute ratingAttr = hotelNode.Attributes["_Rating"]; // get the _Rating attribute
                        if (ratingAttr != null) // if it exists
                        {
                            hotel["_Rating"] = ratingAttr.Value.Trim(); // add it to the hotel dictionary
                        }

                        // wrap the hotels list into the final object shape:
                        hotelsList.Add(hotel);
                    }
                    //wrap the list
                    var hotelsWrapper = new Dictionary<string, object> // outer dictionary
                    {
                        ["Hotel"] = hotelsList // key "Hotel" maps to the list of hotels
                    };
                    var root = new Dictionary<string, object> // root dictionary
                    {
                        ["Hotels"] = hotelsWrapper // key "Hotels" maps to the hotels wrapper
                    };

                    //Convert to formatted JSON string
                    //FIXME: when i run it locally the console says 'formatting' is ambigous UPDATE: fixed it, compiler didnt know to choose formtatting from NewtonSoft.json or System.xml
                    string jsonText = JsonConvert.SerializeObject(root, Newtonsoft.Json.Formatting.Indented);


                    return jsonText;
                }
            }
            //use the catch for any errors we encounter
            catch (Exception ex)
            {
                return "Exception during XML → JSON conversion: " + ex.Message;
            }
        }
    }
}
