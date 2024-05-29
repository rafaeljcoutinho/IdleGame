using System;
using System.Collections.Generic;
using UnityEngine;

public class Cameramod : MonoBehaviour
{
    [SerializeField] public Transform playerTransform;
    [SerializeField] private List<float> distances;
    [SerializeField] private List<float> heights;
    [NonSerialized] public new Camera camera;

    public Vector3 angle = new (45.0f, 0,0 );
    private int selected;
    private float distance => distances[selected];
    private float height => heights[selected];
    private Vector3 offset;
    private Vector3 speed;

    public void Init(Camera cam)
    {
        camera = cam;
    }

    private void Start()
    {
        offset = Quaternion.Euler(angle) * Vector3.back * distance;
        Vector3 newPosition = playerTransform.position + offset;
        newPosition.y = playerTransform.position.y + height;
        camera.transform.position = newPosition;
        camera.transform.rotation = Quaternion.LookRotation(playerTransform.position - camera.transform.position);
    }

    private void ToggleCameraZoom()
    {
        selected++;
        selected %= distances.Count;
    }

    [SerializeField] private float speedT;
    [SerializeField] private float speedR;

    void LateUpdate()
    {
        Vector3 newPosition = playerTransform.position + offset;
        newPosition.y = playerTransform.position.y + height;
        camera.transform.position = Vector3.Slerp(camera.transform.position, newPosition, Time.deltaTime * speedT);
    }
}