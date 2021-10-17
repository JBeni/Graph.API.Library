using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Graph.API.Library.RepositoryService
{
    public class GraphService : IGraphService
    {
        // https://graph.microsoft.com/v1.0/
        // The base url for all the request for working with Graph API for Azure AD

        //{HTTP method}
        //    https://graph.microsoft.com/{version}/{resource}?{query-parameters}
        //The components of a request include:

        //{HTTP method} - The HTTP method used on the request to Microsoft Graph.
        //{version} - The version of the Microsoft Graph API your application is using.
        //{resource} - The resource in Microsoft Graph that you're referencing.
        //{query-parameters} - Optional OData query options or REST method parameters that customize the response.


        //Using filter the result for a GET Request is an array of type 'value'
        //Using directly an value from the object it will return only an object with that data

        public GraphService() { }

        public string GetAccessToken()
        {
            var mainDirectory = Directory.GetCurrentDirectory().Split("\\bin")[0];
            var builder = new ConfigurationBuilder()
                .SetBasePath(mainDirectory)
                .AddJsonFile("appsettings.json", optional: false);
            IConfigurationRoot Configuration = builder.Build();



            var clientId = Configuration["GraphAPISettings:ClientId"];
            var clientSecret = Configuration["GraphAPISettings:ClientSecret"];
            var tenantId = Configuration["GraphAPISettings:TenantId"];

            var adminAccountUsername = Configuration["GraphAPISettings:AdminAccountUsername"];
            var adminAccountPassword = Configuration["GraphAPISettings:AdminAccountPassword"];

            // Admin Account on Azure AD with a role of Service Administrator
            string tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/token/";
            string postData =
                "client_id=" + HttpUtility.UrlEncode(clientId) + "&" +
                "client_secret=" + HttpUtility.UrlEncode(clientSecret) + "&" +
                "username=" + HttpUtility.UrlEncode(adminAccountUsername) + "&" +
                "password=" + HttpUtility.UrlEncode(adminAccountPassword) + "&" +
                "grant_type=" + HttpUtility.UrlEncode("password") + "&" +
                "resource=" + HttpUtility.UrlEncode("https://graph.microsoft.com") + "&" +
                "scope=" + HttpUtility.UrlEncode("openid");

            byte[] bytePostData = Encoding.ASCII.GetBytes(postData);
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(tokenUrl);
            WebReq.Method = "POST";
            WebReq.ContentType = "application/x-www-form-urlencoded";
            WebReq.ContentLength = bytePostData.Length;

            Stream postRequestStream = WebReq.GetRequestStream();
            postRequestStream.Write(bytePostData, 0, bytePostData.Length);
            postRequestStream.Close();

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();

            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            string access_token = jsonResponse["access_token"].Value<string>();
            return access_token;
        }

        //GET /users/{id}
        public string GetUserByObjectId(string access_token, string id)
        {
            string user = $"https://graph.microsoft.com/v1.0/users/{id}";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(user);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["id"].Value<string>();
            return result;
        }

        public bool CheckIfUserExist(string access_token, string userPrincipalName)
        {
            string user = $"https://graph.microsoft.com/v1.0/users?$filter=userPrincipalName%20eq%20'{userPrincipalName}'";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(user);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();

            if (result.Count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        // An idea is to create an Object with this data, with the useful
        //GET User
        public string GetUserByUserPrincipalName(string access_token, string userPrincipalName)
        {
            string user = $"https://graph.microsoft.com/v1.0/users?$filter=userPrincipalName%20eq%20'{userPrincipalName}'";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(user);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();

            var sendResult = "";
            if (result.Count == 1)
            {
                sendResult = (from userDetails in result
                              select userDetails["id"].Value<string>()
                             ).ToList().FirstOrDefault();
            }
            else
            {
                // It's like an error occurred, the user that is logged in, cannot be found
                //after the unique identifier (it's about UserPrincipalName)
                // Log the error
            }
            return sendResult;
        }

        //List Users
        public List<string> GetAllUsers(string access_token)
        {
            string usersRequest = $"https://graph.microsoft.com/v1.0/users";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(usersRequest);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();

            List<string> users = new List<string>();
            if (result.Count > 0)
            {
                users = (from user in result
                         select user["displayName"].Value<string>()
                             ).ToList();
            }
            return users;
        }

        //GET /groups/{id}
        public string GetGroupByObjectId(string access_token, string id)
        {
            string group = $"https://graph.microsoft.com/v1.0/groups/{id}";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
            return result.ToString();
        }

        //POST /groups/{id}/getMemberGroups
        public string GetMemberGroups(string access_token, string id)
        {
            string group = $"https://graph.microsoft.com/v1.0/groups/{id}/getMemberGroups";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "POST";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
            return result.ToString();
        }

        //GET /groups/{id}/members
        public string GetMembersOfGroup(string access_token, string id)
        {
            string group = $"https://graph.microsoft.com/v1.0/groups/{id}/members";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
            return result.ToString();
        }

        //GET /users/{usersId}/managedDevices
        public string GetUserManagedDevices(string access_token, string userId)
        {
            string group = $"https://graph.microsoft.com/v1.0/users/{userId}/managedDevices";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
            return result.ToString();
        }

        //GET /deviceManagement/managedDevices
        public string GetManagedDevicesByDeviceManagement(string access_token)
        {
            string group = $"https://graph.microsoft.com/v1.0/deviceManagement/managedDevices";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
            return result.ToString();
        }

        //GET /deviceManagement/detectedApps/{detectedAppId}/managedDevices
        public string GetManagedDevicesByDetectedApps(string access_token, string detectedAppId)
        {
            string group = $"https://graph.microsoft.com/v1.0/deviceManagement/detectedApps/{detectedAppId}/managedDevices";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "GET";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
            return result.ToString();
        }

        //Delete /devices/{id}
        public void DeleteDevice(string access_token, string id)
        {
            string deleteDevice = $"https://graph.microsoft.com/v1.0/devices/{id}";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(deleteDevice);
            WebReq.Method = "DELETE";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            // 204 No Content - in case of success
        }

        //DELETE /users/{usersId}/managedDevices/{managedDeviceId}
        public void DeleteUserManagedDeviceByUsersManagedDevice(string access_token, string userId, string managedDeviceId)
        {
            string group = $"https://graph.microsoft.com/v1.0/users/{userId}/managedDevices/{managedDeviceId}";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "DELETE";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
        }

        //DELETE /users/{usersId}/managedDevices/{managedDeviceId}
        public void DeleteManagedDeviceByUserManagedDevices(string access_token, string userId, string managedDeviceId)
        {
            string group = $"https://graph.microsoft.com/v1.0/users/{userId}/managedDevices/{managedDeviceId}";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "DELETE";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
        }

        //DELETE /deviceManagement/managedDevices/{managedDeviceId}
        public void DeleteManagedDeviceByDeviceManagement(string access_token, string managedDeviceId)
        {
            string group = $"https://graph.microsoft.com/v1.0/deviceManagement/managedDevices/{managedDeviceId}";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "DELETE";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
        }

        //DELETE /deviceManagement/detectedApps/{detectedAppId}/managedDevices/{managedDeviceId}
        public void DeleteManagedDeviceByDeviceManagementDetectedApps(string access_token, string detectedAppId, string managedDeviceId)
        {
            string group = $"https://graph.microsoft.com/v1.0/deviceManagement/detectedApps/{detectedAppId}/managedDevices/{managedDeviceId}";
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(group);
            WebReq.Method = "DELETE";
            WebReq.Headers.Add("Authorization", "Bearer " + access_token);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream responseStream = WebResp.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream);
            var stringResponse = responseStreamReader.ReadToEnd();
            var jsonResponse = (JObject)JsonConvert.DeserializeObject(stringResponse);
            var result = jsonResponse["value"].ToList();
        }
    }
}
