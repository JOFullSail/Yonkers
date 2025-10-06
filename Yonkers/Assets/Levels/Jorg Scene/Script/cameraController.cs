using System.Text;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] int mouSens;
    [SerializeField] int vertLockMin, vertLockMax;
    [SerializeField] bool invertY;

    float rotX;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        // Get the input
        float mouseX = Input.GetAxisRaw("Mouse X") * mouSens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouSens * Time.deltaTime;

        // Use invertY
        if (invertY)
        {
            // As the player moves the mouse upwards, the camera rotates on the x-axis
            rotX += mouseY;
        }
        else rotX -= mouseY;

        // Clamp camera in the x-axis
        rotX = Mathf.Clamp(rotX, vertLockMin, vertLockMax);

        // Camera rotated on the x-axis
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        // Player always rotates on the y-axis
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
