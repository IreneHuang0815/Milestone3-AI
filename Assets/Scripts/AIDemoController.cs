using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(AINavSteeringController))]
[RequireComponent(typeof(NavMeshAgent))]
public class AIDemoController : MonoBehaviour
{


    public Transform[] waypointSetA;

    public Transform[] waypointSetB;

    public Transform[] waypointSetC;

    public Transform waypointE;

    public Vector3 waypointF;

    public enum State
    {

        A,
        B,
        C,
        D,
        E,
        F

    }



    public State state = State.A;

    public float waitTime = 5f;

    protected float beginWaitTime;


    AINavSteeringController aiSteer;
    NavMeshAgent agent;


    // Use this for initialization
    void Start()
    {

        aiSteer = GetComponent<AINavSteeringController>();

        agent = GetComponent<NavMeshAgent>();

        Debug.Log("NavMesh:avoidancePredictionTime(default): " + NavMesh.avoidancePredictionTime);

        //NavMesh.avoidancePredictionTime = 4f;

        aiSteer.Init();

        aiSteer.waypointLoop = false;
        aiSteer.stopAtNextWaypoint = false;

        transitionToStateA();
		
    }


    void transitionToStateA()
    {

        print("Transition to state A");

        state = State.A;

        aiSteer.setWaypoints(waypointSetA);

        aiSteer.useNavMeshPathPlanning = true;


    }


    void transitionToStateB()
    {

        print("Transition to state B");

        state = State.B;

        aiSteer.setWaypoints(waypointSetB);

        aiSteer.useNavMeshPathPlanning = true;
    }


    void transitionToStateC()
    {

        print("Transition to state C");

        state = State.C;

        aiSteer.setWaypoints(waypointSetC);

        aiSteer.useNavMeshPathPlanning = false;

    }

    void transitionToStateD()
    {

        print("Transition to state D");

        state = State.D;

        beginWaitTime = Time.timeSinceLevelLoad;

        aiSteer.clearWaypoints();

        aiSteer.useNavMeshPathPlanning = true;

    }


    void transitionToStateE()
    {

        print("Transition to state E");

        state = State.E;
	
        aiSteer.setWaypoint(waypointE);

        aiSteer.useNavMeshPathPlanning = true;

    }

    void transitionToStateF()
    {

        print("Transition to state F");

        state = State.F;

        aiSteer.setWaypoint(waypointF);

        aiSteer.useNavMeshPathPlanning = true;

    }

    // Update is called once per frame
    void Update()
    {
		
        switch (state)
        {
            case State.A:
                
                if (aiSteer.waypointsComplete())
                    transitionToStateB();
                break;

            case State.B:
                if (aiSteer.waypointsComplete())
                    transitionToStateC();
                break;

            case State.C:
                if (aiSteer.waypointsComplete())
                    transitionToStateD();
                break;

            case State.D:
                if (Time.timeSinceLevelLoad - beginWaitTime > waitTime)
                    transitionToStateE();
                break;

            case State.E:
                if (aiSteer.waypointsComplete())
                    transitionToStateF();
                break;

            case State.F:
                if (aiSteer.waypointsComplete())
                    transitionToStateA();
                break;

            default:

                print("Weird?");
                break;
        }


    }
}
