using System;
using System.Management.Automation;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsCommon.Set, "SBVolume")]
    public class SBVolume : PSCmdlet
    {
        [Parameter(ParameterSetName = "SetVolume")]
        [ValidateRange(0,1)]
        public double MusicVolume { get; set; }
        
        [Parameter(ParameterSetName = "SetVolume")]
        [ValidateRange(0, 1)]
        public double EffectsVolume { get; set; }

        [Parameter(ParameterSetName = "Mute")]
        public SwitchParameter Mute { get; set; }

        [Parameter(ParameterSetName = "Unmute")]
        public SwitchParameter Unmute { get; set; }

        protected override void ProcessRecord()
        {
            if (MyInvocation.BoundParameters.ContainsKey("MusicVolume"))
            {
                SoundBoard.Instance.MusicVolume = MusicVolume;
            }
            if (MyInvocation.BoundParameters.ContainsKey("MusicVolume"))
            {
                SoundBoard.Instance.EffectsVolume = EffectsVolume;
            }
            if (Mute)
            {
                SoundBoard.Instance.MuteMusic();
                SoundBoard.Instance.MuteEffects();
            }
            if (Unmute)
            {
                SoundBoard.Instance.UnmuteMusic();
                SoundBoard.Instance.UnmuteEffects();
            }
        }
    }
}
