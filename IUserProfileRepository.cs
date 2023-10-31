using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using System;
using System.Threading.Tasks;

namespace  BackOffice.Authorizer.Management.Persistence.Api
{
    public interface IUserProfileRepository
    {
        Task<User> FindUserProfileBy(UserProfileSearchParameters parameters);
        Task<UserRole[]> GetUserRoles();
        Task<UserPermission[]> GetPermissions();
        Task<Acquirer[]> GetUserAcquirerCode(UserAcquirerSearchParameters parameters);
        Task<PageableResult> GetListUser(UserSearchParameter parameters);
        Task ActiveOrDeactiveUser(UserActiveDeactiveSearchParameter parameters);
        Task UpdateUser(User parameters);
        Task<User> FindUserById(string id);
        Task SaveUser(User user);
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
