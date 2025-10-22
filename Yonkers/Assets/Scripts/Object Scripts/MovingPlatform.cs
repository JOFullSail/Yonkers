using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private Transform platform;
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Header("Movement Settings")]
    [SerializeField] private List<float> segSpeeds = new List<float>();
    [SerializeField] private float delay = 0f;
    [SerializeField] private float stopThreshold = 0.01f;

    [Header("Rotation Settings")]
    [SerializeField] private bool rotateTowardsPath = true;
    [SerializeField] private float rotationSpeed = 5f;

    private int waypointIndex = 0;
    private const float DefSpeed = 50f;

    private void OnValidate()
    {
        if (waypoints == null) return;

        int requiredCount = waypoints.Count;

        if (segSpeeds == null)
        {
            segSpeeds = new List<float>();
        }

        while (segSpeeds.Count < requiredCount)
        {
            if (segSpeeds.Count == 0) 
                segSpeeds.Add(DefSpeed);
            else
                segSpeeds.Add(segSpeeds[segSpeeds.Count - 1]);
        }

        while (segSpeeds.Count > requiredCount)
        {
            segSpeeds.RemoveAt(segSpeeds.Count - 1);
        }
    }

    private void Start()
    {
        if (waypoints == null || waypoints.Count < 2)
            return;

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
            float currentSpeed = segSpeeds[previousIndex];

            while ((targetPosition - platform.position).sqrMagnitude > stopThreshold * stopThreshold)
            {
                platform.position = Vector3.MoveTowards(platform.position, targetPosition, currentSpeed * Time.deltaTime);

                if (rotateTowardsPath)
                {
                    Vector3 direction = (targetPosition - platform.position).normalized;
                    if (direction.sqrMagnitude > 0.01f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        platform.rotation = Quaternion.Slerp(platform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }
                }

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
