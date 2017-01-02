﻿#region

using System.Linq;
using System.Text.RegularExpressions;
using Saturn72.Core.Caching;
using Saturn72.Core.Domain.Clients;
using Saturn72.Core.Infrastructure.AppDomainManagement;
using Saturn72.Core.Services.Authentication;
using Saturn72.Core.Services.Events;

#endregion

namespace Saturn72.Core.Services.Impl.Authentication
{
    public class ClientAppService : DomainModelCrudServiceBase<ClientAppDomainModel, long, long>, IClientAppService
    {
        #region ctor

        public ClientAppService(IClientAppRepository clientAppRepository, IEventPublisher eventPublisher, ICacheManager cacheManager, ITypeFinder typeFinder, IWorkContext<long> workContext)
            : base(clientAppRepository, eventPublisher, cacheManager, typeFinder, workContext)
        {
        }

        #endregion

        public ClientAppDomainModel GetClientAppByClientId(string clientId, string clientIpAddress)
        {
            return
                FilterTable(
                    ca => ca.Active && (ca.ClientId == clientId) && Regex.IsMatch(clientIpAddress, ca.AllowedOrigin))
                    .FirstOrDefault();
        }
    }
}