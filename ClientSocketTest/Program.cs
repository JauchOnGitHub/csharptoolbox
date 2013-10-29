using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace SocketConsoleClient
{
   class Program
   {
      static void Main()
      {
         byte[] receivedBytes = new byte[1024];
         IPHostEntry ipHost = Dns.GetHostEntry("mohidland.maretec.ist.utl.pt");
         IPAddress ipAddress = ipHost.AddressList[0];
         IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 2112);
         Console.WriteLine("Starting: Creating Socket object");
         Socket sender = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
         sender.Connect(ipEndPoint);
         Console.WriteLine("Successfully connected to {0}",
         sender.RemoteEndPoint);
         string sendingMessage = "Hello World Socket Test";
         Console.WriteLine("Creating message: Hello World Socket Test");
         byte[] forwardMessage = Encoding.ASCII.GetBytes(sendingMessage
            + "[FINAL]");
         sender.Send(forwardMessage);
         int totalBytesReceived = sender.Receive(receivedBytes);
         Console.WriteLine("Message provided from server: {0}",
            Encoding.ASCII.GetString(receivedBytes,
            0, totalBytesReceived));
         sender.Shutdown(SocketShutdown.Both);
         sender.Close();
         string r = Console.ReadLine();
         if (r.Contains("end"))
         {
            ipHost = Dns.GetHostEntry("mohidland.maretec.ist.utl.pt");
            ipAddress = ipHost.AddressList[0];
            ipEndPoint = new IPEndPoint(ipAddress, 2112);
            Console.WriteLine("Starting: Creating Socket object");
            sender = new Socket(AddressFamily.InterNetwork,
               SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(ipEndPoint);
            Console.WriteLine("Successfully connected to {0}",
            sender.RemoteEndPoint);
            sendingMessage = "Hello World Socket Test";
            Console.WriteLine("Creating message: Shutting down the server...");
            forwardMessage = Encoding.ASCII.GetBytes(sendingMessage
               + "[SHUTDOWN]");
            sender.Send(forwardMessage);
            totalBytesReceived = sender.Receive(receivedBytes);
            Console.WriteLine("Message provided from server: {0}",
               Encoding.ASCII.GetString(receivedBytes,
               0, totalBytesReceived));
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
         }
      }
   }
}