using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    private void Awake()
    {
        if (once)
            GameManager.onGameStateChanged += OnGameStateChanged;

        if (!once)
            GameManager.onNextRound += OnNextRound;
    }

    private void OnDestroy()
    {
        if (once)
            GameManager.onGameStateChanged -= OnGameStateChanged;

        if (!once)
            GameManager.onNextRound -= OnNextRound;
    }

    public GameObject prefab;
    public GameManager.PlayerType team = GameManager.PlayerType.Neutral;
    public int count = 1;

    //public enum SpawnEvent {StartGame,Round }
    public int roundFrequency = 0;
    int roundCount = 999;
    bool once => roundFrequency <= 0;
    //public SpawnEvent spawnEvent = SpawnEvent.StartGame;
    public float randomOffset = 1f;
    public LayerMask groundMask;
    public Vector3 GetGroundSpawnPosition()
    {
        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-randomOffset, randomOffset), 0f, Random.Range(-randomOffset, randomOffset));
        RaycastHit hit;
        Vector3 rayVector = Vector3.down * 20f;
        if (Physics.Linecast(spawnPosition - rayVector, spawnPosition + rayVector, out hit, groundMask))
            spawnPosition = hit.point;

        NavMeshAgent navMeshAgent = prefab.GetComponent<NavMeshAgent>();
        float entityHeight = navMeshAgent != null ? navMeshAgent.baseOffset : 0f;
        return spawnPosition + (Vector3.up * entityHeight);
    }

    private void OnGameStateChanged(GameManager.GameState lastState, GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Battle)
            Spawn();
    }

    private void OnNextRound(int roundCount)
    {
        if (BaseAgent.assailants.Count >= GameManager.instance.maxAssailants)
            return;

        this.roundCount++;
        if (this.roundCount >= roundFrequency)
        {
            this.roundCount = 0;
            Spawn();
        }
    }

    public void Spawn()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(prefab, GetGroundSpawnPosition(), Quaternion.identity);
            BaseEntity entity = instance.GetComponent<BaseEntity>();
            if (entity != null)
                entity.SetTeam(team);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, randomOffset);
    }
}
