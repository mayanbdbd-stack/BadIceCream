using System;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace GameEngine.GameServices
{
    public static class MusicPlayer
    {
        public static MediaPlayer _mediaPlayer = new MediaPlayer(); //הנגן
        public static bool IsOn { get; set; } = false; //האם מוזיקת רקע מתנגנת
        private static double _volume = 0.5;           //הערך התחילי של עוצמת המוסיקה
        public static double Volume
        {
            set
            {
                _volume = value / 100;
                _mediaPlayer.Volume = _volume;
            }
            get
            {
                return _volume * 100;
            }
        }

        public static void Play(string fileName)
        {
            if (!IsOn)              //אם הנגן לא מנגן כעת
            {
                IsOn = true;
                _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///Assets/Music/{fileName}"));
                _mediaPlayer.IsLoopingEnabled = true;
                _mediaPlayer.Play();
            }
        }
        public static void Pause()
        {
            IsOn = false;
            _mediaPlayer.Pause();
        }
    }
}
