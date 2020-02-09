using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public RectTransform collectionGoalLayout;
    public int collectionGoalBaseWidth = 125;
    CollectionGoalPanel[] m_collectionGoalPanels;

    public void SetupCollectionGoalLayout(CollectionGoal[] collectionGoals)
    {
        if (collectionGoalLayout != null && collectionGoals != null && collectionGoals.Length != 0)
        {
            collectionGoalLayout.sizeDelta = new Vector2(collectionGoals.Length * collectionGoalBaseWidth, collectionGoalLayout.sizeDelta.y);
            m_collectionGoalPanels = collectionGoalLayout.gameObject.GetComponentsInChildren<CollectionGoalPanel>();

            for (int i = 0; i < m_collectionGoalPanels.Length; i++)
            {
                if (i < collectionGoals.Length && collectionGoals[i] != null)
                {
                    m_collectionGoalPanels[i].gameObject.SetActive(true);
                    m_collectionGoalPanels[i].collectionGoal = collectionGoals[i];
                    m_collectionGoalPanels[i].SetupPanel();
                }
                else
                {
                    m_collectionGoalPanels[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void UpdateCollectionGoalLayout()
    {
        foreach (CollectionGoalPanel panel in m_collectionGoalPanels)
        {
            if (panel != null && panel.gameObject.activeInHierarchy)
            {
                panel.UpdatePanel();
            }
        }
    }
}
