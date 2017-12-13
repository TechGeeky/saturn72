#region

using System;
using System.Collections.Generic;
using System.Linq;
using Saturn72.Core.Configuration;
using Saturn72.Core.Infrastructure.AppDomainManagement;
using Saturn72.Core.Infrastructure.DependencyManagement;
using Saturn72.Core.Tasks;

#endregion

namespace Saturn72.Core.Infrastructure
{
    public class AppEngineDriver : IAppEngineDriver
    {
        #region Fields

        private Saturn72Config _config;
        private IIocContainerManager _iocContainerManager;

        #endregion

        #region Properties

        public IIocContainerManager IocContainerManager
        {
            get { return _iocContainerManager ?? (_iocContainerManager = CreateContainerManager()); }
        }

        #endregion Properties

        public virtual void Initialize(Saturn72Config config)
        {
            _config = config;
            //TODO: uncomment for plugins support
            //if (!CommonHelper.IsWebApp())
            //    LoadPluginsToAppDomain();

            var typeFinder = GetTypeFinder();
            RegisterDependencies(typeFinder);
            RunStartupTasks(typeFinder);
        }

        public void Dispose()
        {
            RunDisposeTasks();
        }

        protected virtual ITypeFinder GetTypeFinder()
        {
            return new AppDomainTypeFinder();
        }

        protected virtual IIocContainerManager CreateContainerManager()
        {
            return CommonHelper.CreateInstance<IIocContainerManager>(_config.ContainerManager);
        }

        #region Utilities

        /// <summary>
        ///     Registers all dependencies across domain
        /// </summary>
        /// <param name="typeFinder">TypeFinder <see cref="ITypeFinder" /></param>
        protected virtual void RegisterDependencies(ITypeFinder typeFinder)
        {
            var registrars = new List<Action<IIocRegistrator>>
            {
                r => r.RegisterInstance<IAppEngineDriver>(this),
                r => r.RegisterInstance(typeFinder),
                r => r.RegisterInstance<IIocResolver>(IocContainerManager),
                r => r.RegisterInstance<IIocRegistrator>(IocContainerManager)
            };
            IocContainerManager.Register(registrars);

            registrars.Clear();
            //Register other dependencies in app domain
            typeFinder.FindClassesOfTypeAndRunMethod<IDependencyRegistrar>(
                dr => registrars.Add(dr.RegistrationLogic(typeFinder, _config)),
                dr => dr.RegistrationOrder);
            if (registrars.Any())
                IocContainerManager.Register(registrars);
        }

        /// <summary>
        ///     Runs all startup task across domain
        /// </summary>
        /// <param name="typeFinder">TypeFinder <see cref="ITypeFinder" /></param>
        protected virtual void RunStartupTasks(ITypeFinder typeFinder)
        {
            typeFinder.FindClassesOfTypeAndRunMethod<IStartupTask>(s => s.Execute(), s => s.ExecutionIndex);
        }

        protected virtual void RunDisposeTasks()
        {
            this.Resolve<ITypeFinder>().FindClassesOfTypeAndRunMethod<IDisposeTask>(s => s.Execute(), s => s.ExecutionIndex);
        }
        #endregion Utilities
    }
}