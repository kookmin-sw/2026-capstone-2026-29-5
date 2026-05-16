using UnityEngine;

public class TrailOff : StateMachineBehaviour
{
    [Tooltip("비워두면 모든 Trail을 끔. 이름 지정하면 그 이름만 끔.")]
    public string[] trailNames;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var trails = animator.GetComponentsInChildren<Tiny.Trail>(true);
        bool turnOffAll = (trailNames == null || trailNames.Length == 0);

        foreach (var t in trails)
        {
            if (turnOffAll)
            {
                t.enabled = false;
            }
            else
            {
                foreach (var name in trailNames)
                {
                    if (t.name == name)
                    {
                        t.enabled = false;
                        break;
                    }
                }
            }
        }
    }
}