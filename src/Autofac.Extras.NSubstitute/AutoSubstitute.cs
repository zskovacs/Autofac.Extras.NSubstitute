using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core;
using Autofac.Features.ResolveAnything;
using NSubstitute;

namespace Autofac.Extras.NSubstitute
{
    /// <summary>
    /// Wrapper around <see cref="Autofac"/> and <see cref="NSubstitute"/>
    /// </summary>
    public class AutoSubstitute : IDisposable
    {
        private bool _disposed;

        private readonly Stack<ILifetimeScope> _scopes = new Stack<ILifetimeScope>();

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "It's only a reference, dispose is called from the _scopes Stack")]
        private ILifetimeScope _currentScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSubstitute" /> class.
        /// </summary>
        /// <param name="builder">The container builder to use to build the container.</param>
        public AutoSubstitute(ContainerBuilder builder = null)
        {
            if (builder == null)
            {
                builder = new ContainerBuilder();
            }

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource().WithRegistrationsAs(b => b.InstancePerLifetimeScope()));
            builder.RegisterSource(new SubstituteRegistrationHandler());
            Container = builder.Build();
            _currentScope = Container.BeginLifetimeScope();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AutoSubstitute"/> class.
        /// </summary>
        ~AutoSubstitute()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the <see cref="IContainer"/> that handles the component resolution.
        /// </summary>
        public IContainer Container { get; }

        /// <summary>
        /// Disposes internal container.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed)
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>The service.</returns>
        public T Resolve<T>(params Parameter[] parameters)
        {
            return _currentScope.Resolve<T>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed)
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the service.</typeparam>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>The service.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService, TImplementation>(params Parameter[] parameters)
        {
            var scope = _currentScope.BeginLifetimeScope(b =>
            {
                b.RegisterType<TImplementation>().As<TService>().InstancePerLifetimeScope();
            });

            _scopes.Push(scope);
            _currentScope = scope;

            return _currentScope.Resolve<TService>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register specified instance if needed)
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="instance">The instance to register if needed.</param>
        /// <returns>The instance resolved from container.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService>(TService instance)
            where TService : class
        {
            var scope = _currentScope.BeginLifetimeScope(b =>
            {
                b.Register(c => instance).InstancePerLifetimeScope();
            });

            _scopes.Push(scope);
            _currentScope = scope;

            return _currentScope.Resolve<TService>();
        }

        /// <summary>
        /// Resolve the specified type's partial subs in the container (register it if needed)
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the service.</typeparam>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>The service's partial substitute.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService ProvidePartsOf<TService, TImplementation>(params Parameter[] parameters)
            where TImplementation : class
        {
            var scope = _currentScope.BeginLifetimeScope(b => b.Register(c => Substitute.ForPartsOf<TImplementation>(parameters)).As<TService>().InstancePerLifetimeScope());

            _scopes.Push(scope);
            _currentScope = scope;

            return _currentScope.Resolve<TService>();
        }

        /// <summary>
        /// Handles disposal of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to dispose of managed resources (during a manual execution
        /// of <see cref="Autofac.Extras.NSubstitute.AutoSubstitute.Dispose()"/>); or
        /// <see langword="false" /> if this is getting run as part of finalization where
        /// managed resources may have already been cleaned up.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    while (_scopes.Count > 0)
                        _scopes.Pop().Dispose();

                    Container.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
