// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarathonServiceDiscovery.cs" company="Stone Assemblies">
// Copyright © 2021 - 2021 Stone Assemblies. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace StoneAssemblies.Hosting.Services.MarathonClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using Serilog;

    using StoneAssemblies.Hosting.Services.Interfaces;
    using StoneAssemblies.Hosting.Services.MarathonClient.Models;

    /// <summary>
    ///     The marathon client service.
    /// </summary>
    public class MarathonServiceDiscovery : IServiceDiscovery
    {
        /// <summary>
        ///     The default marathon service end point.
        /// </summary>
        private const string ServiceEndPoint = "/service/marathon/v2";

        /// <summary>
        ///     The marathon service end point.
        /// </summary>
        private readonly string marathonServiceEndPoint;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MarathonServiceDiscovery" /> class.
        /// </summary>
        public MarathonServiceDiscovery()
            : this("http://master.mesos")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MarathonServiceDiscovery" /> class.
        /// </summary>
        /// <param name="marathonServiceBaseUrl">
        ///     The marathon service base url.
        /// </param>
        public MarathonServiceDiscovery(string marathonServiceBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(marathonServiceBaseUrl))
            {
                throw new ArgumentException(nameof(marathonServiceBaseUrl));
            }

            this.marathonServiceEndPoint =
                new Uri(new Uri(marathonServiceBaseUrl.TrimEnd(' ', '/')), ServiceEndPoint).AbsoluteUri;
        }

        public Task<string> GetServiceEndPoint(string serviceName, string bindingName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetServiceEndPointAddressAsync(string serviceName, string protocol)
        {
            var endPoint = await this.GetServiceEndPointAsync(serviceName);

            return $"{protocol}://{endPoint}";
        }

        public async Task<string> GetServiceEndPointAddressAsync(
            string serviceName,
            string bindingName,
            string protocol)
        {
            var endPoint = await this.GetServiceEndPointAsync(serviceName, bindingName);

            return $"{protocol}://{endPoint}";
        }

        public async Task<string> GetServiceEndPointAsync(string serviceName)
        {
            return await this.GetServiceEndPointAsync(serviceName, string.Empty);
        }

        public async Task<string> GetServiceEndPointAsync(string serviceName, string bindingName)
        {
            return await this.GetServiceEndPointFrom(this.marathonServiceEndPoint, serviceName, bindingName);
        }

        private Task<int> GetBindingNameIndexAsync(string endPoint, string serviceName, string bindingName)
        {
            // TODO: Implement this correctly?
            // var httpClient = new HttpClient();
            // var stringAsync = await httpClient.GetStringAsync($"{endPoint}/apps/{instanceId}/tasks");
            return Task.FromResult(0);
        }

        private async Task<string> GetServiceEndPointFrom(
            string marathonEndPoint,
            string serviceName,
            string bindingName = "")
        {
            if (string.IsNullOrWhiteSpace(marathonEndPoint))
            {
                throw new ArgumentException(nameof(marathonEndPoint));
            }

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException(nameof(serviceName));
            }

            // TODO: Remove this if is possible.
            if (!serviceName.StartsWith('/'))
            {
                serviceName = "/" + serviceName.Trim();
            }

            Log.Debug("Getting service '{serviceName}' end point...", serviceName);

            var serviceTasks = await this.GetTasksAsync(marathonEndPoint, string.Empty, serviceName);
            var serviceTask = serviceTasks.FirstOrDefault();

            // TODO: Improve this later
            var idx = await this.GetBindingNameIndexAsync(marathonEndPoint, serviceName, bindingName);

            if (serviceTask != null && idx < serviceTask.Ports.Count)
            {
                return $"{serviceTask.Host}:{serviceTask.Ports[idx]}";
            }

            return string.Empty;
        }

        /// <summary>
        ///     Gets task async.
        /// </summary>
        /// <param name="endPoint">
        ///     The marathon end point.
        /// </param>
        /// <param name="groupId">
        ///     The group servicesName.
        /// </param>
        /// <param name="instanceId">
        ///     The instance servicesName.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task<List<ServiceTask>> GetTasksAsync(string endPoint, string groupId, string instanceId)
        {
            var httpClient = new HttpClient();
            ServiceTasks serviceTasks;
            if (!string.IsNullOrWhiteSpace(groupId))
            {
                serviceTasks =
                    await httpClient.GetFromJsonAsync<ServiceTasks>($"{endPoint}/apps/{groupId}/{instanceId}/tasks");
            }
            else
            {
                serviceTasks = await httpClient.GetFromJsonAsync<ServiceTasks>($"{endPoint}/apps/{instanceId}/tasks");
            }

            return serviceTasks.Tasks;
        }
    }
}