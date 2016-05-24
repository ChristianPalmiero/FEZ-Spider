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
using Microsoft.SPOT;

namespace Display
{
    public partial class Program
    {

        public void ProgramStarted()
        {
            // Initialize components
            GlideTouch.Initialize();
            // Starting window: LOGIN
            LoginWindow();
            // Camera event handler
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
        }

        void camera_PictureCaptured(Camera sender, GT.Picture e)
        {
            displayT35.SimpleGraphics.DisplayImage(e, 5, 5);
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

        private void CredentialWindow()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Users));
            Glide.MainWindow = window;
            // Load a keyboard for inserting credentials
            TextBox username = (TextBox)window.GetChildByName("username");
            username.TapEvent += Glide.OpenKeyboard;
            PasswordBox password = (PasswordBox)window.GetChildByName("password");
            password.TapEvent += Glide.OpenKeyboard;
            // Login button event handler
            Button btn = (Button)window.GetChildByName("login");
            //TODO: Query SQL
            btn.TapEvent += TakePicture;
            // Back button event handler
            Button backBtn = (Button)window.GetChildByName("back");
            backBtn.TapEvent += BackToLogin;
        }

        private void PictureWindow()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Picture));
            Glide.MainWindow = window;
            // Back button event handler
            Button backBtn = (Button)window.GetChildByName("back");
            backBtn.TapEvent += OnTap;
            Button picBtn = (Button)window.GetChildByName("Picture_button");
            picBtn.TapEvent += Pic;
        }

        private void Authorized()
        {
            // Load the XML string
            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Authorized));
            Glide.MainWindow = window;
            // Go back to the initial login window after 5 seconds
            Thread.Sleep(5000);
            LoginWindow();
        }

        private void OnTap(object sender)
        {
            // Second window: CREDENTIALS
            CredentialWindow();
        }

        private void BackToLogin(object sender)
        {
            // Starting window: LOGIN
            LoginWindow();
        }

        private void TakePicture(object sender)
        {
            // Third window: PHOTO
            PictureWindow();
        }

        private void Pic(object sender)
        {
            camera.TakePicture();
        }
    }
}
