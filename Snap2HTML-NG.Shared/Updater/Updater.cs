using Newtonsoft.Json;
using Snap2HTMLNG.Shared.Models;
using Snap2HTMLNG.Shared.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Snap2HTMLNG.Shared.Updater
{
    public class Updater
    {
    
        // Repo latest release feed
        private const string _updateURL = "https://api.github.com/repos/laim/snap2html-ng/releases/latest";

        // fall back should always be false, we don't want to force check for
        // updates on people as Snap2HTML OG never had a check for update at all
        // NOTE: DEBUG BUILDS ALWAYS HAVE CHECK FOR UPDATES SET TO TRUE.
        private bool _checkForUpdates = false; 

        public Updater()
        {
          _checkForUpdates = bool.Parse(XmlConfigurator.Read("CheckForUpdates"));

#if DEBUG
            _checkForUpdates = true;
#endif
        }

        /// <summary>
        /// Returns information from the server about the latest release
        /// </summary>
        /// <returns>
        /// If data is available a <see cref="ReleasesModel"/> is returned.  If data is not available or response is invalid, null is returned.
        /// </returns>
        private ReleasesModel GetReleaseInformation()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_updateURL);
            request.UserAgent = new Random(new Random().Next()).ToString();

            var response = request.GetResponse();
            if (response != null)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string output = reader.ReadToEnd();

                var data = JsonConvert.DeserializeObject<ReleasesModel>(output);

                return data;
            }

            // if response is null
            return null;
        }

        /// <summary>
        /// Checks against the <see cref="_updateURL"/> to see if the installed version is the latest or not.
        /// </summary>
        /// <param name="currentProductVersion">Application Version as a string (0.0.0.0)</param>
        /// <returns>true new version is required, false if current version or check for update is disabled in user configuration</returns>
        public bool CheckForUpdate(string currentProductVersion)
        {
            if(_checkForUpdates)
            {
                ReleasesModel releaseInfo = GetReleaseInformation();

                if (GetReleaseInformation() != null)
                {
                    Version latestVersion = new Version(releaseInfo.tag_name);

                    Console.WriteLine(latestVersion);
                    Console.WriteLine(new Version(currentProductVersion));

                    if (latestVersion > new Version(currentProductVersion))
                    {
                        Console.WriteLine("There is a new release version on GitHub");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Newest version is installed");

                        // return false since we don't want anything to pop for the user
                        return false;
                    }
                }

            }

            // Return false if we have check for updates disabled
            // this just prevents anything popping by mistake and
            // confusing the user
            return false;
        }

        /// <summary>
        /// Returns the release information as a <see cref="ReleasesModel"/>
        /// </summary>
        /// <returns>
        /// see <see cref="GetReleaseInformation"/>
        /// </returns>
        public ReleasesModel ReturnReleaseInformation()
        {
            return GetReleaseInformation();
        }
    }
}
