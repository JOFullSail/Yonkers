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
        
        Vector3 PlayerPosition = GameManager.instance.player.transform.position;
        RaycastHit Dectected;
        if(Physics.Raycast(EnemyView.position, (PlayerPosition - EnemyView.position).normalized, out Dectected, DashDistance))
        {

            if (Dectected.collider.CompareTag("Player"))
            {
                PlayerinAttackR = true;
                Debug.Log("Player Dectected");
                transform.position = Vector3.MoveTowards(transform.position, (PlayerPosition - EnemyView.position).normalized, Time.deltaTime * DashSpeed);
            }
            else
            {
                PlayerinAttackR = false;
            }
        }
        
    }


}
