using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Amazon.Lambda.Serialization;
using System.Runtime.Serialization;
using Amazon.Lambda.Serialization.SystemTextJson;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AlexaTest
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillResponse input, ILambdaContext context)
        {
            //return input?.ToUpper();
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;

            IOutputSpeech innerResponse = null;

            var log = context.Logger;
            log.LogLine($"skill request object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            var allResources = GetResources();
            var resource = allResources.FirstOrDefault();

            if(input.GetRequestType() = typeof(LaunchRequest))
            {
                log.LogLine($"Default Launch Request made: Alexa open AWS facts");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = EmitNewFact(resource, true); 
            }
            else if(input.GetRequestType() = typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;
                switch(intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        break;
                    case "GetAWSFactIntent":
                        log.LogLine($"GetFactIntent sent: send nbew fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = EmitNewFact(resource, false);
                        break;
                    case "GetAWSNewFactIntent":
                        log.LogLine($"GetFactIntent sent: send nbew fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = EmitNewFact(resource, false);
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                        break;
                }
            }
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";

            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));
           
            return response;
        }

        public List<FactResource> GetResources()
        {
            List<FactResource> resources = new List<FactResource>();
            FactResource enUSResource = new FactResource("en-US");

            enUSResource.SkillName = "AWS Facts";

            enUSResource.GetFactMessage = "Here's your fact: ";

            enUSResource.HelpMessage = "You can tell me a AWS fact, or, you can say exit... What can I help you with?";
            enUSResource.HelpReprompt = "What can I help you with?";

            enUSResource.StopMessage = "Goodbye!";

            enUSResource.Facts.Add("AWS is super cool, you should use it");

            resources.Add(enUSResource);
            return resources;
        }

        public string EmitNewFact(FactResource resource, bool withPreface)
        {
            Random r = new Random();

            if(withPreface)
            {
                return resource.GetFactMessage + 
                    resource.Facts[r.Next(resource.Facts.Count)];
            }
            return resource.Facts[r.Next(resource.Facts.Count)];
        }
    }
    public class FactResource
    {
        public FactResource(string language)
        {
            this.Language = language;
            this.Facts = new List<string>();
        }
        public string Language { get; set; }
        public string SkillName { get; set; }
        public List<string> Facts { get; set; }
        public string GetFactMessage { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string StopMessage { get; set; }
    }
}

