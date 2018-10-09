using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.ResolveAnything;

namespace Autofac.Extras.NSubstitute
{
    /// <summary>
    /// Wrapper around <see cref="Autofac"/> and <see cref="NSubstitute"/>
    /// </summary>
    [SecurityCritical]
    public class AutoSubstitute : IDisposable
    {
        private readonly IContainer _container;
        private bool _disposed;

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
            this._container = builder.Build();
            this._container.BeginLifetimeScope();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AutoSubstitute"/> class.
        /// </summary>
        [SecuritySafeCritical]
        ~AutoSubstitute()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the <see cref="IContainer"/> that handles the component resolution.
        /// </summary>
        public IContainer Container
        {
            get { return this._container; }
        }

        /// <summary>
        /// Disposes internal container.
        /// </summary>
        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
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
            return this.Container.Resolve<T>(parameters);
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
            this.Container.ComponentRegistry.Register(
                RegistrationBuilder.ForType<TImplementation>().As<TService>().InstancePerLifetimeScope().CreateRegistration());

            return this.Container.Resolve<TService>(parameters);
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
            this.Container.ComponentRegistry.Register(
                RegistrationBuilder.ForDelegate((c, p) => instance).InstancePerLifetimeScope().CreateRegistration());

            return this.Container.Resolve<TService>();
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
            if (!this._disposed)
            {
                if (disposing)
                {
                    this.Container.Dispose();
                }

                this._disposed = true;
            }
        }
    }
}
