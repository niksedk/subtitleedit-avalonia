namespace HanumanInstitute.LibMpv;

/// <summary>
/// Try to play consecutive audio files with no silence or disruption at the point of file change.
/// </summary>
public enum GaplessAudioOption
{
    /// <summary>
    /// Disable gapless audio.
    /// </summary>
    No,
    /// <summary>
    /// The audio device is opened using parameters chosen for the first file played and is then kept open for gapless playback. This means that if the first file for example has a low sample rate, then the following files may get resampled to the same low sample rate, resulting in reduced sound quality. If you play files with different parameters, consider using options such as --audio-samplerate and --audio-format to explicitly select what the shared output format will be.
    /// </summary>
    Yes,
    /// <summary>
    /// Normally, the audio device is kept open (using the format it was first initialized with). If the audio format the decoder output changes, the audio device is closed and reopened. This means that you will normally get gapless audio with files that were encoded using the same settings, but might not be gapless in other cases. The exact conditions under which the audio device is kept open is an implementation detail, and can change from version to version. Currently, the device is kept even if the sample format changes, but the sample formats are convertible. If video is still going on when there is still audio, trying to use gapless is also explicitly given up.
    /// </summary>
    Weak
}