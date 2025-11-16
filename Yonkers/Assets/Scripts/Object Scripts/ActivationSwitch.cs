using UnityEngine;
using System.Collections.Generic;

public class ActivationSwitch : MonoBehaviour, IActivate
{
    [Tooltip("Object with an ActivationEvent script." +
             "\n\n- If there is no object attached, the script will search the parent for the script automatically." +
             "\n\n- If an object is attached, it'll make sure it has the ActivationEvent script.")]
    [SerializeField] GameObject activationEventParent;
    
    [Tooltip("List of objects that will change in color." +
             "\n\n- If the list is empty, it will only change the color of the object that retains this script.")]
    [SerializeField] public List<Renderer> coloredObjects;

    [Tooltip("If ON, this switch will not change materials. " +
             "\n\n- Will change all objects in the Colored Objects list to the colors that are set in the " +
             "ActivationEvent script.")]
    [SerializeField] bool changeColorOnly = true;
    
    [Tooltip("Allows the player to activate this switch by approaching it." +
             "\n\n- Remember, a trigger collider is required for this functionality.")]
    [SerializeField] bool isTouchActivated;
    
    ActivationEvent parentScript;
    
    bool activated;

    public bool ChangeColorOnly
    {
        get { return changeColorOnly; }
    }

    void Start()
    {
        // Prerequisite Checks
        if (activationEventParent) parentScript = activationEventParent.GetComponent<ActivationEvent>();
        else if (!activationEventParent) parentScript = GetComponentInParent<ActivationEvent>();
        
        if (!parentScript)
        { 
            Debug.LogWarning("Parent Script not found. Please attach the ActivationEvent parent script of this " +
                           "activation switch object."); 
            enabled = false; 
            return;
        }

        //if (coloredObjects.Count == 0 && TryGetComponent<Renderer>(out var ren))
        //{
        //    coloredObjects.Add(ren);
        //}
        //else if (coloredObjects.Count == 0)
        //{
        //    Debug.LogWarning("No Objects attached to the \"Colored Objects\" list. Please attach at least one.");
        //}
    }

    public void activate()
    {
        // increment activeCount, change materials or turn them a particular color based on the bool, and set
        // an ontriggerenter if istouchactive is on (you call activate in it). i think that's it
        if (!activated)
        {
            ++parentScript.ActiveCount;

            //if (!parentScript.EventTriggered && changeColorOnly || !parentScript.ActivationMaterial)
            //{
            //    foreach (Renderer obj in coloredObjects)
            //    {
            //        obj.material.color = parentScript.ActivationColor;
            //    }
            //}
            //else if (!parentScript.EventTriggered)
            //{
            //    foreach (Renderer obj in coloredObjects)
            //    {
            //        obj.material = parentScript.ActivationMaterial;
            //    }
            //}
        
            activated = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTouchActivated && enabled && other.CompareTag("Player")) activate();
    }
}
