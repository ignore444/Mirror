using UnityEngine;

namespace Mirror.Experimental
{
    public interface NCNetworkTransform
    {
        Vector3 velocity { get; set; }
        Vector3 position { get; set; }
        Vector3 angularVelocity { get; set; }
        Quaternion rotation { get; set; }
    }

    public class NCNetworkRigidbodyTransform : NCNetworkTransform
    {
        public Rigidbody target;

        public Vector3 velocity { get => target.velocity; set => target.velocity = value; }
        public Vector3 position { get => target.position; set => target.position = value; }
        public Vector3 angularVelocity { get => target.angularVelocity; set => target.angularVelocity = value; }
        public Quaternion rotation { get => target.rotation; set => target.rotation = value; }
    }

    public class NCNetworkObjectTransform : NCNetworkTransform
    {
        public Transform target;
        float temp;

        public Vector3 velocity { get => Vector3.zero; set => temp = value.x; }
        public Vector3 position { get => target.position; set => target.position = value; }
        public Vector3 angularVelocity { get => Vector3.zero; set => temp = value.x; }
        public Quaternion rotation { get => target.rotation; set => target.rotation = value; }
    }

    public class NCNetworkLerpRigidbody : NetworkBehaviour
    {
        internal NCNetworkTransform target = null;

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
