using System.Diagnostics;

namespace FlysEngine.Managers;

public class FpsManager
{
    private long _lastFpsTick = Stopwatch.GetTimestamp();
    private long _lastTick = Stopwatch.GetTimestamp();
    private readonly Stopwatch _stopwatch = new();

    public long Frames { get; private set; }

    public double FrameTimeMs { get; private set; }

    public float Fps { get; private set; }

    public float BeginFrame()
    {
        _stopwatch.Restart();
        var currentTs = Stopwatch.GetTimestamp();
        var duration = currentTs - _lastFpsTick;
        if (duration > Stopwatch.Frequency)
        {
            Fps = 1.0f * Frames / duration * Stopwatch.Frequency;
            _lastFpsTick = currentTs;
            Frames = 0;
        }
        var dt = 1.0f * (currentTs - _lastTick) / Stopwatch.Frequency;
        _lastTick = currentTs;
        return dt;
    }

    public void EndFrame()
    {
        _stopwatch.Stop();
        FrameTimeMs = _stopwatch.Elapsed.TotalMilliseconds;
        ++Frames;
    }
}
