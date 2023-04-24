using System.Net;
using System.Net.NetworkInformation;


class wrs_network
{

    public struct TcpConnectionInfo
    {
        public string? LocalEndPoint { get; set; }
        public int? LocalEndPointPort { get; set; }
        public string? RemoteEndPoint { get; set; }
        public int? RemoteEndPointPort { get; set; }

        public TcpConnectionInfo(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            LocalEndPoint = localEndPoint.Address.ToString();
            LocalEndPointPort = localEndPoint.Port;
            RemoteEndPoint = remoteEndPoint.Address.ToString();
            RemoteEndPointPort = remoteEndPoint.Port;
        }
    }

    public static void DisplayNetworksInfo()
    {
        List<TcpConnectionInfo> networks = GetTcpConnectionsInfo();

        foreach (TcpConnectionInfo network in networks)
        {
            Console.WriteLine("----Network status----");
            Console.WriteLine($"LocalEndPoint: {network.LocalEndPoint}");
            Console.WriteLine($"LocalEndPointPort: {network.LocalEndPointPort}");
            Console.WriteLine($"RemoteEndPoint: {network.RemoteEndPoint}");
            Console.WriteLine($"RemoteEndPointPort: {network.RemoteEndPointPort}");
            Console.WriteLine();
        }
    }

    public static List<TcpConnectionInfo> GetTcpConnectionsInfo()
    {
        List<TcpConnectionInfo> list = new List<TcpConnectionInfo>();

        try
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation connection in connections)
            {
                list.Add(new TcpConnectionInfo(connection.LocalEndPoint, connection.RemoteEndPoint));
            }
        }
        catch (Exception)
        {
            Console.Error.WriteLine("Failed to query network info.");
        }

        return list;
    }
}