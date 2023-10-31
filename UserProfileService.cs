using IDP.Common.Exceptions;
using BackOffice.Authorizer.Management.Core.Api;
using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Persistence.Api;
using SCS.Client.Library;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace BackOffice.Authorizer.Management.Core
{
    public class UserProfileService : IUserProfileService
    {

        private readonly IUserProfileRepository repository;
        private readonly IScsClient scsClient;

        public UserProfileService(IUserProfileRepository repository, IScsClient scsClient)
        {
            this.repository = repository;
            this.scsClient = scsClient;
        }

        public async Task<User> FindUserProfileBy(UserProfileSearchParameters parameters)
        {
            return await repository.FindUserProfileBy(parameters);
        }

        public async Task<Acquirer[]> GetUserAcquirerCode(UserAcquirerSearchParameters parameters)
        {
            return await repository.GetUserAcquirerCode(parameters);
        }

        public async Task<UserRole[]> GetUserRoles()
        {
            return await repository.GetUserRoles();
        }

        public async Task SaveUser(User user)
        {
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await repository.SaveUser(user);
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                throw new BusinessException(ex.Message, ex);
            }
        }

        public async Task<PageableResult> GetListUser(UserSearchParameter parameters)
        {
            return await repository.GetListUser(parameters);
        }

        public async Task ActiveOrDeactiveUser(UserActiveDeactiveSearchParameter parameters)
        {
            try
            {
                var user = await repository.FindUserById(parameters.UserId);

                if (parameters.Active)
                {
                    await scsClient.ActivateUserByKey(user.ExternalKey);
                }
                else
                {
                    await scsClient.DeactivateUserByKey(user.ExternalKey);
                }

                await repository.ActiveOrDeactiveUser(parameters);

            }
            catch (Exception ex)
            {
                throw new BusinessException("UserProfile.Active.User.Error", ex);
            }
        }

        public async Task UpdateUser(User user)
        {
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await scsClient.UpdateUserAccount(user.ExternalKey, user.Name, user.Login);
                    if (user.Active)
                    {
                        await scsClient.ActivateUserByKey(user.ExternalKey);
                    }
                    else
                    {
                        await scsClient.DeactivateUserByKey(user.ExternalKey);
                    }
                    await repository.UpdateUser(user);
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                throw new BusinessException("UserProfile.Edit.User.Error", ex);
            }
        }

        public async Task UpdateUser(User user, Password newPassword)
        {
            try
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await scsClient.UpdateUserAccount(user.ExternalKey, user.Name, user.Login);
                    if (user.Active)
                    {
                        await scsClient.ActivateUserByKey(user.ExternalKey);
                    }
                    else
                    {
                        await scsClient.DeactivateUserByKey(user.ExternalKey);
                    }


                    await scsClient.ChangePassword(user.ExternalKey,Password.FromDecripted(user.Password.Value).Value,Password.FromDecripted(newPassword.Value).Value);
                    user.Password = newPassword;
                    await repository.UpdateUser(user);
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                throw new BusinessException("UserProfile.Edit.User.Error", ex);
            }
        }

        public async Task<User> FindUserById(string userId)
        {
            return await repository.FindUserById(userId);
        }

        public Task<UserPermission[]> GetPermissions()
        {
            return repository.GetPermissions();
        }

        public async Task<bool> UserExist(Guid userKey)
        {
            return await repository.UserExist(userKey);
        }

        public async Task<bool> HasRole(Guid roleKey)
        {
            return await repository.HasRole(roleKey);
        }

        public async Task<bool> HasPermission(Guid permissionKey)
        {
            return await repository.HasPermission(permissionKey);
        }

        public async Task SavePermission(UserPermission permission)
        {
            await repository.SavePermission(permission);
        }
        public async Task<Module[]> GetModules()
        {
            return await repository.GetModules();
        }

        public async Task SaveModule(Module module)
        {
            try
            {
                await repository.SaveModule(module);
            }
            catch (Exception ex)
            {
                throw new BusinessException("Module.New.Error", ex);
            }
        }

        public async Task DeleteModule(string key)
        {
            await repository.DeleteModule(key);
        }

        public async  Task<bool> HasModule(string name)
        {
            return await repository.HasModule(name);
        }
    }
}
