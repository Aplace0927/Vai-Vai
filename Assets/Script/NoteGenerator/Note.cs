using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using static NoteObjectManager;

public enum NoteLocation
{
    A1 = 0, A2 = 1, A3 = 2, A4 = 3, A5 = 4, A6 = 5, A7 = 6, A8 = 7, // Outer Circle + Button
    B1, B2, B3, B4, B5, B6, B7, B8, // Inner Circle
    C                               // Center
};

public enum NoteType { TAP, HOLD, SLIDE };
public enum SlideDirection { STRAIGHT, CCW, CW, SHORTEST }

public class Note : MonoBehaviour
{
    public Note()
    {

    }

    public Note(
        NoteType noteType,
        NoteLocation noteLocation,
        double tapTime,
        double? endTime,
        bool isAdjusted,
        bool isBreakNote,
        List<NoteLocation> slideList
    )
    {
        this.noteType = noteType;
        this.noteLocation = noteLocation;
        this.tapTime = tapTime;
        this.endTime = endTime;
        this.isAdjusted = isAdjusted;
        this.isBreakNote = isBreakNote;
        this.slideList = slideList ?? new List<NoteLocation>();

        this.judgementArray = Enumerable.Repeat(false, this.slideList.Count > 0 ? this.slideList.Count : 1).ToList();
        this.noteScore = 0.0;
    }
    public NoteLocation noteLocation { get; set; }
    public NoteType noteType { get; set; }
    public double tapTime { get; set; }
    public double? endTime { get; set; }
    public bool isAdjusted { get; set; }
    public bool isBreakNote { get; set; }
    public List<NoteLocation> slideList { get; set; }
    public double noteScore { get; set; }

    public List<bool> judgementArray { get; set; }
    public GameObject targetObject()
    {
        return NotesCollection.BindLocation(this.noteLocation);
    }

    public List<GameObject> targetSlideList()
    {
        return this.slideList.ConvertAll(
            (loc) => NotesCollection.BindLocation(loc)
        );
    }

}
public abstract class NotesCollection
{
    public static NoteLocation OuterRingNotes(string c)
    {
        return c switch
        {
            "1" => NoteLocation.A1,
            "2" => NoteLocation.A2,
            "3" => NoteLocation.A3,
            "4" => NoteLocation.A4,
            "5" => NoteLocation.A5,
            "6" => NoteLocation.A6,
            "7" => NoteLocation.A7,
            "8" => NoteLocation.A8,
            _ => throw new ArgumentException("Invalid outer ring note character"),
        };
    }
    public static NoteLocation InnerRingNotes(string c)
    {
        return c switch
        {
            "1" => NoteLocation.B1,
            "2" => NoteLocation.B2,
            "3" => NoteLocation.B3,
            "4" => NoteLocation.B4,
            "5" => NoteLocation.B5,
            "6" => NoteLocation.B6,
            "7" => NoteLocation.B7,
            "8" => NoteLocation.B8,
            _ => throw new ArgumentException("Invalid inner ring note character"),
        };
    }
    public static GameObject BindLocation(NoteLocation location)
    {
        GameObject obj = GameObject.Find("NoteManager");
        NoteObjectManager noteobj = obj.GetComponent<NoteObjectManager>();

        return location switch
        {
            NoteLocation.A1 => noteobj.aNoteTargets[0],
            NoteLocation.A2 => noteobj.aNoteTargets[1],
            NoteLocation.A3 => noteobj.aNoteTargets[2],
            NoteLocation.A4 => noteobj.aNoteTargets[3],
            NoteLocation.A5 => noteobj.aNoteTargets[4],
            NoteLocation.A6 => noteobj.aNoteTargets[5],
            NoteLocation.A7 => noteobj.aNoteTargets[6],
            NoteLocation.A8 => noteobj.aNoteTargets[7],
            NoteLocation.B1 => noteobj.bNoteTargets[0],
            NoteLocation.B2 => noteobj.bNoteTargets[1],
            NoteLocation.B3 => noteobj.bNoteTargets[2],
            NoteLocation.B4 => noteobj.bNoteTargets[3],
            NoteLocation.B5 => noteobj.bNoteTargets[4],
            NoteLocation.B6 => noteobj.bNoteTargets[5],
            NoteLocation.B7 => noteobj.bNoteTargets[6],
            NoteLocation.B8 => noteobj.bNoteTargets[7],
            NoteLocation.C => noteobj.cNoteTargets,
            _ => throw new ArgumentException("Invalid note location for binding"),
        };
    }
    public static List<NoteLocation> InterpolateSlides(
        NoteLocation start,
        NoteLocation end,
        SlideDirection direction
    )
    {
        int slideAngles = ((int)end - (int)start + 8) % 8;
        if (direction == SlideDirection.SHORTEST)
        {
            direction = slideAngles < 4 ? SlideDirection.CW : (slideAngles > 4 ? SlideDirection.CCW : SlideDirection.STRAIGHT);
        }
        List<NoteLocation> slideNotes = new List<NoteLocation>();
        if (direction == SlideDirection.STRAIGHT || slideAngles == 0)
        {
            return new List<NoteLocation> { start, end };
        }
        for (int i = 0; i <= slideAngles; i++)
        {
            int intermediateIndex = direction switch
            {
                SlideDirection.CW => ((int)start + i) % 8,
                SlideDirection.CCW => ((int)start - i + 8) % 8,
                _ => throw new ArgumentException("Invalid slide direction for interpolation"),
            };
            slideNotes.Add((NoteLocation)intermediateIndex);
        }
        return slideNotes;
    }
}