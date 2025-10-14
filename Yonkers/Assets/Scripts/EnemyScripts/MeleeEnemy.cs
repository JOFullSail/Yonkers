using UnityEditor;
using UnityEngine;


public class MeleeEnemy : EnemyAI
{
    [Header("Melee Attributes")]
    [SerializeField] Transform EnemyView;
    [SerializeField] int MeleeDamage;
    [SerializeField] float MeleeDelay;
    [SerializeField] float DashDistance;
    [SerializeField] float DashSpeed;
    [SerializeField] float TargetDistance;
    

    bool isDash;
    bool AttackRange;
    float AttackTimer;
    Vector3 PlayerPosition;
    Vector3 NewPushPosition;
    Vector3 Dir;

    //Vector3 PushPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
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

            if (AttackRange == true) {

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
                Vector3 Dir = transform.forward;
                NewPushPosition = GameManager.instance.player.transform.position;
                Debug.Log("Player Dectected");
                //transform.position = Vector3.Lerp(transform.position, NewPushPosition, Time.deltaTime * DashSpeed);
                AttackRange = true;
               
                
            }
            else
            {
                AttackRange = false;
            }
        }
        
        
    }


}
