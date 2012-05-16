﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace AnalysisPrograms
{
    class BarsAndStripes
    {


        /*************************************************************************
         * Need to install alglib dll to get this funcitonality
1-dimensional real cross-correlation.

For given Pattern/Signal returns corr(Pattern,Signal) (non-circular).

Correlation is calculated using reduction to  convolution.  Algorithm with
max(N,N)*log(max(N,N)) complexity is used (see  ConvC1D()  for  more  info
about performance).

IMPORTANT:
for  historical reasons subroutine accepts its parameters in  reversed
order: CorrR1D(Signal, Pattern) = Pattern x Signal (using  traditional
definition of cross-correlation, denoting cross-correlation as "x").

INPUT PARAMETERS
Signal  -   array[0..N-1] - real function to be transformed,
        signal containing pattern
N       -   problem size
Pattern -   array[0..M-1] - real function to be transformed,
        pattern to search withing signal
M       -   problem size

OUTPUT PARAMETERS
R       -   cross-correlation, array[0..N+M-2]:
        * positive lags are stored in R[0..N-1],
          R[i] = sum(pattern[j]*signal[i+j]
        * negative lags are stored in R[N..N+M-2],
          R[N+M-1-i] = sum(pattern[j]*signal[-i+j]

NOTE:
It is assumed that pattern domain is [0..M-1].  If Pattern is non-zero
on [-K..M-1],  you can still use this subroutine, just shift result by K.

-- ALGLIB --
Copyright 21.07.2009 by Bochkanov Sergey
 * 
 * 
 * 
public static void corrr1d(
double[] signal,
int n,
double[] pattern,
int m,
out double[] r)

*************************************************************************/

        public static double[] CrossCorrelation(double[] v1, double[] v2)
        {
            int n = v1.Length; //assume both vectors of same length
            double[] r;
            alglib.corrr1d(v1, n, v2, n, out r);
            //alglib.complex[] f;
            //alglib.fftr1d(newOp, out f);
            //System.Console.WriteLine("{0}", alglib.ap.format(f, 3));
            //for (int i = 0; i < op.Length; i++) Console.WriteLine("{0}   {1:f2}", i, op[i]);

            //rearrange corr output and normalise
            int xcorrLength = 2 * n;
            double[] xCorr = new double[xcorrLength];
            //for (int i = 0; i < n - 1; i++) newOp[i] = r[i + n];   //rearrange corr output
            //for (int i = n - 1; i < opLength-1; i++) newOp[i] = r[i - n + 1];
            for (int i = 0; i < n - 1; i++) xCorr[i] = r[i + n] / (i + 1);  //rearrange and normalise
            for (int i = n - 1; i < xcorrLength - 1; i++) xCorr[i] = r[i - n + 1] / (xcorrLength - i - 1);


            //add extra value at end so have length = power of 2 for FFT
            //xCorr[xCorr.Length - 1] = xCorr[xCorr.Length - 2];
            //Console.WriteLine("xCorr length = " + xCorr.Length);
            //for (int i = 0; i < xCorr.Length; i++) Console.WriteLine("{0}   {1:f2}", i, xCorr[i]);
            //DataTools.writeBarGraph(xCorr);

            xCorr = DataTools.DiffFromMean(xCorr);
            FFT.WindowFunc wf = FFT.Hamming;
            var fft = new FFT(xCorr.Length, wf);

            var spectrum = fft.Invoke(xCorr);
            return spectrum;
        }//CrossCorrelation()



         public static void TestCrossCorrelation()
         {
            double[] signal2  = {1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            double[] signal4  = {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal6 = { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal7 = { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 };
            double[] signal8 = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            double[] signal10 = {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] signal16 = {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int n = signal2.Length;
            double[] pattern2  = { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };
            double[] pattern4  = { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern6  = { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern7  = { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 };
            double[] pattern8  = { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            double[] pattern10 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] pattern16 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            Console.WriteLine("Signal length = {0}", n);
            int smoothWindow = 3;
            double[] signal  = DataTools.filterMovingAverage(signal16,  smoothWindow);
            double[] pattern = DataTools.filterMovingAverage(pattern16, smoothWindow);

            var spectrum = BarsAndStripes.CrossCorrelation(signal, pattern);
            int zeroCount = 3;
            for (int s = 1; s < zeroCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
            spectrum = DataTools.NormaliseArea(spectrum);
            int maxId = DataTools.GetMaxIndex(spectrum);
            double intensityValue = spectrum[maxId];

            if (maxId == 0) Console.WriteLine("max id = 0");
            else
            {
                double period = 2 * n / (double)maxId;
                Console.WriteLine("max id = {0};   period = {1:f2};    intensity = {2:f3}", maxId, period, intensityValue);
            }
         }//TestCrossCorrelation()


         public static System.Tuple<double[,]> DetectBarsUsingXcorrelation(double[,] m)
         {
             int rowCount = m.GetLength(0);
             int colCount = m.GetLength(1);
             var hits = new double[rowCount, colCount];
             return Tuple.Create(hits);
         }

         public static System.Tuple<double[,]> DetectStripesUsingXcorrelation(double[,] m)
         {
             int rowCount = m.GetLength(0);
             int colCount = m.GetLength(1);
             var hits = new double[rowCount, colCount];
             return Tuple.Create(hits);
         }

        /// <summary>
        /// This method assume the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of sonogram.
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="amplitudeThreshold"></param>
        /// <returns></returns>
         public static System.Tuple<double[], double[]> DetectStripesInColumnsOfMatrix(double[,] m, double threshold, int zeroBinCount)
         {
             int rowCount = m.GetLength(0);
             int colCount = m.GetLength(1);
             var intensity   = new double[rowCount];     //an array of period intensity
             var periodicity = new double[rowCount];     //an array of the periodicity values

             double[] prevRow = MatrixTools.GetRow(m, 0);
             prevRow = DataTools.DiffFromMean(prevRow);

             for (int r = 1; r < rowCount; r++)
             {
                 double[] thisRow = MatrixTools.GetRow(m, r);
                 thisRow = DataTools.DiffFromMean(thisRow);

                 var spectrum = BarsAndStripes.CrossCorrelation(prevRow, thisRow);

                 for (int s = 0; s < zeroBinCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                 spectrum  = DataTools.NormaliseArea(spectrum);
                 int maxId = DataTools.GetMaxIndex(spectrum);
                 double intensityValue = spectrum[maxId];
                 intensity[r] = intensityValue;

                 double period = 0.0;
                 if (maxId != 0) period = 2 * colCount / (double)maxId;
                 periodicity[r] = period;

                 prevRow = thisRow;
             }// rows
             return Tuple.Create(intensity, periodicity);
         }

    } //class
} //AnalysisPrograms
