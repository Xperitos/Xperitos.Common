using System;
using System.Linq;
using System.Management;
using System.Collections.Generic;
using Serilog;

namespace Xperitos.Common.Networking
{
    /// <summary>
    /// Helper class to set networking configuration like IP address, DNS servers, etc.
    /// </summary>
    public static class NetworkUtils
    {
        /// <summary>
        /// Update the computer name.
        /// </summary>
        /// <param name="newName">New name</param>
        /// <returns>Success to update the name</returns>
        public static bool SetComputerName(string newName)
        {
            using (var computerSystemMng = new ManagementClass("Win32_ComputerSystem"))
            {
                var computerSystem = computerSystemMng.GetInstances().Cast<ManagementObject>().First();
                using (var renameParams = computerSystemMng.GetMethodParameters("Rename"))
                {
                    renameParams["Name"] = newName;
                    renameParams["Password"] = null;
                    renameParams["UserName"] = null;
                    var result = (uint)computerSystem.InvokeMethod("Rename", renameParams, null)["returnValue"];
                    Log.Debug("WMI Rename: {0}", result);

                    return result == 0;
                }
            }
        }

        /// <summary>
        /// Gets a list of all network adapters.
        /// </summary>
        /// <param name="predicate">Filter function when requesting the list</param>
        /// <returns></returns>
        public static NetworkAdapter[] GetNetworkAdapters(Func<NetworkAdapter, bool> predicate = null)
        {
            NetworkAdapter[] adapters;
            try
            {
                using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapter"))
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    var netAdapters =
                        networkConfigs.Cast<ManagementObject>()
                            .Where(o => (Convert.ToInt16(o["AdapterTypeID"])) == 0 && o["NetConnectionID"] != null)
                            .Select((m) => new NetworkAdapter(m));

                    if (predicate == null)
                        adapters = netAdapters.ToArray();
                    else
                        adapters = netAdapters
                            .Where(predicate)
                            .ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to get list of adapters");
                adapters = new NetworkAdapter[0];
            }

            return adapters;
        }

        /// <summary>
        /// Gets a list of all CoWan network adapters.
        /// </summary>
        /// <param name="predicate">Filter function when requesting the list</param>
        /// <returns></returns>
        public static NetworkAdapter[] GetCoWanNetworkAdapters(Func<NetworkAdapter, bool> predicate = null)
        {
            NetworkAdapter[] adapters;
            try
            { 
                var adapterTypeIDs = new List<EAdapterTypeID>();
                adapterTypeIDs.Add(EAdapterTypeID.CoWan);

                using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapter"))
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    var netAdapters = 
                        networkConfigs.Cast<ManagementObject>()
                            .Where(o => o["Caption"] != null && o["AdapterTypeID"] != null && adapterTypeIDs.Contains(((EAdapterTypeID)(Convert.ToInt16(o["AdapterTypeID"])))) )
                            .Select((m) => new NetworkAdapter(m));

                    if ( predicate == null )
                        adapters = netAdapters.ToArray();
                    else
                        adapters = netAdapters
                            .Where(predicate)
                            .ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to get list of adapters");
                adapters = new NetworkAdapter[0];
            }

            return adapters;
        }
    }
}
