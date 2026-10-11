using UnityEngine;

public class AgentComponent : MonoBehaviour
{
    public virtual bool detectionAllowed => true;

    public virtual void SetTeam(GameManager.PlayerType team)
    {

    }
}
