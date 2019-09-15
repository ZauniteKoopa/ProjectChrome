using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopwatch : MonoBehaviour {

    private float currentTime;
    private bool recording;

    //Default Constructor
    public Stopwatch() {
        recording = false;
        currentTime = 0f;
    }

    //Set Start constructor
    public Stopwatch(float startTime) {
        currentTime = startTime;
    }

    // Use this for initialization
    void Start () {
        currentTime = 0f;
        recording = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (recording == true)
            currentTime += 1 * Time.deltaTime;
	}

    //Starts recording
    public void start()
    {
        recording = true;
    }

    //Stops recording
    public void stop()
    {
        recording = false;
    }

    //Returns time
    public float getTime()
    {
        return currentTime;
    }

    //Sets time
    public void setTime(float newTime)
    {
        currentTime = newTime;
    }

    //Resets stopwatch
    public void reset()
    {
        currentTime = 0f;
    }

    //Stops stopwatch and then resets
    public void hardReset()
    {
        stop();
        reset();
    }

    //Returns whether or not the stopwatch is active
    public bool isRunning() {
        return recording;
    }
}
