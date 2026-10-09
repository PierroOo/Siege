using UnityEngine;

public class Balistic
{
    public static bool CalculateBallisticVelocity(
        Vector3 start,
        Vector3 target,
        float speed,
        bool highArc, // true pour un tir en cloche, false pour un tir tendu
        out Vector3 velocity)
    {
        velocity = Vector3.zero;

        Vector3 direction = target - start;
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);

        float distanceXZ = directionXZ.magnitude;
        float height = direction.y;

        if (distanceXZ < 0.001f)
            return false;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float speed2 = speed * speed;
        float speed4 = speed2 * speed2;

        float discriminant =
            speed4 - gravity * (gravity * distanceXZ * distanceXZ + 2 * height * speed2);

        float angle;

        if (discriminant >= 0)
        {
            float sqrt = Mathf.Sqrt(discriminant);

            // On choisit le signe en fonction du paramètre highArc (+ pour la cloche, - pour le tir tendu)
            if (highArc)
            {
                angle = Mathf.Atan((speed2 + sqrt) / (gravity * distanceXZ));
            }
            else
            {
                angle = Mathf.Atan((speed2 - sqrt) / (gravity * distanceXZ));
            }
        }
        else
        {
            // Cible hors de portée → angle optimal (45 degrés)
            angle = Mathf.PI / 4f;
        }

        Vector3 directionFinal =
            directionXZ.normalized * Mathf.Cos(angle) +
            Vector3.up * Mathf.Sin(angle);

        velocity = directionFinal * speed;
        return true;
    }
}