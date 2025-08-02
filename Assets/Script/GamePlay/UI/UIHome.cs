using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIHome : UICanvas
{
    [SerializeField] private HomeTab[] homeTabs;
    [HideInInspector] public int currentTabIndex = 0;
    [SerializeField] TextMeshProUGUI[] levelText;
    [SerializeField] TextMeshProUGUI coinText;
    public void PlayBtn() 
    {
        DOVirtual.DelayedCall(0.25f, () =>
        {
            GameManager.Instance.FadeIn(0.25f, () => GameManager.Instance.SetUpLevel());
        });
        UIManager.Instance.OpenUI<UIGamePlay>();
        UIManager.Instance.CloseUI<UIHome>(0.5f);

    }
    private void Start()
    {
        OnTabClick(0); // Default to the first tab (home)

    }
    private void OnEnable()
    {
        UpdateLevelText();
    }
    public void OnTabClick(int index)
    {
        foreach (var tab in homeTabs)
        {
            if (tab.tabIndex == index)
            {
                tab.AnimationOn();
            }
            else
            {
                tab.AnimationOff();
            }
        }
    }

    public void SettingBtn() 
    {
        UIManager.Instance.OpenUI<UIPause>();
    }
    private void UpdateLevelText()
    {
        for (int i = 0; i < levelText.Length; i++)
        {
            if (i == 0)
            {
                levelText[i].text = $"Level {GameManager.Instance.currentLevel:00}";
            }
            else
            {
                levelText[i].text = (i + GameManager.Instance.currentLevel).ToString("00");
            }
        }
        coinText.text = GameManager.Instance.gold.ToString();
    }

    
}
