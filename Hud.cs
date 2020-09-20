using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    public Image m_hpGauge; //HPゲージ
    public Image m_expGauge;//経験値ゲージ
    public Text m_levelText;//レベルテキスト
    public GameObject m_gameOverText;//ゲームオーバーのテキスト


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        //プレイヤーを取得
        var player = Player.m_instance;
        //HPゲージ表示を最新にする
        var hp = player.m_hp;
        var hpMax = player.m_hpMax;
        m_hpGauge.fillAmount = (float)hp / hpMax;
        //経験値ゲージの表示を更新する
        var exp = player.m_exp;
        var needExp = player.m_needExp;
        var prevNeedExp = player.m_prevNeedExp;
        m_expGauge.fillAmount = (float)(exp - prevNeedExp) / (needExp - prevNeedExp);
        m_levelText.text = player.m_level.ToString();
        //プレイヤーが非表示ならゲームオーバーと表示
        m_gameOverText.SetActive(!player.gameObject.activeSelf);
    }
}
