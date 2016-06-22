using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DDSTerminal
{
    public class TCPListenerWrapper
    {
        // Buffer for reading data
        byte[] bytes;
        string data;
        TcpListener server;
        Thread tcpThread;
        bool keepListening;
        Controller c;

        public TCPListenerWrapper(Controller c)
        {
            this.c = c;
        }

        public void Start(int port)
        {         
            server = null;
            c.WriteToConsole("Starting TCPListener.");
            try
            {
                

                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                bytes = new byte[256];
                data = null;

                tcpThread = new Thread(new ThreadStart(listenLoop));
                tcpThread.Start();
            }
            catch (Exception e)
            {
                c.WriteToConsole("Failed. " + e.Message);
            }
        }
        private void listenLoop()
        {
            keepListening = true;
            c.WriteToConsole("Complete.");
            try
            {
                // Enter the listening loop.
                while (keepListening)
                {

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    c.WriteToConsole("Remote connection established.");
                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        if(!data.Equals("\r\0"))
                        {
                            // Print data to console.
                            c.DispatchCommandToDDS(data);
                            c.WriteToConsole(data);
                        }
                        
                    }

                    // Shutdown and end connection
                    client.Close();
                    c.WriteToConsole("Disconnected from remote connection.");
                }

            }
            catch (Exception e)
            {
                c.WriteToConsole("Failed. " + e.Message);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
                if (keepListening)
                {
                    listenLoop();
                }
            }
        }

        public void StopTCPServer()
        {
            keepListening = false;
        }

    }
}
    

