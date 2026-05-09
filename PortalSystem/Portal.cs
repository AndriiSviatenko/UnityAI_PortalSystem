using UnityEngine;
using System.Collections.Generic;

namespace PortalSystem
{
    /// <summary>
    /// Implements a portal that preserves momentum and reorients objects relative to a linked destination portal.
    /// Uses Rigidbody position for accurate crossing detection in the physics loop.
    /// </summary>
    public class Portal : MonoBehaviour
    {
        [Header("Linked Portals")]
        [Tooltip("The portal that objects will exit from when entering this one.")]
        [SerializeField] private Portal linkedPortal;

        [Header("Visuals")]
        [SerializeField] private Color portalColor = Color.cyan;

        // Tracks objects and their relative position state to detect plane crossing.
        private Dictionary<Teleportable, bool> _wasInFront = new Dictionary<Teleportable, bool>();
        private List<Teleportable> _trackedObjects = new List<Teleportable>();

        public Portal LinkedPortal => linkedPortal;

        private void OnTriggerEnter(Collider other)
        {
            var teleportable = other.GetComponentInParent<Teleportable>();
            if (teleportable != null && !_trackedObjects.Contains(teleportable))
            {
                _trackedObjects.Add(teleportable);
                // Initialize tracking state based on current physics position
                _wasInFront[teleportable] = IsInFront(teleportable.Rigidbody.position);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            var teleportable = other.GetComponentInParent<Teleportable>();
            if (teleportable != null && !_trackedObjects.Contains(teleportable))
            {
                _trackedObjects.Add(teleportable);
                _wasInFront[teleportable] = IsInFront(teleportable.Rigidbody.position);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var teleportable = other.GetComponentInParent<Teleportable>();
            if (teleportable != null)
            {
                _trackedObjects.Remove(teleportable);
                _wasInFront.Remove(teleportable);
            }
        }

        private void FixedUpdate()
        {
            // Reverse iteration for safe removal from the tracked list
            for (int i = _trackedObjects.Count - 1; i >= 0; i--)
            {
                var obj = _trackedObjects[i];
                if (obj == null)
                {
                    _trackedObjects.RemoveAt(i);
                    continue;
                }

                if (CheckAndTeleport(obj))
                {
                    // If teleported, remove from this portal's tracking immediately.
                    _trackedObjects.RemoveAt(i);
                    _wasInFront.Remove(obj);
                }
            }
        }

        private bool CheckAndTeleport(Teleportable obj)
        {
            if (!obj.CanTeleport(this)) return false;

            // Use Rigidbody position for crossing detection
            Vector3 currentPos = obj.Rigidbody.position;
            bool isNowInFront = IsInFront(currentPos);
            
            bool previouslyInFront = true;
            if (_wasInFront.TryGetValue(obj, out bool wasFront))
            {
                previouslyInFront = wasFront;
            }

            // Update state for next frame
            _wasInFront[obj] = isNowInFront;

            // Teleport if the object moved from Front to Back.
            if (previouslyInFront && !isNowInFront)
            {
                TeleportObject(obj);
                return true;
            }

            return false;
        }

        private bool IsInFront(Vector3 worldPos)
        {
            Vector3 offset = worldPos - transform.position;
            return Vector3.Dot(offset, transform.forward) >= 0;
        }

        private void TeleportObject(Teleportable obj)
        {
            if (linkedPortal == null) return;

            Rigidbody rb = obj.Rigidbody;

            // 1. Calculate transformation matrix.
            Matrix4x4 m = linkedPortal.transform.localToWorldMatrix 
                         * Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0)) 
                         * transform.worldToLocalMatrix;

            // 2. Transform Position and Rotation using Rigidbody state.
            Vector3 newPos = m.MultiplyPoint3x4(rb.position);
            Quaternion newRot = m.rotation * rb.rotation;

            // 3. Transform Velocities to preserve momentum.
            Vector3 entryVelocity = rb.linearVelocity;
            Vector3 entryAngularVelocity = rb.angularVelocity;
            
            Vector3 exitVelocity = m.MultiplyVector(entryVelocity);
            Vector3 exitAngularVelocity = m.MultiplyVector(entryAngularVelocity);

            // 4. Force Magnitude Preservation.
            float speed = entryVelocity.magnitude;
            if (speed > 0.001f)
            {
                exitVelocity = exitVelocity.normalized * speed;
            }

            // 5. Apply transformations instantly.
            var characterMovement = obj.GetComponent<Character.CharacterMovement>();
            if (characterMovement != null)
            {
                characterMovement.Teleport(newPos, newRot);
            }
            else
            {
                rb.position = newPos;
                rb.rotation = newRot;
            }
            
            rb.linearVelocity = exitVelocity;
            rb.angularVelocity = exitAngularVelocity;

            // 6. Update Teleportable state.
            obj.OnTeleported(linkedPortal);
            
            Debug.Log($"[Portal] {obj.name} teleported from {name} to {linkedPortal.name}. Speed: {speed:F2} m/s");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = portalColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2, 3, 0.1f));
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(Vector3.zero, Vector3.forward * 0.5f);
        }
    }
}
