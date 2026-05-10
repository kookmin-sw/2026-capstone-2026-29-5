using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 사망 ~ 리스폰 사이 본인 입력 차단.
///
/// 동작:
///  - UnifiedCharacterModel.OnDie     → StarterAssetsInputs / PlayerInput 비활성화
///  - UnifiedCharacterModel.OnRespawn → 캐싱된 enabled 값으로 복원
///
/// 본인 캐릭터(IsLocallyControlled)에서만 입력을 잠그며,
/// 원격 플레이어 인스턴스는 OnDie 가 와도 잠그지 않는다.
///
/// GameOver(목숨 소진) 시에는 OnRespawn 이 오지 않으므로 lock 상태가 유지된다.
/// 어차피 NetworkGameManger 가 게임 종료 시 모든 PlayerInput 을 끄므로 무해.
///
/// 컴포넌트는 UnifiedCharacterModel 과 같은 GameObject 에 붙이면 됨.
/// </summary>
public class DeathInputLock : MonoBehaviour
{
    [Header("참조 (비워두면 자동 검색)")]
    public UnifiedCharacterModel selfModel;

    [Header("옵션")]
    [Tooltip("StarterAssetsInputs 비활성화")]
    public bool blockStarterAssetsInputs = true;

    [Tooltip("PlayerInput(Input System) 비활성화")]
    public bool blockPlayerInput = true;

    [Tooltip("디버그 로그 출력")]
    public bool verbose = false;

    // ── 복원 캐시 ──
    private StarterAssetsInputs cachedSAI;
    private bool cachedSAIEnabled;
    private PlayerInput cachedPI;
    private bool cachedPIEnabled;
    private bool isLocked = false;

    // ─────────────────────────────────────────────
    private void Reset() { AutoBind(); }
    private void Awake() { AutoBind(); }

    private void AutoBind()
    {
        if (selfModel == null)
            selfModel = GetComponent<UnifiedCharacterModel>();
    }

    private void OnEnable()
    {
        if (selfModel != null)
        {
            selfModel.OnDie += HandleDie;
            selfModel.OnRespawn += HandleRespawn;
        }
    }

    private void OnDisable()
    {
        if (selfModel != null)
        {
            selfModel.OnDie -= HandleDie;
            selfModel.OnRespawn -= HandleRespawn;
        }

        // 안전장치: lock 상태로 컴포넌트가 꺼지면 복원
        if (isLocked) RestoreInput();
    }

    // ─────────────────────────────────────────────
    private void HandleDie()
    {
        if (!AuthorityGuard.IsLocallyControlled(gameObject)) return;
        if (isLocked) return;

        BlockInput();

        if (verbose) Debug.Log("[DeathInputLock] 사망 → 입력 차단", this);
    }

    private void HandleRespawn()
    {
        // IsLocallyControlled 체크 안 해도 됨 — isLocked 일 때만 동작
        if (!isLocked) return;

        RestoreInput();

        if (verbose) Debug.Log("[DeathInputLock] 리스폰 → 입력 복원", this);
    }

    // ─────────────────────────────────────────────
    private void BlockInput()
    {
        if (blockStarterAssetsInputs)
        {
            cachedSAI = GetComponentInChildren<StarterAssetsInputs>();
            if (cachedSAI != null)
            {
                cachedSAIEnabled = cachedSAI.enabled;
                // 누르고 있던 키 잔류 방지 (사망 직전 입력이 부활 후까지 새지 않도록)
                cachedSAI.move = Vector2.zero;
                cachedSAI.look = Vector2.zero;
                cachedSAI.jump = false;
                cachedSAI.sprint = false;
                cachedSAI.enabled = false;
            }
        }

        if (blockPlayerInput)
        {
            cachedPI = GetComponentInChildren<PlayerInput>();
            if (cachedPI != null)
            {
                cachedPIEnabled = cachedPI.enabled;
                cachedPI.enabled = false;
            }
        }

        isLocked = true;
    }

    private void RestoreInput()
    {
        if (cachedSAI != null)
        {
            cachedSAI.enabled = cachedSAIEnabled;
            cachedSAI = null;
        }
        if (cachedPI != null)
        {
            cachedPI.enabled = cachedPIEnabled;
            cachedPI = null;
        }
        isLocked = false;
    }

    /// <summary>외부에서 강제 잠금/해제 (디버그/특수 상황)</summary>
    public void ForceUnlock()
    {
        if (isLocked) RestoreInput();
    }

    public bool IsLocked => isLocked;
}
