using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Bubble))]
public class TimeBonus : MonoBehaviour
{
    [Range(0,5)]
    public int bonusValue = 5;

    [Range(0f,1f)]
    public float chanceForBonus = 0.1f;

    public GameObject bonusGlow;
    public GameObject ringGlow;

    void Start()
    {
        float random = Random.Range(0f, 1f);

        if (random > chanceForBonus)
        {
            bonusValue = 0;
        }

        SetActive(bonusValue > 0);
    }

    void SetActive(bool state)
    {
        if (bonusGlow != null)
        {
            bonusGlow.SetActive(state);
        }

        if (ringGlow != null)
        {
            ringGlow.SetActive(state);
        }
    }
}
