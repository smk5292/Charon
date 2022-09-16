using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : BaseScene
{
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        UIManager.Instance.ShowSceneUI<UI_InGame>();
        UIManager.Instance.ShowSceneUI<UI_AchievementCompletionNotifier>();
        UIManager.EventHandler.AddListener(UI_EventHandler.UIEventType.ChangeScene, OnChangeScene);
    }

    public override void Clear()
    {
        
    }
}
