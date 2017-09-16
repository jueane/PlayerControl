using UnityEngine;
using System.Collections;

public class FallProcess : MonoBehaviour
{
    //可以忽略的[与地面的间隙]。在这个距离即视为在地面。
    public static float ignoreDisGround = 0.01f;

    PlayerControl role;

    public GroundDetect groundDct;

    //下落速度
    public float fallSpeed = 0;
    //下落加速度
    public float fallAccelerateSpeed = 25;
    //最大下落速度
    public float maxFallSpeed = 18;

    // Use this for initialization
    void Start()
    {
        role = GetComponent<PlayerControl>();
        groundDct = GetComponent<GroundDetect>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateByParent()
    {
        if (isActiveAndEnabled == false)
        {
            return;
        }

        //非上升阶段，处理状态
        if (role.state != RoleState.Raising)
        {
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
                role.state = RoleState.Falling;
            }
            if (groundDct.isClosedGround && groundDct.disGround == 0 && groundDct.IsStandable())
            {
                if (groundDct.slopeLeft < role.maxFrictionSlope && groundDct.slopeRight < role.maxFrictionSlope)
                {
                    role.state = RoleState.Grounded;
                    fallSpeed = 0;
                }
                else
                {
                    role.state = RoleState.Grounded;
                }
                //平稳落于地面
                //role.groundDct.OnStandGround();
            }
        }

        Falling();


        //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //obj.transform.position = transform.position;
        //obj.transform.localScale = Vector3.one * 0.05f;
    }

    void Falling()
    {
        //在地面的情况时候，Y轴位置偏差微调。
        if (role.state == RoleState.Grounded)
        {
            if (groundDct.isClosedGround && groundDct.disGround < 0.1f)
            {
                if (groundDct.disGround < 0)
                {
                    transform.position += Vector3.down * (groundDct.disGround);
                }
                else
                {
                    transform.position += Vector3.down * (groundDct.disGround);
                }
            }
        }
        //下落。
        if (role.state == RoleState.Falling)
        {
            //当前帧预计下落距离
            float fallDis = fallSpeed * role.deltaTime;
            //正常下落
            if (groundDct.isClosedGround == false || (groundDct.isClosedGround && groundDct.disGround >= fallDis))
            {
                transform.position += Vector3.down * fallDis;

                //如果在sliding状态，则设置一个较小的fall速度;
                if (role.slideProc.isSliding)
                {
                    fallSpeed = 4;
                }

                //加速度
                if (fallSpeed < maxFallSpeed)
                {
                    fallSpeed += fallAccelerateSpeed * role.deltaTime;
                }
            }
            //下落距离不足，直接落地。
            if (groundDct.isClosedGround && groundDct.disGround < fallDis)
            {
                if (groundDct.disGround < 0)
                {
                    transform.position += Vector3.down * (groundDct.disGround);
                }
                else if (groundDct.disGround > 0)
                {
                    transform.position += Vector3.down * (groundDct.disGround);
                }
            }


        }
    }

}
