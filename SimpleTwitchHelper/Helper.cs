using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace SimpleTwitchHelper
{
    public class Helper
    {
        public static void ResetSettings()
        {
            Globals.Config.LoadDefaults();
            Globals.Config.Save();
            Globals.Logger.Log("Succesfully reset settings. Restarting...");
            Restart();
        }

        public static void Logout()
        {
            Globals.Config.AuthKey = "";
            Globals.Config.Save();
            Globals.Logger.Log("Succesfully logged out. Restarting...");
            Restart();
        }

        public static void Restart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Globals.Logger.Log("Restarting...");
            Shutdown();
        }

        public static void Shutdown()
        {
            for (var i = 0; i < TrackedThread.Count; i++)
            {
                lock (TrackedThread.ThreadList)
                    TrackedThread.ThreadList.ElementAt(i).Abort();
            }
            Globals.Logger.Log("Threads terminated. Shutting down...");

            Application.Current.Shutdown();
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
            {
                throw new ArgumentNullException("browser");
            }

            // get an IWebBrowser2 from the document
            var sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                var IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                var IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService,
                [In] ref Guid riid,
                [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }
}