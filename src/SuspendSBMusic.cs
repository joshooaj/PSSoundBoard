using System.Management.Automation;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsLifecycle.Suspend, "SBMusic")]
    public class SuspendSBMusic : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            SoundBoard.Instance.PauseMusic();
        }
    }
}