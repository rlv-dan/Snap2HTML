using Newtonsoft.Json;
using Snap2HTMLNG.Shared.Models;
using Snap2HTMLNG.Shared.Settings;
using System;
using System.IO;
using System.Net;

namespace Snap2HTMLNG.Shared.Updater
{
    public class Updater
    {
    
        // Repo Release Feed
        private const string _updateURL = "https://api.github.com/repos/laim/snap2html-ng/releases/latest";

        // fall back should always be false, we don't want to force check for
        // updates on people as Snap2HTML OG never had a check for update at all
        private bool _checkForUpdates = false; 

        public Updater()
        {
          _checkForUpdates = bool.Parse(XmlConfigurator.Read("CheckForUpdates"));
        }

        public void CheckForUpdate()
        {
            if(_checkForUpdates)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_updateURL);
                request.UserAgent = new Random(new Random().Next()).ToString();

                var response = request.GetResponse();
                if(response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    string output = reader.ReadToEnd();

                    var data = JsonConvert.DeserializeObject<ReleasesModel>(output);

                    Console.WriteLine(data.tag_name);
                }

            }
        }

    }
}
