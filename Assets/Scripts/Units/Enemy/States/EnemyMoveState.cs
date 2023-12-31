using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyMoveState : CharacterController.BaseState
{
    public const float CONVERT_UNIT_VALUE = 0.01f;
    public const float DEFAULT_CONVERT_MOVESPEED = 3f;
    public const float DEFAULT_ANIMATION_PLAYSPEED = 0.9f;
    public readonly int moveAnimation = Animator.StringToHash("Move");
    public readonly int moveSpeedAnimation = Animator.StringToHash("MoveSpeed");
    private Enemy enemy;
 

    public EnemyMoveState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public override void OnEnterState()
    {
        enemy.agent.isStopped = false;
        enemy.rigidBody.isKinematic = true;
    }

    public override void OnExitState()
    {
        enemy.agent.isStopped = true;
        enemy.animator.SetBool(moveAnimation, false);
}

    public override void OnFixedUpdateState()
    {
    }

    public override void OnUpdateState()
    {
        if (enemy.agent.enabled)
        {
            if (enemy.IsDetected)
            {
                enemy.agent.isStopped = false;
                float currentMoveSpeed = enemy.MoveSpeed * CONVERT_UNIT_VALUE;
                float moveAnimationSpeed = DEFAULT_ANIMATION_PLAYSPEED + GetAnimationSyncWithMovement(currentMoveSpeed);

                enemy.agent.speed = currentMoveSpeed;
                enemy.animator.SetFloat(moveSpeedAnimation, moveAnimationSpeed);
                enemy.animator.SetBool(moveAnimation, true);
                enemy.agent.SetDestination(enemy.Target.position);
                LookAtMovingDirection();
                return;
            }

            if (enemy.IsAlived || enemy.IsWithinAttackRange)
            {
                if(enemy.isCooltimeDone)
                    enemy.stateMachine.ChangeState(CharacterController.StateName.ENEMY_ATTACK);
                enemy.animator.SetBool(moveAnimation, false);
            }
        }
    }

    protected void LookAtMovingDirection()
    {
        Vector3 direction = enemy.agent.desiredVelocity;
        direction.Set(direction.x, 0f, direction.z);

        Quaternion targetAngle = Quaternion.LookRotation(direction);
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetAngle, enemy.RotationSpeed * Time.deltaTime);
    }



    protected float GetAnimationSyncWithMovement(float changedMoveSpeed)
    {
        if (enemy.IsAlived)
            return -DEFAULT_ANIMATION_PLAYSPEED;

        // (바뀐 이동 속도 - 기본 이동속도) * 0.1f
        return (changedMoveSpeed - DEFAULT_CONVERT_MOVESPEED) * 0.1f;
    }
}
