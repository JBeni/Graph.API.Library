using Graph.API.Library.RepositoryService;
using System;

namespace Graph.API.Library
{
    public class Console
    {
        private static IGraphService _graphService;

        // Configuration needed before working, small changes quite possible to work properly
        // The Library was written in 2019 and I do not have the resources to test it again using Azure
        // but the library will work in the proportion of 90% without issues
        static void Main(String[] args)
        {
            _graphService = new GraphService();

            var id = "29688236-6a92-4122-b6ee-a375fb96f3c3";
            var userPrincipalName = "username@domain.onmicrosoft.com";
            var access_token = _graphService.GetAccessToken();

            //GET /users/{id}
            var user = _graphService.GetUserByObjectId(access_token, id);
            var userExists = _graphService.CheckIfUserExist(access_token, userPrincipalName);
        }
    }
}
