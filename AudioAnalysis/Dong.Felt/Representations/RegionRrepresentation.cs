﻿
namespace Dong.Felt.Representations
{
    using AudioAnalysisTools;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class RegionRerepresentation
    {
        /// <summary>
        /// Index (0-based) for this region's lowest frequency in the source audio file, its unit is .
        /// </summary>
        //public int AudioFrequencyIndex { get; private set; }
        public double FrequencyIndex { get; set; }

        /// <summary>
        /// Index (0-based) for the time where this region starts located in the source audio file.
        /// </summary>
        //public int AudioTimeIndex { get; private set; }
        public double TimeIndex { get; set; }

        public int NhCountInRow { get; set; }

        public int NhCountInCol { get; set; }


        //// frequency range, total duration
        //public double maxFrequency { get; private set; }

        //public double minFrequency { get; private set; }

        //public double startTime { get; private set; }

        //public double endTime { get; private set; }

        //public double frequencyRange { get; private set; }

        //public double duration { get; private set; }

        /// <summary>
        /// Gets or sets the sourceAudioFile which contains the region.  
        /// </summary>
        public FileInfo SourceAudioFile { get; private set; }

        public FileInfo SourceTextFile { get; private set; }

        public List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNeighbourhoods;

        public ICollection<RidgeDescriptionNeighbourhoodRepresentation> ridgeNeighbourhood
        {
            get
            {
                // create a new list , so that only this class can change the list of neighbourhoods.
                return new List<RidgeDescriptionNeighbourhoodRepresentation>(this.ridgeNeighbourhoods);
            }
        }

        public ICollection<RidgeDescriptionNeighbourhoodRepresentation> RidgeNeighbourhoods { get; set; }

        #region  public constructor

        public RegionRerepresentation(List<RidgeDescriptionNeighbourhoodRepresentation> neighbourhoods,
            int nhCountInRow, int nhCountInCol, FileInfo audioFile)
        {
            this.ridgeNeighbourhoods = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            foreach (var nh in neighbourhoods)
            {
                this.ridgeNeighbourhoods.Add(nh);
            }
            // left bottom corner - this is the anchor point
            this.FrequencyIndex = neighbourhoods[0].RowIndex;
            this.TimeIndex = neighbourhoods[0].ColIndex;

            this.NhCountInRow = nhCountInRow;
            this.NhCountInCol = nhCountInCol;
            this.SourceAudioFile = audioFile;
        }

        #endregion
    }
}