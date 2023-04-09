using UnityEngine;
using System.Collections;

public class EnemyAnimation : MonoBehaviour
{

    public float deadZone = 5f;                     // The number of degrees for which the rotation isn't controlled by Mecanim.

    public float speedDampTime = 0.1f;              // Damping time for the Speed parameter.
    public float angularSpeedDampTime = 0.7f;       // Damping time for the AngularSpeed parameter
    public float angleResponseTime = 0.6f;          // Response time for turning an angle into angularSpeed.

    private UnityEngine.AI.NavMeshAgent nav;                   // Reference to the nav mesh agent.
    private Animator anim;                          // Reference to the Animator.

    public Transform target;                    // Destination of the agent.

    void Awake()
    {
        // Setting up the references.
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Making sure the rotation is controlled by Mecanim.
        nav.updateRotation = false;

        // Set the weights for the shooting and gun layers to 1.
        anim.SetLayerWeight(1, 1f);
        anim.SetLayerWeight(2, 1f);

        // We need to convert the angle for the deadzone from degrees to radians.
        deadZone *= Mathf.Deg2Rad;
    }


    void Update()
    {
        if (target != null)
        {
            // Set the destination of the NavMesh agent to the position of the target.
            nav.SetDestination(target.position);

            // Create the parameters to pass to the helper function.
            float speed = 0;
            float angularSpeed = 0;
            DetermineAnimParameters(out speed, out angularSpeed);

            //Set the values of the parameters to the animator.
            anim.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
            anim.SetFloat("AngularSpeed", angularSpeed, angularSpeedDampTime, Time.deltaTime);
        }
    }


    void OnAnimatorMove()
    {
        // Set the NavMeshAgent's velocity to the change in position since the last frame, by the time it took for the last frame.
        nav.velocity = anim.deltaPosition / Time.deltaTime;

        // The gameobject's rotation is driven by the animation's rotation.
        transform.rotation = anim.rootRotation;
    }


    void DetermineAnimParameters(out float speed, out float angularSpeed)
    {
        // Set the speed to the magnitude of the projection of nav.desiredVelocity on to the forward vector...
        speed = Vector3.Project(nav.desiredVelocity, transform.forward).magnitude;

        // Set the angle to the angle between forward and the desired velocity.
        float angle = FindAngle(transform.forward, nav.desiredVelocity, transform.up);

        // If the angle is within the deadZone...
        if (Mathf.Abs(angle) < deadZone)
        {
            // set the direction to be along the desired direction and set the angle to be zero.
            transform.LookAt(transform.position + nav.desiredVelocity);
            angle = 0f;
        }

        angularSpeed = angle / angleResponseTime;
    }


    float FindAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector)
    {
        // Return the angle, in radians, between fromVector and toVector;
        // NOTE that Vector3.Angle returns the ACUTE angle between the two vectors 
        //   (this is: the smaller of the two possible angles between them and never greater than 180 degrees)
        return Mathf.Atan2(Vector3.Dot(upVector, Vector3.Cross(fromVector, toVector)),
                      Vector3.Dot(fromVector, toVector));

    }

    public void setTarget(Transform target, float speed = 2.0f)
	{
		this.target = target;
		nav.speed = speed;
	}
	
	public void Stop()
	{
		nav.speed = 0f;
	}
}
