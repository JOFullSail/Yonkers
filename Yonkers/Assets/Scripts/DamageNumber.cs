using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifetime = 1f;
    public TextMeshProUGUI text;

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }

    public void Initialize(int damage)
    {
        text.text = damage.ToString();
        Destroy(gameObject, lifetime);
    }
}
