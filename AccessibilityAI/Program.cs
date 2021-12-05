using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccessibilityAI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /**
             * 
             * SPEECH
             * 
             */

            //var speech = new Speech();
            //await speech.TextToSpeech();
            //await speech.SpeechToTextWriteConsole();

            //var languageToVoiceMap = new Dictionary<string, string>
            //{
            //    { "ru", "ru-RU-EkaterinaRUS" },
            //    { "it", "it-IT-Cosimo" },
            //    { "fr", "fr-FR-Julie" },
            //    { "zh", "zh-CN-Kangkang" }
            //};

            //foreach (var x in languageToVoiceMap)
            //{
            //    await speech.SpeechToTextAudio(x.Value, x.Key);
            //}

            /**
             * 
             * VISION
             * 
             */

            var vision = new Vision();

            //await vision.DescribeWhatIsOnAnImage();
            //await vision.DetectWhatIsOnAnImage();

            //string germanPresidentImage = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/merkel.jpeg";
            //string ltPresidentImage = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/lt-president.png";
            //string chandlerImage = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/perry.png";

            ////await vision.WhoIsOnTheImage(germanPresidentImage);
            ////await vision.WhoIsOnTheImage(ltPresidentImage);
            ////await vision.WhoIsOnTheImage(chandlerImage);

            ////string ltPresidentAndOthersImage = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/lt-president-and-trump.jpeg";
            ////vision.PredictImage(ltPresidentImage);

            //string imageWithText = "/Users/BF3774/Projects/AccessibilityAI/AccessibilityAI/Images/satya-acc.jpeg";
            //await vision.ReadTextFromImage(imageWithText);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
