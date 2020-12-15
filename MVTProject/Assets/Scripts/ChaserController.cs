using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserController : MonoBehaviour
{

    public GameObject character;


    [SerializeField]
    GameObject chaserObj;
    [SerializeField]
    int createTurn;
    [SerializeField]
    int moveCount;
    [SerializeField]
    GameObject lbBorder;
    [SerializeField]
    GameObject rtBorder;

    private bool createFlag;
    private Vector2 toPos;

    Animator anim;

    enum MoveDirection
    { 
        up, down, left, right
    }



    // Start is called before the first frame update
    void Start()
    {
        createFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(TurnManager.Instance.turnCount == createTurn)
        {
            if (!createFlag)
            {
                LocateChaser();
                anim = chaserObj.GetComponent<Animator>();
                chaserObj.SetActive(true);
            }
        }
    }

    private void LocateChaser()
    {
        float lDiff = lbBorder.transform.position.x - character.transform.position.x;
        float rDiff = rtBorder.transform.position.x - character.transform.position.x;

        if (lDiff >= rDiff)
            chaserObj.transform.position = new Vector2(lbBorder.transform.position.x, lbBorder.transform.position.y - 1);
        else
            chaserObj.transform.position = new Vector2(rtBorder.transform.position.x, lbBorder.transform.position.y - 1);
    }

    // 이동 방향을 정하는 함수
    private MoveDirection DecideDirection()
    {
        // 캐릭터와의 x좌표의 차이
        float xDiff = 0;
        xDiff = Mathf.Abs(character.transform.position.x - chaserObj.transform.position.x);

        // 캐릭터와의 y좌표의 차이
        float yDiff = 0;
        yDiff = Mathf.Abs(character.transform.position.y - chaserObj.transform.position.y);

        if (xDiff > yDiff)
        {
            if (character.transform.position.x > chaserObj.transform.position.x)
            {
                InitAnimParam();
                anim.SetBool("right", true);
                return MoveDirection.right;
            }
            else
            {
                InitAnimParam();
                anim.SetBool("left", true);
                return MoveDirection.left;
            }
        }
        else if (xDiff < yDiff)
        {
            if (character.transform.position.y > chaserObj.transform.position.y)
            {
                InitAnimParam();
                anim.SetBool("up", true);
                return MoveDirection.up;
            }
            else
            {
                InitAnimParam();
                anim.SetBool("down", true);
                return MoveDirection.down;
            }
                
        }
        else
        {
            Random.InitState((int)(Time.time * 100f));
            int dice = Random.Range(1, 7);
            if (dice % 2 == 0)
            {
                if (character.transform.position.x > chaserObj.transform.position.x)
                {
                    InitAnimParam();
                    anim.SetBool("right", true);
                    return MoveDirection.right;
                }
                else
                {
                    InitAnimParam();
                    anim.SetBool("left", true);
                    return MoveDirection.left;
                }
            }
            else
            {
                if (character.transform.position.y > chaserObj.transform.position.y)
                {
                    InitAnimParam();
                    anim.SetBool("up", true);
                    return MoveDirection.up;
                }
                else
                {
                    InitAnimParam();
                    anim.SetBool("down", true);
                    return MoveDirection.down;
                }
            }
        }
    }

    // 게임오버 조건
    private bool GameOver()
    {
        if (Vector2.Distance(character.transform.position, chaserObj.transform.position) < moveCount)
            return true;
        else
            return false;
    }

    private void CalDestination()
    {
        MoveDirection dir = DecideDirection();

        if (dir == MoveDirection.up)
            toPos = new Vector2(chaserObj.transform.position.x, chaserObj.transform.position.y + 1);

        else if (dir == MoveDirection.down)
            toPos = new Vector2(chaserObj.transform.position.x, chaserObj.transform.position.y - 1);

        else if (dir == MoveDirection.right)
            toPos = new Vector2(chaserObj.transform.position.x + 1, chaserObj.transform.position.y);

        else if (dir == MoveDirection.left)
            toPos = new Vector2(chaserObj.transform.position.x - 1, chaserObj.transform.position.y);
    }

    private void InitAnimParam()
    {
        anim.SetBool("up", false);
        anim.SetBool("down", false);
        anim.SetBool("left", false);
        anim.SetBool("right", false);
    }

    // 이동하는 함수
    // selfActivate가 트루 이면 -> 플레이어 턴 끝나면 이동시키도록 public 하게 코루틴. 캐릭터 이동과 똑같이.

    public IEnumerator ChasePlayer()
    {
        if (chaserObj.activeSelf)
        {
            for (int i = 0; i < moveCount; i++)
            {
                float count = 0;
                Vector2 pos = chaserObj.transform.position;
                CalDestination();

                while (true)
                {
                    count += Time.deltaTime;
                    chaserObj.transform.position = Vector2.Lerp(pos, toPos, count);
                    anim.SetBool("walk", true);

                    if (count >= 1)
                    {
                        chaserObj.transform.position = toPos;
                        anim.SetBool("walk", false);
                        break;
                    }
                    yield return null;
                }
            }
        }

        if (GameOver())
            GameManager.Instance.gameState = GameManager.State.Gameover;

    }
}
