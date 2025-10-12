using UnityEditor;
using UnityEngine;


public class MeleeEnemy : EnemyAI
{
    [Header("Melee Attributes")]
    [SerializeField] Transform EnemyPOS;
    [SerializeField] int MeleeDamage;
    [SerializeField] float MeleeDelay;
    [SerializeField] float DashDistance;
    [SerializeField] float DashSpeed;
    [SerializeField] float TargetDistance;
    
    bool AttackRange;
    float AttackTimer;
    Vector3 PlayerPosition;
    Vector3 NewPushPosition;
    Vector3 Dir;

    void Update()
    {
        PlayerPosition = GameManager.instance.player.transform.position - transform.position;
        AttackTimer += Time.deltaTime;
        roamRoutine();
        if (canSeePlayer())
        {
            if (AttackTimer > MeleeDelay)
            {
                dashattack();
                AttackTimer = 0;
            }
            if (AttackRange == true)
            {
                transform.position = Vector3.Lerp(transform.position, NewPushPosition, Time.deltaTime * DashSpeed);
            }
        }
    }

   
    void dashattack()
    {
        RaycastHit Detected;
        Debug.DrawRay(transform.position, PlayerPosition, color: Color.blue);
        if(Physics.Raycast(transform.position, (PlayerPosition).normalized, out Detected, TargetDistance))
        {
            Debug.Log("Object Detected");
            if (Detected.collider.CompareTag("Player"))
            {
                Dir = transform.forward;
                NewPushPosition = new Vector3 (GameManager.instance.player.transform.position.x, 0, GameManager.instance.player.transform.position.z);
                Debug.Log("Player Dectected");
                AttackRange = true; 
            }
            else
                  AttackRange = false;
        }
    }


}
