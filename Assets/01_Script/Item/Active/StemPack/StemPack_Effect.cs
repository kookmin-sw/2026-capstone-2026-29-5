using Mirror;
using StarterAssets;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Active/StemPack/Effect")]
public class StemPack_Effect : ScriptableObject, IActive
{
    [Header("아이템 설정")]
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private float effectDuration = 8f;
    [SerializeField] private float initialDamage = 10f;   // 사용 즉시 자해 데미지
    [SerializeField] private float moveSpeedMultiplier = 1.2f;   // 이동 속도 배율
    [SerializeField] private float animSpeedMultiplier = 1.2f; // 애니메이션 배율

    [Header("ui 이미지")]
    [SerializeField] private Sprite uiSprite;

    [Header("Network")]
    [Tooltip("효과 오브젝트 프리팹. NetworkManager의 spawnable prefabs에도 등록되어야 함.")]
    [SerializeField] private GameObject effectPrefab;

    public Sprite UISprite => uiSprite;

    [Header("Sound Settings")]
    [Tooltip("사용 시 재생되는 사운드 (랜덤 픽). owner 위치에서 PlayClipAtPoint로 재생됨.")]
    [SerializeField] private AudioClip[] useSounds;
    [SerializeField, Range(0f, 1f)] private float useVolume = 1f;

    public float AvailableTime => duration;

    public virtual IEnumerator Activate(GameObject owner)
    {
        if (owner == null) yield break;

        Debug.Log("[StemPack] 활성화 시작");

        // 사용 사운드: owner 위치에서 PlayClipAtPoint
        // (오프라인은 정상, 온라인은 서버에서만 Activate가 돌므로 호스트만 들림 — 옵션 A 한계)
        PlayUseSound(owner);

        // 사용 사운드: owner 위치에서 PlayClipAtPoint
        // (오프라인은 정상, 온라인은 서버에서만 Activate가 돌므로 호스트만 들림 — 옵션 A 한계)
        PlayUseSound(owner);

        //사용시 자해 데미지 적용
        ICharacterModel model = owner.GetComponent<ICharacterModel>();
        if (model != null)
        {
            model.RequestTakeDamage(initialDamage);
        }

        // 효과 오브젝트 스폰 또는 리프레시
        SpawnOrRefresh(owner);

        // 코루틴 자체는 즉시 종료. 라이프사이클은 효과 오브젝트가 관리.
        yield break;
    }

    private void SpawnOrRefresh(GameObject owner)
    {
        NetworkIdentity ownerNid = owner.GetComponent<NetworkIdentity>();
        uint netId = 0;

        if (!AuthorityGuard.IsOffline)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[StemPack] 서버 권한 없음.");
                return;
            }
            if (ownerNid == null)
            {
                Debug.LogWarning("[StemPack] owner에 NetworkIdentity가 없음.");
                return;
            }
            netId = ownerNid.netId;
        }

        // 이미 활성화된 효과가 있으면 리프레시
        StemPackEffect_Object existing = StemPackEffect_Object.FindActiveOn(
            AuthorityGuard.IsOffline ? owner : null,
            netId
        );
        if (existing != null)
        {
            existing.RefreshDuration(effectDuration);
            return;
        }

        // 새로 스폰
        if (effectPrefab == null)
        {
            Debug.LogError("[StemPack] effectPrefab이 비어있음.");
            return;
        }

        GameObject go = Instantiate(effectPrefab);
        StemPackEffect_Object eff = go.GetComponent<StemPackEffect_Object>();
        if (eff == null)
        {
            Debug.LogError("[StemPack] 프리팹에 StemPackEffect_Object 컴포넌트가 없음.");
            Object.Destroy(go);
            return;
        }

        //값 주입
        eff.targetNetId = netId;
        eff.duration = effectDuration;
        eff.moveSpeedMultiplier = moveSpeedMultiplier;
        eff.animSpeedMultiplier = animSpeedMultiplier;

        if (AuthorityGuard.IsOffline)
        {
            HardenOfflineObject(go);
            eff.InitializeOffline(owner, effectDuration, moveSpeedMultiplier, animSpeedMultiplier);
        }
        else
        {
            NetworkServer.Spawn(go);
        }
    }


    private static void HardenOfflineObject(GameObject obj)
    {
        if (obj == null) return;
        if (obj.TryGetComponent(out NetworkIdentity nid))
            nid.enabled = false;
        if (!obj.activeSelf) obj.SetActive(true);
    }

    protected virtual void ApplyEffect(GameObject owner) { }

    protected virtual void RemoveEffect(GameObject owner) { }
    public virtual void OnDeactivate(GameObject owner) { }


    /// <summary>
    /// 사용 사운드 재생. 클립 배열에서 랜덤 픽 → owner 위치에서 PlayClipAtPoint.
    /// </summary>
    private void PlayUseSound(GameObject owner)
    {
        if (useSounds == null || useSounds.Length == 0) return;
        if (owner == null) return;

        AudioClip clip = useSounds[Random.Range(0, useSounds.Length)];
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, owner.transform.position, useVolume);
    }
}