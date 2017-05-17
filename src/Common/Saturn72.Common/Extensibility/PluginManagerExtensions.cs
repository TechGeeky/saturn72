﻿using System;
using Saturn72.Core.Extensibility;
using Saturn72.Core.Services.Extensibility;
using Saturn72.Extensions;

namespace Saturn72.Common.Extensibility
{
    public static  class PluginManagerExtensions
    {
        public static PluginDescriptor GetByType(this IPluginManager pluginManager, string typeName)
        {
            var type = Type.GetType(typeName);
            Guard.NotNull(type);
            return pluginManager.GetByType(type);
        }
    }
}
