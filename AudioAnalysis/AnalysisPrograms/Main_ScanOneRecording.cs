﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


namespace AnalysisPrograms
{
    class Main_ScanOneRecording
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("");


            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            int callID = 2;   // ONLY USE CALL 1 FOR UNIT TESTING
            string wavDirName; string wavFileName;
            AudioRecording recording;
            //WavChooser.ChooseWavFile(out wavDirName, out wavFileName, out recording);      //WARNING! MUST CHOOSE WAV FILE
            WavChooser.DownloadBytesFile(out wavDirName, out wavFileName, out recording);  //WARNING! MUST CHOOSE WAV FILE
            Log.Verbosity = 1;
            //#######################################################################################################

            string appConfigPath = args[0];
            string templateDir = @"C:\SensorNetworks\Templates\Template_" + callID + "\\";
            string templatePath = templateDir + "Template" + callID + ".txt";
          //  string wavPath = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()

            
            //string outputFolder = @"C:\SensorNetworks\Output\";  //default 
            string outputFolder = wavDirName;  //args[2]

            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("CallID        =" + callID);
            Log.WriteIfVerbose("recording Name=" + wavFileName);
            Log.WriteIfVerbose("target   Path =" + outputFolder);




            string serialPath = Path.Combine(templateDir, Path.GetFileNameWithoutExtension(templatePath) + ".serialised");

            //COMMENT OUT OPTION ONE IF A SERIALISED TEMPLATE IS AVAILABLE.
            //OPTION ONE: LOAD TEMPLATE AND SERIALISE
            //var template = BaseTemplate.Load(appConfigPath, templatePath) as Template_CC;
            //Log.WriteLine("\n\nWriting serialised template to file: " + serialPath);
            //var serializedData = QutSensors.Data.Utilities.BinarySerialize(template);
            //Log.WriteLine("\tSerialised byte array: length = " + serializedData.Length + " bytes");
            //FileTools.WriteSerialisedObject(serialPath, serializedData);

            //OPTION TWO: READ SERIALISED TEMPLATE
            Log.WriteLine("\tReading serialised template from file: " + serialPath);
            if (!File.Exists(serialPath)) throw new Exception("SERIALISED FILE DOES NOT EXIST. TERMINATE!");
            BaseTemplate.LoadStaticConfig(appConfigPath);
            var serializedData = FileTools.ReadSerialisedObject(serialPath);
            var template = QutSensors.Shared.Utilities.BinaryDeserialize(serializedData) as Template_CC;




            //LOAD recogniser and scan
            var recogniser = new Recogniser(template as Template_CC); //GET THE TYPE
            var result = recogniser.Analyse(recording);


            string imagePath = Path.Combine(outputFolder, "RESULTS_" + wavFileName + ".png");
            //SAVE RESULTS IMAGE WITHOUT HMM SCORE
            template.SaveResultsImage(recording.GetWavReader(), imagePath, result);
            //INSTEAD OF PREVIOUS LINE USE FOLLOWING LINES WITH ALFREDOS HMM SCORES
            //string hmmPath = Path.Combine(Path.GetDirectoryName(templatePath), "Currawong_HMMScores.txt");
            //List<string> hmmResults = FileTools.ReadTextFile(hmmPath);
            //template.SaveResultsImage(recording.GetWavReader(), imagePath, result, hmmResults);//WITH HMM SCORE



            if (template.LanguageModel.ModelType == LanguageModelType.ONE_PERIODIC_SYLLABLE)
            {
                Log.WriteLine("# Template Hits =" + ((Result_1PS)result).VocalCount);
                Log.Write("# Best Score    =" + ((Result_1PS)result).RankingScoreValue.Value.ToString("F1") + " at ");
                Log.WriteLine(((Result_1PS)result).TimeOfMaxScore.Value.ToString("F1") + " sec");
                Log.WriteLine("# Periodicity   =" + Result_1PS.CallPeriodicity_ms + " ms");
                Log.WriteLine("# Periodic Hits =" + ((Result_1PS)result).NumberOfPeriodicHits);
            } else
            if (template.LanguageModel.ModelType == LanguageModelType.MM_ERGODIC)
            {
                var r2 = result as Result_MMErgodic;
                Log.WriteLine("RESULTS FOR TEMPLATE " + template.config.GetString(template.key_CALL_NAME));
                Log.WriteLine("# Number of vocalisations = " + r2.VocalCount);
                Log.Write("# Best Vocalisation Score    = " + r2.RankingScoreValue.Value.ToString("F1") + " at ");
                Log.WriteLine(r2.TimeOfMaxScore.Value.ToString("F1") + " sec");
            }

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }

    } //end class
}
