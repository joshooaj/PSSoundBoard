using System;
using System.Management.Automation;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsLifecycle.Start, "SBEffect")]
    public class StartSBEffect : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Path { get; set; }
        protected override void ProcessRecord()
        {
            try
            {
                SoundBoard.Instance.PlayEffect(new Uri(Path));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, ex.Message, ErrorCategory.InvalidOperation, SoundBoard.Instance));
            }
        }
    }
}
