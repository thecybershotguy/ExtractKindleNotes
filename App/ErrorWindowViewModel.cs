namespace ExtractKindleNotes
{
    /// <summary>
    /// View model for Error Window
    /// </summary>
    /// <seealso cref="ExtractKindleNotes.BaseClass" />
    public class ErrorWindowViewModel : BaseClass
    {
        /// <summary>
        /// Default Error message
        /// </summary>
        private string _errorMessage = "Error was not able to set";

        /// <summary>
        /// Gets or sets the error message for the Error window.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage
        {
            get => _errorMessage; 
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorWindowViewModel"/> class.
        /// </summary>
        public ErrorWindowViewModel() : base(nameof(ErrorWindowViewModel))
        {

        }
    }
}
