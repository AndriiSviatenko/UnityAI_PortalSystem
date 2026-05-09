using UnityEngine;

namespace PortalSystem
{
    /// <summary>
    /// Attach this component to any GameObject with a Rigidbody to enable it to travel through Portals.
    /// It manages state to prevent infinite loops and provides access to the Rigidbody for the Portal system.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Teleportable : MonoBehaviour
    {
        /// <summary>
        /// Cached reference to the Rigidbody component.
        /// </summary>
        public Rigidbody Rigidbody { get; private set; }
        
        private Portal _lastPortal;
        private float _lastTeleportTime;
        
        // Cooldown in seconds to prevent the same portal from triggering a teleport immediately after exit.
        private const float TeleportCooldown = 0.1f;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Determines if the object is eligible to teleport through the specified portal.
        /// </summary>
        /// <param name="portal">The portal attempting to teleport this object.</param>
        /// <returns>True if the object is not on cooldown from its last teleport.</returns>
        public bool CanTeleport(Portal portal)
        {
            // If the portal is the same one we just came OUT of, check the cooldown.
            if (portal == _lastPortal && Time.time - _lastTeleportTime < TeleportCooldown)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called by the Portal system immediately after a successful teleportation.
        /// Updates the internal state to prevent physics loops.
        /// </summary>
        /// <param name="destinationPortal">The portal the object just exited from.</param>
        public void OnTeleported(Portal destinationPortal)
        {
            _lastPortal = destinationPortal;
            _lastTeleportTime = Time.time;
        }
    }
}
