using System;
using System.Text;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace WebServer
{
    class Interaction
    {
        private TcpClient client;
        private string directory;
        private string mainpage;
        private Hashtable content;

        public Interaction(TcpClient client_, string directory_, string mainpage_)
        {
            client = client_;
            directory = directory_;
            mainpage = mainpage_;
            content = new Hashtable();

            Content();

            Thread interact = new Thread(new ThreadStart(Interact));
            interact.Start();
        }

        private void Content()
        {
            content.Add("", "application/unknown");
            content.Add(".htm", "text/html");
            content.Add(".html", "text/html");
            content.Add(".txt", "text/plain");
            content.Add(".jpg", "image/jpeg");
            content.Add(".gif", "image/gif");
        }

        private void Error(TextWriter output, string message)
        {
            string prefix = "[" + DateTime.Now.ToString("G") + "]" + " " + 
                            "from (" + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + ")\nError:\n";

            output.Write(prefix + message);
        }

        private string GetContent(string path)
        {
            string extension = "";
            int dot_pos = 0;

            if ((dot_pos = path.LastIndexOf(".")) >= 0)
                extension = path.Substring(dot_pos);

            return (string)content[extension];
        }

        private string GetPath(string request)
        {
            int[] space = new int[2];
            space[0] = request.IndexOf(" ");
            space[1] = request.IndexOf(" ", space[0] + 1);

            string path = request.Substring(space[0] + 2, space[1] - space[0] - 2);

            if (path == "")
                path = mainpage;

            return directory + "\\" + path;
        }

        private void SendHeader(int status, string status_msg, string content_type, Int64 len)
        {
            string head = "HTTP/1.1 " + status.ToString() + " " + status_msg + "\n" +
                          "Server: nginx/1.2.1\n" +
                          "Date: " + DateTime.Now.ToString("R") + "\n" +
                          "Content-type: " + content_type + "\n" +
                          "Content-length: " + len.ToString() + "\n" +
                          "Connection: keep-alive\n" +
                          "Accept-Ranges: bytes\n\n";

            client.GetStream().Write(Encoding.ASCII.GetBytes(head), 0, head.Length);
        }

        private void AnswerRequest(string request)
        {
            string path = GetPath(request);

            FileStream input = null;

            try
            {
                input = new FileStream(path, FileMode.Open);
            }

            catch (Exception except)
            {
                Error(Console.Out, "An exception while answering the request:\n" + except.ToString() + "\n\n");
                return;
            }

            byte[] buffer = new byte[2048];
            int len = 0;

            SendHeader(200, "OK", GetContent(path), input.Length);

            while ((len = input.Read(buffer, 0, 2048)) != 0)
                client.GetStream().Write(buffer, 0, len);

            input.Close();
        }

        private void Interact()
        {
            try
            {
                byte[] buffer = new byte[2048];
                string request = "";

                while (true)
                {
                    int length = client.GetStream().Read(buffer, 0, 2048);
                    request += Encoding.ASCII.GetString(buffer, 0, length);

                    if (request.IndexOf("\r\n\r\n") >= 0)
                    {
                        AnswerRequest(request);
                        request = "";
                    }
                }
            }

            catch (Exception except)
            {
                Error(Console.Out, "An exception while interaction\n" + except.ToString());
            }
        }
    }
}
