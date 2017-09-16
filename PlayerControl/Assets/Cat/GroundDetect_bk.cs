//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class GroundDetect : MonoBehaviour
//{
//    //打线起点
//    Vector3 origin;

//    //打线起点与中心点的偏移
//    float offsetY = 0.2f;

//    //检测线长度（腿的长度）
//    float legLength = 1.4f;

//    RoleCtrl3 role;

//    Transform body;

//    RaycastHit hitLeft;
//    RaycastHit hitRight;

//    public bool isHitLeft;
//    public bool isHitRight;

//    Vector3 pL;
//    Vector3 pR;

//    //检测到的地面向量
//    public Vector3 groundVector;

//    public Vector3 midNormal;

//    public float slopeLeft;
//    public float slopeRight;

//    public float slope;

//    public bool isClosedGround;
//    //与地面的距离
//    public float actualDisGround;
//    public float disGround;

//    public bool isHitIce;

//    public bool processBalance;

//    // Use this for initialization
//    void Start()
//    {
//        body = transform.FindChild("body");
//        role = GetComponent<RoleCtrl3>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        BalanceAdjust();
//    }

//    public void UpdateByParent()
//    {
//        DetectingGround();

//        CalculateDisGround();
//    }

//    void DetectingGround()
//    {
//        origin = transform.position + new Vector3(0, offsetY, 0);

//        //初始化数值
//        groundVector = Vector3.zero;
//        midNormal = Vector3.zero;
//        slopeLeft = 0;
//        slopeRight = 0;
//        slope = 0;
//        actualDisGround = 0;
//        disGround = 0;

//        Vector3 size = new Vector3(0.002f, 0.2f, 0);

//        //isHitLeft = Physics.BoxCast(transform.position + new Vector3(-0.3f, 0.2f, 0), size / 2, Vector3.down, out hitLeft, Quaternion.identity, 1.2f, LayerMask.GetMask("ground"), QueryTriggerInteraction.Collide);

//        //isHitRight = Physics.BoxCast(transform.position + new Vector3(0.3f, 0.2f, 0), size / 2, Vector3.down, out hitRight, Quaternion.identity, 1.2f, LayerMask.GetMask("ground"), QueryTriggerInteraction.Collide);


//        Vector3 originLeft = origin + new Vector3(-0.3f, 0, 0);
//        Vector3 originRight = origin + new Vector3(0.3f, 0, 0);

//        isHitLeft = Physics.Raycast(originLeft, Vector3.down, out hitLeft, legLength, LayerMask.GetMask("ground", "Platform"), QueryTriggerInteraction.Collide);

//        isHitRight = Physics.Raycast(originRight, Vector3.down, out hitRight, legLength, LayerMask.GetMask("ground", "Platform"), QueryTriggerInteraction.Collide);


//        float a, b;
//        a = hitLeft.normal.x;
//        b = hitRight.normal.x;

//        if (a < 0 && b > 0)
//        {
//            float absA = Mathf.Abs(a);
//            float absB = Mathf.Abs(b);

//            if (absA < absB)
//            {
//                isHitRight = Physics.Raycast(origin, Vector3.down, out hitRight, legLength, LayerMask.GetMask("ground", "Platform"), QueryTriggerInteraction.Collide);
//            }
//            if (absA > absB)
//            {
//                isHitLeft = Physics.Raycast(origin, Vector3.down, out hitLeft, legLength, LayerMask.GetMask("ground", "Platform"), QueryTriggerInteraction.Collide);
//            }

//        }

//        //Debug.DrawRay(origin + new Vector3(0.3f, 0, 0), Vector3.down, Color.red);


//        pL = hitLeft.point;
//        pR = hitRight.point;

//        pL.z = origin.z;
//        pR.z = origin.z;

//        //是否命中冰面
//        isHitIce = (isHitLeft && "IceGround".Equals(hitLeft.transform.tag)) || (isHitRight && "IceGround".Equals(hitRight.transform.tag));

//        //移动方向
//        if (isHitLeft && isHitRight)
//        {
//            Debug.DrawLine(pL, pR, Color.cyan);
//            //检测到的与地面平行的向量
//            groundVector = pR - pL;
//            midNormal = new Vector3(-groundVector.y, groundVector.x, 0);
//        }

//        //一个命中，一个未命中
//        if (isHitLeft != isHitRight)
//        {
//            if (isHitLeft)
//            {
//                //单脚命中的正常情况。（命中侧水平或向上倾斜）
//                groundVector = new Vector3(hitLeft.normal.y, -hitLeft.normal.x, 0);
//                Debug.DrawRay(pL, groundVector, Color.red);
//                midNormal = hitLeft.normal;
//            }
//            else if (isHitRight)
//            {
//                //单脚命中的正常情况。（命中侧水平或向上倾斜）
//                groundVector = new Vector3(hitRight.normal.y, -hitRight.normal.x, 0);
//                Debug.DrawRay(pR, groundVector, Color.cyan);
//                midNormal = hitRight.normal;


//                ////单侧命中，且命中一侧的角度向下倾斜，则另一侧算做最长命中。
//                //if (hitRight.normal.x > 0)
//                //{
//                //}
//                //else
//                //{
//                //    //单脚命中的正常情况。（命中侧水平或向上倾斜）
//                //    groundVector = new Vector3(hitRight.normal.y, -hitRight.normal.x, 0);
//                //    Debug.DrawRay(pR, groundVector, Color.cyan);
//                //    midNormal = hitRight.normal;
//                //}

//            }
//        }
//        //全未命中
//        if (isHitLeft == false && isHitRight == false)
//        {
//            groundVector = Vector3.right;
//            midNormal = Vector3.up;
//            isClosedGround = false;
//        }
//        else
//        {
//            isClosedGround = true;
//        }
//        groundVector.Normalize();

//        //检测到的地面方向
//        //Debug.DrawRay(transform.position, groundVector * 5, Color.green);
//        //Debug.DrawRay(transform.position, midNormal, Color.blue);

//        //左右脚斜率
//        slopeLeft = GetSlope(hitLeft.normal);
//        slopeRight = GetSlope(hitRight.normal);

//        //算出斜率
//        slope = GetSlope(midNormal);

//    }

//    void CalculateDisGround()
//    {
//        if (isHitLeft && isHitRight)
//        {
//            Vector3 centerGround = (pL + pR) / 2;
//            //Debug.DrawLine(origin, centerGround, Color.red);
//            disGround = origin.y - centerGround.y;
//        }
//        else if (isHitLeft)
//        {
//            Vector3 vecL = pL - origin;
//            float angle = Vector2.Angle(-midNormal, Vector3.down);
//            Vector3 Ver = Vector3.Project(vecL, -midNormal);
//            disGround = Ver.magnitude / Mathf.Cos(angle * Mathf.Deg2Rad);
//        }
//        else if (isHitRight)
//        {
//            Vector3 vecR = pR - origin;
//            float angle = Vector2.Angle(-midNormal, Vector3.down);
//            Vector3 Ver = Vector3.Project(vecR, -midNormal);
//            disGround = Ver.magnitude / Mathf.Cos(angle * Mathf.Deg2Rad);
//            //print("值:" + disGround + "," + Ver);
//        }
//        //至此已算出垂直距离。

//        //减去自身尺寸，同时减去因倾斜造成的尺寸增加。
//        disGround -= 0.5f + 0.1f * slope;

//        if (Mathf.Abs(disGround) < 0.02)
//        {
//            disGround = 0;
//        }
//    }

//    void BalanceAdjust()
//    {
//        if (processBalance == false)
//        {
//            return;
//        }

//        float maxRotation = 0.7f;

//        //旋转速度，根据离地高度决定
//        float rotationSpeed = 1 - disGround;
//        if (rotationSpeed < 0)
//        {
//            rotationSpeed = 0;
//        }
//        rotationSpeed = Mathf.Lerp(0.2f, 0.6f, slope);

//        rotationSpeed = 15f;
//        rotationSpeed *= role.deltaTime;

//        //旋转
//        if ((role.state != RoleState.Raising) && isHitLeft && isHitRight && slope < maxRotation && ((isHitLeft && slopeLeft < maxRotation) || (isHitRight && slopeRight < maxRotation)))
//        {
//            //慢慢旋转
//            Vector3 to = Vector3.Lerp(body.right, groundVector, rotationSpeed);
//            to = new Vector3(to.x, to.y, 0);
//            body.right = to;

//            //角色体向上的法线
//            midNormal.x = -groundVector.y;
//            midNormal.y = groundVector.x;
//        }
//        else
//        {
//            //恢复水平
//            float beforeAngle = Vector3.Angle(body.right, Vector3.right);

//            Vector3 to = Vector3.Lerp(body.right, Vector3.right, rotationSpeed);
//            to = new Vector3(to.x, to.y, 0);
//            body.right = to;

//            float afterAngle = Vector3.Angle(body.right, Vector3.right);
//            if (beforeAngle - afterAngle > 0 && Mathf.Abs(beforeAngle - afterAngle) < 0.1f)
//            {
//                body.right = Vector3.right;
//            }
//        }

//    }

//    public bool IsOnGround()
//    {
//        if (isClosedGround && disGround == 0)
//        {
//            return true;
//        }
//        return false;
//    }

//    public bool IsOnIceground()
//    {
//        if (isHitIce && IsOnGround())
//        {
//            return true;
//        }
//        if (isHitIce && role.state == RoleState.Falling && disGround < 0.15f)
//        {
//            return true;
//        }
//        return false;
//    }

//    float GetSlope(Vector3 vec)
//    {
//        float angle = Vector3.Angle(vec, Vector3.up);
//        if (vec.x == 0)
//        {
//            angle = 0;
//        }
//        float slopeT = angle / 90;
//        return slopeT;
//    }
//}
