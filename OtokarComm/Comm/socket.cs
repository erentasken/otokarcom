using OtokarComm.Model;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace OtokarComm.Comm
{
    public class SocketManager
    {
        private Socket? socket;
        private string _endpoint;
        private int _port;
        private int _connections;

        public List<Device> devices = new List<Device>();

        private readonly object _deviceLock = new object();

        public SocketManager(string endpoint, int port, int connections)
        {
            this._endpoint = endpoint;
            this._port = port;
            this._connections = connections;

             if (!establishConn())
            {
                Console.WriteLine("Failed to establish connection.");
            }else
            {
                Console.WriteLine("Connection established.");
            }

            Task.Run(() => listenClient());
        }

        private void listenClient()
        {
            Console.WriteLine("Listening on {0}:{1}", this._endpoint, this._port);

            while (true)
            {

                if( this.socket == null)
                {
                    Console.WriteLine("Socket is not bound.");
                    return;
                }

                Socket client = this.socket.Accept();

                lock (this._deviceLock)
                {
                    var device = new Device(
                        client.RemoteEndPoint.ToString(),
                        "00:00:00:00:00",
                        client,
                        true
                    );

                    devices.Add(device);
                }

                client.Send(Encoding.ASCII.GetBytes("Hello from server"));


                Console.WriteLine("Client connected -> {0} ", client.RemoteEndPoint.ToString());

                Task.Run(() => handleData(client));
            }
        }
        private bool establishConn()
        {
            IPAddress parsedIP = IPAddress.Parse(this._endpoint);
            IPEndPoint localEndPoint = new IPEndPoint(parsedIP, this._port);

            this.socket = new Socket(parsedIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                this.socket.Bind(localEndPoint);
                this.socket.Listen(this._connections);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        private void handleData(Socket clientSocket)
        {
            while (true)
            {
                try
                {
                    byte[] bytes = new Byte[1024];

                    string data = null;

                    int numByte = clientSocket.Receive(bytes);

                    data += Encoding.ASCII.GetString(bytes, 0, numByte);

                    Console.WriteLine("Data received: {0} from {1}", data, clientSocket.RemoteEndPoint.ToString());
                }
                catch (Exception e)
                {
                    if (devices != null)
                    {
                        devices.Where(x => x.socket == clientSocket).FirstOrDefault().isActive = false;
                    }else
                    {
                        Console.WriteLine(e.ToString());
                    }

                    Console.WriteLine("Client disconnected.");
                    return;
                }
            }
        }

        public void Broadcast(string data)
        {
            if (this.socket == null)
            {
                Console.WriteLine("Socket is not bound.");
                return;
            }

            byte[] msg = Encoding.ASCII.GetBytes(data);

            foreach (Device device in devices)
            {
                if (device.isActive)
                {
                    Console.WriteLine("BROADCAST ==> Message sent. Message: {0} | ReceivedBy: {1}", msg, device.EndPoint);
                    device.socket.Send(msg);
                }
            }
        }

        public void Multicast(MulticastDTO multicast)
        {
            if (this.socket == null)
            {
                Console.WriteLine("Socket is not bound.");
                return;
            }

            if (multicast== null)
            {
                Console.WriteLine("IP list is empty.");
                return;
            }

            byte[] msg = Encoding.ASCII.GetBytes(multicast._message);


            foreach (Device device in devices)
            {
                if (multicast._devices.Contains(device))
                {
                    if (device.isActive == true)
                    {
                        Console.WriteLine("MULTICAST ==> Message sent. Message: {0} | ReceivedBy: {1}", msg,device.EndPoint);
                        device.socket.Send(msg);
                    }

                }
            }

        }

    }
}
