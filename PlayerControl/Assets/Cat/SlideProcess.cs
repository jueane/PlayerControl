using UnityEngine;
using System.Collections;

public class SlideProcess : MonoBehaviour
{
    PlayerControl role;

    //滑落速度
    public float slideSpeed = 4f;
    //冰面滑落速度
    public float slideSpeedOnIce = 8.75f;
    //滑落方向（取平衡向量的垂直向量）
    public Vector3 slideVector;

    //是否可站住脚
    public bool isSliding;

    // Use this for initialization
    void Start()
    {
        role = GetComponent<PlayerControl>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateByParent()
    {
        //先重置
        isSliding = false;
        slideVector = Vector3.zero;

        if (role.groundDct.IsOnIceground())
        {
            //role.state = RoleState.Sliding;
            Sliding();
        }
        else if (role.groundDct.slopeLeft >= role.maxFrictionSlope || role.groundDct.slopeRight >= role.maxFrictionSlope || role.groundDct.slope >= role.maxFrictionSlope)
        {
            Sliding();
        }
    }
    //滑落（在斜度过大的坡上）
    void Sliding()
    {
        //注意！！不能加这个判断（state == RoleState.Grounded），加上会顿挫。

        if (role.groundDct.midNormal.x > 0)
        {
            slideVector = role.groundDct.groundVector;
        }
        else if (role.groundDct.midNormal.x < 0)
        {
            slideVector = -role.groundDct.groundVector;
        }
        //Debug.DrawRay(transform.position, slideVector, Color.white);
        if (slideVector != Vector3.zero)
        {
            slideVector.Normalize();
        }
        //向下滑落。（坡度越大，滑落速度越大。冰面不能加此逻辑【为了匹配下山机关】。）
        if (role.groundDct.IsOnIceground())
        {
            //print("下滑.冰面");

            Vector3 slideOnIceVec = slideVector;
            //如果滑落方向是向左，则改为向右。
            //if (slideOnIceVec.x < 0)
            //{
            //    slideOnIceVec.x = -slideVector.x;
            //    slideOnIceVec.y = -slideVector.y;
            //}
            //else if (slideOnIceVec.x == 0)
            //{
            //    //如果在水平冰面上，则强制使滑行速度为0.5f。
            //    slideOnIceVec.x = 0.5f;
            //}
            transform.Translate(slideOnIceVec.normalized * slideSpeedOnIce * role.deltaTime, Space.World);
        }
        else if (role.groundDct.IsOnGround())
        {
            if (role.groundDct.slope >= role.maxFrictionSlope)
            {
                transform.Translate(slideVector.normalized * slideSpeed * role.deltaTime, Space.World);
                isSliding = true;
            }
            //print("下滑.普通");
            else if (role.groundDct.slope < role.maxFrictionSlope)
            {
                //单脚斜率过大。总斜率正常。
                if (role.groundDct.slopeLeft >= role.maxFrictionSlope || role.groundDct.slopeRight >= role.maxFrictionSlope)
                {
                    //单脚向下滑
                    if (role.groundDct.midNormal.x < 0)
                    {
                        //向右下方向打射线。检测坡度
                        RaycastHit hitMid;
                        Physics.Raycast(transform.position, new Vector3(1f, -1, 0), out hitMid, LayerMask.GetMask("ground"));

                        float angle = Vector3.Angle(hitMid.normal, Vector3.up);

                        if (angle >= 45)
                        {
                            transform.Translate(slideVector.normalized * slideSpeed * 1f * role.deltaTime, Space.World);

                            isSliding = true;
                        }

                    }
                    else if (role.groundDct.midNormal.x > 0)
                    {
                        //向左下方向打射线。检测坡度
                        RaycastHit hitMid;
                        Physics.Raycast(transform.position, new Vector3(-1f, -1, 0), out hitMid, LayerMask.GetMask("ground"));

                        float angle = Vector3.Angle(hitMid.normal, Vector3.up);

                        if (angle >= 45)
                        {
                            transform.Translate(slideVector.normalized * slideSpeed * 1f * role.deltaTime, Space.World);

                            isSliding = true;
                        }

                    }

                }
            }

        }
    }
}
