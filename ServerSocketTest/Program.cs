using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace SocketConsole
{
   class Program
   {
      static void Main()
      {
         bool exits = false;
         Console.WriteLine("Starting: Creating Socket object");
         Socket listener = new Socket(AddressFamily.InterNetwork,
         SocketType.Stream,
         ProtocolType.Tcp);
         listener.Bind(new IPEndPoint(IPAddress.Any, 2112));
         listener.Listen(10);
         while (true)
         {
            Console.WriteLine("Waiting for connection on port 2112");
            Socket socket = listener.Accept();
            string receivedValue = string.Empty;
            while (true)
            {
               byte[] receivedBytes = new byte[1024];
               int numBytes = socket.Receive(receivedBytes);
               Console.WriteLine("Receiving ...");
               receivedValue += Encoding.ASCII.GetString(receivedBytes,
                  0, numBytes);
               if (receivedValue.IndexOf("[SHUTDOWN]") > -1)
               {
                  exits = true;
                  break;
               }
               else if (receivedValue.IndexOf("[FINAL]") > -1)
               {
                  break;
               }
            }
            Console.WriteLine("Received value: {0}", receivedValue);
            string replyValue = "Message successfully received.";
            byte[] replyMessage = Encoding.ASCII.GetBytes(replyValue);
            socket.Send(replyMessage);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            if (exits) break;
         }
         listener.Close();
         Console.WriteLine("The server is shutting down... Press a key.");
         Console.ReadKey(false);
      }
   }
}