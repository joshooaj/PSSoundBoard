using System;
using System.Management.Automation;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsCommon.Set, "SBPlaylist")]
    public class SetSBPlaylist : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string[] Playlist { get; set; }

        [Parameter()]
        public SwitchParameter Repeat { get; set; }

        [Parameter()]
        public SwitchParameter Shuffle { get; set; }

        protected override void ProcessRecord()
        {
            SoundBoard.Instance.StopMusic();
            SoundBoard.Instance.Playlist.Clear();
            foreach (var path in Playlist)
            {
                SoundBoard.Instance.Playlist.Add(new Uri(path));
            }
            SoundBoard.Instance.Repeat = Repeat;
            SoundBoard.Instance.Shuffle = Shuffle;
        }
    }
}
