using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallProcess_1 : MonoBehaviour {

    PlayerControl role;
    GameObject foot1;
    GameObject foot2;

    //打线的起始位置与地面的距离
    public const float originPoint2Ground = 0.4f;

    //下落速度
    public float fallSpeed = 0;
    //下落加速度
    public float fallAccelerateSpeed = 25;
    //最大下落速度
    public float maxFallSpeed = 18;


    //下落射线长度（红线）
    float rayLen = 1.2f;
    //平衡射线长度（绿线）
    float rayLenGreen = 1;


    //与地面的距离（小于0表示未打到或超过贴合距离）
    public float disToGround;
    public float disToGround1;
    public float disToGround2;
    public bool isFootHit;
    public bool isFootHit1;
    public bool isFootHit2;
    Vector3 footNormal;
    Vector3 footNormal1;
    Vector3 footNormal2;

    //脚线是否命中冰面
    public bool hasHitIceGround = false;

	// Use this for initialization
    void Start()
    {
        foot1 = GameObject.Find("foot1");
        foot2 = GameObject.Find("foot2");

        role = GetComponent<PlayerControl>();
	}
	
	// Update is called once per frame
    void Update()
    {	
	}

    public void UpdateByParent()
    {
        FallCheck();
        if (role.state == RoleState.Falling)
        {
            Fall();
            FallCheck();
        }
    }
    
    void FallCheck()
    {
        int layerCollision=LayerMask.GetMask("ground","Platform");

        disToGround = 0;
        disToGround1 = 0;
        disToGround2 = 0;

        bool onIce = false;

        //右脚线
        RaycastHit hit1;
        Vector3 pos1 = foot1.transform.position;
        //根据斜率设置起点偏移
        if (role.groundDct.midNormal.y < 0)
        {
            pos1 -= new Vector3(0, role.groundDct.slope * originPoint2Ground * 2, 0);
        }
        else if (role.groundDct.midNormal.y > 0)
        {
            pos1 += new Vector3(0, role.groundDct.slope * originPoint2Ground * 2, 0);
        }
        //加上在斜面上的时候，中线造成的偏移。
        pos1 -= new Vector3(0, role.groundDct.slope * 0.2f, 0);
        //检测
        //Debug.DrawRay(pos1, Vector3.down * rayLen, Color.red);
        isFootHit1 = Physics.Raycast(pos1, Vector3.down, out hit1, rayLen, layerCollision);
        float hitDis1 = hit1.distance;
        if (Mathf.Abs(hit1.distance - originPoint2Ground) < 0.0001)
        {
            hitDis1 = originPoint2Ground;
        }
        disToGround1 = hitDis1 - originPoint2Ground;
        if (isFootHit1 && "IceGround".Equals(hit1.transform.tag))
        {
            onIce = true;
        }
        
        //左脚线
        RaycastHit hit2;
        Vector3 pos2 = foot2.transform.position;
        //根据斜率设置起点偏移
        if (role.groundDct.midNormal.y > 0)
        {
            pos2 += new Vector3(0, role.groundDct.slope * originPoint2Ground * 2, 0);
        }
        else if (role.groundDct.midNormal.y < 0)
        {
            pos2 -= new Vector3(0, role.groundDct.slope * originPoint2Ground * 2, 0);
        }
        //加上在斜面上的时候，中线造成的偏移。
        pos2 -= new Vector3(0, role.groundDct.slope * 0.2f, 0);
        //检测
        //Debug.DrawRay(pos2, Vector3.down * rayLen, Color.red);
        isFootHit2 = Physics.Raycast(pos2, Vector3.down, out hit2, rayLen, layerCollision);
        float hitDis2 = hit2.distance;
        if (Mathf.Abs(hit2.distance - originPoint2Ground) < 0.0001)
        {
            hitDis2 = originPoint2Ground;
        }
        disToGround2 = hitDis2 - originPoint2Ground;
        if (isFootHit2 && "IceGround".Equals(hit2.transform.tag))
        {
            onIce = true;
        }

        //中间线
        RaycastHit hit;
        Vector3 pos = transform.position;
        //加上在斜面上的时候，中线造成的偏移。
        pos -= new Vector3(0, role.groundDct.slope * 0.2f, 0);
        //检测
        //Debug.DrawRay(pos, Vector3.down * rayLen, Color.red);
        isFootHit = Physics.Raycast(pos, Vector3.down, out hit, rayLen, layerCollision);
        if (getSlope(hit.normal) < 0.95f)
        {
            float hitDis = hit.distance;
            if (Mathf.Abs(hit.distance - originPoint2Ground) < 0.0001)
            {
                hitDis = originPoint2Ground;
            }
            //与地面距离
            disToGround = hitDis - originPoint2Ground;
        }
        else
        {
            disToGround = 0;
            isFootHit = true;
        }
        if (isFootHit && "IceGround".Equals(hit.transform.tag))
        {
            onIce = true;
        }

        hasHitIceGround = onIce;

        //print("dis：" + disToGround + "，dis1：" + disToGround1 + "，dis2：" + disToGround2);

        footNormal = hit.normal;
        footNormal1 = hit1.normal;
        footNormal2 = hit2.normal;

        CheckOnGround();
    }

    //由fallCheck调用，不对其它处开放。
    void CheckOnGround()
    {
        //命中地面且距离为0，表示紧贴地面。否则表示在空中。在空中又分为上升、下落、跌落。

        //设置是否在地面
        if ((isFootHit && disToGround == 0) || (isFootHit1 && disToGround1 == 0) || (isFootHit2 && disToGround2 == 0))
        {
            //满足特殊情况。
            if (CheckCondition())
            {
                //注意！需要判断是"非上升状态"才能将状态改成在地面，否则会出现连跳的bug...
                if (role.jumpProc.jumpInstantSpeed == 0)
                {
                    role.state = RoleState.Grounded;
                    role.jumpProc.remainJumpTimes = 0;
                }
            }

        }
        else
        {
            //print("从高台跌落");
            //在空中的情况下，如果当前状态是grounded，说明这一帧从高台跌落。
            if (role.state == RoleState.Grounded)
            {
                //注意！需要判断是"非上升状态"才能将状态改成下落，否则会出现起跳变下落的bug...
                if (role.jumpProc.jumpInstantSpeed == 0)
                {
                    role.state = RoleState.Falling;
                    //跌落速度。高空跌落时，初始速度为0。当低空且贴近地面跌落时，初始下落速度设为较大值。                    
                    fallSpeed = 5 * (1 - role.groundDct.slope);
                    //fallSpeed = 0;

                }
                if (role.jumpProc.multijump && role.groundDct.slope < role.maxFrictionSlope)
                {
                    //从高处落下可开启二段跳
                    role.jumpProc.remainJumpTimes = 1;
                    print("D");
                }
            }
        }
    }

    void Fall()
    {
        //当前帧下落距离
        float fallDis = fallSpeed * role.deltaTime;
        //print("disToGround：" + disToGround + "，fallDis：" + fallDis);

        //下落状态判断（不在地面的情况）[判断!=0即只判断射线打中地面的情况，距离不够或已超过的情况不在此考虑]
        if (disToGround != 0)
        {
            if (isFootHit == false && isFootHit1 == false && isFootHit2 == false)
            {
                //print("全部未命中，下落中");
                transform.position += Vector3.down * fallDis;
            }
            else
            {
                if (isFootHit)
                {
                    DoFall(fallDis, disToGround);
                }
                else if (isFootHit1)
                {
                    DoFall(fallDis, disToGround1);
                }
                else if (isFootHit2)
                {
                    DoFall(fallDis, disToGround2);
                }
            }
        }

        if (fallSpeed < maxFallSpeed)
        {
            fallSpeed += fallAccelerateSpeed * role.deltaTime;
        }
    }

    //由fall调用。不对其它处开放。
    void DoFall(float fallDis, float dis)
    {
        if (fallDis < dis)
        {
            //print("下落中");
            //速度=falldis*斜率。坡度越大，速度越慢。
            transform.position += Vector3.down * fallDis;
        }
        else if (fallDis == dis)
        {
            //print("将要接触");
            transform.position += Vector3.down * fallDis;
        }
        else if (fallDis > dis)
        {
            //print("将要超过");
            transform.position += Vector3.down * dis;
        }
    }

    //是否在地面，包含冰面。
    public bool IsOnGround()
    {
        if (role.state == RoleState.Grounded)
        {
            return true;
        }
        else if (role.state == RoleState.Falling && Mathf.Abs(GetMinDis()) < 0.0001f)
        {
            //条件成立，则返回true，否则什么也不返回 [因为还要判断其它条件，运行至最后，自动返回false]。
            if (CheckCondition())
            {
                return true;
            }
        }
        else if (isOnIceGround())
        {
            return true;
        }
        return false;
    }

    //判断是否单脚命中斜墙，斜墙斜率大于maxFrictionSlope，且接触距离为零。[这样会误以为在地面]
    bool CheckCondition()
    {
        //特殊情况！！贴大斜率墙面跳的时候，可能出现单脚线刚好接触墙面。贴墙连跳的Bug。
        bool conditionSpec = true;
        if (isFootHit == false && isFootHit1 == false && isFootHit2)
        {
            if (getSlope(footNormal2) >= role.maxFrictionSlope)
            {
                conditionSpec = false;
            }
        }
        if (isFootHit == false && isFootHit1 && isFootHit2 == false)
        {
            if (getSlope(footNormal1) >= role.maxFrictionSlope)
            {
                conditionSpec = false;
            }
        }
        return conditionSpec;
    }

    //是否在冰面（属于地面）
    public bool isOnIceGround()
    {
        //if (hasHitIceGround && (state == RoleState.Grounded || state == RoleState.Falling) && Mathf.Abs(disToGround) < 0.2f)
        if (hasHitIceGround && (role.state == RoleState.Grounded || role.state == RoleState.Falling) && Mathf.Abs(GetMinDis()) < 0.2f)
        {
            return true;
        }
        return false;
    }

    float getSlope(Vector3 vec)
    {
        float angle = Vector3.Angle(vec, Vector3.up);
        if (vec == Vector3.zero)
        {
            angle = 0;
        }
        float VecSlope = angle / 90;
        return VecSlope;
    }

    //最接近地面的距离
    public float GetMinDis()
    {
        //取命中，且大于-0.4f，的最小值。
        List<float> disList = new List<float>();
        if (isFootHit && disToGround > -originPoint2Ground)
        {
            disList.Add(Mathf.Abs(disToGround));
        }
        if (isFootHit1 && disToGround1 > -originPoint2Ground)
        {
            disList.Add(Mathf.Abs(disToGround1));
        }
        if (isFootHit2 && disToGround2 > -originPoint2Ground)
        {
            disList.Add(Mathf.Abs(disToGround2));
        }

        float minDis = -1;
        if (disList.Count > 0)
        {
            minDis = disList[0];

            for (int i = 0; i < disList.Count; i++)
            {
                if (minDis > disList[i])
                {
                    minDis = disList[i];
                }
            }
        }

        return minDis;
    }
    //最低的脚的位置
    //float GetMinLow()
    //{
    //    //取命中，且大于-0.4f，的最小值。
    //    List<float> disList = new List<float>();
    //    if (isFootHit && disToGround > -originPoint2Ground)
    //    {
    //        disList.Add(disToGround);
    //    }
    //    if (isFootHit1 && disToGround1 > -originPoint2Ground)
    //    {
    //        disList.Add(disToGround1);
    //    }
    //    if (isFootHit2 && disToGround2 > -originPoint2Ground)
    //    {
    //        disList.Add(disToGround2);
    //    }

    //    float minDis = -1;
    //    if (disList.Count > 0)
    //    {
    //        minDis = disList[0];

    //        for (int i = 0; i < disList.Count; i++)
    //        {
    //            if (minDis > disList[i])
    //            {
    //                minDis = disList[i];
    //            }
    //        }
    //    }

    //    return minDis;
    //}
}
