﻿using AnalysisBase;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet;
using YamlDotNet.RepresentationModel.Serialization;
using AudioAnalysisTools;
using TowseyLib;


namespace Dong.Felt
{
    public class FeltAnalysis : IAnalyser, IUsage
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string StandardConfigFileName = "Dong.Felt.yml";

        private static readonly string ConfigFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ConfigFiles", StandardConfigFileName);

        public string DisplayName
        {
            get { return "Xueyan Dong's FELT work"; }
        }

        public string Identifier
        {
            get { return "Dong.Felt"; }
        }

        /// <summary>
        /// These are analysis settings
        /// </summary>
        public AnalysisSettings DefaultSettings
        {
            get {
                return new AnalysisSettings();
                }
        }


        /// <summary>
        /// This is the main analysis method.
        /// At this point, there should be no parsing of command line paramenters. This method should be called by the execute method.
        /// </summary>
        /// <param name="analysisSettings"></param>
        /// <returns></returns>
        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            // XUEYAN　－　You should start writing your analysis in here

            // read the config file
            //object settings;
            //using (var reader = new StringReader(analysisSettings.ConfigFile.FullName)) {
            //    //var yaml = new YamlStream();
            //    //yaml.Load(reader);

            //    var serializer = new YamlSerializer();
            //    settings = serializer.Deserialize(reader, new DeserializationOptions() { });
            //}

            // Writing my code here
            // get wav.file path
            string wavFilePath = analysisSettings.SourceFile.FullName;
            //"C:\\Test recordings\\ctest.wav";
            // Read the .wav file
            var recording = new AudioRecording(wavFilePath);
            var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE };
            var spectrogram1 = new SpectralSonogram(config, recording.GetWavReader());

           
            //throw new NotImplementedException();
            var result = new AnalysisResult();
            return result;

        }

        public Tuple<System.Data.DataTable, System.Data.DataTable> ProcessCsvFile(System.IO.FileInfo fiCsvFile, System.IO.FileInfo fiConfigFile)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable ConvertEvents2Indices(System.Data.DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is the (first)entry point, while I am doing developing / testing.
        /// This method should set up any artifitial testing parameters, and then call the execute method. 
        /// </summary>
        /// <param name="arguments"></param>
        public static void Dev(string[] arguments)
        {

            //if (arguments.Length == 0)
            //{
            //    var testDirectory = @"C:\XUEYAN\targetDirectory";
            //    string testConfig = @"C:\XUEYAN\config.yml";
            //    arguments = new[] { testConfig, testDirectory };
            //}
            string date = "# Date and Time:" + DateTime.Now;
            // Log.Info("Read the wav. file and save it as a Spectrogram"); 
            // Log.Info("Read the wav. file path"); // 14/March/2013
            Log.Info("Read the wav. file path");
            Log.Info(date);

            Execute(arguments);

        }

        /// <summary>
        /// This is the (second) main entry point, that my code will use when it is run on a super computer. 
        /// It should take all of the parameters from the arguments parameter.
        /// </summary>
        /// <param name="arguments"></param>
        public static void Execute(string[] arguments)
        {
            if (arguments.Length % 2 != 0)
            {
                throw new Exception("odd number of arguments and values");
            }

            // create a new "analysis"
            var felt = new FeltAnalysis();         
            // merge config settings with analysis settings
            var analysisSettings = felt.DefaultSettings;

            //var configFile = arguments.Skip(arguments.IndexOf("-configFile");
            //if (!File.Exists(ConfigFilePath))
            //{
            //    throw new Exception("Can't find config file");
            //}
            //Log.Info("Using config file: " + ConfigFilePath);
            //analysisSettings.ConfigFile = new FileInfo(ConfigFilePath);

            // get the file path from arguments
            string recordingPath = arguments[1];
            if (!File.Exists(recordingPath))
            {
                throw new Exception("Can't find this recordingfile path: "  + recordingPath);
            }
            analysisSettings.SourceFile = new FileInfo(recordingPath);

            analysisSettings.ConfigDict = new Dictionary<string, string>();
            analysisSettings.ConfigDict["my_custom_setting"] = "hello xueyan";

            var result = felt.Analyse(analysisSettings);

            Log.Info("Finished, yay!");
        }

        public StringBuilder Usage(StringBuilder sb)
        {
            sb.Append("Dong.FELT usage:");
            sb.Append("... dong.felt configurationFile.yml testdirectory");

            return sb;
        }
    }
}