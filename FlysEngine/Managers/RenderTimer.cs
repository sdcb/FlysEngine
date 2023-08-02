using System;
using System.Diagnostics;

namespace FlysEngine.Managers;

/// <summary>
/// Keeps track of the frame rate and duration of a single frame.
/// </summary>
public class RenderTimer
{
    private long _lastFpsTick = Stopwatch.GetTimestamp();
    private long _lastTick = Stopwatch.GetTimestamp();
    private readonly Stopwatch _frameStopwatch = new();

    private long _frames;

    /// <summary>
    /// Duration since the last rendered frame.
    /// </summary>
    public TimeSpan DurationSinceLastFrame { get; private set; }

    /// <summary>
    /// The total number of frames that have been rendered.
    /// </summary>
    public long TotalFrames { get; private set; } = 0;

    /// <summary>
    /// Number of rendered frames per second.
    /// </summary>
    public float FramesPerSecond { get; private set; }

    /// <summary>
    /// Starts a new frame and returns the duration of the last frame.
    /// </summary>
    /// <returns>The duration of the last frame.</returns>
    public float BeginFrame()
    {
        _frameStopwatch.Restart();
        long currentTs = Stopwatch.GetTimestamp();
        long duration = currentTs - _lastFpsTick;
        if (duration > Stopwatch.Frequency)
        {
            FramesPerSecond = 1.0f * _frames / duration * Stopwatch.Frequency;
            _lastFpsTick = currentTs;
            _frames = 0;
        }
        float lastFrameTimeInSeconds = 1.0f * (currentTs - _lastTick) / Stopwatch.Frequency;
        _lastTick = currentTs;
        return lastFrameTimeInSeconds;
    }

    /// <summary>
    /// Ends a rendered frame, increments rendered frame counter and frame rate counter.
    /// </summary>
    public void EndFrame()
    {
        _frameStopwatch.Stop();
        DurationSinceLastFrame = _frameStopwatch.Elapsed;
        ++_frames;
        ++TotalFrames;
    }
}
