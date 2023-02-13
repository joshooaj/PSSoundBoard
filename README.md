# PSSoundBoard Module

## Introduction

This module provides a "soundboard" implementation which offers thread-safe
control of a music playlist, and a way to play sound effects in parallel.

When playing sound effects, a pool of `System.Windows.Media.MediaPlayer`
instances are used to avoid clipping the previous sound effect if it hasn't
finished playing yet. And if music is playing, the music volume will be reduced
at the start of the effect, and ramped up gently when the sound effect has
finished.

I started building this while helping a PowerShell developer with their
minesweeper clone and I have no prior experience in developing games, or using
proper audio mixing equipment. So if I've used terminology incorrectly or if
there's a much better way to handle sound, please feel free to let me know by
opening an issue.

## Usage

There are only a handful of cmdlets in this module - here's how you can use it
to set a playlist, start some music, and play sound effects. You'll need to
supply your own media files.

```powershell
# Set the playlist, repeat and shuffle, and start the music
$music = Get-ChildItem -Path .\music\*.mp3
$effects = Get-ChildItem -Path .\effects\*.mp3
Set-SBPlaylist -Playlist $music -Repeat -Shuffle
Start-SBMusic

# Set the music and effects volume. This can be adjusted any time.
Set-SBVolume -MusicVolume 0.5 -EffectsVolume 0.8

# Skip to the next music track
Skip-SBTrack

# Play a couple random sound effects demonstrating no sound clipping
Start-SBEffect -Path ($effects | Get-Random)
Start-Sleep -Milliseconds 200
Start-SBEffect -Path ($effects | Get-Random)

# Pause the music track, and show sound effects play independent of music
Suspend-SBMusic
Start-SBEffect -Path ($effects | Get-Random)

# Un-pause the music - it will start where it left off
Resume-SBMusic

# Stop the music. The position in the current file will be set to 00:00.
Stop-SBMusic

# Start from the beginning of the last track
Resume-SBMusic
```

## Attributions

The music and sound effects used in the PSJukeBox sample WPF application were
obtained from [Zapsplat.com](https://www.zapsplat.com).
