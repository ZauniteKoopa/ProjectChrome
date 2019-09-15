using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStatus : MonoBehaviour, ITimeEffected {

    //Time variables
    private float curTimeState;
    private bool paused;

    //Damage that can be modified
    public float damage;

    //Material
    public Material defaultMat;
    public Material pausedMat;

    //Layers to go back to
    private const int ACTIVE_LAYER = 8;
    private const int PAUSED_LAYER = 0;

    //Reference to source if any
    public Transform source;

	// Use this for initialization
	void Start () {
        curTimeState = 1.0f;
        paused = false;
	}


    //Applies slow if timeFactor < 1 or reverses slow if timeFactor >=1
    public void applySlow(float timeFactor) {
        curTimeState *= timeFactor;
    }

    private const int PAUSE_FRAMES = 20;

    //Applies pause and unpause
    public IEnumerator applyPause(float pauseDuration) {
        //Set-up for frame checking
        paused = true;
        float frameDuration = pauseDuration / PAUSE_FRAMES;
        Material prevMat = GetComponent<MeshRenderer>().material;

        //Pausing
        gameObject.tag = "PausedProjectile";
        GetComponent<MeshRenderer>().material = pausedMat;
        gameObject.layer = PAUSED_LAYER;

        //Will keep pausing until the end of the duration
        for (int curFrame = 0; curFrame < PAUSE_FRAMES; curFrame++) {
            yield return new WaitForSeconds(frameDuration);

            //When reaching the last third of the frames, begin switching between materials
            if (curFrame >= (PAUSE_FRAMES * 2) / 3)
                GetComponent<MeshRenderer>().material = (curFrame % 2 == 0) ? prevMat : pausedMat;
        }

        //Unpausing
        gameObject.tag = "EnemyAttack";
        GetComponent<MeshRenderer>().material = prevMat;
        gameObject.layer = ACTIVE_LAYER;
        paused = false;
    }

    //Accessor method for curTimeState for movement scripts within gameObject
    public float getTimeState() {
        return curTimeState;
    }

    //Mutator method for getTimeState
    public void resetTimeState() {
        curTimeState = 1.0f;
    }

    //Accessor method for paused
    public bool unpaused() {
        return !paused;
    }
}
