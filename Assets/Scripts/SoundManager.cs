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
