﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexMatrices.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the IndexMatrices type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;

    using log4net;

    using TowseyLibrary;

    public static class IndexMatrices
    {
        public static bool Verbose;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
/*
        public static void test()
        {
            var matrix = Csv.ReadMatrixFromCsv<double>(new FileInfo(""));
        }
*/
/*
        public static Dictionary<string, double[]> ConvertCsvData2DictionaryOfColumns(string[] headers, double[,] M)
        {
            Dictionary<string, double[]> dictionaryOfCsvDataColumns = new Dictionary<string, double[]>();
            for(int i = 0; i < headers.Length; i++)
            {
                dictionaryOfCsvDataColumns.Add(headers[i], MatrixTools.GetColumn(M, i));
            }
            return dictionaryOfCsvDataColumns;
        }
*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files">All the passed files will be concatenated. Filtering needs to be done somewhere else.</param>
        /// <param name="indexCalcDuration">used to match rows of indices to elapsed time in file names</param>
        /// <returns></returns>
        public static List<SummaryIndexValues> ConcatenateSummaryIndexFilesWithTimeCheck(FileInfo[] files, TimeSpan indexCalcDuration)
        {
            TimeSpan? offsetHint = new TimeSpan(10, 0, 0);
            DateTimeOffset[] dtoArray = new DateTimeOffset[files.Length];
            var summaryIndices = new List<SummaryIndexValues>();

            // accumulate the start times for each of the files
            for (int f = 0; f < files.Length; f++)
            {
                if (!files[f].Exists)
                {
                    if (IndexMatrices.Verbose)
                    {
                        LoggedConsole.WriteWarnLine("WARNING: from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + files[f].Extension + ") ");
                        LoggedConsole.WriteWarnLine(string.Format("   MISSING FILE: {0}", files[f].FullName));
                    }
                    continue;
                }

                DateTimeOffset startDto;
                if (!FileDateHelpers.FileNameContainsDateTime(files[f].Name, out startDto, offsetHint))
                {
                    LoggedConsole.WriteWarnLine("WARNING from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + files[f].Name + ") ");
                    LoggedConsole.WriteWarnLine("  File name <{0}> does not contain a valid DateTime = {0}", files[f].Name);
                }

                dtoArray[f] = startDto;
            }

            // now loop through the files again to extact the indices
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Exists)
                {
                    continue;
                }

                var rowsOfCsvFile = Csv.ReadFromCsv<SummaryIndexValues>(files[i], throwOnMissingField: false);
                summaryIndices.AddRange(rowsOfCsvFile);
                // track the row counts
                int partialRowCount = rowsOfCsvFile.Count();
                // calculate elapsed time from the rows
                int partialRowMinutes = (int)Math.Round(partialRowCount * indexCalcDuration.TotalMinutes);

                //track the elapsed minutes
                // calculate the partial elapsed time indexed by file names.
                var partialMinutes = 0;
                if (i < files.Length - 1)
                {
                    TimeSpan partialElapsedTime = dtoArray[i + 1] - dtoArray[i];
                    partialMinutes = (int)Math.Round(partialElapsedTime.TotalMinutes);
                }
                else
                {
                    partialMinutes = partialRowMinutes; // a hack for the last file
                }

                if (partialRowMinutes != partialMinutes)
                {
                    if (IndexMatrices.Verbose)
                    {
                        LoggedConsole.WriteWarnLine("WARNING from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck(" + files[i].Name + ") ");
                        string str1 = $"    Mismatch in csvFile {i + 1}/{files.Length} between rows added and elapsed time according to file names.";
                        LoggedConsole.WriteWarnLine(str1);
                        string str2 = $"    Row Count={partialRowMinutes} != {partialMinutes} elapsed minutes";
                        LoggedConsole.WriteWarnLine(str2);
                    }
                    //dictionary = RepairDictionaryOfArrays(dictionary, rowCounts[i], partialMinutes);
                    for (int j = partialRowMinutes; j < partialMinutes; j++)
                    {
                        summaryIndices.Add(new SummaryIndexValues());
                    }

                }

            }

            // Can prune the list of summary indices as required.
            //int expectedRowCount = (int)Math.Round(numberOfMinutesInDay / indexCalcDuration.TotalMinutes);
            //if (totalRowCount != expectedRowCount)
            //{
            //    if (IndexMatrices.Verbose)
            //        LoggedConsole.WriteLine("WARNING: INCONSISTENT ELAPSED TIME CHECK from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck() ");
            //    string str = String.Format("   Final Data Row Count = {0}     Estimated Cumulative Duration = {1} minutes", totalRowCount, expectedRowCount);
            //    if (IndexMatrices.Verbose)
            //        LoggedConsole.WriteLine(str);
            //    dictionary = RepairDictionaryOfArrays(dictionary, totalRowCount, expectedRowCount);
            //}

            return summaryIndices;
        }




        /// <summary>
        /// WARNING: THIS METHOD ONLY GETS FIXED LIST OF INDICES.
        /// </summary>
        /// <param name="summaryIndices"></param>
        /// <returns></returns>
        public static Dictionary<string, double[]> GetDictionaryOfSummaryIndices(List<SummaryIndexValues> summaryIndices)
        {
            var dictionary = new Dictionary<string, double[]>();
            dictionary.Add("ClippingIndex", summaryIndices.Select(x => x.ClippingIndex).ToArray());
            //dictionary.Add("AvgSignalAmplitude", summaryIndices.Select(x => x.AcousticComplexity).ToArray());
            dictionary.Add("BackgroundNoise", summaryIndices.Select(x => x.BackgroundNoise).ToArray());
            dictionary.Add("Snr", summaryIndices.Select(x => x.Snr).ToArray());
            dictionary.Add("EventsPerSecond", summaryIndices.Select(x => x.EventsPerSecond).ToArray());
            dictionary.Add("Activity", summaryIndices.Select(x => x.Activity).ToArray());
            dictionary.Add("HighFreqCover", summaryIndices.Select(x => x.HighFreqCover).ToArray());
            dictionary.Add("MidFreqCover", summaryIndices.Select(x => x.MidFreqCover).ToArray());
            dictionary.Add("LowFreqCover", summaryIndices.Select(x => x.LowFreqCover).ToArray());
            dictionary.Add("TemporalEntropy", summaryIndices.Select(x => x.TemporalEntropy).ToArray());
            dictionary.Add("EntropyOfAverageSpectrum", summaryIndices.Select(x => x.EntropyOfAverageSpectrum).ToArray());
            dictionary.Add("EntropyOfPeaksSpectrum", summaryIndices.Select(x => x.EntropyOfPeaksSpectrum).ToArray());
            dictionary.Add("AcousticComplexity", summaryIndices.Select(x => x.AcousticComplexity).ToArray());
            dictionary.Add("ClusterCount", summaryIndices.Select(x => x.ClusterCount).ToArray());
            dictionary.Add("ThreeGramCount", summaryIndices.Select(x => x.ThreeGramCount).ToArray());

            return dictionary;
        }

/*
        public static Dictionary<string, double[]> RepairDictionaryOfArrays(Dictionary<string, double[]> dictionary, int rowCount, int requiredCount)
        {
            int differenceCount = Math.Abs(rowCount - requiredCount);
            if (rowCount > requiredCount)
            {
                if (IndexMatrices.Verbose)
                {
                    LoggedConsole.WriteLine("  About to remove end {0} rows", differenceCount);
                }
                int countToRemove = rowCount - requiredCount;
                dictionary = RemoveValuesFromArraysInDictionary(dictionary, countToRemove);
            };

            if (rowCount < requiredCount)
            {
                if (IndexMatrices.Verbose)
                {
                    LoggedConsole.WriteLine("  About to append {0} rows", differenceCount);
                }
                int countToAdd = requiredCount - rowCount;
                dictionary = PadDictionaryArraysWithNulls(dictionary, countToAdd);
            };
            return dictionary;
        }
*/

        static T[,] CreateRectangularArray<T>(IList<T[]> arrays)
        {
            // TODO: Validation and special-casing for arrays.Count == 0
            int minorLength = arrays[0].Length;
            T[,] ret = new T[arrays.Count, minorLength];
            for (int i = 0; i < arrays.Count; i++)
            {
                var array = arrays[i];
                if (array.Length != minorLength)
                {
                    throw new ArgumentException
                        ("All arrays must be the same length");
                }
                for (int j = 0; j < minorLength; j++)
                {
                    ret[i, j] = array[j];
                }
            }
            return ret;
        }

        static T[,] CreateRectangularArrayFromListOfColumnArrays<T>(IList<T[]> arrays)
        {
            // TODO: Validation and special-casing for arrays.Count == 0
            int rowCount = arrays[0].Length;
            int colCount = arrays.Count;
            T[,] ret = new T[rowCount, colCount];
            for (int c = 0; c < colCount; c++)
            {
                var array = arrays[c];
                if (array.Length != rowCount)
                {
                    throw new ArgumentException
                        ("All arrays must be the same length");
                }
                for (int r = 0; r < rowCount; r++)
                {
                    ret[r, c] = array[r];
                }
            }
            return ret;
        }


        public static Dictionary<string, double[,]> GetSpectralIndexFilesAndConcatenate(DirectoryInfo[] dirs, 
                                                                                        string analysisType, 
                                                                                        string[] keys,
                                                                                        IndexGenerationData indexGenerationData,
                                                                                        bool verbose = false)
        {
            IndexMatrices.Verbose = verbose;
            TimeSpan indexCalcTimeSpan = indexGenerationData.IndexCalculationDuration;
            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();

            foreach (string key in keys)
            {
                DateTime now1 = DateTime.Now;
                string pattern = "*__" + analysisType + "." + key + ".csv";
                var files = IndexMatrices.GetFilesInDirectories(dirs, pattern);

                if (files.Length == 0)
                {
                    LoggedConsole.WriteWarnLine("WARNING: No csv files found for KEY="+key);
                    continue;
                }

                List<double[,]> matrices = IndexMatrices.ConcatenateSpectralIndexFilesWithTimeCheck(files, indexCalcTimeSpan);
                double[,] m = MatrixTools.ConcatenateMatrixRows(matrices);
                //Dictionary<string, double[,]> dict = spectralIndexValues.ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.ColumnMajorFlipped);

                m = MatrixTools.MatrixRotate90Anticlockwise(m);
                spectrogramMatrices.Add(key, m);

                var now2 = DateTime.Now;
                TimeSpan et = now2 - now1;
                if (verbose)
                {
                    LoggedConsole.WriteLine(string.Format("Time to read <{0}> spectral index files = {1:f2} seconds", key, et.TotalSeconds));
                }
            }
            return spectrogramMatrices;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="files">All the passed files will be concatenated. Filtering needs to be done somewhere else.</param>
        /// <param name="indexCalcDuration">used to match rows of indices to elapsed time in file names</param>
        /// <returns></returns>
        public static List<double[,]> ConcatenateSpectralIndexFilesWithTimeCheck(FileInfo[] files, TimeSpan indexCalcDuration)
        {
            TimeSpan? offsetHint = new TimeSpan(10, 0, 0);
            DateTimeOffset[] dtoArray = new DateTimeOffset[files.Length];
            var matrices = new List<double[,]>();

            // accumulate the start times for each of the files
            for (int f = 0; f < files.Length; f++)
            {
                if (!files[f].Exists)
                {
                    if (IndexMatrices.Verbose)
                    {
                        LoggedConsole.WriteWarnLine("WARNING: from IndexMatrices.ConcatenateSpectralIndexFilesWithTimeCheck(" + files[f].Extension + ") ");
                        LoggedConsole.WriteWarnLine(string.Format("   MISSING FILE: {0}", files[f].FullName));
                    }
                    continue;
                }

                DateTimeOffset startDto;
                if (!FileDateHelpers.FileNameContainsDateTime(files[f].Name, out startDto, offsetHint))
                {
                    LoggedConsole.WriteWarnLine("WARNING from IndexMatrices.ConcatenateSpectralIndexFilesWithTimeCheck(" + files[f].Name + ") ");
                    LoggedConsole.WriteWarnLine("  File name <{0}> does not contain a valid DateTime = {0}", files[f].Name);
                }

                dtoArray[f] = startDto;
            }

            string fileName = files[0].Name;
            string fileExt = fileName.Substring(fileName.Length - 7);

            // now loop through the files again to extract the indices
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Exists)
                {
                    continue;
                }

                var matrix = Csv.ReadMatrixFromCsv<double>(files[i], TwoDimensionalArray.Normal);
                matrices.Add(matrix);

                // track the row counts
                int partialRowCount = matrix.GetLength(0);
                // calculate elapsed time from the rows
                int partialRowMinutes = (int)Math.Round(partialRowCount * indexCalcDuration.TotalMinutes);

                //track the elapsed minutes
                // calculate the partial elapsed time indexed by file names.
                var partialMinutes = 0;
                if (i < files.Length - 1)
                {
                    TimeSpan partialElapsedTime = dtoArray[i + 1] - dtoArray[i];
                    partialMinutes = (int)Math.Round(partialElapsedTime.TotalMinutes);
                }
                else
                {
                    partialMinutes = partialRowMinutes; // a hack for the last file
                }

                if (partialRowMinutes != partialMinutes)
                {
                    if (IndexMatrices.Verbose)
                    {
                        LoggedConsole.WriteWarnLine("WARNING from IndexMatrices.ConcatenateSpectralIndexFilesWithTimeCheck(" + files[i].Name + ") ");
                        string str1 = $"    Mismatch in csvFile {i + 1}/{files.Length} between rows added and elapsed time according to file names.";
                        LoggedConsole.WriteWarnLine(str1);
                        string str2 = $"    Row Count={partialRowMinutes} != {partialMinutes} elapsed minutes";
                        LoggedConsole.WriteWarnLine(str2);
                    }

                    int missingRowCount = partialMinutes - partialRowMinutes;
                    int columnCount = matrices[0].GetLength(1);
                    var emptyMatrix = new double[missingRowCount, columnCount];
                    if (fileExt.StartsWith("BGN"))
                    {
                        for (int r = 0; r < missingRowCount; r++)
                        {
                            for (int c = 0; c < columnCount; c++)
                            {   // init with low decibel value
                                emptyMatrix[r, c] = -100.0;
                            }
                        }
                    }
                    matrices.Add(emptyMatrix);
                }

            }

            // Can prune the list of summary indices as required.
            //int expectedRowCount = (int)Math.Round(numberOfMinutesInDay / indexCalcDuration.TotalMinutes);
            //if (totalRowCount != expectedRowCount)
            //{
            //    if (IndexMatrices.Verbose)
            //        LoggedConsole.WriteLine("WARNING: INCONSISTENT ELAPSED TIME CHECK from IndexMatrices.GetSummaryIndexFilesAndConcatenateWithTimeCheck() ");
            //    string str = String.Format("   Final Data Row Count = {0}     Estimated Cumulative Duration = {1} minutes", totalRowCount, expectedRowCount);
            //    if (IndexMatrices.Verbose)
            //        LoggedConsole.WriteLine(str);
            //    dictionary = RepairDictionaryOfArrays(dictionary, totalRowCount, expectedRowCount);
            //}

            return matrices;
        }

/*
        /// <summary>
        /// WARNING: THIS METHOD ONLY GETS FIXED LIST OF INDICES.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, double[,]> GetMatrixOfSpectralIndices(List<AnalysisBase.ResultBases.SpectralIndexBase> spectralIndices)
        {
            var d = spectralIndices.ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.ColumnMajorFlipped);
            return d;
        }
*/


        public static FileInfo[] GetFilesInDirectory(string path, string pattern)
        {
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                var directoryNotFoundException = new DirectoryNotFoundException(path);
                LoggedConsole.WriteFatalLine("DIRECTORY DOES NOT EXIST", directoryNotFoundException);
                throw directoryNotFoundException;
            }

            FileInfo[] files = dirInfo.GetFiles(pattern, SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                LoggedConsole.WriteErrorLine("No match - Empty list of files");
            }

            Array.Sort(files, (f1, f2) => f1.Name.CompareTo(f2.Name));

            return files;
        }


        /// <summary>
        /// Returns a sorted list of file paths, sorted on file name.
        /// IMPORTANT: Sorts on alphanumerics, NOT on date or time encoded in the file name. 
        /// </summary>
        /// <param name="directories"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static FileInfo[] GetFilesInDirectories(DirectoryInfo[] directories, string pattern)
        {
            var fileList = new List<FileInfo>();

            foreach (var dir in directories)
            {
                if (!dir.Exists)
                {
                    var directoryNotFoundException = new DirectoryNotFoundException(dir.FullName);
                    LoggedConsole.WriteFatalLine("DIRECTORY DOES NOT EXIST", directoryNotFoundException);
                    throw directoryNotFoundException;
                }
                
                //FileInfo[] files = dir.GetFiles(pattern, SearchOption.TopDirectoryOnly);
                var files = dir.GetFiles(pattern, SearchOption.AllDirectories);
                fileList.AddRange(files);
            }

            if (fileList.Count == 0)
            {
                LoggedConsole.WriteErrorLine("No match - Empty list of files");
            }

            FileInfo[] returnList = fileList.ToArray();
            Array.Sort(returnList, (f1, f2) => f1.Name.CompareTo(f2.Name));

            return returnList;
        }

/*
        public static double[,] ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(FileInfo[] paths, TimeSpan indexCalcDuration)
        {
            TimeSpan? offsetHint = new TimeSpan(10, 0, 0);
            var list = new List<double[,]>();
            int[] matrixRowCounts = new int[paths.Length];
            DateTimeOffset[] startDto = new DateTimeOffset[paths.Length];

            // cycle through the files
            for (int i = 0; i < paths.Length; i++)
            {
                FileInfo file = paths[i];
                if (file.Exists)
                {
                    int freqBinCount;
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out freqBinCount);
                    list.Add(matrix);
                    matrixRowCounts[i] = matrix.GetLength(0);
                    DateTimeOffset dto;
                    if (!FileDateHelpers.FileNameContainsDateTime(file.Name, out dto, offsetHint))
                    {
                        LoggedConsole.WriteLine("SERIOUS WARNING from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(" + file.Name + ") ");
                        LoggedConsole.WriteLine("  File name <{0}> does not contain a valid DateTime = {0}", file.Name);
                        LoggedConsole.WriteLine("  You should not call this method. Instead try method GetSpectralIndexFilesAndConcatenate()");
                        LoggedConsole.WriteLine("                 which does not require valid DateTime to be in file name.");
                    }
                    startDto[i] = dto;
                }
                else
                {
                    LoggedConsole.WriteLine("SERIOUS WARNING: from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck(" + file.Extension + ") ");
                    string str = $"   MISSING CSV FILE: {file.FullName}";
                    LoggedConsole.WriteLine(str);
                }
            }


            int cummulativeRowCount = 0;
            // cycle through the time stamps and row counts files
            for (int i = 0; i < paths.Length-1; i++)
            {
                // now calculate elapsed time from the file name TimeStamps
                TimeSpan partialElapsedTime = startDto[i+1] - startDto[i];
                int partialTimeSpanMinutes = (int)Math.Round(partialElapsedTime.TotalMinutes);
                //int partialTimeSpanMinutes = (int)Math.Ceiling(partialElapsedTime.TotalMinutes);

                if (matrixRowCounts[i] != partialTimeSpanMinutes)
                {
                    string str = String.Format("  Matrix {0}/{1}: Row Count={2} does not tally with Elapsed Minutes in File Name = {3} minutes", 
                                                    (i+1), paths.Length, matrixRowCounts[i], partialTimeSpanMinutes);
                    LoggedConsole.WriteLine(str);
                    LoggedConsole.WriteLine("  About to append/remove rows to get correct partial length = {0}", partialTimeSpanMinutes);

                    // repair previous matrix
                    list[i] = RepairMatrixRowCount(list[i], partialTimeSpanMinutes);
                    //FileTools.WriteMatrix2File(matrix, @"C:\Users\towsey\temp\delete.csv");
                    cummulativeRowCount += list[i].GetLength(0);
                }
                else
                {
                    cummulativeRowCount += matrixRowCounts[i];
                }                
            } // end for loop
            cummulativeRowCount += list[list.Count-1].GetLength(0);

            var M = MatrixTools.ConcatenateMatrixRows(list);
            int matrixRowCount = M.GetLength(0);

            // ########### NOW DO FINAL CHECK ON ROW-COUNT OF CONCATENATED FILES
            // IMPORTANT: IT IS ASSUMED THAT A FULL 24 HOURS OF DATA HAS BEEN CONCATENATED

            int numberOfMinutesInDay = 1440;
            int expectedRowCount = (int)Math.Round(numberOfMinutesInDay / indexCalcDuration.TotalMinutes);

            if (cummulativeRowCount > expectedRowCount)
            {
                M = RepairMatrixRowCount(M, numberOfMinutesInDay);
                if (Verbose)
                {
                    LoggedConsole.WriteLine("WARNING: ROW COUNT EXCEEDS 1440 MINUTES from IndexMatrices.ReadAndConcatenateSpectrogramCSVFilesWithTimeCheck() ");
                    string str = String.Format("   Final Cummulative Row Count = {0}   Removing excess rows!", cummulativeRowCount);
                    LoggedConsole.WriteLine(str);
                }

            }
            //FileTools.WriteMatrix2File(M, @"C:\Users\towsey\temp\delete2.csv");

            return M;
        }
*/
/*
        public static double[,] RepairMatrixRowCount(double[,] M, int requiredRowCount)
        {
            int currentRowCount = M.GetLength(0);

            if (currentRowCount < requiredRowCount)
            {
                int rowDeficiency = requiredRowCount - currentRowCount;
                double[,] M1 = MatrixTools.AddBlankRows(M, rowDeficiency);
                return M1;
            }

            if (currentRowCount > requiredRowCount)
            {
                int surplusRows = currentRowCount - requiredRowCount;
                double[,] M2 = MatrixTools.RemoveLastNRows(M, surplusRows);
                return M2;
            }
            return M; // must have correct row count
        }
*/
/*
        public static Dictionary<string, double[]> PadDictionaryArraysWithNulls(Dictionary<string, double[]> dict, int countToAdd)
        {
            double[] nullArray = new double[countToAdd];
            for (int i = 0; i < countToAdd; i++)
                nullArray[i] = double.NaN;

            string[] keys = dict.Keys.ToArray();
            foreach(string key in keys)
            {
                double[] array = dict[key];
                double[] result = array.Concat(nullArray).ToArray();
                dict[key] = result;
            }

            return dict; 
        }
*/
/*
        public static Dictionary<string, double[]> RemoveValuesFromArraysInDictionary(Dictionary<string, double[]> dict, int countToRemove)
        {
            string[] keys = dict.Keys.ToArray();
            foreach (string key in keys)
            {
                double[] array = dict[key];
                int newArrayLength = array.Length - countToRemove;
                double[] result = DataTools.Subarray(array, 0, newArrayLength);
                dict[key] = result;
            }

            return dict;
        }
*/

        public static Dictionary<string, double[,]> AddDerivedIndices(Dictionary<string, double[,]> spectrogramMatrices)
        {
            string key = "POW";
            string newKey = "Sqrt" + key;
            if ((spectrogramMatrices.ContainsKey(key)) && (! spectrogramMatrices.ContainsKey(newKey)))
            // add another matrix with square root and log transform  of values for lop-sided distributions
            {
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.SquareRootOfValues(m));
                newKey = "Log" + key;
                spectrogramMatrices.Add(newKey, MatrixTools.LogTransform(m));
            }

            // add another matrix with square root and log transform of values for lop-sided distributions
            key = "ENT";
            newKey = "Sqrt" + key;
            if ((spectrogramMatrices.ContainsKey(key)) && (! spectrogramMatrices.ContainsKey(newKey)))
            {
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.SquareRootOfValues(m));
            }

            newKey = "Log" + key;
            if ((spectrogramMatrices.ContainsKey(key)) && (! spectrogramMatrices.ContainsKey(newKey)))
            {
                var m = spectrogramMatrices[key];
                spectrogramMatrices.Add(newKey, MatrixTools.LogTransform(m));
            }
            return spectrogramMatrices;
        }



        /// <summary>
        /// DO NOT DELETE THIS METHOD DESPITE NO REFERENCES
        /// It can be useful in future.
        /// </summary>
        /// <param name="summaryIndices"></param>
        /// <returns></returns>
        public static Dictionary<string, double[]> AddDerivedIndices(Dictionary<string, double[]> summaryIndices)
        {
            // insert some transformed data columns
            summaryIndices.Add("SqrtTempEntropy", DataTools.SquareRootOfValues(summaryIndices["TemporalEntropy"]));

            // insert some transformed data columns
            summaryIndices.Add("LogTempEntropy", DataTools.LogTransform(summaryIndices["TemporalEntropy"]));

            // Calculate Normalised Difference Soundscape Index if not already done
            // caluclate two ratios for three bands.  DO NOT CHANGE THESE KEYS
            string ndsiKey = "NDSI-LM";
            if (!summaryIndices.ContainsKey(ndsiKey))
            {
                summaryIndices = IndexMatrices.AddNDSI_GageGauge(summaryIndices, ndsiKey);
            }
            ndsiKey = "NDSI-MH";
            if (!summaryIndices.ContainsKey(ndsiKey))
            {
                summaryIndices = IndexMatrices.AddNDSI_GageGauge(summaryIndices, ndsiKey);
            }

            return summaryIndices;
        }



        //public static double[,] ReadSummaryIndicesFromFile(FileInfo csvPath)
        //{
        //    Tuple<List<string>, List<double[]>> tuple = CsvTools.ReadCSVFile(csvPath.FullName);

        //    double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath.FullName);
        //    // matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 1);
        //    return matrix;
        //}


        /// <summary>
        /// This method reads spectrogram csv files where the first row contains column names 
        /// and the first column contains row/time names.
        /// </summary>
        /// <param name="csvPath"></param>
        /// <param name="binCount"></param>
        /// <returns></returns>
        public static double[,] ReadSpectrogram(FileInfo csvPath, out int binCount)
        {
            //TwoDimensionalArray dimensionality = TwoDimensionalArray.RowMajor;
            //double[,] matrix = Csv.ReadMatrixFromCsv<double>(csvPath, dimensionality);
            // MICHAEL: the new Csv class can read this in, and optionally transpose as it reads
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath.FullName);
            binCount = matrix.GetLength(1) - 1; // -1 because first bin is the index numbers 
            // calculate the window/frame that was used to generate the spectra. This value is only used to place grid lines on the final images

            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, binCount);
            return matrix;
        }

/*
        public static Dictionary<string, double[,]> ReadCsvFiles(FileInfo[] paths, string[] keys)
        {
            string warning = null;

            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();
            for (int i = 0; i < keys.Length; i++)
            {
                DateTime now1 = DateTime.Now;

                // get the path containing keys[i]
                FileInfo file = null;
                for (int p = 0; p < paths.Length; p++)
                {
                    if (paths[p].Name.Contains(keys[i]))
                    {
                        file = paths[p];
                        break;
                    }
                }

                if (file.Exists)
                {
                    int freqBinCount;
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out freqBinCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    spectrogramMatrices.Add(keys[i], matrix);
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method IndexMatrices.ReadCsvFiles()";
                    }

                    warning += "\n      {0} File does not exist: {1}".Format2(keys[i], file.FullName);
                }

                if (IndexMatrices.Verbose)
                { 
                    DateTime now2 = DateTime.Now;
                    TimeSpan et = now2 - now1;
                    LoggedConsole.WriteLine("Time to read spectral index file <" + keys[i] + "> = " + et.TotalSeconds + " seconds");
                }
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (spectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: from method IndexMatrices.ReadCsvFiles()");
                LoggedConsole.WriteLine("         NO FILES were read from the passed paths");
            }

            return spectrogramMatrices;
        }
        */


        /// <summary>
        /// returns dictionary of spectral indices.
        /// Assumes both arrays of same length and keys correspond to file name. Just too lazy to do it better!
        /// </summary>
        /// <param name="files"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static Dictionary<string, double[,]> ReadSummaryIndexFiles(FileInfo[] files, string[] keys)
        {
            int count = files.Length;
            var dict = new Dictionary<string, double[,]>();
            for (int c = 0; c < count; c++)
            {
                int freqBinCount;
                double[,] matrix = ReadSpectrogram(files[c], out freqBinCount);
                dict.Add(keys[c], matrix);
            }
            return dict;
        }

        public static Dictionary<string, double[,]> ReadCsvFiles(DirectoryInfo ipdir, string fileName, string[] keys)
        {
            // parallel reading of CSV files
            var readData = keys
                .AsParallel()
                .Select(key => ReadInSingleCsvFile(ipdir, fileName, key))
                .Where(x => x != null);
            
            // actual work done here
            // ReSharper disable PossibleInvalidOperationException
            var spectrogramMatrices = readData.ToDictionary(kvp => kvp.Value.Key, kvp => kvp.Value.Value);
            // ReSharper restore PossibleInvalidOperationException


            if (spectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: from method IndexMatrices.ReadCsvFiles()");
                LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);
            }

            return spectrogramMatrices;
        }

        private static KeyValuePair<string, double[,]>? ReadInSingleCsvFile(DirectoryInfo ipdir, string fileName, string indexKey)
        {
            //Log.Info($"Starting to read CSV file for index {indexKey}");
            //Stopwatch timer = Stopwatch.StartNew();

            FileInfo file = new FileInfo(Path.Combine(ipdir.FullName, fileName + "." + indexKey + ".csv"));
            double[,] matrix;
            if (file.Exists)
            {
                int freqBinCount;
                matrix = ReadSpectrogram(file, out freqBinCount);
                matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                //this.FrameLength = freqBinCount * 2;
            }
            else
            {
                Log.Warn(
                    "\nWARNING: from method IndexMatrices.ReadCsvFiles()"
                    + $"\n      {indexKey} File does not exist: {file.FullName}");
                return null;
            }

            //timer.Stop();
            //Log.Info($"Time to read spectral index file <{indexKey}> = {timer.Elapsed.TotalSeconds} seconds");
            return new KeyValuePair<string, double[,]>(indexKey, matrix);
        }

        /// <summary>
        /// compresses the spectral index data in the temporal direction by a factor dervied from the data scale and required image scale.
        /// In all cases, the compression is done by taking the average.
        /// The method got more complicated in June 2016 when refactored it to cope with recording blocks less than one minute long.
        /// </summary>
        /// <param name="spectra"></param>
        /// <param name="imageScale"></param>
        /// <param name="dataScale"></param>
        /// <returns></returns>
        public static Dictionary<string, double[,]> CompressIndexSpectrograms(Dictionary<string, double[,]> spectra, TimeSpan imageScale, TimeSpan dataScale)
        {
            int scalingFactor = (int)Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);
            var compressedSpectra = new Dictionary<string, double[,]>();
            int step = scalingFactor - 1;

            // if there's no need to compress, simply return
            if (step == 0)
            {
                return spectra;
            }

            foreach (string key in spectra.Keys)
            {
                double[,] matrix = spectra[key];
                int rowCount = matrix.GetLength(0);
                int colCount = matrix.GetLength(1);

                int compressionWindow = scalingFactor;
                int compressedLength = (colCount / scalingFactor);
                if (compressedLength < 1)
                    compressedLength = 1;
                var newMatrix = new double[rowCount, compressedLength];
                double[] tempArray = new double[scalingFactor];
                int maxColCount = colCount - scalingFactor;
                if (maxColCount < 0)
                {
                    maxColCount = matrix.GetLength(1);
                    compressionWindow = maxColCount;
                }

                // the ENTROPY matrix requires separate calculation
                if ((key == "ENT") && (scalingFactor > 1))
                {
                    matrix = spectra["SUM"];
                    for (int r = 0; r < rowCount; r++)
                    {
                        for (int c = 0; c <= maxColCount; c += step)
                        {
                            var colIndex = c / scalingFactor;
                            for (int i = 0; i < compressionWindow; i++)
                            {
                                // square the amplitude to give energy
                                tempArray[i] = matrix[r, c + i] * matrix[r, c + i];
                            }
                            double entropy = DataTools.Entropy_normalised(tempArray);
                            if (double.IsNaN(entropy)) entropy = 1.0;
                            newMatrix[r, colIndex] = 1 - entropy;
                        }
                    }
                }
                else
                    // THE ACI matrix requires separate calculation
                    if ((key == "ACI") && (scalingFactor > 1))
                    {
                        double[] DIFArray = new double[scalingFactor];
                        double[] SUMArray = new double[scalingFactor];
                        for (int r = 0; r < rowCount; r++)
                        {
                            for (int c = 0; c <= maxColCount; c += step)
                            {
                                var colIndex = c / scalingFactor;
                                for (int i = 0; i < compressionWindow; i++)
                                {
                                    DIFArray[i] = spectra["DIF"][r, c + i];
                                    SUMArray[i] = spectra["SUM"][r, c + i];
                                }
                                newMatrix[r, colIndex] = DIFArray.Sum() / SUMArray.Sum();
                            }
                        }
                    }
                    else // average all other spectral indices
                    {
                        matrix = spectra[key];
                        for (int r = 0; r < rowCount; r++)
                        {
                            for (int c = 0; c <= maxColCount; c += step)
                            {
                                var colIndex = c / scalingFactor;
                                for (int i = 0; i < compressionWindow; i++)
                                tempArray[i] = matrix[r, c + i];
                                newMatrix[r, colIndex] = tempArray.Average();
                            }
                        }
                    }
                compressedSpectra[key] = newMatrix;
            }
            return compressedSpectra;
        }

        public static Dictionary<string, double[,]> ReadSpectrogramCSVFiles(DirectoryInfo ipdir, string fileName, string indexKeys, out int freqBinCount)
        {
            string[] keys = indexKeys.Split('-');
            return ReadSpectrogramCSVFiles(ipdir, fileName, keys, out freqBinCount);
        }

        public static Dictionary<string, double[,]> ReadSpectrogramCSVFiles(DirectoryInfo ipdir, string fileName, string[] keys, out int freqBinCount)
        {
            var dict = new Dictionary<string, double[,]>();
            string warning = null;
            freqBinCount = 256; // the default
            for (int key = 0; key < keys.Length; key++)
            {
                var file = new FileInfo(Path.Combine(ipdir.FullName, fileName + "." + keys[key] + ".csv"));
                if (file.Exists)
                {
                    int binCount;
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out binCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    dict.Add(keys[key], matrix);
                    freqBinCount = binCount;
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method IndexMatrices.ReadSpectrogramCSVFiles()";
                    }

                    warning += $"\n      {keys[key]} File does not exist: {file.FullName}";
                }
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (dict.Count != 0)
            {
                return dict;
            }

            LoggedConsole.WriteLine("WARNING: from method IndexMatrices.ReadSpectrogramCSVFiles()");
            LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);

            return dict;
        }


        public static Dictionary<string, double[]> AddNDSI_GageGauge(Dictionary<string, double[]> dictionaryOfCsvColumns, string newKey)
        {
            const string highKey = "HighFreqCover";
            const string midKey  = "MidFreqCover";
            const string lowKey  = "LowFreqCover";
            if (newKey.EndsWith("-LM"))
            {
                if (!dictionaryOfCsvColumns.ContainsKey(midKey)) return null;
                if (!dictionaryOfCsvColumns.ContainsKey(lowKey)) return null;
                double[] midArray = dictionaryOfCsvColumns[midKey];
                double[] lowArray = dictionaryOfCsvColumns[lowKey];
                if (lowArray.Length != midArray.Length) return null;

                var array = new double[lowArray.Length];
                for (int i = 0; i < lowArray.Length; i++)
                {
                    array[i] = (midArray[i] - lowArray[i]) / (midArray[i] + lowArray[i]);
                }

                dictionaryOfCsvColumns.Add(newKey, array);
            }
            else if (newKey.EndsWith("-MH"))
            {
                if (!dictionaryOfCsvColumns.ContainsKey(midKey)) return null;
                if (!dictionaryOfCsvColumns.ContainsKey(highKey)) return null;
                double[] midArray  = dictionaryOfCsvColumns[midKey];
                double[] highArray = dictionaryOfCsvColumns[highKey];
                if (highArray.Length != midArray.Length) return null;

                var array = new double[highArray.Length];
                for (int i = 0; i < highArray.Length; i++)
                {
                    array[i] = (highArray[i] - midArray[i]) / (highArray[i] + midArray[i]);
                }

                dictionaryOfCsvColumns.Add(newKey, array);
            }


            return dictionaryOfCsvColumns;
        }


    }
}
