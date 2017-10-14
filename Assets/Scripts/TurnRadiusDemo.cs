using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurnRadiusDemo : MonoBehaviour {

    public float angularVelocityDegSec = 10f;
    public float velocity = 2f;

    bool firstTime = true;

    public float avgWeight = 10f;

    public Vector3 hackAverageCenter;

    public float diffFromCalcCenters;

    public GameObject checkIfInsideTurnRadius;

    public bool isInside = false;

    public float radius;

    public float distFromObj;

    public Vector2 prjCenter;
    public Vector2 prjTestObj;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 oldPos = transform.position;
        Vector3 oldRight = transform.right;

        transform.rotation *= Quaternion.Euler(0f, angularVelocityDegSec * Time.deltaTime, 0f);

        transform.position += transform.forward * velocity * Time.deltaTime;

        //w = s/rt
        //r = s/wt

        float s = (transform.position - oldPos).magnitude;
        float w = angularVelocityDegSec * Mathf.Deg2Rad;

        radius = s / (w * Time.deltaTime);

        float sign = Mathf.Sign(angularVelocityDegSec);

        Vector3 centerOfTurn = oldPos + sign * oldRight * radius;

        Debug.DrawLine(centerOfTurn, centerOfTurn + Vector3.up * 40f, Color.green);

        if (firstTime)
        {
            firstTime = false;

            hackAverageCenter = centerOfTurn;
        }

        hackAverageCenter = Vector3.Lerp(hackAverageCenter, centerOfTurn, Mathf.Min(1f, avgWeight * Time.deltaTime));
        diffFromCalcCenters = (centerOfTurn - hackAverageCenter).magnitude;

        if (checkIfInsideTurnRadius != null)
        {
            isInside = testAgainstCircle(centerOfTurn, radius, Vector3.up, checkIfInsideTurnRadius.transform.position);

            Color c = Color.red;

            if (isInside)
                c = Color.green;

            Debug.DrawLine(checkIfInsideTurnRadius.transform.position, checkIfInsideTurnRadius.transform.position + Vector3.up * 40f, c); 

        }

	}


    bool testAgainstCircle(Vector3 circleCenter, float circleRadius, Vector3 circlePlaneNormal, Vector3 testPoint) 
    {

        Vector3 circleCenterPrj = Vector3.ProjectOnPlane(circleCenter, circlePlaneNormal);
        Vector3 testPointPrj = Vector3.ProjectOnPlane(testPoint, circlePlaneNormal);
        Vector2 circleCenter2D = new Vector2(circleCenterPrj.x, circleCenterPrj.z);
        Vector2 testPoint2D = new Vector2(testPointPrj.x, testPointPrj.z);

        float dist = (testPoint2D - circleCenter2D).magnitude;

        distFromObj = dist;
        prjCenter = circleCenter2D;
        prjTestObj = testPoint2D;

        if (dist < circleRadius)
            return true;

        return false;

    }

}
