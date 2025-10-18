
using System.Collections;
using UnityEngine;



public class MeleeEnemy : EnemyAI
{

    
    [Header("Melee Attributes")]
    [SerializeField] Transform EnemyPOS;// Enemy transfrom
    [SerializeField] float Upwardforce; //
    [SerializeField] int MeleeDamage;
    [SerializeField] float MeleeDelay;
    [SerializeField] float DashDistance;
    [SerializeField] float DashSpeed;
    [SerializeField] float TargetDistance;
    [SerializeField] float Reach;

    bool Collide;
    bool AttackRange;
    float AttackTimer;
    Vector3 PlayerPosition;
    Vector3 NewPushPosition;
    Vector3 Dir ;

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

                transform.position = Vector3.Lerp(transform.position, NewPushPosition, Time.deltaTime * DashSpeed);
                punch(Upwardforce, (GameManager.instance.player.transform.position - transform.position));
                
            }
            
        }
    }

    void punch(float Force, Vector3 Dir)
    {

        Dir = Dir.normalized;
        Vector3 TotalPunch = Dir * Force;
        

        if (Vector3.Distance(GameManager.instance.player.transform.position, transform.position) <= Reach) {
            GameManager.instance.playerScript.knockbacked = true;
            GameManager.instance.playerScript.applyPushback(TotalPunch);
            GameManager.instance.playerScript.takeDamage(MeleeDamage);
            AttackTimer = 0;
        }
        
    }


    void dashattack()
    {
        RaycastHit Detected;
        Debug.DrawRay(transform.position, PlayerPosition, color: Color.blue);
        if(Physics.Raycast(transform.position, (PlayerPosition).normalized, out Detected, TargetDistance))
        {
            if (Detected.collider.CompareTag("Player"))
            {
                Dir = transform.forward;
                NewPushPosition = new Vector3 (GameManager.instance.player.transform.position.x, transform.position.y, GameManager.instance.player.transform.position.z);
            }
            
        }
    }
}
