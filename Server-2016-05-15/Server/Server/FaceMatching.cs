using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;

namespace ServerPack
{
    // This class verifies whether two faces belong to a same person and is based on FACE API v1.0 (Face - Verify) provided by Windows
    public class FaceMatching
    {
        // Registration key
        private readonly IFaceServiceClient faceServiceClient = 
                new FaceServiceClient("40c7e5bc21bd441d8b97a6097b09b5c2");
        //private readonly IFaceServiceClient faceServiceClientTwo =
        //        new FaceServiceClient("6afdaca1cdf84d049fd8f75930d1b817");
        private string[] photos = new string[] {"",""};
        // Coefficient that indicates the confidence of whether two faces belong to one person
        private static float coeff;
        // Semaphore for handling the asynchronous MakeRequest method 
        private static Semaphore _pool;

        public float Matching(byte[] firstImage, string SecondPath)
        {
            _pool = new Semaphore(0, 1);

            Task.Run(async () =>
            {
            Face[] face = await
                UploadAndDetectFaces(firstImage,SecondPath);
            }).Wait();

            if (photos.Length != 2)
            {
                return 0;
            }
            else
            {
                this.MakeRequest();
                _pool.WaitOne();
                Console.WriteLine("Coeff: " + coeff);
                return coeff;
            }
        }

        // This methods receives as input a picture converted in byte[] and a path correspoding to an another picture
        // It detects one face per picture (if there is more than one face per picture, the algorithm returns a confidence coefficient equal to 0)
        // Each face is assigned to an ID
        // The two IDs are converted in strings and are stored into the photos array
        private async Task<Face[]> UploadAndDetectFaces(byte[] firstImage, string SecondPath)
        {
            try
            {
                Face[] faceArray;
                Face[] faceArrayTwo;
                int i = 0;
                using (Stream imageFileStream = new MemoryStream(firstImage))
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
                using (Stream imageFileStream = File.OpenRead(SecondPath))
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

        // This method performs an HTTP Request (POST) in order to retrieve the confidence coefficient
        // Request URL: https://api.projectoxford.ai/face/v1.0/verify
        // Request Body (JSON): "faceId1":"ID1", "faceId2":"ID2"
        // Response Body: "isIdentical":bool, "confidence":float
        private async void MakeRequest()
        {
            try
            {
                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "40c7e5bc21bd441d8b97a6097b09b5c2");

                var uri = "https://api.projectoxford.ai/face/v1.0/verify?" + queryString;

                HttpResponseMessage response;

                // Request body
                string json = "{\"faceId1\":\"" + photos[0] + "\",\"faceId2\":\"" + photos[1] + "\"}";
                byte[] byteData = Encoding.UTF8.GetBytes(json);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    // Response body
                    response = await client.PostAsync(uri, content);
                    string s = response.Content.ReadAsStringAsync().Result;
                    var r = new Regex(@"[0-9]+\.[0-9]+");
                    Match m = r.Match(s);
                    coeff = float.Parse(m.Value, CultureInfo.InvariantCulture.NumberFormat);
                    _pool.Release();
                }
            }
            catch
            {
                coeff = 0;
                Console.WriteLine("Exception");
                _pool.Release();
                return;
            }
        }
    }
}
