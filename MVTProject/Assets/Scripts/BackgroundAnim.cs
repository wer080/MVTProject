using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAnim : MonoBehaviour
{
    [SerializeField] Transform[] background;
    [SerializeField] Transform Rabbit;

    private float speed = 2f;
    private float rabbitSpeed = 3f;
    private float viewWidth;
    private int startIndex = 0;
    private int endIndex = 2;
    private float extraDist = 2f;

    private Vector3 rabbitStartPos;

    // Start is called before the first frame update
    void Start()
    {
        viewWidth = Camera.main.orthographicSize * (float)Screen.width / (float)Screen.height;
        rabbitStartPos = Rabbit.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        MoveBackground();
        MoveRabbit();
    }

    void MoveBackground()
    {
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.left * speed * Time.deltaTime;
        transform.position = curPos + nextPos;

        if(background[endIndex].position.x < viewWidth)
        {
            Vector3 backSpritePos = background[startIndex].localPosition;
            Vector3 frontSpritePos = background[endIndex].localPosition;
            background[endIndex].transform.localPosition = backSpritePos + Vector3.right * viewWidth;

            int startIndexSave = startIndex;
            startIndex = endIndex;
            endIndex = startIndexSave - 1 == -1 ? background.Length -1 : startIndexSave - 1;
        }
    }

    void MoveRabbit()
    {
        Rabbit.transform.position = new Vector3(Rabbit.transform.position.x + Time.deltaTime * rabbitSpeed, Rabbit.transform.position.y, Rabbit.transform.position.z);

        if (Rabbit.transform.position.x > viewWidth + extraDist)
            Rabbit.transform.position = rabbitStartPos;

    }
}
