using UnityEngine;
using System.Collections;

public class TestRay : MonoBehaviour
{
    public bool isHit;

    public RaycastHit hit;

    public GameObject obj;

    public float dis;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //RaycastHit hit;
        isHit = Physics.Linecast(transform.position, transform.position + Vector3.down * 5, out hit);
        //isHit = Physics.Raycast(transform.position, Vector3.down, out hit, 5);
        Debug.DrawRay(transform.position, Vector3.down * 5, Color.red);

        obj = hit.collider.gameObject;
        dis = hit.distance;


    }


    //void Update()
    //{
    //    //RaycastHit hit;
    //    isHit = Physics.Raycast(transform.position, Vector3.down, out hit, 5);
    //    Debug.DrawRay(transform.position, Vector3.down * 5, Color.red);

    //    obj = hit.collider.gameObject;
    //    dis = hit.distance;


    //}
}
