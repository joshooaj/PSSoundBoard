using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace PSSoundBoardLib
{
    /// <summary>
    /// The SoundBoard is a singleton class responsible for handling music and sound effects together.
    /// It ensures music plays on repeat, can be shuffled, and that sound effects can be heard by
    /// reducing the volume on the music player until the sound effect has finished.
    /// </summary>
    public class SoundBoard : IDisposable
    {
        private int _playlistPosition;
        private bool _shuffle;
        private readonly Random _rng = new Random();

        private readonly ObservableCollection<Uri> _playlist = new ObservableCollection<Uri>();
        private List<Uri> _setList = new List<Uri>();

        private double _musicVolume = 0.75;
        private double _effectsVolume = 1;

        /// <summary>
        /// Provides direct access to the SoundBoardPlayer instance controlling music playback.
        /// </summary>
        public ISoundBoardPlayer MusicPlayer { get; }

        /// <summary>
        /// Provides direct access to the SoundBoardPlayerPool instance controlling music playback.
        /// </summary>
        public ISoundBoardPlayer EffectsPlayer { get; }

        /// <summary>
        /// Updates the music volume on the MusicPlayer MediaPlayer instance.
        /// </summary>
        public double MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = value;
                MusicPlayer.Volume = value;
            }
        }

        /// <summary>
        /// Updates the effects volume on the EffectsPlayer pool of MediaPlayer instances.
        /// </summary>
        public double EffectsVolume
        {
            get => _effectsVolume;
            set
            {
                _effectsVolume = value;
                EffectsPlayer.Volume = value;
            }
        }

        /// <summary>
        /// Enables repeat of the music in the playlist.
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// Shuffles songs in the playlist when set to true or returns the order to the original playlist order when set to false.
        /// </summary>
        public bool Shuffle
        {
            get => _shuffle;
            set
            {
                _shuffle = value;
                if (Playlist.Any())
                {
                    _setList = value ? Playlist.OrderBy(item => _rng.Next()).ToList() : Playlist.ToList();
                }
            }
        }

        /// <summary>
        /// Provides access to add or remove items from the music playlist.
        /// </summary>
        public ObservableCollection<Uri> Playlist => _playlist;

        private SoundBoard()
        {
            MusicPlayer = new SoundBoardPlayer();
            MusicPlayer.MediaEnded += MusicPlayerOnMediaEnded;
            EffectsPlayer = new SoundEffectPlayerPool(5);
            EffectsPlayer.MediaEnded += EffectsPlayerOnMediaEnded;
            _playlist.CollectionChanged += PlaylistOnCollectionChanged;
        }

        private void PlaylistOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // When the playlist changes, invoke the Shuffle setter where the setlist is updated differently depending on if shuffle is true
            Shuffle = Shuffle;
        }

        /// <summary>
        /// Plays the current song in the MusicPlayer playlist.
        /// </summary>
        public void PlayMusic()
        {
            if (!Playlist.Any())
            {
                throw new InvalidOperationException("The playlist is empty. Please add one or more items to the playlist.");
            }

            MusicPlayer.Open(_setList[_playlistPosition]);
            MusicPlayer.Play();
        }

        /// <summary>
        /// Plays a sound effect by passing the resource Uri. If music is playing, the volume will be reduced to 10% of the original volume until the effect is finished.
        /// </summary>
        /// <param name="source">A Uri referencing a local sound file ideally.</param>
        public void PlayEffect(Uri source)
        {
            lock (EffectsPlayer)
            {
                if (MusicPlayer.Volume == 0)
                {
                    return;
                }
                MusicPlayer.Volume = MusicVolume * 0.10;
                if (EffectsPlayer.Source != source)
                {
                    EffectsPlayer.Open(source);
                }
                else
                {
                    EffectsPlayer.Position = TimeSpan.Zero;
                }
                EffectsPlayer.Play(); 
            }
        }

        private void EffectsPlayerOnMediaEnded(object sender, EventArgs e)
        {
            // Slowly ramp up the music volume after the sound effects are over.
            // If sound effects start during this time, break and ramp up next time
            // the effects player tells us it's finished.
            while (!EffectsPlayer.IsActive && MusicPlayer.Volume < MusicVolume)
            {
                lock (EffectsPlayer)
                {
                    MusicPlayer.Volume += 0.01;
                    if (MusicPlayer.Volume > MusicVolume)
                    {
                        MusicPlayer.Volume = MusicVolume;
                        break;
                    } 
                }
                Task.Delay(10).Wait();
            }
        }

        /// <summary>
        /// Call Stop() on the MusicPlayer MediaPlayer instance.
        /// </summary>
        public void StopMusic()
        {
            MusicPlayer.Stop();
        }

        /// <summary>
        /// Call Pause() on the MusicPlayer MediaPlayer instance.
        /// </summary>
        public void PauseMusic()
        {
            MusicPlayer.Pause();
        }

        /// <summary>
        /// Call Play() on the MusicPlayer MediaPlayer instance.
        /// </summary>
        public void UnpauseMusic()
        {
            MusicPlayer.Play();
        }

        /// <summary>
        /// Load the next song in the playlist or stop playing music on the MusicPlayer MediaPlayer instance.
        /// </summary>
        public void NextTrack()
        {
            IncrementPlaylistPosition();
            if (!Repeat && _playlistPosition == 0)
            {
                StopMusic();
            }
            else
            {
                MusicPlayer.Position = TimeSpan.Zero;
                PlayMusic();
            }
        }

        /// <summary>
        /// Plays the previous song in the playlist, or if more than 5 seconds have elapsed since the start of the current song, the current song is played from the beginning.
        /// </summary>
        public void PreviousTrack()
        {
            if (MusicPlayer.Position >= TimeSpan.FromSeconds(5))
            {
                MusicPlayer.Position = TimeSpan.Zero;
            }
            else
            {
                DecrementPlaylistPosition();
                MusicPlayer.Position = TimeSpan.Zero;
                PlayMusic();
            }
        }

        /// <summary>
        /// Sets MusicPlayer volume to zero.
        /// </summary>
        public void MuteMusic()
        {
            MusicPlayer.Volume = 0;
        }

        /// <summary>
        /// Returns MusicPlayer volume to original level.
        /// </summary>
        public void UnmuteMusic()
        {
            MusicPlayer.Volume = MusicVolume;
        }

        /// <summary>
        /// Sets EffectsPlayer volume to zero.
        /// </summary>
        public void MuteEffects()
        {
            EffectsPlayer.Volume = 0;
        }

        /// <summary>
        /// Returns EffectsPlayer volume to original level.
        /// </summary>
        public void UnmuteEffects()
        {
            EffectsPlayer.Volume = EffectsVolume;
        }

        private void MusicPlayerOnMediaEnded(object sender, EventArgs e)
        {
            if (!Playlist.Any() || (!Repeat && _playlistPosition >= Playlist.Count() - 1))
            {
                return;
            }

            NextTrack();
        }

        private void IncrementPlaylistPosition()
        {
            _playlistPosition++;
            if (_playlistPosition >= _setList.Count())
            {
                _playlistPosition = 0;
            }
        }

        private void DecrementPlaylistPosition()
        {
            _playlistPosition--;
            if (_playlistPosition < 0)
            {
                _playlistPosition = _setList.Count - 1;
            }
        }

        /// <summary>
        /// Dispose of all MediaPlayer instances and set SoundBoard.Instance to null.
        /// </summary>
        public void Dispose()
        {
            MusicPlayer.Dispose();
            EffectsPlayer.Dispose();

            lock (InstanceLock)
            {
                Instance = null;
            }
        }

        private static readonly object InstanceLock = new object();
        private static SoundBoard _instance;

        /// <summary>
        /// Provides access to a singleton instance of the SoundBoard class and initializes a new one if necessary.
        /// </summary>
        public static SoundBoard Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    return _instance ?? (_instance = new SoundBoard());
                }
            }

            private set
            {
                lock (InstanceLock)
                {
                    _instance = value;
                }
            }
        }
    }
}
