using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{


    private static GameManager _instance;

    public PlayerControl Player;


    // Use this for initialization
    void Start()
    {
        Player.Init();
    }

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
