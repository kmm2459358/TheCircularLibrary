#if UNITY_EDITOR
using NUnit.Framework;
#endif
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LightDarkWorld : MonoBehaviour
{
    private GameObject player;
    private PlayerState playerState;
    private Light worldLight;  //世界の光源
    private MeshRenderer backGround;  //背景のマテリアルの色
    [SerializeField] private Image uiBackground;  //BuddyUIの背景
    [SerializeField] private TextMeshProUGUI text;  //光闇状態表示用テキスト

    public enum brightness {Dark, Light};  //光と闇
    public brightness brightnessState = brightness.Dark;  //現在の世界の輝度

    //private float lightDuration = 15f;     //光の継続時間
    //private float lightTimer = 0f;         //光の世界の時間private
    private float transparency = 0.3f;     //白と黒の床壁の透明度
    private List<GameObject> whiteGroup = new List<GameObject>();
    private List<GameObject> blackGroup = new List<GameObject>();

    IEnumerator Start()
    {
        player = GameObject.Find("PlayerModel");
        if (player != null)
        {
            playerState = player.GetComponent<PlayerState>();
        }

        GameObject lightObj = GameObject.Find("Directional Light");  //光源オブジェクト
        if (lightObj != null)
        {
            worldLight = lightObj.GetComponent<Light>();
        }

        GameObject bgObj = GameObject.Find("StageBackGround");  //背景オブジェクト
        if (bgObj != null)
        {
            backGround = bgObj.GetComponent<MeshRenderer>();
        }

        //白い床
        AddRange(whiteGroup, GameObject.FindGameObjectsWithTag("LightWhite"));

        //黒い床
        AddRange(blackGroup, GameObject.FindGameObjectsWithTag("DarkBlack"));

        ////StalkerHand
        //var stalkers = GameObject.FindGameObjectsWithTag("StalkerHand");

        //int whiteLayer = LayerMask.NameToLayer("WhiteOther");
        //int blackLayer = LayerMask.NameToLayer("BlackOther");

        //foreach (var s in stalkers)
        //{
        //    if (s.layer == whiteLayer)
        //    {
        //        whiteGroup.Add(s);
        //    }
        //    else if (s.layer == blackLayer)
        //    {
        //        blackGroup.Add(s);
        //    }
        //}
        
        yield return null; // 他のスクリプトのStart処理が完了するのを1フレーム待つ

        LayerChange(false);
    }

    //配列をリストに追加する
    private void AddRange(List<GameObject> list, GameObject[] objs)
    {
        foreach (var o in objs)
        {
            list.Add(o);
        }
    }

    void Update()
    {
        //光と闇切り替え
        if (Input.GetMouseButtonDown(1))
        {
            if (brightnessState == brightness.Dark)  //闇→光
            {
                LightDarkChange(brightness.Light);
            }
            else  //光→闇
            {
                LightDarkChange(brightness.Dark);
            }
        }

        //光闇世界の違い
        //if (brightnessState == brightness.Dark)  //闇の世界
        //{

        //}
        //else  //光の世界
        //{
        //    lightTimer -= Time.deltaTime;
        //    if (lightTimer <= 0)
        //    {
        //        LightDarkChange(brightness.Dark);
        //    }
        //}
    }

    //光と闇の切り替え
    private void LightDarkChange(brightness s)
    {
        if (brightnessState == brightness.Dark && s == brightness.Light)  //闇→光
        {
            brightnessState = brightness.Light;
            //lightTimer = lightDuration;
            if (text != null) text.color = Color.black;
            LayerChange(true);
        }
        else if (brightnessState == brightness.Light && s == brightness.Dark)  //光→闇
        {
            brightnessState = brightness.Dark;
            if (text != null) text.color = Color.white;
            LayerChange(false);
        }
    }

    //リスポーン時に闇の状態に戻す
    public void ResetToDarkState()
    {
        brightnessState = brightness.Dark;
        if (text != null) text.color = Color.white;
        LayerChange(false);
    }

    //光闇切り替えに伴うレイヤーの当たり判定変更＋オブジェクトの透明度変化
    private void LayerChange(bool isLight)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int buddyLayer = LayerMask.NameToLayer("Buddy");
        int bombLayer = LayerMask.NameToLayer("Bomb");
        int groundLayer = LayerMask.NameToLayer("Ground");
        int whiteGroundLayer = LayerMask.NameToLayer("WhiteGround");
        int blackGroundLayer = LayerMask.NameToLayer("BlackGround");
        int whiteOtherLayer = LayerMask.NameToLayer("WhiteOther");
        int blackOtherLayer = LayerMask.NameToLayer("BlackOther");

        int[] target = { playerLayer, buddyLayer, bombLayer };  //動く側のレイヤー
        (int layer, bool whatBrightness)[] obj = {
            (whiteGroundLayer, true),
            (blackGroundLayer, false),
            (whiteOtherLayer, true),
            (blackOtherLayer, false)
        };  //白黒のレイヤー

        //物理的な当たり判定制御
        foreach (int t in target)
        {
            if (t == -1) continue;
            foreach (var (lay, what) in obj)
            {
                if (lay != -1)
                {
                    Physics.IgnoreLayerCollision(t, lay, isLight == what);
                }
            }
        }

        Color lightColor = new Color(195f / 255f, 195f / 255f, 190f / 255f, 1f);  //光の背景の色
        Color darkColor = new Color(85f / 255f, 85f / 255f, 85f / 255f, 1f);  //闇の背景の色

        //判定用LayerMaskの設定
        if (!isLight)
        {
            if (playerState != null && groundLayer != -1 && whiteGroundLayer != -1)
            {
                playerState.groundLayerMask = (1 << groundLayer) | (1 << whiteGroundLayer);
            }

            //白系黒系の透明度変化（黒を半透明に）
            ObjectTransparency(whiteGroup, 1f);
            ObjectTransparency(blackGroup, transparency);
            if (worldLight != null) worldLight.color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1f);  //闇のときの光源の色
            if (backGround != null) backGround.material.color = darkColor;
            if (uiBackground != null) uiBackground.color = darkColor;
        }
        else
        {
            if (playerState != null && groundLayer != -1 && blackGroundLayer != -1)
            {
                playerState.groundLayerMask = (1 << groundLayer) | (1 << blackGroundLayer);
            }

            //白系黒系の透明度変化（白を半透明に）
            ObjectTransparency(whiteGroup, 0.1f);
            ObjectTransparency(blackGroup, 1f);
            if (worldLight != null) worldLight.color = Color.white;  //光のときの光源の色
            if (backGround != null) backGround.material.color = lightColor;
            if (uiBackground != null) uiBackground.color = lightColor;
        }
    }

    //白系黒系の透明度変化
    private void ObjectTransparency(List<GameObject> objs, float tp)
    {
        foreach (GameObject obj in objs)
        {
            if (obj == null) continue;

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr == null) continue;

            Color c = mr.material.color;
            mr.material.color = new Color(c.r, c.g, c.b, tp);
        }
    }

    //途中で追加された白黒オブジェクトの仕分け
    public void RegisterObject(GameObject obj)
    {
        int whiteLayer = LayerMask.NameToLayer("WhiteOther");
        int blackLayer = LayerMask.NameToLayer("BlackOther");
        
        //リストに追加
        if (obj.layer == whiteLayer)
        {
            whiteGroup.Add(obj);
        }
        else if (obj.layer == blackLayer)
        {
            blackGroup.Add(obj);
        }

        //今の光闇状態に合わせて透明度反映
        if (brightnessState == brightness.Light)
        {
            if (obj.layer == whiteLayer)
                ObjectTransparency(new List<GameObject>() { obj }, transparency);
            else if (obj.layer == blackLayer)
                ObjectTransparency(new List<GameObject>() { obj }, 1f);
        }
        else
        {
            if (obj.layer == whiteLayer)
                ObjectTransparency(new List<GameObject>() { obj }, 1f);
            else if (obj.layer == blackLayer)
                ObjectTransparency(new List<GameObject>() { obj }, transparency);
        }
    }

    //途中で削除された白黒オブジェクトのリストからの除外
    public void UnregisterObject(GameObject obj)
    {
        whiteGroup.Remove(obj);
        blackGroup.Remove(obj);
    }
}
