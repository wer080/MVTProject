using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterController : MonoBehaviour
{
    //캐릭터 오브젝트
    public GameObject character;
    [SerializeField] GameObject goal;

    Direction chrDir;

    [HideInInspector]public Vector2 toPos;

    enum Direction
    { 
        up, down, left, right
    }

    [SerializeField]
    GameObject playGround;

    TileController tileController;

    //애니메이션 변수
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        chrDir = Direction.down;
        tileController = playGround.GetComponent<TileController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        /*
         * 
         * 터치하면 캐릭터의 방향을 바꾼다.
         * 타일이 움직임이 확정되면 캐릭터의 방해물을 체크한다.
         * 방해물이 없다 판별되면 캐릭터의 움직인다.
         * 방해물이 있으면, 타일의 위치 롤백 
         * 
         * 캐릭터가 타일 영역 밖으로 벗어나지 못하게 제한하는 함수 필요 -> lb타일 / rt타일을 참조해서 해결하도록?
         * 
         */
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
                ChangeDirection();
        }

    }

    private void FixedUpdate()
    {
        if (CheckClear())
            GameManager.Instance.gameState = GameManager.State.Clear;

    }


    void ChangeDirection()
    {
        int chrLayer = 1 << LayerMask.NameToLayer("Character");
        Vector2 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 10f, chrLayer);

        if (hit.collider != null)
        {
            if (chrDir == Direction.up)
            {
                chrDir = Direction.right;
                anim.SetBool("up", false);
                anim.SetBool("right", true);
            }
            else if (chrDir == Direction.down)
            {
                chrDir = Direction.left;
                anim.SetBool("down", false);
                anim.SetBool("left", true);
            }
            else if (chrDir == Direction.left)
            {
                chrDir = Direction.up;
                anim.SetBool("left", false);
                anim.SetBool("up", true);
            }
            else if (chrDir == Direction.right)
            {
                chrDir = Direction.down;
                anim.SetBool("right", false);
                anim.SetBool("down", true);
            }
        }
    }

    public bool FrontOfCharacter()
    {
        int obstacleLayer = 1 << LayerMask.NameToLayer("InObstacle");

        toPos = character.transform.position;

        if (chrDir == Direction.up)
            toPos = new Vector2(toPos.x, toPos.y+1);
        else if (chrDir == Direction.down)
            toPos = new Vector2(toPos.x, toPos.y-1);
        else if (chrDir == Direction.left)
            toPos = new Vector2(toPos.x-1, toPos.y);
        else if (chrDir == Direction.right)
            toPos = new Vector2(toPos.x+1, toPos.y);

        RaycastHit2D hit = Physics2D.Raycast(toPos, Vector2.zero, 10f, obstacleLayer);

        if (hit.collider != null)
            return true;
        else
            return false;
    }

    public bool CheckOutside()
    {
        if (toPos.x < tileController.lbTile.transform.position.x || toPos.y < tileController.lbTile.transform.position.y)
            return true;

        if (toPos.x > tileController.rtTile.transform.position.x || toPos.y > tileController.rtTile.transform.position.y)
            return true;

        return false;
    }

    bool CheckClear()
    {
        if (this.transform.position.y == goal.transform.position.y - 1 && this.transform.position.x == goal.transform.position.x)
        {
            SoundManager.instance.PlaySE("Clear");
            return true;
        }
        else
            return false;
    }

    public IEnumerator MoveCharacter(Vector2 toPos)
    {
        float count = 0;
        Vector2 pos = character.transform.position;
        StartCoroutine(PlayWalkSound());
        while (true)
        {
            count += Time.deltaTime;
            character.transform.position = Vector2.Lerp(pos, toPos, count);
            anim.SetBool("walk", true);

            if (count >= 1)
            {
                character.transform.position = toPos;
                anim.SetBool("walk", false);
                TurnManager.Instance.turnState = TurnManager.TurnState.EnemyTurn;
                break;
            }
            yield return null;
        }
    }
    IEnumerator PlayWalkSound()
    {
        float time = 0;

        while (true)
        {
            time += 0.3f;
            SoundManager.instance.PlaySE("Walk");
            yield return new WaitForSeconds(0.3f);

            if (time >= 1)
                break;

            yield return null;
        }
    }
}
