using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EPlayer : MonoBehaviour
{
    public CharacterController characterController;
    public Rigidbody rigidbody;
    

    [Header("Diagnostics")]
    public float horizontal;
    public float vertical;
    public float turn;
    public float jumpSpeed;
    public bool isGrounded = true;
    public bool isFalling;
    public Vector3 velocity;
    public Vector3 distance;


    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float turnSensitivity = 5f;
    public float maxTurnSpeed = 150f;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.orthographic = false;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0f, 3f, -8f);
        Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Q and E cancel each other out, reducing the turn to zero
        if (Input.GetKey(KeyCode.Q))
            turn = Mathf.MoveTowards(turn, -maxTurnSpeed, turnSensitivity);
        if (Input.GetKey(KeyCode.E))
            turn = Mathf.MoveTowards(turn, maxTurnSpeed, turnSensitivity);
        if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
            turn = Mathf.MoveTowards(turn, 0, turnSensitivity);
        if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
            turn = Mathf.MoveTowards(turn, 0, turnSensitivity);

        if (isGrounded)
            isFalling = false;

        if ((isGrounded || !isFalling) && jumpSpeed < 5f && Input.GetKey(KeyCode.Space))
        {
            jumpSpeed = Mathf.Lerp(jumpSpeed, 5f, 0.5f);
        }
        else if (!isGrounded)
        {
            isFalling = true;
            jumpSpeed = 0;
        }

        if (rigidbody)
        {
            if (true)
            {// impulse
                Vector3 moveSpeed = new Vector3(horizontal, jumpSpeed, vertical);
                Vector3 impluse = moveSpeed * rigidbody.mass;
                rigidbody.AddForce(impluse, ForceMode.Impulse);
            }

            /*
            if (false)
            {// transform
                Vector3 speed = new Vector3(horizontal, 0f, vertical);
                Vector3 move = speed * Time.deltaTime;
                distance = move;
                transform.Translate(move,Space.World);
            }
            */
        }
    }

    void FixedUpdate()
    {
       
        if (!rigidbody)
        {
            {// position base
                Vector3 speed = new Vector3(horizontal, jumpSpeed, vertical) * moveSpeed;
                Vector3 move = speed * Time.fixedDeltaTime;
                distance = move;

                if (jumpSpeed > 0)
                {
                    characterController.Move(move);
                }
                else
                {
                    transform.Translate(move);
                    //characterController.Move(move);
                    //characterController.SimpleMove(speed);
                }
            }
        }
        
        if( characterController )
        {
            isGrounded = characterController.isGrounded;
            velocity = characterController.velocity;
        }


        //transform.Rotate(0f, turn * Time.fixedDeltaTime, 0f);
        //transform.Rotate(0f, 0f, 0f);
        /*
        Vector3 direction = new Vector3(horizontal, jumpSpeed, vertical);
        //direction = Vector3.ClampMagnitude(direction, 1f);
        direction = transform.TransformDirection(direction);
        direction *= moveSpeed;

        if( direction.Equals(new Vector3(0.0f,0.0f,0.0f)) == false)
        {
            int k = 0;
        }

        if (jumpSpeed > 0)
            characterController.Move(direction * Time.fixedDeltaTime);
        else
            characterController.SimpleMove(direction);

        isGrounded = characterController.isGrounded;
        velocity = characterController.velocity;
        */
    }
}
