using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// プレイヤーを制御するコンポーネント
public class Player : MonoBehaviour
{
    public float m_speed; // 移動の速さ

    public Shot m_shotPrefab; // 弾のプレハブ
    public Shot m_bombShotPrefab; // ボムのプレハブ

    public float m_shotSpeed; //弾速
    public float m_shotAngleRange; //複数弾の発射角度
    public float m_shotTimer; //発射間隔
    public int m_shotCount; //弾の発射数
    public float m_shotInterval; // 弾の発射間隔（秒）
    public int m_hpMax; //HPの最大値
    public int m_hp; //現在のHP
    public static Player m_instance;
    public float m_magnetDistance; //宝石を引きつける距離
    public int m_nextExpBase;//次のレベルまでに必要な経験値の基本値
    public int m_nextExpInterval;//次のレベルまでに必要な経験値の増加値
    public int m_level;//レベル
    public int m_exp;//経験値
    public int m_prevNeedExp;//前のレベルに必要だった経験値
    public int m_needExp;//次のレベルに必要な経験値
    public int m_bombCount;//ボムの発射数
    public float m_bombSpeed;//ボムの弾速基礎値
    public AudioClip m_levelUpClip;//レベルアップの時に流す音源
    public AudioClip m_damageClip;//ダメージの時に流す音源
    public int m_levelMax;//レベルの最大値
    public int m_shotCountFrom;//弾の発射数min
    public int m_shotCountTo;//弾の発射数Max
    public float m_shotIntervalFrom;//弾の発射間隔min
    public float m_shotIntervalTo;//弾の発射間隔max
    public float m_magnetDistanceFrom;//ドロー範囲min
    public float m_magnetDistanceTo;//ドロー範囲Max
    private void Awake()
    {
        m_instance = this; //static変数にインスタンス情報を格納
        m_hp = m_hpMax; //HP
        m_level = 1;//レベル
        m_needExp = GetNeedExp(1);//次のレベルに必要な経験値
        m_shotCount = m_shotCountFrom;//弾の発射数
        m_shotInterval = m_shotIntervalFrom;//弾の発射間隔
        m_magnetDistance = m_magnetDistanceFrom;//ドロー範囲
    }
    // 毎フレーム呼び出される関数
    private void Update()
    {
        // 矢印キーの入力情報を取得する
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        // 矢印キーが押されている方向にプレイヤーを移動する
        var velocity = new Vector3(h, v) * m_speed;
        transform.localPosition += velocity;
        transform.localPosition = Utils.ClampPosition(transform.localPosition);

        // プレイヤーのスクリーン座標を計算
        var screenPos = Camera.main.WorldToScreenPoint(transform.position);

        // プレイヤーから見たマウスカーソルの方向を計算する
        var direction = Input.mousePosition - screenPos;

        //　マウスカーソルが存在する方向の角度を取得する
        var angle = Utils.GetAngle(Vector3.zero, direction);

        // プレイヤーがマウスカーソルの方向を見るようにする
        var angles = transform.localEulerAngles;
        angles.z = angle - 90;
        transform.localEulerAngles = angles;


        // 弾の発射タイミングを管理するタイマーの更新
        m_shotTimer += Time.deltaTime;

        if (m_shotTimer < m_shotInterval) return;

        // 弾の発射タイミングタイマーリセット
        m_shotTimer = 0;

        // 弾を発射する
        ShootNWay(angle, m_shotAngleRange, m_shotSpeed, m_shotCount, m_shotPrefab);

    }

    // 弾を発射する関数
    private void ShootNWay(
    float angleBase, float angleRange, float speed, int count, Shot sp)
    {
        var pos = transform.localPosition; // プレイヤーの位置
        var rot = transform.localRotation; // プレイヤーの向き

        // 弾を複数発射する場合
        if (1 < count)
        {
            // 発射する回数分ループする
            for (int i = 0; i < count; ++i)
            {
                // 弾の発射角度を計算する
                var angle = angleBase +
                    angleRange * ((float)i / (count - 1) - 0.5f);

                // 発射する弾を生成する
                var shot = Instantiate(sp, pos, rot);

                // 弾を発射する方向と速さを設定する
                shot.Init(angle, speed);
            }
        }
        // 弾を 1 つだけ発射する場合
        else if (count == 1)
        {
            // 発射する弾を生成する
            var shot = Instantiate(sp, pos, rot);

            // 弾を発射する方向と速さを設定する
            shot.Init(angleBase, speed);
        }
    }

    //ダメージを受ける関数
    public void Damage(int damage)
    {
        m_hp -= damage;
        if (0 < m_hp) return;

        //プレイヤーの死亡
        gameObject.SetActive(false);
    }
    public void AddExp(int exp)
    {
        //経験値を増やす
        m_exp += exp;
        //レベルアップに必要な経験値が足りていない場合ここで終わる
        if (m_exp < m_needExp) return;
        m_level++;
        //今回のレベルアップに必要だった経験値の記憶
        m_prevNeedExp = m_needExp;
        //次のレベルアップに必要な経験値の計算
        m_needExp = GetNeedExp(m_level);

        //レベルアップした時にボムを発動
        var angleBase = 0;
        var angleRange = 360;
        var count = m_bombCount;

        ShootNWay(angleBase, angleRange, 1.00f * m_bombSpeed, count, m_bombShotPrefab);
        ShootNWay(angleBase, angleRange, 1.25f * m_bombSpeed, count, m_bombShotPrefab);
        ShootNWay(angleBase, angleRange, 0.75f * m_bombSpeed, count, m_bombShotPrefab);

        //レベルアップ時のSEを再生
        var audioSource = FindObjectOfType<AudioSource>();
        audioSource.PlayOneShot(m_levelUpClip);
        //レベルアップによるパラメータ更新
        var t = (float)(m_level - 1) / (m_levelMax - 1);
        m_shotCount = Mathf.RoundToInt(
            Mathf.Lerp(m_shotCountFrom, m_shotCountTo, t)
        );
        m_shotInterval = Mathf.Lerp(m_shotIntervalFrom, m_shotIntervalTo, t);
        m_magnetDistance = Mathf.Lerp(m_magnetDistanceFrom, m_magnetDistanceTo, t);
    }
    private int GetNeedExp(int level)
    {
        return m_nextExpBase + m_nextExpInterval * ((level - 1) * 2);
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.name.Contains("SukeruArea"))
        {
            Debug.Log("OnTriggerStay2D:" + collision.gameObject.name);
        }
    }
}