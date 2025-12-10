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
    
    //TODO 이거 score 제대로
    double tap_note_score = 10;
    double hold_note_score = 20;
    double slide_note_score = 30;


    // Start is called before the first frame update
    void Start()
    {
        note = GetComponent<Note>();
        GameObject targetObject = note.targetObject;
        clue = targetObject.GetComponent<JudgementClueProvider>();
    }

    // Update is called once per frame
    void Update()
    {
        // tap note 
        if (note.type == NoteType.TAP)
        {
            //TODO 시간 단위 맞추기
            //TODO score 게산 제대로
            if (note.isBreakNote)
            {
                score_scale = 5;
                free_time = 20;
            }
            else
            {
                score_scale = 1;
                free_time = 80;
            }

            if (TimeManager.Instance.PROGRESS_TIME < (note.tapTime - free_time)){}
            else if (TimeManager.Instance.PROGRESS_TIME > (note.tapTime + free_time)){}
            else
            {
                if (((note.tapTime - free_time) <= clue.lastEnterSelectTime) & ((note.tapTime + free_time) >= clue.lastEnterSelectTime))
                {
                    if (((note.tapTime - free_time) <= clue.lastExitSelectTime) &((note.tapTime + free_time) >= clue.lastExitSelectTime))
                    {
                        if (!note.judgementArray[0]){
                        note.judgementArray[0] = true;
                        ScoreManager.Instance.AddScore(tap_note_score*score_scale);
                        }
                    }
                }
            }
        } 

        // hold note
        else if (note.type == NoteType.HOLD)
        {
            if (note.isBreakNote)
            {
                score_scale = 5;
                free_time = 20;
            }
            else
            {
                score_scale = 1;
                free_time = 80;
            }
            if (TimeManager.Instance.PROGRESS_TIME < (note.tapTime - free_time)){}
            else if (TimeManager.Instance.PROGRESS_TIME > (note.endTime + free_time)){}
            else
            {
                if (((note.tapTime - free_time) <= clue.lastEnterSelectTime) & ((note.tapTime + free_time) >= clue.lastEnterSelectTime))
                {
                    if (((note.endTime - free_time) <= clue.lastExitSelectTime) &((note.endTime + free_time) >= clue.lastExitSelectTime))
                    {
                        if (!note.judgementArray[0]){
                        note.judgementArray[0] = true;
                        ScoreManager.Instance.AddScore(hold_note_score*score_scale);
                        }
                    }
                }
            }
        }

        // slide note
        else if (note.type == NoteType.SLIDE)
        {
            List<GameObject> slide_notes = note.slideNotes;
            if (note.isBreakNote)
            {
                score_scale = 5;
                free_time = 20;
            }
            else
            {
                score_scale = 1;
                free_time = 80;
            }
            if (TimeManager.Instance.PROGRESS_TIME < (note.tapTime - free_time)){}
            else if (TimeManager.Instance.PROGRESS_TIME > (note.endTime + free_time)){}
            else
            {
                if (((note.tapTime - free_time) <= clue.lastEnterSelectTime) & ((note.tapTime + free_time) >= clue.lastEnterSelectTime))
                {
                    note.judgementArray[0] = true;
                }
                for (int i = 1; i < note.slideNotes.Count - 1; i++)
                {
                    if (slide_notes[i].GetComponent<JudgementClueProvider>().isSelected)
                    {
                        if (note.judgementArray[i-1])
                        {
                            note.judgementArray[i] = true;
                        }
                    }
                }
                if (((note.endTime - free_time) <= slide_notes[note.slideNotes.Count - 1].GetComponent<JudgementClueProvider>().lastEnterSelectTime) &((note.endTime + free_time) >= slide_notes[note.slideNotes.Count - 1].GetComponent<JudgementClueProvider>().lastEnterSelectTime))
                {
                    if (note.judgementArray[note.slideNotes.Count - 2])
                    {
                        if (!note.judgementArray[note.slideNotes.Count - 1]){
                        note.judgementArray[note.slideNotes.Count - 1] = true;
                        ScoreManager.Instance.AddScore(slide_note_score*score_scale);
                        }
                    }
                }
            }
        }
        
    }
}
