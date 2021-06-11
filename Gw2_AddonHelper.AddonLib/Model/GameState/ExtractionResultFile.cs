using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gw2_AddonHelper.AddonLib.Model.GameState
{
    public class ExtractionResultFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _fileName;
        private string _relativePath;
        private byte[] _fileContent;

        [JsonProperty("file_name")]
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value; Notify();
            }
        }

        [JsonProperty("relative_path")]
        public string RelativePath
        {
            get => _relativePath;
            set
            {
                _relativePath = value; Notify();
            }
        }

        [JsonIgnore]
        public byte[] FileContent
        {
            get => _fileContent;
            set
            {
                _fileContent = value; Notify();
            }
        }
    }
}
