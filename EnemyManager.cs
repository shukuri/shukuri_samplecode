using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public Enemy[] m_enemyPrefabs; //エネミープレハブを管理する配列
    public float m_intervalFrom; //出現間隔（秒）０の時
    public float m_intervalTo; //出現間隔（秒）m_elapsedTimeの時
    public float m_elapsedTimeMax; //経過時間の最大値
    public float m_elapsedTime; //経過時間
    private float m_timer; //出現タイミングを管理するタイマー

    //毎フレーム呼び出される関数
    private void Update()
    {
        //出現管理タイマーの更新
        m_elapsedTime += Time.deltaTime;
        //出現タイミングを管理するタイマーを更新する
        m_timer += Time.deltaTime;
        //ゲームの経過時間から出現間隔を算出する
        //ゲームの経過時間が長くなるほど敵の出現間隔が短くなる
        var t = m_elapsedTime / m_elapsedTimeMax;
        var interval = Mathf.Lerp(m_intervalFrom, m_intervalTo, t);
        //まだ敵が出現するタイミングではない場合ここで終わる
        if (m_timer < interval) return;

        //出現タイミングの管理タイマーリセット
        m_timer = 0;

        //出現する敵をランダムに決定
        var enemyIndex = Random.Range(0, m_enemyPrefabs.Length);

        //出現する敵のプレハブを配列から取得する
        var enemyPrefab = m_enemyPrefabs[enemyIndex];

        //敵のゲームオブジェクトを生成する
        var enemy = Instantiate(enemyPrefab);

        //敵を画面外のどの位置に出現させるかランダムに決定
        var respawnType = (RESPAWN_TYPE)Random.Range(
            0, (int)RESPAWN_TYPE.SIZEOF);

        //敵を初期化する
        enemy.Init(respawnType);
    }
}
