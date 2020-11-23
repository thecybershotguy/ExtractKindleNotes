using ProjectTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExtractKindleNotes
{
    public class Book : BaseClass
    {
        #region Properties and Fields
        public string Author { get; set; }
        public Dictionary<string, string> Notes { get; set; } = new Dictionary<string, string>();
        public TextInfo TextInfo { get; }
        public string Title { get; set; }
        #endregion

        #region Constructor
        public Book(string rawData) : base(nameof(Book))
        {
            var newlineFormatedData = rawData.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (newlineFormatedData is null)
                throw new InvalidDataException($"Data could not be formated");

            if (newlineFormatedData.Length <= 8)
                throw new InvalidDataException($"Time to change the logic for extracting lines from CSV data, Amazon Kindle CSV format has changed. Data received: {newlineFormatedData.Fuse()} ; Tried to parse: {rawData.Fuse()}");


            TextInfo = new CultureInfo("en-US").TextInfo;
            ParseRawCSVData(newlineFormatedData);
        }

        #endregion

        public override string ToString() => $"Title: {Title} - Author: {Author}";

        /// <summary>
        /// Extracts the author.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <exception cref="ArgumentException">rawData</exception>
        private void ExtractAuthor(string[] rawData)
        {
            try
            {
                if (rawData is null)
                    throw new ArgumentException($"{nameof(rawData)} is null or empty at {nameof(ExtractAuthor)}", nameof(rawData));

                if (string.IsNullOrEmpty(rawData[2]) is false)
                {
                    var authorName = rawData[2].Split(',').First().ToLower();


                    if (string.IsNullOrEmpty(authorName) is false)
                        Author = TextInfo.ToTitleCase(authorName);
                }
            }
            catch (Exception exc)
            {
                LogError(exc, $"This is really bad , time to change things, I hate hard-coded stuff. ----> {nameof(ExtractAuthor)}");
                throw;
            }
        }

        /// <summary>
        /// Extracts the notes highlighted.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <exception cref="ArgumentException">rawData</exception>
        /// <exception cref="InvalidDataException">
        /// Time to change the logic for extracting (Highlights, Location and Notes), Amazon Kindle CSV format has changed. Data received: {unformatedNotes.Fuse()} ; Trying to parse: {locationAndNotes.Fuse()} ; Book Details: {this}
        /// or
        /// Time to change the logic for extracting Location number for notes, Amazon Kindle CSV format has changed. Data received: {unformatedNotes.Fuse()} ; Trying to parse: {locationArray.Fuse()} ; Book Details: {this}
        /// </exception>
        private void ExtractNotes(string[] rawData)
        {
            try
            {
                if (rawData is null)
                    throw new ArgumentException($"{nameof(rawData)} is null or empty at {nameof(ExtractNotes)}", nameof(rawData));

                var linesNeeded = rawData.Length - 1 - 8;
                var unformatedNotes = rawData.Skip(8).Take(linesNeeded).ToArray();

                foreach (var item in unformatedNotes)
                {

                    var locationAndNotes = item.Split('"').Where(s => !string.IsNullOrEmpty(s)).Where(k => k != ",").Skip(1).ToArray();

                    if (locationAndNotes.Length < 2)
                        throw new InvalidDataException($"Time to change the logic for extracting (Highlights, Location and Notes), Amazon Kindle CSV format has changed. Data received: {unformatedNotes.Fuse()} ; Trying to parse: {locationAndNotes.Fuse()} ; Book Details: {this}");

                    var locationArray = locationAndNotes[0].Split(' ').ToArray();

                    if (locationArray.Length < 2)
                        throw new InvalidDataException($"Time to change the logic for extracting Location number for notes, Amazon Kindle CSV format has changed. Data received: {unformatedNotes.Fuse()} ; Trying to parse: {locationArray.Fuse()} ; Book Details: {this}");

                    var location = locationArray[1];

                    var note = TextInfo.ToTitleCase(locationAndNotes[1].ToLower());

                    if (Notes.ContainsKey(location) is false)
                        Notes.Add(location, note);
                }
            }
            catch (Exception exc)
            {
                LogError(exc, $"This is really bad , time to change things, I hate hard-coded stuff. ----> {nameof(ExtractNotes)}");
                throw;
            }
        }



        /// <summary>
        /// Extracts the notes highlighted.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <exception cref="ArgumentException">rawData</exception>
        /// <exception cref="InvalidDataException">
        /// Time to change the logic for extracting (Highlights, Location and Notes), Amazon Kindle CSV format has changed. Data received: {unformatedNotes.Fuse()} ; Trying to parse: {locationAndNotes.Fuse()} ; Book Details: {this}
        /// or
        /// Time to change the logic for extracting Location number for notes, Amazon Kindle CSV format has changed. Data received: {unformatedNotes.Fuse()} ; Trying to parse: {locationArray.Fuse()} ; Book Details: {this}
        /// </exception>
        private void ExtractNotes(string rawData)
        {
            try
            { 
                if (string.IsNullOrEmpty(rawData))
                    throw new ArgumentException($"{nameof(rawData)} is null or empty at {nameof(ExtractNotes)}", nameof(rawData));

                var newlineFormatedData = rawData.Split(new[] { Environment.NewLine }, StringSplitOptions.None);


                var linesNeeded = newlineFormatedData.Length - 1 - 8;
                var unformatedNotes = newlineFormatedData.Skip(8).Take(linesNeeded).ToArray();

                foreach (var item in unformatedNotes)
                {

                    var locationAndNotes = item.Split('"').Where(s => !string.IsNullOrEmpty(s)).Where(k => k != ",").Skip(1).ToArray();

                    if (locationAndNotes.Length < 2)
                        throw new InvalidDataException($"Time to change the logic for extracting (Highlights, Location and Notes), Amazon Kindle CSV format has changed. Data received: {unformatedNotes.Fuse()} ; Trying to parse: {locationAndNotes.Fuse()} ; Book Details: {this}");

                    var locationArray = locationAndNotes[0].Split(' ').ToArray();

                    if (locationArray.Length < 2)
                        throw new InvalidDataException($"Time to change the logic for extracting Location number for notes, Amazon Kindle CSV format has changed. Data received: {unformatedNotes.Fuse()} ; Trying to parse: {locationArray.Fuse()} ; Book Details: {this}");

                    var location = locationArray[1];

                    var note = TextInfo.ToTitleCase(locationAndNotes[1].ToLower());

                    if (Notes.ContainsKey(location) is false)
                        Notes.Add(location, note);
                }
            }
            catch (Exception exc)
            {
                LogError(exc, $"This is really bad , time to change things, I hate hard-coded stuff. ----> {nameof(ExtractNotes)}");
                throw;
            }
        }

        /// <summary>
        /// Extracts the title of the book.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <exception cref="ArgumentException">rawData</exception>
        private void ExtractTitle(string[] rawData)
        {
            try
            {
                if (rawData is null)
                    throw new ArgumentException($"{nameof(rawData)} is null or empty at {nameof(ExtractTitle)}", nameof(rawData));


                if (string.IsNullOrEmpty(rawData[1]) is false)
                {
                    var title = rawData[1].Split(',').First().ToLower();


                    if (string.IsNullOrEmpty(title) is false)
                    {
                        Title = TextInfo.ToTitleCase(title);
                    }
                }
            }
            catch (Exception exc)
            {
                LogError(exc, $"This is really bad , time to change things, I hate hard-coded stuff. ----> {nameof(ExtractTitle)}");
                throw;
            }
        }


        /// <summary>
        /// Parses the raw CSV data.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <exception cref="ArgumentException">message - rawData</exception>
        private void ParseRawCSVData(string[] rawData)
        {
            try
            {
                if (rawData is null)
                    throw new ArgumentException("message", nameof(rawData));

                ExtractTitle(rawData);
                ExtractAuthor(rawData);
                ExtractNotes(rawData);
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }
    }
}
