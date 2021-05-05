// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceDiscoveryFactory.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Hosting.Services
{
    using StoneAssemblies.Hosting.Services.Interfaces;
    using StoneAssemblies.Hosting.Services.MarathonClient;

    /// <summary>
    /// The service discovery factory.
    /// </summary>
    public static class ServiceDiscoveryFactory
    {
        /// <summary>
        /// Gets the service discovery.
        /// </summary>
        /// <returns>
        /// The <see cref="IServiceDiscovery"/>.
        /// </returns>
        public static IServiceDiscovery GetServiceDiscovery()
        {
            if (HostingEnvironment.IsMarathonHosted())
            {
                return new MarathonServiceDiscovery();
            }

            return new KubernetesServiceDiscovery();
        }
    }
}