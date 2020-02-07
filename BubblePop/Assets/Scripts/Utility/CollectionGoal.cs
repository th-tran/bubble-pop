using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionGoal : MonoBehaviour
{
    public Bubble prefabToCollect;

    [Range(1,50)]
    public int numberToCollect = 5;

    SpriteRenderer m_spriteRenderer;

    void Awake()
    {
        if (prefabToCollect != null)
        {
            m_spriteRenderer = prefabToCollect.GetComponent<SpriteRenderer>();
        }
    }

    public void CollectBubble(Bubble bubble)
    {
        if (bubble != null)
        {
            SpriteRenderer spriteRenderer = bubble.GetComponent<SpriteRenderer>();

            if (m_spriteRenderer.sprite == spriteRenderer.sprite && prefabToCollect.matchValue == bubble.matchValue)
            {
                numberToCollect--;
                numberToCollect = Mathf.Clamp(numberToCollect, 0, numberToCollect);
            }
        }
    }
}
