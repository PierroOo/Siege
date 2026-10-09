using UnityEngine;

public class Impact : MonoBehaviour
{
    public float radius = 4f;
    public int damage = 4;
    public float force = 1f;

    public float killDelay = 1f;
    void Start()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider collider in colliders)
        {
            BaseEntity entity = collider.GetComponent<BaseEntity>();
            if (entity != null)
            {
                Vector3 direction = (entity.transform.position - transform.position).normalized;

                entity.Hit(damage, direction * force);
            }
        }

        Destroy(gameObject, killDelay);
    }
}
