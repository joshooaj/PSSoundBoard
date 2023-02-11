using System;
#pragma warning disable 1591

namespace PSSoundBoardLib
{
    /// <summary>
    /// Defines the interface of a media player compatible with the SoundBoard class.
    /// </summary>
    public interface ISoundBoardPlayer : IDisposable
    {
        event EventHandler MediaEnded;
        event EventHandler MediaOpened;
        event EventHandler MediaFailed;
        double Volume { get; set; }
        double Balance { get; set; }
        TimeSpan Position { get; set; }
        double BufferingProgress { get; }
        bool CanPause { get; }
        Uri Source { get; }
        bool IsActive { get; }
        void Open(Uri source);
        void Play();
        void Play(Uri source);
        void Pause();
        void Stop();
    }
}