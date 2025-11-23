using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform spawnPos;

    [Tooltip("Models that will have their materials changed.")]
    [SerializeField] Renderer[] objects;

    [Tooltip("Material the object will change to when touched.")]
    [SerializeField] Material newMaterial;

    [Tooltip("Allows the object to revert to the previous material.\n\n" +
        "- All objects will change to the original material of the first object in the list.\n\n" +
        "- If disabled, the objects will permanently retain the new material.")]
    [SerializeField] bool canRevertMaterial;

    [Tooltip("Duration of the new material on the object before it reverts back to the original.")]
    [SerializeField] float materialDuration = 2f;

    //[Tooltip("Duration of the UI Checkpoint Label before it disappears")]
    //[SerializeField] float labelDuration;

    bool hasTriggered;
    bool triggeredThisSession = false;
    Material matOrig;

    public Transform SpawnPos
    { get { return spawnPos; } }
    private void Start()
    {
        if (objects.Length > 0) matOrig = objects[0].material;
        //else Debug.LogWarning("Please assign an object to the Objects array in the checkpoint.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        if(gameObject.name == "Train Checkpoint Flag")
        {
            GameManager.instance.playerSpawn.transform.parent = transform;
        }
        else
        {
            GameManager.instance.playerSpawn.transform.parent = null;
        }
        if (!triggeredThisSession)
        {
            triggeredThisSession = true;

            if (!hasTriggered)
            {
                hasTriggered = true;

                GameManager.instance.playerSpawn.transform.position = spawnPos.transform.position;
                GameManager.instance.playerSpawn.transform.rotation = spawnPos.transform.rotation;
                StartCoroutine(feedback());

                if (canRevertMaterial)
                    StartCoroutine(flashMaterial());
                else
                    foreach (Renderer model in objects)
                        model.material = newMaterial;
            }

            GameManager.instance.SaveGame();
        }
    }

    IEnumerator flashMaterial()
    {
        foreach (Renderer model in objects)
        {
            model.material = newMaterial;
        }

        yield return new WaitForSeconds(materialDuration);

        foreach (Renderer model in objects)
        {
            model.material = matOrig;
        }
    }

    IEnumerator feedback()
    {
        GameManager.instance.checkpointLabel.SetActive(true);
        yield return new WaitForSeconds(2f);
        GameManager.instance.checkpointLabel.SetActive(false);
    }
}