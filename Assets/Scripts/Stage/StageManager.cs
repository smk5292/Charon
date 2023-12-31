using System.Collections.Generic;
using UnityEngine;

public enum StageType
{
    Unknown,
    Title,
    Opening,
    Loading,
    Lobby,
    Stage11,
    Stage12,
    Stage13,
    Stage14,
    Stage15,
    Stage21,
    Ending
}

public enum MainStageType
{
    Unknown = 0,
    Stage1 = 1,
    Stage2 = 2
}

public class StageManager : MonoBehaviour
{
    private static StageManager instance;
    public static StageManager Instance
    {
        get
        {
            if (applicationQuitting)
                return null;

            Init();
            return instance;
        }
    }

    private static object _lock = new object();
    private static bool applicationQuitting = false;

    // 현재 스테이지 타입을 확인하여 몬스터를 활성화할 때 사용
    // 메인 화면에서 일시정지 UI를 띄우지 못 하도록 하는 등 SceneType의 역할도 겸함
    [SerializeField]
    private StageType currentStage = StageType.Unknown;
    public StageType CurrentStage
    {
        get { return currentStage; }
        set
        {
            currentStage = value;
            SetMainStage();
        }
    }

    [SerializeField]
    private MainStageType currentMainStage = MainStageType.Unknown;
    public MainStageType CurrentMainStage { get { return currentMainStage; } }

    // 클리어 여부를 판단하기 위해 업적 시스템 활용
    // 클리어 여부가 변화할 경우 알림을 보내, UI를 표시하거나 스테이지 장벽을 제거하는 등의 요소로 활용
    [SerializeField]
    private bool isCleared = false;
    public bool IsCleared
    {
        get { return isCleared; }
        set
        {
            bool postValue = isCleared;
            if (postValue != value)
            {
                isCleared = value;
                UIManager.EventHandler.PostNotification(UI_EventHandler.UIEventType.ChangeClear, this, isCleared);
            }
        }
    }

    [SerializeField]
    private int clearCount = 0;
    [SerializeField]
    private int currentCount = 0;
    public int CurrentCount
    {
        get { return currentCount; }
        set
        {
            currentCount = value;
            if (currentCount >= clearCount)
                IsCleared = true;
        }
    }

    [SerializeField]
    private bool isClearedByLoad = false;
    public bool IsClearedByLoad { get { return isClearedByLoad; } set { isClearedByLoad = value; } }

    // Enemy Object들을 자식으로 가지는 부모 Object
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@Enemy_Root");
            if (root == null)
                root = new GameObject("@Enemy_Root");
            return root;
        }
    }

    private void Start()
    {
        Init();
        UIManager.EventHandler.AddListener(UI_EventHandler.UIEventType.DieEnemy, OnCheckCount);
    }

    private void OnDestroy()
    {
        applicationQuitting = true;
    }

    static void Init()
    {
        lock (_lock)
        {
            if (instance == null)
            {
                GameObject stageManager = GameObject.Find("@Stage_Manager");
                if (stageManager == null)
                {
                    stageManager = new GameObject { name = "@Stage_Manager" };
                    stageManager.AddComponent<StageManager>();
                }

                DontDestroyOnLoad(stageManager);
                instance = stageManager.GetComponent<StageManager>();
            }
        }
    }

    public void SetStage()
    {
        if (currentStage == StageType.Unknown || currentStage == StageType.Lobby || currentStage == StageType.Ending || currentStage == StageType.Title || currentStage == StageType.Opening || currentStage == StageType.Loading)
            return;

        SetEnemies();
        ActiveStage(currentStage, true);
    }

    public void ActiveStage(StageType type, bool isSet = false)
    {
        CurrentStage = type;
        isCleared = false;
        clearCount = 0;
        currentCount = 0;

        if (!isSet)
        {
            if (currentStage.Equals(StageType.Ending))
            {
                FadeInOutController.Instance.FadeOutAndLoadScene("EndingScene1", StageType.Ending);
                return;
            }

            if (!GetMainStage(currentStage).Equals(GetMainStage(currentStage - 1)) && GetMainStage(currentStage - 1) != -1)
            {
                FadeInOutController.Instance.FadeOutAndLoadScene($"Stage{GetMainStage(currentStage)}Scene", StageType.Stage11);
                return;
            }
        }

        if (isClearedByLoad)
        {
            IsCleared = true;
            isClearedByLoad = false;
            return;
        }

        ActiveEnemies();
    }

    public void SetMainStage()
    {
        if (currentStage == StageType.Unknown || currentStage == StageType.Lobby || currentStage == StageType.Ending || currentStage == StageType.Title || currentStage == StageType.Opening || currentStage == StageType.Loading)
        {
            currentMainStage = MainStageType.Unknown;
            return;
        }

        string stage = currentStage.ToString();
        stage = stage.Substring(stage.Length - 2, 1);
        int stageInt = int.Parse(stage);

        currentMainStage = (MainStageType)GetMainStage(currentStage);
    }

    private void SetEnemies()
    {
        // 메인 스테이지에 맞는 Dictionary의 몬스터를 생성
        // 이를 관리할 수 있도록 spawnDictionary에 저장
        List<EnemyData> enemyData;
        DataManager.EnemyDict.TryGetValue(currentMainStage, out enemyData);

        if (enemyData == null)
            return;

        foreach (EnemyData enemy in enemyData)
        {
            GameObject root = Utils.FindChild(Root, enemy.sstage);
            if (root == null)
                root = new GameObject(enemy.sstage);
            root.transform.SetParent(Root.transform);

            GameObject go = Utils.Instantiate($"Units/{enemy.type}");
            go.transform.position = new Vector3(enemy.position[0], enemy.position[1], enemy.position[2]);
            go.SetActive(false);

            go.transform.SetParent(root.transform);
        }
    }

    private void ActiveEnemies()
    {
        if (currentStage == StageType.Unknown || currentStage == StageType.Lobby || currentStage == StageType.Ending || currentStage == StageType.Title || currentStage == StageType.Opening || currentStage == StageType.Loading)
            return;

        // 현재 스테이지에 맞는 spawnDictionary의 몬스터를 활성화
        // 약간 프레임 드랍 발생
        GameObject root = Utils.FindChild(Root, currentStage.ToString());
        if (root == null || root.GetComponentInChildren<Transform>() == null)
        {
            clearCount = 0;
            CurrentCount = clearCount;
            Debug.Log("Empty Stage.");
            return;
        }

        foreach (Transform child in root.transform)
        {
            child.gameObject.SetActive(true);
            clearCount++;
        }
    }

    public void OnCheckCount(UI_EventHandler.UIEventType eventType, Component sender, object param = null)
    {
        // 특정 몬스터에 가중치를 주거나, 보스를 판별하는 용도로 param 사용 가능
        CurrentCount++;
    }

    public int GetMainStage(StageType type)
    {
        if (type == StageType.Unknown || type == StageType.Lobby || type == StageType.Ending || type == StageType.Title || type == StageType.Opening || type == StageType.Loading)
            return -1;

        string stage = type.ToString();
        stage = stage.Substring(stage.Length - 2, 1);
        int stageInt = int.Parse(stage);

        return stageInt;
    }
}
