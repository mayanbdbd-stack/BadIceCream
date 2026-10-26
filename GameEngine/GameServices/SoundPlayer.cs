using System;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace GameEngine.GameServices
{
    public static class SoundPlayer
    {
        private static MediaPlayer _mediaPlayer = new MediaPlayer();
        public static bool IsOn { set; get; } = false; //האם האפקטים מושמעים

        /// <summary>
        /// הפעולה משמיע אפקט מוסיקלי בתנאי
        /// </summary>
        /// <param name="fileName">שם הקובץ להשמעה</param>
        public static void Play(string fileName)
        {
            if (IsOn)
            {
                _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///Assets/Sounds/{fileName}"));
                _mediaPlayer.Play();
            }
        }

        public static void Stop()
        {
            _mediaPlayer.Pause();
        }
    }
}
