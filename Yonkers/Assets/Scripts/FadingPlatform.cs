using UnityEngine;
using System.Collections;

public class FadingPlatform : MonoBehaviour
{
    [SerializeField] float fadeSpeed = 1f, fadeDelay = 0.5f, respawnTime = 3f;
    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform targetObject;

    MeshRenderer rend;
    Collider coll;
    Material instancedMat;
    Color baseColor;
    bool playerOn, fading;

    void Awake()
    {
        if (!targetObject) targetObject = transform;

        rend = targetObject.GetComponent<MeshRenderer>();
        coll = targetObject.GetComponent<Collider>();

        if (!rend || !coll)
        {
            enabled = false;
            return;
        }

        instancedMat = new Material(rend.material);
        rend.material = instancedMat;
        baseColor = instancedMat.color;
        SetupTransparent(instancedMat);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerOn = true;
            if (!fading) StartCoroutine(FadeRoutine());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerOn = false;
    }

    IEnumerator FadeRoutine()
    {
        fading = true;

        float timer = 0f;
        while (timer < fadeDelay)
        {
            if (!playerOn)
            {
                fading = false;
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }

        rend.enabled = coll.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        SetAlpha(1f);
        rend.enabled = coll.enabled = true;
        fading = false;
    }


    void SetAlpha(float alpha)
    {
        var c = baseColor;
        c.a = alpha;
        instancedMat.color = c;
    }

    void SetupTransparent(Material mat)
    {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
    }
}
