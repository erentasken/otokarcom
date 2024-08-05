using System.Net.Sockets;

namespace OtokarComm.Model
{
    public class Device
    {
        public Socket socket { get; set; }

        public string EndPoint { get; set; }

        public string Mac { get; set; }

        public Device(string endpoint, string mac, Socket socket)
        {
            this.socket = socket;

            this.EndPoint = endpoint;
            this.Mac = mac;

        }
    }
}
