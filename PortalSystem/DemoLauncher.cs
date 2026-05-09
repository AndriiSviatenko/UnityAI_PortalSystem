using UnityEngine;

namespace PortalSystem
{
    public class DemoLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private float force = 10f;
        [SerializeField] private float fireRate = 1f;
        
        private float _nextFire;

        private void Update()
        {
            if (Time.time > _nextFire)
            {
                _nextFire = Time.time + fireRate;
                Launch();
            }
        }

        private void Launch()
        {
            if (prefab == null) return;
            
            GameObject go = Instantiate(prefab, transform.position, transform.rotation);
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * force;
            }
            
            // Cleanup after 10 seconds
            Destroy(go, 10f);
        }
    }
}
