using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class MeleeEnemy : EnemyAI
{
    [SerializeField] Transform EnemyView;
    [SerializeField] int MeleeDamage;
    [SerializeField] float MeleeDelay;
    [SerializeField] float DashDistance;
    [SerializeField] float DashSpeed;
    [SerializeField] float DashRate;

    bool isDash;
    bool PlayerinAttackR;
    float AttackTimer;
    Ray OnSight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {

        roamRoutine();
        AttackTimer += Time.deltaTime;
        if(canSeePlayer())
        {

           if(AttackTimer >= MeleeDelay )
           {
                dashattack();
                AttackTimer = 0;
           }
            
        }
        
    }

   
    void dashattack()
    {
        AttackTimer += Time.deltaTime;
        Vector3 PlayerPosition = GameManager.instance.player.transform.position;
        Vector3 PushPosition = new Vector3(PlayerPosition.x, 0, PlayerPosition.z);
        RaycastHit Detected;
        float Dist = Vector3.Distance(GameManager.instance.player.transform.position, transform.position);
        if(Physics.Raycast(EnemyView.position, (PlayerPosition - transform.position).normalized, out Detected, DashDistance))
        {

            if (Detected.collider.CompareTag("Player"))
            {
                PlayerinAttackR = true;
                Debug.Log("Player Dectected");
                //transform.position = Vector3.MoveTowards(transform.position, PushPosition, Dist);
                transform.position = Vector3.Lerp(transform.position, PushPosition, DashDistance/AttackTimer);
            }
            else
            {
                PlayerinAttackR = false;
            }
        }
        
    }


}
