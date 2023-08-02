using System;

namespace FlysEngine.Tools;

/// <summary>
/// A utility class for handling timed events with a buffer time for performance optimization.
/// </summary>
public class BufferTimer
{
    private float _total = 0;

    /// <summary>
    /// Executes the given action every bufferTime seconds.
    /// </summary>
    /// <param name="dt">The change in time since the last update.</param>
    /// <param name="bufferTime">The time buffer interval to execute the action.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>The count of times the action was executed.</returns>
    public int Tick(float dt, float bufferTime, Action action)
    {
        _total += dt;
        int tickCount = 0;
        while (_total > bufferTime)
        {
            action();
            tickCount++;
            _total -= bufferTime;
        }
        return tickCount;
    }
}
