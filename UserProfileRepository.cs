using BackOffice.Authorizer.Management.Domains;
using BackOffice.Authorizer.Management.Domains.DTOs.SearchParameters;
using BackOffice.Authorizer.Management.Persistence.Api;
using BackOffice.Authorizer.Management.Persistence.Daos;
using BackOffice.Authorizer.Management.Persistence.Entities;
using BackOffice.Authorizer.Management.Persistence.Factory;
using IDP.Authorizer;
using IDP.DBX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Threading.Tasks;

namespace BackOffice.Authorizer.Management.Persistence
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly IDataContext dataContext;
        private readonly IAuthorizerRepository authorizerRepository;
        private readonly ICampaignConfigurationFactory campaignConfigurationFactory;
        private readonly IAuthenticationRepository authenticationRepository;

        public UserProfileRepository(IAuthorizerRepository authorizerRepository, IDataContext dataContext, IAuthenticationRepository authenticationRepository,
            ICampaignConfigurationFactory campaignConfigurationFactory)
        {
            this.authorizerRepository = authorizerRepository;
            this.authenticationRepository = authenticationRepository;
            this.dataContext = dataContext;
            this.campaignConfigurationFactory = campaignConfigurationFactory;
        }

        public async Task<User> FindUserProfileBy(UserProfileSearchParameters parameters)
        {
            using (var connectionContextUser = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContextUser, campaignConfigurationFactory.Create(CampaignEnvironmentType.Production));
                return await userDAO.FindUserProfileBy(parameters);
            }
        }

        public async Task<UserRole[]> GetUserRoles()
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDao = new UserDAO(connectionContext);

                return await userDao.GetUserRoles();
            }
        }    

        public async Task<PageableResult> GetListUser(UserSearchParameter parameter)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContext, campaignConfigurationFactory.Create(CampaignEnvironmentType.Production));
                return await userDAO.GetListUser(parameter);
            }
        }


        public async Task ActiveOrDeactiveUser(UserActiveDeactiveSearchParameter parameters)
        {

            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContext);
                await userDAO.ActiveOrDeactiveUser(parameters);
            }
        }

        public async Task UpdateUser(User user)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContext);
                await userDAO.UpdateUser(user);
            }
        }

        public async Task<User> FindUserById(string id)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContext, campaignConfigurationFactory.Create(CampaignEnvironmentType.Production));

                return await userDAO.FindUserById(id);
            }
        }

        public async Task SaveUser(User user)
        {
            await DoSaveRemoteUser(user);
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContext);
                await userDAO.DoSaveUser(user, connectionContext);
                await userDAO.DoUpdateExernalAuthorizerKey(user);
            }
        }

        private async Task DoSaveRemoteUser(User user)
        {
            var userCredential = new UserCredentials(user.Name, user.Login, Password.FromDecripted(user.Password.Value).Value, user.Login);
            var registerUser = await authenticationRepository.CreateUserCredentials(userCredential);
            user.ExternalKey = registerUser.Id;
        }

        public async Task<UserPermission[]> GetPermissions()
        {
            var permissions = new List<UserPermission>();
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var permissionDAO = new PermissionDAO(connectionContext);
                return await permissionDAO.GetPermissions();
            }
        }

        public async Task<Acquirer[]> GetUserAcquirerCode(UserAcquirerSearchParameters parameters)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContext, campaignConfigurationFactory.Create(CampaignEnvironmentType.Production));
                return await userDAO.GetUserAcquirerCode(parameters);
            }
           
        }

        public async Task<bool> UserExist(Guid userKey)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContext);
                return await userDAO.UserExist(userKey);
            }
        }

        public async Task<bool> HasRole(Guid roleKey)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var userDAO = new UserDAO(connectionContext);
                return await userDAO.HasRole(roleKey);
            }
           
        }

        public async Task<bool> HasPermission(Guid permissionKey)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var permissionDAO = new PermissionDAO(connectionContext);
                return await permissionDAO.HasPermission(permissionKey);
            }
        }

        public async Task SavePermission(UserPermission permission)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var permissionDAO = new PermissionDAO(connectionContext);
                await permissionDAO.SavePermission(permission);
            }
        }

        public async Task<Module[]> GetModules()
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var moduleDAO = new ModuleDAO(connectionContext);
                return await moduleDAO.GetModules();
            }
        }

        public async Task SaveModule(Module module)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var moduleDAO = new ModuleDAO(connectionContext);
                await moduleDAO.SaveModule(module);

            }
        }

        public async Task DeleteModule(string key)
        {
            using(var connectionContext = dataContext.AcquireConnection())
            {
                var moduleDAO = new ModuleDAO(connectionContext);
                await moduleDAO.DeleteModule(key);   
            }
        }

        public async Task<bool> HasModule(string name)
        {
            using (var connectionContext = dataContext.AcquireConnection())
            {
                var moduleDAO = new ModuleDAO(connectionContext);
                return await moduleDAO.HasModule(name);
            }
        }
    }
}