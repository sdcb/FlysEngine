using System.Diagnostics;

namespace FlysEngine.Managers;

/// <summary>
/// The manager responsible for managing the FPS values and times.
/// </summary>
public class FpsManager
{
    private long _lastFpsTick = Stopwatch.GetTimestamp();
    private long _lastTick = Stopwatch.GetTimestamp();
    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// The total number of frames that have occurred.
    /// </summary>
    public long Frames { get; private set; }

    /// <summary>
    /// The total time it takes to process a single frame.
    /// </summary>
    public double FrameTimeMs { get; private set; }

    /// <summary>
    /// The current frames per second.
    /// </summary>
    public float Fps { get; private set; }

    /// <summary>
    /// Begins a frame for measuring FPS.
    /// </summary>
    /// <returns>The amount of time since the last frame in seconds.</returns>
    public float BeginFrame()
    {
        _stopwatch.Restart();
        long currentTs = Stopwatch.GetTimestamp();
        long duration = currentTs - _lastFpsTick;
        if (duration > Stopwatch.Frequency)
        {
            Fps = 1.0f * Frames / duration * Stopwatch.Frequency;
            _lastFpsTick = currentTs;
            Frames = 0;
        }
        float dt = 1.0f * (currentTs - _lastTick) / Stopwatch.Frequency;
        _lastTick = currentTs;
        return dt;
    }

    /// <summary>
    /// Ends the current frame and updates FPS values.
    /// </summary>
    public void EndFrame()
    {
        _stopwatch.Stop();
        FrameTimeMs = _stopwatch.Elapsed.TotalMilliseconds;
        ++Frames;
    }
}
