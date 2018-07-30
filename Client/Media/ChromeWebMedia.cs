using CefSharp;
using CefSharp.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace XiboClient
{
    internal class ChromeWebMedia : Media
    {
        private readonly RegionOptions options;
        private string filePath;
        private string localWebPath;
        private ChromiumWebBrowser webBrowser;
        private int documentCompletedCount;
        private bool disposed;
        private bool reloadOnXmdsRefresh;

        public ChromeWebMedia(RegionOptions options) : base(options.width, options.height, options.top, options.left)
        {
            this.options = options;

            var modeId = options.Dictionary.Get("modeid");
            var nativeOpen = !string.IsNullOrEmpty(modeId) && modeId == "1";

            if (nativeOpen)
            {
                filePath = Uri.UnescapeDataString(options.uri).Replace('+', ' ');
            }
            else
            {
                filePath = $"{ApplicationSettings.Default.LibraryPath}/{options.mediaid}.htm";
                localWebPath = $"{ApplicationSettings.Default.EmbeddedServerAddress + options.mediaid}.htm";
            }

            if (!Cef.IsInitialized)
            {
                Cef.Initialize();
            }

            webBrowser = new ChromiumWebBrowser("http://ourcodeworld.com")
            {
                Size = Size,
                Visible = true,
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            webBrowser.LoadingStateChanged += OnDocumentCompleted;

            if (nativeOpen)
            {
                webBrowser.Load(filePath);
            }
            else if (HtmlReady)
            {
                ReadControlMeta();

                webBrowser.LoadHtml(localWebPath);
            }
            else
            {
                Debug.WriteLine("HTML Resource is not ready to be shown (meaning the file doesn't exist at all) - wait for the download the occur and then show");
            }

            Controls.Add(webBrowser);

            Show();
        }

        private void OnDocumentCompleted(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                documentCompletedCount++;

                if (documentCompletedCount > 1) return;

                RestartTimer();

                if (disposed) return;

                //webBrowser.Visible = true;

                Loaded = true;
            }
        }

        private bool HtmlReady
        {
            get
            {
                if (!File.Exists(filePath))
                {
                    reloadOnXmdsRefresh = true;

                    RefreshFromXmds();

                    return false;
                }

                var lastWriteDate = File.GetLastWriteTime(filePath);

                if (options.updateInterval == 0)
                {
                    RefreshFromXmds();
                }
                else if (options.LayoutModifiedDate > lastWriteDate || DateTime.Now > lastWriteDate.AddMinutes(options.updateInterval))
                {
                    RefreshFromXmds();
                }

                UpdateCache();

                return true;
            }
        }

        private void ReadControlMeta()
        {
            using (var reader = new StreamReader(filePath))
            {
                var html = reader.ReadToEnd();

                try
                {
                    var match = Regex.Match(html, "<!-- DURATION=(.*?) -->");

                    if (match.Success)
                    {
                        Duration = int.Parse(match.Groups[1].Value);
                    }
                }
                catch (Exception)
                {
                    Trace.WriteLine(new LogMessage("Html - ReadControlMeta", "Unable to pull duration using RegEx").ToString());
                }
            }
        }

        private void RefreshFromXmds()
        {
            xmds.xmds xmds = new XiboClient.xmds.xmds();
            xmds.Url = ApplicationSettings.Default.XiboClient_xmds_xmds;
            xmds.GetResourceCompleted += GetResourceCompleted;

            xmds.GetResourceAsync(ApplicationSettings.Default.ServerKey, ApplicationSettings.Default.HardwareKey, options.layoutId, options.regionId, options.mediaid, ApplicationSettings.Default.Version);
        }

        private void GetResourceCompleted(object sender, xmds.GetResourceCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Trace.WriteLine(new LogMessage("xmds_GetResource", "Unable to get Resource: " + e.Error.Message), LogType.Info.ToString());

                    if (reloadOnXmdsRefresh)
                    {
                        if (File.Exists(filePath))
                        {
                            UpdateCache();

                            webBrowser.LoadHtml(localWebPath);
                        }
                        else
                        {
                            Trace.WriteLine(new LogMessage("xmds_GetResource", "We haven't been able to download this widget and there isn't a pre-cached one to use. Skipping."), LogType.Info.ToString());

                            Duration = 2;
                            RenderMedia();
                        }
                    }
                }
                else
                {
                    var cachedFile = e.Result;

                    string bodyStyle;
                    var bgColor = options.Dictionary.Get("backgroundColor", options.backgroundColor);

                    if (string.IsNullOrEmpty(options.backgroundImage))
                    {
                        bodyStyle = $"background-color:{bgColor};";
                    }
                    else
                    {
                        bodyStyle = $"background-image:url(\"{options.backgroundImage}\"); background-attachment:fixed; background-color: \"{bgColor}\"; background-repeat: no-repeat; background-position: {options.backgroundLeft}px {options.backgroundTop}px";
                    }

                    var html = cachedFile.Replace("</head>", "<style type='text/css'>body {" + bodyStyle + " }</style></head>");
                    html = html.Replace("[[ViewPortWidth]]", _width.ToString());

                    using (FileStream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        using (StreamWriter sw = new StreamWriter(fileStream))
                        {
                            sw.Write(html);
                            sw.Close();
                        }
                    }

                    if (reloadOnXmdsRefresh)
                    {
                        // Read the control meta back out
                        ReadControlMeta();

                        // Handle Navigate in here because we will not have done it during first load
                        webBrowser.LoadHtml(localWebPath);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                Trace.WriteLine(new LogMessage("WebMedia", "Retrived the resource, stored the document but the media has already expired."), LogType.Error.ToString());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(new LogMessage("WebMedia", "Unknown exception " + ex.Message), LogType.Error.ToString());

                // This should exipre the media
                Duration = 5;
                base.RenderMedia();
            }
        }

        private void UpdateCache()
        {
            string cachedFile = "";

            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    cachedFile = reader.ReadToEnd();
                }
            }

            if (cachedFile.Contains("[[ViewPortWidth]]"))
            {
                // Handle the background
                String bodyStyle;
                String backgroundColor = options.Dictionary.Get("backgroundColor", options.backgroundColor);

                if (options.backgroundImage == null || options.backgroundImage == "")
                {
                    bodyStyle = "background-color:" + backgroundColor + " ;";
                }
                else
                {
                    bodyStyle = "background-image: url('" + options.backgroundImage + "'); background-attachment:fixed; background-color:" + backgroundColor + "; background-repeat: no-repeat; background-position: " + options.backgroundLeft + "px " + options.backgroundTop + "px;";
                }

                string html = cachedFile.Replace("</head>", "<style type='text/css'>body {" + bodyStyle + " }</style></head>");
                html = html.Replace("[[ViewPortWidth]]", _width.ToString());

                // Write to the library
                using (FileStream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (StreamWriter sw = new StreamWriter(fileStream))
                    {
                        sw.Write(html);
                        sw.Close();
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            disposed = true;

            if (disposing)
            {
                // Remove the webbrowser control
                try
                {
                    // Remove the web browser control
                    Controls.Remove(webBrowser);

                    // Workaround to remove COM object
                    PerformLayout();

                    // Detatch event and remove
                    webBrowser.LoadingStateChanged -= OnDocumentCompleted;

                    if (!webBrowser.IsDisposed)
                    {
                        webBrowser.Load("about:blank");
                        webBrowser.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(new LogMessage("IeWebMedia - Dispose", "Cannot dispose of web browser. E = " + e.Message), LogType.Info.ToString());
                }
            }

            base.Dispose(disposing);
        }
    }
}