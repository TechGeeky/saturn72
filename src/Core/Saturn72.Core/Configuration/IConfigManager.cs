﻿#region

using System;
using System.Collections.Generic;
using Saturn72.Core.Configuration.Maps;
using Saturn72.Core.Infrastructure.AppDomainManagement;

#endregion

namespace Saturn72.Core.Configuration
{
    public interface IConfigManager
    {
        IDictionary<string, Lazy<IConfigMap>> ConfigMaps { get; }

        AppDomainLoadData AppDomainLoadData { get; }
    }
}