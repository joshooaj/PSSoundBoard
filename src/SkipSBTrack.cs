using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSSoundBoardLib
{
    [Cmdlet(VerbsCommon.Skip, "SBTrack")]
    public class SkipSBTrack : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                SoundBoard.Instance.NextTrack();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, ex.Message, ErrorCategory.InvalidOperation, SoundBoard.Instance));
            }
        }
    }
}
