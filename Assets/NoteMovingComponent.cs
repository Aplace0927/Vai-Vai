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
    private double? endTime;

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
        GameObject tarObj = noteinfomation.targetObject();
        targetPosition = tarObj.transform.position;
        // ��Ʈ ���� �ٶ󺸱� (z�� ����)
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
        if (noteinfomation.noteType == NoteType.TAP)
        {
            if (currentTime < moveStartTime)
            {
                // TODO: alpha�� 0���� 255���� ��ȭ
                ;
            }
            else if (currentTime < hitTime + 10.0f)
            {
                float totalMoveTime = (float)(hitTime - moveStartTime);
                float currentMoveTime = (float)(currentTime - moveStartTime);
                float progress = currentMoveTime / totalMoveTime;

                // ���� ����(Lerp)���� ��ġ �̵�
                transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            }
            else
            {
                transform.position = targetPosition;

                Destroy(gameObject);
            }
        }
        else if (noteinfomation.noteType == NoteType.HOLD)
        {
            // TODO: ��Ʈ ���̴� ��� �ٽ� �����ϱ�
        }
        else { }
    }
}
