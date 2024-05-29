using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    private float movingAverage = 1;
    private List<string> fpsLabels = new();
    private Queue<int> frames = new(10);

    private void Start()
    {
        for (var i = 0; i < 500; i++)
        {
            fpsLabels.Add(i.ToString());
        }
    }


    void Update()
    {
        if (frames.Count > 30)
            frames.Dequeue();
        var dt = Time.deltaTime;
        var fps = 1 / dt;
        frames.Enqueue((int)fps);
        var min = (int)fps;
        foreach (var val in frames)
        {
            if (val < min)
            {
                min = val;
            }
        }
        label.text = fpsLabels[min];
    }
}
