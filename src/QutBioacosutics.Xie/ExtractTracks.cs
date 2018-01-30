﻿namespace QutBioacosutics.Xie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools.StandardSpectrograms;
    using MathNet.Numerics;
    using MathNet.Numerics.LinearAlgebra.Single;
    using TowseyLibrary;

    class ExtractTracks
    {
        public static Tuple<double[], double[,], double[], double[,]> GetTracks(SpectrogramStandard sonogram, double[,] matrix, int minHz, int maxHz, double binToreance, int frameThreshold,
                                                                            int duraionThreshold, double trackThreshold, int maximumDuration, int minimumDuration, double maximumDiffBin,
                                                                            bool doSlope)

        {
            // matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            var row = matrix.GetLength(0);
            var column = matrix.GetLength(1);

            int maxBin = row - (int)(minHz / sonogram.FBinWidth) - 1;
            int minBin = row - (int)(maxHz / sonogram.FBinWidth) - 1;

            double binToreanceStable = binToreance;
            // Save local peaks to an array of list
            var pointList = new List<Peak>[column];

            for (int j = 0; j < column; j++)
            {
                var Points = new List<Peak>();
                for (int i = 0; i < row; i++)
                {
                    var point = new Peak();
                    if (matrix[i, j] > 0)
                    {
                        point.Y = i;
                        point.X = j;
                        point.Amplitude = matrix[i, j];
                        Points.Add(point);
                    }
                }
                pointList[j] = Points;
            }

            // Use neareast distance to form tracks of the first two columns

            var shortTrackList = new List<Track>();
            var longTrackList = new List<Track>();
            var closedTrackList = new List<Track>();

            var longTrackXList = new List<List<double>>();
            var longTrackYList = new List<List<double>>();
            var closedTrackXList = new List<List<double>>();
            var closedTrackYList = new List<List<double>>();

            var indexI = new List<int>();
            var indexJ = new List<int>();

            // save nearest peaks into longTracks
            for (int i = 0; i < pointList[0].Count; i++)
            {
                binToreance = binToreanceStable;
                for(int j = 0; j < pointList[1].Count; j++)
                {
                    if (Math.Abs(pointList[0][i].Y - pointList[1][j].Y) < binToreance)
                    {
                        indexI.Add(i);
                        indexJ.Add(j);
                        binToreance = Math.Abs(pointList[0][i].Y - pointList[1][j].Y);
                    }
                }
            }

            for (int i = 0; i < indexI.Count; i++)
            {
                var longTrackX = new List<double>();
                var longTrackY = new List<double>();

                longTrackX.AddMany(0, 1);
                longTrackY.AddMany(pointList[0][indexI[i]].Y, pointList[1][indexJ[i]].Y);
                longTrackXList.Add(longTrackX);
                longTrackYList.Add(longTrackY);
            }

            for (int i = 0; i < indexI.Count; i++)
            {
                var longTrack = new Track();
                longTrack.StartFrame = 0;
                longTrack.EndFrame = 1;
                longTrack.LowBin = Math.Min(pointList[0][indexI[i]].Y, pointList[1][indexJ[i]].Y);
                longTrack.HighBin = Math.Max(pointList[0][indexI[i]].Y, pointList[1][indexJ[i]].Y);
                longTrackList.Add(longTrack);
            }

            // Remove peaks which have already been used to produce long tracks
            for (int i = 0; i < indexI.Count; i++)
            {
                pointList[0][indexI[i]] = null;
            }

            for (int i = 0; i < pointList[0].Count; i++)
            {
                if (pointList[0][i] == null)
                {
                    pointList[0].RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < indexJ.Count; i++)
            {
                pointList[1][indexJ[i]] = null;
            }

            for (int i = 0; i < pointList[1].Count; i++)
            {
                if(pointList[1][i] == null)
                {
                    pointList[1].RemoveAt(i);
                    i--;
                }
            }

            // Save individual peaks into shortTracks
            for (int i = 0; i < pointList[0].Count; i++)
            {
                var shortTrack = new Track();
                shortTrack.StartFrame = 0;
                shortTrack.EndFrame = 0;
                shortTrack.LowBin = pointList[0][i].Y;
                shortTrack.HighBin = pointList[0][i].Y;
                shortTrackList.Add(shortTrack);
            }

            for (int i = 0; i < pointList[1].Count; i++)
            {
                var shortTrack = new Track();
                shortTrack.StartFrame = 1;
                shortTrack.EndFrame = 1;
                shortTrack.LowBin = pointList[1][i].Y;
                shortTrack.HighBin = pointList[1][i].Y;
                shortTrackList.Add(shortTrack);
            }

            // Use linear regression to extend long tracks and use neareast distance to extend short tracks
            int c = 2;
            while (c < column)
            {
                // Save the long tracks to closed track lists
                for (int i = 0; i < longTrackList.Count; i++)
                {
                    if ((c - longTrackList[i].EndFrame) > frameThreshold)
                    {
                        if (longTrackList[i].Duration > duraionThreshold)
                        {
                            closedTrackList.Add(longTrackList[i]);
                            closedTrackXList.Add(longTrackXList[i]);
                            closedTrackYList.Add(longTrackYList[i]);

                            longTrackList.RemoveAt(i);
                            longTrackXList.RemoveAt(i);
                            longTrackYList.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            longTrackList.RemoveAt(i);
                            longTrackXList.RemoveAt(i);
                            longTrackYList.RemoveAt(i);
                            i--;

                        }
                    }
                }

                for (int i = 0; i < shortTrackList.Count; i++)
                {
                    if ((c - shortTrackList[i].EndFrame) > frameThreshold)
                    {
                        shortTrackList.RemoveAt(i);
                        i--;
                    }
                }


                if (longTrackList.Count != 0)
                {
                    var numberA = new List<int>();
                    var numberB = new List<int>();

                    // use linear regression to predict the next position of long tracks
                    for (int i = 0; i < longTrackList.Count; i++)
                    {
                        var xdata = new double[longTrackXList.Count];
                        xdata = longTrackXList[i].ToArray();

                        var ydata = new double[longTrackYList.Count];
                        ydata = longTrackYList[i].ToArray();

                        var p = Fit.Line(xdata, ydata);
                        var offset = p[0];
                        var slope = p[1];

                        var position = c * slope + offset;

                        // need to be refresh according to the profile document
                        binToreance = binToreanceStable;
                        for (int j = 0; j < pointList[c].Count; j++)
                        {
                            if (Math.Abs(position - pointList[c][j].Y) < binToreance)
                            {
                                numberA.Add(i);
                                numberB.Add(j);

                                binToreance = Math.Abs(position - pointList[c][j].Y);
                            }
                        }
                    }

                    for (int i = 0; i < numberA.Count; i++)
                    {
                        longTrackList[numberA[i]].EndFrame = c;
                        longTrackList[numberA[i]].LowBin = Math.Min(longTrackList[numberA[i]].LowBin, pointList[c][numberB[i]].Y);
                        longTrackList[numberA[i]].HighBin = Math.Max(longTrackList[numberA[i]].HighBin, pointList[c][numberB[i]].Y);

                        longTrackXList[numberA[i]].Add(c);
                        longTrackYList[numberA[i]].Add(pointList[c][numberB[i]].Y);
                    }

                    for (int i = 0; i < numberB.Count; i++)
                    {
                        pointList[c][numberB[i]] = null;
                    }

                    for (int i = 0; i < pointList[c].Count; i++)
                    {
                        if (pointList[c][i] == null)
                        {
                            pointList[c].RemoveAt(i);
                            i--;
                        }
                    }

                    // Add points of current frame to short tracks
                    var numberE = new List<int>();
                    var numberF = new List<int>();

                    for (int i = 0; i < shortTrackList.Count; i++)
                    {
                        binToreance = binToreanceStable;
                        for (int j = 0; j < pointList[c].Count; j++)
                        {
                            if (Math.Abs(shortTrackList[i].HighBin - pointList[c][j].Y) < binToreance)
                            {
                                numberE.Add(i);
                                numberF.Add(j);
                                binToreance = Math.Abs(shortTrackList[i].HighBin - pointList[c][j].Y);
                            }
                        }
                    }
                    //.......................................//
                    for (int i = 0; i < numberE.Count; i++)
                    {
                        var longTrack = new Track();
                        var longTrackX = new List<double>();
                        var longTrackY = new List<double>();

                        longTrack.StartFrame = shortTrackList[numberE[i]].StartFrame;
                        longTrack.EndFrame = c;
                        longTrack.LowBin = Math.Min(shortTrackList[numberE[i]].LowBin, pointList[c][numberF[i]].Y);
                        longTrack.HighBin = Math.Max(shortTrackList[numberE[i]].HighBin, pointList[c][numberF[i]].Y);
                        longTrackList.Add(longTrack);

                        longTrackX.AddMany(shortTrackList[numberE[i]].StartFrame, pointList[c][numberF[i]].X);
                        longTrackY.AddMany(shortTrackList[numberE[i]].LowBin, pointList[c][numberF[i]].Y);

                        longTrackXList.Add(longTrackX);
                        longTrackYList.Add(longTrackY);
                    }


                    for (int i = 0; i < numberE.Count; i++)
                    {
                        shortTrackList[numberE[i]] = null;
                    }

                    for (int i = 0; i < shortTrackList.Count; i++)
                    {
                        if (shortTrackList[i] == null)
                        {
                            shortTrackList.RemoveAt(i);
                            i--;
                        }
                    }

                    //..........................................//
                    for (int i = 0; i < numberF.Count; i++)
                    {
                        pointList[c][numberF[i]] = null;
                    }

                    for (int i = 0; i < pointList[c].Count; i++)
                    {
                        if (pointList[c][i] == null)
                        {
                            pointList[c].RemoveAt(i);
                            i--;
                        }
                    }

                    //..........................................//

                    for (int i = 0; i < pointList[c].Count; i++)
                    {
                        var shortTrack = new Track();
                        shortTrack.StartFrame = c;
                        shortTrack.EndFrame = c;
                        shortTrack.LowBin = pointList[c][i].Y;
                        shortTrack.HighBin = pointList[c][i].Y;
                        shortTrackList.Add(shortTrack);
                    }

                    c = c + 1;
                }
                else if (longTrackList.Count == 0)
                {
                    if (c < (column - 2))
                    {
                        c = c + 1;
                        var numberC = new List<int>();
                        var numberD = new List<int>();

                        for (int i = 0; i < pointList[c].Count; i++)
                        {
                            binToreance = binToreanceStable;
                            for (int j = 0; j < pointList[c + 1].Count; j++)
                            {
                                if (Math.Abs(pointList[c][i].Y - pointList[c + 1][j].Y) < binToreance)
                                {
                                    numberC.Add(i);
                                    numberD.Add(j);
                                    binToreance = Math.Abs(pointList[c][i].Y - pointList[c + 1][j].Y);
                                }
                            }
                        }

                        //.............................................//
                        for (int i = 0; i < numberC.Count; i++)
                        {
                            var longTrackX = new List<double>();
                            var longTrackY = new List<double>();
                            longTrackX.AddMany(c, (c + 1));
                            longTrackY.AddMany(pointList[c][numberC[i]].Y, pointList[(c + 1)][numberD[i]].Y);

                            longTrackXList.Add(longTrackX);
                            longTrackYList.Add(longTrackY);
                        }

                        for (int i = 0; i < numberC.Count; i++)
                        {
                            var longTrack = new Track();
                            longTrack.StartFrame = c;
                            longTrack.EndFrame = c + 1;
                            longTrack.LowBin = Math.Min(pointList[c][numberC[i]].Y, pointList[c + 1][numberD[i]].Y);
                            longTrack.HighBin = Math.Max(pointList[c][numberC[i]].Y, pointList[c + 1][numberD[i]].Y);

                            longTrackList.Add(longTrack);
                        }

                        for (int i = 0; i < numberC.Count; i++)
                        {
                            pointList[c][numberC[i]] = null;
                        }

                        for (int i = 0; i < pointList[c].Count; i++)
                        {
                            if (pointList[c][i] == null)
                            {
                                pointList[c].RemoveAt(i);
                                i--;
                            }
                        }

                        for (int i = 0; i < numberD.Count; i++)
                        {
                            pointList[c + 1][numberD[i]] = null;
                        }

                        for (int i = 0; i < pointList[c + 1].Count; i++)
                        {
                            if (pointList[c + 1][i] == null)
                            {
                                pointList[c + 1].RemoveAt(i);
                                i--;
                            }
                        }

                        for (int i = 0; i < pointList[c].Count; i++)
                        {
                            var shortTrack = new Track();
                            shortTrack.StartFrame = c;
                            shortTrack.EndFrame = c;
                            shortTrack.LowBin = pointList[c][i].Y;
                            shortTrack.HighBin = pointList[c][i].Y;
                            shortTrackList.Add(shortTrack);
                        }

                        for (int i = 0; i < pointList[c + 1].Count; i++)
                        {
                            var shortTrack = new Track();
                            shortTrack.StartFrame = c + 1;
                            shortTrack.EndFrame = c + 1;
                            shortTrack.LowBin = pointList[c + 1][i].Y;
                            shortTrack.HighBin = pointList[c + 1][i].Y;
                            shortTrackList.Add(shortTrack);
                        }

                        c = c + 1;
                    }
                    else
                    {
                        break;
                    }
                }

            }

            // Remove tracks with few points
            for (int i = 0; i < closedTrackList.Count; i++)
            {
                if((closedTrackXList[i].Count / closedTrackList[i].Duration) < trackThreshold)
                {
                    closedTrackList.RemoveAt(i);
                    closedTrackXList.RemoveAt(i);
                    closedTrackYList.RemoveAt(i);
                    i--;
                }
            }

            // Remove one track with two peaks or two tracks with one peak
            for (int i = 0; i < closedTrackXList.Count; i++)
            {
                for (int j = 0; j < (closedTrackXList[i].Count - 1); j++)
                {
                    if ((closedTrackXList[i][j + 1] - closedTrackXList[i][j]) == 0)
                    {
                        closedTrackXList[i].RemoveAt(j+1);
                        closedTrackYList[i].RemoveAt(j+1);
                        j--;
                    }
                }
            }


            // Remove track with big binToreance
            for (int i = 0; i < closedTrackList.Count; i++)
            {
                if ((closedTrackList[i].HighBin - closedTrackList[i].LowBin) > maximumDiffBin)
                {
                    closedTrackList.RemoveAt(i);
                    closedTrackXList.RemoveAt(i);
                    closedTrackYList.RemoveAt(i);
                    i--;
                 }
            }


            // Remove too long tracks
            for (int i = 0; i < closedTrackList.Count; i++)
            {
                if (closedTrackList[i].Duration > maximumDuration)
                {
                    closedTrackList.RemoveAt(i);
                    closedTrackXList.RemoveAt(i);
                    closedTrackYList.RemoveAt(i);
                    i--;
                }
            }


            // Remove too short tracks
            for (int i = 0; i < closedTrackList.Count; i++)
            {
                if (closedTrackList[i].Duration < minimumDuration)
                {
                    closedTrackList.RemoveAt(i);
                    closedTrackXList.RemoveAt(i);
                    closedTrackYList.RemoveAt(i);
                    i--;
                }
            }

            // Calculate slope
            if (doSlope == true)
            {
                for (int i = 0; i < closedTrackList.Count; i++)
                {
                    var slope = (closedTrackList[i].HighBin - closedTrackList[i].LowBin) / (closedTrackList[i].EndFrame - closedTrackList[i].StartFrame);
                    if (slope < 0.05 & slope > 0.01)
                    {
                        closedTrackList.RemoveAt(i);
                        closedTrackXList.RemoveAt(i);
                        closedTrackYList.RemoveAt(i);
                        i--;
                    }
                }
            }


            // Fill the gap among tracks
            var finalTrackXList = new List<List<int>>();
            var finalTrackXListD = new List<List<double>>();
            var finalTrackYList = new List<List<double>>();
            var addTrackXList = new List<List<double>>();
            var addTrackYList = new List<List<double>>();

            // Get the the point of x_direction of one track
            for (int i = 0; i < closedTrackList.Count; i++)
            {
                var finalTrackX = new List<int>();
                for (int j = closedTrackList[i].StartFrame; j <= closedTrackList[i].EndFrame; j++)
                {
                    finalTrackX.Add(j);
                }
                finalTrackXList.Add(finalTrackX);
            }

            finalTrackXListD = finalTrackXList.Select(x => x.Select(y => (double)y).ToList()).ToList();

            var diffTrackXList = new List<List<double>>();
            for (int i = 0; i < closedTrackXList.Count; i++)
            {
                var diffTrackX = new List<double>();
                diffTrackX = finalTrackXListD[i].Except(closedTrackXList[i]).ToList();
                diffTrackXList.Add(diffTrackX);
            }

            var diffTrackXListI = new List<List<int>>();
            diffTrackXListI = diffTrackXList.Select(x => x.Select(y => (int)y).ToList()).ToList();

            for (int i = 0; i < diffTrackXList.Count; i++)
            {
                if (diffTrackXList[i].Count > 0)
                {
                    var tempTrackYList = new List<double>();
                    var tempaddTrackXList = new List<double>();
                    var tempaddTrackYList = new List<double>();

                    for (int j = 0; j < diffTrackXList[i].Count; j++)
                    {

                        var xdata = new List<double>();
                        var ydata = new List<double>();
                        for (int s = closedTrackList[i].StartFrame; s < diffTrackXList[i][j]; s++)
                        {
                            xdata.Add(s);
                        }

                        if (xdata.Count == 1)
                        {
                            int index = diffTrackXListI[i][j] - closedTrackList[i].StartFrame;
                            tempTrackYList = closedTrackYList[i];
                            tempTrackYList.Insert(index, closedTrackYList[i][0]);
                            closedTrackYList[i] = tempTrackYList;

                            tempaddTrackXList.Add(diffTrackXListI[i][j]);
                            tempaddTrackYList.Add(closedTrackYList[i][0]);
                        }
                        else
                        {
                            for (int t = 0; t < xdata.Count; t++)
                            {
                                ydata.Add(closedTrackYList[i][t]);
                            }

                            var xdataArray = new double[xdata.Count];
                            var ydataArray = new double[xdata.Count];
                            xdataArray = xdata.ToArray();
                            ydataArray = ydata.ToArray();

                            var p = Fit.Line(xdataArray, ydataArray);
                            var offset = p[0];
                            var slope = p[1];
                            var position = (xdata[xdata.Count - 1] + 1) * slope + offset;

                            int index = diffTrackXListI[i][j] - closedTrackList[i].StartFrame;
                            tempTrackYList = closedTrackYList[i];
                            tempTrackYList.Insert(index, position);
                            closedTrackYList[i] = tempTrackYList;

                            tempaddTrackXList.Add(diffTrackXListI[i][j]);
                            tempaddTrackYList.Add(position);
                        }
                    }
                    finalTrackYList.Add(closedTrackYList[i]);

                    addTrackXList.Add(tempaddTrackXList);
                    addTrackYList.Add(tempaddTrackYList);
                }
                else
                {
                    finalTrackYList.Add(closedTrackYList[i]);
                }
            }

            // Convert closedTrackList to trackMatrix
            // To do: convert double to int

            // Calculate the entropy based on frequency band
            var tempMatrix = new double[row, column];
            for (int i = 0; i < closedTrackList.Count; i++)
            {
                for (int col = closedTrackList[i].StartFrame; col < closedTrackList[i].EndFrame; col++)
                {
                    for (int r = closedTrackList[i].LowBin; r < (closedTrackList[i].HighBin + 1); r++)
                    {
                         tempMatrix[r,col] = matrix[r,col];
                    }
                }
            }

            var entropy = new double[row];

            for(int i = 0; i < row; i++)
            {
                double energy = 0;
                for (int j = 0; j < column; j++)
                {
                    var tempEnergy = Math.Pow(tempMatrix[i, j], 2);
                    energy = energy + tempEnergy;
                }

                entropy[i] = energy;
            }

            // Band hits
            var result = new double[row, column];
            for (int i = 0; i < closedTrackList.Count; i++)
            {
                for (int col = closedTrackList[i].StartFrame; col < closedTrackList[i].EndFrame; col++)
                {
                    for (int r = minBin; r < maxBin; r++)
                    //for (int r = closedTrackList[i].LowBin; r < (closedTrackList[i].HighBin + 1); r++)
                    {
                        result[r, col] = 1;
                    }
                }
            }


            // Track hits
            var trackHits = new double[row, column];
            var xArray = finalTrackXList.ToArray();
            var yArray = finalTrackYList.ToArray();

            for (int i = 0; i < closedTrackList.Count; i++)
            {
                var xIndex = xArray[i];
                var yIndex = yArray[i];
                for (int j = 0; j < closedTrackXList[i].Count; j++)
                {
                    trackHits[(int) yIndex[j] , xIndex[j]] = 1;
                }
            }

            // Count the number of track hits in each frequency band
            var arrayResult = new double[row];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    if (result[i,j] == 1)
                    {
                        arrayResult[i]++;
                    }
                }

            }
            return Tuple.Create(arrayResult, result, entropy, trackHits);
        }

    }
}