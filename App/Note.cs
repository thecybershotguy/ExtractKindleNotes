using ProjectTools;
using System;
using System.Collections.Generic;

namespace ExtractKindleNotes
{
    public class Note : BaseClass
    {
        private string _text;
        private string _location;

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location
        {
            get => _location; set
            {
                _location = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get => _text; set
            {
                _text = value;
                NotifyPropertyChanged();
            }
        }

        public Note(string location, string text) : base(nameof(Note))
        {
            Location = location;
            Text = text;
        }
    }
}
