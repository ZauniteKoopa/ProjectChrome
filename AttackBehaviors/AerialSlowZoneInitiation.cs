using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialSlowZoneInitiation : MonoBehaviour
{
    private const float TAIL_KNOCKBACK = 675f;
    private const float RECOIL_DURATION = 0.35f;
    public Transform slowZone;
    private bool triggered;

    //Update checks when player is grounded. If so, initiate slow zone on player
    void Update(){
        if(transform.parent.GetComponent<CollisionSystem>().grounded && !triggered) {
            Instantiate(slowZone, transform.parent);
            transform.parent.GetComponent<PlayerStatus>().enableAttack = true;
            transform.parent.GetComponent<Rigidbody>().AddForce(Vector3.up * TAIL_KNOCKBACK);
            Object.Destroy(gameObject);
        }
    }

    //When triggered, create slow zone
    void OnTriggerEnter(Collider other) {
        if((other.tag == "Enemy" || other.tag == "EnemyAttack") && !triggered) {
            Instantiate(slowZone, other.ClosestPoint(GetComponent<Transform>().position), Quaternion.identity, transform.parent);

            //Make the initiation circle disappear and disable this script before recoiling
            GetComponent<MeshRenderer>().enabled = false;
            triggered = true;
            transform.parent.GetComponent<PlayerStatus>().enableAttack = true;
            StartCoroutine(recoil(other));
        }
    }


    //Have player recoil when initiated
    IEnumerator recoil(Collider initiator) {
        //Create knockback
        Vector3 knockback = SystemCalc.findDirection(initiator.bounds.center, GetComponent<Transform>().position);
        knockback *= TAIL_KNOCKBACK;

        //Apply knockback
        Rigidbody parentRB = transform.parent.GetComponent<Rigidbody>();
        parentRB.velocity = Vector3.zero;
        parentRB.AddForce(knockback);

        yield return new WaitForSeconds(RECOIL_DURATION);

        //Cancel knockback and destroy gameObject
        if(parentRB.velocity.y >= 0)
            parentRB.velocity = Vector3.zero;
        else
            parentRB.velocity = new Vector3(0, parentRB.velocity.y, 0);

        Object.Destroy(gameObject);
    }
}
