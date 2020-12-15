using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterController : MonoBehaviour
{
    [HideInInspector]public bool detected;

    [SerializeField] Direction targetDir;

    [SerializeField] GameObject character;

    [SerializeField] GameObject warning;

    enum Direction
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        targetDir = Direction.right;
    }

    // Update is called once per frame
    void Update()
    {
        TargetPlayer();
    }

    private void FixedUpdate()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        if (Mathf.Abs(character.transform.position.y - this.transform.position.y) <= 1)
        {
            detected = true;
            if (!warning.activeSelf)
                warning.SetActive(true);
        }
        else
        {
            detected = false;
            if (warning.activeSelf)
                warning.SetActive(false);
        }
    }

    void TargetPlayer()
    {
        Vector2 toPos;
        if (detected)
        {
           if(this.transform.position.y == character.transform.position.y && TurnManager.Instance.turnState == TurnManager.TurnState.EnemyTurn)
            {
                if (targetDir == Direction.left)
                    toPos = Vector2.left;
                else
                    toPos = Vector2.right;

                int layerMask = (1 << LayerMask.NameToLayer("InObstacle") | 1 << LayerMask.NameToLayer("Character"));
                RaycastHit2D hit = Physics2D.Raycast(this.transform.position, toPos, 15f, layerMask);

                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.Log("GameOver");
                        GameManager.Instance.gameState = GameManager.State.Gameover;
                    }
                }
            }
        }
    }

}
