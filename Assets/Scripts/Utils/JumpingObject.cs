using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingObject : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float circleRadius = 1f;

    private int currentWaypointIndex = 0;
    private Vector3 circleCenter;
    private bool isCircling = false;
    private float circleAngle = 0f;
    private float startTime;

    void Start()
    {
        transform.position = waypoints[currentWaypointIndex].position;
        circleCenter = transform.position;
        startTime = Time.time;
    }

    void Update()
    {
        if (isCircling)
        {
            circleAngle += speed * Time.deltaTime;

            if (circleAngle >= 2 * Mathf.PI)
            {
                isCircling = false;
            }
            else
            {
                Vector3 offset = new Vector3(Mathf.Cos(circleAngle), 0f, Mathf.Sin(circleAngle)) * circleRadius;
                MoveToNextWaypoint();
                transform.position = circleCenter + offset;
            }
        }
        else
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
            float timeToWaypoint = distanceToWaypoint / speed;

            if (Time.time - startTime >= timeToWaypoint)
            {
                isCircling = true;
                circleCenter = waypoints[currentWaypointIndex].position;
                startTime = Time.time;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, waypoints[currentWaypointIndex].position, (Time.time - startTime) / timeToWaypoint);
            }
        }
    }

    void MoveToNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        //transform.position = waypoints[currentWaypointIndex].position;
        //circleCenter = transform.position;
        startTime = Time.time;
    }
}
