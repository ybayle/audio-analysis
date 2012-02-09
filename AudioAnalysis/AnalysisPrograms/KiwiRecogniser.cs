﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using AudioTools.AudioUtlity;
using QutSensors.Shared.LogProviders;




//Here is link to wiki page containing info about how to write Analysis techniques
//https://wiki.qut.edu.au/display/mquter/Audio+Analysis+Processing+Architecture

//HERE ARE COMMAND LINE ARGUMENTS TO PLACE IN START OPTIONS - PROPERTIES PAGE
//od  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3" C:\SensorNetworks\Output\OD_CaneToad\CaneToad_DetectionParams.txt events.txt
//


namespace AnalysisPrograms
{
    class KiwiRecogniser
    {
        //Following lines are used for the debug command line.
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-female\TOWER_20091107_07200_21.LSK.F.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-male\TOWER_20091112_072000_25.LSK.M.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\Kiwi\Results_MixedTest\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"   "C:\SensorNetworks\WavFiles\Kiwi\Results\lskiwi_Params.txt"
        // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\lskiwi_Params.txt"


        

        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_FILE_EXT        = "FILE_EXT";
        //public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_SEGMENT_DURATION  = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP   = "SEGMENT_OVERLAP";
        
        public static string key_FRAME_LENGTH    = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_MIN_HZ_MALE     = "MIN_HZ_MALE";
        public static string key_MAX_HZ_MALE     = "MAX_HZ_MALE";
        public static string key_MIN_HZ_FEMALE   = "MIN_HZ_FEMALE";
        public static string key_MAX_HZ_FEMALE   = "MAX_HZ_FEMALE";
        public static string key_DCT_DURATION    = "DCT_DURATION";
        public static string key_DCT_THRESHOLD   = "DCT_THRESHOLD";
        public static string key_MIN_PERIODICITY = "MIN_PERIODICITY";
        public static string key_MAX_PERIODICITY = "MAX_PERIODICITY";
        public static string key_MIN_DURATION    = "MIN_DURATION";
        public static string key_MAX_DURATION    = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";
        public static string key_REPORT_FORMAT   = "REPORT_FORMAT";


        /// <summary>
        /// a set of parameters derived from ini file
        /// </summary>
        public struct KiwiParams
        {
            public int frameLength, minHzMale, maxHzMale, minHzFemale, maxHzFemale;
            public double segmentDuration, segmentOverlap; 
            public double frameOverlap, dctDuration, dctThreshold, minPeriodicity, maxPeriodicity, minDuration, maxDuration, eventThreshold;
            public int DRAW_SONOGRAMS;
            public string reportFormat;

            public KiwiParams(double _segmentDuration, double _segmentOverlap, 
                              int _minHzMale, int _maxHzMale, int _minHzFemale, int _maxHzFemale, int _frameLength, int _frameOverlap, double _dctDuration, double _dctThreshold,
                              double _minPeriodicity, double _maxPeriodicity, double _minDuration, double _maxDuration, double _eventThreshold, 
                              int _DRAW_SONOGRAMS, string _fileFormat)
            {
                segmentDuration = _segmentDuration;
                segmentOverlap  = _segmentOverlap;
                minHzMale = _minHzMale;
                maxHzMale = _maxHzMale;
                minHzFemale = _minHzFemale;
                maxHzFemale = _maxHzFemale;
                frameLength = _frameLength;
                frameOverlap = _frameOverlap;
                dctDuration = _dctDuration;
                dctThreshold = _dctThreshold;
                minPeriodicity = _minPeriodicity;
                maxPeriodicity = _maxPeriodicity;
                minDuration    = _minDuration;
                maxDuration    = _maxDuration;
                eventThreshold = _eventThreshold;
                DRAW_SONOGRAMS = _DRAW_SONOGRAMS; //av length of clusters > 1 frame.
                reportFormat   = _fileFormat;
            }
        }





        public static void Dev(string[] args)
        {
            string title = "# SOFTWARE TO DETECT CALLS OF THE LITTLE SPOTTED KIWI (Apteryx owenii)";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //GET COMMAND LINE ARGUMENTS
            Log.Verbosity = 1;
            CheckArguments(args);
            string recordingPath = args[0];
            string iniPath   = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\"; //output directory is the one in which ini file is located.
            Log.WriteIfVerbose("# Output dir: " + outputDir);
                       

            //READ PARAMETER VALUES FROM INI FILE
            KiwiParams kiwiParams = ReadIniFile(iniPath);

            // Get the file time duration
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            var fileInfo = new FileInfo(recordingPath);
            var mimeType = QutSensors.Shared.MediaTypes.GetMediaType(fileInfo.Extension);
            //var dateInfo = fileInfo.CreationTime;
            var duration = audioUtility.Duration(fileInfo, mimeType);
            Log.WriteIfVerbose("# Recording - filename: " + Path.GetFileName(recordingPath));
            Log.WriteIfVerbose("# Recording - datetime: {0}    {1}", fileInfo.CreationTime.ToLongDateString(), fileInfo.CreationTime.ToLongTimeString());
            Log.WriteIfVerbose("# Recording - duration: {0}hr:{1}min:{2}s:{3}ms", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);

            //SET UP THE REPORT FILE
            string reportSeparator = "\t";
            if (kiwiParams.reportFormat.Equals("CSV")) reportSeparator = ",";
            string reportfileName = outputDir + "LSKReport_" + Path.GetFileNameWithoutExtension(recordingPath);
            if (kiwiParams.reportFormat.Equals("CSV")) reportfileName += ".csv";
            else reportfileName += ".txt";
            string line = String.Format("Start{0}Duration{0}__Label__{0}EvStart{0}EvStart{0}EvDur{0}MinHz{0}MaxHz{0}WtScore{0}AvOscRate", reportSeparator);
            FileTools.WriteTextFile(reportfileName, line);


            // LOOP THROUGh THE FILE
            double startMinutes = 0.0;
            int overlap = (int)Math.Floor(kiwiParams.segmentOverlap * 1000);
            for (int s = 0; s < Int32.MaxValue; s++)
            {
                Console.WriteLine();
                Log.WriteLine("## SAMPLE {0}:-   starts@ {1} minutes", s, startMinutes);

                AudioRecording recording = GetSegmentFromAudioRecording(recordingPath, startMinutes, kiwiParams.segmentDuration, overlap, outputDir);
                string segmentDuration = DataTools.Time_ConvertSecs2Mins(recording.GetWavReader().Time.TotalSeconds);
                //Log.WriteLine("Signal Duration: " + segmentDuration);
                int sampleCount = recording.GetWavReader().Samples.Length;
                int minLength = 3 * kiwiParams.frameLength;
                if (sampleCount <= minLength)
                {
                    Log.WriteLine("# WARNING: Recording is less than {0} samples (three frames) long. Will ignore.", sampleCount);
                    //Console.ReadLine();
                    //System.Environment.Exit(666);
                    break;
                }

                //#############################################################################################################################################
                var results = Execute_KiwiDetect(recording, kiwiParams.minHzMale, kiwiParams.maxHzMale, kiwiParams.minHzFemale, kiwiParams.maxHzFemale,
                                                 kiwiParams.frameLength, kiwiParams.frameOverlap,
                                                 kiwiParams.dctDuration, kiwiParams.dctThreshold, kiwiParams.minPeriodicity, kiwiParams.maxPeriodicity,
                                                 kiwiParams.eventThreshold, kiwiParams.minDuration, kiwiParams.maxDuration);
                //#############################################################################################################################################

                recording.Dispose();
                var sonogram = results.Item1;
                var hits = results.Item2;
                var scores = results.Item3;
                var oscRates = results.Item4;
                var predictedEvents = results.Item5;
                Log.WriteLine("# Event count = " + predictedEvents.Count());

                //write events to results file. 
                double sigDuration = sonogram.Duration.TotalSeconds;
                string fname = Path.GetFileName(recordingPath);
                int count = predictedEvents.Count;

                StringBuilder sb = KiwiRecogniser.WriteEvents(startMinutes, sigDuration, count, predictedEvents, reportSeparator);
                if (sb.Length > 1)
                {
                    sb.Remove(sb.Length - 2, 2); //remove the last endLine to prevent line gaps.
                    FileTools.Append2TextFile(reportfileName, sb.ToString());
                }


                //draw images of sonograms
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_" + startMinutes.ToString() + "min.png";
                if (kiwiParams.DRAW_SONOGRAMS == 2)
                {
                    DrawSonogram(sonogram, imagePath, hits, scores, oscRates, predictedEvents, kiwiParams.eventThreshold);
                }
                else
                    if ((kiwiParams.DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))
                    {
                        DrawSonogram(sonogram, imagePath, hits, scores, oscRates, predictedEvents, kiwiParams.eventThreshold);
                    }


                startMinutes += kiwiParams.segmentDuration;
            } //end of for loop

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        public static KiwiParams ReadIniFile(string iniPath)
        {
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            KiwiParams kiwiParams; // st
            kiwiParams.segmentDuration = Double.Parse(dict[key_SEGMENT_DURATION]);
            kiwiParams.segmentOverlap  = Double.Parse(dict[key_SEGMENT_OVERLAP]);
            kiwiParams.minHzMale = Int32.Parse(dict[key_MIN_HZ_MALE]);
            kiwiParams.maxHzMale = Int32.Parse(dict[key_MAX_HZ_MALE]);
            kiwiParams.minHzFemale = Int32.Parse(dict[key_MIN_HZ_FEMALE]);
            kiwiParams.maxHzFemale = Int32.Parse(dict[key_MAX_HZ_FEMALE]);
            kiwiParams.frameLength = Int32.Parse(dict[key_FRAME_LENGTH]);
            kiwiParams.frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            kiwiParams.dctDuration = Double.Parse(dict[key_DCT_DURATION]);        //duration of DCT in seconds 
            kiwiParams.dctThreshold = Double.Parse(dict[key_DCT_THRESHOLD]);      //minimum acceptable value of a DCT coefficient
            kiwiParams.minPeriodicity = Double.Parse(dict[key_MIN_PERIODICITY]);  //ignore oscillations with period below this threshold
            kiwiParams.maxPeriodicity = Double.Parse(dict[key_MAX_PERIODICITY]);  //ignore oscillations with period above this threshold
            kiwiParams.minDuration = Double.Parse(dict[key_MIN_DURATION]);        //min duration of event in seconds 
            kiwiParams.maxDuration = Double.Parse(dict[key_MAX_DURATION]);        //max duration of event in seconds 
            kiwiParams.eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);  //min score for an acceptable event
            kiwiParams.DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);    //options to draw sonogram
            kiwiParams.reportFormat   = dict[key_REPORT_FORMAT];                  //options are TAB or COMMA separator 

            Log.WriteIfVerbose("# PARAMETER SETTINGS:");
            Log.WriteIfVerbose("Segment size: Duration = {0} minutes;  Overlap = {1} seconds.", kiwiParams.segmentDuration, kiwiParams.segmentOverlap);
            Log.WriteIfVerbose("Male   Freq band: {0} Hz - {1} Hz.)", kiwiParams.minHzMale, kiwiParams.maxHzMale);
            Log.WriteIfVerbose("Female Freq band: {0} Hz - {1} Hz.)", kiwiParams.minHzFemale, kiwiParams.maxHzFemale);
            Log.WriteIfVerbose("Periodicity bounds: {0:f1}sec - {1:f1}sec", kiwiParams.minPeriodicity, kiwiParams.maxPeriodicity);
            Log.WriteIfVerbose("minAmplitude = " + kiwiParams.dctThreshold);
            Log.WriteIfVerbose("Duration bounds: " + kiwiParams.minDuration + " - " + kiwiParams.maxDuration + " seconds");
            Log.WriteIfVerbose("####################################################################################");
            //Log.WriteIfVerbose("Male   Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHzMale, maxHzMale, binCount_male);
            //Log.WriteIfVerbose("Female Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHzFemale, maxHzFemale, binCount_female);
            //Log.WriteIfVerbose("DctDuration=" + dctDuration + "sec.  (# frames=" + (int)Math.Round(dctDuration * sonogram.FramesPerSecond) + ")");
            //Log.WriteIfVerbose("Score threshold for oscil events=" + eventThreshold);
            return kiwiParams;
        }



        public static AudioRecording GetAudioRecording(string recordingPath)
        {
            //OLD CODE
            //AudioRecording recording = new AudioRecording(recordingPath);
            //if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            audioUtility.SoxAudioUtility.ResampleQuality = SoxAudioUtility.SoxResampleQuality.VeryHigh; //Options: Low, Medium, High, VeryHigh 
            audioUtility.SoxAudioUtility.TargetSampleRateHz = 17640;
            audioUtility.SoxAudioUtility.ReduceToMono = true;
            audioUtility.SoxAudioUtility.UseSteepFilter = true;
            //##### ######  IMPORTANT NOTE 1 :: THE EFFECT OF THE ABOVE RESAMPLING PARAMETERS IS TO SET NYQUIST = SAMPLERATE / 2 Hz.
            //##### ######  IMPORTANT NOTE 2 :: THE RESULTING SIGNAL ARRAY VARIES SLIGHTLY FOR EVERY LOADING - NOT SURE WHY? A STOCHASTOIC COMPONENT TO FILTER? 
            //##### ######                               BUT IT HAS THE EFFECT THAT STATISTICS VARY SLIGHTLY FOR EACH RUN OVER THE SAME FILE.
            audioUtility.LogLevel = LogType.Error;  //Options: None, Fatal, Error, Debug, 
            AudioRecording recording = new AudioRecording(recordingPath, audioUtility);

            return recording;
        }

        public static AudioRecording GetSegmentFromAudioRecording(string recordingPath, double startMinutes, double durationMinutes, int overlap, string opDir)
        {
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            audioUtility.SoxAudioUtility.ResampleQuality = SoxAudioUtility.SoxResampleQuality.VeryHigh; //Options: Low, Medium, High, VeryHigh 
            audioUtility.SoxAudioUtility.TargetSampleRateHz = 17640;
            audioUtility.SoxAudioUtility.ReduceToMono = true;
            audioUtility.SoxAudioUtility.UseSteepFilter = true;
            //##### ######  IMPORTANT NOTE 1 :: THE EFFECT OF THE ABOVE RESAMPLING PARAMETERS IS TO SET NYQUIST = SAMPLERATE / 2 Hz.
            //##### ######  IMPORTANT NOTE 2 :: THE RESULTING SIGNAL ARRAY VARIES SLIGHTLY FOR EVERY LOADING - NOT SURE WHY? A STOCHASTOIC COMPONENT TO FILTER? 
            //##### ######                               BUT IT HAS THE EFFECT THAT STATISTICS VARY SLIGHTLY FOR EACH RUN OVER THE SAME FILE.
            audioUtility.LogLevel = LogType.Error;  //Options: None, Fatal, Error, Debug, 

            FileInfo inFile = new FileInfo(recordingPath);
            //FileInfo outFile = new FileInfo(@"C:\SensorNetworks\WavFiles\Kiwi\Samples\test.wav");
            FileInfo outFile = new FileInfo(opDir + @"temp.wav");
            int startMilliseconds = (int)(startMinutes * 60000);
            int endMilliseconds   = startMilliseconds + (int)(durationMinutes * 60000) + overlap;

            SpecificWavAudioUtility.GetSingleSegment(audioUtility, inFile, outFile, startMilliseconds, endMilliseconds);
            AudioRecording recording = new AudioRecording(outFile.FullName, audioUtility);

            return recording;
        }

        public static System.Tuple<BaseSonogram, Double[,], double[], double[], List<AcousticEvent>> Execute_KiwiDetect(AudioRecording recording, 
            int minHzMale, int maxHzMale, int minHzFemale, int maxHzFemale, int frameLength, double frameOverlap, double dctDuration, double dctThreshold,
            double minPeriodicity, double maxPeriodicity, double eventThreshold, double minDuration, double maxDuration)
        {
            //i: MAKE SONOGRAM
            //Log.WriteLine("Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName    = recording.FileName;
            sonoConfig.WindowSize     = frameLength;
            sonoConfig.WindowOverlap  = frameOverlap;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            //Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, recording.SampleRate);
            //Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
            //                           sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
            //                          (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            //int binCount_male   = (int)(maxHzMale   / sonogram.FBinWidth) - (int)(minHzMale   / sonogram.FBinWidth) + 1;
            //int binCount_female = (int)(maxHzFemale / sonogram.FBinWidth) - (int)(minHzFemale / sonogram.FBinWidth) + 1;
            //Log.WriteLine("Start oscillation detection");

            //iii: DETECT OSCILLATIONS
            bool normaliseDCT = true;
            double minOscilFreq = 1 / maxPeriodicity;  //convert max period (seconds) to oscilation rate (Herz).
            double maxOscilFreq = 1 / minPeriodicity;  //convert min period (seconds) to oscilation rate (Herz).

            //ii: CHECK FOR MALE KIWIS
            List<AcousticEvent> predictedMaleEvents;  //predefinition of results event list
            Double[,] maleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] maleScores;                      //predefinition of score array
            double[] maleOscRate;
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHzMale, maxHzMale, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out maleScores, out predictedMaleEvents, out maleHits, out maleOscRate);
            ProcessKiwiEvents(predictedMaleEvents, "Male LSK", maleOscRate, minDuration, maxDuration);
            //int gapThreshold = (int)Math.Round(dctDuration);     //merge events that are closer than duration of DCT
            //AcousticEvent.MergeAdjacentEvents(predictedMaleEvents, gapThreshold);

            //iii: CHECK FOR FEMALE KIWIS
            Double[,] femaleHits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] femaleScores;                      //predefinition of score array
            double[] femaleOscRate;
            List<AcousticEvent> predictedFemaleEvents;  //predefinition of results event list
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, minHzFemale, maxHzFemale, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out femaleScores, out predictedFemaleEvents, out femaleHits, out femaleOscRate);
            ProcessKiwiEvents(predictedFemaleEvents, "Female LSK", femaleOscRate, minDuration, maxDuration);
            //AcousticEvent.MergeAdjacentEvents(predictedFemaleEvents, gapThreshold);


            //iv: MERGE MALE AND FEMALE INFO
            foreach (AcousticEvent ae in predictedFemaleEvents) predictedMaleEvents.Add(ae);
            // Merge the male and female hit matrices. Each hit matrix shows where there is an oscillation of sufficient amplitude in the correct range.
            // Values in the matrix are the oscillation rate. i.e. if OR = 2.0 = 2 oscillations per second. </param>
            Double[,] hits = DataTools.AddMatrices(maleHits, femaleHits);
            //merge the two score arrays
            for (int i = 0; i < maleScores.Length; i++) if (femaleScores[i] > maleScores[i]) maleScores[i] = femaleScores[i];
            //merge the two oscillation rate arrays
            //for (int i = 0; i < maleOscRate.Length; i++) if (femaleOscRate[i] > maleOscRate[i]) maleOscRate[i] = femaleOscRate[i];

            return System.Tuple.Create(sonogram, hits, maleScores, maleOscRate, predictedMaleEvents);

        }//end Execute_KiwiDetect()


        public static void ProcessKiwiEvents(List<AcousticEvent> events, string tag, double[] oscRate, double minDuration, double maxDuration)
        {
            foreach (AcousticEvent ae in events)
            {
                ae.Name = tag;
                int eventLength = (int)(ae.Duration * ae.FramesPerSecond);
                //int objHt = ae.oblong.RowWidth;
                //Console.WriteLine("{0}    {1} = {2}-{3}", eventLength, objHt, ae.oblong.r2, ae.oblong.r1);

                //calculate score for duration. Value lies in [0,1]. Shape the ends.
                double durationScore = 1.0;
                if (ae.Duration < minDuration +  5) durationScore = (ae.Duration - minDuration) / 5;
                else
                if (ae.Duration > maxDuration - 10) durationScore = (maxDuration - ae.Duration) / 10;

                // %hit score = ae.
                double hitScore = ae.Score;


                //calculate score for change in oscillation over the event.
                int onetenth = eventLength / 10;
                int onefifth = eventLength / 5; 
                int sevenTenth = eventLength * 7 / 10;
                int startOffset = ae.oblong.r1 + onetenth;
                int endOffset = ae.oblong.r1 + sevenTenth;
                double startISD = 0; //Inter-Syllable Distance in seconds
                double endISD   = 0; //Inter-Syllable Distance in seconds
                for (int i = 0; i < onefifth; i++)
                {
                    startISD += (1/oscRate[startOffset + i]);
                    endISD   += (1/oscRate[endOffset + i]);
                }
                double deltaISD = (endISD - startISD) / onefifth; //get average
                double deltaScore = 1.0;
                if (deltaISD < -0.1) deltaScore = 0.0;
                else
                if (deltaISD <  0.2) deltaScore = (3.3333 * deltaISD)+0.3333;  //y=mx+c where c=0.333 and m=3.333

                ae.Score = (durationScore + hitScore + deltaScore) / 3;
                ae.ScoreNormalised = ae.Score;
            }
        }



        public static StringBuilder WriteEvents(double segmentStart, double segmentDuration, int eventCount, List<AcousticEvent> eventList, string separator)
        {
            string duration = DataTools.Time_ConvertSecs2Mins(segmentDuration);
            StringBuilder sb = new StringBuilder();
            if (eventList.Count == 0)
            {
                //string line = String.Format("{1}{0}{2,8:f3}{0}0{0}N/A{0}N/A{0}N/A{0}N/A{0}N/A{0}0{0}0",
                //                     separator, segmentStart, duration);
                //sb.AppendLine(line);
            }
            else
            {
                foreach (AcousticEvent ae in eventList)
                {
                    int startSec = (int)((segmentStart * 60) + ae.StartTime);
                    string line = String.Format("{1}{0}{2,8:f3}{0}{3}{0}{4:f2}{0}{5}{0}{6:f1}{0}{7}{0}{8}{0}{9:f3}{0}{10:f3}",
                                         separator, segmentStart, duration, ae.Name, ae.StartTime, startSec, ae.Duration, ae.MinFreq, ae.MaxFreq, ae.Score, ae.Score2);
                    sb.AppendLine(line);
                }
            }
            return sb;
        }



        public static void DrawSonogram(BaseSonogram sonogram, string path, double[,] hits, double[] scores, double[] oscillationRates,
                                        List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            //Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                image.AddTrack(Image_Track.GetScoreTrack(oscillationRates, 0.5, 1.5, 1.0));
                double maxScore = 16.0;
                image.AddSuperimposedMatrix(hits, maxScore);
                image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }


        public static void CheckArguments(string[] args)
        {
            if (args.Length != 2)
            {
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", 2);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of the two files whose paths are expected as first two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        public static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("KiwiDetect.exe recordingPath iniPath outputFileName");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) The path of the audio file to be processed.");
            Console.WriteLine("iniPath:-          (string) The path of the ini file containing all required parameters.");
            Console.WriteLine();
            Console.WriteLine("NOTE: By default, the output dir is that containing the ini file.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }

    } //end class
}
