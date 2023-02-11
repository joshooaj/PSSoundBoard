using System.Management.Automation;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsLifecycle.Resume, "SBMusic")]
    public class ResumeSBMusic : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            SoundBoard.Instance.UnpauseMusic();
        }
    }
}