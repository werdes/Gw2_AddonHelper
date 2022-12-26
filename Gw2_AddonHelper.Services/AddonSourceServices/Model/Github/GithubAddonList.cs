using Gw2_AddonHelper.Common.Model.AddonList;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Services.AddonSourceServices.Model.Github
{
    public class GithubAddonList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public GithubAddonList()
        {
            _addons = new ObservableCollection<Addon>();
        }

        private ObservableCollection<Addon> _addons;

        [JsonProperty("addons")]
        public ObservableCollection<Addon> Addons
        {
            get => _addons;
            set
            {
                _addons = value;
                Notify();
            }
        }

        private string _commitSha;
        [JsonProperty("commit_sha")]
        public string CommitSha
        {
            get => _commitSha;
            set
            {
                _commitSha = value;
                Notify();
            }
        }

        private DateTime _retrievedAt;
        [JsonProperty("retrieved_at")]
        public DateTime RetrievedAt
        {
            get => _retrievedAt;
            set
            {
                _retrievedAt = value;
                Notify();
            }
        }
    }
}
