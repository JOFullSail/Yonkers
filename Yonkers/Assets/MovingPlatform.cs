using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private Transform platform;
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float delay = 1f;
    [SerializeField] private float stopThreshold = 0.01f;

    private int waypointIndex = 0;

    private void Start()
    {
        platform.position = waypoints[0].position;
        waypointIndex = 1;
        StartCoroutine(MovePlatform());
    }

    private IEnumerator MovePlatform()
    {
        while (true)
        {
            Vector3 targetPosition = waypoints[waypointIndex].position;

            while ((targetPosition - platform.position).sqrMagnitude > stopThreshold * stopThreshold)
            {
                platform.position = Vector3.MoveTowards(platform.position, targetPosition, speed * Time.deltaTime);
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
