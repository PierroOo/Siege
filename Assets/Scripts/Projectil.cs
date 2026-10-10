using UnityEngine;
using UnityEngine.UIElements;

public class Projectil : MonoBehaviour
{
    Vector3 velocity;
    Vector3 lastPosition;
    LayerMask collisionMask;
    static RaycastHit hit;

    public int damage = 4;
    public float weight = 1f;
    public float stuntTime = 0.1f;

    public float maxLifeTime = 6f;
    float timer;

    public SpriteRenderer spriteRenderer;
    //public float rotationScale = 0.7f;

    public float rayDeltaTime = 0.1f;
    float lastRayTime;

    public enum OrientType {Velocity,Static }
    public OrientType orientType;

    public float killDelay = 2f;

    public GameObject impactPrefab;

    GameManager.PlayerType team;

    public void AddForce(Vector3 velocity)
    {
        this.velocity = velocity;        
    }

    private void Start()
    {
        lastPosition = transform.position;
        lastRayTime = Time.time;
    }
    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer > maxLifeTime)
        { 
            Kill(0f);
            return;
        }

        Vector3 position = transform.position;

        velocity += Physics.gravity * Time.fixedDeltaTime;
        position += velocity * Time.fixedDeltaTime;

        if (lastRayTime + rayDeltaTime < Time.time)
        {
            lastRayTime = Time.time;
            Debug.DrawLine(lastPosition, position,Color.gray,2f);
            if (Physics.Linecast(lastPosition, position, out hit, collisionMask))
            {
                position = hit.point;

                if (impactPrefab != null)
                {
                    Impact impact = Instantiate(impactPrefab, transform.position, Quaternion.identity).GetComponent<Impact>();
                    impact.SetTeam(team);
                }

                BaseEntity entity = hit.collider.GetComponent<BaseEntity>();
                if (entity != null)
                {
                    entity.Hit(damage, velocity * weight, stuntTime);
                    Kill(0f);
                }
                else
                    Kill(killDelay);
            }
            lastPosition = position;
        }

        bool toLeft = Vector3.Dot(velocity, Camera.main.transform.right) < 0f;
        //spriteRenderer.flipX = toLeft;

        //float angle = Vector3.Dot(velocity.normalized, Vector3.up) * 90f * rotationScale * (toLeft ? -1f : 1f);
        //transform.rotation = Quaternion.AngleAxis(angle, CameraManager.forward) * CameraManager.rotation;

        if(orientType == OrientType.Velocity)
            transform.rotation = Quaternion.LookRotation(CameraManager.forward, velocity) * Quaternion.AngleAxis(90f, Vector3.forward);
        else
            transform.rotation = Quaternion.LookRotation(CameraManager.forward, Vector3.up);

        transform.position = position;
    }

    public void Kill(float deathTime)
    { 
        Destroy(gameObject, deathTime);
        this.enabled = false;
    }

    public void SetTeam(GameManager.PlayerType team)
    {
        this.team = team;
        spriteRenderer.color = GameManager.GetTeamColor(team);
        collisionMask = GameManager.GetTeamCollisionLayer(team);
    }
}
