
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
    public List<GameObject> objectsToSwap;
    //Track multiple pages per tab
    public int currentTabIndex = 0;
    public int currentPageIndex = 0;
    public List<List<GameObject>> tabPages = new List<List<GameObject>>();

    [Header("Navigation Buttons")]
    public Button nextButton;
    public Button prevButton;

    void Start()
    {
        // NEW: Initialize tab pages structure
        InitializeTabPages();

        // NEW: Set up navigation buttons
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);

        UpdateNavigationButtons();
    }

    void InitializeTabPages()
    {
        // Clear existing
        tabPages.Clear();

        // For each tab, create a list of pages
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            List<GameObject> pages = new List<GameObject>();

            // Get all child pages of this tab object
            Transform tabTransform = objectsToSwap[i].transform;
            for (int j = 0; j < tabTransform.childCount; j++)
            {
                pages.Add(tabTransform.GetChild(j).gameObject);
            }

            tabPages.Add(pages);
        }

        // Hide all pages initially
        HideAllPages();
    }

    // NEW: Hide all pages across all tabs
    void HideAllPages()
    {
        foreach (var pages in tabPages)
        {
            foreach (var page in pages)
            {
                page.SetActive(false);
            }
        }
    }

    // NEW: Show specific page for current tab
    void ShowCurrentPage()
    {
        if (currentTabIndex < tabPages.Count && currentPageIndex < tabPages[currentTabIndex].Count)
        {
            tabPages[currentTabIndex][currentPageIndex].SetActive(true);
        }
    }

    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }

        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if (selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
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
        int index = button.transform.GetSiblingIndex();
        currentTabIndex = index;
        currentPageIndex = 0; // Reset to first page when switching tabs
        HideAllPages();
        ShowCurrentPage();
        UpdateNavigationButtons();

        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            if (i == index)
            {
                objectsToSwap[i].SetActive(true);
            }
            else
            {
                objectsToSwap[i].SetActive(false);
            }
        }
    }
    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab) { continue; }
            button.background.sprite = tabIdle;
        }
    }
    public void NextPage()
    {
        if (tabPages.Count == 0) return;

        List<GameObject> currentTab = tabPages[currentTabIndex];
        if (currentPageIndex < currentTab.Count - 1)
        {
            currentTab[currentPageIndex].SetActive(false);
            currentPageIndex++;
            currentTab[currentPageIndex].SetActive(true);

            Debug.Log($"Next Page: {currentTab[currentPageIndex].name}");
        }

        UpdateNavigationButtons();
    }
    public void PrevPage()
    {
        if (tabPages.Count == 0) return;

        List<GameObject> currentTab = tabPages[currentTabIndex];
        if (currentPageIndex > 0)
        {
            currentTab[currentPageIndex].SetActive(false);
            currentPageIndex--;
            currentTab[currentPageIndex].SetActive(true);

            Debug.Log($"Previous Page: {currentTab[currentPageIndex].name}");
        }

        UpdateNavigationButtons();
    }
    void UpdateNavigationButtons()
    {
        if (tabPages.Count == 0) return;
        
        List<GameObject> currentTab = tabPages[currentTabIndex];
        
        // Enable/disable buttons based on current page position
        if (prevButton != null)
            prevButton.interactable = (currentPageIndex > 0);
        
        if (nextButton != null)
            nextButton.interactable = (currentPageIndex < currentTab.Count - 1);
    }

}
