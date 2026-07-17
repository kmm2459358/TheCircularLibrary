using UnityEngine;
using TMPro;

public class SanityErosionCountText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private PlayerState playerState;
    private LightDarkWorld lightDarkWorld;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        playerState = GameObject.Find("PlayerModel").GetComponent<PlayerState>();
        if (GameObject.Find("LightDarkWorld") != null)
        {
            lightDarkWorld = GameObject.Find("LightDarkWorld").GetComponent<LightDarkWorld>();
        }
    }

    void Update()
    {
        if (lightDarkWorld != null)
        {
            //正気度と明暗状態を表示
            text.text = lightDarkWorld.brightnessState + " SAN : " + playerState.sanityLevel;
        }
    }
}
