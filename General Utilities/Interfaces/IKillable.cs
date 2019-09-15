using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKillable {

    //Applies damage to this unit. If health hits 0, destroy object and any unique enemy behaviors if necessary
    void getDamage(DamagePackage dmgPackage);
}
