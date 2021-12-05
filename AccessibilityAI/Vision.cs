using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace AccessibilityAI
{
    public class Vision
    {
        private static readonly string SubscriptionKey = "9d0c4309b0d34999bc2e2dc9164d12fa";
        private static readonly string Endpoint = "https://westeurope.api.cognitive.microsoft.com/";

        private static readonly string CustomPredictionKey = "358a0a9c1f69440d80dbc8d7e8954f20";
        private static readonly string CustomPredictionEndpoint = "https://ailtpresident-prediction.cognitiveservices.azure.com/";

        private readonly ComputerVisionClient _computerVisionClient;

        private readonly CustomVisionPredictionClient _customPredictionClient;

        public Vision()
        {
            _computerVisionClient = Authenticate(Endpoint, SubscriptionKey);

            _customPredictionClient = new CustomVisionPredictionClient(
                new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(CustomPredictionKey))
            {
                Endpoint = CustomPredictionEndpoint
            };
        }

        private static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        public async Task DescribeWhatIsOnAnImage(string localImage = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/amsterdam-streets-2.jpeg")
        {
            Console.WriteLine("Opening an image...");
            DisplayImage(localImage);

            using Stream analyzeImageStream = File.OpenRead(localImage);
            ImageDescription results = await _computerVisionClient.DescribeImageInStreamAsync(analyzeImageStream);

            if (null != results && null != results.Captions)
            {
                Console.WriteLine("Summary:");
                foreach (var caption in results.Captions)
                {
                    Console.WriteLine($"{caption.Text} with confidence {caption.Confidence}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public async Task DetectWhatIsOnAnImage(string localImage = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/amsterdam-streets-2.jpeg")
        {
            Console.WriteLine("Opening an image...");
            DisplayImage(localImage);

            using Stream analyzeImageStream = File.OpenRead(localImage);
            DetectResult results = await _computerVisionClient.DetectObjectsInStreamAsync(analyzeImageStream);

            if (null != results.Objects)
            {
                Console.WriteLine("Objects:");
                foreach (var obj in results.Objects)
                {
                    Console.WriteLine($"{obj.ObjectProperty} with confidence {obj.Confidence} at location {obj.Rectangle.X}, " +
                      $"{obj.Rectangle.X + obj.Rectangle.W}, {obj.Rectangle.Y}, {obj.Rectangle.Y + obj.Rectangle.H}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public async Task WhoIsOnTheImage(string localImage)
        {
            Console.WriteLine("Opening an image...");
            DisplayImage(localImage);

            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Objects
            };

            using Stream analyzeImageStream = File.OpenRead(localImage);
            var results = await _computerVisionClient.AnalyzeImageInStreamAsync(analyzeImageStream, features);

            if (null != results.Description && null != results.Description.Captions)
            {
                Console.WriteLine("Summary:");
                foreach (var caption in results.Description.Captions)
                {
                    Console.WriteLine($"{caption.Text} with confidence {caption.Confidence}");
                }
                Console.WriteLine();
            }

            if (null != results.Faces)
            {
                Console.WriteLine("Faces:");
                foreach (var face in results.Faces)
                {
                    Console.WriteLine($"A {face.Gender} of age {face.Age} at location {face.FaceRectangle.Left}, {face.FaceRectangle.Top}, " +
                      $"{face.FaceRectangle.Left + face.FaceRectangle.Width}, {face.FaceRectangle.Top + face.FaceRectangle.Height}");
                }
                Console.WriteLine();
            }

            if (null != results.Tags)
            {
                Console.WriteLine("Tags:");
                foreach (var tag in results.Tags)
                {
                    Console.WriteLine($"{tag.Name} {tag.Confidence}");
                }
                Console.WriteLine();
            }

            if (null != results.Objects)
            {
                Console.WriteLine("Objects:");
                foreach (var obj in results.Objects)
                {
                    Console.WriteLine($"{obj.ObjectProperty} with confidence {obj.Confidence} at location {obj.Rectangle.X}, " +
                      $"{obj.Rectangle.X + obj.Rectangle.W}, {obj.Rectangle.Y}, {obj.Rectangle.Y + obj.Rectangle.H}");
                }
                Console.WriteLine();
            }

            if (null != results.Categories)
            {
                Console.WriteLine("Celebrities:");
                foreach (var category in results.Categories)
                {
                    if (category.Detail?.Celebrities != null)
                    {
                        foreach (var celeb in category.Detail.Celebrities)
                        {
                            Console.WriteLine($"{celeb.Name} with confidence {celeb.Confidence} at location {celeb.FaceRectangle.Left}, " +
                              $"{celeb.FaceRectangle.Top},{celeb.FaceRectangle.Height},{celeb.FaceRectangle.Width}");
                        }
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public void PredictImage(string imagePath)
        {
            Console.WriteLine("Making a prediction:");

            var projectId = new Guid("f54400ae-6418-4833-b5b5-a03dc8c4a63e");

            using (var stream = File.OpenRead(imagePath))
            {
                var result = _customPredictionClient.DetectImage(projectId, "Iteration1", stream);

                // Loop over each prediction and write out the results
                foreach (var c in result.Predictions)
                {
                    Console.WriteLine($"\t{c.TagName}: {c.Probability:P1} [ {c.BoundingBox.Left}, {c.BoundingBox.Top}, {c.BoundingBox.Width}, {c.BoundingBox.Height} ]");
                }
            }
            Console.ReadKey();
        }

        public async Task ReadTextFromImage(string localImage)
        {
            Console.WriteLine("Opening an image...");
            DisplayImage(localImage);

            using Stream analyzeImageStream = File.OpenRead(localImage);
            var textHeaders = await _computerVisionClient.ReadInStreamAsync(analyzeImageStream);

            string operationLocation = textHeaders.OperationLocation;

            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            ReadOperationResult results;

            do
            {
                results = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));
   
            Console.WriteLine();

            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    Console.WriteLine(line.Text);
                }
            }
            Console.WriteLine();
        }

        public async Task AnalizeLocalImage(string localImage = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/amsterdam-streets-2.jpeg")
        {
            Console.WriteLine("ANALYZE IMAGE - LOCAL IMAGE");
            Console.WriteLine();

            // Creating a list that defines the features to be extracted from the image. 
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            Console.WriteLine($"Analyzing the local image {Path.GetFileName(localImage)}...");
            Console.WriteLine();

            using (Stream analyzeImageStream = File.OpenRead(localImage))
            {
                // Analyze the local image.
                ImageAnalysis results = await _computerVisionClient.AnalyzeImageInStreamAsync(analyzeImageStream, visualFeatures: features);

                // Sunmarizes the image content.
                if (null != results.Description && null != results.Description.Captions)
                {
                    Console.WriteLine("Summary:");
                    foreach (var caption in results.Description.Captions)
                    {
                        Console.WriteLine($"{caption.Text} with confidence {caption.Confidence}");
                    }
                    Console.WriteLine();
                }

                // Display categories the image is divided into.
                Console.WriteLine("Categories:");
                foreach (var category in results.Categories)
                {
                    Console.WriteLine($"{category.Name} with confidence {category.Score}");
                }
                Console.WriteLine();

                // Image tags and their confidence score
                if (null != results.Tags)
                {
                    Console.WriteLine("Tags:");
                    foreach (var tag in results.Tags)
                    {
                        Console.WriteLine($"{tag.Name} {tag.Confidence}");
                    }
                    Console.WriteLine();
                }

                // Objects
                if (null != results.Objects)
                {
                    Console.WriteLine("Objects:");
                    foreach (var obj in results.Objects)
                    {
                        Console.WriteLine($"{obj.ObjectProperty} with confidence {obj.Confidence} at location {obj.Rectangle.X}, " +
                          $"{obj.Rectangle.X + obj.Rectangle.W}, {obj.Rectangle.Y}, {obj.Rectangle.Y + obj.Rectangle.H}");
                    }
                    Console.WriteLine();
                }

                // Detected faces, if any.
                if (null != results.Faces)
                {
                    Console.WriteLine("Faces:");
                    foreach (var face in results.Faces)
                    {
                        Console.WriteLine($"A {face.Gender} of age {face.Age} at location {face.FaceRectangle.Left}, {face.FaceRectangle.Top}, " +
                          $"{face.FaceRectangle.Left + face.FaceRectangle.Width}, {face.FaceRectangle.Top + face.FaceRectangle.Height}");
                    }
                    Console.WriteLine();
                }

                // Well-known brands, if any.
                if (null != results.Brands)
                {
                    Console.WriteLine("Brands:");
                    foreach (var brand in results.Brands)
                    {
                        Console.WriteLine($"Logo of {brand.Name} with confidence {brand.Confidence} at location {brand.Rectangle.X}, " +
                          $"{brand.Rectangle.X + brand.Rectangle.W}, {brand.Rectangle.Y}, {brand.Rectangle.Y + brand.Rectangle.H}");
                    }
                    Console.WriteLine();
                }

                // Celebrities in image, if any.
                if (null != results.Categories)
                {
                    Console.WriteLine("Celebrities:");
                    foreach (var category in results.Categories)
                    {
                        if (category.Detail?.Celebrities != null)
                        {
                            foreach (var celeb in category.Detail.Celebrities)
                            {
                                Console.WriteLine($"{celeb.Name} with confidence {celeb.Confidence} at location {celeb.FaceRectangle.Left}, " +
                                  $"{celeb.FaceRectangle.Top},{celeb.FaceRectangle.Height},{celeb.FaceRectangle.Width}");
                            }
                        }
                    }
                    Console.WriteLine();
                }

                // Popular landmarks in image, if any.
                if (null != results.Categories)
                {
                    Console.WriteLine("Landmarks:");
                    foreach (var category in results.Categories)
                    {
                        if (category.Detail?.Landmarks != null)
                        {
                            foreach (var landmark in category.Detail.Landmarks)
                            {
                                Console.WriteLine($"{landmark.Name} with confidence {landmark.Confidence}");
                            }
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        private static void DisplayImage(string imagePath = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/amsterdam-streets-2.jpeg")
        {
            //using Process process = new Process();

            //process.StartInfo.FileName = "/System/Applications/Preview.app/Contents/MacOS/Preview";
            //process.StartInfo.Arguments = imagePath;
            //process.StartInfo.RedirectStandardOutput = true;
            //process.Start();
            //process.WaitForExit();
        }
    }
}
