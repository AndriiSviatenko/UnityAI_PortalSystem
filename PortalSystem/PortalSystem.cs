using UnityEngine;

namespace PortalSystem
{
    /// <summary>
    /// Manages a pair of portals.
    /// </summary>
    public class PortalSystem : MonoBehaviour
    {
        [SerializeField] private Portal portalA;
        [SerializeField] private Portal portalB;

        public Portal PortalA => portalA;
        public Portal PortalB => portalB;

        private void Awake()
        {
            if (portalA == null || portalB == null)
            {
                Debug.LogError("[PortalSystem] Portal pair not fully assigned!", this);
                return;
            }

            // Ensure they are linked to each other
            // We use reflection/private fields or just assume they are set in inspector
            // For now, let's just log verification.
        }
    }
}
