using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{

    public Animator animator;
    public SpriteRenderer spriteRenderer;
    Rigidbody rb;
    NavMeshAgent navMeshAgent;
    public Vector3 velocity => rb.isKinematic ? navMeshAgent.velocity : rb.linearVelocity;
    public bool flip = false;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        navMeshAgent = GetComponentInParent<NavMeshAgent>();
    }
    private void Start()
    {
        animator.Play("Idle", 0,Random.value);
    }

    void Update()
    {
        bool moving = velocity.sqrMagnitude > 10f;
        animator.SetBool("Walking", moving);
        if (moving && rb.isKinematic)
            spriteRenderer.flipX = (Vector3.Dot(velocity, Camera.main.transform.right) < 0f) ^ flip;
    }

    public void Action(Vector3 direction)
    {
        Action(Vector3.Dot(direction, CameraManager.right) > 0f);
    }
    public void Action(bool toRight)
    {
        spriteRenderer.flipX = !toRight ^ flip;
        animator.SetTrigger("Action");
    }

    public void Hit(Vector3 direction)
    {
        Hit(Vector3.Dot(direction, CameraManager.right) < 0f);
    }
    public void Hit(bool fromRight)
    {
        spriteRenderer.flipX = !fromRight ^ flip;
        animator.SetTrigger("Hit");
    }

    public void SetTeam(GameManager.PlayerType team)
    {
        spriteRenderer.color = GameManager.GetTeamColor(team);
    }
}
