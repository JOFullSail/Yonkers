using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float shakeTime = 2f, shakeSpeed = 20f, shakeAmount = 0.5f;
    [SerializeField] bool shakeX = true, shakeY = false, shakeZ = true;
    [SerializeField] float fallSpeed = 3f, fallDistance = 5f, respawnTime = 5f;
    [SerializeField] string playerTag = "Player";

    Vector3 startPos;
    bool active = true;
    Coroutine shakeRoutine;
    bool playerOnPlatform;

    Renderer rend;
    Collider col;

    void Awake()
    {
        if (!target) target = transform;
        startPos = target.position;

        rend = target.GetComponent<Renderer>();
        col = target.GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!active || !other.CompareTag(playerTag) || !col) return;

        playerOnPlatform = true;
        if (shakeRoutine == null)
            shakeRoutine = StartCoroutine(ShakeThenFall());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerOnPlatform = false;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
            target.position = startPos;
        }
    }

    IEnumerator ShakeThenFall()
    {
        Vector3 original = target.position;
        float timer = 0f;
        float interval = 1f / shakeSpeed;
        float shakeTimer = 0f;
        Vector3 currentOffset = Vector3.zero;
        Vector3 nextOffset = Random.insideUnitSphere * shakeAmount;

        while (timer < shakeTime)
        {
            if (!playerOnPlatform)
                yield break;

            timer += Time.deltaTime;
            shakeTimer += Time.deltaTime;

            if (shakeTimer >= interval)
            {
                shakeTimer = 0f;
                currentOffset = nextOffset;
                nextOffset = Random.insideUnitSphere * shakeAmount;
            }

            Vector3 offset = Vector3.Lerp(currentOffset, nextOffset, shakeTimer / interval);
            offset = new Vector3(shakeX ? offset.x : 0, shakeY ? offset.y : 0, shakeZ ? offset.z : 0);
            target.position = original + offset;
            yield return null;
        }

        StartCoroutine(FallRoutine());
    }

    IEnumerator FallRoutine()
    {
        active = false;
        Vector3 fallTo = startPos + Vector3.down * fallDistance;

        while ((target.position - fallTo).sqrMagnitude > 0.001f)
        {
            target.position = Vector3.MoveTowards(target.position, fallTo, fallSpeed * Time.deltaTime);
            yield return null;
        }

        if (rend) rend.enabled = false;
        if (col) col.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        target.position = startPos;
        if (rend) rend.enabled = true;
        if (col) col.enabled = true;
        active = true;
    }
}
