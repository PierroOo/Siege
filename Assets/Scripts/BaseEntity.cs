using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class BaseEntity : MonoBehaviour
{
    public static List<BaseEntity> entities { get; private set; } = new List<BaseEntity>();

    public void Awake()
    {
        if(!entities.Contains(this))
            entities.Add(this);

        cursorSelect.SetActive(false);
        cursorTarget.SetActive(false);
    }
    public void OnDestroy()
    {
        if(entities.Contains(this))
            entities.Remove(this);
    }

    public UnityEvent OnSelectedEvent;
    public UnityEvent OnDeselectedEvent;

    public GameObject cursorSelect;
    public GameObject cursorTarget;

    public GameManager.PlayerType team;

    private void Start()
    {
        //cursorSelect.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.down);
        //cursorTarget.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.down);
    }

    public bool isSelected { get; private set; }
    public virtual void Select()
    {
        isSelected = true;

        cursorSelect.SetActive(true);

        if (OnSelectedEvent != null)
            OnSelectedEvent.Invoke();
    }
    public virtual void Deselect()
    {
        isSelected = false;

        cursorSelect.SetActive(false);

        if (OnDeselectedEvent != null)
            OnDeselectedEvent.Invoke();
    }

    bool haveTarget;
    Vector3 targetDestination;
    public virtual void SetTarget(Vector3 destination)
    {
        haveTarget = true;
        targetDestination = destination;
        cursorTarget.SetActive(true);
        cursorTarget.transform.position = targetDestination;
    }
    public virtual void Untarget()
    {
        haveTarget = false;
        cursorTarget.SetActive(false);
    }

    public void ApplyTarget()
    {
        if (haveTarget)
        {
            ApplyTarget(targetDestination);
            haveTarget = false;
        }
    }
    protected virtual void ApplyTarget(Vector3 targetDestination)
    {

    }

    public virtual int attack => 0;

    public virtual void Hit(BaseEntity source, Vector3 hitForce)
    {
        Hit(source.attack, hitForce);
    }
    public virtual void Hit(int damage, Vector3 hitForce)
    { 
        
    }
    public virtual void Kill()
    {
        Destroy(gameObject);
    }

    public virtual void SetTeam(GameManager.PlayerType team)
    {
        this.team = team;
        gameObject.layer = GameManager.GetTeamLayer(team);
    }
}
