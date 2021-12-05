using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace AccessibilityAI
{
    public class Speech
    {
        private const string RecordingName = "Belarus-crisis.wav";
        private const string ServiceRegion = "westeurope";
        private const string SpeechKey = "7a2bd190567a4e03a864aa92d4c97f1d";

        private const string Sentence = "Accessibility empowers everyone. Accessibility and inclusion are essential to delivering our mission to empower every person and every organisation on the planet to achieve more";

        private readonly SpeechConfig _speechConfig;
        private readonly SpeechTranslationConfig _speechTranslationConfig;

        public Speech()
        {
            _speechConfig = SpeechConfig.FromSubscription(SpeechKey, ServiceRegion);
            _speechConfig.SpeechSynthesisLanguage = "en-US";
            _speechConfig.SpeechSynthesisVoiceName = "en-GB-SoniaNeural";
            //_speechConfig.SpeechSynthesisVoiceName = "en-US-BrandonNeural";

            _speechTranslationConfig = SpeechTranslationConfig.FromSubscription(SpeechKey, ServiceRegion);
            _speechTranslationConfig.SpeechRecognitionLanguage = "en-US";
        }

        public async Task TextToSpeech(string textToSpeech = Sentence)
        {
            try
            {
                var audioFile = $"{_speechConfig.SpeechSynthesisVoiceName}.wav";
                using var audioConfig = AudioConfig.FromWavFileOutput(audioFile);
                using var synthesizer = new SpeechSynthesizer(_speechConfig, audioConfig);

                using (var result = await synthesizer.SpeakTextAsync(textToSpeech))
                {
                    if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        Console.WriteLine($"Speech synthesized to speaker for text [{Sentence}]");
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task SpeechToTextWriteConsole()
        {
            AddTargetLanguages();

            using var audioConfig = AudioConfig.FromWavFileInput(RecordingName);
            using var recognizer = new TranslationRecognizer(_speechTranslationConfig, audioConfig);

            var result = await recognizer.RecognizeOnceAsync();
            if (result.Reason == ResultReason.TranslatedSpeech)
            {
                Console.WriteLine($"Recognized: \"{result.Text}\":");
                foreach (var (language, translation) in result.Translations)
                {
                    Console.WriteLine($"\n\nTranslated into '{language}': {translation}");
                }
            }
        }

        public async Task SpeechToTextAudio(string voiceName, string targetLanguage)
        {
            RemoveTargetLanguages();
            _speechTranslationConfig.AddTargetLanguage(targetLanguage);
            _speechTranslationConfig.VoiceName = voiceName;

            using var audioConfig = AudioConfig.FromWavFileInput(RecordingName);
            using var recognizer = new TranslationRecognizer(_speechTranslationConfig, audioConfig);

            byte[] combinedAudio = null;

            recognizer.Synthesizing += (_, e) =>
            {
                var audio = e.Result.GetAudio();
                Console.WriteLine($"Audio synthesized: {audio.Length:#,0} byte(s) {(audio.Length == 0 ? "(Complete)" : "")}");

                if (audio.Length > 0)
                {
                    if (combinedAudio is null)
                    {
                        combinedAudio = audio;
                    }
                    else
                    {
                        combinedAudio = combinedAudio.Concat(audio).ToArray();
                    }
                }
            };

            var result = await recognizer.RecognizeOnceAsync();
            if (result.Reason == ResultReason.TranslatedSpeech)
            {
                Console.WriteLine("Translation completed!");

                if (combinedAudio.Length > 0)
                {
                    File.WriteAllBytes($"translated-demo-{voiceName}.wav", combinedAudio);
                }
            }
        }

        private void AddTargetLanguages()
        {
            _speechTranslationConfig.AddTargetLanguage("lt");
            _speechTranslationConfig.AddTargetLanguage("ru");
            _speechTranslationConfig.AddTargetLanguage("pl");
            _speechTranslationConfig.AddTargetLanguage("fr");
        }

        private void RemoveTargetLanguages()
        {
            _speechTranslationConfig.RemoveTargetLanguage("lt");
            _speechTranslationConfig.RemoveTargetLanguage("ru");
            _speechTranslationConfig.RemoveTargetLanguage("pl");
            _speechTranslationConfig.RemoveTargetLanguage("fr");
        }


    }
}
