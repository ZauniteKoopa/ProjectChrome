using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePackage {

    private float damage;
    private Vector3 knockback;

    //Damage Package Constructor for 2 set values
    public DamagePackage(float damageValue, Vector3 knockbackValue) {
        damage = damageValue;
        knockback = knockbackValue;
    }

    //Damage constructor for only damage
    public DamagePackage(float damageValue) {
        damage = damageValue;
        knockback = Vector3.zero;
    }

    //Acessor method for damage
    public float getDamage() {
        return damage;
    }

    //Acessor method for knockback
    public Vector3 getKnockback() {
        return knockback;
    }

    //Mutator method for damage
    public void setDamage(float newDmg){
        damage = newDmg;
    }


    //Sets centralized knockback based on central position of attacker / projectile and central position of player
    //  Post: Sets knockback to a Vector3 that represents the direction from attacker to victim
    public void setCentralizedKnockback(Vector3 attackerPos, Vector3 victimPos, float knockbackValue) {
        Vector3 setKnockback = SystemCalc.findDirection(attackerPos, victimPos);
        knockback = setKnockback * knockbackValue;
    }

    //Sets orientationalKnockback based on the "orientation" of the weapon used (Mostly used for melee attacks)
    //  Post: Sets this knockback to a vector3 based on melee hitbox rotation
    public void setOrientationalKnockback(float rotation, float knockbackValue) {
        if (rotation == 0)
            knockback = Vector3.right * knockbackValue;
        else if (rotation == 90)
            knockback = Vector3.up * knockbackValue;
        else if (rotation == 180)
            knockback = Vector3.left * knockbackValue;
        else if (rotation == 270)
            knockback = Vector3.zero;
    }
}
