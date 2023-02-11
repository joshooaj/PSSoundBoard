using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PSSoundBoardLib
{
    public class SoundEffectPlayerPool : ISoundBoardPlayer
    {
        private readonly List<SoundBoardPlayer> _playerPool;
        private readonly object _playerLock = new object();
        private int _currentPlayer;
        private double _volume;
        private double _balance;

        public SoundEffectPlayerPool(int poolSize)
        {
            if (poolSize <= 0)
            {
                throw new InvalidOperationException("Pool size must be at least 1");
            }

            _playerPool = new List<SoundBoardPlayer>();
            for (var i = 0; i < poolSize; i++)
            {
                var player = new SoundBoardPlayer();
                player.MediaOpened += (sender, args) => OnMediaOpened();
                player.MediaEnded += (sender, args) => OnMediaEnded();
                player.MediaFailed += (sender, args) => OnMediaFailed();

                _playerPool.Add(player);
            }
        }

        public event EventHandler MediaEnded;
        public event EventHandler MediaOpened;
        public event EventHandler MediaFailed;

        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                _playerPool.ForEach(p => p.Volume = value);
            }
        }

        public double Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                _playerPool.ForEach(p => p.Balance = value);
            }
        }

        public TimeSpan Position
        {
            get => _playerPool[_currentPlayer].Position;
            set => _playerPool[_currentPlayer].Position = value;
        }
        public double BufferingProgress => _playerPool[_currentPlayer].BufferingProgress;
        public bool CanPause => _playerPool[_currentPlayer].CanPause;
        public Uri Source => _playerPool[_currentPlayer].Source;

        public bool IsActive
        {
            get
            {
                lock (_playerLock)
                {
                    return _playerPool.Any(p => p.IsActive);
                }
            }
        }

        public void Open(Uri source)
        {
            _playerPool[_currentPlayer].Stop();
            _playerPool[_currentPlayer].Open(source);
        }

        public void Play()
        {
            lock (_playerLock)
            {
                _playerPool[_currentPlayer].Play();
                IncrementPlayerIndex();
                Debug.WriteLine($"SoundEffectPlayerPool Active Players = {_playerPool.Count(p=>p.IsActive)}");
            }
        }

        private void IncrementPlayerIndex()
        {
            lock (_playerPool)
            {
                if (_currentPlayer + 1 >= _playerPool.Count)
                {
                    _currentPlayer = 0;
                }
                else
                {
                    _currentPlayer++;
                }
            }
        }

        public void Play(Uri source)
        {
            Open(source);
            Play();
        }

        public void Pause()
        {
            _playerPool.ForEach(p => p.Pause());
        }

        public void Stop()
        {
            _playerPool.ForEach(p => p.Stop());
        }

        public void Dispose()
        {
            _playerPool.ForEach(p => p.Dispose());
            _playerPool.Clear();
        }

        protected virtual void OnMediaEnded()
        {
            lock (_playerLock)
            {
                if (_playerPool.All(p => !p.IsActive))
                {
                    MediaEnded?.Invoke(this, EventArgs.Empty);
                }
                Debug.WriteLine($"SoundEffectPlayerPool Active Players = {_playerPool.Count(p => p.IsActive)}");
            }
        }

        protected virtual void OnMediaOpened()
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMediaFailed()
        {
            MediaFailed?.Invoke(this, EventArgs.Empty);
        }
    }
}
