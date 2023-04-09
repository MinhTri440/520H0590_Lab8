using UnityEngine;
using System.Collections;

/// <summary>
/// Place the labels for the Transitions in this enum.
/// Don't change the first label, NullTransition as FSMSystem class uses it.
/// </summary>
public enum Transition
{
	NullTransition = 0, // Use this transition to represent a non-existing transition in your system
	SawPlayer = 1,
	LostPlayer = 2,
}

/// <summary>
/// Place the labels for the States in this enum.
/// Don't change the first label, NullTransition as FSMSystem class uses it.
/// </summary>
public enum StateID
{
	NullStateID = 0, // Use this ID to represent a non-existing State in your system
	PatrollingID = 1,
	ChasingPlayerID = 2,
}

public class EnemyBehaviour : MonoBehaviour {

	public FSMSystem fsm;
	public GameObject player;
	public Transform[] wp;
	
	public float fieldOfViewAngle = 55f;
	public float sightRange = 200f;

	// Use this for initialization
	void Start () {
		MakeFSM();
	}

    private void MakeFSM()
    {
        //Create two states (PatrolState and ChasePlayerState), using the constructors defined below.
        //Add one transition to each state (so each state can transit to the other), using the function FSMState.AddTransition
        //Add both states to the FSM at the end of this method

        PatrolState patrol = new PatrolState(gameObject, wp);
        ChasePlayerState chase = new ChasePlayerState(gameObject, player);

        patrol.AddTransition(Transition.SawPlayer, StateID.ChasingPlayerID);
        chase.AddTransition(Transition.LostPlayer, StateID.PatrollingID);

        fsm = new FSMSystem();
        fsm.AddState(patrol);
        fsm.AddState(chase);
    }

    public void SetTransition(Transition t) { fsm.PerformTransition(t); }

    // Update is called once per frame
    void Update()
    {
        fsm.CurrentState.Reason(player, gameObject);
        fsm.CurrentState.Act(player, gameObject);
    }


    public bool PlayerInSight(GameObject player, GameObject npc)
    {
        Vector3 toPlayer = player.transform.position - npc.transform.position;
        float angle = Vector3.Angle(npc.transform.forward, toPlayer);

        //Check if the player is in sight. Consider that:
        //He can only be in sight if angle is less than the field of view.
        //You should use Raycasting to determine if obstacles are blocking the view of the player.

        if (angle <= fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(npc.transform.position, toPlayer.normalized, out hit, sightRange))
            {
                if (hit.collider.gameObject == player)
                {
                    return true;
                }
            }
        }

        return false;
    }
}


public class PatrolState : FSMState
{
	private int currentWayPoint;
	private Transform[] waypoints;
	private EnemyAnimation enemyAnimation;
	private float patrolSpeed = 2.5f;

    public PatrolState(GameObject thisObject, Transform[] wp)
    {
        waypoints = wp;
        currentWayPoint = 0;
        stateID = StateID.PatrollingID;
        enemyAnimation = thisObject.GetComponent<EnemyAnimation>();
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        //Check line of sight.
        if (npc.GetComponent<EnemyBehaviour>().PlayerInSight(player, npc))
        {
            //Make a transition using Transition.SawPlayer.
            npc.GetComponent<EnemyBehaviour>().SetTransition(Transition.SawPlayer);
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        //Program the Patrol State Act. It should update the currentWayPoint to the next one,
        //in case the current one is reached by the agent.
        if (Vector3.Distance(npc.transform.position, waypoints[currentWayPoint].position) < 1f)
        {
            currentWayPoint = (currentWayPoint + 1) % waypoints.Length;
        }

        //Update the target.
        enemyAnimation.setTarget(waypoints[currentWayPoint], patrolSpeed);
    }

}


public class ChasePlayerState : FSMState
{
    private EnemyAnimation enemyAnimation;
    private float chaseSpeed = 4f;
    private float stopDist = 2f;

    public ChasePlayerState(GameObject thisObject, GameObject tgt)
    {
        stateID = StateID.ChasingPlayerID;
        enemyAnimation = thisObject.GetComponent<EnemyAnimation>();
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        // Check line of sight.
        if (!npc.GetComponent<EnemyBehaviour>().PlayerInSight(player, npc))
        {
            // Transition to LostPlayer state if player is not in sight.
            npc.GetComponent<FSMSystem>().PerformTransition(Transition.LostPlayer);
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        // Chase the player's position until being at a distance less than 'stopDist'.
        float distance = Vector3.Distance(npc.transform.position, player.transform.position);
        if (distance > stopDist)
        {
            npc.transform.LookAt(player.transform);
            npc.transform.position += npc.transform.forward * chaseSpeed * Time.deltaTime;
        }
        else
        {
            enemyAnimation.Stop();
        }
    }
}




