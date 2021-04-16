using UnityEngine;

namespace Mirror.Experimental
{
    public class NNetworkLerpRigidbody : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] internal Rigidbody target = null;
        [Tooltip("How quickly current velocity approaches target velocity")]
        [SerializeField] float lerpVelocityAmount = 0.5f;
        [Tooltip("How quickly current position approaches target position")]
        [SerializeField] float lerpPositionAmount = 0.5f;

        [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
        [SerializeField] bool clientAuthority = false;

        float nextSyncTime;


        [SyncVar()]
        Vector3 targetVelocity;

        [SyncVar()]
        Vector3 targetPosition;

        [SyncVar()]
        Vector3 targetAngularVelocity;

        [SyncVar()]
        Quaternion targetRotation;

        /// <summary>
        /// Ignore value if is host or client with Authority
        /// </summary>
        /// <returns></returns>
        bool IgnoreSync => isServer || ClientWithAuthority;

        bool ClientWithAuthority => clientAuthority && hasAuthority;

        void OnValidate()
        {
            if (target == null)
            {
                target = GetComponent<Rigidbody>();
            }
        }

        void Update()
        {
            if (isServer)
            {
                SyncToClients();
            }
            else if (ClientWithAuthority)
            {
                SendToServer();
            }
        }

        void SyncToClients()
        {
            targetVelocity = target.velocity;
            targetPosition = target.position;
            targetAngularVelocity = target.angularVelocity;
            targetRotation = target.rotation;
        }

        void SendToServer()
        {
            float now = Time.time;
            if (now > nextSyncTime)
            {
                nextSyncTime = now + syncInterval;
                CmdSendState(target.velocity, target.position, target.angularVelocity, target.rotation);
            }
        }

        [Command]
        void CmdSendState(Vector3 velocity, Vector3 position, Vector3 angularVelocity, Quaternion rotation)
        {
            target.velocity = velocity;
            target.position = position;
            target.angularVelocity = angularVelocity;
            target.rotation = rotation;

            targetVelocity = velocity;
            targetPosition = position;
            targetAngularVelocity = angularVelocity;
            targetRotation = rotation;
        }

        void FixedUpdate()
        {
            if (IgnoreSync) { return; }

            target.velocity = Vector3.Lerp(target.velocity, targetVelocity, lerpVelocityAmount);
            target.position = Vector3.Lerp(target.position, targetPosition, lerpPositionAmount);
            target.angularVelocity = Vector3.Lerp(target.angularVelocity, targetAngularVelocity, lerpVelocityAmount);
            target.rotation = Quaternion.Slerp(target.rotation, targetRotation, lerpPositionAmount);

            // add velocity to position as position would have moved on server at that velocity
            targetPosition += target.velocity * Time.fixedDeltaTime;
            targetRotation = targetRotation * Quaternion.Euler(target.angularVelocity * Time.fixedDeltaTime);

            // TODO does this also need to sync acceleration so and update velocity?
        }


    }
}
