using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimeEffected {

    //Applies slow based on time factor. Can be reversed by inserting time factor's reciprocoal
    //  If less than 1, slow
    //  If more than 1, return to normal
    void applySlow(float timeFactor);

    //Applies pause for a set duration in seconds. For enemies, duration can be canceled if attacked by player
    //This method is split to 3 parts: pause, wait duration, and unpause. Also, make sure to include 
    //2 materials: defaultMat and paused
    IEnumerator applyPause(float pauseDuration);
}
