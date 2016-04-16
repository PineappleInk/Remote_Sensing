using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    class XMLsave
    {
        public void saveToXML(string str)
        {
            // Create the XmlDocument.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<item><name>Pulse: </name></item>");


            // Add a price element.
            XmlElement newElem = doc.CreateElement("Pulse");
            newElem.InnerText = str;
            doc.DocumentElement.AppendChild(newElem);

            // Save the document to a file. White space is
            // preserved (no white space).
            doc.PreserveWhitespace = true;
            doc.Save(@"C:\Users\Jakob\Documents\Skola\Kandidatprojekt\Remote_Sensing\WebSite1\XMLFile.xml");
        }
    }
}
