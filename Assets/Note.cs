using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public enum NoteType
    {
        TAP = 0,
        HOLD = 1,
        SLIDE = 2
    }

    public NoteType type;
    public GameObject targetObject;
    public double tapTime;
    public double endTime;
    public List<bool> judgementArray;
    public bool isAdjusted;
    public bool isBreakNote;
    public List<GameObject> slideNotes;

    public void initVariables(NoteType type, GameObject targetObject, double tapTime, double endTime, List<bool> judgementArray, bool isAdjusted, bool isBreakNote, List<GameObject> slideNotes)
    { 
        this.type = type;
        this.targetObject = targetObject;
        this.tapTime = tapTime;
        this.endTime = endTime;
        this.judgementArray = judgementArray;
        this.isAdjusted = isAdjusted;
        this.isBreakNote = isBreakNote;
        this.slideNotes = slideNotes;
    }
}
