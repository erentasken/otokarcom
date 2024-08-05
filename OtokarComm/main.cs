using OtokarComm.Comm;
using OtokarComm.Model;
using System.Net;
using System.Net.Sockets;
using System.Text;

const int PORT = 1111;
const string IP = "192.168.97.227";

try
{
    // Start the mock message sender thread

    Task.Run(() =>
    {
        for (int i = 0; i < 15; i++)
        {
            Task.Run(() => SendMockMessages());
        }
    });

    SocketManager listenModule = new SocketManager(IP, PORT);

    while (true)
    {
        Thread.Sleep(3000);
        test(listenModule);

    }

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
    int time = 3;

    var devices = listenModule._devices;

    Console.WriteLine("Message publishing has initialized...");

    while (time > 0)
    {
        Console.WriteLine("time remaining: {0}", time--);
        Thread.Sleep(1000);
    }


    Console.WriteLine("Multicast");
    if (devices.Count < 5)
    {
        Console.WriteLine("Not enough devices to multicast.");
        return;
    }
    MulticastDTO multcast = new MulticastDTO(devices.GetRange(2, 3), "ITS ME MULTICAST<EOF>");
    listenModule.Multicast(multcast);

    Thread.Sleep(3000);


    Console.WriteLine("Broadcast");
    listenModule.Broadcast("Broadcast message<EOF>");

}

// DEVICE SIMULATION 
static void SendMockMessages()
{
    Random random = new Random();

    int randomInt = (random.Next() % 3 + 1) * 1000;

    Thread.Sleep(randomInt);

    try
    {
        IPAddress parsedIP = IPAddress.Parse(IP);
        IPEndPoint localEndPoint = new IPEndPoint(parsedIP, PORT);
        using Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint remoteEP = new IPEndPoint(parsedIP, PORT);


        sender.Connect(remoteEP);
        /*
                if (sender.Connected)
                {
                    string message = "testMsg<EOF>";
                    byte[] msg = Encoding.ASCII.GetBytes(message);
                    sender.Send(msg);
                }*/

        while (true)
        {

        }

        /*while (true)
        {
            randomInt = (random.Next() % 15 + 1) * 1000;
            Thread.Sleep(randomInt);
            sender.Shutdown(SocketShutdown.Both);
            sender.Disconnect(true);
            return;
        }*/
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }
}
