
using UnityEngine;
using static GameManager;
using System.Collections.Generic;
using static Interactions;
using UnityEngine.Rendering.Universal;

public class Interactions : MonoBehaviour
{
    void Awake()
    {
        GameManager.onNextTurn += OnNextTurn;

        selectDecal = SelectArea.GetComponentInChildren<DecalProjector>();
        targetDecal = TargetArea.GetComponentInChildren<DecalProjector>();
    }
    private void OnDestroy()
    {
        GameManager.onNextTurn -= OnNextTurn;
    }

    // Update is called once per frame
    public void OnNextTurn(PlayerType player, int roundCount)
    {
        foreach (BaseEntity selectedEntity in selectedEntities)
            selectedEntity.ApplyTarget();

        Reset();
        canInteract = player != PlayerType.Neutral;
    }

    List<BaseEntity> selectedEntities = new List<BaseEntity>();
    public void SelectEntity(BaseEntity entity)
    {
        selectedEntities.Add(entity);
        entity.Select();
    }

    public void DeselectAll()
    {
        foreach (BaseEntity selectedEntity in selectedEntities)
            selectedEntity.Deselect();

        selectedEntities.Clear();
        HUDManager.instance.HideSelection();

        SelectArea.SetActive(false);
    }

    public void Targeting(BaseEntity entity)
    {
        Vector3 relativeEntityPosition = entity.transform.position - selectCenter;
        relativeEntityPosition.y = 0f;
        Vector3 relativeTargetPosition = relativeEntityPosition * (targetRadius / selectRadius);
        Vector3 targetPosition = relativeTargetPosition + targetCenter;
        targetPosition.y = entity.transform.position.y;

        RaycastHit hit;
        if (Physics.Linecast(targetPosition + Vector3.up * 20f, targetPosition + Vector3.down * 20f,out hit, groundMask))
            targetPosition = hit.point;

        entity.SetTarget(targetPosition);
    }

    public void UntargetAll()
    {
        foreach (BaseEntity selectedEntity in selectedEntities)
            selectedEntity.Untarget();
        
        TargetArea.SetActive(false);
    }

    public void Reset()
    {
        UntargetAll();
        DeselectAll();
        mouseHold = false;
        orderState = OrderState.NoSelection;
    }

    public float radiusSpeed = 4f;
    public float radiusMin = 2f;

    Vector3 selectCenter;
    float selectRadius;

    Vector3 targetCenter;
    float targetRadius;

    public GameObject SelectArea;
    public GameObject TargetArea;
    public DecalProjector selectDecal;
    public DecalProjector targetDecal;

    bool mouseHold = false;
    Vector3 mouseDownPosition;

    public enum OrderState { NoSelection,Selection,SelectionAndTarget  }
    OrderState orderState = OrderState.NoSelection;

    Vector2 mousePositionNormalized => Vector2.Scale(Input.mousePosition, new Vector2(1f/Screen.width, 1f/Screen.height));

    bool canInteract = false;

    public LayerMask entitiesMask;
    public LayerMask groundMask;
    public LayerMask gridMask;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            GameManager.instance.EndTurn();

        if (!canInteract)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            switch (orderState)
            {
                case OrderState.Selection:
                    Reset();
                    break;
                case OrderState.SelectionAndTarget:
                    UntargetAll();
                    mouseHold = false;
                    orderState = OrderState.Selection;
                    break;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseHold = false;
            switch (orderState)
            {
                case OrderState.Selection:
                    Collider[] selections = Physics.OverlapCapsule(selectCenter + Vector3.down * 20f, selectCenter + Vector3.up * 20f, selectRadius, entitiesMask);
                    //Debug.Log("selections count "+ selections.Length);
                    foreach (var selection in selections)
                    {
                        BaseAgent selectAgent = selection.GetComponent<BaseAgent>();
                        if (selectAgent != null && selectAgent.team == GameManager.instance.turn)
                            SelectEntity(selectAgent);
                    }
                    if( selectedEntities.Count == 0 )
                        Reset();
                    break;
                case OrderState.SelectionAndTarget:
                    foreach (var entity in selectedEntities)
                    {
                        Targeting(entity);
                    }
                    break;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Lancer le raycast
            if (Physics.Raycast(ray, out hit, 1000f , gridMask))
            {
                switch (orderState)
                {
                    default:
                    case OrderState.NoSelection:
                    case OrderState.SelectionAndTarget:
                        Reset();
                        selectCenter = hit.point;
                        
                        SelectArea.SetActive(true);
                        SelectArea.transform.position = selectCenter;

                        orderState = OrderState.Selection;
                        break;
                    case OrderState.Selection:
                        targetCenter = hit.point;
                        
                        TargetArea.SetActive(true);
                        TargetArea.transform.position = targetCenter;

                        orderState = OrderState.SelectionAndTarget;
                        break;
                }
                mouseHold = true;
                mouseDownPosition = mousePositionNormalized;
            }
        }

        if (mouseHold)
        {
            //Debug.Log(mousePositionNormalized);
            switch (orderState)
            {
                case OrderState.Selection:
                    selectRadius = Vector2.Distance(mouseDownPosition, mousePositionNormalized) * radiusSpeed + radiusMin;
                    Vector3 selectSize = new Vector3(selectRadius * 2f, 1f, selectRadius * 2f);
                    SelectArea.transform.localScale = selectSize;
                    selectDecal.size = new Vector3(selectSize.x , selectSize.z, selectDecal.size.z);
                    targetDecal.material.SetFloat("_Scale", Mathf.Clamp( selectRadius * 0.5f,0f,10f));
   
                    break;
                case OrderState.SelectionAndTarget:
                    targetRadius = Vector2.Distance(mouseDownPosition, mousePositionNormalized) * radiusSpeed + radiusMin;

                    Vector3 targetSize = new Vector3(targetRadius * 2f, 1f, targetRadius * 2f);
                    TargetArea.transform.localScale = targetSize;
                    targetDecal.size = new Vector3(targetSize.x, targetSize.z, targetDecal.size.z);
                    targetDecal.material.SetFloat("_Scale", Mathf.Clamp(targetRadius * 0.5f, 0f, 10f));
                    break;
            }
        }

        // Clic gauche
        /*if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Lancer le raycast
            if (Physics.Raycast(ray, out hit))
            {
                BaseEntity otherEntity = hit.collider.gameObject.GetComponent<BaseEntity>();
                if (otherEntity != null )
                {
                    if (otherEntity.team == GameManager.instance.turn)
                    {
                        SelectEntity(otherEntity);
                    }
                    else
                    {
                        BaseAgent otherAgent = otherEntity as BaseAgent;
                        if (otherAgent != null)
                            foreach (BaseEntity selectedEntity in selectedEntities)
                            {
                                BaseAgent agent = selectedEntity as BaseAgent;
                                if (agent != null)
                                    agent.MoveTo(otherAgent);
                            }
                    }
                }
                else if (selectedEntities.Count > 0)
                {
                    foreach (BaseEntity selectedEntity in selectedEntities)
                    {
                        BaseAgent agent = selectedEntity as BaseAgent;
                        if (agent != null)
                            agent.MoveTo(hit.point);
                    }
                }

            }
        }*/
    }


}
