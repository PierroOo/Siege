using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;
using static UnityEngine.UI.GridLayoutGroup;

public class BaseAgent : BaseEntity
{
    public static List<BaseAgent> assailants { get; private set; } = new List<BaseAgent>();
    public static List<BaseAgent> defenders { get; private set; } = new List<BaseAgent>();

    public int healthPoints { get; private set; } = 20;
    public int maxHealthPoints = 20;

    public override int attack => (int)Mathf.Max(1f, (float)maxAttack * ((float)healthPoints / (float)maxHealthPoints));
    public int maxAttack = 8;

    
    //public Vector3 destination => agentTarget != null ? agentTarget.footPoint : m_destination;
    Vector3 m_destination;
    public BaseAgent agentTarget;
    public BaseAction action;
    public float actionDistance = 3f;
    NavMeshAgent navMeshAgent;

    public float destinationDistanceAction = 0f;
    public float destinationDistanceMove = 0f;

    
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
    }
    public new void OnDestroy()
    {
        base.OnDestroy();

        if (assailants.Contains(this))
            assailants.Remove(this);

        if (defenders.Contains(this))
            defenders.Remove(this);
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

    protected override void ApplyTarget(Vector3 targetDestination)
    {
        MoveTo(targetDestination);
    }


    public void MoveTo(Vector3 destination)
    {
        navMeshAgent.stoppingDistance = destinationDistanceMove;
        MoveTo(destination, null);
    }
    public void MoveTo(BaseAgent target)
    {
        navMeshAgent.stoppingDistance = destinationDistanceAction;
        MoveTo(target.footPoint, target);
    }

    void MoveTo(Vector3 destination, BaseAgent target)
    {
        if (!navMeshAgentAvailable)
            return;

        StopOrder();

        agentTarget = target;
        behaviour = AgentBehaviour.haveTarget;

        
        SetDestination(destination);

        HUDManager.instance.DrawSelection(this);

        OnOrder();
    }

    float lastOrderTime = float.MinValue;
    public float focusTime = 2f;
    public bool isFocus => lastOrderTime + focusTime > Time.time;
    void OnOrder()
    {
        lastOrderTime = Time.time;
    }


    protected float lastHitTime = float.MinValue;

    public bool stunt => lastHitTime + hitTime > Time.time;
    public bool paralyzed => stunt || action.busy;
    public float hitTime = 0.5f;
    public float hitPriority = 1f;
    public float hitForce = 4f;

    public void TryAction(BaseAgent target)
    {
        if (paralyzed && target.paralyzed)
            return;

        action.Action(this,target);
    }

    public override void Hit(int damage, Vector3 hitForce)
    {
        healthPoints = Mathf.Clamp(healthPoints - damage, 0 , maxHealthPoints);
        if (healthPoints <= 0)
            Kill();

        lastHitTime = Time.time;

        if (hitSequence != null)
            StopCoroutine(hitSequence);

        hitSequence = StartCoroutine(HitSequence(hitForce));

        character.Hit(hitForce);
    }

    Coroutine hitSequence;
    public IEnumerator HitSequence(Vector3 hitForce)
    {
        rb.isKinematic = false;
        navMeshAgent.enabled = false;

        rb.linearVelocity = hitForce;

        yield return new WaitForSeconds(1f);

        while (lastCollisionTime + 0.5f < Time.time)
            yield return null;

        rb.isKinematic = true;
        navMeshAgent.enabled = true;
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
        Destroy(gameObject);
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
        if (paralyzed)
            return;

        if (behaviour == AgentBehaviour.haveTarget)
        {
            if (navMeshAgentAvailable && agentTarget != null && Vector3.Distance(m_destination, agentTarget.footPoint) > 1f)
                SetDestination(agentTarget.footPoint);

            for (int i = 1; i < navMeshAgent.path.corners.Length; i++)
                Debug.DrawLine(navMeshAgent.path.corners[i - 1] + Vector3.up, navMeshAgent.path.corners[i] + Vector3.up, Color.yellow, 1f);

            if (agentTarget == null && Distance2D(transform.position, m_destination) < 1f)
                StopOrder();
        }

        if (agentTarget != null && Vector3.Distance(transform.position, agentTarget.transform.position) < actionDistance)
            TryAction(agentTarget);
    }

    void SetDestination(Vector3 newDestination)
    {
        m_destination = newDestination;

        if (navMeshAgent.stoppingDistance == 0f || (transform.position - m_destination).sqrMagnitude > navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance)
            navMeshAgent.destination = newDestination;
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
            if (!isFocus)
            {
                Vector3 position = transform.position;
                float detectionRadius = behaviour == AgentBehaviour.noTarget ? detectionIdleRadius : detectionActionRadius;
                int detectionCount = Physics.OverlapSphereNonAlloc(position, detectionRadius, detections, detectionMask);

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
                        MoveTo(detectedAgent);
                    }
                }
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionIdleRadius);
    }
}
