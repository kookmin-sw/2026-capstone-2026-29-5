using UnityEngine;
using Tiny;

public class trailController : MonoBehaviour
{
    [SerializeField] private Trail trail;

    // 기본 무기의 Trail (Awake에서 자동 탐색된 것). Unequip 시 복원용.
    private Trail defaultTrail;

    private void Awake()
    {
        if (trail == null)
            trail = GetComponentInChildren<Trail>(true);

        defaultTrail = trail;

        // 시작할 때는 꺼둠
        if (trail != null)
            trail.enabled = false;
    }

    /// <summary>
    /// 무기 장착 시 외부에서 컨트롤할 Trail을 지정한다.
    /// 이전 Trail이 켜져 있었다면 안전하게 정리하고 새 Trail은 꺼진 상태로 시작.
    /// </summary>
    public void SetTrail(Trail newTrail)
    {
        if (trail == newTrail)
        {
            // 동일 참조라도 꺼진 상태를 보장
            if (trail != null) trail.enabled = false;
            return;
        }

        // 이전 Trail 정리 (GameObject가 활성일 때만 Clear 호출 가능)
        if (trail != null)
        {
            if (trail.gameObject.activeInHierarchy && trail.enabled)
                trail.Clear();
            trail.enabled = false;
        }

        trail = newTrail;
        if (trail != null)
            trail.enabled = false;
    }

    /// <summary>무기 해제 시 Awake에서 캐싱한 기본 무기 Trail로 되돌린다.</summary>
    public void RestoreDefaultTrail()
    {
        SetTrail(defaultTrail);
    }

    /// <summary>제어 중인 Trail 참조를 해제한다 (다음 SetTrail 전까지 동작 안 함).</summary>
    public void ClearTrail()
    {
        if (trail != null)
        {
            if (trail.gameObject.activeInHierarchy && trail.enabled)
                trail.Clear();
            trail.enabled = false;
        }
        trail = null;
    }

    // Animation Event에서 호출
    public void TrailOn()
    {
        if (trail == null) return;
        if (trail.enabled) return;  // 이미 켜져 있으면 무시
        if (!trail.gameObject.activeInHierarchy) return; // 비활성 무기는 무시
        trail.enabled = true;
    }

    public void TrailOff()
    {
        if (trail == null) return;
        if (!trail.enabled) return; // 이미 꺼져 있으면 무시
        // 1안: GameObject가 비활성이면 Clear(코루틴) 호출을 건너뛴다.
        if (trail.gameObject.activeInHierarchy) trail.Clear();
        trail.enabled = false;
    }
}