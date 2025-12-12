using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Note;
public class JudgementManager : MonoBehaviour
{
    Note note;
    JudgementClueProvider clue;
    int score_scale;
    double free_time; 
    GameObject targetObject;
    
    //TODO 이거 score 제대로
    double tap_note_score = 10;
    double hold_note_score = 20;
    double slide_note_score = 30;

    bool initialized = false;

    public void Initialize(Note noteComponent)
    {
        note = noteComponent;
        targetObject = note.targetObject();
    
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized) return;
        clue = targetObject.GetComponent<JudgementClueProvider>(); // update 

        // tap note 
        free_time = note.isBreakNote ? 20 : 80;

        if (note.noteType == NoteType.TAP)
        {
            if (TimeManager.Instance.PROGRESS_TIME < (note.tapTime - free_time)) { }
            else if (TimeManager.Instance.PROGRESS_TIME > (note.tapTime + free_time)) { }
            else
            {
                if (((note.tapTime - free_time) <= clue.lastEnterSelectTime) & ((note.tapTime + free_time) >= clue.lastEnterSelectTime))
                {
                    if (((note.tapTime - free_time) <= clue.lastExitSelectTime) & ((note.tapTime + free_time) >= clue.lastExitSelectTime))
                    {
                        if (!note.judgementArray[0]){
                        note.judgementArray[0] = true;
                        ScoreManager.Instance.AddScore(note.noteScore);

                        Debug.Log(note.noteScore);
                        }
                    }
                }
            }
        } 
        // hold note
        else if (note.noteType == NoteType.HOLD)
        {

            if (TimeManager.Instance.PROGRESS_TIME < (note.tapTime - free_time)) { }
            else if (TimeManager.Instance.PROGRESS_TIME > (note.endTime + free_time)) { }
            else
            {
                if (((note.tapTime - free_time) <= clue.lastEnterSelectTime) & ((note.tapTime + free_time) >= clue.lastEnterSelectTime))
                {
                    if (((note.endTime - free_time) <= clue.lastExitSelectTime) & ((note.endTime + free_time) >= clue.lastExitSelectTime))
                    {
                        if (!note.judgementArray[0])
                        {
                            note.judgementArray[0] = true;
                            ScoreManager.Instance.AddScore(note.noteScore);
                        }
                    }
                }
            }
        }

        // slide note
        else if (note.noteType == NoteType.SLIDE)
        {
            List<GameObject> slide_notes = note.targetSlideList();
            if (TimeManager.Instance.PROGRESS_TIME < (note.tapTime - free_time)){}
            else if (TimeManager.Instance.PROGRESS_TIME > (note.endTime + free_time)){}
            else
            {
                if (((note.tapTime - free_time) <= clue.lastEnterSelectTime) & ((note.tapTime + free_time) >= clue.lastEnterSelectTime))
                {
                    note.judgementArray[0] = true;
                }
                for (int i = 1; i < note.slideList.Count - 1; i++)
                {
                    if (slide_notes[i].GetComponent<JudgementClueProvider>().isSelected)
                    {
                        if (note.judgementArray[i - 1])
                        {
                            note.judgementArray[i] = true;
                        }
                    }
                }
                if (((note.endTime - free_time) <= slide_notes[note.slideList.Count - 1].GetComponent<JudgementClueProvider>().lastEnterSelectTime) & ((note.endTime + free_time) >= slide_notes[note.slideList.Count - 1].GetComponent<JudgementClueProvider>().lastEnterSelectTime))
                {
                    if (note.judgementArray[note.slideList.Count - 2])
                    {
                        if (!note.judgementArray[note.slideList.Count - 1])
                        {
                            note.judgementArray[note.slideList.Count - 1] = true;
                            ScoreManager.Instance.AddScore(note.noteScore);
                        }
                    }
                }
            }
        }

    }
}
