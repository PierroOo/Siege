using Unity.VisualScripting;
using UnityEngine;

public class BaseAction : MonoBehaviour
{
    protected float lastActionTime = float.MinValue;
    public float actionTime = 1.5f;
    public bool busy => lastActionTime + actionTime > Time.time;
    
    public virtual void Action(BaseAgent self, BaseAgent target)
    {
        lastActionTime = Time.time;
    }
}
