﻿using JMS.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using JMS.Impls;
using JMS.Dtos;

namespace JMS
{
    class Gateway
    {
        public string Id { get; }
        TcpListener _tcpListener;
        ILogger<Gateway> _Logger;
        IRequestReception _requestReception;
        internal IServiceProvider ServiceProvider { get; set; }
        public List<IMicroServiceReception> OnlineMicroServices { get; set; }


        public Gateway(ILogger<Gateway> logger)
        {
            this.Id = Guid.NewGuid().ToString("N");
            _Logger = logger;
            OnlineMicroServices = new List<IMicroServiceReception>();
        }

        public RegisterServiceInfo GetServiceById(int id)
        {
            try
            {
                for (int i = 0; i < OnlineMicroServices.Count; i++)
                {
                    if (OnlineMicroServices[i].ServiceInfo.ServiceId == id)
                        return OnlineMicroServices[i].ServiceInfo;
                }
            }
            catch
            {
                
            }
            return null;
        }
        public void Run(int port)
        {
            _requestReception = ServiceProvider.GetService<IRequestReception>();
               _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _Logger?.LogInformation("Gateway started, port:{0}", port);
            while (true)
            {
                try
                {
                    var socket = _tcpListener.AcceptSocket();
                    Task.Run(() => _requestReception.Interview(socket));
                }
                catch (Exception ex)
                {
                    _Logger?.LogError(ex, ex.Message);
                    break;
                }
               
            }
        }

        public RegisterServiceInfo[] GetAllServiceProviders()
        {
            List<RegisterServiceInfo> ret = new List<RegisterServiceInfo>();
            for(int i = 0; i < OnlineMicroServices.Count; i ++)
            {
                var client = OnlineMicroServices[i];
                if(client != null && ret.Contains(client.ServiceInfo) == false)
                {
                    ret.Add(client.ServiceInfo);
                }
            }
            return ret.ToArray();
        }
    }
}