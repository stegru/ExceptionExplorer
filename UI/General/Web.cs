namespace ExceptionExplorer.General
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;
    using ExceptionExplorer.Config;


    public enum SiteLink
    {
        // name of each link is added to the end of WebsiteLinkBase
        None = 0,
        Home = 1,
        Buy = 2,
        NewVersion = 3, // special case; uses Options.Current.Update.UpgradeURL.Value
        ErrorReport = 4,
        CheckVersion
    }

    public class Web
    {
        //private const string WebsiteRoot = "http://exceptionexplorer.net";
        //private const string WebsiteLinkBase = "http://exceptionexplorer.net/App/";
        //private const string VersionCheck = "http://update.test.exceptionexplorer.net/";

        private const string WebsiteRoot = "http://localhost:59447";
        private const string WebsiteLinkBase = "http://localhost:59447/App/";
        private const string VersionCheck = "http://localhost:59447/";

        public static string GetSiteUrl(SiteLink link)
        {
            if (link == SiteLink.None)
            {
                return WebsiteRoot;
            }



            string baseUrl = WebsiteLinkBase;
            string linkString = link.ToString();

            if (link == SiteLink.CheckVersion)
            {
                baseUrl = VersionCheck;
                linkString = "Update";
            }
            else if (link == SiteLink.NewVersion)
            {
                linkString = Options.Current.Update.UpgradeURL.Value;
                return string.Format("{0}{1}", WebsiteRoot, linkString);
            }


            return string.Format("{0}{1}/{2}", baseUrl, linkString, AppVersion.Current.ToString());
        }

        public static void OpenSite(SiteLink link)
        {
            OpenUrl(GetSiteUrl(link));
        }

        public static void OpenUrl(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                Process.Start(url);
            }
        }
    }
}
