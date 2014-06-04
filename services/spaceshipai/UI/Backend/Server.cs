using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SpaceshipAI.UI.Frontend;

namespace SpaceshipAI.UI.Backend
{
    public class Server
    {
        private readonly int _port;
        private readonly TcpListener _server;

        public Server(int port)
        {
            _port = port;
            _server = new TcpListener(IPAddress.Any, _port);
        }

        public static int ConnectedUsers { get; private set; }

        public void Start()
        {
            BackendScreen.WriteHint("Listening @ " + _port + "...");
            _server.Start();


            WaitForClients();
        }

        private void WaitForClients()
        {
            while (true)
            {
                TcpClient newClient = _server.AcceptTcpClient();

                var t = new Thread(HandleClient);
                t.Start(newClient);
            }
        }

        private static void HandleClient(object obj)
        {
            var client = (TcpClient) obj;
            lock (typeof (Server))
            {
                ConnectedUsers++;
            }

            try
            {
                IPAddress ip = IPAddress.Parse(((
                    IPEndPoint) client.Client.RemoteEndPoint).Address.ToString());
                var writer = new StreamWriter(client.GetStream(), Encoding.ASCII);
                var reader = new StreamReader(client.GetStream(), Encoding.ASCII);
                BackendScreen.WriteHint(ip + " connected.");
                try
                {
                    var ws = new WelcomeScreen(reader, writer);
                    ws.Start();
                }
                catch (IOException)
                {
                    BackendScreen.WriteHint(ip + " disconnected.");
                }
                catch (Exception e)
                {
                    BackendScreen.WriteError("Unhandled " + e.GetType().Name + " occured while executing the UI: " +
                                             e.Message);
                }
                finally
                {
                    lock (typeof (Server))
                    {
                        ConnectedUsers--;
                    }
                    BackendScreen.WriteHint(ip + " disconnected.");
                }

                client.Close();
            }
            catch (Exception e)
            {
                BackendScreen.WriteError("Unhandled " + e.GetType().Name +
                                         " occured while executing handling connection: " + e.Message);
            }
        }
    }
}