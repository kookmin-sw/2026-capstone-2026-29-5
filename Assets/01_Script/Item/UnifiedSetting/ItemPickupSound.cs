using UnityEngine;

/// <summary>
/// 필드 아이템 오브젝트에 부착해서 픽업(파괴) 시 사운드를 재생.
///
/// 동작 방식:
/// - <see cref="OnDestroy"/> 시점에 <see cref="AudioSource.PlayClipAtPoint"/>로 임시 AudioSource를 생성.
/// - 자기 자신(필드 아이템)은 <see cref="UnifiedSetItem"/>의 Save 흐름에서 Destroy되지만,
///   PlayClipAtPoint가 만든 AudioSource는 사운드가 끝나면 자동 정리되므로 끊기지 않음.
///
/// 가드:
/// - 씬 언로드 / 어플리케이션 종료 시에는 재생하지 않음 (오발 방지).
///
/// 사용:
/// - 필드 아이템 프리팹 (Battery 등) 에 이 컴포넌트 부착.
/// - 인스펙터에서 pickupSounds 배열에 효과음 클립 드래그.
///
/// 한계:
/// - 픽업이 아닌 이유로 GameObject가 파괴돼도 (예: 스포너에 의한 미획득 만료) 사운드가 재생됨.
///   Battery 같은 일반 픽업 아이템에서는 실용상 문제 없음.
///   엄격히 분리하려면 외부에서 호출하는 명시적 메소드 패턴으로 바꿔야 함.
/// </summary>
public class ItemPickupSound : MonoBehaviour
{
    [Tooltip("픽업 시 재생되는 사운드 (랜덤 픽)")]
    public AudioClip[] pickupSounds;
    [Range(0f, 1f)] public float pickupVolume = 1f;

    private void OnDestroy()
    {
        // 어플리케이션 종료 / 씬 언로드 시 재생 안 함
        if (!Application.isPlaying) return;
        if (!gameObject.scene.isLoaded) return;

        if (pickupSounds == null || pickupSounds.Length == 0) return;

        AudioClip clip = pickupSounds[Random.Range(0, pickupSounds.Length)];
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, transform.position, pickupVolume);
    }
}
