using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Adaptive.DocumentOcrSdk;
using Newtonsoft.Json;

namespace PassportVisionRecognizer
{
    class Program
    {
        private const string RecognizerId = "passportvision";

        private static readonly Recognizer Engine = new Recognizer();

        [STAThread]
        public static void Main(string[] args)
        {
            var dataPath = "../../data/";
            var resultPath = "../../result/";

            if (args.Length == 2)
            {
                dataPath = args[0];
                resultPath = args[1];
            }

            Console.WriteLine();
            Console.WriteLine("Data path:   " + dataPath);
            Console.WriteLine("Result path: " + resultPath);
            Console.WriteLine();

            if (!Directory.Exists(dataPath))
            {
                Console.WriteLine();
                Console.WriteLine("Failed to open data directory");
            }

            if (!Directory.Exists(resultPath))
            {
                Console.WriteLine();
                Console.WriteLine("Failed to open result directory");
            }

            try
            {
                ProcessData(dataPath, resultPath);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static void ProcessData(string dataPath, string resultPath)
        {
            foreach (var dataPackDirectory in Directory.GetDirectories(dataPath))
            {
                var resultDirPath = resultPath + RecognizerId + "/" + Path.GetFileName(dataPackDirectory);
                Directory.CreateDirectory(resultDirPath);

                foreach (var dataPackFile in Directory.GetFiles(dataPackDirectory))
                {
                    Console.WriteLine();
                    Console.WriteLine(dataPackFile);

                    var result = RecognizeImage(dataPackFile);
                    var json = JsonConvert.SerializeObject(result);

                    var resultFilePath = resultDirPath + "/" + Path.GetFileName(dataPackFile) + ".json";
                    File.WriteAllText(resultFilePath, json);
                }
            }
        }

        private static Dictionary<string, object> RecognizeImage(string imagePath)
        {
            var watch = new Stopwatch();
            watch.Start();

            var result = Engine.Recognize(imagePath, DocumentType.RussianPassportComboSmart);
            
            watch.Stop();

            var data = new Dictionary<string, object>
            {
                ["image_path"] = imagePath,
                ["time"] = watch.ElapsedMilliseconds,
                ["failure"] = result == null
            };

            if (result != null)
            {
                var rowsData = new Dictionary<string, object>()
                {
                    ["rows_count"] = result.Count
                };
                data["data"] = rowsData;

                foreach (var row in result)
                {
                    var rowData = new Dictionary<string, object>();
                    rowsData[row.Id] = rowData;

                    rowData["confidence"] = row.Confidence;
                    rowData["value"] = row.Text;
                }
            }

            return data;
        }
    }
}