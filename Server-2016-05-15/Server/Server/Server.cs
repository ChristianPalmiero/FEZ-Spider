using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerPack
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client socket
        public Socket workSocket = null;
        // Size of the received buffer
        public const int BufferSize = 1024;
        // Received buffer
        public byte[] buffer = new byte[BufferSize];
        // Incoming content size
        public int contentSize;
        // Reciving stage allows the server to understend what to receive
        // stage zero -> the server will receive the username and the password in the following format : username@password
        // stage one -> the server will receive the image
        // stage two -> the server will receive the nonce that should be equal to the nonce sent via sms
        // stage three -> the server will receive a string and will close the socket
        public int receiving_stage;
        // Received dynamic buffer
        public byte[] _contentDynamicBuff;
        // How many bytes have been read
        public int _totBytesRead;
        // Nonce (string+cnt)
        public string nonce;
        public int cnt;
        // Username and password
        public string username;
        public string password;
        // Image path retrieved from a MySql Database
        public string img;
        // Face matching coefficient
        public float coeff;
        // FaceMatching object
        public FaceMatching f = new FaceMatching();
    }

    public class Server
    {
        // Thread signal
        private ManualResetEvent allDone = new ManualResetEvent(false);
        // Database connection
        private Database db = new Database("server=localhost; user id=Chris; password=christian8; database=sys");
        // Log file
        private string logFile = @"C:\Users\Chris\Documents\Visual Studio 2013\Projects\Server-2016-05-15\Current Log File.txt";
        FileStream fs;
        // TODO: REMOVE THE CONTENT OF THE FILE WHEN THE EMAIL IS SENT

        public void StartListening()
        {
            // Data buffer for the incoming data
            byte[] bytes = new Byte[1024];
            // Local endpoint for the socket
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 8080);
            // TCP/IP socket
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Create the Log file
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
            fs = File.Create(logFile);
            fs.Close();
            // Bind the socket to the local endpoint and listen for incoming connections
            try
            {
                listener.Bind(localEndPoint);
                // Set pending queue to 100
                listener.Listen(100);
                while (true)
                {
                    // Set the event to nonsignaled state
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections
                    Console.WriteLine("Waiting for a connection");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    // Wait until a connection is established before continuing
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Send a signal to the main thread in order to continue
            allDone.Set();
            // Get the socket that handles the client request
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Create the state object
            StateObject state = new StateObject();
            state.receiving_stage = 0;
            state.workSocket = handler;
            Console.WriteLine("Connected with : {0}", state.workSocket.RemoteEndPoint.ToString());
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadHeaderCallback), state);
        }

        private void ReadHeaderCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket from the asynchronous state object
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            // Read the data from the client socket
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead < sizeof(int))
            {
                // Read again
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
              new AsyncCallback(ReadHeaderCallback), state);
            }
            else
            {
                state.contentSize = BitConverter.ToInt32(state.buffer, 0);
                state._totBytesRead = 0;
                state._contentDynamicBuff = new byte[state.contentSize];
                Console.WriteLine("ReadHeaderCallback size of data = {0}", state.contentSize);
                // Send an ack to the client
                Send(handler, "ACK");
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                   new AsyncCallback(ReadCallback), state);
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket from the asynchronous state object
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            // Check whether the data provided by the client is correct
            bool control = false;
            // Read data from the client socket
            int bytesRead = handler.EndReceive(ar);
            // String to be written into the Log file
            string text;

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far
                Buffer.BlockCopy(state.buffer, 0, state._contentDynamicBuff, state._totBytesRead, bytesRead);
                state._totBytesRead += bytesRead;
                // Check whether we have read the entire content
                if (state._totBytesRead >= state.contentSize)
                {
                    switch (state.receiving_stage)
                    {
                        case 0:
                            // Expect username and password 
                            Console.WriteLine("Received from {0} username and password equal to {1}", state.workSocket.RemoteEndPoint.ToString(), System.Text.Encoding.UTF8.GetString(state._contentDynamicBuff));
                            string[] tokens = System.Text.Encoding.UTF8.GetString(state._contentDynamicBuff).Split('@');
                            state.username = tokens[0];
                            state.password = tokens[1];
                            text = "Attempt of login with these credentials -> Username: " + state.username + ", Password: " + state.password;
                            WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                            control = CheckUserPass(System.Text.Encoding.UTF8.GetString(state._contentDynamicBuff));
                            if (control == true)
                            {
                                // Send the ACK and move to the next stage
                                text = "Login OK";
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                Send(handler, "ACK");
                                state.receiving_stage++;
                            }
                            else
                            {
                                // Send the NACK and remain in the current stage
                                text = "Login Failed";
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                Send(handler, "NACK");
                            }
                            break;
                        case 1:
                            // Expect the image
                            //state.coeff = 0; //to be commented
                            state.img = RetrieveImgPath(state.username);
                            Console.WriteLine("Imagepath: {0}", state.img);
                            string ack = String.Format("Image correctly recived.{0}", Environment.NewLine);
                            //var fs = new BinaryWriter(new FileStream(@"C:\Users\Public\Pictures\Sample Pictures\temp2.jpeg", FileMode.Append, FileAccess.Write));
                            //fs.Write(state._contentDynamicBuff);
                            //fs.Close();
                            state.coeff = state.f.Matching(state._contentDynamicBuff, state.img); 
                            // If coeff >= 0.5  => Same person
                            // Else if coef = -1 => The picture taken does not contain only one face
                            if (state.coeff == -1){
                                text = "Picture taken. Result: the picture does not contain only one face; the user has to retake the picture";
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                Send(handler, "NACKS");
                            }
                            else if (state.coeff >= 0.5)
                            {
                                // Send the ACK and remain in the current stage
                                text = "Picture taken. Result: Match";
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                text = "Authorized access";
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                Send(handler, "ACK");
                                state.receiving_stage += 2;
                            }
                            else
                            {
                                // Send the NACK and move to the next stage
                                text = "Picture taken. Result: Non match";
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                Send(handler, "NACK");
                                state.receiving_stage++;
                            }
                            break;
                        case 2:
                            // Expect the nonce
                            // Generate nonce
                            //state.nonce = GenerateRandomNumber();
                            state.nonce = "0000";
                            text = "Nonce to be inserted equal to " + state.nonce;
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                            Console.WriteLine("Generated nonce: " + state.nonce);
                            // Send nonce as sms
                            //SendSMS(state.nonce);
                            control = CheckNonce(System.Text.Encoding.UTF8.GetString(state._contentDynamicBuff), state.nonce);
                            if (control == true)
                            {
                                // Send the ACK and move to the next stage
                                text = "Nonce correctly inserted at attempt " + (state.cnt+1);
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                text = "Authorized access";
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                Send(handler, "ACK");
                                state.receiving_stage++;
                            }
                            else
                            {
                                // Send the NACK and remain in the current stage, unless there is no available trial
                                text = "Nonce not correctly inserted at attempt " + (state.cnt + 1);
                                WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                Send(handler, "NACK");
                                if (state.cnt == 2)
                                {
                                    text = "Failed access";
                                    WriteLogFile(text, state.workSocket.LocalEndPoint.ToString(), state.username, logFile);
                                    state.receiving_stage++;
                                }
                            }
                            state.cnt++;
                            break;
                        case 3:
                            // Send the ACK and close the socket
                            Send(handler, "ACK");
                            Console.WriteLine("Closing the connection");
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            state.workSocket.Close();
                            break;
                        default:
                            Console.WriteLine("Switch case index does not exist");
                            break;
                    }
                    if (state.workSocket.Connected)
                    {
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReadHeaderCallback), state);
                    }
                }
                else
                {
                    Console.WriteLine("Not all data received! Read more..");
                    // Not all data received: get more
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private bool CheckUserPass(string nameAndPassword)
        {
            return db.CheckPassword(nameAndPassword);
        }

        private string RetrieveImgPath(string user)
        {
            return db.RetrieveImage(user);
        }

        // Check if the received nonce is equal to the previously generated one
        private bool CheckNonce(string nonce_recived, string state)
        {
            Console.WriteLine("Checking nonce");
            bool ret = nonce_recived.Equals(state);
            return ret;
        }

        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void WriteLogFile(string s, string IPAddr, string user, string path)
        {

            string str = "IP Address and port: " + IPAddr + ", User: " + user + ", Timestamp: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(str);
                sw.WriteLine(s);
            }
        }

        private void SendEmail()
        {
            // Specify senders gmail address
            string SendersAddress = "l.chelini1@gmail.com";
            // Specify the address you want to sent Email to (it can be any valid email address)
            string ReceiversAddress = "l.chelini@icloud.com";
            // Specify the password of the gmail account you are using to sent mail (pw of sender@gmail.com)
            const string SendersPassword = "loredo1993";
            // Write the subject of your mail
            const string subject = "Testing";
            // Write the content of your mail
            const string body = "Hi This Is my Mail From Application";

            try
            {
                // Use Smtp client which allows to send email using SMTP Protocol
                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(SendersAddress, SendersPassword),
                    Timeout = 3000
                };
                // MailMessage represents a mail message: it is 4 parameters(From,TO,subject,body)
                MailMessage message = new MailMessage(SendersAddress, ReceiversAddress, subject, body);
                smtp.Send(message);
                Console.WriteLine("Message Sent Successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in sending the email");
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private void SendSMS(String message)
        {
            // This function uses files and services provided by : https://www.vianett.com/
            // In order to use it you shuld register first

            // Set parameters
            string username = "l.chelini@icloud.com";
            string password = "cvbht";
            string msgsender = "+393347055531";
            string destinationaddr = "+393334556569";
            // Create ViaNettSMS object with username and password
            ViaNettSMS s = new ViaNettSMS(username, password);
            // Declare Result object returned by the SendSMS function
            ViaNettSMS.Result result;

            try
            {
                // Send SMS through HTTP API
                result = s.sendSMS(msgsender, destinationaddr, message);
                // Show sent SMS response
                if (result.Success)
                {
                    Console.WriteLine("Message successfully sent");
                }
                else
                {
                    Console.WriteLine("Received error: " + result.ErrorCode + " " + result.ErrorMessage);
                }
            }
            catch (System.Net.WebException ex)
            {
                // Catch error occurred while connecting to server
                Console.WriteLine(ex.Message);
            }
        }

        // TODO: generate a random string
        private String GenerateRandomNumber()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int c = rnd.Next(1000, 9999);
            Console.WriteLine("Casual number format int is {0}", c);
            String number = c.ToString();
            Console.WriteLine("Casual number format string is {0}", number);
            return number;
        }

    }
}