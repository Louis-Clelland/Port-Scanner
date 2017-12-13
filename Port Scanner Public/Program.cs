using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Port_Scanner_Public
{
    internal class Program
    {
        private static bool stop = false;
        private static List<int> openPorts = new List<int>();
        private static object consoleLock = new object();
        private static int startPort;
        private static int endPort;
        private static int waitingForResponses;

        private static void Main(string[] args)
        {
			IPAddress address;
            do
            {
                Console.WriteLine();
                Console.WriteLine("Enter the IP Address of the target: ");
                Console.WriteLine();
            }
            while (!IPAddress.TryParse(Console.ReadLine(), out address));
            do
            {
                Console.WriteLine();
                Console.WriteLine("Enter the starting port to start scanning on: ");
                Console.WriteLine();
            }
            while (!int.TryParse(Console.ReadLine(), out Program.startPort));
            do
            {
                Console.WriteLine();
                Console.WriteLine("Enter the end port to end scanning on: ");
                Console.WriteLine();
            }
            while (!int.TryParse(Console.ReadLine(), out Program.endPort));
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to stop scanning");
            Console.WriteLine();
            ThreadPool.QueueUserWorkItem(new WaitCallback(Program.StartScan), (object)address);
            Console.ReadKey();
            Program.stop = true;
            Console.ReadKey();
        }

        private static void StartScan(object o)
        {
            IPAddress address = o as IPAddress;
            for (int startPort = Program.startPort; startPort < Program.endPort; ++startPort)
            {
                lock (Program.consoleLock)
                {
                    int cursorTop = Console.CursorTop;
                    Console.CursorTop = 13;
                    Console.WriteLine("Scanning port: {0}     ", (object)startPort);
                    Console.CursorTop = cursorTop;
                }
                if (Program.stop)
                    break;
                try
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.BeginConnect((EndPoint)new IPEndPoint(address, startPort), new AsyncCallback(Program.EndConnect), (object)socket);
                    Interlocked.Increment(ref Program.waitingForResponses);
                }
                catch (Exception)
                {
                }
            }
        }

        private static void EndConnect(IAsyncResult ar)
        {
            try
            {
                Program.DecrementResponses();
                Socket asyncState = ar.AsyncState as Socket;
                asyncState.EndConnect(ar);
                if (!asyncState.Connected)
                    return;
                int int32 = Convert.ToInt32(asyncState.RemoteEndPoint.ToString().Split(':')[1]);
                Program.openPorts.Add(int32);
                lock (Program.consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(" This port is open: {0} ", (object)int32);
                    Console.ResetColor();
                }
                asyncState.Disconnect(true);
            }
            catch (Exception)
            {
            }
        }

        private static void IncrementResponses()
        {
            Interlocked.Increment(ref Program.waitingForResponses);
            Program.PrintWaitingForResponses();
        }

        private static void DecrementResponses()
        {
            Interlocked.Decrement(ref Program.waitingForResponses);
            Program.PrintWaitingForResponses();
        }

        private static void PrintWaitingForResponses()
        {
            lock (Program.consoleLock);
        }
    }
}
