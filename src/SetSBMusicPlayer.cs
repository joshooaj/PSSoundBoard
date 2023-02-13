using System.Management.Automation;

namespace PSSoundBoardLib
{
  [Cmdlet(VerbsCommon.Set, "SBMusicPlayer")]
  public class SetSBMusicPlayer : PSCmdlet
  {
    [Parameter()] public double Volume { get; set; }

    protected override void ProcessRecord()
    {
      if (MyInvocation.BoundParameters.ContainsKey("Volume"))
      {
        SoundBoard.Instance.MusicVolume = Volume;
      }
    }
  }
}