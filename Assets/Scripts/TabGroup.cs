
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public TabButton selectedTab;


    public void Subscribe(TabButton button)
    {
        if(tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }

        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        button.background.sprite = tabHover;
    }
    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.sprite = tabActive;
    }
    public void ResetTabs()
    {
        foreach(TabButton button in tabButtons)
        {
            button.background.sprite = tabIdle;
        }
    }
}
