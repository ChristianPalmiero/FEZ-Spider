using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;

namespace Client
{
    public class FEZ 
    {
        // The port number for the listening socket of the server
        private const int port = 8080;
        // The touch screen display
        private Display display;
        // A bool used as a flag
        private bool flag = false;
        // A TCP/IP Socket
        private Socket server;
        // The Ethernet J11D module
        private EthernetJ11D ethernetJ11D;
        // The Camera module
        private Camera camera;
        // The multicolor LED
        private MulticolorLED led;
        // Threads
        Thread workerThread;
        Thread workerThreadOne;

        // Constructor
        public FEZ(Camera camera, MulticolorLED led, EthernetJ11D ethernetJ11D)
        {
            this.camera = camera;
            this.led = led;
            this.ethernetJ11D = ethernetJ11D;
            display = new Display(this);
        }

        public void ProgramStarted()
        {
            // Set up the ethernetJ11D component
            ethernetJ11D.UseThisNetworkInterface();
            ethernetJ11D.UseStaticIP("192.168.100.2", "255.255.255.0", "192.168.100.1");
            ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;
            ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            // Set up the camera component
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
            //new Thread(display.Start).Start();
            //Thread.Sleep(5000); 
            //new Thread(StartClient).Start();
            workerThread = new Thread(display.Start);
            workerThread.Start();
            Thread.Sleep(5000);
            workerThreadOne = new Thread(StartClient);
            workerThreadOne.Start();
        }

        private void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network is down!");
            workerThread.Join();
            workerThreadOne.Join();
            ProgramStarted();
        }

        private void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network is up!");
            Debug.Print("My IP is: " + ethernetJ11D.NetworkSettings.IPAddress);
            Debug.Print("DHCP Enabled: " + ethernetJ11D.NetworkSettings.IsDhcpEnabled);
            Debug.Print("Subnet Mask:  " + ethernetJ11D.NetworkSettings.SubnetMask);
            Debug.Print("Gateway:      " + ethernetJ11D.NetworkSettings.GatewayAddress);
            Debug.Print("------------------------------------------------");
        }

        //void RunWebServer()
        //{
        //    // Wait for the network...
        //    while (ethernetJ11D.IsNetworkUp == false)
        //    {
        //        Debug.Print("Waiting...");
        //        Thread.Sleep(1000);
        //    }
        //    new Thread(StartClient).Start();
        //}
        
        public void StartClient()
        {
            try
            {
                // Establish the remote endpoint for the socket
                IPAddress ServerIP = new IPAddress(new byte[] { 192, 168, 100, 4 });
                IPEndPoint remoteEP = new IPEndPoint(ServerIP, port);
                // Create a TCP/IP socket
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Set the timeout for synchronous receive methods to 120 second (120000 milliseconds)
                server.ReceiveTimeout = 120000;
                // Connect to the remote endpoint
                server.Connect(remoteEP);
                Debug.Print("Connection to : " + remoteEP.ToString());
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
        }

        public bool checkCredentials(string content)
        {
            // Send the string content to the remote host; content="user@password"
            string response = SendString(server, content);
            flag = false;
            Debug.Print("Response = " + response);
            if ((response.IndexOf("ACK")) == 0)
            {
                Debug.Print("Credentials OK");
                flag = true;
            }
            return flag;
        }

        public bool checkNonce(string content)
        {
            // Send the Nonce to the remote host
            string response = SendString(server, content);
            flag = false;
            Debug.Print("Response = " + response);
            if ((response.IndexOf("ACK")) == 0)
            {
                Debug.Print("Nonce OK");
                flag = true;
            }
            return flag;
        }

        public void Matching()
        {
            camera.TakePicture();
        }

        void camera_PictureCaptured(Camera sender, GT.Picture e)
        {
            // Send the image to the remote host
            byte[] output = e.PictureData;
            string response = SendImage(server, output);
            flag = false;
            Debug.Print("Response = " + response);
            if ((response.IndexOf("ACK")) == 0)
            {
                Debug.Print("Face matching OK");
                flag = true;
            }
            else
            {
                SendString(server, "Send me nonce");
            }
            // Call a display method according to the face matching algorithm result
            display.AfterMatching(flag);

        }

        private string reciveAck(Socket server)
        {
            // Read the response from the remote device
            byte[] bytes = new byte[20];
            int received;
            received = server.Receive(bytes);
            Debug.Print(new string(Encoding.UTF8.GetChars(bytes)));
            return new string(Encoding.UTF8.GetChars(bytes));
        }

        private string SendString(Socket server, string data)
        {
            // Protocol for sending a string:
            // First, send the length of the content
            // Then, send the content
            Debug.Print("String sent");
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            server.Send(BitConverter.GetBytes(byteData.Length));
            reciveAck(server);
            server.Send(byteData);
            return reciveAck(server);
        }

        private string SendImage(Socket server, byte[] byteData)
        {
            // Protocol for sending an image:
            // First, send the length of the content
            // Then, send the content
            Debug.Print("Image sent");
            server.Send(BitConverter.GetBytes(byteData.Length));
            reciveAck(server);
            server.Send(byteData);
            return reciveAck(server);
        }

        public void SetLed(int i)
        {
            // Set the led according to the input parameter
            switch(i)
            {
                case 0: 
                    led.TurnRed();
                    break;
                case 1:
                    led.TurnGreen();
                    break;
                case 2:
                    led.TurnOff();
                    break;
            }
        }

        public void Restart()
        {
            // Protocol for closing the connection: 
            // First, send the following string
            // Then, release the socket
            string content = "Close the connection";
            string response = SendString(server, content);
            Debug.Print("Response = " + response);
            server.Close();
            // Sleep for 5 seconds and restart
            Thread.Sleep(5000);
            ProgramStarted();
        }
    }
}