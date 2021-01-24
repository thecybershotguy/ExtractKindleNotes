using ProjectTools;
using System;
using System.IO;
using System.Linq;

namespace ExtractKindleNotes
{
    public class HighlightNoteAndLocation : BaseClass
    {
        public string HighlightText { get; }

        public string Location { get; }
        public string Text { get;  }

        public HighlightNoteAndLocation(string rawCSVLine) : base(nameof(HighlightNoteAndLocation))
        {
            string[] locationNotesAndHighlight = rawCSVLine.Split(ParasingConstants.HIGHLIGHT_LOCATION_AND_NOTES_SEPRATOR).
                              Where(s => string.IsNullOrEmpty(s) == false && s != ParasingConstants.UNNECESSARY_CHARACTER)
                             .ToArray();
            try
            {
                // Removing all the unnecessary chunks of th data from raw string 

                if (locationNotesAndHighlight != null && locationNotesAndHighlight.Length < 3)
                    throw new InvalidDataException($"Time to change the logic for extracting (Highlights, Location and Notes), Amazon Kindle CSV format has changed. Extracted data: {locationNotesAndHighlight.Fuse()}");

                HighlightText = locationNotesAndHighlight[0];
                // Location string looks like this ("Location 584") so we split with white space and extract the location
                Location = locationNotesAndHighlight[1].Split(Tools.WhiteSpace)[1];
                Text = Tools.ToTitleCase(locationNotesAndHighlight[2]);
            }
            catch (IndexOutOfRangeException e)
            {
                LogError(e, $"Something might have gone wrong parsing this: {locationNotesAndHighlight.Fuse()}");
                throw;
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        public override string ToString()
        {
            return $"Location: {Location}, Note: {Text}, Highlight: {HighlightText}";
        }
    }
}
