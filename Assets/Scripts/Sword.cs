using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Sword : BaseAction
{
    public override void Action(BaseAgent self, BaseAgent target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Sword targetSword = target.action as Sword;
        if (targetSword != null)
        {
            bool priority = Vector3.Dot(self.velocity, direction) * self.hitPriority > Vector3.Dot(target.velocity, -direction) * target.hitPriority;
            if (target.paralyzed)
                priority = true;
            else if (self.paralyzed)
                priority = false;

            if (priority)
            {
                Attack(self,target);
                target.Hit(self, (direction + Vector3.up) * self.hitForce);
            }
            else
            {
                targetSword.Attack(target,self);
                self.Hit(target, (-direction + Vector3.up) * target.hitForce);
            }
        }
        else
        {
            Attack(self, target);
            target.Hit(self, (direction + Vector3.up) * self.hitForce);
        }
    }

    public void Attack(BaseAgent self, BaseAgent target)
    {
        lastActionTime = Time.time;
        self.character.Action(target.transform.position - self.transform.position);
    }
}
