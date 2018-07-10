using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPSoundBoard.Models
{
    public class SoundManager
    {
        public static void GetAllSounds(ObservableCollection<Sound> sounds)
        {
            sounds.Clear();
            var AllSounds = GetSounds();
            AllSounds.ForEach(p => sounds.Add(p));
        }

        public static void GetSoundsByCategory(ObservableCollection<Sound> sounds, SoundCategory requiredCategory)
        {
            sounds.Clear();
            var AllSounds = GetSounds();
            var FilteredSounds = AllSounds.Where(p => p.Category == requiredCategory).ToList();

            FilteredSounds.ForEach(p => sounds.Add(p));
                
        }
        private static List<Sound> GetSounds()
        {
            var Sounds = new List<Sound>();

            Sounds.Add(new Sound("Cat", SoundCategory.Animals));
            Sounds.Add(new Sound("Cow", SoundCategory.Animals));
            Sounds.Add(new Sound("Gun", SoundCategory.Cartoons));
            Sounds.Add(new Sound("Spring", SoundCategory.Cartoons));
            Sounds.Add(new Sound("Clock", SoundCategory.Taunts));
            Sounds.Add(new Sound("LOL", SoundCategory.Taunts));
            Sounds.Add(new Sound("Ship", SoundCategory.Warnings));
            Sounds.Add(new Sound("Siren", SoundCategory.Warnings));

            return Sounds;
        }
    }
}
