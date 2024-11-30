﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RoutingServer.ProxyServiceReference
{


    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "ProxyServiceReference.IProxyService")]
    public interface IProxyService
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IProxyService/GetAllContracts", ReplyAction = "http://tempuri.org/IProxyService/GetAllContractsResponse")]
        ProxyCache.Models.Contract[] GetAllContracts();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IProxyService/GetAllContracts", ReplyAction = "http://tempuri.org/IProxyService/GetAllContractsResponse")]
        System.Threading.Tasks.Task<ProxyCache.Models.Contract[]> GetAllContractsAsync();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IProxyService/GetContractStations", ReplyAction = "http://tempuri.org/IProxyService/GetContractStationsResponse")]
        ProxyCache.Models.Station[] GetContractStations(string contractName, int pageNumber);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IProxyService/GetContractStations", ReplyAction = "http://tempuri.org/IProxyService/GetContractStationsResponse")]
        System.Threading.Tasks.Task<ProxyCache.Models.Station[]> GetContractStationsAsync(string contractName, int pageNumber);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IProxyService/ResolveAddress", ReplyAction = "http://tempuri.org/IProxyService/ResolveAddressResponse")]
        ProxyCache.Models.Position ResolveAddress(string address);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IProxyService/ResolveAddress", ReplyAction = "http://tempuri.org/IProxyService/ResolveAddressResponse")]
        System.Threading.Tasks.Task<ProxyCache.Models.Position> ResolveAddressAsync(string address);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IProxyService/GetItinerary", ReplyAction = "http://tempuri.org/IProxyService/GetItineraryResponse")]
        ProxyCache.Models.Itinerary GetItinerary(ProxyCache.Models.Position startPosition, ProxyCache.Models.Position endPosition, string profile);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IProxyService/GetItinerary", ReplyAction = "http://tempuri.org/IProxyService/GetItineraryResponse")]
        System.Threading.Tasks.Task<ProxyCache.Models.Itinerary> GetItineraryAsync(ProxyCache.Models.Position startPosition, ProxyCache.Models.Position endPosition, string profile);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IProxyServiceChannel : RoutingServer.ProxyServiceReference.IProxyService, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ProxyServiceClient : System.ServiceModel.ClientBase<RoutingServer.ProxyServiceReference.IProxyService>, RoutingServer.ProxyServiceReference.IProxyService
    {

        public ProxyServiceClient()
        {
        }

        public ProxyServiceClient(string endpointConfigurationName) :
                base(endpointConfigurationName)
        {
        }

        public ProxyServiceClient(string endpointConfigurationName, string remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public ProxyServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public ProxyServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
        {
        }

        public ProxyCache.Models.Contract[] GetAllContracts()
        {
            return base.Channel.GetAllContracts();
        }

        public System.Threading.Tasks.Task<ProxyCache.Models.Contract[]> GetAllContractsAsync()
        {
            return base.Channel.GetAllContractsAsync();
        }

        public ProxyCache.Models.Station[] GetContractStations(string contractName, int pageNumber)
        {
            return base.Channel.GetContractStations(contractName, pageNumber);
        }

        public System.Threading.Tasks.Task<ProxyCache.Models.Station[]> GetContractStationsAsync(string contractName, int pageNumber)
        {
            return base.Channel.GetContractStationsAsync(contractName, pageNumber);
        }

        public ProxyCache.Models.Position ResolveAddress(string address)
        {
            return base.Channel.ResolveAddress(address);
        }

        public System.Threading.Tasks.Task<ProxyCache.Models.Position> ResolveAddressAsync(string address)
        {
            return base.Channel.ResolveAddressAsync(address);
        }

        public ProxyCache.Models.Itinerary GetItinerary(ProxyCache.Models.Position startPosition, ProxyCache.Models.Position endPosition, string profile)
        {
            return base.Channel.GetItinerary(startPosition, endPosition, profile);
        }

        public System.Threading.Tasks.Task<ProxyCache.Models.Itinerary> GetItineraryAsync(ProxyCache.Models.Position startPosition, ProxyCache.Models.Position endPosition, string profile)
        {
            return base.Channel.GetItineraryAsync(startPosition, endPosition, profile);
        }
    }
}
