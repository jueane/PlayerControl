using UnityEngine;
using System.Collections;

public class JumpProcess : MonoBehaviour
{

    PlayerControl role;
    MoveDetect movDct;
    GroundDetect groundDct;
    //动画
    AnimatorControl _animator;

    public bool multijump = false;
    public int remainJumpTimes = 0;

    //跳跃条件1，keyDown.
    bool readyJump = false;
    //跳跃速度
    public float jumpSpeed = 7;
    //实时速度[跳跃过程中的速度]
    public float jumpInstantSpeed = 0;
    //用力跳[长按空格跳的更高]幅度
    public float jumpHigher = 0.55f;
    //上升衰减速度
    public float jumpAttenuation = 13;

    // Use this for initialization
    void Start()
    {
    }

    public void init()
    {
        role = GetComponent<PlayerControl>();
        movDct = GetComponent<MoveDetect>();
        groundDct = GetComponent<GroundDetect>();
        _animator = GetComponent<AnimatorControl>();
        //roleActionsControl = RoleActionsInputBindings.ActionsBindings();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateByParent()
    {
        //设置连跳状态。每次落到地面都会重置双跳。
        if (multijump && role.groundDct.IsOnGround() && role.groundDct.slope < role.maxFrictionSlope)
        {
            //落地恢复连跳次数
            remainJumpTimes = 1;
        }
        else if (multijump == false && role.groundDct.IsOnGround())
        {
            //连跳未开，落地恢复0.
            remainJumpTimes = 0;
        }


        Raise();
    }

    public void JumpOnGround()
    {
        //从平台跌落，设置三段跳（因为跌落后执行，所以在跌落的那一帧，会没来得及设置三段跳，因此在这也设置一次）
        if (groundDct.isClosedGround == false || (groundDct.isClosedGround && groundDct.disGround > 0))
        {
            //从平台跌落。还可以跳2次。
            if (role.state == RoleState.Grounded)
            {
                if (role.jumpProc.multijump)
                {
                    role.jumpProc.remainJumpTimes = 2;
                }
                else
                {
                    role.jumpProc.remainJumpTimes = 1;
                }
            }
        }


        //起跳条件1
        if (InputMgr.Instance.catInput.KeyA.WasPressed)
        {
            //print("readyJump");
            readyJump = true;
        }
        if (InputMgr.Instance.catInput.KeyA.IsPressed)
        {
            //起跳条件2
            if (readyJump)
            {
                //普通跳或二段跳
                if (role.groundDct.IsOnGround() && groundDct.IsStandable())
                {
                    //print("跳1");
                    _JumpOnGround(jumpSpeed);
                }
                else if (remainJumpTimes > 0)
                {
                    //print("跳2");

                    //重置惯性一系统参数
                    role.moveProc.turnCount = 0;
                    role.moveProc.StaySkyTime = 0;
                    role.moveProc.lastFrameSpeedVector = Vector3.zero;

                    _JumpInTheSky(jumpSpeed, true);
                }
                //注意：onIceGround此处属于特殊情况。。。。
            }
        }
    }

    //强制跳（外部调用，如演出）
    public void ForceJump(float speed, int times, bool isMultijump)
    {
        if (times > 0)
        {
            remainJumpTimes = times;
            _JumpInTheSky(speed, isMultijump);
        }
    }

    //地面跳
    public void _JumpOnGround(float speed)
    {
        //在地面或贴近地面
        if (role.groundDct.IsOnGround())
        {
            bool isCanJump = false;
            //在冰面（此条件属于特殊情况）
            if (role.groundDct.IsOnIceground())
            {
                isCanJump = true;
            }
            else
            {
                //双脚着地，过斜禁跳
                if (role.groundDct.isHitLeft && role.groundDct.isHitRight && role.groundDct.slope < role.maxFrictionSlope)
                {
                    isCanJump = true;
                }
                //单脚着地，过斜禁跳
                if (role.groundDct.isHitLeft && role.groundDct.isHitRight == false && role.groundDct.slopeLeft < role.maxFrictionSlope)
                {
                    isCanJump = true;
                }
                //单脚着地，过斜禁跳
                if (role.groundDct.isHitLeft == false && role.groundDct.isHitRight && role.groundDct.slopeRight < role.maxFrictionSlope)
                {
                    isCanJump = true;
                }
            }
            //执行跳
            if (isCanJump)
            {
                readyJump = false;
                role.state = RoleState.Raising;
                jumpInstantSpeed = speed * 1f;
            }
        }
    }

    //空中跳
    void _JumpInTheSky(float speed, bool isMultijump)
    {
        //空中跳板动画
        if (isMultijump)
        {
            _animator.DoubleJumpEffect(transform.position);
        }
        //print("空中跳");
        remainJumpTimes--;
        readyJump = false;
        //执行跳
        role.state = RoleState.Raising;
        jumpInstantSpeed = speed * 1f;
    }

    //执行跳的过程
    void Raise()
    {
        //上升阶段
        if (jumpInstantSpeed > 0)
        {
            //上升
            if (movDct.moveToUp)
            {
                float raiseDis = role.deltaTime * jumpInstantSpeed;
                if (InputMgr.Instance.catInput.KeyA.IsPressed)
                {
                    raiseDis += raiseDis * jumpHigher;
                }
                ////防穿越（即将穿过）
                //if (Mathf.Abs(raiseDis) > upDis && upDis > 0)
                //{
                //    raiseDis = upDis;
                //}
                //else if (upDis < 0)
                //{
                //    print("updis小于0");
                //    raiseDis = upDis;
                //}
                transform.position += Vector3.up * raiseDis;
            }
            else
            {
                jumpInstantSpeed = 0;
            }

            //上升速度递减
            jumpInstantSpeed -= jumpAttenuation * role.deltaTime;
            if (jumpInstantSpeed < 0)
            {
                jumpInstantSpeed = 0;
            }
        }
        else
        {
            //上升结束，状态从rising->falling
            if (role.state == RoleState.Raising)
            {
                role.state = RoleState.Falling;
                role.fallProc.fallSpeed = 0;
            }
        }
    }


}
