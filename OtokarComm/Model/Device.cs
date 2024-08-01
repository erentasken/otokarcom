using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Net;

namespace OtokarComm.Model
{
    public class Device
    {
        public bool isActive { get; set; }
        public Socket socket { get; set; }

        public string EndPoint { get; set; }

        public string Mac { get; set; }

        public Device(string endpoint, string mac, Socket socket, bool isActive = true)
        {
            this.isActive = isActive;
            this.socket = socket;

            this.EndPoint = endpoint;
            this.Mac = mac;

        }
    }
}
