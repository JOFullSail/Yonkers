using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class MeleeEnemy : EnemyAI
{
    [Header("Melee Attributes")]
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
    Vector3 SetPushPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {

        roamRoutine();
        AttackTimer += Time.deltaTime;
        if(canSeePlayer())
        {



            if (PlayerinAttackR == true)
            {
               
                transform.position = SetPushPosition;
            }
            
            if (AttackTimer >= MeleeDelay)
            {

                dashattack();
                AttackTimer = 0;
            }
            
        }
        
    }

   
    void dashattack()
    {
        Vector3 PlayerPosition = GameManager.instance.player.transform.position;
        Vector3 PushPosition = new Vector3(PlayerPosition.x, transform.forward.y, PlayerPosition.z);
        RaycastHit Detected;
        if(Physics.Raycast(transform.position, (PlayerPosition - transform.position).normalized, out Detected, DashDistance))
        {

            if (Detected.collider.CompareTag("Player"))
            {
                //PlayerinAttackR = true;
                Debug.Log("Player Dectected");
                SetPushPosition = Vector3.Lerp(EnemyView.forward, SetPushPosition, Time.deltaTime * DashSpeed);
                ////transform.position = Vector3.MoveTowards(transform.position, PushPosition, Dist);
                //transform.position = Vector3.Lerp(transform.position, PushPosition, Time.deltaTime * DashSpeed);

            }
            
        }
        
    }


}
