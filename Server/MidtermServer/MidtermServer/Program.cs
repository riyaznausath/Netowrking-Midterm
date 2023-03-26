using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MidermServer
{
    class Program
    {
        private static byte[] buffer = new byte[512];

        //socket for 2clients
        private static Socket client1TCP;
        private static Socket client2TCP;
        private static List<Socket> clientSockets = new List<Socket>(); //store sockets 4 client

        private static byte[] outBuffer = new byte[512];

        //checkers for udp oorts
        public static bool UDPClient1 = false;
        public static bool UDPClient2 = false;
        public static EndPoint client1Endpoint;
        public static EndPoint client2Endpoint;
        public static int currentClient; //used to see which client is currently sending message
        public static int clientId; //stores client id of each client logged in, respectivelyu
        static void Main(string[] args)
        {

            client1TCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client2TCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress serverIP = IPAddress.Parse("127.0.0.1");
            //IPAddress serverIP = IPAddress.Parse("192.168.2.113");

            //listened to each port for logins and accepts them
            client1TCP.Bind(new IPEndPoint(serverIP, 8888));
            client1TCP.Listen(5);
            client1TCP.BeginAccept(new AsyncCallback(AcceptCallback), null);

            client2TCP.Bind(new IPEndPoint(serverIP, 8889));
            client2TCP.Listen(5);
            client2TCP.BeginAccept(new AsyncCallback(AcceptCallback), null);


            StartServer();
        }
        public static void StartServer()
        {
            byte[] buffer = new byte[512];
            byte[] buffer2 = new byte[512];
            IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Console.WriteLine("Server name: {0}  IP: {1}", hostInfo.HostName, ip);
            IPEndPoint localEP = new IPEndPoint(ip, 8888);

            Socket server = new Socket(ip.AddressFamily,
                        SocketType.Dgram, ProtocolType.Udp);
            EndPoint remoteClient = new IPEndPoint(IPAddress.Any, 0);

            IPEndPoint localEP2 = new IPEndPoint(ip, 8889);
            Socket server2 = new Socket(ip.AddressFamily,
                        SocketType.Dgram, ProtocolType.Udp);
            EndPoint remoteClient2 = new IPEndPoint(IPAddress.Any, 0);


            try
            {
                server.Bind(localEP);
                server2.Bind(localEP2);
                //string position = remoteClient.ToString();

                Console.WriteLine("Waiting for data....");

                while (true)
                {
                    if (server.Available > 0) 
                    {
                        int recv = server.ReceiveFrom(buffer, ref remoteClient);
                        float[] pos = new float[recv / 4];
                        Buffer.BlockCopy(buffer, 0, pos, 0, recv);

                        if (UDPClient1 == false)
                        {
                            client1Endpoint = remoteClient;
                            UDPClient1 = true;
                        }

                        //Console.WriteLine("Recv from: {0}   Data: {1}", remoteClient.ToString(), Encoding.ASCII.GetString(buffer, 0, recv));
                        Console.WriteLine("Recv from:" + remoteClient.ToString() + " Data: (" + pos[0] + ", " + pos[1] + ", " + pos[2] + ")");
                        if (UDPClient2 == true)
                        {
                           // Console.WriteLine("Sent: {0}    to   {1}", Encoding.ASCII.GetString(buffer, 0, recv), client2Endpoint.ToString());
                            Console.WriteLine("Sent: (" + pos[0] + ", " + pos[1] + ", " + pos[2] + ") to " + client2Endpoint.ToString());

                        }


                        string newPos = "Client 1: " + Encoding.ASCII.GetString(buffer, 0, recv);
                        //byte[] outBuffer = Encoding.ASCII.GetBytes(newPos);
                        byte[] outBuffer = new byte[pos.Length * 4];
                        Buffer.BlockCopy(pos, 0, outBuffer, 0, outBuffer.Length);
                        //server.SendTo(outBuffer, remoteClient); //send message back to the owner.
                        if (UDPClient2 == true)
                        {
                            server.SendTo(outBuffer, client2Endpoint); //send message to other client
                        }

                    }

                    if (server2.Available > 0) 
                    {
                        int recv2 = server2.ReceiveFrom(buffer2, ref remoteClient2);
                        float[] pos2 = new float[recv2 / 4];
                        Buffer.BlockCopy(buffer2, 0, pos2, 0, recv2);

                        if (UDPClient2 == false)
                        {
                            client2Endpoint = remoteClient2;
                            UDPClient2 = true;
                        }


                        //Console.WriteLine("Recv from: " + remoteClient2.ToString() + " Data: " + pos2[0] + " " + pos2[1] + " " + pos2[2]);
                        Console.WriteLine("Recv from:" + remoteClient.ToString() + " Data: (" + pos2[0] + ", " + pos2[1] + ", " + pos2[2] + ")");
                        if (UDPClient1 == true)
                        {

                           // Console.WriteLine("Sent: {0}    to   {1}", Encoding.ASCII.GetString(buffer, 0, recv2), client1Endpoint.ToString());
                            Console.WriteLine("Sent: (" + pos2[0] + ", " + pos2[1] + ", " + pos2[2] + ") to " + client2Endpoint.ToString());

                        }


                        string newPos = "Client 2: " + pos2;
                        string client2Positon = "Client 2: " + pos2;

                        byte[] outBuffer2 = new byte[pos2.Length * 4];
                        Buffer.BlockCopy(pos2, 0, outBuffer2, 0, outBuffer2.Length);
                        if (UDPClient1 == true)
                        {
                            server2.SendTo(outBuffer2, client1Endpoint); 
                        }

                    }

                }

            }


            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
       

        private static void AcceptCallback(IAsyncResult result)
        {
            Socket client = client1TCP.EndAccept(result);
            Console.WriteLine("Client IP:" + client.RemoteEndPoint.ToString() + " Connected");

            clientSockets.Add(client);

            client.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), client);
        }

        private static void AcceptCallback2(IAsyncResult result2)
        {
            Socket client2 = client2TCP.EndAccept(result2);
            Console.WriteLine("Client IP:" + client2.RemoteEndPoint.ToString() + " Connected");

            clientSockets.Add(client2);

            client2.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), client2);
        }

        private static void ReceiveCallback(IAsyncResult results)
        {
            Socket socket = (Socket)results.AsyncState;
            int rec = socket.EndReceive(results);
            byte[] data = new byte[rec];
            Array.Copy(buffer, data, rec);



            string msg = Encoding.ASCII.GetString(data);
            for (int i = 0; i < clientSockets.Count; i++)
            {

                if (socket == clientSockets[i])
                {
                    clientId = i + 1;
                }
            }


            //checks which client is sending the msg
            if (clientId == 1)
            {
                currentClient = 1;
            }
            else if(clientId == 0)
            {
                currentClient = 0;
            }

            //message chatbox system
            if (msg != "quit")
            {
                Console.WriteLine("Recv message: " + msg + " from: " + socket.RemoteEndPoint.ToString());
                if (clientSockets.Count > 1)
                {
                    Console.WriteLine("Sent message: " + msg + " to " + clientSockets[currentClient].RemoteEndPoint.ToString());


                }

                string clientMsg = "Client #" + clientId + ":" + msg;
                byte[] clientMsgData = Encoding.ASCII.GetBytes(clientMsg);

                foreach (Socket sockets in clientSockets)
                {

                    sockets.BeginSend(clientMsgData,   0, clientMsgData.Length,   0,  new AsyncCallback(SendCallback),  sockets);


                }

                socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), socket);


            }
            else
            {

                Console.WriteLine(socket.RemoteEndPoint.ToString() + " has disconected!");
                clientSockets[clientId - 1].Close();

                clientSockets.RemoveAt(clientId - 1);


            }

        }



        private static void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);


        }




        public static void SendMessageToClient(Socket client, string msg)
        {

            byte[] data = Encoding.ASCII.GetBytes(msg); 
            client.BeginSend(data,  0, data.Length,  0, new AsyncCallback(SendCallback),  client);
             
        }
    }
}
