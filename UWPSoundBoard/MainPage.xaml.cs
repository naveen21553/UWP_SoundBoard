using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPSoundBoard.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPSoundBoard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Sound> Sounds;
        private List<string> AllSounds = new List<string>();
        private List<MenuItem> MenuItems;
        private TimeSpan TotalTime;
        private TimeSpan RunningTime = new TimeSpan(0,0,0);
        public DispatcherTimer timerPlayTime { get; private set; }


        public MainPage()
        {
            this.InitializeComponent();

            Sounds = new ObservableCollection<Sound>();
            SoundManager.GetAllSounds(Sounds);

            BackButton.Visibility = Visibility.Collapsed;

            MenuItems = new List<MenuItem>();
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/animals.png", Category = SoundCategory.Animals });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/cartoon.png", Category = SoundCategory.Cartoons });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/taunt.png", Category = SoundCategory.Taunts });
            MenuItems.Add(new MenuItem { IconFile = "Assets/Icons/warning.png", Category = SoundCategory.Warnings });

            Sounds.ToList().ForEach(p => AllSounds.Add(p.Name));
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.GetAllSounds(Sounds);
            MenuListView.SelectedItem = null;
            CategoryTextBlock.Text = "All Sounds";
            BackButton.Visibility = Visibility.Collapsed;
        }

        private void SoundSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if(string.IsNullOrEmpty(SoundSuggestBox.Text))
            {
                SoundManager.GetAllSounds(Sounds);
                CategoryTextBlock.Text = "All Sounds";
                MenuListView.SelectedItem = null;
            }
            SoundSuggestBox.ItemsSource = AllSounds.Where(p => p.ToLower().Contains(SoundSuggestBox.Text.ToLower())).Any()?
                AllSounds.Where(p => p.ToLower().Contains(SoundSuggestBox.Text.ToLower())) : new List<string> { "No Results"};
            
        }

        private void SoundSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            SoundManager.GetAllSounds(Sounds);
            var filteredItems = Sounds.Where(p => p.Name.ToLower() == SoundSuggestBox.Text.ToLower()).ToList();
            if (filteredItems.Any())
            {
                Sounds.Clear();
                filteredItems.ForEach(p => Sounds.Add(p));
                BackButton.Visibility = Visibility.Visible;
                CategoryTextBlock.Text = SoundSuggestBox.Text;
                MenuListView.SelectedItem = null;
            }
        }

        private void SoundSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            SoundManager.GetAllSounds(Sounds);
            var item = Sounds.Where(p => p.Name.ToLower() == args.SelectedItem.ToString().ToLower()).ToList();
            Sounds.Clear();
            item.ForEach(p => Sounds.Add(p));
            MenuListView.SelectedItem = null;
            BackButton.Visibility = Visibility.Visible;
        }

        private void MenuListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SoundCategory Category = (SoundCategory)(((MenuItem)e.ClickedItem).Category);
            CategoryTextBlock.Text = Category.ToString();
            SoundManager.GetSoundsByCategory(Sounds, Category);
            BackButton.Visibility = Visibility.Visible;
            MySplitView.IsPaneOpen = false;
        }

        private void SoundGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var sound = (Sound)e.ClickedItem;
            MyMediaPlayer.Source = new Uri(this.BaseUri, sound.AudioFile);
        }

        private void SoundGridView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;

            e.DragUIOverride.Caption = "Drop here to play the track";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;

        }

        private async void SoundGridView_Drop(object sender, DragEventArgs e)
        {
            if(e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if(items.Any())
                {
                    StorageFile storageFile = items[0] as StorageFile;
                    var contentType = storageFile.ContentType;

                    if(contentType == "audio/mpeg" || contentType == "audio/wav")
                    {
                        MyMediaPlayer.SetSource(await storageFile.OpenAsync(FileAccessMode.Read), contentType);
                        MyMediaPlayer.Play();
                        PlayPauseButton.Content = "\xE769";
                        
                    }
                }
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (PlayPauseButton.Content.ToString() == "\xE768")
            {
                PlayPauseButton.Content = "\xE769";
                if(RunningTime != TotalTime)
                {
                    MyMediaPlayer.Position = RunningTime;
                    timerPlayTime.Start();
                    MyMediaPlayer.Play();

                }

            }
            else if (PlayPauseButton.Content.ToString() == "\xE769")
            {
                PlayPauseButton.Content = "\xE768";
                MyMediaPlayer.Stop();
                timerPlayTime.Stop();
            }
            
        }

        private void MyMediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayPauseButton.Content = "\xE768";
        }

        private void MyMediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            TotalTime = MyMediaPlayer.NaturalDuration.TimeSpan;
            TotalTimeTextBlock.Text = TotalTime.Hours.ToString() + ":" + TotalTime.Minutes.ToString() + ":" + TotalTime.Seconds.ToString();
            MediaSlider.Maximum = TotalTime.TotalSeconds;
            timerPlayTime = new DispatcherTimer();
            timerPlayTime.Interval = TimeSpan.FromSeconds(1);
            timerPlayTime.Tick += delegate
            {
                if (MyMediaPlayer.NaturalDuration.TimeSpan.TotalSeconds > 0)
                {
                    if (TotalTime.TotalSeconds > 0)
                    {
                        MediaSlider.Value = MyMediaPlayer.Position.TotalSeconds;
                        RunningTime += new TimeSpan(0, 0, 1);
                        CurrentTimeTextBlock.Text = RunningTime.Hours.ToString() + ":" +  RunningTime.Minutes.ToString() + ":" + RunningTime.Seconds.ToString();
                    }
                }
            };
            timerPlayTime.Start();
        }


    }
}
