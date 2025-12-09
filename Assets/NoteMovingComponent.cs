using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Note;

public class NoteMovingComponent : MonoBehaviour
{
    public GameObject noteObject;
    public Note noteinfomation;

    private double visibleOffsetTime = 0.33f; // t1 - 20fps
    private double moveOffsetTime = 0.16f;    // t1 - 10fps

    private double appearTime;
    private double moveStartTime;
    private double hitTime;
    private double endTime;

    Vector3 startPosition;
    Vector3 targetPosition;
    Vector3 tmpPosition;

    SpriteRenderer noteRenderer;
    Vector2 holdNoteVector;

    private void Start()
    {
        noteinfomation = noteObject.GetComponent<Note>();
        noteRenderer = GetComponent<SpriteRenderer>();

        hitTime = noteinfomation.tapTime;
        endTime = noteinfomation.endTime;
        appearTime = hitTime - visibleOffsetTime;
        moveStartTime = hitTime - moveOffsetTime;
        

        startPosition = transform.position;
        targetPosition = noteinfomation.targetObject.transform.position;
        // 노트 방향 바라보기 (z축 제외)
        Vector3 direction = targetPosition - transform.position;
        direction.x = 0;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }

    private void Update()
    {
        double currentTime = Time.time;

        // tap note
        if (noteinfomation.type == NoteType.TAP)
        {
            if (currentTime < moveStartTime)
            {
                // TODO: alpha값 0부터 255까지 변화
                ;
            }
            else if (currentTime < hitTime + 10.0f)
            {
                float totalMoveTime = (float)(hitTime - moveStartTime);
                float currentMoveTime = (float)(currentTime - moveStartTime);
                float progress = currentMoveTime / totalMoveTime;

                // 선형 보간(Lerp)으로 위치 이동
                transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            }
            else
            {
                transform.position = targetPosition;

                Destroy(gameObject);
            }
        }
        else if (noteinfomation.type == NoteType.HOLD)
        {
            // TODO: 노트 보이는 방식 다시 생각하기
        }
        else { }
    }
}
