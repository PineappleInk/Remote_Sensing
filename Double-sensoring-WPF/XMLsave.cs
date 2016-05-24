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
        private string pulse;
        private string breath;

        public void saveToXML(string str, string index)
        {
            if (index == "pulse")
            {
                pulse = str;
            }
            else if(index == "breath")
            {
                breath = str;
            }
            // Create the XmlDocument.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<item><nameP>Pulse: </nameP><nameB>Breath: </nameB></item>");

            // Add a price element.
            XmlElement newElemP = doc.CreateElement("pulse");
            newElemP.InnerText = pulse;
            doc.DocumentElement.AppendChild(newElemP);
            XmlElement newElemB = doc.CreateElement("breath");
            newElemB.InnerText = breath;
            doc.DocumentElement.AppendChild(newElemB);

            // Save the document to a file. White space is
            // preserved (no white space).
            doc.PreserveWhitespace = true;
            try
            {
                doc.Save(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory()) + @"\..\..\..\..\WebSite1\XMLFile.xml");
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("Felmeddelande");
            }
        }
    }
}
