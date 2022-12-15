using SQLite;
using System;
using System.ComponentModel;

namespace ColorController.Models
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        [PrimaryKey]
        public string Id { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        [Ignore]
        public byte[] Version { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
