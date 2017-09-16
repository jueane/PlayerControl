using UnityEngine;
using System.Collections;

public class TestBoxCast : MonoBehaviour {

    public int a;
    public bool isHit;
    public float angle;
    public Vector3 normal;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        RaycastHit hit;
        isHit = Physics.BoxCast(transform.position, Vector3.one * 0.5f, Vector3.down, out hit, Quaternion.identity, 5);

        normal = hit.normal;

        angle = Vector3.Angle(normal, Vector3.up);

        Vector3 midValue = Vector3.Lerp(transform.up, normal, 0.05f);


        float rot = angle / 90;
        if (rot < 1f)
        {

            transform.up = midValue;
        }
        else
        {
            midValue = Vector3.Lerp(transform.up, Vector3.up, 0.1f);
            transform.up = midValue;
        }

        //if (ishit)
        //{
        //    float rot = angle / 90;
        //    if (normal.x < 0)
        //    {
        //        transform.localRotation = new Quaternion(0, 0, rot*0.1f, 1);
        //    }
        //    else
        //    {
        //        transform.localRotation = new Quaternion(0, 0, -rot*0.1f, 1);

        //    }
        //}
        //else
        //{

        //}
	}
}
