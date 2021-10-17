using System.Collections.Generic;

namespace Graph.API.Library.RepositoryService
{
    public interface IGraphService
    {
        //Access token
        string GetAccessToken();

        //DELETE /devices/{id}
        void DeleteDevice(string access_token, string id);

        //GET /users/{id}
        string GetUserByObjectId(string access_token, string id);
        bool CheckIfUserExist(string access_token, string userPrincipalName);
        List<string> GetAllUsers(string access_token);

        //GET /users/{userPrincipalName}
        string GetUserByUserPrincipalName(string access_token, string userPrincipalName);

        //GET /groups/{id}
        string GetGroupByObjectId(string access_token, string id);

        //POST /groups/{id}/getMemberGroups
        string GetMemberGroups(string access_token, string id);

        //GET /groups/{id}/members
        string GetMembersOfGroup(string access_token, string id);

        //DELETE /users/{usersId}/managedDevices/{managedDeviceId}
        void DeleteUserManagedDeviceByUsersManagedDevice(string access_token, string userId, string managedDeviceId);

        //GET /users/{usersId}/managedDevices
        string GetUserManagedDevices(string access_token, string userId);

        //GET /deviceManagement/managedDevices
        string GetManagedDevicesByDeviceManagement(string access_token);

        //GET /deviceManagement/detectedApps/{detectedAppId}/managedDevices
        string GetManagedDevicesByDetectedApps(string access_token, string detectedAppId);

        //DELETE /users/{usersId}/managedDevices/{managedDeviceId}
        void DeleteManagedDeviceByUserManagedDevices(string access_token, string userId, string managedDeviceId);

        //DELETE /deviceManagement/managedDevices/{managedDeviceId}
        void DeleteManagedDeviceByDeviceManagement(string access_token, string managedDeviceId);

        //DELETE /deviceManagement/detectedApps/{detectedAppId}/managedDevices/{managedDeviceId}
        void DeleteManagedDeviceByDeviceManagementDetectedApps(string access_token, string detectedAppId, string managedDeviceId);
    }
}
