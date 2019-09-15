using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDoorActivation : MonoBehaviour {

    private const float DOOR_OPEN_DELAY = 0.5f;
    public AudioSource unlocking;

    public IEnumerator linksBroken() {
        unlocking.Play();
        yield return new WaitForSeconds(DOOR_OPEN_DELAY);
        gameObject.SetActive(false);
    }
}
