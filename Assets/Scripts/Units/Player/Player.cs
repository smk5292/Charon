using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterController;

public class Player : MonoBehaviour
{
    public static Player Instance { get { return instance; } }
    public WeaponManager weaponManager { get; private set; }
    public StateMachine stateMachine { get; private set; }

    public Rigidbody rigidBody { get; private set; }
    public Animator animator { get; private set; }
    public CapsuleCollider capsuleCollider { get; private set; }

    [SerializeField]
    private Transform rightHand;
    private static Player instance;


    #region #ĳ���� ����
    public float MaxHP     { get { return maxHP; } }
    public float CurrentHP { get { return currentHP; } }
    public float Armor     { get { return armor; } }
    public float MoveSpeed { get { return moveSpeed; } }
    public int DashCount { get { return dashCount; } }

    [Header("ĳ���� ����")]
    [SerializeField] protected float maxHP;
    [SerializeField] protected float currentHP;
    [SerializeField] protected float armor;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected int dashCount;
    #endregion

    #region #Unity �Լ�
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            weaponManager = new WeaponManager(rightHand);
            weaponManager.unRegisterWeapon = (weapon) => { Destroy(weapon); };
            rigidBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            DontDestroyOnLoad(gameObject);
            return;
        }
        DestroyImmediate(gameObject);
    }

    void Start()
    {
        InitStateMachine();
    }

    void Update()
    {
        stateMachine?.UpdateState();
    }

    void FixedUpdate()
    {
        stateMachine?.FixedUpdateState();
    }
    #endregion

    public void OnUpdateStat(float maxHP, float currentHP, float armor, float moveSpeed, int dashCount)
    {
        this.maxHP = maxHP;
        this.currentHP = currentHP;
        this.armor = armor;
        this.moveSpeed = moveSpeed;
        this.dashCount = dashCount;
    }

    private void InitStateMachine()
    {
        PlayerController controller = GetComponent<PlayerController>();
        stateMachine = new StateMachine(StateName.MOVE, new MoveState(controller));
        stateMachine.AddState(StateName.DASH, new DashState(controller));
        stateMachine.AddState(StateName.ATTACK, new AttackState(controller));
    }
}
