using UnityEngine;

public class TrailOff : StateMachineBehaviour
{
    [Tooltip("비워두면 모든 Trail을 끔. 이름 지정하면 그 이름만 끔.")]
    public string trailName = "";

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var trails = animator.GetComponentsInChildren<Tiny.Trail>(true);
        foreach (var t in trails)
        {
            if (string.IsNullOrEmpty(trailName) || t.name == trailName)
                t.enabled = false;
        }
    }
}