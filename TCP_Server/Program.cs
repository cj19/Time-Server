using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_Server
{
    class Program
    {
        private static byte[] _buffer = new byte[1024];

        // clients
        private static List<Socket> _clientSockets = new List<Socket>();

        // Server socket global
        private static Socket _serverSocket =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        public static void Main(string[] args)
        {
            Console.Title = "Time Server";
            Console.WriteLine("Time Server!");
            SetupServer();
            Console.ReadLine();
        }

        // Server instantiation
        private static void SetupServer()
        {
            Console.WriteLine("Setting up server....");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 1024));
            _serverSocket.Listen(5); //how many clients available
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        // Allow us to accept more than one connection
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSockets.Add(socket);
            Console.WriteLine("Client connected!");
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void ReceiveCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket) AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] dataBuffer = new byte[received];
            Array.Copy(_buffer, dataBuffer, received);

            string text = Encoding.ASCII.GetString(dataBuffer);
            Console.WriteLine("Text Received: "+text);

            string response = string.Empty;
            if (text.ToLower() == "get time")
            {
                response = DateTime.Now.ToLongTimeString();
            }
            else
            {
                response = "Invalid Request!";
            }
            byte[] data = Encoding.ASCII.GetBytes(response);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        
        private static void SendCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket) AR.AsyncState;
            socket.EndSend(AR);
        }
    }
}