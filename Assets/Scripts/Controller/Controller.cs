using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Controller : MonoBehaviour
{
    public static bool EnabledInput = true;

    public _Input theInput { get; private set; }
    public _Physics thePhysics { get; private set; }
    public _Animator theAnimator { get; private set; }
    public Sondol sondol { get; private set; }

    public bool isDash { get; set; } = false;
    public bool isDashAttack { get; set; } = false;
    public bool canInputKey { get; set; } = false;
    public bool isCharging { get; set; } = false;
    public bool isAttack  = false;
    public bool canComboAttack = false;


    void Awake()
    {
        theInput = GetComponent<_Input>();
        thePhysics = GetComponent<_Physics>();
        theAnimator = GetComponent<_Animator>();
        sondol = GetComponent<Sondol>();
    }

    void Update()
    {
        if (EnabledInput)
        {
            theInput.UpdateComponent(this);
            theAnimator.UpdateComponent(this);
        }
    }

    void FixedUpdate()
    {
        thePhysics.UpdateComponent(this);
    }


    public void _ResetMove()
    {
        theInput.moveInputLock = false;
        thePhysics.moveLock = false;
    }

    public void _ResetAttack()
    {
        isAttack = false;
        canComboAttack = false;
        isDashAttack = false;
    }

    public void OnPossibleCombo()
    {
        canComboAttack = true;
    }
}

