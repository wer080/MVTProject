using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour
{
    public GameObject character;    
    public GameObject[] tileArr; // 플레이 영역이 되는 타일

    // 유저 타일 선택 모드
    private bool selMode;

    //선택된 타일 리스트
    private List<GameObject> selTiles = new List<GameObject>();
    //유저가 선택한 타일
    private GameObject previousTile;
    private GameObject selectedTile;
    private Vector2 wasSelTilePos;
    private bool sameTile;
    private bool isTile;
    
    //영역 테두리 좌표 탐색 기준 타일
    [HideInInspector]public GameObject lbTile; // 왼쪽 아래 타일
    [HideInInspector]public GameObject rtTile; // 오른쪽 위 타일

    //타일의 위치 저장
    private List<Vector2[]> tilePos = new List<Vector2[]>();

    //유저 터치 인식 변수
    private Vector2 touchBeganPos;
    private Vector2 touchEndPos;
    private Vector2 touchDif;
    private float minSwipe = 2f;



    [SerializeField]
    GameObject mapBorderLB;
    [SerializeField]
    GameObject mapBorderRT;

    //선택 모드
    private bool isVert; // True - 세로로 선택, False - 가로로 선택

    //캐릭터 스크립트 변수
    private CharacterController chrController;
    //추적자 스크립트 변수
    [SerializeField]
    GameObject chaser;
    private ChaserController chaserController;

    //플레이어가 움직이려는 방향 변수
    enum Direction
    {
        up, down, left, right
    }

    //유저 컨트롤 방향
    private Direction userDir;

    // Start is called before the first frame update
    void Start()
    {
        selMode = true;
        previousTile = null;
        sameTile = false;
        chrController = character.GetComponent<CharacterController>();
        chaserController = chaser.GetComponent<ChaserController>();
        SaveTilePos();
    }

    // Update is called once per frame
    void Update()
    {
        /*
         * 
         * 터치단계       : 타일판별 -> (가로/세로) 선택타일 리스트 작성 -> 테두리 판별(선택 타일들 제외) -> 가운데 타일인지 판별 -> 타일 위치 저장              **테두리 판별은 선택된 타일을 제외하여 계산하도록 하는 것이 맞는것 같다.
         * 터치드래그 단계: 가운데 타일에 따른 정렬 -> 유저의 스와이프 방향 판별 -> 방향에 따른 후보지에 타일 배정 및 alpha = 0.4로 변경
         * 터치엔드 단계  : 방해물 판별 검사 -> 타일 이동 확정 or 타일 위치 롤백 -> 캐릭터 이동
         * 
         * 
         * 타일이 전체 영역에서 밖으로 벗어나지 못하게. -> 일단 배경이 되는 타일에 대해 만들어두고, 그거의 lb,rt를 벗어나지 못하게 하는 방식이 좋을듯. 
         * 그러기 위해서는 전체 타일의 빈칸으로 구성된 타일이 존재해야 한다.
         * 
         * 
         */

        if(Input.GetMouseButtonDown(0))
        {
            if(!EventSystem.current.IsPointerOverGameObject())
            {
                TouchBegin();
                TurnManager.Instance.turnState = TurnManager.TurnState.playerTurn;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                //타일 선택을 했을 때
                if (selTiles.Count > 0 && sameTile == true)
                {
                    //유저가 스와이프 행동을 하는지 판별
                    if (UserSwipe())
                    {
                        // 선택된 타일이 가운데 타일일 경우, 캐릭터 위치 기준으로 정렬
                        if (IsBetween())
                        {
                            ArrangeTile();
                        }
                        // 터치 방향에 따라 예상 지점 표시 (* 추후, 원래 위치로 복귀 시, 타일 위치를 다시 되돌리는 기능 추가)
                        MoveTile();
                        selMode = false;
                    }
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (!selMode)
                {
                    if (UserSwipe())
                    {
                        if (selTiles.Count > 0)
                        {

                            if (DetectMoving())
                            {
                                if (CheckObstacle() || CheckOutsideMap())
                                {
                                    RollBackTiles();
                                    DeHilightOutline();
                                }
                                else if (!CheckObstacle() && !CheckOutsideMap())
                                {
                                    ConfirmTiles();
                                    InitTileTag();
                                    FindBorder();
                                    if (!chrController.FrontOfCharacter())
                                    {
                                        if (!chrController.CheckOutside())
                                        {
                                            chrController.StartCoroutine(chrController.MoveCharacter(chrController.toPos));
                                            chaserController.StartCoroutine(chaserController.ChasePlayer());
                                            DeHilightOutline();
                                            SaveTilePos();
                                            TurnManager.Instance.turnCount++;
                                        }
                                        else
                                        {
                                            RollBackTiles();
                                            DeHilightOutline();
                                        }
                                    }
                                    else
                                    {
                                        RollBackTiles();
                                        DeHilightOutline();
                                    }
                                }

                                sameTile = false;
                                previousTile = null;
                                selMode = true;
                            }
                        }
                    }
                }
                else
                {
                    //기존의 정보들 초기화
                    InitTileTag();
                    DeHilightOutline();

                    SelectTile();
                    FindBorder();
                }
            }
        }
    }

    // 선택 영역 배정
    void SelectTile()
    {
        if (isTile)
        {
            selTiles.Clear();
            if (sameTile)
                isVert = isVert ? isVert = false : isVert = true;
            else
                isVert = false;

            for (int i = 0; i < tileArr.Length; i++)
            {
                // 세로로 타일들을 선택할 때, 
                if (isVert)
                {
                    if (selectedTile.transform.localPosition.x - character.transform.localPosition.x != 0)
                    {
                        if (selectedTile.transform.localPosition.x - tileArr[i].transform.localPosition.x == 0)
                        {
                            tileArr[i].tag = "Selected";
                            selTiles.Add(tileArr[i]);
                        }
                    }
                }
                // 가로로 타일을 선택할 때
                else
                {
                    if (selectedTile.transform.localPosition.y - character.transform.localPosition.y != 0)
                    {
                        if (selectedTile.transform.localPosition.y - tileArr[i].transform.localPosition.y == 0)
                        {
                            tileArr[i].tag = "Selected";
                            selTiles.Add(tileArr[i]);
                        }
                    }
                }
            }

            HilightOutline();

        }


        /*
        selTiles.Clear();

        //클릭(터치)의 타일 판별
        int layerMask = 1 << LayerMask.NameToLayer("Tile");
        Vector2 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 10f, layerMask);

        // 타일을 터치했을 때,
        if(hit.collider != null)
        {
            if (previousTile == null)
            {
                previousTile = hit.collider.gameObject;
            }
            else
            {
                if (previousTile == hit.collider.gameObject)
                {
                    sameTile = true;
                    isVert = isVert ? isVert = false : isVert = true;
                }
                else
                {
                    previousTile = hit.collider.gameObject;
                    sameTile = false;
                    isVert = false;
                }
            }

            for (int i = 0; i < tileArr.Length; i++)
            {
                // 세로로 타일들을 선택할 때, 
                if (isVert)
                {
                    if (hit.transform.localPosition.x - character.transform.localPosition.x != 0)
                    {
                        if (hit.transform.localPosition.x - tileArr[i].transform.localPosition.x == 0)
                        {
                            tileArr[i].tag = "Selected";
                            selTiles.Add(tileArr[i]);
                        }
                    }
                }
                // 가로로 타일을 선택할 때
                else
                {
                    if (hit.transform.localPosition.y - character.transform.localPosition.y != 0)
                    {
                        if (hit.transform.localPosition.y - tileArr[i].transform.localPosition.y == 0)
                        {
                            tileArr[i].tag = "Selected";
                            selTiles.Add(tileArr[i]);
                        }
                    }
                }
            }
            HilightOutline();
        }

        Debug.Log(selTiles.Count);
        */
    }

    // 영역 경계 기준 타일 배정 문제 - 선택한 타일을 제외한 범위를 했어야 했다.
    void FindBorder()
    {
        for (int i = 0; i < tileArr.Length; i++)
        {
            if (tileArr[i].CompareTag("Unselected"))
            {
                lbTile = tileArr[i];
                rtTile = tileArr[i];
                break;
            }
        }

        for (int i = 0; i < tileArr.Length; i++)
        {
            if (tileArr[i].CompareTag("Unselected"))
            {
                if (lbTile.transform.localPosition.x >= tileArr[i].transform.localPosition.x)
                    if (lbTile.transform.localPosition.y >= tileArr[i].transform.localPosition.y)
                        lbTile = tileArr[i];

                if (rtTile.transform.localPosition.x <= tileArr[i].transform.localPosition.x)
                    if (rtTile.transform.localPosition.y <= tileArr[i].transform.localPosition.y)
                        rtTile = tileArr[i];
            }            
        }
    }
    

    // 가운데 타일 선택 여부 
    bool IsBetween()
    {
        if(isVert)
        {
            if (selTiles[0].transform.localPosition.x > lbTile.transform.localPosition.x && selTiles[0].transform.localPosition.x < rtTile.transform.localPosition.x)
                return true;
            else
                return false;
        }
        else
        {
            if (selTiles[0].transform.localPosition.y > lbTile.transform.localPosition.y && selTiles[0].transform.localPosition.y < rtTile.transform.localPosition.y)
                return true;
            else
                return false;
        }
    }

    // 타일 정렬 함수
    void ArrangeTile()
    {
        bool move; // true 일때(캐릭터가 선택한 타일들 보다 오른쪽(위쪽)에 있는 경우), false 일 때(캐릭터가 선택한 타일들 보다 왼쪽(아래쪽)에 있는 경우)

        if(isVert)
        {
            if (character.transform.localPosition.x > selTiles[0].transform.localPosition.x)
                move = true;
            else
                move = false;
        }
        else
        {
            if (character.transform.localPosition.y > selTiles[0].transform.localPosition.y)
                move = true;
            else
                move = false;
        }

        for(int i = 0; i < tileArr.Length; i++)
        {
            if(!tileArr[i].CompareTag("Selected"))
            {
                if(isVert)
                {
                    if(move)
                    {
                        if(tileArr[i].transform.localPosition.x < character.transform.localPosition.x)
                            tileArr[i].transform.localPosition = new Vector2(tileArr[i].transform.localPosition.x + 1, tileArr[i].transform.localPosition.y);
                    }
                    else
                    {
                        if (tileArr[i].transform.localPosition.x > character.transform.localPosition.x)
                            tileArr[i].transform.localPosition = new Vector2(tileArr[i].transform.localPosition.x - 1, tileArr[i].transform.localPosition.y);
                    }
                }
                else
                {
                    if (move)
                    {
                        if (tileArr[i].transform.localPosition.y < character.transform.localPosition.y)
                            tileArr[i].transform.localPosition = new Vector2(tileArr[i].transform.localPosition.x, tileArr[i].transform.localPosition.y + 1);
                    }
                    else
                    {
                        if (tileArr[i].transform.localPosition.y > character.transform.localPosition.y)
                            tileArr[i].transform.localPosition = new Vector2(tileArr[i].transform.localPosition.x, tileArr[i].transform.localPosition.y - 1);
                    }
                }
                
            }
        }
    }

    // 터치 지점
    void TouchBegin()
    {

        int layerMask = 1 << LayerMask.NameToLayer("Tile");
        Vector2 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        touchBeganPos = mousePos;
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 10f, layerMask);

        // 타일을 터치했을 때,
        if (hit.collider != null)
        {
            selectedTile = hit.collider.gameObject;
            isTile = true;

            if (previousTile == null)
            {
                previousTile = hit.collider.gameObject;
                sameTile = false;
            }
            else
            {
                if (previousTile == hit.collider.gameObject)
                    sameTile = true;
                else
                {
                    previousTile = hit.collider.gameObject;
                    sameTile = false;
                }
            }
        }
        else
        {
            selectedTile = null;
            isTile = false;
        }
    }

    // 유저 입력 판별
    bool UserSwipe()
    {
        Vector2 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        touchEndPos = mousePos;
        touchDif = (touchEndPos - touchBeganPos);
        float distance = Vector2.Distance(touchBeganPos, touchEndPos);
        if (distance > minSwipe)
        {
            if (touchDif.y > 0 && Mathf.Abs(touchDif.y) > Mathf.Abs(touchDif.x))
            {
                userDir = Direction.up;
            }
            else if (touchDif.y < 0 && Mathf.Abs(touchDif.y) > Mathf.Abs(touchDif.x))
            {
                userDir = Direction.down;
            }
            else if (touchDif.x > 0 && Mathf.Abs(touchDif.y) < Mathf.Abs(touchDif.x))
            {
                userDir = Direction.right;
            }
            else if (touchDif.x < 0 && Mathf.Abs(touchDif.y) < Mathf.Abs(touchDif.x))
            {
                userDir = Direction.left;
            }
            return true;
        }
        else
            return false;
    }

    // 타일 놓기
    void MoveTile()
    {
        for(int i = 0; i < selTiles.Count; i++)
        {
            if(isVert)
            {
                if (userDir == Direction.right)
                {
                    selTiles[i].transform.localPosition = new Vector2(rtTile.transform.localPosition.x + 1, selTiles[i].transform.localPosition.y);
                }
                else if (userDir == Direction.left)
                {
                    selTiles[i].transform.localPosition = new Vector2(lbTile.transform.localPosition.x - 1, selTiles[i].transform.localPosition.y);
                }
            }
            else
            {
                if (userDir == Direction.up)
                {
                    selTiles[i].transform.localPosition = new Vector2(selTiles[i].transform.localPosition.x, rtTile.transform.localPosition.y + 1);
                }
                else if (userDir == Direction.down)
                {
                    selTiles[i].transform.localPosition = new Vector2(selTiles[i].transform.localPosition.x, lbTile.transform.localPosition.y - 1);
                }
            }
        }        
    }

    // 방해물 검사
    bool CheckObstacle()
    {
        for (int i = 0; i < selTiles.Count; i++)
        {
            int obstacleLayer = (1 << LayerMask.NameToLayer("Obstacle") | 1 << LayerMask.NameToLayer("Tile"));
            Vector2 pos = selTiles[i].transform.position;
            RaycastHit2D[] hitInfo = Physics2D.RaycastAll(pos, Vector2.zero, 2f, obstacleLayer);

            foreach(RaycastHit2D hit in hitInfo)
            {
                if (hit.collider != null)
                {
                    if (hit.transform.CompareTag("Unselected"))
                    {
                        return true;
                    }
                       

                    if (hit.transform.CompareTag("Obstacle"))
                    {
                        return true;
                    }
                }
            }            
        }
        return false;
    }

    // 타일 위치 저장
    void SaveTilePos()
    {
        Vector2[] posArr = new Vector2[9];

        for (int i = 0; i < tileArr.Length ; i++)
        {
            posArr[i] = tileArr[i].transform.position;
        }

        tilePos.Add(posArr);
    }

    // 타일 위치 롤백
    void RollBackTiles()
    {
        int idx = tilePos.Count - 1;

        Vector2[] rollbackPos = tilePos[idx];

        for (int i = 0; i < rollbackPos.Length; i++)
        {
            tileArr[i].transform.position = rollbackPos[i];
        }

        //tilePos.RemoveAt(idx);

    }

    // 이동 확정
    void ConfirmTiles()
    {
        for (int i = 0; i < selTiles.Count; i++)
        {
            Material mat = selTiles[i].GetComponent<SpriteRenderer>().material;
            mat.SetFloat("_GhostTransparency", 0f);
        }

        SoundManager.instance.PlaySE("TileMove");
    }

    // 태그 복원
    void InitTileTag()
    {
        for (int i = 0; i < tileArr.Length; i++)
        {
            if(tileArr[i].CompareTag("Selected"))
            {
                tileArr[i].transform.tag = "Unselected";
            }
        }
    }

    // 아웃라인 강조
    void HilightOutline()
    {
        for(int i = 0; i < selTiles.Count; i++)
        {
            selTiles[i].GetComponent<ShaderControl>().enabled = true;
        }
    }

    void DeHilightOutline()
    {
        for (int i = 0; i < selTiles.Count; i++)
        {
            selTiles[i].GetComponent<ShaderControl>().enabled = false;
        }
    }

    bool DetectMoving()
    {
        for(int i = 0; i < selTiles.Count; i++)
        {
            if (wasSelTilePos.Equals(selTiles[i].transform.localPosition))
                return false;                
        }
        return true;
    }

    bool CheckOutsideMap()
    {
        for(int i = 0; i < selTiles.Count; i++)
        {
            if (selTiles[i].transform.position.x < mapBorderLB.transform.position.x || selTiles[i].transform.position.y < mapBorderLB.transform.position.y)
                return true;

            if (selTiles[i].transform.position.x > mapBorderRT.transform.position.x || selTiles[i].transform.position.y > mapBorderRT.transform.position.y)
                return true;
        }

        return false;
    }

}
