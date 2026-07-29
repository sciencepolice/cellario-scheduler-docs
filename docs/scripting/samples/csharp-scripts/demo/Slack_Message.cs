/**
 * 02 -  Slack Message
 * 
 * Posts messages to Slack channels via webhook integration
 */

using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using HRB.Cellario.Scripting.API;

namespace Customer.Scripting
{
    public class MyScript : AbstractScript
    {
        public override void Execute(IScriptingApi api)
        {
            // Replace with your own Slack Incoming Webhook URL. Create one at
            // https://api.slack.com/messaging/webhooks -- the URL is a credential,
            // so keep it out of source control (read it from config or an
            // environment variable in anything beyond a throwaway sample).
            var url = "https://hooks.slack.com/services/YOUR/WEBHOOK/URL";
            var message = "Shake is Complete";
            var slackMessage = @"
                                {
                                ""text"": """ + message + @"""
                                }";
                       
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json =  slackMessage;
            
                streamWriter.Write(json);
            }            
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }
    }
}