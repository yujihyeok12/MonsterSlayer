using UnityEngine;

public class ThrowingDaggerWeapon : MonoBehaviour
{
    [Header("단검 스탯")]
    public float damage = 5f;
    public int count = 1;           
    public float fireRate = 1.5f;  
    public float projectileSpeed = 15f; 
    public float spreadAngle = 15f; 
    public float spriteAngleOffset = 90f;

    [Header("연결")]
    public Player player;
    public GameObject daggerPrefab;

    private float timer;
    private Vector2 lastDir = Vector2.right;

    void Update()
    {
        if (player.inputVec != Vector2.zero)
        {
            lastDir = player.inputVec.normalized;
        }

        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            FireDaggers();
            timer = 0f;
        }
    }

    void FireDaggers()
    {
        float baseAngle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;

        SoundManager.instance.PlaySFX(SoundManager.SFX.DaggerThrow);

        for (int i = 0; i < count; i++)
        {
            float offsetAngle = 0f;
            if (count > 1)
            {
                offsetAngle = spreadAngle * (i - (count - 1) / 2f);
            }

            float finalAngle = baseAngle + offsetAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, finalAngle + spriteAngleOffset);

            GameObject dagger = Instantiate(daggerPrefab, transform.position, rotation);

            ThrowingDagger projectile = dagger.GetComponent<ThrowingDagger>();
            if (projectile != null)
            {
                projectile.damage = damage;
                projectile.speed = projectileSpeed;
                projectile.dir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
            }
        }
    }
}

/*
========================================================
[ ThrowingDaggerWeapon.cs 상세 설명서 (투척 단검 매니저)]
1. 스크립트 역할:
   - 플레이어가 바라보고 있거나 '마지막으로 이동했던 방향'을 기억해 두었다가, 쿨타임이 찰 때마다 그 방향을 향해 단검을 쫙 뿌려주는 무기 본체입니다.

2. 주요 변수:
   - count & spreadAngle: 단검 개수가 늘어났을 때, 단검들이 한 곳으로만 날아가지 않고 부채꼴(샷건) 모양으로 예쁘게 퍼지도록 만드는 각도 변수입니다.
   - lastDir: 플레이어가 가만히 서 있어도 마지막으로 걸어갔던 방향으로 단검을 던지기 위해 그 방향을 기억해 두는 나침반입니다.

3. 핵심 작동 흐름 및 함수:
   - Update(): 플레이어의 이동 키(inputVec)가 눌릴 때마다 lastDir를 갱신해 두고, 타이머(timer)가 발사 속도(fireRate)에 도달하면 FireDaggers()를 장전합니다.
   - FireDaggers() [🌟부채꼴 각도 계산의 핵심!]:
     1) Atan2 함수를 이용해 목표 방향(lastDir)의 기본 각도(baseAngle)를 구합니다.
     2) count가 여러 개일 경우, for문을 돌면서 `spreadAngle * (i - (count - 1) / 2f)` 공식을 통해 단검들이 중앙을 기준으로 양옆으로 대칭되게 각도를 틀어줍니다.
     3) 계산된 최종 각도(finalAngle)로 단검 프리팹을 소환하고, 투사체 스크립트(ThrowingDagger.cs)에 데미지, 속도, 날아갈 방향 벡터(dir)를 주입해 줍니다!
========================================================
*/