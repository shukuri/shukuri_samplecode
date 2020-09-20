using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour
{
    private Vector3 m_velocity; //弾速

    private void Update()
    {
        transform.localPosition += m_velocity; //弾の位置を加算する
    }

    public void Init(float angle, float speed)
    {
        var direction = Utils.GetDirection(angle); //弾の角度をベクトルに変換して格納

        m_velocity = direction * speed; //ベクトルに速さをかけ弾速を算出

        //弾が進行方向を向くようにする
        var angles = transform.localEulerAngles;
        angles.z = angle - 90;
        transform.localEulerAngles = angles;

        Destroy(gameObject, 2); //２秒後に弾が消滅
    }

}
