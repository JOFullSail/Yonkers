using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private Transform platform;
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Header("Movement Settings")]
    [SerializeField] private List<float> segmentSpeeds = new List<float>();

    [SerializeField] private float delay = 1f;
    [SerializeField] private float stopThreshold = 0.01f;

    private int waypointIndex = 0;

    private void Start()
    {
        if (waypoints == null)
        {
            return;
        }

        if (segmentSpeeds == null)
        {
            segmentSpeeds = new List<float>(new float[waypoints.Count]);
            for (int i = 0; i < segmentSpeeds.Count; i++)
            {
                segmentSpeeds[i] = 5f;
            }
        }

        platform.position = waypoints[0].position;
        waypointIndex = 1;
        StartCoroutine(MovePlatform());
    }

    private IEnumerator MovePlatform()
    {
        while (true)
        {
            Vector3 targetPosition = waypoints[waypointIndex].position;

            int previousIndex = (waypointIndex - 1 + waypoints.Count) % waypoints.Count;
            float currentSpeed = segmentSpeeds[previousIndex];

            while ((targetPosition - platform.position).sqrMagnitude > stopThreshold * stopThreshold)
            {
                platform.position = Vector3.MoveTowards(platform.position, targetPosition, currentSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(delay);

            waypointIndex = (waypointIndex + 1) % waypoints.Count;
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform currentWaypoint = waypoints[i];
            Transform nextWaypoint = waypoints[(i + 1) % waypoints.Count]; 
            if (currentWaypoint != null && nextWaypoint != null)
            {
                Gizmos.DrawLine(currentWaypoint.position, nextWaypoint.position);
                Gizmos.DrawSphere(currentWaypoint.position, 0.1f);
            }
        }
    }
}