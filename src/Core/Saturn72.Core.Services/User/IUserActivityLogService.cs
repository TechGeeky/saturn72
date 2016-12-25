﻿
using System.Threading.Tasks;
using Saturn72.Core.Domain.Users;

namespace Saturn72.Core.Services.User
{
    public interface IUserActivityLogService
    {
        Task<UserActivityLogDomainModel> AddUserActivityLogAsync(UserActivityType userActivityType, UserDomainModel user);
    }
}
