//using UnityEngine;
//using System.Collections;

//public class GroundDetect : MonoBehaviour
//{
//    public Transform body;

//    public bool isHitLeft;
//    public bool isHitRight;

//    //检测到的地面向量
//    public Vector3 groundVector;

//    public Vector3 midNormal;

//    public float slopeLeft;
//    public float slopeRight;
//    public float slope;

//    public bool isClosedGround;
//    //与地面的距离
//    public float disGround;

//    // Use this for initialization
//    void Start()
//    {
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        Vector3 size = new Vector3(0.2f, 0.2f, 0);

//        RaycastHit hitLeft;
//        RaycastHit hitRight;

//        isHitLeft = Physics.BoxCast(transform.position + new Vector3(-0.3f, 0.2f, 0), size / 2, Vector3.down, out hitLeft, Quaternion.identity, 1.5f, LayerMask.GetMask("ground"), QueryTriggerInteraction.Ignore);
//        isHitRight = Physics.BoxCast(transform.position + new Vector3(0.3f, 0.2f, 0), size / 2, Vector3.down, out hitRight, Quaternion.identity, 1.5f, LayerMask.GetMask("ground"), QueryTriggerInteraction.Ignore);

//        //移动方向
//        if (isHitLeft && isHitRight)
//        {
//            Vector3 pL = hitLeft.point;
//            Vector3 pR = hitRight.point;
//            pL.z = 0;
//            pR.z = 0;

//            Debug.DrawLine(pL, pR, Color.cyan);

//            Vector3 vec1Temp = transform.position - pL;

//            //轴点到地面的向量
//            Vector2 vecToGround = Vector3.ProjectOnPlane(vec1Temp, groundVector);

//            disGround = vecToGround.magnitude;

//            Debug.DrawRay(transform.position + new Vector3(0.001f, 0, 0), -vecToGround, Color.red);

//            //检测到的与地面平行的向量
//            groundVector = pR - pL;
//            midNormal = new Vector3(-groundVector.y, groundVector.x, 0);

//            //检测到的地面方向
//            //Debug.DrawRay(transform.position, groundVector*2, Color.green);
//        }
//        //一个命中，一个未命中
//        if (isHitLeft != isHitRight)
//        {
//            if (isHitLeft)
//            {
//                groundVector = new Vector3(hitLeft.normal.y, -hitLeft.normal.x, 0);
//                midNormal = hitLeft.normal;
//            }
//            else if (isHitRight)
//            {
//                groundVector = new Vector3(-hitRight.normal.y, hitRight.normal.x, 0);
//                midNormal = hitRight.normal;
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

//        //左右脚斜率
//        slopeLeft = GetSlope(hitLeft.normal);
//        slopeRight = GetSlope(hitRight.normal);

//        //算出斜率
//        slope = GetSlope(midNormal);

//        //旋转
//        if ((isHitLeft && slopeLeft < 0.5f) || (isHitRight && slopeRight < 0.5f))
//        {
//            //慢慢旋转
//            Vector3 to = Vector3.Lerp(body.right, groundVector, 0.12f);
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

//            Vector3 to = Vector3.Lerp(body.right, Vector3.right, 0.12f);
//            to = new Vector3(to.x, to.y, 0);
//            body.right = to;

//            float afterAngle = Vector3.Angle(body.right, Vector3.right);
//            if (beforeAngle - afterAngle > 0 && Mathf.Abs(beforeAngle - afterAngle) < 0.1f)
//            {
//                body.right = Vector3.right;
//            }
//        }

//        Debug.DrawRay(body.position, midNormal,Color.black);
//        float angle = Vector3.Angle(midNormal, Vector3.up);
//        if (midNormal.x == 0)
//        {
//            angle = 0;
//        }
//        slope = angle / 90;


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
