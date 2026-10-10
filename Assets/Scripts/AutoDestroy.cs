using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 2f;
    void Start()
    {
        Destroy(gameObject,delay);
    }
}
