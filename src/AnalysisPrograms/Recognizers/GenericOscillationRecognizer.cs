// <copyright file="GenericOscillationRecognizer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Recognizers
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared.ConfigFile;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    public class GenericOscillationRecognizer
    {
        /// <summary>
        /// THis method does the work.
        /// </summary>
        /// <param name="audioRecording">the recording.</param>
        /// <param name="configuration">the config file.</param>
        /// <param name="profileName">name of call/event type to be found.</param>
        /// <param name="segmentStartOffset">where one segment is located in the total recording.</param>
        /// <returns>a list of events.</returns>
        public static RecognizerResults OscillationRecognizer(AudioRecording audioRecording, Config configuration, string profileName, TimeSpan segmentStartOffset)
        {
            ConfigFile.TryGetProfile(configuration, profileName, out var profile);

            // get the common properties
            string speciesName = configuration[AnalysisKeys.SpeciesName] ?? "Pteropus species";
            string abbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "Pteropus";

            // The following parameters worked well on a ten minute recording containing 14-16 calls.
            // Note: if you lower the dB threshold, you need to increase maxDurationSeconds
            int minHz = profile.GetIntOrNull(AnalysisKeys.MinHz) ?? 100;
            int maxHz = profile.GetIntOrNull(AnalysisKeys.MaxHz) ?? 3000;
            double minDurationSeconds = profile.GetDoubleOrNull(AnalysisKeys.MinDuration) ?? 1.0;
            double maxDurationSeconds = profile.GetDoubleOrNull(AnalysisKeys.MaxDuration) ?? 10.0;
            double decibelThreshold = profile.GetDoubleOrNull("DecibelThreshold") ?? 6.0;
            double dctDuration = profile.GetDoubleOrNull("DctDuration") ?? 1.0;
            double dctThreshold = profile.GetDoubleOrNull("DctThreshold") ?? 0.5;
            double minOscFreq = profile.GetDoubleOrNull("MinOscilFreq") ?? 4.0;
            double maxOscFreq = profile.GetDoubleOrNull("MaxOscilFreq") ?? 6.0;
            double eventThreshold = profile.GetDoubleOrNull("EventThreshold") ?? 0.3;

            //######################

            //2. Don't use samples in this recognizer.
            //var samples = audioRecording.WavReader.Samples;
            //Instead, convert each segment to a spectrogram.
            var sonogram = RecognizerTools.GetSonogram(configuration, audioRecording);
            var decibelArray = SNR.CalculateFreqBandAvIntensity(sonogram.Data, minHz, maxHz, sonogram.NyquistFrequency);

            // Call oscillation detector
            /*
            int scoreSmoothingWindow = 11; // sets a default that was good for Cane toad
            Oscillations2019.Execute(
            (SpectrogramStandard)sonogram,
                minHz,
                maxHz,
                decibelThreshold,
                dctDuration,
                (int)Math.Floor(minOscFreq),
                (int)Math.Floor(maxOscFreq),
                dctThreshold,
                eventThreshold,
                minDurationSeconds,
                maxDurationSeconds,
                scoreSmoothingWindow,
                out var scores,
                out var acousticEvents,
                //out var hits,
                segmentStartOffset);
*/
            Oscillations2012.Execute(
                (SpectrogramStandard)sonogram,
                minHz,
                maxHz,
                //decibelThreshold,
                dctDuration,
                (int)Math.Floor(minOscFreq),
                (int)Math.Floor(maxOscFreq),
                dctThreshold,
                eventThreshold,
                minDurationSeconds,
                maxDurationSeconds,
                out var scores,
                out var acousticEvents,
                out var hits,
                segmentStartOffset);

            // prepare plots
            double intensityNormalisationMax = 3 * decibelThreshold;
            var normThreshold = decibelThreshold / intensityNormalisationMax;
            var normalisedIntensityArray = DataTools.NormaliseInZeroOne(decibelArray, 0, intensityNormalisationMax);
            var plot1 = new Plot(speciesName + " Wing-beat band", normalisedIntensityArray, normThreshold);
            var plot2 = new Plot(speciesName + " Wing-beat Osc Score", scores, eventThreshold);
            var plots = new List<Plot> { plot1, plot2 };

            // ######################################################################

            // add additional information about the recording and sonogram properties from which the event is derived.
            acousticEvents.ForEach(ae =>
            {
                ae.FileName = audioRecording.BaseName;
                ae.SpeciesName = speciesName;
                ae.Name = abbreviatedSpeciesName + profileName;
                ae.Profile = profileName;
                ae.SegmentDurationSeconds = audioRecording.Duration.TotalSeconds;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                var frameOffset = sonogram.FrameStep;
                var frameDuration = sonogram.FrameDuration;
                ae.SetTimeAndFreqScales(frameOffset, frameDuration, sonogram.FBinWidth);

                //UNCOMMENT following lines to get spectral profiles of the Wingbeat events.
                /*    double[,] spectrogramData = sonogram.Data;
                    int maxBin = (int)Math.Round(8000 / sonogram.FBinWidth);
                    double startSecond = ae.EventStartSeconds - ae.SegmentStartSeconds;
                    int startFrame = (int)Math.Round(startSecond / sonogram.FrameStep);
                    int frameLength = (int)Math.Round(ae.EventDurationSeconds / sonogram.FrameStep);
                    int endFrame = startFrame + frameLength;

                    // get only the frames from centre of the acoustic event
                    var subMatrix = DataTools.Submatrix(spectrogramData, startFrame + 10, 0, endFrame - 10, maxBin);
                    var spectrum = MatrixTools.GetColumnAverages(subMatrix);
                    var normalisedSpectrum = DataTools.normalise(spectrum);
                    normalisedSpectrum = DataTools.filterMovingAverageOdd(normalisedSpectrum, 11);
                    var maxId = DataTools.GetMaxIndex(normalisedSpectrum);
                    var hzMax = (int)Math.Ceiling(maxId * sonogram.FBinWidth);
                    string name = "BeatSpectrum " + (ae.SegmentStartSeconds / 60) + "m" + (int)Math.Floor(startSecond) + "s hzMax" + hzMax;
                    var bmp2 = GraphsAndCharts.DrawGraph(name, normalisedSpectrum, 100);

                    //Set required path
                    bmp2.Save(Path.Combine(@"C:\PATH", name + ".png"));
                    */
            });

            return new RecognizerResults()
            {
                Events = acousticEvents,
                Hits = null,
                ScoreTrack = null,
                Plots = plots,
                Sonogram = sonogram,
            };
        }

    }
}
