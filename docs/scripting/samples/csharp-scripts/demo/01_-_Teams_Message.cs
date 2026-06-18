/**
 * 01 - Teams Message
 * 
 * Sends notifications to Microsoft Teams channels using webhooks
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
            var url = "https://highresbiosolutions.webhook.office.com/webhookb2/77df5ff6-0046-4103-b872-b24f33bfa708@e5670c64-747d-4682-9c55-bae5601793fa/IncomingWebhook/6b4ed68a89bb4943967d78245d4800bc/bbc4d2f4-242d-468d-8727-5e5014165cce";
            var message = "Shake is Complete!";
            var adaptiveCard = @"
            {
                ""type"": ""message"",
                ""attachments"": [
                    {
                        ""contentType"": ""application/vnd.microsoft.card.adaptive"",
                        ""contentUrl"": null,
                        ""content"": {
                            ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                            ""type"": ""AdaptiveCard"",
                            ""version"": ""1.5"",
                            ""body"": [
                                {
                                    ""type"": ""TextBlock"",
                                    ""text"": """ + message + @"""
                                }
                            ]
                        }
                    }
                ]
            }";
                       
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json =  adaptiveCard;
            
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