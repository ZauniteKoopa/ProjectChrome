using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Class that contains all directed inward edges of a certain gameObject
public class CentralNode : MonoBehaviour {

    private HashSet<Transform> dependentLinks;
	
	// Accessor Method for In-Degree
    public int getInDegree() {
        return (dependentLinks == null) ? 0 : dependentLinks.Count;
    }

    //Adds links to the central node
    public void addLink(Transform newNode) {
        //Check if list created yet, if not, create it. (Avoids links being made before node made)
        if (dependentLinks == null)
            dependentLinks = new HashSet<Transform>();

        dependentLinks.Add(newNode);
    }

    //Removes link from central node. If isolated, alert other scripts in gameObject
    public void breakLink(Transform cutNode) {
        dependentLinks.Remove(cutNode);

        //If central node is isolated, send message that links are all broken
        if (getInDegree() == 0)
            gameObject.SendMessage("linksBroken");
    }
}
