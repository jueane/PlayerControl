using UnityEngine;
using System.Collections;

public class PositionAdjust : MonoBehaviour
{
    PlayerControl role;

    Transform body;

    bool isHitL;
    bool isHitR;

    //三维半径。
    Vector3 size = new Vector3(0.1f, 0.2f, 1);

    public float speed=1;

    GameObject boxLeft;
    GameObject boxRight;

    // Use this for initialization
    void Start()
    {
        role = GetComponent<PlayerControl>();

        body = transform.FindChild("body").transform;
        boxLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boxRight = GameObject.CreatePrimitive(PrimitiveType.Cube);

        boxLeft.transform.localScale = size;
        boxRight.transform.localScale = size;
    }

    // Update is called once per frame
    void Update()
    {
        if (role.state != RoleState.Grounded)
        {
            return;
        }
        //右俯，且角度过大，则不处理。
        if (role.groundDct.groundVector.y < 0 && role.groundDct.isHitRight == false && role.groundDct.slope > role.maxFrictionSlope)
        {
            return;
        }
        //左俯，且角度过大，则不处理。
        if (role.groundDct.groundVector.y > 0 && role.groundDct.isHitLeft == false && role.groundDct.slope > role.maxFrictionSlope)
        {
            return;
        }
        if (role.moveProc.horizontalInputSpeed!=0||role.moveProc.horizontalExternalSpeed!=0||role.moveProc.lastFrameSpeedVector!=Vector3.zero)
        {
            return;
        }

        //起点位置.
        Vector3 posL;
        Vector3 posR;

        posL = body.TransformPoint(body.localPosition + new Vector3(-0.5f, -0.4f, 0));
        posR = body.TransformPoint(body.localPosition + new Vector3(0.5f, -0.4f, 0));

        boxLeft.transform.position = posL;
        boxRight.transform.position = posR;

        isHitL = Physics.CheckBox(posL, size / 2, Quaternion.identity, LayerMask.GetMask("ground"), QueryTriggerInteraction.Ignore);
        isHitR = Physics.CheckBox(posR, size / 2, Quaternion.identity, LayerMask.GetMask("ground"), QueryTriggerInteraction.Ignore);

        if (isHitL != isHitR)
        {
            float tempSpeed = speed * role.deltaTime;
            if (isHitL == false)
            {
                transform.position += new Vector3(tempSpeed, 0, 0);
            }
            else if (isHitR == false)
            {
                transform.position += new Vector3(-tempSpeed, 0, 0);
            }
        }

    }
}
