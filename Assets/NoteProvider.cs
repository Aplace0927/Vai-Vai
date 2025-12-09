using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Note;

public enum NoteType
{
    TAP = 0,
    HOLD = 1,
    SLIDE = 2
}

public class Note
{
    public NoteType type;
    public GameObject targetObject;
    public double tapTime;
    public double endTime;
    public bool[] judgementArray;
    public bool isAdjusted;
    public bool isBreakNote;
    public GameObject[] slideNotes;
}

public class NoteProvider : MonoBehaviour
{
    [Header("Settings")]
    public GameObject notePrefab;
    public Transform spawnPoint;

    [Header("Note Targets")]
    [SerializeField] GameObject[] aNoteTargets;
    [SerializeField] GameObject[] bNoteTargets;
    [SerializeField] GameObject cNoteTargets;

    [Header("Note Sprites")]
    [SerializeField] Sprite tapSprite;
    [SerializeField] Sprite holdSprite;
    [SerializeField] Sprite starSprite;
    [SerializeField] Sprite slideSprite;
    [SerializeField] Sprite tapBreakSprite;
    [SerializeField] Sprite holdBreakSprite;
    [SerializeField] Sprite starBreakSprite;

    [Header("Note Material")]
    [SerializeField] Material PinkMaterial;
    [SerializeField] Material OrangeMaterial;
    // [SerializeField] Material YellowMaterial; // 동타 구현은 하지 않습니다!
    [SerializeField] Material BlueMaterial;
    // 보정 없음

    public double visibleOffsetTime = 0.33f; // t1 - 20fps
    public double moveOffsetTime = 0.16f;    // t1 - 10fps

    List<Note> noteList = new List<Note>();

    int nextNoteIndex;

    // parsed array
    // [A1, t1, [(bool)], bool, bool]

    // t1 - 20fps = 노트가 보이기 시작함
    // t1 - 10fps = 노트가 움직이기 시작함
    // t1 = 노트가 사라짐

    NoteType type;
    GameObject noteTargetObject;
    double noteTapTime;
    double noteEndTime;
    bool[] noteJudgementArray;
    bool isnoteAdjusted;
    bool isnoteBreakNote;

    private void Start()
    {
        noteList.Clear();
    }

    void Update()
    {
        double currentTime = Time.time;

        while (nextNoteIndex < noteList.Count)
        {
            Note nextNote = noteList[nextNoteIndex];
            double spawnTime = nextNote.tapTime - visibleOffsetTime;

            if (currentTime >= spawnTime)
            {
                SpawnNote(nextNote);
                nextNoteIndex++;
            }
            else
            {
                break;
            }
        }

    }

    void SpawnNote(Note noteData)
    {
        GameObject noteObject = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
        
        // NoteController controller = noteObject.GetComponent<NoteController>();
        // if (controller != null)
        {
            // TODO: 생성된 Object의 움직임을 기술하는 스크립트 추가
        }

        VisualizeNoteType(noteObject, noteData);

        if (noteData.type == NoteType.SLIDE)
        {
            foreach (var vertex in noteData.slideNotes)
            {
                // TODO: vertex에 따라 슬라이드 object 만들기
            }
        }

    void VisualizeNoteType(GameObject noteObject, Note noteData)
        {
            var spriteRenderer = noteObject.GetComponent<SpriteRenderer>();

            if (noteData.isBreakNote)
            {
                spriteRenderer.material = OrangeMaterial;
                switch (noteData.type)
                {
                    case NoteType.TAP:
                        spriteRenderer.sprite = tapBreakSprite;
                        break;
                    case NoteType.HOLD:
                        spriteRenderer.sprite = holdBreakSprite;
                        break;
                    case NoteType.SLIDE:
                        spriteRenderer.sprite = starBreakSprite;
                        break;
                }
            }
            else
            {
                switch (noteData.type)
                {
                    case NoteType.TAP:
                        spriteRenderer.sprite = tapSprite;
                        spriteRenderer.material = PinkMaterial;
                        break;
                    case NoteType.HOLD:
                        spriteRenderer.sprite = holdSprite;
                        spriteRenderer.material = PinkMaterial;
                        break;
                    case NoteType.SLIDE:
                        spriteRenderer.sprite = starSprite;
                        spriteRenderer.material = BlueMaterial;
                        break;
                }
            }
        }
    }

}
