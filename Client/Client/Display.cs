using System;
using System.Collections;
using System.Threading;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;

namespace Client
{
    public class Display
    {
        private FEZ client;
        private TextBox username;
        private PasswordBox password;
        private TextBox nonce;
        private int nonceCount = 0;

        // Constructor
        public Display(FEZ client)
        {
            this.client = client;
        }

        public void Start()
        {
            // Initialize components
            GlideTouch.Initialize();
            // Starting window: LOGIN
            LoginWindow();
        }

        private void LoginWindow()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Login));
            Glide.MainWindow = window;
            // Login button event handler
            Button btn = (Button)window.GetChildByName("Login_button");
            btn.TapEvent += OnTap;
        }

        private void CredentialsWindow()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Users));
            Glide.MainWindow = window;
            // Load a keyboard for inserting credentials
            username = (TextBox)window.GetChildByName("username");
            username.Text = "";
            username.TapEvent += Glide.OpenKeyboard;
            password = (PasswordBox)window.GetChildByName("password");
            password.Text = "";
            password.TapEvent += Glide.OpenKeyboard;
            // Login button event handler
            Button btn = (Button)window.GetChildByName("login");
            btn.TapEvent += Query;
            // Back button event handler
            Button backBtn = (Button)window.GetChildByName("back");
            backBtn.TapEvent += BackToLogin;
        }

        private void NotUserWindow()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.NotUsers));
            Glide.MainWindow = window;
            // Back button event handler
            Button backBtn = (Button)window.GetChildByName("back_button");
            backBtn.TapEvent += OnTap;
        }

        private void PictureWindow()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Picture));
            Glide.MainWindow = window;
            // Take a picture button event handler
            Button picBtn = (Button)window.GetChildByName("Picture_button");
            picBtn.TapEvent += TakePicture;
        }

        private void Authorized()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Authorized));
            Glide.MainWindow = window;
            // Set Led to green for 5 seconds and restart the client
            client.SetLed(1);
            Thread.Sleep(5000);
            client.SetLed(2);
            client.Restart();
        }

        private void Nonce()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Nonce));
            Glide.MainWindow = window;
            // Specify the current try
            TextBlock sms = (TextBlock)window.GetChildByName("SMS");
            sms.Text = sms.Text + " (" + (nonceCount + 1).ToString() + "/3)";
            window.FillRect(sms.Rect);
            sms.Invalidate();
            // Load a keyboard for inserting Nonce
            nonce = (TextBox)window.GetChildByName("Nonce_text");
            nonce.Text = "";
            nonce.TapEvent += Glide.OpenKeyboard;
            // Done button event handler
            Button doneBtn = (Button)window.GetChildByName("Done");
            doneBtn.TapEvent += NonceCheck;
        }

        private void NotAuthorized()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Failure));
            Glide.MainWindow = window;
            // Set Led to red for 5 seconds and restart the client
            client.SetLed(0);
            Thread.Sleep(5000);
            client.SetLed(2);
            client.Restart();
        }

        private void EmptyNonce()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.EmptyNonce));
            Glide.MainWindow = window;
            // Back button event handler
            Button backBtn = (Button)window.GetChildByName("back_button");
            backBtn.TapEvent += GoToNonce;
        }

        private void WrongNonce()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.WrongNonce));
            Glide.MainWindow = window;
            // Back button event handler
            Button backBtn = (Button)window.GetChildByName("back_button");
            backBtn.TapEvent += GoToNonce;
        }

        private void OnTap(object sender)
        {
            // Second window: CREDENTIALS
            CredentialsWindow();
        }

        private void BackToLogin(object sender)
        {
            // Starting window: LOGIN
            LoginWindow();
        }

        private void Query(object sender)
        {
            string userPass = username.Text + "@" + password.Text;
            // The following FEZ method executes an SQL query and returns a bool:
            // if true, the user is present in the DB and can proceed with the picture; if false, go back to the initial window
            if (client.checkCredentials(userPass))
            {
                // Third window: TAKE A PICTURE
                PictureWindow();
            }
            else
            {
                // Wrong credentials window
                NotUserWindow();
            } 
        }

        private void TakePicture(object sender)
        {
            // The following method executes the face matching algorithm and is followed by the AfterMaching method
            client.Matching();
        }

        public void AfterMatching(bool flag)
        {
            if (flag)
            {
                // If the face matching operation has been succesful, last window: AUTHORIZED ACCESS
                Authorized();
            }
            else
            {
                // If not, fourth window: NONCE CHECK
                Nonce();
            }
        }

        private void GoToNonce(object sender)
        {
            // Fourth window: NONCE CHECK
            Nonce();
        }

        private void NonceCheck(object sender)
        {
            // If the nonce field is empty, retry
            if (nonce.Text.Equals(""))
            {
                EmptyNonce();
            }
            else
            {
                // Nonce check
                if (client.checkNonce(nonce.Text))
                {
                    // Last window: AUTHORIZED ACCESS
                    Authorized();
                }
                else
                {
                    // If the client inserts a wrong nonce, he can retry for at most three times
                    nonceCount++;
                    if (nonceCount == 3)
                    {
                        nonceCount = 0;
                        // Last window: NOT AUTHORIZED ACCESS
                        NotAuthorized();
                    }
                    else
                    {
                        WrongNonce();
                    }
                }
            }
        }
    }
}
