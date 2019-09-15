using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxBehavior : MonoBehaviour
{
    //Reference variables
    private Rigidbody parentRB;
    private Transform hitbox;
    private float rotation;
    private PlayerStatus parentStatus;
    private bool facingRight;

    //Damage
    public float damage;
    private const string DAMAGE_TYPE = "Claw";

    //Duration of Slash
    private const float SLASH_FRAME = 0.2f;

    //List of hit enemies to ensure no hitting twice
    private HashSet<Collider> hit;

    //Aerial slash check. Can only jump once
    private bool slashJumped;

    // Use this for initialization
    void Start() {
        //Gain parent components
        parentRB = GetComponentInParent<Rigidbody>();
        parentStatus = GetComponentInParent<PlayerStatus>();
        facingRight = parentStatus.isFacingRight;

        //Get damage from parent maps
        float dmgValue;
        if (parentStatus.damageMap.TryGetValue(DAMAGE_TYPE, out dmgValue))
            damage = dmgValue;

        //Set up hitbox and variables
        hitbox = GetComponent<Transform>();
        rotation = hitbox.eulerAngles.z;
        hit = new HashSet<Collider>();
        Object.Destroy(gameObject, SLASH_FRAME);
    }

    private const float KNOCKBACK_SELF = 50f;
    private const float KNOCKBACK_SLASH_JUMP = 1300f;
    private const float KNOCKBACK_OTHER = 400f;

    //Applies damage to enemy through broadcast and then knockbacks players
    private void OnTriggerEnter(Collider other){
        //If player has not been hit by this slash already, hit it.
        if(!hit.Contains(other)){
            hit.Add(other);
            hitTarget(other);
        }

    }

    //Algorithm that allows htting a target
    private void hitTarget(Collider other) {
        //Creates small recoil when fighting enemies horizontally
        if (rotation == 0){
            if (facingRight)
                parentRB.AddForce(Vector3.left * KNOCKBACK_SELF);
            else{
                parentRB.AddForce(Vector3.right * KNOCKBACK_SELF);
                rotation = 180;
            }


        }else if (rotation == 270 && other.tag == "Enemy" && !slashJumped){       //Allows pogo claw jumping
            parentRB.velocity = Vector3.zero;
            parentRB.AddForce(Vector3.up * KNOCKBACK_SLASH_JUMP);
            slashJumped = true;
        }else if (rotation == 90 && (other.tag == "Enemy" || other.tag == "EnemyAttack")){
            parentRB.velocity = Vector3.zero;
            parentRB.AddForce(Vector3.down * KNOCKBACK_SELF);
        }

        //Create damagePackage to send to enemy hit
        DamagePackage dmgPackage = new DamagePackage(damage);           //Creates damage package to broadcast
        dmgPackage.setOrientationalKnockback(rotation, KNOCKBACK_OTHER);

        if (other.tag == "Enemy" || other.tag == "EnemyAttack")
            other.SendMessage("getDamage", dmgPackage);
    }
}
