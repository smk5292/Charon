using CharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Char_Jinkwang : Enemy
{
    public float ChargeTimer { get; set; } = 0f;
    public bool IsSecondAttack { get; set; }
    public Vector3 Direction { get; set; }

    private GameObject effect;
    [SerializeField] private GameObject farAttackEffect;
    [SerializeField] private GameObject closeAttackEffect;
    [SerializeField] private Transform effectGenerator;

    public UnityEngine.Events.UnityEvent onDead;

    void OnEnable()
    {
        currentHP = maxHP;
        IsSecondAttack = false;
        //skinnedMeshRenderer.material.color = originMaterial.color;
        stateMachine?.ChangeState(StateName.ENEMY_CHARGE);
    }

    private void OnDisable()
    {
        onDead?.Invoke();
    }

    void Start()
    {
        InitSettings();
        Target = Player.Instance.transform;

        if (effectSounds.ContainsKey(SoundType.DIE) || effectSounds.ContainsKey(SoundType.HIT))
            return;

        AudioClip clip = Resources.Load<AudioClip>("Sounds/EffectSounds/Enemy/Char_Private_K/Sound_Eff_Char_Private_K_Die");
        effectSounds.Add(SoundType.DIE, clip);

        clip = Resources.Load<AudioClip>("Sounds/EffectSounds/Enemy/Sound_Eff_EnemyHit");
        effectSounds.Add(SoundType.HIT, clip);

        clip = Resources.Load<AudioClip>("Sounds/EffectSounds/Enemy/JinKwang/Sound_Eff_JinkwangSkill1");
        effectSounds.Add(SoundType.JINKWANG_SKILL1, clip);

        clip = Resources.Load<AudioClip>("Sounds/EffectSounds/Enemy/JinKwang/Sound_Eff_JinkwangSkill2");
        effectSounds.Add(SoundType.JINKWANG_SKILL2, clip);


        transform.LookAt(Target.transform.position);
        IsBoss = true;

        stateMachine.AddState(StateName.ENEMY_CHARGE, new EnemyChargeState(this));
        stateMachine.AddState(StateName.ENEMY_CHARGE_HIT, new EnemyChargeHitState(this));
        stateMachine.AddState(StateName.ENEMY_CLOSE_SKILL, new EnemyCloseSkillState(this));
        stateMachine.AddState(StateName.ENEMY_FAR_SKILL, new EnemyFarSkillState(this));

        stateMachine?.ChangeState(StateName.ENEMY_CHARGE);
    }
    

    public void StopForAttack()
    {
        if (!IsSecondAttack)
        {
            IsSecondAttack = true;
            animator.SetTrigger("NextComboAttack");
        }
    }


    public void OnFinsithFarAttack()
    {
        weapon.StopAttack();

        if (attackDelayCoroutine != null)
            StopCoroutine(attackDelayCoroutine);
        attackDelayCoroutine = StartCoroutine(CorSecondAttack());
    }

    private IEnumerator CorSecondAttack()
    {
        float timer = 0f;
        stateMachine.ChangeState(StateName.ENEMY_CHARGE);
        isCooltimeDone = false;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= attackDelay)
            {
                EnemyFarSkillState skillState = stateMachine.GetState(StateName.ENEMY_FAR_SKILL) as EnemyFarSkillState;
                skillState.IsAttack = false;
                skillState.isCheckedPlayerPosition = false;
                IsSecondAttack = false;

                isCooltimeDone = true;
                break;
            }

            yield return null;
        }
    }

    public void PlayFarAttackEffect()
    {
        effect = Instantiate(farAttackEffect);

        effect.transform.position = effectGenerator.position;
        effect.transform.rotation = Quaternion.LookRotation(Direction);
        effect.transform.SetParent(effectGenerator);
        effect.GetComponent<ParticleSystem>().Play();
    }

    public void PlayCloseAttackEffect()
    {
        effect = Instantiate(closeAttackEffect);

        effect.transform.position = effectGenerator.position;
        effect.transform.rotation = Quaternion.LookRotation(Direction);
        effect.GetComponent<ParticleSystem>().Play();
    }


    public void OnPlaySkill1Sound()
    {
        audioSource.PlayOneShot(effectSounds[SoundType.JINKWANG_SKILL1]);
    }

    public void OnPlaySkill2Sound()
    {
        audioSource.PlayOneShot(effectSounds[SoundType.JINKWANG_SKILL2]);
    }


    public void StopAttackEffect()
    {
        if (effect == null)
            return;

        effect.GetComponent<ParticleSystem>().Stop();
    }
}
