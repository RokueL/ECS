using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// MonoBehaviour that measures and displays frame timing statistics:
/// - Average, Minimum, Maximum FPS over a sliding window.
/// Attach to any GameObject in scene.
/// </summary>
public class FrameRateMonitor : MonoBehaviour
{
    [Tooltip("Number of frames to sample for statistics")]
    public int sampleSize = 300;

    private readonly Queue<float> frameTimes = new Queue<float>();
    private float minTime = float.MaxValue;
    private float maxTime = 0f;

    public float averageFPS { get; private set; }
    public float minFPS { get; private set; }
    public float maxFPS { get; private set; }

    void Update()
    {
        // Record current frame time
        float dt = Time.deltaTime;
        frameTimes.Enqueue(dt);

        // Maintain sample window
        if (frameTimes.Count > sampleSize)
            frameTimes.Dequeue();

        // Compute stats
        minTime = frameTimes.Min();
        maxTime = frameTimes.Max();
        float avgTime = frameTimes.Average();

        averageFPS = 1f / avgTime;
        minFPS = 1f / maxTime;
        maxFPS = 1f / minTime;
    }

    void OnGUI()
    {
        // Display in top-left corner
        GUILayout.BeginArea(new Rect(10, 10, 200, 90));
        GUILayout.Label($"FPS (Avg): {averageFPS:F1}");
        GUILayout.Label($"FPS (Min): {minFPS:F1}");
        GUILayout.Label($"FPS (Max): {maxFPS:F1}");
        GUILayout.Label($"Samples: {frameTimes.Count}");
        GUILayout.EndArea();
    }
}