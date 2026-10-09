using UnityEngine;

public class Bow : BaseAction
{
    public GameObject projectilPrefab;
    public float force = 16f;
    public float randomForce = 4f;
    public float randomAim = 0.2f;
    public Vector3 spawnLocalPoint = Vector3.zero;

    public bool highArc;

    public override void Action(BaseAgent self, BaseAgent target)
    {
        lastActionTime = Time.time;

        Vector3 spawnPoint = self.transform.TransformPoint(spawnLocalPoint);

        Projectil projectil = Instantiate(projectilPrefab, spawnPoint, Quaternion.identity).GetComponent<Projectil>();

        Vector3 projectilVelocity;
        Balistic.CalculateBallisticVelocity(spawnPoint, target.transform.position, force + Random.Range(randomForce * -0.5f, randomForce * 0.5f), highArc, out projectilVelocity);

        projectilVelocity += Random.insideUnitSphere * randomAim;
        projectil.SetTeam(self.team);
        projectil.AddForce(projectilVelocity);
        self.character.Action( projectilVelocity );
    }
}
