using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NCPlayer : NetworkBehaviour
{
    public Rigidbody rd;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float turnSensitivity = 5f;
    public float turnSpeed = 30f;

    [Header("Diagnostics")]
    public float horizontal;
    public float vertical;
    public float jump;
    public float angularDeltaView;
    public float jumpSpeed;
    public bool isGrounded = true;
    public bool isFalling;
    public Vector3 velocity;

    public override void OnStartClient()
    {
        rd.isKinematic = true;
    }

    enum EServerPhysicsMethods
    {
        Rigidbody,
        Kinematic,
        PhysicsOff,
    };

    public override void OnStartServer()
    {
        EServerPhysicsMethods method = EServerPhysicsMethods.Rigidbody;

        switch( method )
        {
            case EServerPhysicsMethods.Rigidbody:
                rd.isKinematic = false;
                break;

            case EServerPhysicsMethods.Kinematic:
                rd.isKinematic = true;
                break;

            case EServerPhysicsMethods.PhysicsOff:
                Destroy(rd);
                break;

            default:
                break;
        }
    }
    
    void Update()
    {
        if (!isLocalPlayer)
            return;

        Camera.main.orthographic = false;
        Camera.main.transform.position = transform.position + new Vector3(0f, 2f, -5f);
        Camera.main.transform.LookAt(transform);

        if (NetworkClient.active == false)
            return;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Space))
        {
            jump = 1.0f;
        }
        else
        {
            jump = 0.0f;
        }

        float angularDelta = 0f;
        // Q and E cancel each other out, reducing the turn to zero
        if (Input.GetKey(KeyCode.Q))
            angularDelta = -turnSpeed;
        if (Input.GetKey(KeyCode.E))
            angularDelta = turnSpeed;
        if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
            angularDelta = 0f;
        angularDeltaView = angularDelta;

        Vector3 delta = new Vector3(horizontal, jump, vertical);
        if (delta.Equals(Vector3.zero) == false)
        {
            rd.AddForce(delta * moveSpeed, ForceMode.Impulse);
        }

        if (angularDelta != 0f)
        {
            rd.AddRelativeTorque(transform.up * angularDelta * moveSpeed * 0.3f);
        }
    }
}
