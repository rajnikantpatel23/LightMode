using SQLite;

namespace ColorController.Models
{
    public class Controller : BaseModel
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSelected { get; set; }

        [Ignore]
        public string IsDefaultString => IsDefault == true ? "(Default)" : string.Empty;
    }
}
