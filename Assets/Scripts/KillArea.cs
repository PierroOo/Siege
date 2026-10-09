using UnityEngine;

public class KillArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BaseAgent agent = other.GetComponent<BaseAgent>();
        agent.Kill();
    }
}
