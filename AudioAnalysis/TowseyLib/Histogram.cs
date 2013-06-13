﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLib
{
    public static class Histogram
    {


        public static int[] Histo(double[] data, int binCount)
        {
            double min;
            double max;
            DataTools.MinMax(data, out min, out max);
            double range = max - min;
            double binWidth = range / (double)binCount;
            // init freq bin array
            int[] bins = new int[binCount];
            for (int i = 0; i < data.Length; i++)
            {
                int id = (int)((data[i] - min) / binWidth);
                if (id >= binCount) id--;
                else if (id < 0) id = 0;
                bins[id]++;
            }
            return bins;
        }

        public static int[] Histo(double[] data, int binCount, out double binWidth, out double min, out double max)
        {
            DataTools.MinMax(data, out min, out max);
            double range = max - min;
            binWidth = range / (double)binCount;
            // init freq bin array
            int[] bins = new int[binCount];
            for (int i = 0; i < data.Length; i++)
            {
                int id = 0;
                if (binWidth != 0.0) id = (int)((data[i] - min) / binWidth);
                if (id >= binCount) id = binCount - 1; // check because too lazy to do it properly.
                bins[id]++;
            }
            return bins;
        }



        static public int[] Histo(double[,] data, int binCount, out double binWidth, out double min, out double max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int[] histo = new int[binCount];
            min = double.MaxValue;
            max = -double.MaxValue;
            DataTools.MinMax(data, out min, out max);
            binWidth = (max - min) / binCount;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    int bin = (int)((data[i, j] - min) / binWidth);
                    if (bin >= binCount) bin = binCount - 1;
                    if (bin < 0) bin = 0;
                    histo[bin]++;
                }

            return histo;
        }

        
        /// <summary>
        /// returns a fixed width histogram.
        /// Width is determined by user supplied min and max.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="binWidth"> should be an integer width</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int[] Histo_FixedWidth(int[] data, int binWidth, int min, int max)
        {
            int range = max - min + 1;
            int binCount = range / binWidth;
            // init freq bin array
            int[] bins = new int[binCount];
            for (int i = 0; i < data.Length; i++)
            {
                int id = (int)((data[i] - min) / binWidth);
                if (id >= binCount) id = binCount - 1;
                else
                    if (id < 0) id = 0;
                bins[id]++;
            }
            return bins;
        }
        /// <summary>
        /// returns a fixed width histogram.
        /// Width is determined by user supplied min and max.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="binWidth"> should be an integer width</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int[] Histo_FixedWidth(double[] data, double binWidth, double min, double max)
        {
            double range = max - min + 1;
            int binCount = (int)(range / binWidth);
            // init freq bin array
            int[] bins = new int[binCount];
            for (int i = 0; i < data.Length; i++)
            {
                int id = (int)((data[i] - min) / binWidth);
                if (id >= binCount) id = binCount - 1;
                else
                    if (id < 0) id = 0;
                bins[id]++;
            }
            return bins;
        }


        /// <summary>
        /// HISTOGRAM from an array of int
        /// </summary>
        /// <param name="data"></param>
        /// <param name="binCount"></param>
        /// <param name="binWidth"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        static public int[] Histo(int[] data, int binCount, out double binWidth, out int min, out int max)
        {
            int length = data.Length;
            int[] histo = new int[binCount];
            min = Int32.MaxValue;
            max = -Int32.MaxValue;
            DataTools.MinMax(data, out min, out max);
            binWidth = (max - min + 1) / (double)binCount;

            for (int i = 0; i < length; i++)
            {
                int bin = (int)((double)(data[i] - min) / binWidth);
                if (bin >= binCount) bin = binCount - 1;
                histo[bin]++;
            }

            return histo;
        }


        /// <summary>
        /// HISTOGRAM from a matrix of double
        /// </summary>
        /// <param name="data"></param>
        /// <param name="binCount"></param>
        /// <returns></returns>
        static public int[] Histo(double[,] data, int binCount)
        {
            double min;
            double max;
            DataTools.MinMax(data, out min, out max);
            double binWidth = (max - min + 1) / (double)binCount;
            LoggedConsole.WriteLine("data min=" + min + "  data max=" + max + " binwidth=" + binWidth);

            return Histo(data, binCount, min, max, binWidth);
        }

        static public int[] Histo(double[,] data, int binCount, double min, double max, double binWidth)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int[] histo = new int[binCount];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    int bin = (int)((data[i, j] - min) / binWidth);
                    if (bin >= binCount) bin = binCount - 1;
                    if (bin < 0) bin = 0;
                    histo[bin]++;
                }

            return histo;
        }


        static public int[] Histo_addition(double[,] data, int[] histo, double min, double max, double binWidth)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int binCount = histo.Length;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    int bin = (int)((data[i, j] - min) / binWidth);
                    if (bin >= binCount) bin = binCount - 1;
                    if (bin < 0) bin = 0;
                    histo[bin]++;
                }

            return histo;
        }

        static public void writeConciseHistogram(int[] data)
        {
            int min;
            int max;
            DataTools.MinMax(data, out min, out max);
            int[] histo = new int[max + 1];
            for (int i = 0; i < data.Length; i++)
            {
                histo[data[i]]++;
            }
            for (int i = min; i <= max; i++)
                LoggedConsole.WriteLine(" " + i + "|" + histo[i]);
            LoggedConsole.WriteLine();
        }

        static public void writeConcise2DHistogram(int[,] array, int max)
        {
            int[,] matrix = new int[max, max];
            for (int i = 0; i < array.Length; i++)
            {
                matrix[array[i, 0], array[i, 1]]++;
            }

            for (int r = 0; r < max; r++)
            {
                for (int c = 0; c < max; c++)
                {
                    if (matrix[r, c] > 0) LoggedConsole.WriteLine(r + "|" + c + "|" + matrix[r, c] + "  ");
                }
                LoggedConsole.WriteLine();
            }
        }



        public static void GetModeAndOneStandardDeviation(double[] histo, int upperBoundOfMode, out int indexOfMode, out int indexOfOneSD)
        {
            indexOfMode = DataTools.GetMaxIndex(histo);
            if(indexOfMode > upperBoundOfMode) indexOfMode = upperBoundOfMode;

            //calculate SD of the background noise
            double totalAreaUnderLowerCurve = 0.0;
            for (int i = 0; i <= indexOfMode; i++) totalAreaUnderLowerCurve += histo[i];

            indexOfOneSD = indexOfMode;
            double partialSum = 0.0; //sum
            double thresholdSum = totalAreaUnderLowerCurve * 0.68; // 0.68 = area under one standard deviation
            for (int i = indexOfMode; i > 0; i--) 
            {
                partialSum += histo[i];
                indexOfOneSD = i;
                if (partialSum > thresholdSum) // we have passed the one SD point
                {
                    break;
                }
            } // for loop
        } // GetModeAndOneStandardDeviation()



    } // class Histrogram
}
