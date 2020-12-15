using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    private static TurnManager _instance;

    public static TurnManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(TurnManager)) as TurnManager;

                if (_instance == null)
                    Debug.Log("no TurnManager");
            }

            return _instance;
        }
    }

    [SerializeField] Text turnText;

    public enum TurnState
    { 
        playerTurn, EnemyTurn
    }

    [HideInInspector] public TurnState turnState;

    //전체적인 턴 횟수
    [HideInInspector] public int turnCount;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        turnCount = 0;
        turnState = TurnState.EnemyTurn;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        turnText.text = "" + turnCount;
    }





}
