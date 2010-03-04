﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms
{
    class MainEntry
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: YOU HAVE CALLED THE AanalysisPrograms.MainEntry() method without command line arguments.");
            }
            else
            {
                string[] r = args.Skip(1).ToArray();
                switch (args[0])
                {
                    case "aed":  //acoustic event detection
                        AED.Dev(r);
                        break;
                    case "od":   //Oscillation Recogniser
                        OscillationRecogniser.Dev(r);
                        break;
                    case "groundparrot":
                        GroundParrotRecogniser.Dev(r); //event pattern recognition
                        break;
                    case "snr":   //signal to noise ratio
                        SnrAnalysis.Main(r);
                        break;
                    case "fpfnAnalysis":   //calculate false pos false neg percents
                        SnrAnalysis.Main(r);
                        break;
                    default:
                        Console.WriteLine("unrecognised");
                        break;
                }
            }
        }
    }
}
