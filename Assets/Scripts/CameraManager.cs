using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using static GameManager;

[ExecuteInEditMode]
public class CameraManager : MonoBehaviour
{
    public static CameraManager instance { get; private set; }

    new Camera camera;
    public static Vector3 forward => rotation * Vector3.forward;
    public static Vector3 right => rotation * Vector3.right;
    public static Vector3 up => rotation * Vector3.up;
    public static Quaternion rotation { get; private set; }

    public static Vector3 sceneForward => rotation * Vector3.forward;
    public static Vector3 sceneRight => rotation * Vector3.right;
    public static Quaternion SceneRotation { get; private set; }

    public float moveSpeed = 16f;

    public float minSize = 10f;
    public float neutralSize = 15f;
    public float maxSize = 20f;

    Vector3 autoFocusPosition;
    float autoFocusSize;
    bool autoFocus;
    float autoFocusTime;
    Vector3 autoFocusTempOffset;

    public float autoFocusSpeed = 8f;

    private void Awake()
    {
        instance = this;
        camera = GetComponent<Camera>();

        GameManager.onNextTurn += OnNextTurn;
        GameManager.onGameStateChanged += OnGameStateChanged;

        autoFocusTempOffset = transform.position;
    }
    private void OnDestroy()
    {
        GameManager.onNextTurn -= OnNextTurn;
        GameManager.onGameStateChanged -= OnGameStateChanged;
    }

    private void Start()
    {
        SetAutoFocus(Vector3.zero, maxSize);
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;

        rotation = transform.rotation;
        Shader.SetGlobalVector("_CameraRight", right);
        Shader.SetGlobalVector("_CameraUp", up);
        Shader.SetGlobalVector("_CameraForward", forward);

        if(Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;

        if (Cursor.lockState != CursorLockMode.Confined && Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Confined;

        if (Application.isPlaying && Cursor.lockState == CursorLockMode.Confined)
        {
            Vector3 sceneForward = transform.forward;
            sceneForward.y = 0f;
            sceneForward.Normalize();
            SceneRotation = Quaternion.LookRotation(sceneForward, Vector3.up);

            Vector2 screenPositionNormalized = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            float moveUp = Mathf.InverseLerp(0.8f, 0.9f, screenPositionNormalized.y);
            float moveDown = Mathf.InverseLerp(0.2f, 0.1f, screenPositionNormalized.y);
            float moveRight = Mathf.InverseLerp(0.8f, 0.9f, screenPositionNormalized.x);
            float moveLeft = Mathf.InverseLerp(0.2f, 0.1f, screenPositionNormalized.x);
            Vector3 moves = new Vector3(moveRight - moveLeft, 0f, moveUp - moveDown);
            float zoom = -Input.mouseScrollDelta.y;

            if (moves.sqrMagnitude > 0f || zoom != 0f)
                autoFocus = false;

            if (autoFocus && autoFocusTime < 1f)
            {
                autoFocusTime += deltaTime * autoFocusSpeed;
                //Vector3 focusForce = transform.InverseTransformPoint(autoFocusPosition);
                //transform.position = Vector3.Lerp(transform.position, transform.position + new Vector3(focusForce.x, 0f, focusForce.y), autoFocusTime);
                Vector3 focusPoint = new Vector3(autoFocusPosition.x + autoFocusTempOffset.x, autoFocusTempOffset.y, autoFocusPosition.z + autoFocusTempOffset.z);
                transform.position = Vector3.Lerp(transform.position, focusPoint, autoFocusTime);
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, autoFocusSize, autoFocusTime);
            }
            else
            {
                if (GameManager.instance.state == GameManager.GameState.Battle)
                    transform.position += SceneRotation * moves * moveSpeed * (camera.orthographicSize / neutralSize) * deltaTime;

                camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + zoom, minSize, maxSize);
            }
        }
    }

    public void OnNextTurn(PlayerType player, int roundCount)
    {

        Vector3 focusPoint = Vector3.zero;
        int entitiesCount = 0;
        if (player != PlayerType.Defender)
        {
            entitiesCount += BaseAgent.assailants.Count;
            foreach (BaseAgent assailant in BaseAgent.assailants)
                focusPoint += assailant.transform.position;
        }
        if (player != PlayerType.Assailant)
        {
            entitiesCount += BaseAgent.defenders.Count;
            foreach (BaseAgent defender in BaseAgent.defenders)
                focusPoint += defender.transform.position;
        }

        if(entitiesCount > 0)
            focusPoint /= entitiesCount;

        SetAutoFocus(focusPoint, player == PlayerType.Neutral ? maxSize : neutralSize);
    }

    public void OnGameStateChanged(GameManager.GameState lastGameState, GameManager.GameState newGameState)
    {
        SetAutoFocus(Vector3.zero, maxSize);
    }

    public void SetAutoFocus(Vector3 position, float size)
    {
        autoFocus = true;
        autoFocusPosition = position;
        autoFocusSize = size;
        autoFocusTime = 0f;
    }
}
