using StarterAssets;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Active/StemPack/Effect")]
public class StemPack_Effect : ScriptableObject, IActive
{
    [Header("아이템 설정")]
    [SerializeField] private float duration = 8f;
    [SerializeField] private float initialDamage = 10f;   // 사용 즉시 자해 데미지

    [SerializeField] private float moveSpeedMultiplier = 1.2f;   // 이동 속도 배율
    [SerializeField] private float animSpeedMultiplier = 1.2f; // 애니메이션 배율

    [SerializeField] private Sprite uiSprite;
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

        ICharacterModel model = owner.GetComponent<ICharacterModel>();
        if (model != null)
        {
            model.RequestTakeDamage(initialDamage);
        }

        //이동속도 조정
        UnifiedThirdPersonController controller = owner.GetComponent<UnifiedThirdPersonController>();
        float originalMoveMul = 1f;
        bool moveApplied = false;

        if (controller != null)
        {
            originalMoveMul = controller.GetSpeedMultiplier();
            controller.SetSpeedMultiplier(originalMoveMul * moveSpeedMultiplier);
            moveApplied = true;
        }

        //애니메이션 속도 조정
        Animator animator = owner.GetComponentInChildren<Animator>();
        float originalAnimSpeed = 1f;
        bool animApplied = false;

        if (animator != null)
        {
            originalAnimSpeed = animator.speed;
            controller.RequestSetAnimatorSpeed(originalAnimSpeed * animSpeedMultiplier);
            animApplied = true;
        }

        yield return new WaitForSeconds(duration);


        if (moveApplied && controller != null)
        {
            controller.SetSpeedMultiplier(originalMoveMul);
        }
        if (animApplied && animator != null)
        {
            controller.RequestSetAnimatorSpeed(originalAnimSpeed);
        }

        Debug.Log("[StemPack] 활성화 종료");
    }

    public virtual void OnDeactivate(GameObject owner)
    {
        if (owner == null) return;
        RemoveEffect(owner);
    }

    protected virtual void ApplyEffect(GameObject owner) { }

    protected virtual void RemoveEffect(GameObject owner)
    {
        UnifiedThirdPersonController controller = owner.GetComponent<UnifiedThirdPersonController>();
        if (controller != null && moveSpeedMultiplier > 0f)
        {
            controller.SetSpeedMultiplier(controller.GetSpeedMultiplier() / moveSpeedMultiplier);
        }

        Animator animator = owner.GetComponentInChildren<Animator>();
        if (controller != null && animator != null && animSpeedMultiplier > 0f)
        {
            controller.RequestSetAnimatorSpeed(animator.speed / animSpeedMultiplier);
        }
    }

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