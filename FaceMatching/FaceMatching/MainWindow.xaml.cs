using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System.IO;

namespace FaceMatching
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly IFaceServiceClient faceServiceClient = 
                new FaceServiceClient("40c7e5bc21bd441d8b97a6097b09b5c2");
        //private readonly IFaceServiceClient faceServiceClientTwo =
        //        new FaceServiceClient("6afdaca1cdf84d049fd8f75930d1b817");
        private static string[] photos = new string[] {"",""};

        public MainWindow()
        {
            InitializeComponent();
            Task.Run(async () =>
            {
            Face[] face = await
                UploadAndDetectFaces(@"C:\Users\Public\Pictures\Sample Pictures\");
            }).Wait();

            //TODO: Controlli sul numero di facce, cioè elementi in photos

            MakeRequest();
        }

        private async Task<Face[]> UploadAndDetectFaces(string imageFolderPath)
        {
            try
            {
                Face[] faceArray;
                Face[] faceArrayTwo;
                int i = 0;
                using (Stream imageFileStream = File.OpenRead(imageFolderPath+"temp.jpeg"))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream, true, true);
                    foreach (var face in faces)
                    {
                        var id = face.FaceId;
                        photos[i++] = id.ToString();
                    }
                    faceArray = faces.ToArray();
                    //var faceRects = faces.Select(face => face.FaceRectangle);
                }
                using (Stream imageFileStream = File.OpenRead(imageFolderPath + "tmp.jpeg"))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream, true, true);
                    foreach (var face in faces)
                    {
                        var id = face.FaceId;
                        photos[i++] = id.ToString();
                    }
                    faceArrayTwo = faces.ToArray();
                    //var faceRects = faces.Select(face => face.FaceRectangle);
                }
                return faceArray.Concat(faceArrayTwo).ToArray();
            }
            catch (Exception)
            {
                return new Face[0];
            }
        }

        static async void MakeRequest()
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "40c7e5bc21bd441d8b97a6097b09b5c2");

            var uri = "https://api.projectoxford.ai/face/v1.0/verify?" + queryString;

            HttpResponseMessage response;

            // Request body
            string json = "{\"faceId1\":\""+photos[0]+"\",\"faceId2\":\""+photos[1]+"\"}";
            byte[] byteData = Encoding.UTF8.GetBytes(json);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                string s = response.Content.ReadAsStringAsync().Result;
                var r = new Regex(@"[0-9]+\.[0-9]+");
                Match m = r.Match(s);
                float coeff = float.Parse(m.Value, CultureInfo.InvariantCulture.NumberFormat);
                MessageBox.Show(coeff.ToString());
            }
        }
    }
}