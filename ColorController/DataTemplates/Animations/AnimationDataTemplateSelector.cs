using ColorController.Models;
using Xamarin.Forms;

namespace ColorController.DataTemplates
{
    public class AnimationDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SolidDataTemplate { get; set; }
        public DataTemplate RobocopDataTemplate { get; set; }
        public DataTemplate BeaconDataTemplate { get; set; }
        public DataTemplate BreatheDataTemplate { get; set; }
        public DataTemplate CyberneticDataTemplate { get; set; }
        public DataTemplate MeteorDataTemplate { get; set; }
        public DataTemplate NightRiderDataTemplate { get; set; }
        public DataTemplate SuperWaveDataTemplate { get; set; }
        public DataTemplate TechnetiumDataTemplate { get; set; }
        public DataTemplate WaveDataTemplate { get; set; }
        public DataTemplate FirePowerDataTemplate { get; set; }
        public DataTemplate FireballDataTemplate { get; set; }
        public DataTemplate SpectrumDataTemplate { get; set; }
        public DataTemplate LooperDataTemplate { get; set; }
        public DataTemplate AudializeDataTemplate { get; set; }
        public DataTemplate ChromaticDataTemplate { get; set; }
        public DataTemplate BlackHoleDataTemplate { get; set; }
        public DataTemplate HazardsDataTemplate { get; set; }
        public DataTemplate SunFadeDataTemplate { get; set; }
        public DataTemplate YingYangDataTemplate { get; set; }
        //New 30-July-2022
        public DataTemplate BlockchainDataTemplate { get; set; }
        public DataTemplate Bombs_AwayDataTemplate { get; set; }
        public DataTemplate DiffractionDataTemplate { get; set; }
        public DataTemplate Dual_MeteorDataTemplate { get; set; }
        public DataTemplate Initialize_V3DataTemplate { get; set; }
        public DataTemplate OrbitDataTemplate { get; set; }
        public DataTemplate PulsarDataTemplate { get; set; }
        public DataTemplate Star_PowerDataTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            DataTemplate dataTemplate = null;
            if (item.GetType() == typeof(AnimationModel))
            {
                switch (((AnimationModel)item).AnimationType)
                {
                    case Enums.AnimationType.Solid:
                        dataTemplate = SolidDataTemplate;
                        break;
                    case Enums.AnimationType.Robocop:
                        dataTemplate = RobocopDataTemplate;
                        break;
                    case Enums.AnimationType.Rainbow:
                        dataTemplate = SolidDataTemplate;
                        break;
                    case Enums.AnimationType.Meteor:
                        dataTemplate = MeteorDataTemplate;
                        break;
                    case Enums.AnimationType.MaxVisibility:
                        dataTemplate = SolidDataTemplate;
                        break;
                    case Enums.AnimationType.Beacon:
                        dataTemplate = BeaconDataTemplate;
                        break;
                    case Enums.AnimationType.Breathe:
                        dataTemplate = BreatheDataTemplate;
                        break;
                    case Enums.AnimationType.Cybernetic:
                        dataTemplate = CyberneticDataTemplate;
                        break;
                    case Enums.AnimationType.NightRider:
                        dataTemplate = NightRiderDataTemplate;
                        break;
                    case Enums.AnimationType.Portal:
                        dataTemplate = SuperWaveDataTemplate;
                        break;
                    case Enums.AnimationType.Technetium:
                        dataTemplate = TechnetiumDataTemplate;
                        break;
                    case Enums.AnimationType.Wave:
                        dataTemplate = WaveDataTemplate;
                        break;
                    case Enums.AnimationType.FirePower:
                        dataTemplate = FirePowerDataTemplate;
                        break;
                    case Enums.AnimationType.Fireball:
                        dataTemplate = FireballDataTemplate;
                        break;
                    case Enums.AnimationType.Looper:
                        dataTemplate = LooperDataTemplate;
                        break;
                    case Enums.AnimationType.Spectrum:
                        dataTemplate = SpectrumDataTemplate;
                        break; 
                    case Enums.AnimationType.Audialize:
                        dataTemplate = AudializeDataTemplate;
                        break; 
                    case Enums.AnimationType.Chromatic:
                        dataTemplate = ChromaticDataTemplate;
                        break; 
                    case Enums.AnimationType.BlackHole:
                        dataTemplate = BlackHoleDataTemplate;
                        break;
                    case Enums.AnimationType.Hazards:
                        dataTemplate = HazardsDataTemplate;
                        break;
                    case Enums.AnimationType.SunFade:
                        dataTemplate = SunFadeDataTemplate;
                        break;
                    case Enums.AnimationType.YingYang:
                        dataTemplate = YingYangDataTemplate;
                        break;
                       
                        //30-July-2022
                    case Enums.AnimationType.Blockchain:
                        dataTemplate = BlockchainDataTemplate;
                        break;
                    case Enums.AnimationType.Bombs_Away:
                        dataTemplate = Bombs_AwayDataTemplate;
                        break;
                    case Enums.AnimationType.Diffraction:
                        dataTemplate = DiffractionDataTemplate;
                        break;
                    case Enums.AnimationType.Dual_Meteor:
                        dataTemplate = Dual_MeteorDataTemplate;
                        break;
                    case Enums.AnimationType.Initialize_V3:
                        dataTemplate = Initialize_V3DataTemplate;
                        break;
                    case Enums.AnimationType.Orbit:
                        dataTemplate = OrbitDataTemplate;
                        break;
                    case Enums.AnimationType.Pulsar:
                        dataTemplate = PulsarDataTemplate;
                        break;
                    case Enums.AnimationType.Star_Power:
                        dataTemplate = Star_PowerDataTemplate;
                        break;

                    default:
                        dataTemplate = SolidDataTemplate;
                        break;
                }
            }
            if (item.GetType() == typeof(FavoriteAnimation))
            {
                switch (((FavoriteAnimation)item).AnimationType)
                {
                    case Enums.AnimationType.Solid:
                        dataTemplate = SolidDataTemplate;
                        break;
                    case Enums.AnimationType.Robocop:
                        dataTemplate = RobocopDataTemplate;
                        break;
                    case Enums.AnimationType.Rainbow:
                        dataTemplate = SolidDataTemplate;
                        break;
                    case Enums.AnimationType.Meteor:
                        dataTemplate = MeteorDataTemplate;
                        break;
                    case Enums.AnimationType.MaxVisibility:
                        dataTemplate = SolidDataTemplate;
                        break;
                    case Enums.AnimationType.FirePower:
                        dataTemplate = SolidDataTemplate;
                        break;
                    case Enums.AnimationType.Beacon:
                        dataTemplate = BeaconDataTemplate;
                        break;
                    case Enums.AnimationType.Breathe:
                        dataTemplate = BreatheDataTemplate;
                        break;
                    case Enums.AnimationType.Cybernetic:
                        dataTemplate = CyberneticDataTemplate;
                        break;
                    case Enums.AnimationType.NightRider:
                        dataTemplate = NightRiderDataTemplate;
                        break;
                    case Enums.AnimationType.Portal:
                        dataTemplate = SuperWaveDataTemplate;
                        break;
                    case Enums.AnimationType.Technetium:
                        dataTemplate = TechnetiumDataTemplate;
                        break;
                    case Enums.AnimationType.Wave:
                        dataTemplate = WaveDataTemplate;
                        break;
                    case Enums.AnimationType.Audialize:
                        dataTemplate = AudializeDataTemplate;
                        break;
                    case Enums.AnimationType.Chromatic:
                        dataTemplate = ChromaticDataTemplate;
                        break;
                    case Enums.AnimationType.BlackHole:
                        dataTemplate = BlackHoleDataTemplate;
                        break;
                    case Enums.AnimationType.Hazards:
                        dataTemplate = HazardsDataTemplate;
                        break;
                    case Enums.AnimationType.SunFade:
                        dataTemplate = SunFadeDataTemplate;
                        break;
                    case Enums.AnimationType.YingYang:
                        dataTemplate = YingYangDataTemplate;
                        break;

                    //30-July-2022
                    case Enums.AnimationType.Blockchain:
                        dataTemplate = BlockchainDataTemplate;
                        break;
                    case Enums.AnimationType.Bombs_Away:
                        dataTemplate = Bombs_AwayDataTemplate;
                        break;
                    case Enums.AnimationType.Diffraction:
                        dataTemplate = DiffractionDataTemplate;
                        break;
                    case Enums.AnimationType.Dual_Meteor:
                        dataTemplate = Dual_MeteorDataTemplate;
                        break;
                    case Enums.AnimationType.Initialize_V3:
                        dataTemplate = Initialize_V3DataTemplate;
                        break;
                    case Enums.AnimationType.Orbit:
                        dataTemplate = OrbitDataTemplate;
                        break;
                    case Enums.AnimationType.Pulsar:
                        dataTemplate = PulsarDataTemplate;
                        break;
                    case Enums.AnimationType.Star_Power:
                        dataTemplate = Star_PowerDataTemplate;
                        break;

                    default:
                        dataTemplate = SolidDataTemplate;
                        break;
                }
            }


            return dataTemplate;
        }
    }
}
