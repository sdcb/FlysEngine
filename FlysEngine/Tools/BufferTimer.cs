using System;

namespace FlysEngine.Tools;

public class BufferTimer
	{
		float _total = 0;

		public void Tick(float dt, float bufferTime, Action action)
		{
			_total += dt;
			while (_total > bufferTime)
			{
				action();
				_total -= bufferTime;
			}
		}
	}
