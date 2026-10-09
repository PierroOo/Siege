using UnityEngine;

public class CameraOrient : MonoBehaviour
{
    public Vector3 localPivot;
    public bool maintainOffset = true;
    void Start()
    {
        Vector3 startPivotPosition = transform.TransformPoint(localPivot);
        transform.rotation = CameraManager.rotation * transform.localRotation;
        Vector3 endPivotPosition = transform.TransformPoint(localPivot);

        Vector3 delta = startPivotPosition - endPivotPosition;
        //Debug.DrawLine(startPivotPosition, endPivotPosition, Color.blue,2f);
        //Debug.Log(delta);
        transform.position = transform.position + delta;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.TransformPoint(localPivot), Vector3.one * 0.1f);
    }
}
