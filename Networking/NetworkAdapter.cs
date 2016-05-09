using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Splat;

namespace Xperitos.Common.Networking
{
    public class NetworkAdapter : IEnableLogger, IDisposable
    {
        internal NetworkAdapter(ManagementObject adapter)
        {
            m_adapter = adapter;
            NetworkStatusObservable = ConnectionStatusStream
                .Where(v => Equals(v.Item1, DeviceID))
                .Select(v => v.Item2)
                .DistinctUntilChanged();
        }

        readonly CompositeDisposable m_resources = new CompositeDisposable();

        public void Dispose()
        {
            m_resources.Dispose();
        }

        /// <summary>
        /// Provides a periodic stream of connection statuses.
        /// </summary>
        /// <remarks>Tuple is a device id and a connection status</remarks>
        private static readonly IObservable<Tuple<string, NetworkConnectionStatus>> ConnectionStatusStream = Observable
            .Interval(TimeSpan.FromSeconds(5))
            .StartWith(0)
            .SelectMany(v =>
            {
                var results = new List<Tuple<string, NetworkConnectionStatus>>();
                using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapter"))
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    foreach (var obj in networkConfigs.Cast<ManagementObject>())
                        using (obj)
                        {
                            var devId = Convert.ToString(obj["DeviceID"]);
                            var status = (NetworkConnectionStatus) Convert.ToInt16(obj["NetConnectionStatus"]);
                            results.Add(new Tuple<string, NetworkConnectionStatus>(devId, status));
                        }
                }
                return results;
            })
            .Publish()
            .RefCount();

        /// <summary>
        /// Observable for connection status changes.
        /// </summary>
        public IObservable<NetworkConnectionStatus> NetworkStatusObservable { get; private set; }

        private readonly ManagementObject m_adapter;
        private ManagementObject m_configuration;

        private ManagementObject Conf
        {
            get
            {
                if (m_configuration == null)
                    m_configuration = m_adapter.GetRelated("Win32_NetworkAdapterConfiguration").Cast<ManagementObject>().First();

                return m_configuration;
            }
        }

        private ManagementObject Adapter => m_adapter;

        /// <summary>
        /// Force object to refresh values.
        /// </summary>
        public void Refresh()
        {
            // Clearing the management object will force refresh on next calls.
            m_configuration = null;
        }

        /// <summary>
        /// Obtain the host name of this computer.
        /// </summary>
        public string HostName => (string)Conf["DNSHostName"];

        /// <summary>
        /// MAC address for this computer.
        /// </summary>
        public string MACAddress => (string)Conf["MACAddress"];

        /// <summary>
        /// Is this adapeter set to use DHCP
        /// </summary>
        public bool IsDHCP => (bool)Conf["DHCPEnabled"];

        /// <summary>
        /// Obtain the static IP for this adapter (if set).
        /// </summary>
        public string StaticIP
        {
            get
            {
                var addrList = ( Conf["IPAddress"] as IEnumerable );
                if ( addrList == null )
                    return "";
                return addrList.Cast<string>().FirstOrDefault(); 
            }
        }

        /// <summary>
        /// Obtain the NetMask for this adapter (if set).
        /// </summary>
        public string StaticNetmask
        {
            get
            {
                var addrList = (Conf["IPSubnet"] as IEnumerable);
                if (addrList == null)
                    return "";
                return addrList.Cast<string>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Is this device enabled?
        /// </summary>
        public bool IPEnabled => (bool)Conf["IPEnabled"];

        /// <summary>
        /// Device ID for this adapter.
        /// </summary>
        public string DeviceID => (string)Adapter["DeviceID"];

        /// <summary>
        /// Display name for the adapter (as shown in network adapters)
        /// </summary>
        public string Name => (string)Adapter["NetConnectionID"];

        /// <summary>
        /// Obtain the Gateway for this adapter (if set).
        /// </summary>
        public string StaticGateway
        {
            get
            {
                var gws = ((string[])Conf["DefaultIPGateway"]);
                if (gws == null)
                    return null;
                if (gws.Length == 0)
                    return null;

                return gws[0];
            }
        }

        /// <summary>
        /// Update this adapeter to use DHCP.
        /// </summary>
        /// <returns></returns>
        public bool SetDHCP()
        {
            uint result = (uint)Conf.InvokeMethod("EnableDHCP", null);

            return (result == 0 || result == 1);
        }

        /// <summary>
        /// Set's a new IP Address and it's Submask of the local machine
        /// </summary>
        /// <param name="ipAddress">The IP Address</param>
        /// <param name="subnetMask">The Submask IP Address</param>
        /// <param name="gateway">The gateway.</param>
        /// <remarks>Requires a reference to the System.Management namespace</remarks>
        public bool SetIP(string ipAddress, string subnetMask, string gateway = null)
        {
            using (var newIP = Conf.GetMethodParameters("EnableStatic"))
            {
                // Set new IP address and subnet if needed
                if ((!String.IsNullOrEmpty(ipAddress)) || (!String.IsNullOrEmpty(subnetMask)))
                {
                    if (!String.IsNullOrEmpty(ipAddress))
                    {
                        newIP["IPAddress"] = new[] {ipAddress};
                    }

                    if (!String.IsNullOrEmpty(subnetMask))
                    {
                        newIP["SubnetMask"] = new[] {subnetMask};
                    }

                    var result = (uint)Conf.InvokeMethod("EnableStatic", newIP, null)["returnValue"];
                    this.Log().Debug("WMI EnableStatic: {0}", result);

                    if (result != 0 && result != 1 && result != 81)
                        return false;
                }

                // Set mew gateway if needed
                if (!String.IsNullOrWhiteSpace(gateway))
                {
                    using (var newGateway = Conf.GetMethodParameters("SetGateways"))
                    {
                        newGateway["DefaultIPGateway"] = new[] {newGateway};
                        newGateway["GatewayCostMetric"] = new[] {1};
                        var result = (uint)Conf.InvokeMethod("SetGateways", newGateway, null)["returnValue"];
                        this.Log().Debug("WMI SetGateways: {0}", result);
                        if (result != 0 && result != 1)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}