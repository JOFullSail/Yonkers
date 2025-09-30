using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] int sens;
    [SerializeField] int lockVertMin, LockVertMax;
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
        // get input
        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;

        // use invertY
        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;

        // clamp the camera on the X axis
        rotX = Mathf.Clamp(rotX, lockVertMin, LockVertMax);

        // rotate the camera on the X axis
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        // rotate the player on the Y axis
        transform.parent.Rotate(Vector3.up * mouseX);

    }
}
