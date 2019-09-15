using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSystem : MonoBehaviour {

    //Main Object collision data
    private Renderer render;
    private Vector3 objectBoundMax;
    private Vector3 objectBoundMin;
    private const float OFFSET = 0.5f;
    private Vector3 OFFSET_VECTOR;

    //Relationship data
    private bool withinXBound;
    private bool withinYBound;

    //Collider Data
    private string colliderTag;
    private Vector3 colliderMaxBounds;
    private Vector3 colliderMinBounds;

    //Behavior enablers
    public bool grounded;
    public bool leftWalled;
    public bool rightWalled;
    public bool hitEnemy;

    //Editable Enemy Tag
    public bool isPlayer;
    private string enemyTag;
    private string enemyAttackTag;

    //Collider Dictionary
    private Dictionary<string, Collider> touching;

	// Use this for initialization
	void Start () {
        render = GetComponent<Renderer>();
        OFFSET_VECTOR = new Vector3(OFFSET, OFFSET);
        touching = new Dictionary<string, Collider>();

        if (isPlayer) {
            enemyTag = "Enemy";
            enemyAttackTag = "EnemyAttack";
        } else {
            enemyTag = "Player";
            enemyAttackTag = "PlayerAttack";
        }
	}

    //Updates by checking if still touching with colliders in "touching" dictionary
    void Update() {
        //Updates offset bound every frame
        Vector3 offsetExtent = render.bounds.extents + OFFSET_VECTOR;
        Bounds touchBound = new Bounds(render.bounds.center, 2 * offsetExtent);

        //Get all keys in a list
        List<string> keyList = new List<string>(touching.Keys);

        //Checks the status of touching colliders. If collider destroyed or not touching anymore, remove it
        foreach(string key in keyList) {
            Collider touchedObj = touching[key];

            if (touchedObj == null || (touchedObj.tag != "Platform" && touchedObj.tag != "PausedProjectile") || !touchedObj.bounds.Intersects(touchBound))
                touching.Remove(key);
        }

        //Updates condition variables as needed depending if its touching assigned obj
        leftWalled = touching.ContainsKey("leftWall");
        rightWalled = touching.ContainsKey("rightWall");
            
    }


    //checks enter collision
    void OnCollisionEnter(Collision collision){
        setColliderData(collision);

        //If object is on a wall (platform) (withinYBound && !withinXBound) and object is right of collider, leftWalled = true
        if (withinYBound && !withinXBound && objectBoundMax.x > colliderMaxBounds.x && (colliderTag == "Platform" || colliderTag == "PausedProjectile") && !leftWalled)
            touching.Add("leftWall", collision.collider);

        //If object is on a wall (platform) and object is left of collider, rightWalled = true
        if (withinYBound && !withinXBound && objectBoundMin.x < colliderMinBounds.x && (colliderTag == "Platform" || colliderTag == "PausedProjectile") && !rightWalled)
            touching.Add("rightWall", collision.collider);

        //If hit opposing force, hitEnemy true, else false
        if (colliderTag == enemyTag || colliderTag == enemyAttackTag)
            hitEnemy = true;
    }

    //Checks exit collision
    void OnCollisionExit(Collision collision){
        setColliderData(collision);

        //If player moves away from enemy, hitEnemy false.
        if (colliderTag.Equals(enemyTag))
            hitEnemy = false;
    }

    //Sets all collision data after every collision
    private void setColliderData(Collision collision) {
        Collider collider = collision.collider;

        //Set Collider Variables
        colliderTag = collider.tag;
        colliderMaxBounds = collider.bounds.max;
        colliderMinBounds = collider.bounds.min;

        //Set Main Object variables
        objectBoundMax = render.bounds.max;
        objectBoundMin = render.bounds.min;

        //Set relationship Variables: withinXBound
        withinXBound = colliderMaxBounds.x > objectBoundMin.x + OFFSET && colliderMinBounds.x < objectBoundMax.x - OFFSET;
        withinYBound = colliderMaxBounds.y > objectBoundMin.y + OFFSET && colliderMinBounds.y < objectBoundMax.y - OFFSET;

    }
}
