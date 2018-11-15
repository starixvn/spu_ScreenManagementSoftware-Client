using System;
using System.Net;

namespace XiboClient.XmdsAgents
{
    public class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);
            req.Timeout = 600000;
            return req;
        }
    }
}
