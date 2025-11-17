using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActivationEvent : MonoBehaviour
{
    [Tooltip("Object or object containing script that will be activated when all activation switches are ON.")] 
    [SerializeField] GameObject[] eventObjects;
    
    [Tooltip("Allows the activation switches to activate the Event Object's script instead." +
             "\n\n- The script that will be activated should inherit from the IActivate interface.")]
    [SerializeField] bool[] activateScript;
    
    [Tooltip("List containing all activation switches the ActivationEvent will use." +
             "\n\n- If the list is empty, the script will search for its children and add " +
             "the ones with the ActivationSwitch script automatically." +
             "\n\n- If the list has a switch attached, the script will assume those are the only switches that " +
             "will be used to activate the event.")]
    [SerializeField] List<ActivationSwitch> activationSwitches;
    
    [Tooltip("Material the switches will swap to when they get activated." +
             "\n\n- If no material is attached, the respective color will be used instead." +
             "\n\n- You can override the materials in the ActivationSwitch script.")]
    [SerializeField] Material activationMaterial;
    
    [Tooltip("Color the switches will swap to when they get activated. This option is also included " +
             "as a way to change the color of a switch if they already have a texture attached to them." +
             "\n\n- You can override the materials in the ActivationSwitch script.")]
    [SerializeField] Color activationColor = Color.blue;
    
    [Tooltip("Material the switches will swap to when all switches in the list are activated." +
             "\n\n- If no material is attached, the respective color will be used instead." +
             "\n\n- You can override the materials in the ActivationSwitch script.")]
    [SerializeField] Material groupCompleteMaterial;
    
    [Tooltip("Color the switches will swap to when all switches in the list are activated. This option is also included " +
             "as a way to change the color of a switch if they already have a texture attached to them." +
             "\n\n- You can override the materials in the ActivationSwitch script.")]
    [SerializeField] Color groupCompleteColor = Color.green;
    
    IActivate[] eventScript;

    int activeCount = 0;

    bool[] eventTriggered;

    public Material ActivationMaterial
    {
        get { return activationMaterial; }
    }

    public Color ActivationColor
    {
        get { return activationColor; }
    }
    
    public bool EventTriggered (int index)
    {

        return eventTriggered[index];
    }

    public int ActiveCount
    {
        get { return activeCount; }
        set {  activeCount = value; }
    }

    void Start()
    {
        eventScript = new IActivate[eventObjects.Count()];
        eventTriggered = new bool[eventObjects.Count()];
        // Searches for the switches automatically.
        if (activationSwitches.Count == 0)
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<ActivationSwitch>(out var act))
                {
                    activationSwitches.Add(act);
                }
            }
            if (activationSwitches.Count == 0)
            {
                //Debug.LogWarning("No activation switches found. Please turn the switches into children of the " + "activation event object or manually attach their scripts to the \"Activation Switches\" list.");
                enabled = false;
                return;
            }
        }
        for (int index = 0; index < eventObjects.Count(); index++)
        {
            // Prerequisite Checks
            if (activateScript[index])
            {
                if (!eventObjects[index] || !eventObjects[index].TryGetComponent<IActivate>(out eventScript[index]))
                {
                    //Debug.LogWarning("No IActivate object attached. Please attach an Object with an \"IActivate\" interface " + "or turn OFF \"Activate Script\" and attach an event object to \"Event Object.\"");
                    enabled = false;
                    return;
                }
            }
            else if (!activateScript[index] && !eventObjects[index])
            {
                //Debug.LogWarning("No event object attached. Please attach an event object to \"Event Object\" or turn ON " + "\"Activate Script\" and attach a object with an \"IActivate\" interface.");
                enabled = false;
                return;
            }
        
            if (!activateScript[index])
            {
                eventObjects[index].SetActive(!eventObjects[index].activeSelf);
            }
        }

    }

    void Update()
    {                
        for (int index = 0; index < eventObjects.Count(); index++)
             {
        // Changes the color of activation switches and triggers the event.
            if (!eventTriggered[index] && activeCount >= activationSwitches.Count)
            {
                foreach (ActivationSwitch swt in activationSwitches)
                {
                    if (swt.ChangeColorOnly || !groupCompleteMaterial)
                    {
                        foreach (Renderer obj in swt.coloredObjects)
                        {
                            obj.material.color = groupCompleteColor;
                        }
                    }
                    else
                    {
                        foreach (Renderer obj in swt.coloredObjects)
                        {
                            obj.material = groupCompleteMaterial;
                        }
                    }
                }
                if (activateScript[index])
                {
                    eventScript[index].activate();
                }
                else
                {
                    eventObjects[index].SetActive(!eventObjects[index].activeSelf);
                }
                eventTriggered[index] = true;
                }
        }
    }
}
