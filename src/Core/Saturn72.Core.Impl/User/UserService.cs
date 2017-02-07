﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Saturn72.Core.Caching;
using Saturn72.Core.Domain.Security;
using Saturn72.Core.Domain.Users;
using Saturn72.Core.Services.Events;
using Saturn72.Core.Services.User;
using Saturn72.Extensions;

#endregion

namespace Saturn72.Core.Services.Impl.User
{
    public class UserService : IUserService
    {
        private readonly AuditHelper _auditHelper;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository, IEventPublisher eventPublisher, ICacheManager cacheManager, AuditHelper auditHelper)
        {
            _userRepository = userRepository;
            _eventPublisher = eventPublisher;
            _cacheManager = cacheManager;
            _auditHelper = auditHelper;
        }

        public Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            return Task.Run(() => _cacheManager.Get(SystemSharedCacheKeys.AllUsersCacheKey, () => _userRepository.GetAll()));
        }

        public async Task<UserModel> GetUserByUsername(string username)
        {
            Guard.HasValue(username);
            return await GetUserByFuncAndCacheIfExists(u => u.Active && u.Username == username);
        }

        public async Task<UserModel> GetUserByEmail(string email)
        {
            Guard.HasValue(email);
            Guard.MustFollow(CommonHelper.IsValidEmail(email), "invalid email address");
            return await GetUserBy(user => user.Email.EqualsTo(email));
        }

        public async Task<IEnumerable<UserRoleModel>> GetUserUserRolesByUserIdAsync(long userId)
        {
            Guard.GreaterThan(userId, (long)0);

            return await Task.FromResult(
                _cacheManager.Get(SystemSharedCacheKeys.UserRolesUserCacheKey.AsFormat(userId),
                    () => _userRepository.GetUserUserRoles(userId) ?? new UserRoleModel[] { }));
        }

        public async Task UpdateUser(UserModel user)
        {
            Guard.NotNull(user);
            _auditHelper.PrepareForUpdateAudity(user);
            await Task.Run(() => _userRepository.Update(user));
            _cacheManager.RemoveByPattern(SystemSharedCacheKeys.AllUsersCacheKey);
            _eventPublisher.DomainModelUpdated(user);
        }

        public async Task<IEnumerable<PermissionRecordModel>> GetUserPermissionsAsync(long userId)
        {
            Guard.GreaterThan(userId, (long)0);

            return await Task.Run(() => _userRepository.GetUserPermissions(userId));
        }

        public async Task<UserModel> GetUserBy(Func<UserModel, bool> func)
        {
            var users = await GetAllUsersAsync();
            return users.FirstOrDefault(func);
        }


        protected virtual async Task<UserModel> GetUserByFuncAndCacheIfExists(Func<UserModel, bool> func)
        {
            Guard.NotNull(func);
            var allUsers = await GetAllUsersAsync();
            var user = allUsers.FirstOrDefault(func);
            if (user.NotNull())
                _cacheManager.SetIfNotExists(SystemSharedCacheKeys.UserCacheKey.AsFormat(user.Id), user);
            return user;
        }
    }
}