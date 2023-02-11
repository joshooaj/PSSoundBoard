using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace PSSoundBoardLib
{
    public class SoundBoardPlayer : IDisposable, ISoundBoardPlayer
    {
        private MediaPlayer _player;
        private readonly object _playerLock = new object();
        private readonly Task _dispatcherTask;

        public event EventHandler MediaEnded;
        public event EventHandler MediaOpened;
        public event EventHandler MediaFailed;

        public double Volume
        {
            get
            {
                double result = 0;
                RaiseToDispatcher(() => result = _player.Volume);
                return result;
            }
            set
            {
                RaiseToDispatcher(() => _player.Volume = value);
            }
        }

        public double Balance
        {
            get
            {
                double result = 0;
                RaiseToDispatcher(() => result = _player.Balance);
                return result;
            }
            set
            {
                RaiseToDispatcher(() => _player.Balance = value);
            }
        }

        public TimeSpan Position
        {
            get
            {
                var result = TimeSpan.Zero;
                RaiseToDispatcher(() => result = _player.Position);
                return result;
            }
            set
            {
                RaiseToDispatcher(() => _player.Position = value);
            }
        }

        public double BufferingProgress
        {
            get
            {
                double result = 0;
                RaiseToDispatcher(() => result = _player.BufferingProgress);
                return result;
            }
        }

        public bool CanPause
        {
            get
            {
                bool result = false;
                RaiseToDispatcher(() => result = _player.CanPause);
                return result;
            }
        }

        public Uri Source
        {
            get
            {
                Uri result = null;
                RaiseToDispatcher(() => result = _player.Source);
                return result;
            }
        }

        public bool IsActive
        {
            get;
            private set;
        }

        public SoundBoardPlayer()
        {
            _dispatcherTask = Task.Run(StartMediaPlayer);
            while (_player == null)
            {
                Debug.WriteLine("Awaiting MediaPlayer initialization");
                Task.Delay(50).Wait();
            }
        }

        private void StartMediaPlayer()
        {
            _player = new MediaPlayer();
            _player.MediaOpened += PlayerOnMediaOpened;
            _player.MediaEnded += PlayerOnMediaEnded;
            _player.MediaFailed += PlayerOnMediaFailed;
            Dispatcher.Run();
        }

        private void PlayerOnMediaOpened(object sender, EventArgs e)
        {
            RaiseToDispatcher(OnMediaOpened);
        }

        private void PlayerOnMediaEnded(object sender, EventArgs e)
        {
            RaiseToDispatcher(OnMediaEnded);
        }

        private void PlayerOnMediaFailed(object sender, ExceptionEventArgs e)
        {
            RaiseToDispatcher(() => OnMediaFailed(e));
        }

        public void Open(Uri source)
        {
            RaiseToDispatcher(() => _player.Open(source));
        }

        public void Play()
        {
            IsActive = true;
            RaiseToDispatcher(() => _player.Play());
        }

        public void Play(Uri source)
        {
            Open(source);
            Play();
        }

        public void Pause()
        {
            IsActive = false;
            RaiseToDispatcher(() => _player.Pause());
        }

        public void Stop()
        {
            IsActive = false;
            RaiseToDispatcher(() => _player.Stop());
        }

        private void RaiseToDispatcher(Action action)
        {
            _player.Dispatcher.Invoke(action);
        }

        protected virtual void OnMediaOpened()
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMediaEnded()
        {
            IsActive = false;
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMediaFailed(ExceptionEventArgs e)
        {
            MediaFailed?.Invoke(this, e);
        }

        public void Dispose()
        {
            RaiseToDispatcher(() => _player.Close());
            _player.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            _dispatcherTask.Wait();
        }
    }
}
