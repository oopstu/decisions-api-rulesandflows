using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

/// <summary>
/// This is an example console application illustrating using a variety of 
/// calls to decisions to look for and activate the items and designs that are
/// usually useful when running the rule and workflow engine.  This is example code
/// provided as-is without any warranty or any other legal-esque hangups.  
/// Hope it helps!
/// 
/// 1. Login
/// 2. Get Folders for User
/// 3. Select Folder
/// 4. List contents of folder
/// 5. Specify item in folder
/// 6. Get Inputs and Outputs for Selected Item
/// 7. Invoke selected flow with inputs
/// </summary>
namespace Decisions.RESTAPI.Example
{
    class Program
    {
        private static readonly string VDIR = "decisions";

        static void Main(string[] args)
        {
            string ipOfDecisions = PromptForString("Enter ip of your decisions install (or nothing for localhost):", "localhost");
            string user = PromptForString("Enter Decisions user email (nothing for admin@decisions.com):", "admin@decisions.com");
            string password = PromptForString("Password (or nothing for admin):", "admin");

            // To make calls to decisions services you have to have a user context
            // the BEST way is to take out a session id by logging in.  Alternatively you can pass
            // user id and password to every call that you make and have every call authenticated
            // independently.
            string sessionId = GetSessionUsingUIDandPWD(ipOfDecisions, user, password);
            Console.Out.WriteLine($"Your session id is {sessionId}");

            // Now list folders because folders contain flows and rules and this will allow us to select 
            // a folder and list contents of the folder for the next steps.
            string[] folderNamesAndIds = GetFoldersForUser(ipOfDecisions, sessionId);
            int index = 1;
            Console.Out.WriteLine("Your folders:");
            foreach (string folderNameAndId in folderNamesAndIds)
            {
                Console.Out.WriteLine($"{index++}: {folderNameAndId}");
            }
            int folderIndexToSelect = PromptForInt("Select a folder to list contents: ");

            // Get the selected folder and call the service to list the items 
            // in the folder so that one can be selected.
            string[] entitiesInFolder = ListContentsForFolderToSeeFlowsRulesAndForms(ipOfDecisions, sessionId, folderNamesAndIds[folderIndexToSelect - 1].Split(':')[1]);
            index = 1;

            Console.Out.WriteLine("");
            Console.Out.WriteLine("Select Item in Folder:");
            foreach (string itemInFolder in entitiesInFolder)
            {
                Console.Out.WriteLine($"{index++}: {itemInFolder}");
            }
            int itemInFolderIndex = PromptForInt("Select an item for more detail: ");
            string[] inputsForFlowOrRule = GetDataForFlowOrRule(ipOfDecisions, sessionId, entitiesInFolder[itemInFolderIndex - 1].Split(':')[1]);

            Console.Out.WriteLine("");
            Console.Out.WriteLine("Data Required to Run Flow:");
            foreach (string flowInput in inputsForFlowOrRule)
            {
                Console.Out.WriteLine($"{flowInput}");
            }
            Console.Out.WriteLine("Now you can marry this up to the pattern established by using 'View Integration Details' in a FLOW or RULE to learn how to call the flow or rule with the described inputs.");



            Console.In.Read(); // Hold console open to allow you to review changes.
        }

        private static string[] GetDataForFlowOrRule(string ipOfDecisions, string sessionId, string flowId)
        {

            // On the ConfigurationStorage service there are many many methods for dealing with
            // flows rules reports pages and more.  For more info: http://localhost/decisions/Primary/Help/ConfigurationStorage
            string url = $"http://{ipOfDecisions}/{VDIR}/Primary/REST/ConfigurationStorage/GetElementInputTypeDescription";
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"sessionid\" : \"").Append(sessionId).Append("\",");
            sb.Append("\"elementRegistrationID\" : \"").Append(flowId).Append("\", ");
            sb.Append("\"outputtype\" : \"Json\" }");

            string body = sb.ToString();

            // Get input Data....
            List<string> allInputAndOutputData = new List<string>();
            // URL to Account 
            GetPOSTResponse(new Uri(url), body, (res) =>
            {
                JObject json = JObject.Parse(res);
                JToken inputsResult = json.SelectToken("GetElementInputTypeDescriptionResult.TypeDescription");
                if (inputsResult != null)
                {
                    foreach (var x in inputsResult)
                    {
                        JToken item = x.SelectToken("NodeInfo");
                        string inputName = item.Value<string>("Name");
                        string inputType = item.Value<string>("FormattedTypeName");
                        string inputIsList = item.Value<string>("IsList");
                        allInputAndOutputData.Add($"Input: '{inputName}' ({inputType}) Is List: {inputIsList}");
                    }
                }
            });

            url = $"http://{ipOfDecisions}/{VDIR}/Primary/REST/ConfigurationStorage/GetElementOutputTypeDescription";
            GetPOSTResponse(new Uri(url), body, (res) =>
            {
                JObject json = JObject.Parse(res);
                JToken outputsResult = json.SelectToken("GetElementOutputTypeDescription.TypeDescription");
                if (outputsResult != null)
                {
                    foreach (var x in outputsResult)
                    {
                        JToken item = x.SelectToken("NodeInfo");
                        string outputNAme = item.Value<string>("Name");
                        string outputTypeName = item.Value<string>("FormattedTypeName");
                        string outputIsList = item.Value<string>("IsList");
                        allInputAndOutputData.Add($"Output: '{outputNAme}' ({outputTypeName}) Is List: {outputIsList}");
                    }
                }
            });


            return allInputAndOutputData.ToArray();
        }

        private static string[] ListContentsForFolderToSeeFlowsRulesAndForms(string ipOfDecisions, string sessionId, string selectedFolderId)
        {
            // On the folder service there are many many methods for getting entities and searching
            // folders using different filters.  This is not necessarily a sensible way to list items 
            // in a folder, especially if the folder has many contents.  Please see this (substituting sensible
            // values) for more info: http://localhost/decisions/Primary/Help/FolderService
            string url = $"http://{ipOfDecisions}/{VDIR}/Primary/REST/FolderService/GetAllEntities";
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"sessionid\" : \"").Append(sessionId).Append("\",");
            sb.Append("\"id\" : \"").Append(selectedFolderId).Append("\", ");
            sb.Append("\"outputtype\" : \"Json\" }");

            string body = sb.ToString();

            List<string> folderResults = new List<string>();
            // URL to Account 
            GetPOSTResponse(new Uri(url), body, (res) =>
            {
                JObject json = JObject.Parse(res);
                JToken foldersResult = json.GetValue("GetAllEntitiesResult");
                foreach (var x in foldersResult)
                {
                    string folderName = ((JToken)x).Value<string>("EntityName");
                    string folderId = ((JToken)x).Value<string>("Id");
                    string shortTypeName = ((JToken)x).Value<string>("ShortTypeName");
                    folderResults.Add($"{folderName} ({shortTypeName}):{folderId}");
                }
            });
            return folderResults.ToArray();
        }

        // List folders for users.  Folders contain flows and rules
        // http://localhost/decisions/Primary/Help/FolderService <-- For more info on folder service
        // Folders are the main "categories" and "security" in Decisions and useful for sorting,
        // grouping, searching.  Folders are OFTEN related to a "project"
        private static string[] GetFoldersForUser(string ipOfDecisions, string sessionId)
        {
            string url = $"http://{ipOfDecisions}/{VDIR}/Primary/REST/FolderService/GetMyRootFolders";
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"sessionid\" : \"").Append(sessionId).Append("\",");
            sb.Append("\"outputtype\" : \"Json\" }");

            string body = sb.ToString();

            List<string> folderResults = new List<string>();
            // URL to Account 
            GetPOSTResponse(new Uri(url), body, (res) =>
            {
                JObject json = JObject.Parse(res);
                JToken foldersResult = json.GetValue("GetMyRootFoldersResult");
                foreach (var x in foldersResult)
                {
                    string folderName = ((JToken)x).Value<string>("EntityName");
                    string folderId = ((JToken)x).Value<string>("FolderID");
                    folderResults.Add($"{folderName}:{folderId}");
                }
            });
            return folderResults.ToArray();
        }

        private static string GetSessionUsingUIDandPWD(string ipOfDecisions, string user, string password)
        {
            string sessionId = null;

            string url = $"http://{ipOfDecisions}/{VDIR}/Primary/REST/AccountService/LoginUser";
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"userid\" : \"").Append(user).Append("\", \"password\" : \"").Append(password).Append("\",");
            sb.Append("\"outputtype\" : \"Json\" }");

            string body = sb.ToString();

            // URL to Account 
            GetPOSTResponse(new Uri(url), body, (res) =>
            {
                sessionId = ReadValueFromResponse(res, "LoginUserResult.SessionValue");
            });
            return sessionId;
        }


        private static string ReadValueFromResponse(string jsonResult, string pathToValue)
        {
            JObject o = JObject.Parse(jsonResult);

            //This will be "Apple"
            string stringResultIfAny = (string)o.SelectToken(pathToValue);
            return stringResultIfAny;
        }

        private static string PromptForString(string v, string def)
        {
            Console.Out.WriteLine(v);
            string resultValue = Console.In.ReadLine();
            if (string.IsNullOrEmpty(resultValue))
            {
                return def;
            }
            return resultValue;
        }

        private static int PromptForInt(string v)
        {
            Console.Out.WriteLine(v);
            string resultValue = Console.In.ReadLine();
            int result = -1;
            int.TryParse(resultValue, out result);
            return result;
        }

        private static void GetResponse(Uri uri, Action<string> callback)
        {
            WebClient wc = new WebClient();
            wc.OpenReadCompleted += (o, a) =>
            {
                if (callback != null)
                {
                    //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(WebResponse));
                    //callback(ser.ReadObject(a.Result) as WebResponse);
                }
            };
            wc.OpenReadAsync(uri);
        }

        private static void GetPOSTResponse(Uri uri, string data, Action<string> callback)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);

            request.Method = "POST";
            request.ContentType = "text/plain;charset=utf-8";

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(data);

            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                // Send the data.
                requestStream.Write(bytes, 0, bytes.Length);
            }

            request.BeginGetResponse((x) =>
            {
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(x))
                    {
                        if (callback != null)
                        {
                            StreamReader sr = new StreamReader(response.GetResponseStream());
                            callback(sr.ReadToEnd());

                        }
                    }
                }
                catch (WebException we)
                {
                    StreamReader sr = new StreamReader(we.Response.GetResponseStream());
                    Console.Out.WriteLine("Error! " + sr.ReadToEnd());
                }
            }, null);

        }
    }
}
