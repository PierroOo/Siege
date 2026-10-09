using UnityEngine;

public class ProjectilSpawner : MonoBehaviour
{
    public GameObject prefab;
    public float force = 16f;
    public float randomDirection = 0.1f;
    public float randomForce = 4f;
    void Update()
    {

        if (Input.GetKey(KeyCode.X))
        {
            Time.timeScale = 1f;

            Projectil projectil = Instantiate(prefab,transform.position,Quaternion.identity).GetComponent<Projectil>();
            projectil.SetTeam(GameManager.PlayerType.Neutral);
            projectil.AddForce((transform.forward + Random.insideUnitSphere * randomDirection).normalized * (force + Random.Range(0f,randomForce)));
        }

        if (Input.GetKeyDown(KeyCode.V) && BaseAgent.defenders.Count > 0)
        {
            Time.timeScale = 1f;
            Projectil projectil = Instantiate(prefab, transform.position, Quaternion.identity).GetComponent<Projectil>();

            BaseAgent agent = BaseAgent.defenders[Random.Range(0, BaseAgent.defenders.Count)];
            Vector3 velocity;
            Balistic.CalculateBallisticVelocity(transform.position, agent.transform.position, force + Random.Range(0f, randomForce), false, out velocity);

            projectil.SetTeam(GameManager.PlayerType.Assailant);
            projectil.AddForce(velocity);
        }
    }
}
