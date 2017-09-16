using UnityEngine;
using System.Collections;

public class MoveDetect : MonoBehaviour
{
    public bool isDebug;

    Transform body;

    PlayerControl role;

    GroundDetect groundDct;

    public bool processCross = true;
    public float unCrossSpeed = 0.2f;

    public float leftDis;
    public float rightDis;

    public bool moveToUp;
    public bool moveToLeft;
    public bool moveToRight;

    // Use this for initialization
    void Start()
    {
        role = GetComponent<PlayerControl>();
        groundDct = GetComponent<GroundDetect>();
        body = transform.FindChild("body");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateByParent()
    {
        if (processCross)
        {
            UnCross();
        }

        CheckUp();
        CheckLeft();
        CheckRight();

        //不能向上移动的时，也禁止向左上、右上移动。
        if (moveToUp == false)
        {
            if (groundDct.groundVector.y > 0)
            {
                moveToRight = false;
            }
            else if (groundDct.groundVector.y < 0)
            {
                moveToLeft = false;
            }
        }
    }

    void UnCross()
    {
        Vector3 origin = transform.position + new Vector3(0, 0.1f, 0);
        Vector3 size = new Vector3(0.01f, 0.3f, 1);
        RaycastHit hitLeft, hitRight;

        bool isHitLeft = Physics.BoxCast(origin + new Vector3(0, 0, 0), size / 2, -body.right, out hitLeft, body.rotation, 0.5f, LayerMask.GetMask("ground"), QueryTriggerInteraction.Ignore);
        bool isHitRight = Physics.BoxCast(origin - new Vector3(0, 0, 0), size / 2, body.right, out hitRight, body.rotation, 0.5f, LayerMask.GetMask("ground"), QueryTriggerInteraction.Ignore);


        if (isHitLeft)
        {
            float dis = hitLeft.distance;
            if (dis < 0.5f)
            {
                float disTemp = (0.5f - dis) * unCrossSpeed;
                transform.position += new Vector3(disTemp, 0, 0);
            }
        }

        if (isHitRight)
        {
            float dis = hitRight.distance;
            if (dis < 0.5f)
            {
                float disTemp = (0.5f - dis) * unCrossSpeed;
                //print("反穿" + disTemp);
                transform.position -= new Vector3(disTemp, 0, 0);
            }
        }

    }

    void CheckUp()
    {
        RaycastHit hitUp;
        bool isHitUp = Physics.BoxCast(transform.position, new Vector3(0.4f, 0.1f, 0.5f), Vector3.up, out hitUp, Quaternion.identity, 1, LayerMask.GetMask("ground"), QueryTriggerInteraction.Ignore);
        if (isHitUp && hitUp.distance < 0.4f)
        {
            moveToUp = false;
        }
        else
        {
            moveToUp = true;
        }
    }

    void CheckLeft()
    {
        moveToLeft = true;

        RaycastHit hitLeft;
        bool isHitLeft = Physics.BoxCast(transform.position + new Vector3(0, 0.2f, 0), new Vector3(0.1f, 0.2f, 0.5f), -body.right, out hitLeft, body.rotation, 1, LayerMask.GetMask("ground"));

        leftDis = hitLeft.distance;
        if (isHitLeft && hitLeft.distance < 0.6f)
        {
            moveToLeft = false;
        }
        else
        {
            bool isClosedGround = groundDct.isClosedGround && groundDct.disGround < 0.2f;
            //判断斜率，超过最大则不能移动
            if (isClosedGround && groundDct.midNormal.x > 0 && groundDct.slope > GameManager.Instance.Player.maxFrictionSlope)
            {
                moveToLeft = false;
            }
            //向右滑时不能向左移动
            if (role.slideProc.isSliding && groundDct.midNormal.x > 0)
            {
                moveToLeft = false;
            }
        }


        if (isDebug && isHitLeft)
        {
            Debug.DrawLine(transform.position + new Vector3(0, 0.1f, 0), hitLeft.point, Color.yellow);
        }
    }

    void CheckRight()
    {
        moveToRight = true;

        RaycastHit hitRight;
        bool isHitRight = Physics.BoxCast(transform.position + new Vector3(0, 0.2f, 0), new Vector3(0.1f, 0.2f, 0.5f), body.right, out hitRight, body.rotation, 1, LayerMask.GetMask("ground"));

        rightDis = hitRight.distance;
        if (isHitRight && hitRight.distance < 0.6f)
        {
            moveToRight = false;
        }
        else
        {
            bool isClosedGround = groundDct.isClosedGround && groundDct.disGround < 0.2f;
            //判断斜率，超过最大则不能移动
            if (isClosedGround && groundDct.midNormal.x < 0 && groundDct.slope > GameManager.Instance.Player.maxFrictionSlope)
            {
                moveToRight = false;
            }
            //向左滑时不能向右移动
            if (role.slideProc.isSliding && groundDct.midNormal.x < 0)
            {
                moveToRight = false;
            }

        }

    }

}
