using System;
using System.Xml;

namespace Snap2HTMLNG.Shared.Settings
{
    /// <summary>
    /// Configuration Class used for reading and writing user settings
    /// </summary>
    public class XmlConfigurator
    {

        /// <summary>
        /// The UserSettings file, saved in the Current Running Directory of the executable
        /// </summary>
        private static readonly string SettingsFile = "UserSettings.xml";

        /// <summary>
        /// Writes the setting value to the <see cref="SettingsFile"/>
        /// </summary>
        /// <param name="nodeList">
        /// string[] array of nodes in the settings file, see <see cref="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays"/>
        /// </param>
        /// <param name="valueList">
        /// string[] array of values for the nodes in the settings file
        /// </param>
        /// <param name="rootNode">
        /// Root Node of the Configuration File
        /// </param>
        public static void Write (string[] nodeList, string[] valueList, string rootNode = "UserSettings")
        {
            XmlWriterSettings XmlSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = " ",
                NewLineChars = Environment.NewLine
            };

            using (XmlWriter w = XmlWriter.Create(SettingsFile, XmlSettings))
            {
                w.WriteStartElement(rootNode);
                int index = 0;

                foreach (var i in nodeList)
                {
                    w.WriteElementString(i, valueList[index++]);
                }
                w.WriteEndElement();
                w.Flush();
            }
        }

        /// <summary>
        /// Reads the setting value from <see cref="SettingsFile"/>
        /// </summary>
        /// <param name="node">
        /// Settings Node you require data from
        /// </param>
        /// <param name="rootNode">
        /// Root Node of the Configuration File
        /// </param>
        /// <returns>
        /// Node Value from Configuration File as a <see cref="string"/>
        /// </returns>
        public static string Read(string node, string rootNode = "UserSettings")
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(SettingsFile);

            XmlNodeList nodeList = xml.GetElementsByTagName(rootNode);
            string nodeValue = string.Empty;
            foreach(XmlElement element in nodeList)
            {
                nodeValue = element[node].InnerText;
            }
            return nodeValue;
        }
    }
}
