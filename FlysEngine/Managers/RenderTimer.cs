using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FlysEngine.Managers
{
    public class RenderTimer
    {
        private long _lastFpsTick = Stopwatch.GetTimestamp();
        private long _lastTick = Stopwatch.GetTimestamp();
        private Stopwatch _frameStopwatch = new Stopwatch();

        private long _frames;

        public TimeSpan DurationSinceLastFrame { get; private set; }

        public long TotalFrames { get; private set; } = 0;

        public float FramesPerSecond { get; private set; }

        public float BeginFrame()
        {
            _frameStopwatch.Restart();
            var currentTs = Stopwatch.GetTimestamp();
            var duration = currentTs - _lastFpsTick;
            if (duration > Stopwatch.Frequency)
            {
                FramesPerSecond = 1.0f * _frames / duration * Stopwatch.Frequency;
                _lastFpsTick = currentTs;
                _frames = 0;
            }
            var lastFrameTimeInSecond = 1.0f * (currentTs - _lastTick) / Stopwatch.Frequency;
            _lastTick = currentTs;
            return lastFrameTimeInSecond;
        }

        public void EndFrame()
        {
            _frameStopwatch.Stop();
            DurationSinceLastFrame = _frameStopwatch.Elapsed;
            ++_frames;
            ++TotalFrames;
        }
    }
}
