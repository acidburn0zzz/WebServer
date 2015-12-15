using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace WebServer
{
    class Server
    {
        private TcpListener listener;

        public Server(int port)
        {
            listener = new TcpListener(port);
            listener.Start();
            Listen(Console.Out);
        }

        private void Listen(TextWriter output)
        {
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                output.WriteLine("[" + DateTime.Now.ToString("G") + "] Connected " +
                                 ((IPEndPoint)(client.Client.RemoteEndPoint)).Address.ToString());

                new Interaction(client, Environment.CurrentDirectory, "index.html");
            }
        }

        ~Server()
        {
            listener.Stop();
        }
    }
}
