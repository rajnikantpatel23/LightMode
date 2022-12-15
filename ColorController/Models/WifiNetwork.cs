namespace ColorController.Models
{
    public class WifiNetwork : BaseModel
    {
        public string Name { get; set; }

        private int _strength;
        public int Strength
        {
            get { return _strength; }
            set
            {
                _strength = value;
                OnPropertyChanged(nameof(Strength));
                OnPropertyChanged(nameof(SignalStrength));
            }
        }

        public string SignalStrength 
        {
            get 
            {
                if (Strength <= 0 && Strength >= -55)
                {
                    //Best signal
                    return "Wifi3Bars.png";
                }
                else if (Strength < -55 && Strength >= -75)
                {
                    //Low signal
                    return "Wifi2Bars.png";
                }
                else if (Strength < -75 && Strength >= -100)
                {
                    //Very weak signal
                    return "Wifi1Bar.png";
                }
                else
                {
                    // No signals
                    return "Wifi0Bars.png";
                }
            }
        } 
    }
}
