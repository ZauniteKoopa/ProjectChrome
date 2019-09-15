using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class SystemCalc {

    //Finds direction vector between 2 points
    //  Pre: Both start and end must be positions on the plane (2D ONLY)
    //  Post: Returns a Vector 3 where the direction headed is 1, X and Y changes accordingly
    public static Vector3 findDirection(Vector3 start, Vector3 end) {
        //Calculates the X-Distance and Y-Distance between points
        float a = end.x - start.x;
        float b = end.y - start.y;

        //Use Pythagorean's Theorem to calculate length of diagonal
        float c = (float)Math.Pow((double)a, 2) + (float)Math.Pow((double)b, 2);
        c = (float)Math.Sqrt((double)c);

        //Reduces diagonal to 1 in new Vector
        Vector3 result = new Vector3(a / c, b / c, 0);
        return result;
    }

    //Finds distance between 2 points using pythagorean's theorem
    public static float findDistance(Vector3 start, Vector3 end) {
        float xDist = start.x - end.x;
        float yDist = start.y - end.y;

        float fullDist = (float)Math.Pow((double)xDist, 2) + (float)Math.Pow((double)yDist, 2);
        fullDist = (float)Math.Sqrt((double)fullDist);

        return fullDist;
    }
}
