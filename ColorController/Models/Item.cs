using System;

namespace ColorController.Models
{
    public class Item : BaseModel
    {
        public string Text { get; set; }

        public string Description { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }
    }
}