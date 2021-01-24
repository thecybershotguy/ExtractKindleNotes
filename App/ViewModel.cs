using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ExtractKindleNotes
{

    public class ViewModel
    {
        private readonly string _pathWithFileName;

        public ObservableCollection<Book> BooksRead { get; set; } = new ObservableCollection<Book>();

        [JsonIgnore]
        public Book FirstOrDefault { get => BooksRead.Count == 0 ? null : BooksRead[0]; }

        public ViewModel(string pathWithFileName)
        {

            if (string.IsNullOrEmpty(pathWithFileName))
                throw new System.ArgumentException($"'{nameof(pathWithFileName)}' cannot be null or empty", nameof(pathWithFileName));

            _pathWithFileName = pathWithFileName;

            if (File.Exists(pathWithFileName))
            {
                var fileContent = File.ReadAllText(pathWithFileName);
                BooksRead = new ObservableCollection<Book>(JsonConvert.DeserializeObject<List<Book>>(fileContent));
            }
        }

        public void CreateAndUpdateJsonDataBase()
        {
            var updatedBooks = JsonConvert.SerializeObject(BooksRead, Formatting.Indented);

            File.WriteAllText(_pathWithFileName, updatedBooks);
        }
    }
}