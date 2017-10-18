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

	GameObject theBall;
	float throwingForce = 50f;

	bool throwing = false;


    // Use this for initializations
    void Start()
    {
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

    void transitionToStateA()
    {

        print("Transition to state A");

        state = State.A;

        aiSteer.setWaypoints(waypointSetA);

        aiSteer.useNavMeshPathPlanning = true;


    }


//

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

    void transitionToStateF(GameObject target)
    {
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
		print ("Transition to staste G: Throwing");
		throwing = true;
		Vector3 shootPos = theBall.transform.position;
		Vector3 targetPos = target.transform.position;
		Vector3 shooterVel = Vector3.zero;
		Vector3 targetVel = target.GetComponent<Rigidbody>() ? target.GetComponent<Rigidbody>().velocity : Vector3.zero;
		throwingDirection = firstOrderIntercept (shootPos, shooterVel, throwingForce, targetPos, targetVel);
		throwAball(throwingDirection.normalized);
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
                if (aiSteer.waypointsComplete())
                    transitionToStateD();
//					target = findClosestEnemy();
//					transitionToStateG(target);
                break;
            case State.D:
                if (Time.timeSinceLevelLoad - beginWaitTime > waitTime)
                    transitionToStateE();
                break;

			case State.E:
				if (aiSteer.waypointsComplete()) {
					target = findClosestEnemy();
//					player.GetComponent<Rigidbody> ().velocity = player.GetComponent<Rigidbody> ().velocity * 2f;
					transitionToStateF (target);
				}
                break;
		case State.F:
			if (aiSteer.waypointsComplete())
				target = findClosestEnemy();
				transitionToStateG(target);
//				if (targetHit) {
//	//							player.GetComponent<Rigidbody> ().velocity = player.GetComponent<Rigidbody> ().velocity * 0.5f;
//					
//					transitionToStateG (target);
//					targetHit = false;	
//				} else {
//				transitionToStateF (target);
//				}
				break;
			case State.G:
				//If target hit
				
				transitionToStateA();
				break;
            default:

                print("Weird?");
                break;
        }
    }
		

	void throwAball(Vector3 direction)
	{
		theBall.transform.SetParent(null);
		theBall.GetComponent<Rigidbody> ().isKinematic = false;
		theBall.GetComponent<Rigidbody> ().useGravity = true;
		theBall.GetComponent<Collider> ().enabled = true;
		theBall.GetComponent<Rigidbody>().AddForce(direction * throwingForce, ForceMode.Impulse);
//		theBall.GetComponent<Rigidbody>().useGravity = true;
//		theBall.GetComponent<Rigidbody>().AddForce(direction * throwingForce, ForceMode.Impulse);
		throwing = true;
	}

	void endThrowing(){
		throwing = false;
	}
}
