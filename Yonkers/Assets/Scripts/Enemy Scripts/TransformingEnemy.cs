using System.Linq;
using UnityEngine;

public class TransformingEnemy : EnemyAI
{
    [Header("Transforming Enemy Parameters")]
    [SerializeField] GameObject[] enemyForms;

    [SerializeField] bool canTurnIntoItems;
    [SerializeField] GameObject[] itemForms;
    [SerializeField][Range(0, 100)] int itemTransformChance;

    [SerializeField] ParticleSystem transformVFX;
    
    void Update()
    {
        enemyRoutine();

        if (playerDetected)
            Transform();
    }

    private void Transform()
    {
        if(transformVFX != null)
            Instantiate(transformVFX, transform.position, transform.rotation);

        Vector3 currentPosition = transform.position;
        currentPosition.y += 0.5f;
        Quaternion currentRotation = transform.rotation;

        gameObject.SetActive(false);

        if (canTurnIntoItems && Random.Range(0, 100) <= itemTransformChance && itemForms.Count() > 0)
        {
            int itemPos = Random.Range(0, itemForms.Length);
            Instantiate(itemForms[itemPos], currentPosition, itemForms[itemPos].transform.rotation);
        }
            

        else if (enemyForms.Count() > 0)
            Instantiate(enemyForms[Random.Range(0, enemyForms.Length)], currentPosition, currentRotation);

        Destroy(gameObject);
    }
}
