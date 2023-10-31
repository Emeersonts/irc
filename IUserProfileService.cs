using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using System;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Core.Api
{
    public interface IUserProfileService
    {
        Task<User> FindUserProfileBy(UserProfileSearchParameters parameters);
        Task<UserRole[]> GetUserRoles();
        Task<Acquirer[]> GetUserAcquirerCode(UserAcquirerSearchParameters parameters);
        Task SaveUser(User user);
        Task<PageableResult> GetListUser(UserSearchParameter parameters);
        Task ActiveOrDeactiveUser(UserActiveDeactiveSearchParameter parameters);
        Task UpdateUser(User user);
        Task UpdateUser(User user, Password newPassword);
        Task<User> FindUserById(string userId);
        Task<UserPermission[]> GetPermissions();
        Task<bool> UserExist(Guid userKey);
        Task<bool> HasRole(Guid roleKey);
        Task<bool> HasPermission(Guid permissionKey);
        Task SavePermission(UserPermission permission);
        Task<Module[]> GetModules();
        Task SaveModule(Module module);
        Task DeleteModule(string key);
        Task<bool> HasModule(string name);
    }
}
