using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Note;


public class NoteProvider : MonoBehaviour
{
    public GameObject TestGameObject;

    [Header("Settings")]
    public Transform spawnPoint;

    [Header("Note Targets")]
    [SerializeField] GameObject[] aNoteTargets;
    [SerializeField] GameObject[] bNoteTargets;
    [SerializeField] GameObject cNoteTargets;

    [Header("Note Prefabs")]
    [SerializeField] GameObject tapPrefabs;
    [SerializeField] GameObject holdPrefabs;
    [SerializeField] GameObject starPrefabs;
    [SerializeField] GameObject slidePrefabs;
    [SerializeField] GameObject tapBreakPrefabs;
    [SerializeField] GameObject holdBreakPrefabs;
    [SerializeField] GameObject starBreakPrefabs;

    [Header("Note Material")]
    [SerializeField] Material PinkMaterial;
    [SerializeField] Material OrangeMaterial;
    // [SerializeField] Material YellowMaterial; // ��Ÿ ������ ���� �ʽ��ϴ�!
    [SerializeField] Material BlueMaterial;
    // ���� ����

    public double visibleOffsetTime = 0.33f; // t1 - 20fps
    public double moveOffsetTime = 0.16f;    // t1 - 10fps

    List<Note> noteList = new List<Note>();

    int nextNoteIndex;

    // parsed array
    // [A1, t1, [(bool)], bool, bool]

    // t1 - 20fps = ��Ʈ�� ���̱� ������
    // t1 - 10fps = ��Ʈ�� �����̱� ������
    // t1 = ��Ʈ�� �����

    private void Start()
    {
        nextNoteIndex = 0;
        noteList.Clear();

        Note note = new Note();
        note.type = NoteType.TAP;
        note.targetObject = TestGameObject;
        note.tapTime = 5.0f;
        note.endTime = 0;
        note.judgementArray = new List<bool>();
        note.isAdjusted = false;
        note.isBreakNote = false;
        note.slideNotes = null;

        noteList.Add(note);
        note.judgementArray.Add(TestGameObject);
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
        GameObject noteObject ;
        if (noteData.isBreakNote)
        {
            switch (noteData.type)
            {
                case NoteType.TAP:
                    noteObject = Instantiate(tapBreakPrefabs, spawnPoint.position, Quaternion.identity);
                    break;
                case NoteType.HOLD:
                    noteObject = Instantiate(holdBreakPrefabs, spawnPoint.position, Quaternion.identity);
                    break;
                case NoteType.SLIDE:
                    noteObject = Instantiate(starBreakPrefabs, spawnPoint.position, Quaternion.identity);
                    break;
                default:
                    noteObject = Instantiate(tapBreakPrefabs, spawnPoint.position, Quaternion.identity);
                    break;

            }
        }
        else
        {
            switch (noteData.type)
            {
                case NoteType.TAP:
                    noteObject = Instantiate(tapPrefabs, spawnPoint.position, Quaternion.identity);
                    break;
                case NoteType.HOLD:
                    noteObject = Instantiate(holdPrefabs, spawnPoint.position, Quaternion.identity);
                    break;
                case NoteType.SLIDE:
                    noteObject = Instantiate(starPrefabs, spawnPoint.position, Quaternion.identity);
                    break;
                default:
                    noteObject = Instantiate(tapBreakPrefabs, spawnPoint.position, Quaternion.identity);
                    break;
            }
        }
        Note myNote = noteObject.AddComponent<Note>();
        myNote.initVariables(NoteType.TAP, noteData.targetObject, noteData.tapTime, noteData.endTime, noteData.judgementArray,
            noteData.isAdjusted, noteData.isBreakNote, noteData.slideNotes);


        if (noteData.type == NoteType.SLIDE)
        {
            foreach (var vertex in noteData.slideNotes)
            {
                // TODO: vertex�� ���� �����̵� object �����
            }
        }
    }

}
