using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum RESPAWN_TYPE
{
    UP, //上
    RIGHT, //右
    DOWN, //下
    LEFT, //左
    SIZEOF, //敵の出現位置の数
}

public class Enemy : MonoBehaviour
{
    public Vector2 m_respawnPosInside; // 敵の出現位置（内側）
    public Vector2 m_respawnPosOutside; // 敵の出現位置（外側）
    public float m_speed; // 移動する速さ
    public int m_hpMax; //HPの最大値
    public int m_exp; //獲得経験値
    public int m_damage; //エネミーの攻撃力
    public Explosion m_explosionPrefab; //爆発エフェクトのプレハブ
    private int m_hp; // HP
    private Vector3 m_direction; //進行方向
    public bool m_isFollow; //プレイヤーを追尾する場合true
    public bool m_isRound;//回転移動してくる場合true
    public Gem[] m_gemPrefabs; //宝石のプレハブの管理
    public float m_gemSpeedMin; //生成する宝石の移動の速さ（最小値）
    public float m_gemSpeedMax; //生成する宝石の移動の速さ（最大値）
    public AudioClip m_deathClip;//敵を倒した時に再生するSE

    //敵の生成時に呼び出される関数
    private void Start()
    {
        // HPの初期化
        m_hp = m_hpMax;
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_isFollow)
        {
            // プレイヤーの現在位置へ向かうベクトルの作成
            var angle = Utils.GetAngle(
                transform.localPosition,
                Player.m_instance.transform.localPosition);
            var direction = Utils.GetDirection(angle);
            //プレイヤーが存在する方向に移動する
            transform.localPosition += direction * m_speed;
            //プレイヤーが存在する方向を向く
            var angles = transform.localEulerAngles;
            angles.z = angle - 90;
            transform.localEulerAngles = angles;
            return;
        }
        if (m_isRound)
        {
            // 中心に向かうベクトルの作成
            var angle = Utils.GetAngle(transform.localPosition, new Vector2(0, 0));
            var direction = Utils.GetDirection(angle);
            transform.localPosition += direction * m_speed;
            var angles = transform.localEulerAngles;
            angles.z = angle;
            transform.localEulerAngles = angles;
            return;
        }

        //まっすぐ移動する
        transform.localPosition += m_direction * m_speed;
    }

    //敵が出現時に初期化する関数
    public void Init(RESPAWN_TYPE respawntype)
    {
        var pos = Vector3.zero;

        //指定された出現位置の種類に応じて出現位置と進行方向を決定
        switch (respawntype)
        {
            //出現位置が上の場合
            case RESPAWN_TYPE.UP:
                pos.x = Random.Range(-m_respawnPosInside.x, m_respawnPosInside.x);
                pos.y = m_respawnPosOutside.y;
                m_direction = Vector2.down;
                break;
            //出現位置が右の場合
            case RESPAWN_TYPE.RIGHT:
                pos.x = m_respawnPosOutside.x;
                pos.y = Random.Range(-m_respawnPosInside.y, m_respawnPosInside.y);
                m_direction = Vector2.left;
                break;
            //出現位置が下の場合
            case RESPAWN_TYPE.DOWN:
                pos.x = Random.Range(-m_respawnPosInside.x, m_respawnPosInside.x);
                pos.y = -m_respawnPosOutside.y;
                m_direction = Vector2.up;
                break;
            //出現位置が左の場合
            case RESPAWN_TYPE.LEFT:
                pos.x = -m_respawnPosOutside.x;
                pos.y = Random.Range(-m_respawnPosInside.y, m_respawnPosInside.y);
                m_direction = Vector2.right;
                break;
        }
        //位置を反映する
        transform.localPosition = pos;
    }
    //他のオブジェクトと衝突した時に呼び出される関数
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //弾と衝突した場合
        if (collision.name.Contains("Shot"))
        {
            //爆発エフェクトを再生する
            Instantiate(
                m_explosionPrefab,
                collision.transform.localPosition,
                Quaternion.identity);
            //弾を削除する
            Destroy(collision.gameObject);
            //敵のHPを減らす
            m_hp--;
            //敵のHPがまだ残っている場合はここで処理を終える
            if (0 < m_hp) return;
            //敵を倒したときのSEを再生
            var audioSource = FindObjectOfType<AudioSource>();
            audioSource.PlayOneShot(m_deathClip);
            //敵を削除する
            Destroy(gameObject);
            /*経験値４の時４、２＋１＋１、１＋１＋１＋１のいずれかでいずれかで落ちる */
            var exp = m_exp;
            while (0 < exp)
            {
                //生成可能な宝石を配列で取得
                var gemPrefabs = m_gemPrefabs.Where(c => c.m_exp <= exp).ToArray();
                //生成可能な宝石の配列から生成する宝石をランダムに決定する
                var gemPrefab = gemPrefabs[Random.Range(0, gemPrefabs.Length)];
                //敵の位置に宝石を生成する
                var gem = Instantiate(
                    gemPrefab, transform.localPosition, Quaternion.identity);
                //宝石を初期化する
                gem.Init(m_exp, m_gemSpeedMin, m_gemSpeedMax);
                //まだ宝石を生成できるかどうか計算する
                exp -= gem.m_exp;

            }
        }

        //プレイヤーと衝突した場合
        if (collision.name.Contains("Player"))
        {
            //プレイヤーにダメージを与える
            var player = collision.GetComponent<Player>();
            player.Damage(m_damage);
            return;
        }
    }
}
