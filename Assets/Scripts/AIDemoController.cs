using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(AINavSteeringController))]
//Added:
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class AIDemoController : MonoBehaviour
{


    public Transform[] waypointSetA;

	
    public Transform waypointE;

    public Vector3 movingDirection;
	public Vector3 throwingDirection;

    public enum State
    {

        A,
        D,
        E,
        F,
		G
    }



    public State state = State.A;

    public float waitTime = 5f;

    protected float beginWaitTime;


    AINavSteeringController aiSteer;
    NavMeshAgent agent;
	//Added:
	bool targetHit = false;
	Animator anim;
	GameObject player;
	GameObject target;
	GameObject[] possibleTargets;
	GameObject leftWrist;
	GameObject theBall;
	float throwingForce = 10f;

	bool throwing = false;

	string[] states;
	string currentState = "A";

    // Use this for initializations
    void Start()
    {
		leftWrist = GameObject.FindGameObjectWithTag ("leftWrist");
		theBall = GameObject.FindGameObjectWithTag ("ball");
		player = GameObject.FindGameObjectWithTag ("player");
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
		states = new string[5];
		states[0] = "A";
		states[1] = "D";
		states[2] = "E";
		states[3] = "F";
		states[4] = "G";
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
		if (theCollision.gameObject == target){
			Debug.Log ("target hit!!!!");
			targetHit = true;
		}
	}

	void randomlyChooseAState()
	{
		endThrowing ();
		int index = Random.Range(0, 4);
		while (currentState == states [index]) {
			index = Random.Range(0, 4);
		}
		currentState = states [index];
		detectState();
	}

	void detectState()
	{
		if (currentState == "A") {
		
			transitionToStateA ();
		}
		if (currentState == "D") {
		
			transitionToStateD();
		}
		if (currentState == "E") {
			transitionToStateE ();
		}
		if (currentState == "F")
		{
			target = findClosestEnemy();
			transitionToStateF(target);
		}
		if (currentState == "G") {
			target = findClosestEnemy();
			transitionToStateG(target);}
	}

    void transitionToStateA()
    {
		textManager.state = "A";
        print("Transition to state A");

        state = State.A;

        aiSteer.setWaypoints(waypointSetA);

        aiSteer.useNavMeshPathPlanning = true;
		theBall.transform.SetParent (leftWrist.transform);
		Debug.Log ("Balls's grand parent: " + theBall.transform.parent.name);
		theBall.GetComponent<Rigidbody> ().isKinematic = true;
		theBall.GetComponent<Rigidbody> ().useGravity = false;
		theBall.GetComponent<Collider> ().enabled = false;



    }
		


    void transitionToStateD()
    {
		textManager.state = "D";
        print("Transition to state D");

        state = State.D;

        beginWaitTime = Time.timeSinceLevelLoad;

        aiSteer.clearWaypoints();

        aiSteer.useNavMeshPathPlanning = true;

    }


    void transitionToStateE()
    {
		textManager.state = "E";
        print("Transition to state E");

        state = State.E;
	
        aiSteer.setWaypoint(waypointE);

        aiSteer.useNavMeshPathPlanning = true;

    }

    void transitionToStateF(GameObject target)
    {
		textManager.state = "F";
        print("Transition to state F : Chasing enemy");
        state = State.F;
		Vector3 shootPos = player.transform.position;
		Vector3 targetPos = target.transform.position;
		Vector3 shooterVel = player.GetComponent<Rigidbody>() ? player.GetComponent<Rigidbody>().velocity : Vector3.zero;
		Vector3 targetVel = target.GetComponent<Rigidbody>() ? target.GetComponent<Rigidbody>().velocity : Vector3.zero;
		movingDirection = firstOrderIntercept (shootPos, shooterVel, shooterVel.magnitude, targetPos, targetVel);
		aiSteer.setWaypoint(movingDirection);
		aiSteer.useNavMeshPathPlanning = true;
    }

	//Add a Throwing State:
	void transitionToStateG(GameObject target)
	{
		textManager.state = "G";
		print ("Transition to staste G: Throwing");
		throwing = true;
		Vector3 shootPos = theBall.transform.position;
		Vector3 targetPos = target.transform.position;
		Vector3 shooterVel = Vector3.zero;
		Vector3 targetVel = target.GetComponent<Rigidbody>() ? target.GetComponent<Rigidbody>().velocity : Vector3.zero;
		throwingDirection = firstOrderIntercept (shootPos, shooterVel, throwingForce, targetPos, targetVel);
		state = State.G;
		anim.SetTrigger("Throw");
	}
		

	Vector3 firstOrderIntercept(Vector3 shooterPosition,
		Vector3 shooterVelocity,
		float shotSpeed,
		Vector3 targetPosition,
		Vector3 targetVelocity)
	{
		Vector3 targetRelativePosition = targetPosition - shooterPosition;
		Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
		float t = firstOrderInterceptTime(shotSpeed, targetRelativePosition, targetRelativeVelocity);
		return targetPosition + t * (targetRelativeVelocity);
	}

	float firstOrderInterceptTime (float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity)
	{
		float velocitySquared = targetRelativeVelocity.sqrMagnitude;
		if (velocitySquared < 0.001f) {
			return 0f;
		}
		float a = velocitySquared - shotSpeed * shotSpeed;
		if (Mathf.Abs (a) < 0.001f) {
			float t = -targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot
				(
				          targetRelativeVelocity,
				          targetRelativePosition
			          ));
			return Mathf.Max (t, 0f);
		}
		float b = 2f * Vector3.Dot (targetRelativeVelocity, targetRelativePosition);
		float c = targetRelativePosition.sqrMagnitude;
		float determinant = b * b - 4f * a * c;
		if (determinant > 0f) {
			float t1 = (-b + Mathf.Sqrt (determinant)) / (2f * a);
			float t2 = (-b - Mathf.Sqrt (determinant)) / (2f * a);
			if (t1 > 0f) {
				if (t2 > 0f) {
					return Mathf.Min (t1, t2);
				} else {
					return t1;
				}
			} else {
				return Mathf.Max (t2, 0f);
			}
		} else if (determinant < 0f) {
			return 0f;
		} else {
			return Mathf.Max (-b / (2f * a), 0f);
		}
	}

    // Update is called once per frame
    void Update()
	{
        switch (state)
        {
		case State.A:
//			Debug.Log("")
			if (aiSteer.waypointsComplete ())
					randomlyChooseAState ();
                break;
		case State.D:
			if (Time.timeSinceLevelLoad - beginWaitTime > waitTime)
				endThrowing ();
				randomlyChooseAState ();
                break;

			case State.E:
				if (aiSteer.waypointsComplete()) {
					
//					player.GetComponent<Rigidbody> ().velocity = player.GetComponent<Rigidbody> ().velocity * 2f;
				randomlyChooseAState();		
			}
                break;
		case State.F:
			if (aiSteer.waypointsComplete ())
				randomlyChooseAState ();
				if (targetHit) {
					player.GetComponent<Rigidbody> ().velocity = player.GetComponent<Rigidbody> ().velocity * 0.5f;
					transitionToStateG (target);
					targetHit = false;	
				} else {
					transitionToStateF (target);
				}
				break;
		case State.G:
				//If target hit
				
			transitionToStateD ();
			//endThrowing ();
				break;
            default:

                print("Weird?");
                break;
        }
    }

	void throwTheBall()
	{
		theBall.transform.SetParent(null);
		theBall.GetComponent<Rigidbody> ().isKinematic = false;
		theBall.GetComponent<Rigidbody> ().useGravity = true;
		theBall.GetComponent<Collider> ().enabled = true;
		theBall.GetComponent<Rigidbody>().AddForce(player.transform.forward * throwingForce, ForceMode.Impulse);
		throwing = true;
	}

	void endThrowing(){
		throwing = false;
		Debug.Log ("Position should be changed!!!!");
		theBall.transform.position = leftWrist.transform.position;
		theBall.transform.SetParent (leftWrist.transform);
		Debug.Log ("Balls's grand parent: " + theBall.transform.parent.name);
		theBall.GetComponent<Rigidbody> ().isKinematic = true;
		theBall.GetComponent<Rigidbody> ().useGravity = false;
		theBall.GetComponent<Collider> ().enabled = false;
	}
}
