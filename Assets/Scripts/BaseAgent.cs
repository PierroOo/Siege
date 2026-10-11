using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class BaseAgent : BaseEntity
{
    public static List<BaseAgent> assailants { get; private set; } = new List<BaseAgent>();
    public static List<BaseAgent> defenders { get; private set; } = new List<BaseAgent>();

    public int healthPoints { get; private set; } = 20;
    public int maxHealthPoints = 20;

    public override int attack => (int)Mathf.Max(1f, (float)maxAttack * ((float)healthPoints / (float)maxHealthPoints));
    public int maxAttack = 8;

    Vector3 m_destination;
    public BaseAgent agentTarget;
    public BaseAction action;
    public float actionDistance = 3f;
    NavMeshAgent navMeshAgent;

    public float destinationDistanceAction = 0f;
    public float destinationDistanceMove = 0f;

    AgentComponent component;
    bool haveComponent;

    public AudioSource audioSource;
    public AudioResource spawnClip;
    public AudioResource actionClip;
    public AudioResource deathClip;
    public AudioResource onGroundClip;
    public AudioResource orderClip;
    public void PlayAudio(AudioResource clip)
    {
        if (clip == null)
            return;

        audioSource.resource = clip;
        audioSource.Play();
    }

    public enum AgentBehaviour { noTarget, haveTarget}
    public AgentBehaviour behaviour { get; private set; }

    private Rigidbody rb;
    public Vector3 velocity => rb.isKinematic ? navMeshAgent.velocity : rb.linearVelocity;

    public Vector3 footPoint => transform.position + Vector3.down;

    [System.NonSerialized]
    public Character character;

    public MeshRenderer xRayRenderer;

    public new void Awake()
    {
        base.Awake();
        
        rb = GetComponent<Rigidbody>();


        healthPoints = maxHealthPoints;

        character = GetComponentInChildren<Character>();

        navMeshAgent = GetComponent<NavMeshAgent>();

        component = GetComponent<AgentComponent>();
        haveComponent = component != null;
    }
    public new void OnDestroy()
    {
        base.OnDestroy();

        if (assailants.Contains(this))
            assailants.Remove(this);

        if (defenders.Contains(this))
            defenders.Remove(this);
    }
    private void Start()
    {
        playerOrderDestination = transform.position;
    }

    public override void Select()
    {
        base.Select();
        //UpdateCursorTargetVisibility();

        HUDManager.instance.DrawSelection(this);
    }
    public override void Deselect()
    {
        base.Deselect();
        //UpdateCursorTargetVisibility();
    }

    //Ordre du joueur
    Vector3 playerOrderDestination;
    public enum OrderType {Player,Detection}
    OrderType orderType;

    protected override void ApplyPlayerOrder(Vector3 targetDestination)
    {
        playerOrderDestination = targetDestination;
        RefreshPlayerOrder();
        Focus();
    }
    public void RefreshPlayerOrder()
    {
        MoveTo(playerOrderDestination, OrderType.Player);
    }

    public void MoveTo(Vector3 destination, OrderType orderType)
    {
        navMeshAgent.stoppingDistance = destinationDistanceMove;
        m_MoveTo(destination, null, orderType);
}
    public void MoveTo(BaseAgent target, OrderType orderType)
    {
        navMeshAgent.stoppingDistance = destinationDistanceAction;
        m_MoveTo(target.footPoint, target, orderType);
    }

    void m_MoveTo(Vector3 destination, BaseAgent target, OrderType orderType)
    {
        if (!navMeshAgentAvailable)
            return;

        StopOrder();

        agentTarget = target;
        behaviour = AgentBehaviour.haveTarget;
        this.orderType = orderType;

        SetDestination(destination);

        HUDManager.instance.DrawSelection(this);
    }

    float lastFocusTime = float.MinValue;
    public float focusTime = 2f;
    public bool isFocus => lastFocusTime + focusTime > Time.time;
    void Focus()
    {
        lastFocusTime = Time.time;
    }

    protected float lastHitTime = float.MinValue;

    public bool invincible => (lastHitTime + invincibleTime) > Time.time;
    public bool paralyzed => invincible || action.busy;
    public float invincibleTime = 0.5f;
    public float hitPriority = 1f;
    public float hitForce = 4f;

    public bool actionKeepInertia;
    //public float targetDestinationExtend = 0f;

    public bool debugAgent;

    public void TryAction(BaseAgent target)
    {
        if (paralyzed && target.paralyzed)
            return;

        action.Action(this,target);

        PlayAudio(actionClip);

        if (actionKeepInertia)
        {
            if (physSequence != null)
                StopCoroutine(physSequence);

            physSequence = StartCoroutine(PhysSequence(Vector3.zero,0f));
        }
    }

    public override void Hit(int damage, Vector3 hitForce, float stuntTime)
    {
        healthPoints = Mathf.Clamp(healthPoints - damage, 0 , maxHealthPoints);
        if (healthPoints <= 0)
            Kill();

        lastHitTime = Time.time;

        if (stuntTime > 0f)
        {
            if (physSequence != null)
                StopCoroutine(physSequence);

            physSequence = StartCoroutine(PhysSequence(hitForce, stuntTime));
        }
        else
            navMeshAgent.velocity += hitForce;

        character.Hit(hitForce);
    }

    Coroutine physSequence;
    public IEnumerator PhysSequence(Vector3 hitForce, float minTime)
    {
        if (rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.linearVelocity = navMeshAgent.velocity * 0.5f;
        }

        rb.AddForce(hitForce, ForceMode.Impulse);

        navMeshAgent.enabled = false;

        if (minTime > 0f)
            yield return new WaitForSeconds(minTime);

        while ( !rb.IsSleeping() && lastCollisionTime + 0.5f < Time.time || action.busy)
            yield return null;

        navMeshAgent.enabled = true;
        navMeshAgent.velocity = rb.linearVelocity;
        rb.isKinematic = true;

        RefreshNavMeshMovement();
    }
    bool navMeshAgentAvailable => navMeshAgent.enabled && rb.isKinematic;

    float lastCollisionTime = float.MinValue;
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer == 0)
            lastCollisionTime = Time.time;
    }


    public void Heal(int healPoints)
    { 
        healthPoints = Mathf.Clamp(healthPoints + healPoints,0,maxHealthPoints);
    }

    public override void Kill()
    {
        healthPoints = 0;
        base.Kill();
    }

    public void StopOrder()
    {
        behaviour = AgentBehaviour.noTarget;
        m_destination = footPoint;
        agentTarget = null;
        //UpdateCursorTargetVisibility();
    }

    public void FixedUpdate()
    {
        if (debugAgent)
            Debug.DrawLine(transform.position + Vector3.up, m_destination + Vector3.up, Color.red);

        if (paralyzed)
            return;

        if (behaviour == AgentBehaviour.haveTarget)
        {
            if (debugAgent)
                for (int i = 1; i < navMeshAgent.path.corners.Length; i++)
                    Debug.DrawLine(navMeshAgent.path.corners[i - 1] + Vector3.up, navMeshAgent.path.corners[i] + Vector3.up, Color.yellow, 1f);

            if (navMeshAgentAvailable && agentTarget != null && Vector3.Distance(m_destination, agentTarget.footPoint) > 1f)
                SetDestination(agentTarget.footPoint);
            else if ( agentTarget == null && Distance2D(transform.position, m_destination) < 1f)
            {
                StopOrder();
            }
        }

        if (agentTarget != null && Vector3.Distance(transform.position, agentTarget.transform.position) < actionDistance)
            TryAction(agentTarget);
    }

    void SetDestination(Vector3 newDestination)
    {
        m_destination = newDestination;

        RefreshNavMeshMovement();
    }
    void RefreshNavMeshMovement()
    {
        if (navMeshAgent.stoppingDistance == 0f || (transform.position - m_destination).sqrMagnitude > navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance)
            navMeshAgent.destination = m_destination;
    }

    public float Distance2D(Vector3 A , Vector3 B)
    { 
        return Vector2.Distance(new Vector2(A.x,A.z),new Vector2 (B.x,B.z));
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(DetectionUpdate());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public float detectionPerSecond;
    public float detectionIdleRadius = 4f;
    public float detectionActionRadius = 8f;
    LayerMask detectionMask;
    static Collider[] detections = new Collider[16];
    public IEnumerator DetectionUpdate()
    {
        yield return null;
        while (true)
        {
            if (!isFocus && (!haveComponent || component.detectionAllowed ) )
            {
                Vector3 position = transform.position;
                float detectionRadius = behaviour == AgentBehaviour.noTarget ? detectionIdleRadius : detectionActionRadius;

                int detectionCount;
                if (detectionRadius > 0f)
                    detectionCount = Physics.OverlapSphereNonAlloc(position, detectionRadius, detections, detectionMask);
                else
                    detectionCount = 0;

                float closestDistance = float.MaxValue;
                Collider closestCollider = null;
                bool findCollision = false;

                for (int i = 0; i < detectionCount; i++)
                {
                    Collider collider = detections[i];
                    float distance = (position - collider.transform.position).sqrMagnitude;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCollider = detections[i];
                        findCollision = true;
                    }
                }
                if (findCollision)
                {
                    BaseAgent detectedAgent = closestCollider.gameObject.GetComponent<BaseAgent>();
                    if (detectedAgent != null)
                    {
                        MoveTo(detectedAgent, OrderType.Detection);
                        Focus();
                    }
                }
                else if (orderType == OrderType.Detection)
                    RefreshPlayerOrder();
            }
            yield return new WaitForSeconds(1f/ detectionPerSecond);
        }
    }

    public override void SetTeam(GameManager.PlayerType team)
    {
        base.SetTeam(team);

        if (team == GameManager.PlayerType.Assailant && !assailants.Contains(this))
            assailants.Add(this);

        if (team == GameManager.PlayerType.Defender && !defenders.Contains(this))
            defenders.Add(this);

        detectionMask = GameManager.GetTeamEnnemisLayer(team);
        xRayRenderer.material.color = GameManager.GetTeamColor(team);
        character.SetTeam(team);

        if(component != null)
            component.SetTeam(team);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionIdleRadius);
    }
}
