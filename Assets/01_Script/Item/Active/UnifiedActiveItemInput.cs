using UnityEngine;
using StarterAssets;


public class UnifiedActiveItemInput : MonoBehaviour
{
    private StarterAssetsInputs _input;
    private UnifiedItemManager unifiedManager;
    private ItemManager legacyManager;
    private UnifiedItemPickUp pickUp;

    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        unifiedManager = GetComponent<UnifiedItemManager>();
        if (unifiedManager == null) legacyManager = GetComponent<ItemManager>();
        pickUp = GetComponent<UnifiedItemPickUp>();

        if (_input == null)
            Debug.LogError($"[{nameof(UnifiedActiveItemInput)}] StarterAssetsInputs가 없음.");
    }

    private void Update()
    {
        // 입력 권한: 오프라인은 본인, 온라인은 로컬 플레이어만
        if (!AuthorityGuard.IsLocallyControlled(gameObject)) return;
        if (_input == null) return;

        // 액티브 아이템 사용
        if (_input.useActive)
        {
            if (unifiedManager != null)
                unifiedManager.RequestUseActive();
            else if (legacyManager != null)
                legacyManager.RequestUseActive();

            _input.useActive = false;   // 입력 소비
        }

        // 아이템 픽업
        if (_input.interaction)
        {
            if (pickUp != null)
                pickUp.TryPickupNearest();

            _input.interaction = false;   // 입력 소비
        }
    }
}