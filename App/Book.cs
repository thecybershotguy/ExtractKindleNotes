using Newtonsoft.Json;
using ProjectTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExtractKindleNotes
{
    public class Book : BaseClass, IEquatable<Book>
    {
        #region Private Fields

        private List<string> _newlineFormatedData;
        private string _author;
        private string _title;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get => _title; set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        /// <value>
        /// The author.
        /// </value>
        public string Author
        {
            get => _author; set
            {
                _author = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();

        /// <summary>
        /// Gets the unprocessed notes.
        /// </summary>
        /// <value>
        /// The unprocessed notes.
        /// </value>
        [JsonIgnore] public HashSet<Note> UnprocessedNotes { get; private set; } = new HashSet<Note>();

        #endregion Public Properties

        #region Public Constructors


        [JsonConstructor]
        public Book()
        {

        }


        public Book(string rawData) : base(nameof(Book))
        {
            StoreRawDataAsCSVLine(rawData);

           

            ExtractTitle();
            ExtractAuthor();
        }

        #endregion Public Constructors

        #region Public Methods

        public static bool operator !=(Book left, Book right)
        {
            return !(left == right);
        }

        public static bool operator ==(Book left, Book right)
        {
            return EqualityComparer<Book>.Default.Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Book);
        }

        public bool Equals(Book other)
        {
            return other != null &&
                   Author == other.Author &&
                   Title == other.Title;
        }

        public override int GetHashCode()
        {
            int hashCode = 507744655;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Author);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            return hashCode;
        }

        public override string ToString() => $"Title: {Title} - Author: {Author}";

        public void UpdateNotes(string rawData, bool bookAlreadyExisits)
        {
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentException($"'{nameof(rawData)}' cannot be null or empty", nameof(rawData));

            try
            {
                if (bookAlreadyExisits)
                    StoreRawDataAsCSVLine(rawData);

                ExtractNotes();
            }
            catch (Exception exc)
            {
                LogError(exc);
                throw;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Extracts the author.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <exception cref="ArgumentException">rawData</exception>
        private void ExtractAuthor()
        {
            try
            {
                if (string.IsNullOrEmpty(_newlineFormatedData[2]) is false)
                {
                    var authorName = _newlineFormatedData[2].Split(',').First().ToLower().Trim('"');

                    if (string.IsNullOrEmpty(authorName) is false)
                        Author = Tools.ToTitleCase(authorName);

                    // Yay! found the Author
                    return;
                }

                throw new InvalidDataException($"Could not extract Author from provided input: {nameof(_newlineFormatedData)}");
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
        private void ExtractNotes()
        {
            try
            {
                // Figure out why I did this
                var linesNeeded = _newlineFormatedData.Count - 1 - 8;
                
                // First 8 lines are garbage
                var unformatedNotes = _newlineFormatedData.Skip(8).Take(linesNeeded).ToList();

                var locationsAlreadyExist = Notes.Select(s => s.Location).ToList();

                List<HighlightNoteAndLocation> notesToAdd = unformatedNotes
                    .Select(line => new HighlightNoteAndLocation(line))
                    .Where(s => locationsAlreadyExist.Contains(s.Location) == false).ToList();

                foreach (var item in notesToAdd)
                    Notes.Add(new Note(item.Location, item.Text));
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
        private void ExtractTitle()
        {
            try
            {

                if (string.IsNullOrEmpty(_newlineFormatedData[1]) is false)
                {
                    var title = _newlineFormatedData[1].Split(',').First().ToLower().Trim('"'); ;

                    if (string.IsNullOrEmpty(title) is false)
                        Title = Tools.ToTitleCase(title);
                    // Yay , found the title
                    return;
                }

                throw new InvalidDataException($"Could not extract Title from provided input");
            }
            catch (Exception exc)
            {
                LogError(exc, $"This is really bad , time to change things, I hate hard-coded stuff. ----> {nameof(ExtractTitle)}");
                throw;
            }
        }

        private void StoreRawDataAsCSVLine(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentException($"'{nameof(rawData)}' cannot be null or empty", nameof(rawData));

            _newlineFormatedData = rawData.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            if (_newlineFormatedData is null)
                throw new InvalidDataException($"Data could not be formated");

            if (_newlineFormatedData.Count <= 8)
                throw new InvalidDataException($"Time to change the logic for extracting lines from CSV data, Amazon Kindle CSV format has changed. Data received: {_newlineFormatedData.Fuse()} ; Tried to parse: {rawData.Fuse()}");
        }

        #endregion Private Methods
    }
}
