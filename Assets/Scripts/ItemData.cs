using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        // 1. 도는 칼 (Orbit)
        Orbit_Damage,   // 데미지 증가
        Orbit_Count,    // 검 개수 증가 (최대 10)
        Orbit_Speed,    // 도는 속도 증가

        // 2. 이기어검 (Flying Sword)
        Flying_Damage,  // 데미지 증가
        Flying_Count,   // 검 개수 증가 (최대 10)
        Flying_Duration,// 지속 시간 증가 (최대 10초)

        // 3. 새끼용 (Baby Dragon)
        Dragon_Damage,  // 데미지 증가
        Dragon_Count,   // 불덩이 개수 증가 (최대 10)
        Dragon_Speed,   // 발사 속도 증가

        //4. 마법:번개 (Lightning)
        Lightning_Damage, // 데미지 증가
        Lightning_Count,  // 번개 개수 증가 (동시에 때리는 적 마리 수)
        Lightning_Range,  // 탐색 범위 증가

        // 5. 투척용 단검 (Throwing Dagger) 
        Dagger_Damage, // 데미지 증가
        Dagger_Count,  // 투척 개수 증가 (부채꼴)
        Dagger_Speed,  // 발사 속도 증가 (쿨타임 감소)

        // 4. 공통 패시브 (Passives)
        Passive_Boots,  // 이동속도 증가
        Passive_Armor,  // 방어력 증가
        Passive_Vampire,// 흡혈 확률/회복량 증가
        Passive_MaxHp,  // 최대 체력 증가
        Passive_Magnet, // 자석
        Passive_Exp, //경험치 증가

        LimitBreak_Damage, // 영구 데미지 상승
        LimitBreak_Heal,   // 체력 50% 회복
        LimitBreak_Gold,    // 100 골드 획득


        Aura_Size,
        Aura_Damage,
        Aura_Distance,

        LimitBreak_Armor,  // 방어력 1% 상승
        LimitBreak_MaxHp   //  최대 체력 1% 상승
    }

    [Header("--- 기본 정보 ---")]
    public ItemType itemType;
    public int itemId;
    public string itemName; 
    public Sprite itemIcon;

    [Header("--- 레벨 & 텍스트 ---")]
    public int maxLevel = 10;

    [TextArea]
    public string baseDesc; 

    [TextArea]
    public string[] levelUpDescs; 

    [Header("--- 핵심 수치 (레벨별) ---")]
    public float[] values;
}
