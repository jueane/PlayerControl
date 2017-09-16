using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoleCtrl2 : MonoBehaviour
{
    //打线的起始位置与地面的距离
    const float originPoint2Ground = 0.4f;

    float deltaTime;

    //角色主体
    Transform mainBody;
    Transform model;
    //平衡检测线
    Transform poise1;
    Transform poise2;
    //支撑脚线
    GameObject foot1;
    GameObject foot2;
    //动画
    AnimatorControl _animator;

    // 角色动作控制
    public RoleActionsInputBindings roleActionsControl;

    //角色状态
    public RoleState state;

    //所有碰撞层
    int layerCollision;
    //仅地面
    int layerGround;

    public bool multijump = false;
    public int remainJumpTimes = 0;

    //平衡法线
    public Vector3 midNormal;
    //倾斜度
    public float slope;

    //朝向（左和右）
    public bool faceLeft = false;

    //平移速度
    public float moveSpeed = 5;
    //水平输入速度
    public float horizontalInputSpeed = 0;
    //水平外部施加速度
    public float horizontalExternalSpeed = 0;

    //滑落速度
    public float slideSpeed = 7.5f;
    //冰面滑落速度
    public float slideSpeedOnIce = 8.75f;
    //滑落方向（取平衡向量的垂直向量）
    Vector3 slideVector;
    //最大摩擦斜率（滑落、斜向上跑）
    float maxFrictionSlope = 0.5f;

    //下落速度
    public float fallSpeed = 0;
    //下落加速度
    public float fallAccelerateSpeed = 25;
    //最大下落速度
    public float maxFallSpeed = 18;

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

    //上一帧角色的移动方向
    Vector3 lastFrameSpeedVector;
    //惯性
    public float inertia = 4;

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

    //可移动方向检测结果
    public bool moveToLeft;
    public bool moveToRight;
    public bool moveToUp;
    //剩余最大移动距离
    float leftDis;
    float rightDis;
    float upDis;

    //脚线是否命中冰面
    public bool hasHitIceGround = false;

    // Use this for initialization
    void Start()
    {
        mainBody = transform.FindChild("body");
        model = mainBody.FindChild("model");

        poise1 = transform.FindChild("poise1");
        poise2 = transform.FindChild("poise2");

        foot1 = GameObject.Find("foot1");
        foot2 = GameObject.Find("foot2");

        state = RoleState.Falling;
        layerCollision = LayerMask.GetMask("ground") | LayerMask.GetMask("Platform");
        layerGround = LayerMask.GetMask("ground");

        _animator = GetComponent<AnimatorControl>();

        roleActionsControl = RoleActionsInputBindings.ActionsBindings();
    }

    void BeforeUpdate()
    {
        deltaTime = Time.deltaTime;
        if (deltaTime > 0.06)
        {
            deltaTime = 0.06f;
        }

        if (roleActionsControl == null)
        {
            roleActionsControl = RoleActionsInputBindings.ActionsBindings();
        }

        //记录上一帧结束时，角色的受力状态。
        if (isOnIceGround())
        {
            //在冰面
            lastFrameSpeedVector = slideVector;
            lastFrameSpeedVector.x = slideSpeedOnIce;
            lastFrameSpeedVector.Normalize();
        }
        else if (IsOnGround())
        {
            //在地面。
            if (slope > maxFrictionSlope)
            {
                lastFrameSpeedVector = slideVector;
                lastFrameSpeedVector.Normalize();
            }
            else
            {
                //如果不符合条件，则把受力归零。
                lastFrameSpeedVector = Vector3.zero;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //obj.transform.localScale = Vector3.one * 0.2f;
        //obj.transform.position = transform.position;

        BeforeUpdate();

        if (layerCollision == 0)
        {
            layerCollision = LayerMask.GetMask("ground") | LayerMask.GetMask("Platform");
        }
        if (layerGround == 0)
        {
            layerGround = LayerMask.GetMask("ground");
        }

        AdjustBalance();

        KeepMoving();

        //以下是之前在FixedUpdate里的代码。

        Raise();

        FallCheck();

        //下滑和下落是否成功执行。
        bool hasFall = false;
        bool hasSlide = false;
        //记录[下落+滑落]的开始和结束位置。
        Vector3 posBegin;
        Vector3 posEnd;
        posBegin = transform.position;
        if (state == RoleState.Falling)
        {
            //先尝试下落，再检查状态。（因为下落是被动动作。先检查状态后下落，会导致检查出来的状态被下落改变）
            Fall();
            if (posBegin != transform.position)
            {
                FallCheck();
                hasFall = true;
            }
        }
        //下滑。（不能加isOnGround的判断，加上会顿挫。）
        Vector3 pos2 = transform.position;
        Slide();
        posEnd = transform.position;
        if (pos2 != transform.position)
        {
            FallCheck();
            hasSlide = true;
        }
        //如果既下落又滑落，则整体的移动位置减半。
        if (hasFall && hasSlide || isOnIceGround())
        {
            if (isOnIceGround())
            {
                //在冰上的时候，速度更大些。
                transform.position = Vector3.Lerp(posBegin, posEnd, 1f);
            }
            else
            {
                transform.position = Vector3.Lerp(posBegin, posEnd, 0.5f);
            }
        }
    }

    //滑落（在斜度过大的坡上）
    void Slide()
    {
        //注意！！不能加这个判断（state == RoleState.Grounded），加上会顿挫。

        slideVector = Vector3.zero;

        if (midNormal.x != 0)
        {
            float x = midNormal.y;
            float y = -midNormal.x;
            if (midNormal.x < 0)
            {
                slideVector = new Vector3(-x, -y, 0);
                Debug.DrawRay(transform.position, slideVector, Color.white);
            }
            else if (midNormal.x > 0)
            {
                slideVector = new Vector3(x, y, 0);
                Debug.DrawRay(transform.position, slideVector, Color.white);
            }
        }

        if (state != RoleState.Raising)
        {
            //向下滑落。（坡度越大，滑落速度越大。冰面不能加此逻辑【为了匹配下山机关】。）
            if (isOnIceGround())
            {
                //print("下滑.冰面");
                Vector3 slideOnIceVec = slideVector;
                //如果滑落方向是向左，则改为向右。
                if (slideOnIceVec.x < 0)
                {
                    slideOnIceVec.x = -slideVector.x;
                    slideOnIceVec.y = -slideVector.y;
                }
                else if (slideOnIceVec.x == 0)
                {
                    //如果在水平冰面上，则强制使滑行速度为0.5f。
                    slideOnIceVec.x = 0.5f;
                }
                transform.Translate(slideOnIceVec * slideSpeedOnIce * deltaTime, Space.World);
            }
            else if (slope > maxFrictionSlope)
            {
                //print("下滑.普通");
                transform.Translate(slideVector * slideSpeed * deltaTime, Space.World);
            }
        }
    }

    //调整平衡（1.下落到一定高度后再开始调整角度.2.离开地面时调成水平.）
    void AdjustBalance()
    {
        midNormal = Vector3.zero;

        RaycastHit hit1, hit2, hit3;

        Vector3 pos1 = (poise1.position + poise2.position) / 2;
        Vector3 pos2 = poise1.position;
        Vector3 pos3 = poise2.position;

        Debug.DrawRay(pos1, Vector3.down * rayLenGreen, Color.green);
        Debug.DrawRay(pos2, Vector3.down * rayLenGreen, Color.green);
        Debug.DrawRay(pos3, Vector3.down * rayLenGreen, Color.green);
        bool isHit1 = Physics.Raycast(pos1, Vector3.down, out hit1, rayLenGreen, layerCollision);
        bool isHit2 = Physics.Raycast(pos2, Vector3.down, out hit2, rayLenGreen, layerCollision);
        bool isHit3 = Physics.Raycast(pos3, Vector3.down, out hit3, rayLenGreen, layerCollision);

        Vector3 normal1 = hit1.normal;
        Vector3 normal2 = hit2.normal;
        Vector3 normal3 = hit3.normal;

        //边线未命中，则设为朝上。
        if (isHit1 == false || getSlope(normal1) > 0.9f)
        {
            normal1 = Vector3.zero;
        }
        if (isHit2 == false || getSlope(normal2) > 0.9f)
        {
            normal2 = Vector3.zero;
        }
        if (isHit3 == false || getSlope(normal3) > 0.9f)
        {
            normal3 = Vector3.zero;
        }

        Debug.DrawRay(pos1, normal1, Color.yellow);
        Debug.DrawRay(pos2, normal2, Color.yellow);
        Debug.DrawRay(pos3, normal3, Color.yellow);

        //取斜率
        midNormal = (normal1 + normal2 + normal3).normalized;

        //防抖动！如果中法线和一条边法线的相同，且不同于另一条边法线（如n==n1!=n2或n==n2!=n1），则视另一条边法线为vector.zero。
        //if (normal1 != Vector3.zero && normal1 == normal2 && normal1 != normal3)
        //{
        //    normal3 = Vector3.zero;
        //}
        //if (normal1 != Vector3.zero && normal1 == normal3 && normal1 != normal2)
        //{
        //    normal2 = Vector3.zero;
        //}

        //防抖动！如果中线和一条边线命中，且另一条边线命中位置最低或未命中，则忽视这条边线的法线。
        if (isHit1 && isHit2 && midNormal.x < 0)
        {
            normal3 = Vector3.zero;
        }
        if (isHit1 && isHit3 && midNormal.x > 0)
        {
            normal2 = Vector3.zero;
        }
        //重算midNormal。
        midNormal = (normal1 + normal2 + normal3).normalized;

        Debug.DrawRay(transform.position, midNormal, Color.blue);
        setCheckedSlope();

        //需要进行角色旋转的三种情况。1.在下降且接近地面[地面可能平坦也可能不平坦]。2.在台子边缘（只有一条平衡线命中）。3.在地面（两条平衡线均命中[地面可能平坦也可能不平坦]）。
        if ((state == RoleState.Falling && GetMinDis() < 1f) || (state == RoleState.Grounded) || (state == RoleState.Raising && GetMinDis() <= 0.2f))
        {
            if (slope < maxFrictionSlope * 1.5f)
            {
                float agl = Vector3.Angle(mainBody.up, midNormal);

                Vector3 midValue = Vector3.Lerp(mainBody.up, midNormal, 0.2f);

                //最大倾斜角度
                if (midValue.x < -maxFrictionSlope)
                {
                    midValue.x = -maxFrictionSlope;
                }
                else if (midValue.x > maxFrictionSlope)
                {
                    midValue.x = maxFrictionSlope;
                }
                mainBody.up = midValue;
            }
        }

        //平衡法线为0时，或者斜率大于最大摩擦斜率时，不进行旋转。
        if (midNormal == Vector3.zero || slope > maxFrictionSlope)
        {
            //离开地面时，逐渐调整至水平.
            float beforeAngle = Vector3.Angle(mainBody.up, Vector3.up);
            //先是每次按0.2的比例旋转。
            Vector3 midValue = Vector3.Lerp(mainBody.up, Vector3.up, 0.15f);
            mainBody.up = midValue;

            //如果旋转到最后，0.2f已经不再明显，则直接旋转完剩余角度。
            float afterAngle = Vector3.Angle(mainBody.up, Vector3.up);
            if (beforeAngle - afterAngle > 0)
            {
                //print("差：" + (beforeAngle - afterAngle));
                if (Mathf.Abs(beforeAngle - afterAngle) < 0.1f)
                {
                    mainBody.up = Vector3.up;
                }
            }
        }

    }

    //跳跃事件调用（外部调用）
    public void JumpOnGround()
    {
        //起跳条件1
        if (roleActionsControl.KeyA.WasPressed)
        {
            //print("readyJump");
            readyJump = true;
        }
        if (roleActionsControl.KeyA.IsPressed)
        {
            //起跳条件2
            if (readyJump)
            {
                //设置连跳状态
                if (multijump && IsOnGround() && slope < maxFrictionSlope)
                {
                    remainJumpTimes = 1;
                }
                //普通跳或二段跳
                if (IsOnGround())
                {
                    _JumpOnGround(jumpSpeed);
                }
                else if (remainJumpTimes > 0)
                {
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
        if (IsOnGround())
        {
            bool isCanJump = false;
            //在冰面（此条件属于特殊情况）
            if (isOnIceGround())
            {
                isCanJump = true;
            }
            else if (slope < maxFrictionSlope)
            {
                //角色斜度超过定值，禁止跳
                isCanJump = true;
            }
            //执行跳
            if (isCanJump)
            {
                readyJump = false;
                state = RoleState.Raising;
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
        state = RoleState.Raising;
        jumpInstantSpeed = speed * 1f;
    }

    //执行跳的过程
    void Raise()
    {
        //上升阶段
        if (jumpInstantSpeed > 0)
        {
            //上升
            if (moveToUp)
            {
                float raiseDis = deltaTime * jumpInstantSpeed;
                if (roleActionsControl.KeyA.IsPressed)
                {
                    raiseDis += raiseDis * jumpHigher;
                }
                //防穿越（即将穿过）
                if (Mathf.Abs(raiseDis) > upDis && upDis > 0)
                {
                    raiseDis = upDis;
                }
                else if (upDis < 0)
                {
                    print("updis小于0");
                    raiseDis = upDis;
                }
                transform.position += Vector3.up * raiseDis;
            }
            else
            {
                jumpInstantSpeed = 0;
            }

            //上升速度递减
            jumpInstantSpeed -= jumpAttenuation * deltaTime;
            if (jumpInstantSpeed < 0)
            {
                jumpInstantSpeed = 0;
            }
        }
        else
        {
            //上升结束，状态从rising->falling
            if (state == RoleState.Raising)
            {
                state = RoleState.Falling;
                fallSpeed = 0;
            }
        }
    }

    void FallCheck()
    {
        disToGround = 0;
        disToGround1 = 0;
        disToGround2 = 0;

        bool onIce = false;

        //右脚线
        RaycastHit hit1;
        Vector3 pos1 = foot1.transform.position;
        //根据斜率设置起点偏移
        if (midNormal.x > 0)
        {
            pos1 -= new Vector3(0, slope * originPoint2Ground * 2, 0);
        }
        else if (midNormal.x < 0)
        {
            pos1 += new Vector3(0, slope * originPoint2Ground * 2, 0);
        }
        //加上在斜面上的时候，中线造成的偏移。
        pos1 -= new Vector3(0, slope * 0.2f, 0);
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
        if (midNormal.x > 0)
        {
            pos2 += new Vector3(0, slope * originPoint2Ground * 2, 0);
        }
        else if (midNormal.x < 0)
        {
            pos2 -= new Vector3(0, slope * originPoint2Ground * 2, 0);
        }
        //加上在斜面上的时候，中线造成的偏移。
        pos2 -= new Vector3(0, slope * 0.2f, 0);
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
        pos -= new Vector3(0, slope * 0.2f, 0);
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
                if (jumpInstantSpeed == 0)
                {
                    state = RoleState.Grounded;
                    remainJumpTimes = 0;
                }
            }

        }
        else
        {
            //print("从高台跌落");
            //在空中的情况下，如果当前状态是grounded，说明这一帧从高台跌落。
            if (state == RoleState.Grounded)
            {
                //注意！需要判断是"非上升状态"才能将状态改成下落，否则会出现起跳变下落的bug...
                if (jumpInstantSpeed == 0)
                {
                    state = RoleState.Falling;
                    //跌落速度。高空跌落时，初始速度为0。当低空且贴近地面跌落时，初始下落速度设为较大值。                    
                    fallSpeed = 5 * (1 - slope);
                    //fallSpeed = 0;

                }
                if (multijump && slope < maxFrictionSlope)
                {
                    //从高处落下可开启二段跳
                    remainJumpTimes = 1;
                }
            }
        }
    }

    void Fall()
    {
        //当前帧下落距离
        float fallDis = fallSpeed * deltaTime;
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
            fallSpeed += fallAccelerateSpeed * deltaTime;
        }
    }

    //由fall调用。不对其它处开放。
    void DoFall(float fallDis, float dis)
    {
        if (fallDis < dis)
        {
            print("下落中");
            //速度=falldis*斜率。坡度越大，速度越慢。
            transform.position += Vector3.down * fallDis;
        }
        else if (fallDis == dis)
        {
            print("将要接触");
            transform.position += Vector3.down * fallDis;
        }
        else if (fallDis > dis)
        {
            print("将要超过");
            transform.position += Vector3.down * dis;
        }
    }

    //转身，自动设置faceleft值。
    public void Turn()
    {
        if (model.rotation.eulerAngles.y > 90)
        {
            //print("向右转");
            model.Rotate(Vector3.down, 180);
            faceLeft = false;
        }
        else
        {
            //print("向左转");
            model.Rotate(Vector3.up, 180);
            faceLeft = true;
        }
    }

    void MoveCheck()
    {
        moveToLeft = true;
        moveToRight = true;
        moveToUp = true;
        leftDis = 0;
        rightDis = 0;
        upDis = 0;

        //左上（可移动距离0.4）
        Debug.DrawRay(transform.position, new Vector3(-1f, 0.5f, 0) * 0.4f, Color.red);
        RaycastHit hit3;
        bool isHit3 = Physics.Raycast(transform.position, new Vector3(-1f, 0.5f, 0), out hit3, 0.4f, layerGround);
        if (isHit3)
        {
            moveToLeft = false;
            moveToUp = false;
        }

        //中上（可移动距离0.2）
        Debug.DrawRay(transform.position, new Vector3(0f, 1f, 0) * 1.2f, Color.red);
        RaycastHit hit4;
        bool isHit4 = Physics.Raycast(transform.position, new Vector3(0f, 1f, 0), out hit4, 1.2f, layerGround);
        if (isHit4)
        {
            upDis = hit4.distance - 0.2f;
            //注意！这里不能用绝对值，会造成向上穿越。
            if (upDis < 0.001f)
            {
                moveToUp = false;
            }
        }

        //右上（可移动距离0.4）
        Debug.DrawRay(transform.position, new Vector3(1f, 0.5f, 0) * 0.4f, Color.red);
        RaycastHit hit5;
        bool isHit5 = Physics.Raycast(transform.position, new Vector3(1f, 0.5f, 0), out hit5, 0.4f, layerGround);
        if (isHit5)
        {
            moveToUp = false;
            moveToRight = false;
        }

        //左（可移动距离0.4）
        Debug.DrawRay(transform.position, new Vector3(-1f, 0f, 0) * 1.4f, Color.red);
        RaycastHit hit6;
        bool isHit6 = Physics.Raycast(transform.position, new Vector3(-1f, 0f, 0), out hit6, 1.4f, layerGround);
        if (isHit6)
        {
            leftDis = hit6.distance - 0.45f;
            if (leftDis < 0.001f)
            {
                moveToLeft = false;
            }
        }

        //右（可移动距离0.4）
        Debug.DrawRay(transform.position, new Vector3(1f, 0f, 0) * 1.4f, Color.red);
        RaycastHit hit7;
        bool isHit7 = Physics.Raycast(transform.position, new Vector3(1f, 0f, 0), out hit7, 1.4f, layerGround);
        if (isHit7)
        {
            rightDis = hit7.distance - 0.45f;
            if (rightDis < 0.001f)
            {
                moveToRight = false;
            }
        }

        //如果左、右、上未命中。则使用左上，右上判断。
        //……逻辑暂缺！！


        //如果角色的滑落角度过大，则不能向上移动
        if (slope > maxFrictionSlope)
        {
            if (midNormal.x > 0)
            {
                moveToLeft = false;
            }
            else if (midNormal.x < 0)
            {
                moveToRight = false;
            }
        }

    }

    //输入力
    public void SetMove(float value)
    {
        horizontalInputSpeed = value;
        //有输入力时，清除外部受力。
        if (horizontalInputSpeed != 0)
        {
            horizontalExternalSpeed = 0;
        }
    }

    //外部瞬间力（落地会消失，输入会消失）
    public void SetExternalMove(float value)
    {
        horizontalExternalSpeed = value;
    }

    void KeepMoving()
    {
        //转向---------------------------begin
        //向右转
        if (horizontalInputSpeed > 0 && faceLeft)
        {
            Turn();
        }
        else if (horizontalInputSpeed < 0 && faceLeft == false)
        {
            Turn();
        }
        //转向---------------------------end

        MoveCheck();


        float _moveDis = 0;
        //计算移动距离（如果按左右方向键，则设置速度，否则速度递减）
        if (horizontalInputSpeed != 0)
        {
            _moveDis = horizontalInputSpeed * deltaTime * moveSpeed;
        }
        else if (horizontalExternalSpeed != 0)
        {
            //外力重置为0
            if (Mathf.Abs(GetMinDis()) < originPoint2Ground)
            {
                horizontalExternalSpeed = 0;
            }
            if (IsOnGround() == false && horizontalExternalSpeed != 0)
            {
                Vector3 externalVector = new Vector3(horizontalExternalSpeed, 0, 0);
                //受外力平移
                transform.Translate(externalVector * deltaTime * 5f);
            }
        }
        else if (lastFrameSpeedVector != Vector3.zero)
        {
            //惯性移动（冰面、大斜面）
            if (IsOnGround() == false)
            {
                //注意：惯性不能使用Y值。否则会影响跳跃高度，甚至穿越地面。
                Vector3 inertiaVector = new Vector3(lastFrameSpeedVector.x, 0, 0);
                transform.Translate(inertiaVector * deltaTime * inertia);
            }
        }

        //反穿越计算
        if (horizontalInputSpeed < 0 && moveToLeft)
        {
            if (Mathf.Abs(_moveDis) > leftDis && leftDis > 0)
            {
                //print("左。moveDis：" + _moveDis + "，remaindis：" + leftDis);
                _moveDis = -leftDis;
            }
        }
        else if (horizontalInputSpeed > 0 && moveToRight)
        {
            if (Mathf.Abs(_moveDis) > rightDis && rightDis > 0)
            {
                //print("右。moveDis：" + _moveDis + "，remaindis：" + rightDis);
                _moveDis = rightDis;
            }
        }


        //进行左右移动（有速度时才移动）
        if (_moveDis != 0)
        {
            if (_moveDis < 0 && moveToLeft)
            {
                transform.Translate(-getMoveVector() * _moveDis, Space.World);
            }
            if (_moveDis > 0 && moveToRight)
            {
                transform.Translate(getMoveVector() * _moveDis, Space.World);
            }
        }

        //反穿越移动（比反穿越更重要的作用是：防止滑落不到地面，因为快贴近地面时角度会变小。）
        if (state == RoleState.Grounded)
        {
            //只有小于最大摩擦角度，才进行反穿越处理。
            if (slope <= maxFrictionSlope)
            {
                if (leftDis < 0)
                {
                    //print("左移动：" + leftDis);
                    //0.02f此值越大，，X形斜面侧面抖动越大。
                    transform.Translate(-mainBody.right * leftDis * 0.2f, Space.World);
                }
                if (rightDis < 0)
                {
                    //print("右移动：" + mainBody.right * rightDis);
                    transform.Translate(mainBody.right * rightDis * 0.2f, Space.World);
                }
            }
        }
    }

    //判断是否单脚命中斜墙，斜墙斜率大于maxFrictionSlope，且接触距离为零。[这样会误以为在地面]
    bool CheckCondition()
    {
        //特殊情况！！贴大斜率墙面跳的时候，可能出现单脚线刚好接触墙面。贴墙连跳的Bug。
        bool conditionSpec = true;
        if (isFootHit == false && isFootHit1 == false && isFootHit2)
        {
            if (getSlope(footNormal2) >= maxFrictionSlope)
            {
                conditionSpec = false;
            }
        }
        if (isFootHit == false && isFootHit1 && isFootHit2 == false)
        {
            if (getSlope(footNormal1) >= maxFrictionSlope)
            {
                conditionSpec = false;
            }
        }
        return conditionSpec;
    }

    //是否在地面，包含冰面。
    public bool IsOnGround()
    {
        if (state == RoleState.Grounded)
        {
            return true;
        }
        else if (state == RoleState.Falling && Mathf.Abs(GetMinDis()) < 0.0001f)
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

    //是否在冰面（属于地面）
    public bool isOnIceGround()
    {
        //if (hasHitIceGround && (state == RoleState.Grounded || state == RoleState.Falling) && Mathf.Abs(disToGround) < 0.2f)
        if (hasHitIceGround && (state == RoleState.Grounded || state == RoleState.Falling) && Mathf.Abs(GetMinDis()) < 0.2f)
        {
            return true;
        }
        return false;
    }

    public void Kill()
    {
        this.jumpInstantSpeed = 0;
        this.gameObject.SetActive(false);
    }

    //计算检测到的斜率
    void setCheckedSlope()
    {
        float angle = Vector3.Angle(midNormal, Vector3.up);
        if (midNormal == Vector3.zero)
        {
            angle = 0;
        }
        slope = angle / 90;
    }

    //传入向量，返回斜率
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

    //获取移动朝向
    Vector3 getMoveVector()
    {
        //正面朝向
        Vector3 faceVector = Vector3.zero;

        float x = midNormal.y;
        float y = -midNormal.x;
        if (horizontalInputSpeed < 0)
        {
            x = -midNormal.y;
            y = midNormal.x;
        }
        if (midNormal.x == 0)
        {
            if (horizontalInputSpeed < 0)
            {
                faceVector.x = -1;
            }
            else if (horizontalInputSpeed > 0)
            {
                faceVector.x = 1;
            }
            faceVector.y = 0;
        }
        else
        {
            faceVector.x = x;
            faceVector.y = y;
        }
        Debug.DrawRay(transform.position, new Vector3(x, y, 0) * 3, Color.green);

        return faceVector;
    }

    //最低的脚的位置
    float GetMinLow()
    {
        //取命中，且大于-0.4f，的最小值。
        List<float> disList = new List<float>();
        if (isFootHit && disToGround > -originPoint2Ground)
        {
            disList.Add(disToGround);
        }
        if (isFootHit1 && disToGround1 > -originPoint2Ground)
        {
            disList.Add(disToGround1);
        }
        if (isFootHit2 && disToGround2 > -originPoint2Ground)
        {
            disList.Add(disToGround2);
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

    //最接近地面的距离
    float GetMinDis()
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
}
