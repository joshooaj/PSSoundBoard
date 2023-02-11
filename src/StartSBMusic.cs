using System;
using System.Management.Automation;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsLifecycle.Start, "SBMusic")]
    public class StartSBMusic : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                SoundBoard.Instance.PlayMusic();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, ex.Message, ErrorCategory.InvalidOperation, SoundBoard.Instance));
            }
        }
    }
}
