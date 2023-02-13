Add-Type -Assembly PresentationFramework
foreach ($folder in (Get-ChildItem -Path "$PSScriptRoot\Modules" -Directory)) {
    $subfolder = $folder | Get-ChildItem -Directory | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
    $manifest = [io.path]::Combine($subfolder.FullName, '{0}.psd1' -f $folder.Name)
    Import-Module $manifest -Force
}

$music = Get-ChildItem -Path $PSScriptRoot\music\*.mp3
$effects = Get-ChildItem -Path $PSScriptRoot\effects\*.mp3

Set-SBPlaylist -Playlist $music -Repeat
Set-SBVolume -MusicVolume 1 -EffectsVolume 1

function Start-PSJukeBox {
    $xaml = [xml][io.file]::ReadAllText("$PSScriptRoot\window.xaml")
    $reader = [System.Xml.XmlNodeReader]::new($xaml)
    $window = [System.Windows.Markup.XamlReader]::Load($reader)

    $window.FindName("StopButton").add_Click({
        param([object]$sender, [object]$e)
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name)")
        Stop-SBMusic
    })

    $window.FindName("PlayButton").add_Click({
        param([object]$sender, [object]$e)
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name)")
        Start-SBMusic
    })

    $window.FindName("PauseButton").add_Click({
        param([object]$sender, [object]$e)
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name)")
        if ([PSSoundBoardLib.SoundBoard]::Instance.MusicPlayer.IsActive) {
            Suspend-SBMusic
        } else {
            Resume-SBMusic
        }
    })

    $window.FindName("SkipButton").add_Click({
        param([object]$sender, [object]$e)
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name)")
        Skip-SBTrack
    })

    $window.FindName("ShuffleButton").add_Click({
        param([object]$sender, [object]$e)
        $newValue = ![pssoundboardlib.soundboard]::Instance.Shuffle
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name) - $newValue")
        [pssoundboardlib.soundboard]::Instance.Shuffle = $newValue
    })

    $window.FindName("RepeatButton").add_Click({
        param([object]$sender, [object]$e)
        $newValue = ![pssoundboardlib.soundboard]::Instance.Repeat
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name) - $newValue")
        [pssoundboardlib.soundboard]::Instance.Repeat = $newValue
    })

    $window.FindName("EffectButton").add_Click({
        param([object]$sender, [object]$e)
        $effect = ($script:effects | Get-Random)
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name) - $effect")
        try {
            Start-SBEffect -Path $effect.FullName
        } catch {
            [system.console]::WriteLine("Exception: $($_.Exception)")
        }
    })

    $volume = $window.FindName("MusicVolumeControl")
    $volume.Value = 1
    $volume.add_ValueChanged({
        param([object]$sender, [object]$e)
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name) - NewValue = $($e.NewValue)")
        Set-SBVolume -MusicVolume ($e.NewValue)
    })

    $effectsVolume = $window.FindName("EffectsVolumeControl")
    $effectsVolume.Value = 1
    $effectsVolume.add_ValueChanged({
        param([object]$sender, [object]$e)
        [system.console]::WriteLine("$($sender.Name):$($e.RoutedEvent.Name) - NewValue = $($e.NewValue)")
        Set-SBVolume -EffectsVolume ($e.NewValue)
    })

    $null = $window.ShowDialog()
    $window = $null
    Stop-SBMusic
}

Export-ModuleMember -Function Start-PSJukeBox