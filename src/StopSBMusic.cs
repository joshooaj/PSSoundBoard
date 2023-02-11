using System.Management.Automation;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsLifecycle.Stop, "SBMusic")]
    public class StopSBMusic : PSCmdlet
    {
        [Parameter()]
        public SwitchParameter Dispose { get; set; }
        protected override void ProcessRecord()
        {
            SoundBoard.Instance.StopMusic();
            if (Dispose)
            {
                SoundBoard.Instance.Dispose();
            }
        }
    }
}
