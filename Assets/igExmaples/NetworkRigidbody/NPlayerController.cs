using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NPlayerController : NetworkBehaviour
{
    Rigidbody rd;

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

    private void Start()
    {
        rd = GetComponent<Rigidbody>();
        if( netIdentity.isClient )
        {
            rd.isKinematic = true;
        }
        else
        {
            rd.isKinematic = false;
        }
    }
    public override void OnStartLocalPlayer()
    {
        Camera.main.orthographic = false;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0f, 3f, -8f);
        Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
    }

    
    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (NetworkClient.active == false)
            return;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        if( Input.GetKey(KeyCode.Space) )
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
            AddImpluse(delta);
        }

        if(angularDelta != 0f)
        {
            Turn(angularDelta);
        }
    }

    [Command]
    void AddImpluse(Vector3 impluseDelta)
    {
        rd.AddRelativeForce(impluseDelta * moveSpeed, ForceMode.Impulse);
    }

    [Command]
    void Turn(float angularDelta)
    {
        rd.AddRelativeTorque(transform.up * angularDelta * moveSpeed * 0.3f);
    }
}
