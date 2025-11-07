using UnityEngine;

public class ascendTest : MonoBehaviour, IActivate
{
    // Simple test to demonstrate that Activation Switches can trigger scripts.
    bool active = false;

    void Start()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (active) transform.Translate(Vector3.up * 5f * Time.deltaTime, Space.World);
    }
    
    // Function inherited from IActivate
    public void activate()
    {
        gameObject.SetActive(true);
        active = true;
    }
}
