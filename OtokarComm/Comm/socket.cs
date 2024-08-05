
using OtokarComm.Model;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OtokarComm.Comm
{
    public class SocketManager
    {
        private Socket? _socket;
        private readonly string _endpoint;
        private readonly int _port;

        public readonly List<Device> _devices = new List<Device>();
        private readonly object _deviceLock = new object();

        public SocketManager(string endpoint, int port)
        {
            _endpoint = endpoint;
            _port = port;

            if (EstablishConnection())
            {
                Console.WriteLine("Connection established.");
                Task.Run(() => ListenForClients());
            }
            else
            {
                Console.WriteLine("Failed to establish connection.");
            }
        }

        private bool EstablishConnection()
        {
            try
            {
                var parsedIP = IPAddress.Parse(_endpoint);
                var localEndPoint = new IPEndPoint(parsedIP, _port);

                _socket = new Socket(parsedIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _socket.Bind(localEndPoint);
                _socket.Listen(10);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error establishing connection: {ex}");
                return false;
            }
        }

        private void ListenForClients()
        {
            Console.WriteLine($"Listening on {_endpoint}:{_port}");


            while (true)
            {
                try
                {
                    if (_socket == null)
                    {
                        Console.WriteLine("Socket is not bound.");
                        return;
                    }

                    var clientSocket = _socket.Accept();


                    lock (_deviceLock)
                    {
                        var device = new Device(clientSocket.RemoteEndPoint?.ToString() ?? "Unknown", "00:00:00:00:00", clientSocket);
                        _devices.Add(device);
                    }

                    Console.WriteLine($"Client connected: {clientSocket.RemoteEndPoint}");

                    Task.Run(() => HandleClientData(clientSocket));
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Socket exception: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected exception: {ex.Message}");
                }
            }
        }

        private void HandleClientData(Socket clientSocket)
        {
            while ( true )
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes = clientSocket.Receive(buffer);

                    if (receivedBytes > 0)
                    {
                        var receivedData = Encoding.ASCII.GetString(buffer, 0, receivedBytes);
                        Console.WriteLine($"Data received: {receivedData} from {clientSocket.RemoteEndPoint}");
                    }
                }
                catch (SocketException ex)
                {
                    DisconnectClient(clientSocket);
                }
            }
            
            

        }

        private void DisconnectClient(Socket clientSocket)
        {

            string endPoint = clientSocket.RemoteEndPoint.ToString();
            lock (_deviceLock)
            {
                var device = _devices.FirstOrDefault(d => d.socket == clientSocket);
                if (device != null)
                {
                    _devices.Remove(device);
                }
            }

            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client disconnected already : {clientSocket.RemoteEndPoint}");
                return;
            }
            Console.WriteLine("Disconnecting client...: {0}", endPoint);


        }

        public void Broadcast(string message)
        {
            if (_socket == null)
            {
                Console.WriteLine("Socket is not bound.");
                return;
            }

            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            lock (_deviceLock)
            {
                foreach (var device in _devices)
                {
                    try
                    {
                        if (device.socket == null)
                        {
                            Console.WriteLine("Socket is null.");
                            continue;
                        }

                        device.socket.Send(messageBytes);

                        Console.WriteLine($"Broadcast message sent to {device.EndPoint}");
                    }
                    catch (SocketException ex)
                    {

                        Console.WriteLine($"Failed to send message to {device.EndPoint}, {ex.ErrorCode}");
                    }
                }
            }
        }

        public void Multicast(MulticastDTO multicast)
        {
            if (_socket == null)
            {
                Console.WriteLine("Socket is not bound.");
                return;
            }

            if (multicast == null || multicast._devices == null || multicast._devices.Count == 0)
            {
                Console.WriteLine("IP list is empty.");
                return;
            }

            byte[] messageBytes = Encoding.ASCII.GetBytes(multicast._message);

            lock (_deviceLock)
            {
                foreach (var device in _devices)
                {
                    if (multicast._devices.Contains(device))
                    {
                        try
                        {
                            device.socket.Send(messageBytes);
                            Console.WriteLine($"Multicast message sent to {device.EndPoint}");
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine($"Failed to send message to {device.EndPoint}");
                        }
                    }
                }
            }
        }
    }
}


