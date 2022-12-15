using ColorController.Enums;
using ColorController.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace ColorController.Helpers
{
    public class Constants
    {
        public static string BinaryUrl = "http://files.lightmodehelmets.com/app_bin/lm_ctrller.bin";
      
        public static List<AnimationModel> GetAnimations()
        {
            var selectedColorJson = JsonConvert.SerializeObject(App.CurrentSelectedColor);
            Preferences.Set("SelectedColor", selectedColorJson);

            var animations = new List<AnimationModel>
            {
                //Test Animations-Start
                //new AnimationModel{ Id = "001", ControllerVersion = "3.1.10", Command = "PATT 1080", Code="1080", Title= "PATT 1080", Detail="Portal 2 description", AnimationType = AnimationType.Portal, BaseColorJson = selectedColorJson, FileName = "ColorController.portal.lmd"},
                //new AnimationModel{ Id = "002", ControllerVersion = "3.1.10", Command = "PATT 1081", Code="1081", Title= "PATT 1081", Detail="Robocop 2 description", AnimationType = AnimationType.Robocop, BaseColorJson = selectedColorJson, FileName = "ColorController.Robocop.lmd"},
                //Test Animations-End
                
                new AnimationModel{ Id = "100", ControllerVersion = "3.1.9", Command = "PATT 0", Title= "Solid", Detail="Solid description", AnimationType = AnimationType.Solid, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "105", ControllerVersion = "3.1.9", Command = "PATT 5", Title= "Audialize", Detail="Audialize description", AnimationType = AnimationType.Audialize, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "111", ControllerVersion = "3.1.9", Command = "PATT 1001", Code="1001", Title= "Robocop", Detail="Robocop description", AnimationType = AnimationType.Robocop, BaseColorJson = selectedColorJson, FileName = "ColorController.Robocop.lmd"},
                new AnimationModel{ Id = "112", ControllerVersion = "3.1.9", Command = "PATT 1002", Title= "Chromatic", Detail="Chromatic description", AnimationType = AnimationType.Chromatic, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "113", ControllerVersion = "3.1.9", Command = "PATT 1003", Title= "Spectrum", Detail="Spectrum description", AnimationType = AnimationType.Spectrum, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "114", ControllerVersion = "3.1.9", Command = "PATT 1004", Title= "Wave", Detail="Wave description", AnimationType = AnimationType.Wave, BaseColorJson = selectedColorJson},
                //SuberWave
                new AnimationModel{ Id = "115", ControllerVersion = "3.1.9", Command = "PATT 1005", Title= "Portal", Detail="SuperWave description", AnimationType = AnimationType.Portal, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "116", ControllerVersion = "3.1.9", Command = "PATT 1006", Code="1006", Title= "Cybernetic", Detail="Cybernetic description", AnimationType = AnimationType.Cybernetic, BaseColorJson = selectedColorJson, FileName = "ColorController.cybernetic.lmd"},
                new AnimationModel{ Id = "117", ControllerVersion = "3.1.9", Command = "PATT 1007", Code="1007", Title= "Meteor", Detail="Meteor description", AnimationType = AnimationType.Meteor, BaseColorJson = selectedColorJson, FileName = "ColorController.Meteor.lmd"},
                new AnimationModel{ Id = "118", ControllerVersion = "3.1.9", Command = "PATT 1008", Title= "Fire Power", Detail="FirePower description", AnimationType = AnimationType.FirePower, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "119", ControllerVersion = "3.1.9", Command = "PATT 1009", Title= "Technetium", Detail="Technetium description", AnimationType = AnimationType.Technetium, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "120", ControllerVersion = "3.1.9", Command = "PATT 1010", Title= "Looper", Detail="Looper description", AnimationType = AnimationType.Looper, BaseColorJson = selectedColorJson , FileName = "ColorController.Looper.lmd", Code="1010"},
                //PATT 1011		patt_1011.lmd		Hazards
                new AnimationModel{ Id = "121", ControllerVersion = "3.1.9", Command = "PATT 1011", Title= "Hazards", Detail="Hazards description", AnimationType = AnimationType.Hazards, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "122", ControllerVersion = "3.1.9", Command = "PATT 1012", Title= "Night Rider", Detail="NightRider description", AnimationType = AnimationType.NightRider, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "123", ControllerVersion = "3.1.9", Command = "PATT 1013", Title= "Fireball", Detail="FireBall description", AnimationType = AnimationType.Fireball, BaseColorJson = selectedColorJson, FileName = "ColorController.Fireball.lmd", Code = "1013"},
                //PATT 1014		patt_1014.lmd		Ying Yang
                new AnimationModel{ Id = "124", ControllerVersion = "3.1.9", Command = "PATT 1014", Title= "Yin Yang", Detail="Yin Yang description", AnimationType = AnimationType.YingYang, BaseColorJson = selectedColorJson},
                new AnimationModel{ Id = "125", ControllerVersion = "3.1.9", Command = "PATT 1015", Title= "Black Hole", Detail="BlackHole description", AnimationType = AnimationType.BlackHole, BaseColorJson = selectedColorJson},
                //PATT 1016		patt_1016.lmd		Solid (backup) 
                new AnimationModel{ Id = "127", ControllerVersion = "3.1.9", Command = "PATT 1017", Title= "Beacon", Detail="Beacon description", AnimationType = AnimationType.Beacon, BaseColorJson = selectedColorJson },
                new AnimationModel{ Id = "128", ControllerVersion = "3.1.9", Command = "PATT 1018", Title= "Oxygen", Detail="Oxygen description", AnimationType = AnimationType.Breathe, BaseColorJson = selectedColorJson},
                //PATT 1019       patt_1019.lmd       Sun Fade
                new AnimationModel{ Id = "129", ControllerVersion = "3.1.9", Command = "PATT 1019", Title= "Sun Fade", Detail="Sun Fade description", AnimationType = AnimationType.SunFade, BaseColorJson = selectedColorJson},
                
                //30-July-2022 : v3.1.15
                //new AnimationModel{ Id = "1020", ControllerVersion = "3.1.15", Command = "PATT 1020", Title= "Bombs Away", Detail="Bombs Away description", AnimationType = AnimationType.Bombs_Away, BaseColorJson = selectedColorJson, FileName = "ColorController.Bombs_Away.lmd", Code = "1020"},
                //new AnimationModel{ Id = "1021", ControllerVersion = "3.1.15", Command = "PATT 1021", Title= "Pulsar", Detail="Pulsar description", AnimationType = AnimationType.Pulsar, BaseColorJson = selectedColorJson, FileName = "ColorController.Pulsar.lmd", Code = "1021"},
                //new AnimationModel{ Id = "1022", ControllerVersion = "3.1.15", Command = "PATT 1022", Title= "Diffraction", Detail="Diffraction description", AnimationType = AnimationType.Diffraction, BaseColorJson = selectedColorJson, FileName = "ColorController.Diffraction.lmd", Code = "1022"},
                //new AnimationModel{ Id = "1023", ControllerVersion = "3.1.15", Command = "PATT 1023", Title= "Star Power", Detail="Star Power description", AnimationType = AnimationType.Star_Power, BaseColorJson = selectedColorJson, FileName = "ColorController.Star_Power.lmd", Code = "1023"},
                //new AnimationModel{ Id = "1024", ControllerVersion = "3.1.15", Command = "PATT 1024", Title= "Dual Meteor", Detail="Dual Meteor description", AnimationType = AnimationType.Dual_Meteor, BaseColorJson = selectedColorJson, FileName = "ColorController.Dual_Meteor.lmd", Code = "1024"},
                //new AnimationModel{ Id = "1025", ControllerVersion = "3.1.15", Command = "PATT 1025", Title= "Orbit", Detail="Orbit description", AnimationType = AnimationType.Orbit, BaseColorJson = selectedColorJson, FileName = "ColorController.Orbit.lmd", Code = "1025"},
                //new AnimationModel{ Id = "1026", ControllerVersion = "3.1.15", Command = "PATT 1026", Title= "Initialize", Detail="Initialize description", AnimationType = AnimationType.Initialize_V3, BaseColorJson = selectedColorJson, FileName = "ColorController.Initialize.lmd", Code = "1026"},
                //new AnimationModel{ Id = "1027", ControllerVersion = "3.1.15", Command = "PATT 1027", Title= "Blockchain", Detail="Blockchain description", AnimationType = AnimationType.Blockchain, BaseColorJson = selectedColorJson, FileName = "ColorController.Blockchain.lmd", Code = "1027"},
            };

            return animations;
        }
    }
}
