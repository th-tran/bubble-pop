using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(MaskableGraphic))]
public class ScreenFader : MonoBehaviour
{
    public float solidAlpha = 1f;
    public float clearAlpha = 0f;

    public float delay = 0f;
    public float timeToFade = 1f;

    MaskableGraphic m_graphic;

    // Start is called before the first frame update
    void Start()
    {
        m_graphic = GetComponent<MaskableGraphic>();
    }

    public void FadeOn(bool fadeChildren = true)
    {
        StartCoroutine(FadeRoutine(solidAlpha));
    }

    public void FadeOff(bool fadeChildren = true)
    {
        StartCoroutine(FadeRoutine(clearAlpha));
    }

    IEnumerator FadeRoutine(float alpha, bool fadeChildren = true)
    {
        yield return new WaitForSeconds(delay);
        m_graphic.CrossFadeAlpha(alpha, timeToFade, true);

        if (fadeChildren)
        {
            MaskableGraphic[] maskableGraphics = GetComponentsInChildren<MaskableGraphic>();
            foreach (MaskableGraphic graphic in maskableGraphics)
            {
                graphic.CrossFadeAlpha(alpha, timeToFade, true);
            }
        }
    }
}
