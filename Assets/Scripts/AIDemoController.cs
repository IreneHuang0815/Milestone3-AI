using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(AINavSteeringController))]
//Added:
[RequireComponent(typeof(Animator))]
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
        F,
		G,
		H
    }



    public State state = State.A;

    public float waitTime = 5f;

    protected float beginWaitTime;


    AINavSteeringController aiSteer;
    NavMeshAgent agent;
	//Added:
	Animator anim;
	GameObject target;
	GameObject[] possibleTargets;



    // Use this for initialization
    void Start()
    {

		possibleTargets = GameObject.FindGameObjectsWithTag ("target");
        aiSteer = GetComponent<AINavSteeringController>();

        agent = GetComponent<NavMeshAgent>();
		//Added
		anim = GetComponent<Animator>();

        Debug.Log("NavMesh:avoidancePredictionTime(default): " + NavMesh.avoidancePredictionTime);

        //NavMesh.avoidancePredictionTime = 4f;

        aiSteer.Init();

        aiSteer.waypointLoop = false;
        aiSteer.stopAtNextWaypoint = false;

        transitionToStateA();
		
    }

	GameObject randomlyChooseEnemy()
	{
		GameObject randomEnemy = possibleTargets [Random.Range (0, possibleTargets.Length)];
		return randomEnemy;
	}

	GameObject findClosestEnemy ()
	{
		GameObject closest = randomlyChooseEnemy();
		float distance = Mathf.Infinity;
		Vector3 position = transform.position;
		foreach(GameObject go in possibleTargets)
		{
			Vector3 diff = go.transform.position - position;
			float curDistance = diff.sqrMagnitude;
			if (curDistance < distance) {
				closest = go;
				distance = curDistance;
			}
		}
		return closest;
	}

	void OnCollisionEnter(Collision theCollision){
		if (theCollision.gameObject.tag == "target"){
		Debug.Log ("Hit something!");
		}
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

	//Add a Throwing State:
	void transitionToStateG()
	{
		print ("Transition to staste G: Throwing");
		state = State.G;
		anim.SetTrigger("Throw");
	}

	//Add a chasing State
	void transitionToStateH()
	{
		print ("Transition to State H : chasing enemy");
		state = State.H;
		target = findClosestEnemy();
		aiSteer.setWaypoint (target.transform);
		aiSteer.useNavMeshPathPlanning = true;
	}

    // Update is called once per frame
    void Update()
    {
		
        switch (state)
        {
            case State.A:
                
                if (aiSteer.waypointsComplete())
                    transitionToStateG();
                break;
			case State.G:
				//If target hit
				transitionToStateH();
				break;
			case State.H:
				//
				transitionToStateD ();
				break;
//            case State.B:
//                if (aiSteer.waypointsComplete())
//                    transitionToStateC();
//                break;
//
//            case State.C:
//                if (aiSteer.waypointsComplete())
//                    transitionToStateD();
//                break;

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
			//Throw State
			//Todo: 
			//Add a throwing state
			//Add chasing state: make a gameobject a waypoint
            default:

                print("Weird?");
                break;
        }

		 
    }
}
