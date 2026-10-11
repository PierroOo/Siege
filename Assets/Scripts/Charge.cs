using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AI;

public class Charge : AgentComponent
{
    NavMeshAgent navMeshAgent;
    BaseAgent agent;
    public void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        agent = GetComponent<BaseAgent>();
        audioSource = GetComponentInChildren<AudioSource>();
    }

    public float runningMinSpeed = 2f;
    public float runningMinTime = 1f;
    float runTimer;

    public float radius = 4f;
    public float randomForceDirection = 1f;

    public int damage = 8;
    public float force = 2f;
    public float stuntTime = 2f;

    AudioSource audioSource;
    public AudioResource hitClip;

    public override bool detectionAllowed => !running;
    bool running;

    public override void SetTeam(GameManager.PlayerType team)
    {
        collisionMaks = GameManager.GetTeamEnnemisLayer(team);
    }

    LayerMask collisionMaks;
    static Collider[] colliders = new Collider[4];

    private void FixedUpdate()
    {
        Vector3 position = transform.position;

        Vector3 velocity = navMeshAgent.velocity;
        running = velocity.sqrMagnitude > runningMinSpeed * runningMinSpeed;

        if (running)
            runTimer += Time.fixedDeltaTime;
        else
            runTimer = 0f;

        if (runTimer > runningMinTime)
        {
            int detectionCount = Physics.OverlapSphereNonAlloc(position, radius, colliders, collisionMaks);
            for (int i = 0; i < detectionCount; i++)
            {
                BaseAgent ennemy = colliders[i].gameObject.GetComponent<BaseAgent>();
                if (ennemy != null && !ennemy.invincible)
                {
                    //Vector3 direction = (velocity.normalized + Random.insideUnitSphere * randomForceDirection).normalized;
                    //direction.y = Mathf.Abs(direction.y);

                    Vector3 direction = ennemy.transform.position - position;
                    if (Vector3.Dot(velocity, direction) < 0f)
                        direction = Vector3.up;

                    direction = (direction.normalized + Random.insideUnitSphere * randomForceDirection).normalized;

                    ennemy.Hit(damage, direction * force, stuntTime);
                    audioSource.resource = hitClip;
                    audioSource.Play();
                }
            }
        }
    }
}
