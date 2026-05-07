using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("BGM (배경음악) 설정")]
    public AudioClip bgmClip;
    public AudioClip bossBgmClip;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    private AudioSource bgmPlayer;

    [Header("SFX (효과음) 설정")]
    public Sound[] sfxSounds;
    public int channels = 16;
    private AudioSource[] sfxPlayers;

    public enum SFX
    {
        PlayerMove,    // 0
        DragonFire,    // 1
        FlyingSword,   // 2
        DaggerThrow,   // 3
        Lightning,     // 4
        GetGold,       // 5
        GetHeal,       // 6
        MonsterHit,    // 7
        LevelUp,       // 8
        PlayerDead,    // 9
        Click,         // 10
        ChestOpen,     // 11
        GameWin,       // 12
        GameLose       // 13
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        Init();
    }

    void Init()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < channels; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
        }
    }

    public void PlayBGM(bool isBoss = false)
    {
        bgmPlayer.clip = isBoss ? bossBgmClip : bgmClip;
        if (bgmPlayer.clip != null) bgmPlayer.Play();
    }

    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    public void PlaySFX(SFX sfx)
    {
        int index = (int)sfx;
        if (index >= sfxSounds.Length || sfxSounds[index].clip == null) return;

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            if (!sfxPlayers[i].isPlaying)
            {
                sfxPlayers[i].clip = sfxSounds[index].clip;
                sfxPlayers[i].volume = sfxSounds[index].volume;
                sfxPlayers[i].Play();
                return;
            }
        }
    }
}

/*
========================================================
[ SoundManager.cs 상세 설명서 (사운드 종합 감독)]
1. 스크립트 역할:
   - BGM과 효과음을 총괄하며, 효과음이 여러 개 겹칠 때 소리가 씹히지 않게 다중 채널을 관리하는 싱글톤(어디서든 부를 수 있는 전역 객체)입니다.

2. 주요 변수 및 원리:
   - sfxPlayers: 효과음을 낼 '스피커(AudioSource)'를 미리 channels(예: 16개) 개수만큼 만들어 둔 배열입니다.
   - PlaySFX(SFX sfx): 타격음이나 레벨업 소리를 재생하라는 명령이 들어오면, 16개의 스피커 중 현재 소리가 안 나고 쉬고 있는(!isPlaying) 스피커를 하나 재빨리 찾아서 그곳에 효과음을 할당하고 Play() 합니다. 다 쓰고 있으면 그냥 무시합니다.
========================================================
*/