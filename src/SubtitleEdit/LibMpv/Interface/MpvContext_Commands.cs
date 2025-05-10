// ReSharper disable PossibleMultipleEnumeration

namespace HanumanInstitute.LibMpv;

public partial class MpvContext
{
    private MpvCommand<T> C<T>(params object?[] commandName) => new(this, commandName);
    private MpvCommand C(params object?[] commandName) => new(this, commandName);

    // /// <summary>
    // /// Returns the name of the client as string. This is the string ipc-N with N being an integer number.
    // /// </summary>
    // public MpvCommand<string?> GetClientName() => 
    //     C<string?>("client_name");

    /// <summary>
    /// Returns the current mpv internal time in microseconds as a number. This is basically the system time, with an arbitrary offset.
    /// </summary>
    public MpvCommand<int?> GetClientTime() =>
        C<int?>("get_time_us");

    /// <summary>
    /// Use this to "block" keys that should be unbound, and do nothing. Useful for disabling default bindings, without disabling all bindings with --no-input-default-bindings.
    /// </summary>
    public MpvCommand IgnoreKeys() =>
        C("ignore");

    /// <summary>
    /// Changes the playback position. By default, seeks by a relative amount of seconds.
    /// </summary>
    /// <param name="units">The amount of units to seek. Seconds by default.</param>
    /// <param name="flags">Flags controlling the seek mode. Flags can be combined.</param>
    public MpvCommand Seek(double units, SeekOption flags) =>
        C("seek", units, flags.GetFlags().FormatMpvFlag());

    /// <summary>
    /// Undoes the seek command, and some other commands that seek (but not necessarily all of them). Calling this command once will jump to the playback position before the seek. Calling it a second time undoes the revert-seek command itself. This only works within a single file.
    /// </summary>
    /// <param name="mark">If true, marks the current time position. The next normal revert-seek command will seek back to this point, no matter how many seeks happened since last time.</param>
    public MpvCommand RevertSeek(bool mark) =>
        C("revert-seek", mark ? "mark" : null);

    /// <summary>
    /// Plays one frame, then pause. Does nothing with audio-only playback.
    /// </summary>
    public MpvCommand FrameStep() =>
        C("frame-step");

    /// <summary>
    /// Go back by one frame, then pause. Note that this can be very slow (it tries to be precise, not fast), and sometimes fails to behave as expected. How well this works depends on whether precise seeking works correctly (e.g. see the --hr-seek-demuxer-offset option). Video filters or other video post-processing that modifies timing of frames (e.g. deinterlacing) should usually work, but might make backstepping silently behave incorrectly in corner cases. Using --hr-seek-framedrop=no should help, although it might make precise seeking slower.
    /// This does not work with audio-only playback.
    /// </summary>
    public MpvCommand FrameBackStep() =>
        C("frame-back-step");
    
    // /// <summary>
    // /// Get the given property or option value.
    // /// </summary>
    // /// <param name="name">The name of a property or option.</param>
    // public MpvCommand<T> Get<T>(string name) =>
    //     C<T>("get_property", name.CheckNotNullOrEmpty(nameof(name)));

    // /// <summary>
    // /// Set the given property or option to the given value.
    // /// </summary>
    // /// <param name="name">The name of a property or option.</param>
    // /// <param name="value">The value to set.</param>
    // public MpvCommand Set(string name, object? value) =>
    //     C("set", name.CheckNotNullOrEmpty(nameof(name)), value);

    /// <summary>
    /// Add the given value to the property or option. On overflow or underflow, clamp the property to the maximum. If value is omitted, assume 1.
    /// </summary>
    /// <param name="name">The name of a property or option.</param>
    /// <param name="value">The value to add.</param>
    public MpvCommand Add(string name, object? value) =>
        C("add", name.CheckNotNullOrEmpty(nameof(name)), value);

    /// <summary>
    /// Cycles the given property or option. The second argument can be up or down to set the cycle direction. On overflow, set the property back to the minimum, on underflow set it to the maximum.
    /// </summary>
    /// <param name="name">The name of a property or option.</param>
    /// <param name="direction">The cycling direction. By default, Up.</param>
    public MpvCommand Cycle(string name, CycleDirection direction = CycleDirection.Up) =>
        C("cycle", name.CheckNotNullOrEmpty(nameof(name)), direction.ToStringInvariant().ToLowerInvariant());

    /// <summary>
    /// Similar to add, but multiplies the property or option with the numeric value.
    /// </summary>
    /// <param name="name">The name of a property or option.</param>
    /// <param name="value">The multiplication factor.</param>
    public MpvCommand Multiply(string name, object? value) =>
        C("multiply", name.CheckNotNullOrEmpty(nameof(name)), value);

    /// <summary>
    /// Takes a screenshot.
    /// </summary>
    /// <param name="flags">Screenshot options. Multiple flags can be combined.</param>
    public MpvCommand Screenshot(ScreenshotOptions flags = ScreenshotOptions.None) =>
        C("screenshot", flags.GetFlags().FormatMpvFlag());

    /// <summary>
    /// Takes a screenshot and save it to a given file. If the file already exists, it's overwritten. 
    /// The format of the file will be guessed by the extension (and --screenshot-format is ignored - the behavior when the extension is missing or unknown is arbitrary).
    /// </summary>
    /// <param name="fileName">The file name where to save the screenshot.</param>
    /// <param name="flags">Screenshot options. Multiple flags can be combined.</param>
    public MpvCommand ScreenshotToFile(string fileName, ScreenshotOptions flags = ScreenshotOptions.None) =>
        C("screenshot-to-file", 
            fileName.CheckNotNullOrEmpty(nameof(fileName)),
            flags.GetFlags().FormatMpvFlag());

    /// <summary>
    /// Goes to the next entry on the playlist.
    /// </summary>
    /// <param name="terminateOnEnd">When the last file is being played, if true, ends playback, otherwise does nothing.</param>
    public MpvCommand PlaylistNext(bool terminateOnEnd = false) =>
        C("playlist-next", terminateOnEnd ? "force" : null);

    /// <summary>
    /// Goes to the previous entry on the playlist.
    /// </summary>
    /// <param name="terminateOnEnd">When the first file is being played, if true, ends playback, otherwise does nothing.</param>
    public MpvCommand PlaylistPrev(bool terminateOnEnd = false) =>
        C("playlist-prev", terminateOnEnd ? "force" : null);

    /// <summary>
    /// Loads the given file or URL and plays it.
    /// </summary>
    /// <param name="path">The path of the file or URL to play.</param>
    /// <param name="append">Append the file to the playlist.</param>
    /// <param name="appendPlay">Append the file, and if nothing is currently playing, start playback. (Always starts with the added file, even if the playlist was not empty before running this command.)</param>
    /// <param name="extraArgs">A list of options and values which should be set while the file is playing. Not all options can be changed this way. Some options require a restart of the player.</param>
    public MpvCommand LoadFile(string path, bool append = false, bool appendPlay = false, IDictionary<string, object>? extraArgs = null)
    {
        var flag = append ? (appendPlay ? "append-play" : "append") : "replace";
        var args = new object[3 + (extraArgs?.Count ?? 0)];
        args[0] = "loadfile";
        args[1] = path.CheckNotNullOrEmpty(nameof(path));
        args[2] = flag;
        if (extraArgs != null)
        {
            var index = 2;
            foreach (var item in extraArgs)
            {
                // Note: options and values are not validated and will be passed as-is.
                args[index++] = item.Key + "=" + item.Value.ToStringInvariant();
            }
        }
        return C(args);
    }

    /// <summary>
    /// Loads the given playlist file or URL.
    /// </summary>
    /// <param name="path">The path of the playlist file or URL to play.</param>
    /// <param name="append">Append the new playlist at the end of the current internal playlist.</param>
    public MpvCommand LoadPlaylist(string path, bool append = false) =>
        C("loadlist", path, append ? "append" : "replace");

    /// <summary>
    /// Clears the playlist, except the currently played file.
    /// </summary>
    public MpvCommand PlaylistClear() => 
        C("playlist-clear");

    /// <summary>
    /// Removes the playlist entry at the given index. Index values start counting with 0.
    /// </summary>
    /// <param name="index">The index of the playlist entry to remove.</param>
    public MpvCommand PlaylistRemove(int index) =>
        C("playlist-remove", 
            index.CheckRange(nameof(index), min: 0));

    /// <summary>
    /// Removes the current playlist entry, stops playback and starts playing the next entry.
    /// </summary>
    public MpvCommand PlaylistRemoveCurrent() =>
        C("playlist-remove", "current");

    /// <summary>
    /// Move the playlist entry at index1, so that it takes the place of the entry index2. (Paradoxically, the moved playlist entry will not have the index value index2 after moving if index1 was lower than index2, because index2 refers to the target entry, not the index the entry will have after moving.)
    /// </summary>
    /// <param name="index">The index of the playlist entry to move.</param>
    /// <param name="destIndex">The index to move the destination to.</param>
    public MpvCommand PlaylistMove(int index, int destIndex) =>
        C("playlist-move", 
            index.CheckRange(nameof(index), min: 0), 
            destIndex.CheckRange(nameof(destIndex), min: 0));

    /// <summary>
    /// Shuffle the playlist.
    /// </summary>
    public MpvCommand PlaylistShuffle() =>
        C("playlist-shuffle");

    /// <summary>
    /// Attempt to revert the previous PlaylistShuffle command. This works only once (multiple successive PlaylistUnshuffle commands do nothing). May not work correctly if new recursive playlists have been opened since a PlaylistShuffle command.
    /// </summary>
    public MpvCommand PlaylistUnshuffle() =>
        C("playlist-unshuffle");

    /// <summary>
    /// Runs the given shell command.
    /// </summary>
    /// <param name="command">The command to run.</param>
    public MpvCommand RunShell(string command) =>
        C("run", "/bin/sh", "-c", 
            command.CheckNotNullOrEmpty(nameof(command)));

    /// <summary>
    /// Similar to run, but gives more control about process execution to the caller, and does does not detach the process.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="args">Array of strings with the command as first argument, and subsequent command line arguments following. This is just like the run command argument list.
    /// The first array entry is either an absolute path to the executable, or a filename with no path components, in which case the PATH environment variable.On Unix, this is equivalent to posix_spawnp and execvp behavior.</param>
    /// <param name="playbackOnly">Whether the process should be killed when playback terminates (optional, default: True). If enabled, stopping playback will automatically kill the process, and you can't start it outside of playback.</param>
    /// <param name="captureSize">Maximum number of stdout plus stderr bytes that can be captured (optional, default: 64MB). If the number of bytes exceeds this, capturing is stopped. The limit is per captured stream.</param>
    /// <param name="captureStdOut">Capture all data the process outputs to stdout and return it once the process ends (optional, default: false).</param>
    /// <param name="captureStdErr">Capture all data the process outputs to stderr and return it once the process ends (optional, default: false).</param>
    /// <returns>Process data of type SubProcessResponse.</returns>
    public MpvCommand<SubProcessResponse?> SubProcess(string command, IEnumerable<string>? args = null, bool? playbackOnly = null,
        int? captureSize = null, bool? captureStdOut = null, bool? captureStdErr = null)
    {
        var data = new SubProcessRequest
        {
            Name = "subprocess",
            PlaybackOnly = playbackOnly,
            CaptureSize = captureSize,
            CaptureStdOut = captureStdOut,
            CaptureStdErr = captureStdErr,
        }
        ;
        data.Args.Add(command.CheckNotNullOrEmpty(nameof(command)));
        if (args?.Any() == true)
        {
            foreach (var item in args)
            {
                data.Args.Add(item);
            }
        }

        return C<SubProcessResponse?>(data, data.Name);
    }

    /// <summary>
    /// Exits the player. If an argument is given, it's used as process exit code.
    /// </summary>
    /// <param name="exitCode">The process exit code.</param>
    public MpvCommand Quit(int? exitCode = null) =>
        C("quit", exitCode);

    /// <summary>
    /// Exits the player, and store current playback position. Playing that file later will seek to the previous position on start.
    /// </summary>
    /// <param name="exitCode">The process exit code.</param>
    public MpvCommand QuitWatchLater(int? exitCode = null) =>
        C("quit-watch-later", exitCode);

    /// <summary>
    /// Loads the given subtitle file or stream. By default, it is selected as current subtitle after loading.
    /// </summary>
    /// <param name="path">The file path or URL of the subtitle to load.</param>
    /// <param name="option">Subtitle loading options.</param>
    /// <param name="title">The track title in the UI.</param>
    /// <param name="lang">The track language. It can influence stream selection when using LoadOption.Auto.</param>
    public MpvCommand SubAdd(string path, LoadOption option = LoadOption.Select, string? title = null, string? lang = null) =>
        C("sub-add", 
            path.CheckNotNullOrEmpty(nameof(path)), 
            option.CheckEnumValid(nameof(option)).FormatMpvFlag(),
            title, lang);

    /// <summary>
    /// Removes the given subtitle track. If the id argument is missing, remove the current track. (Works on external subtitle files only.)
    /// </summary>
    /// <param name="id">The ID of the subtitle track to remove.</param>
    public MpvCommand SubRemove(int? id = null) =>
        C("sub-remove", id);

    /// <summary>
    /// Reloads the given subtitle tracks. If the id argument is missing, reload the current track. (Works on external subtitle files only.)
    /// This works by unloading and re-adding the subtitle track.
    /// </summary>
    /// <param name="id">The ID of the subtitle track to reload.</param>
    public MpvCommand SubReload(int? id = null) =>
        C("sub-reload", id);

    /// <summary>
    /// Changes subtitle timing such, that the subtitle event after the next skip subtitle events is displayed. skip can be negative to step backwards.
    /// </summary>
    /// <param name="skip">The amount of subtitle events to move to, forwards or backwards.</param>
    public MpvCommand SubStep(int skip) =>
        C("sub-step", skip);

    /// <summary>
    /// Seeks to the next (skip set to 1) or the previous (skip set to -1) subtitle. This is similar to sub-step, except that it seeks video and audio instead of adjusting the subtitle delay.
    /// For embedded subtitles(like with Matroska), this works only with subtitle events that have already been displayed, or are within a short prefetch range.
    /// </summary>
    /// <param name="skip">The amount of subtitle events to move to, forwards or backwards.</param>
    public MpvCommand SubSeek(int skip) =>
        C("sub-seek", skip);

    /// <summary>
    /// Prints text to stdout. The string can contain properties (see Property Expansion).
    /// </summary>
    /// <param name="text">The text to print to stdout.</param>
    public MpvCommand PrintText(string text) =>
        C("print-text", text);

    /// <summary>
    /// Shows text on the OSD. The string can contain properties, which are expanded as described in Property Expansion. This can be used to show playback time, filename, and so on.
    /// </summary>
    /// <param name="text">The OSD text to show.</param>
    /// <param name="duration">The time in ms to show the message for. By default, it uses the same value as --osd-duration.</param>
    /// <param name="minOsdLevel">The minimum OSD level to show the text at (see --osd-level).</param>
    /// <returns></returns>
    public MpvCommand ShowText(string text, int duration, int minOsdLevel) =>
        C("show-text", text, duration, 
            minOsdLevel.CheckRange(nameof(minOsdLevel), min: 1, max: 3));

    /// <summary>
    /// Shows the progress bar, the elapsed time and the total duration of the file on the OSD.
    /// </summary>
    public MpvCommand ShowProgress() =>
        C("show-progress");

    /// <summary>
    /// Writes the resume config file that the QuitWatchLater command writes, but continue playback normally.
    /// </summary>
    public MpvCommand WriteWatchLaterConfig() =>
        C("write-watch-later-config");

    /// <summary>
    /// Delete any existing resume config file that was written by 'quit-watch-later' or 'write-watch-later-config'.
    /// </summary>
    /// <param name="fileName">If specified, then the deleted config is for that file; otherwise, it is the same one as would be written by 'quit-watch-later' or 'write-watch-later-config' in the current circumstance.</param>
    public MpvCommand DeleteWatchLaterConfig(string? fileName = null) =>
        C("delete-watch-later-config", fileName);

    /// <summary>
    /// Stops playback and clear playlist. With default settings, this is essentially like quit. Useful for the client API: playback can be stopped without terminating the player.
    /// </summary>
    public MpvCommand Stop() =>
        C("stop");

    /// <summary>
    /// Sends a mouse event with given coordinate (x, y).
    /// </summary>
    /// <param name="x">The horizontal mouse coordinate to send.</param>
    /// <param name="y">The vertical mouse coordinate to send.</param>
    /// <param name="button">The button number of clicked mouse button. This should be one of 0-19. If omitted, only the position will be updated.</param>
    /// <param name="doubleClick">If true, the mouse event represents double-click, otherwise regular single-click.</param>
    /// <returns></returns>
    public MpvCommand Mouse(int x, int y, int? button = null, bool doubleClick = false) =>
        C("mouse", 
            x.CheckRange(nameof(x), min: 0),
            y.CheckRange(nameof(y), min: 0),
            button?.CheckRange(nameof(button), min: 0, max: 19),
            doubleClick ? "double" : null);

    /// <summary>
    /// Sends a key event through mpv's input handler, triggering whatever behavior is configured to that key. name uses the input.conf naming scheme for keys and modifiers. Useful for the client API: key events can be sent to libmpv to handle internally.
    /// </summary>
    /// <param name="key">The key to send.</param>
    public MpvCommand KeyPress(string key) =>
        C("keypress", key);

    /// <summary>
    /// Similar to KeyPress, but sets the KEYDOWN flag so that if the key is bound to a repeatable command, it will be run repeatedly with mpv's key repeat timing until the keyup command is called.
    /// </summary>
    /// <param name="key">The key to send.</param>
    public MpvCommand KeyDown(string key) =>
        C("keydown", key);

    /// <summary>
    /// Set the KEYUP flag, stopping any repeated behavior that had been triggered. If key is null, KEYUP will be set on all keys. Otherwise, KEYUP will only be set on the key specified by name.
    /// </summary>
    /// <param name="key">The key to send.</param>
    public MpvCommand KeyUp(string? key = null) =>
        C("keyup", key);

    /// <summary>
    /// Binds a key to an input command. command must be a complete command containing all the desired arguments and flags. Both name and command use the input.conf naming scheme. This is primarily useful for the client API.
    /// </summary>
    /// <param name="key">The key to bind.</param>
    /// <param name="command">The full command to trigger containing all the desired arguments and flags.</param>
    public MpvCommand KeyBind(string key, string command) =>
        C("keybind", key, command);

    /// <summary>
    /// Loads the given audio file or stream. By default, it is selected as current subtitle after loading.
    /// </summary>
    /// <param name="path">The file path or URL of the audio to load.</param>
    /// <param name="option">Audio loading options.</param>
    /// <param name="title">The track title in the UI.</param>
    /// <param name="lang">The track language. It can influence stream selection when using LoadOption.Auto.</param>
    public MpvCommand AudioAdd(string path, LoadOption option = LoadOption.Select, string? title = null, string? lang = null) =>
        C("audio-add", 
            path.CheckNotNullOrEmpty(nameof(path)),
            option.CheckEnumValid(nameof(option)).FormatMpvFlag(), 
            title, lang);

    /// <summary>
    /// Removes the given audio track. If the id argument is missing, remove the current track. (Works on external audio files only.)
    /// </summary>
    /// <param name="id">The ID of the audio track to remove.</param>
    public MpvCommand AudioRemove(int? id = null) =>
        C("audio-remove", id);

    /// <summary>
    /// Reloads the given audio tracks. If the id argument is missing, reload the current track. (Works on external audio files only.)
    /// This works by unloading and re-adding the audio track.
    /// </summary>
    /// <param name="id">The ID of the audio track to reload.</param>
    public MpvCommand AudioReload(int? id = null) =>
        C("audio-reload", id);

    /// <summary>
    /// Loads the given video file or stream. By default, it is selected as current video after loading.
    /// </summary>
    /// <param name="path">The file path or URL of the video to load.</param>
    /// <param name="option">Video loading options.</param>
    /// <param name="title">The track title in the UI.</param>
    /// <param name="lang">The track language. It can influence stream selection when using LoadOption.Auto.</param>
    /// <param name="albumArt">Tells mpv to load the given video as album art.</param>
    public MpvCommand VideoAdd(string path, LoadOption option = LoadOption.Select, string? title = null, string? lang = null, string? albumArt = null) =>
        C("video-add",
            path.CheckNotNullOrEmpty(nameof(path)),
            option.CheckEnumValid(nameof(option)).FormatMpvFlag(), 
            title, lang, albumArt);

    /// <summary>
    /// Removes the given video track. If the id argument is missing, remove the current track. (Works on external video files only.)
    /// </summary>
    /// <param name="id">The ID of the video track to remove.</param>
    public MpvCommand VideoRemove(int? id = null) =>
        C("video-remove", id);

    /// <summary>
    /// Reloads the given video tracks. If the id argument is missing, reload the current track. (Works on external video files only.)
    /// This works by unloading and re-adding the video track.
    /// </summary>
    /// <param name="id">The ID of the video track to reload.</param>
    public MpvCommand VideoReload(int? id = null) =>
        C("video-reload", id);

    /// <summary>
    /// Rescans external files according to the current --sub-auto and --audio-file-auto settings. This can be used to auto-load external files after the file was loaded.
    /// </summary>
    /// <param name="reselect">If true, select the default audio and subtitle streams, which typically selects external files with the highest preference. If false, do not change current track selections.</param>
    public MpvCommand RescanExternalFiles(bool reselect = true) =>
        C("rescan-external-files", reselect ? "reselect" : "keep-selection");

    /// <summary>
    /// Changes the audio filter chain.
    /// </summary>
    /// <param name="operation">Decides what happens to the filter.</param>
    /// <param name="value">The value to apply with the operation.</param>
    public MpvCommand ChangeAudioFilter(FilterOperation operation, string value = "") =>
        C("af", operation.CheckEnumValid(nameof(operation)).FormatMpvFlag(), value);

    /// <summary>
    /// Changes the video filter chain.
    /// </summary>
    /// <param name="operation">Decides what happens to the filter.</param>
    /// <param name="value">The value to apply with the operation.</param>
    public MpvCommand ChangeVideoFilter(FilterOperation operation, string value = "") =>
        C("vf", operation.CheckEnumValid(nameof(operation)).FormatMpvFlag(), value);

    /// <summary>
    /// Cycles through a list of values. Each invocation of the command will set the given property to the next value in the list. The command will use the current value of the property/option, and use it to determine the current position in the list of values. Once it has found it, it will set the next value in the list (wrapping around to the first item if needed).
    /// </summary>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="values">The list of values to set.</param>
    /// <param name="reverse">Can be used to cycle the value list in reverse. The only advantage is that you don't need to reverse the value list yourself when adding a second key binding for cycling backwards.</param>
    public MpvCommand CycleValues(string propertyName, IEnumerable<object> values, bool reverse = false)
    {
        propertyName.CheckNotNullOrEmpty(nameof(propertyName));
        values.CheckNotNullOrEmpty(nameof(values));

        var args = new object?[values.Count() + (reverse ? 3 : 2)];
        args[0] = "cycle-values";
        var i = 1;
        if (reverse)
        {
            args[i++] = "!reverse";
        }
        args[i++] = propertyName;
        foreach (var item in values)
        {
            args[i++] = item;
        }

        return C(args);
    }

    /// <summary>
    /// Adds an OSD overlay sourced from raw data. This might be useful for scripts and applications controlling mpv, and which want to display things on top of the video window.
    /// </summary>
    /// <param name="id">Identify the overlay element between 0 and 63. The ID can be used to add multiple overlay parts, update a part by using this command with an already existing ID, or to remove a part with overlay-remove. Using a previously unused ID will add a new overlay, while reusing an ID will update it.</param>
    /// <param name="x">Specify the horizontal position where the OSD should be displayed.</param>
    /// <param name="y">Specify the vertical position where the OSD should be displayed.</param>
    /// <param name="w">The visible width of the overlay.</param>
    /// <param name="h">The visible height of the overlay.</param>
    /// <param name="file">Specify the file the raw image data is read from. It can be either a numeric UNIX file descriptor prefixed with @ (e.g. @4), or a filename. The file will be mapped into memory with mmap(), copied, and unmapped before the command returns (changed in mpv 0.18.1). 
    /// It is also possible to pass a raw memory address for use as bitmap memory by passing a memory address as integer prefixed with an & character. Passing the wrong thing here will crash the player. This mode might be useful for use with libmpv. The offset parameter is simply added to the memory address (since mpv 0.8.0, ignored before).</param>
    /// <param name="stride">Gives the width in bytes in memory.</param>
    /// <param name="offset">The byte offset of the first pixel in the source file. (The current implementation always mmap's the whole file from position 0 to the end of the image, so large offsets should be avoided. Before mpv 0.8.0, the offset was actually passed directly to mmap, but it was changed to make using it easier.)</param>
    /// <param name="fmt">String identifying the image format. Currently, only bgra is defined. This format has 4 bytes per pixels, with 8 bits per component. The least significant 8 bits are blue, and the most significant 8 bits are alpha (in little endian, the components are B-G-R-A, with B as first byte). This uses premultiplied alpha: every color component is already multiplied with the alpha component. This means the numeric value of each component is equal to or smaller than the alpha component. (Violating this rule will lead to different results with different VOs: numeric overflows resulting from blending broken alpha values is considered something that shouldn't happen, and consequently implementations don't ensure that you get predictable behavior in this case.)</param>
    public MpvCommand ImageOverlayAdd(int id, int x, int y, int w, int h, string file, int stride, int offset = 0, string fmt = "bgra") =>
        C("overlay-add",
            id.CheckRange(nameof(id), min: 0, max: 63),
            x, y,
            w.CheckRange(nameof(w), min: 0),
            h.CheckRange(nameof(h), min: 0),
            file.CheckNotNullOrEmpty(nameof(file)),
            stride.CheckRange(nameof(stride), min: 0), offset, fmt);

    /// <summary>
    /// Removes an overlay added with overlay-add and the same ID. Does nothing if no overlay with this ID exists.
    /// </summary>
    /// <param name="id">The id of the overlay to remove.</param>
    public MpvCommand ImageOverlayRemove(int id) =>
        C("overlay-remove", id.CheckRange(nameof(id), min: 0, max: 63));

    /// <summary>
    /// Adds or updates an OSD overlay of ASS format.
    /// (Although this sounds similar to overlay-add, osd-overlay is for text overlays, while overlay-add is for bitmaps.Maybe overlay-add will be merged into osd-overlay to remove this oddity.)
    /// If the libmpv client is destroyed, all overlays associated with it are also deleted. In particular, connecting via --input-ipc-server, adding an overlay, and disconnecting will remove the overlay immediately again.
    /// </summary>
    /// <param name="text">String defining the overlay contents.</param>
    /// <param name="id">Arbitrary integer that identifies the overlay. Multiple overlays can be added by calling this command with different id parameters. Calling this command with the same id replaces the previously set overlay.
    /// There is a separate namespace for each libmpv client (i.e. IPC connection, script), so IDs can be made up and assigned by the API user without conflicting with other API users.</param>
    /// <param name="playResX">Specify the value of ASS PlayResX.</param>
    /// <param name="playResY">Specify the value of ASS PlayResY.</param>
    /// <param name="zOrder">The Z order of the overlay.</param>
    public MpvCommand AssOverlayAdd(string text, int id = 0, int? playResX = null, int? playResY = null, int? zOrder = null) =>
        C("osd-overlay", new OverlayRequest()
            {
                Name = "osd-overlay",
                Id = id,
                Format = "ass-events",
                Data = text,
                ResX = playResX,
                ResY = playResY,
                Z = zOrder
            });

    /// <summary>
    /// Removes an OSD overlay.
    /// </summary>
    /// <param name="id">Arbitrary integer that identifies the overlay.</param>
    public MpvCommand AssOverlayRemove(int id = 0) =>
        C("osd-overlay", new OverlayRequest
            {
                Id = id,
                Format = "none"
            });


    /// <summary>
    /// Sends a message to all clients, and pass it the following list of arguments. What this message means, how many arguments it takes, and what the arguments mean is fully up to the receiver and the sender. Every client receives the message, so be careful about name clashes (or use script-message-to).
    /// This command has a variable number of arguments, and cannot be used with named arguments.
    /// </summary>
    /// <param name="args">The values to send to other applications.</param>
    public MpvCommand ScriptMessage(IEnumerable<object?> args) =>
        C(new[] { "script-message" }
            .Concat(args.CheckNotNullOrEmpty(nameof(args))).ToArray());

    /// <summary>
    /// Same as script-message, but sends it only to the client named target. Each client (scripts etc.) has a unique name. For example, Lua scripts can get their name via mp.get_script_name().
    /// This command has a variable number of arguments, and cannot be used with named arguments.
    /// </summary>
    /// <param name="target">The name of the api client to send the message to, which can be obtained with get_script_name.</param>
    /// <param name="args">The values to send to the other application.</param>
    public MpvCommand ScriptMessageTo(string target, IEnumerable<object?> args) =>
        C(new[] { "script-message-to", target }
            .Concat(args.CheckNotNullOrEmpty(nameof(args))).ToArray());

    /// <summary>
    /// Invokes a script-provided key binding. This can be used to remap key bindings provided by external Lua scripts.
    /// </summary>
    /// <param name="name">The name of the binding.</param>
    public MpvCommand ScriptBinding(string name) =>
        C("script-binding", name.CheckNotNullOrEmpty(nameof(name)));

    /// <summary>
    /// Cycles through A-B loop states. The first command will set the A point (the ab-loop-a property); the second the B point, and the third will clear both points.
    /// </summary>
    public MpvCommand AbLoop() =>
        C("ab-loop");

    /// <summary>
    /// Drops audio/video/demuxer buffers, and restart from fresh. Might help with unseekable streams that are going out of sync. This command might be changed or removed in the future.
    /// </summary>
    public MpvCommand DropBuffers() =>
        C("drop-buffers");

    /// <summary>
    /// Sends a command to the video filter with the given label. The command and argument string is filter specific. Currently, this only works with the lavfi filter - see the libavfilter documentation for which commands a filter supports.
    /// Note that the label is a mpv filter label, not a libavfilter filter name.
    /// </summary>
    /// <param name="label">The label of the filter, or 'all' to send to all filters at once.</param>
    /// <param name="command">The command to send, which is filter-specific.</param>
    /// <param name="argument">The argument to send with the command.</param>
    public MpvCommand VideoFilterCommand(string label, string command, string argument) =>
        C("vf-command",
            label.CheckNotNullOrEmpty(nameof(label)),
            command.CheckNotNullOrEmpty(nameof(command)),
            argument.CheckNotNullOrEmpty(nameof(argument)));

    /// <summary>
    /// Sends a command to the audio filter with the given label. The command and argument string is filter specific. Currently, this only works with the lavfi filter - see the libavfilter documentation for which commands a filter supports.
    /// Note that the label is a mpv filter label, not a libavfilter filter name.
    /// </summary>
    /// <param name="label">The label of the filter, or 'all' to send to all filters at once.</param>
    /// <param name="command">The command to send, which is filter-specific.</param>
    /// <param name="argument">The argument to send with the command.</param>
    public MpvCommand AudioFilterCommand(string label, string command, string argument) =>
        C("af-command",
            label.CheckNotNullOrEmpty(nameof(label)),
            command.CheckNotNullOrEmpty(nameof(command)),
            argument.CheckNotNullOrEmpty(nameof(argument)));

    /// <summary>
    /// Applies the contents of a named profile. This is like using profile=name in a config file, except you can map it to a key binding to change it at runtime.
    /// There is no such thing as "unapplying" a profile - applying a profile merely sets all option values listed within the profile.
    /// </summary>
    /// <param name="name">The name of the profile to set.</param>
    public MpvCommand ApplyProfile(string name) =>
        C("apply-profile", name.CheckNotNullOrEmpty(nameof(name)));

    /// <summary>
    /// Load a script, similar to the --script option. Whether this waits for the script to finish initialization or not changed multiple times, and the future behavior is left undefined.
    /// </summary>
    /// <param name="fileName">The file path of the script to load.</param>
    public MpvCommand LoadScript(string fileName) =>
        C("load-script", fileName.CheckNotNullOrEmpty(nameof(fileName)));

    /// <summary>
    /// Changes list options as described in List Options.
    /// Some operations take no value, but the command still requires the value parameter. In these cases, the value must be an empty string.
    /// </summary>
    /// <param name="name">The normal option name.</param>
    /// <param name="operation">The suffix or action used on the option.</param>
    /// <param name="value">The value to apply the operation with.</param>
    public MpvCommand ChangeList(string name, ListOptionOperation operation, string value = "") =>
        C("change-list",
            name.CheckNotNullOrEmpty(nameof(name)),
            operation.CheckEnumValid(nameof(operation)).FormatMpvFlag(),
            value);

    //public MpvCommand ChangeList(string name, ListOptionOperation operation, IEnumerable<string> values)
    //{
    //    name.CheckNotNullOrEmpty(nameof(name));
    //    operation.CheckEnumValid(nameof(operation));
    //    values.CheckNotNull(nameof(values));

    //    var strValue = string.Join(",", values.Select(x => "%{0}%{1}".FormatInvariant(x.Length, x)));

    //    CommandRet("change-list", name, operation.FormatMpvFlag(), strValue);
    //}
}
