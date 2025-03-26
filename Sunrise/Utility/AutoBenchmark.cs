using System.Diagnostics;

namespace Sunrise.Utility;

internal class AutoBenchmark(string targetName, float reportInterval = 10)
{
    readonly Stopwatch _consumedTime = new();
    readonly Stopwatch _totalTime = new();

    public void Start()
    {
        _consumedTime.Start();
        _totalTime.Start();
    }

    public void Stop()
    {
        _consumedTime.Stop();

        if (_totalTime.Elapsed.TotalSeconds >= reportInterval)
        {
            double precentage = _consumedTime.Elapsed.TotalSeconds / _totalTime.Elapsed.TotalSeconds;

            Debug.Log($"Time consumed by {targetName} in last {_totalTime.Elapsed.TotalSeconds:F}: {_consumedTime.Elapsed.TotalSeconds:F} ({precentage:P2})");

            _consumedTime.Reset();
            _totalTime.Restart();
        }
    }
}