using OtokarComm.Comm;
using OtokarComm.Model;
using System.Net;
using System.Net.Sockets;
using System.Text;

const int PORT = 1111;
const string IP = "192.168.43.17";
const int CONNECTIONS = 10;

try
{
    // Start the mock message sender thread
    
    for (int i = 0; i < 5; i++)
    {
        Task.Run(() => SendMockMessages());
    }

    SocketManager listenModule = new SocketManager(IP, PORT, CONNECTIONS);

    test(listenModule);
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}

while (true)
{
    Thread.Sleep(1000);
}

static void test(SocketManager listenModule)
{
    int time = 10;

    var devices = listenModule.devices;

    Console.WriteLine("Waiting for devices to connect...");

    while (time > 0)
    {
        Console.WriteLine("time remaining: {0}", time--);
        Thread.Sleep(1000);
    }

    Console.WriteLine("Broadcast is starting...");
    Thread.Sleep(2000);
    // Broadcast test
    listenModule.Broadcast("Broadcast message<EOF>");

    Console.WriteLine("Multicast is starting...");
    Thread.Sleep(2000);
    if (devices.Count < 5)
    {
        Console.WriteLine("Not enough devices to multicast.");
        return;
    }
    // Multicast test
    MulticastDTO multcast = new MulticastDTO(devices.GetRange(2, 3), "slmITS ME<EOF>");
    listenModule.Multicast(multcast);

    
}

// DEVICE SIMULATION 
static void SendMockMessages()
{

    Random random = new Random();

    int randomInt = (random.Next() % 15 + 1) * 1000;

    Thread.Sleep(randomInt);

    try
    {
        IPAddress parsedIP = IPAddress.Parse(IP);
        IPEndPoint localEndPoint = new IPEndPoint(parsedIP, PORT);
        using Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint remoteEP = new IPEndPoint(parsedIP, PORT);


        sender.Connect(remoteEP);

        if (sender.Connected)
        {
            string message = "slm<EOF>";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            sender.Send(msg);
        }

        while ( true )
        {

        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }
}
