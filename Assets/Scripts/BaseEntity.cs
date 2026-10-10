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

    public GameObject deathPrefab;

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

    bool haveOrder;
    Vector3 orderDestination;
    public virtual void SetOrder(Vector3 destination)
    {
        haveOrder = true;
        orderDestination = destination;
        cursorTarget.SetActive(true);
        cursorTarget.transform.position = orderDestination;
    }
    public virtual void UnOrder()
    {
        haveOrder = false;
        cursorTarget.SetActive(false);
    }

    public void ApplyOrder()
    {
        if (haveOrder)
        {
            ApplyPlayerOrder(orderDestination);
            haveOrder = false;
        }
    }
    protected virtual void ApplyPlayerOrder(Vector3 targetDestination)
    {

    }

    public virtual int attack => 0;

    public virtual void Hit(BaseEntity source, Vector3 hitForce, float stuntTime = 1f)
    {
        Hit(source.attack, hitForce, stuntTime);
    }
    public virtual void Hit(int damage, Vector3 hitForce, float stuntTime = 1f)
    { 
        
    }
    public virtual void Kill()
    {
        if (deathPrefab != null)
            Instantiate(deathPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public virtual void SetTeam(GameManager.PlayerType team)
    {
        this.team = team;
        gameObject.layer = GameManager.GetTeamLayer(team);
    }
}
