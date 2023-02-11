using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsCommon.Set, "SBMusicPlayer")]
    public class SetSBMusicPlayer : PSCmdlet
    {
        [Parameter()]
        public double Volume { get; set; }
        protected override void ProcessRecord()
        {
            if (MyInvocation.BoundParameters.ContainsKey("Volume"))
            {
                SoundBoard.Instance.MusicVolume = Volume;
            }
        }
    }
}
