using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour, IActivate
{
    [Header("Platform")]
    public Transform platform;
    public List<Transform> waypoints = new();
    public List<float> segSpeeds = new();
    public float delay, stopThreshold = 0.01f;
    public bool rotateTowardsPath = true;
    public float rotationSpeed = 5f;
    public bool UseActivate;
    bool onetimeactivate;

    [System.Serializable]
    public class TriggerInfo
    {
        public Collider trigger;
        public bool stayTriggered;
    }

    [System.Serializable]
    public class Condition
    {
        [Header("Condition Settings")]
        public int waypointIndex;
        public int targetIndex;
        public bool requireAll = true;
        public bool whenTriggered = true;

        [Header("Triggers")]
        public List<TriggerInfo> triggers = new();
        [HideInInspector] public List<bool> states = new();
    }

    public List<Condition> conditions = new();
    int index;
    const float DefSpeed = 50f;

    void Start()
    {
        onetimeactivate = true;
        if (waypoints.Count < 2) return;

        while (segSpeeds.Count < waypoints.Count) segSpeeds.Add(DefSpeed);
        while (segSpeeds.Count > waypoints.Count) segSpeeds.RemoveAt(segSpeeds.Count - 1);

        foreach (var c in conditions)
        {
            c.states = new List<bool>(new bool[c.triggers.Count]);
            for (int i = 0; i < c.triggers.Count; i++)
            {
                if (!c.triggers[i].trigger) continue;
                var relay = c.triggers[i].trigger.gameObject.AddComponent<TriggerRelay>();
                relay.Setup(c, i);
            }
        }

        platform.position = waypoints[0].position;
        index = 1;
    }

    void Update()
    {
        if (!UseActivate && onetimeactivate)
        {
            onetimeactivate = false;
            StartCoroutine(Move());
        }
    }

    IEnumerator Move()
    {
        while (true)
        {
            Vector3 target = waypoints[index].position;
            float speed = segSpeeds[Mathf.Clamp(index - 1, 0, segSpeeds.Count - 1)];

            while (true)
            {
                Vector3 toTarget = target - platform.position;
                float sqrDist = toTarget.sqrMagnitude;
                float sqrThresh = stopThreshold * stopThreshold;

                if (sqrDist <= sqrThresh) break;

                Vector3 nextPos = Vector3.MoveTowards(platform.position, target, speed * Time.deltaTime);
                float nextSqrDist = (target - nextPos).sqrMagnitude;
                if (nextSqrDist < sqrThresh)
                {
                    Vector3 dir = toTarget.normalized;
                    platform.position = target - dir * stopThreshold;
                    break;
                }

                platform.position = nextPos;

                if (rotateTowardsPath)
                {
                    Vector3 dir = toTarget.normalized;
                    if (dir.sqrMagnitude > 0.01f)
                        platform.rotation = Quaternion.Slerp(
                            platform.rotation,
                            Quaternion.LookRotation(dir),
                            rotationSpeed * Time.deltaTime
                        );
                }

                yield return null;
            }


            yield return new WaitForSeconds(delay);
            index = GetNextIndex(index);
        }

    }

    int GetNextIndex(int current)
    {
        foreach (var c in conditions)
        {
            if (c.waypointIndex != current) continue;

            int active = 0;
            for (int i = 0; i < c.states.Count; i++)
                if (c.states[i]) active++;

            bool met = c.requireAll ? active == c.states.Count : active > 0;

            if (c.whenTriggered ? met : !met)
                return Mathf.Clamp(c.targetIndex, 0, waypoints.Count - 1);
        }

        return (current + 1) % waypoints.Count;
    }

    class TriggerRelay : MonoBehaviour
    {
        Condition cond;
        int id;

        public void Setup(Condition c, int i)
        {
            cond = c;
            id = i;
        }

        void OnTriggerEnter(Collider o)
        {
            if (o.CompareTag("Player"))
                cond.states[id] = true;
        }

        void OnTriggerExit(Collider o)
        {
            if (!o.CompareTag("Player")) return;
            if (cond.triggers[id].stayTriggered) return;

            cond.states[id] = false;
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] && waypoints[(i + 1) % waypoints.Count])
                Gizmos.DrawLine(waypoints[i].position, waypoints[(i + 1) % waypoints.Count].position);
        }
    }

    public void activate()
    {
        UseActivate = false;
    }
}
