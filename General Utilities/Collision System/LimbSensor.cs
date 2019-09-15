using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbSensor : MonoBehaviour
{
    private CollisionSystem collisionSys;
    private HashSet<Collider> touching;
    private HashSet<Collider> potentialPlatforms;

    // Start is called before the first frame update
    void Start(){
        collisionSys = transform.root.GetComponent<CollisionSystem>();
        potentialPlatforms = new HashSet<Collider>();
        touching = new HashSet<Collider>();
    }

    //Update checks if any element is destroyed. If so, remove the element from the set and set grounded if nothing is in the area
    void Update() {
        //Only checks if touching a viable platform: Removes if platform is no longer a platform or destroyed
        if(touching.Count > 0) {
            //Set up a clone to avoid iterator errors
            Collider[] touchingClone = new Collider[touching.Count];
            touching.CopyTo(touchingClone);

            foreach (Collider obj in touchingClone)
                if (obj == null || obj.tag == "EnemyAttack") {
                    touching.Remove(obj);

                    if (touching.Count == 0)
                        collisionSys.grounded = false;
                }
        }

        //Checks if projectile turned into a platform or destroyed
        if(potentialPlatforms.Count > 0) {
            //Set up clone
            Collider[] potPlatClone = new Collider[touching.Count];
            potentialPlatforms.CopyTo(potPlatClone);

            foreach(Collider obj in potPlatClone) {
                //If object destroyed, remove from list
                if (obj == null)
                    potentialPlatforms.Remove(obj);

                //If object turned into a platform, add to toching list
                if(obj.tag == "PausedProjectile") {
                    touching.Add(obj);
                    potentialPlatforms.Remove(obj);

                    if (collisionSys.grounded == false)
                        collisionSys.grounded = true;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        //Input for viable platforms
        if (other.tag == "Platform" || other.tag == "PausedProjectile") {
            if (touching.Count == 0)
                collisionSys.grounded = true;

            touching.Add(other);
        }

        //Input for potential platforms
        if (other.tag == "EnemyAttack")
            potentialPlatforms.Add(other);
    }

    void OnTriggerExit(Collider other) {
        //Removal of viable platforms when not touching anymore
        if (other.tag == "Platform" || other.tag == "PausedProjectile") {
            touching.Remove(other);

            if (touching.Count == 0)
                collisionSys.grounded = false;
        }

        //Removal of potential platforms when not touching anymore.
        if (other.tag == "EnemyAttack")
            potentialPlatforms.Remove(other);
    }
}
