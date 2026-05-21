using UnityEngine;

// Quản lý nhạc nền (BGM) và hiệu ứng âm thanh (SFX) cho toàn game.
// Gán clip cho các trường trong Inspector. AudioSources sẽ được tự tạo nếu chưa có.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioClip prepBGM;
    public AudioClip combatBGM;
    [Range(0f, 1f)] public float bgmVolume = 0.6f;

    [Header("SFX Clips")]
    public AudioClip sfxBuy;
    public AudioClip sfxMoveCard;
    public AudioClip sfxRoll;
    public AudioClip sfxFreeze;
    public AudioClip sfxBattleStart;
    public AudioClip sfxWin;
    public AudioClip sfxLose;
    public AudioClip sfxAttack;
    public AudioClip sfxDestroyed;
    public AudioClip sfxReborn;
    public AudioClip sfxSpell;
    public AudioClip sfxStarUp;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    public void PlayPrepBGM()    => PlayBGM(prepBGM);
    public void PlayCombatBGM()  => PlayBGM(combatBGM);

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.Stop();
        bgmSource.clip   = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.loop   = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // Shortcuts để code gọi dễ đọc hơn
    public void Buy()         => PlaySFX(sfxBuy);
    public void MoveCard()    => PlaySFX(sfxMoveCard);
    public void Roll()        => PlaySFX(sfxRoll);
    public void Freeze()      => PlaySFX(sfxFreeze);
    public void BattleStart() => PlaySFX(sfxBattleStart);
    // Win/Lose dùng PlayClipAtPoint để chắc chắn phát được kể cả khi AudioManager bị
    // disable, GameObject bị tắt, hoặc StopAllCoroutines vừa được gọi trên manager nào đó.
    public void Win()  => PlayDetached(sfxWin);
    public void Lose() => PlayDetached(sfxLose);

    private void PlayDetached(AudioClip clip)
    {
        if (clip == null) return;
        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, pos, sfxVolume);
    }
    public void Attack()      => PlaySFX(sfxAttack);
    public void Destroyed()   => PlaySFX(sfxDestroyed);
    public void Reborn()      => PlaySFX(sfxReborn);
    public void Spell()       => PlaySFX(sfxSpell);
    public void StarUp()      => PlaySFX(sfxStarUp);
}
