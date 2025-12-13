using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using static NoteObjectManager;

public enum NoteLocation
{
    A1 = 0x00, A2 = 0x01, A3 = 0x02, A4 = 0x03, A5 = 0x04, A6 = 0x05, A7 = 0x06, A8 = 0x07, // Outer Circle + Button
    B1 = 0x10, B2 = 0x11, B3 = 0x12, B4 = 0x13, B5 = 0x14, B6 = 0x15, B7 = 0x16, B8 = 0x17, // Inner Circle
    C = 0x20                               // Center
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
        List<List<NoteLocation>> slideList
    )
    {
        this.noteType = noteType;
        this.noteLocation = noteLocation;
        this.tapTime = tapTime;
        this.endTime = endTime;
        this.isAdjusted = isAdjusted;
        this.isBreakNote = isBreakNote;
        this.slideList = slideList ?? new List<List<NoteLocation>>();

        this.judgementArray = Enumerable.Repeat(false, this.slideList.Count > 0 ? this.slideList.Count : 1).ToList();
        this.noteScore = 0.0;
    }
    public NoteLocation noteLocation { get; set; }
    public NoteType noteType { get; set; }
    public double tapTime { get; set; }
    public double? endTime { get; set; }
    public bool isAdjusted { get; set; }
    public bool isBreakNote { get; set; }
    public List<List<NoteLocation>> slideList { get; set; }
    public double noteScore { get; set; }

    public List<bool> judgementArray { get; set; }
    public GameObject targetObject()
    {
        return NotesCollection.BindLocation(this.noteLocation);
    }

    public List<List<GameObject>> targetSlideList()
    {
        return this.slideList.ConvertAll(
            (loc) => loc.ConvertAll(
                (noteLoc) => NotesCollection.BindLocation(noteLoc)
            )
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
    public static List<NoteLocation> InterpolateSlidesIndex(
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
    public static List<List<NoteLocation>> InterpolateLinearSlides(
        NoteLocation start,
        NoteLocation end,
        bool includeLastPoint = false
    )
    {
        // LIN  ANG  LIST
        // 1-2   45: [s.Out, e.Out]
        // 1-3   90: [s.Out, (s++.Out | s++.In), e.Out]
        // 1-4  135: [s.Out, (s.In | s++.In), (e--.In | e.In), e.Out]
        // 1-5  180: [s.Out, s.In, C, e.In, e.Out]
        // 1-6  225: [s.Out, (s.In | s--.In), (e++.In | e.In), e.Out]
        // 1-7  270: [s.Out, (s--.Out | s--.In), e.Out]
        // 1-8  315: [s.Out, e.Out]
        int slideAngles = ((int)end - (int)start + 8) % 8;
        List<List<NoteLocation>> linearSlides = slideAngles switch
        {
            1 => new List<List<NoteLocation>> {
                new List<NoteLocation> { OutOf(start)},
            },
            2 => new List<List<NoteLocation>> {
                new List<NoteLocation> { OutOf(start)},
                new List<NoteLocation> { OutOf(CWOf(start)), InOf(CWOf(start)) },
            },
            3 => new List<List<NoteLocation>> {
                new List<NoteLocation> { OutOf(start)},
                new List<NoteLocation> { InOf(start), InOf(CWOf(start)) },
                new List<NoteLocation> { InOf(end), InOf(CCWOf(end))}
            },
            4 => new List<List<NoteLocation>> {
                new List<NoteLocation> { OutOf(start)},
                new List<NoteLocation> { InOf(start)},
                new List<NoteLocation> { NoteLocation.C },
                new List<NoteLocation> { InOf(end)},
            },
            5 => new List<List<NoteLocation>> {
                new List<NoteLocation> { OutOf(start)},
                new List<NoteLocation> { InOf(start), InOf(CCWOf(start)) },
                new List<NoteLocation> { InOf(CWOf(end)), InOf(end)},
            },
            6 => new List<List<NoteLocation>> {
                new List<NoteLocation> { OutOf(start)},
                new List<NoteLocation> { OutOf(CCWOf(start)), InOf(CCWOf(start)) },
            },
            7 => new List<List<NoteLocation>> {
                new List<NoteLocation> { OutOf(start)},
            },
            0 => new List<List<NoteLocation>> { },
            _ => throw new ArgumentException("Invalid linear slide angle for interpolation"),
        };
        if (includeLastPoint)
        {
            linearSlides.Add(new List<NoteLocation> { OutOf(end) });
        }
        return linearSlides;
    }
    public static List<List<NoteLocation>> InterpolateSlides(
        List<NoteLocation> indexInterpolated
    )
    {
        List<List<NoteLocation>> slideNotes = new List<List<NoteLocation>>();
        foreach (var pair in indexInterpolated.Zip(indexInterpolated.Skip(1), (a, b) => (a, b)))
        {
            slideNotes.AddRange(
                InterpolateLinearSlides(pair.a, pair.b, includeLastPoint: false)
            );
        }
        slideNotes.Add(new List<NoteLocation> { OutOf(indexInterpolated.Last()) });
        return slideNotes;
    }
    private static NoteLocation CCWOf(NoteLocation loc)
    {
        if ((int)loc >= 0x00 && (int)loc <= 0x07)
        {   // Outer ring
            return (NoteLocation)(((int)loc - 1 + 8) % 8);
        }
        else if ((int)loc >= 0x10 && (int)loc <= 0x17)
        {   // Inner ring
            return (NoteLocation)(((int)loc - 0x10 - 1 + 8) % 8 + 0x10);
        }
        else
        {   // Center
            return NoteLocation.C;
        }
    }
    private static NoteLocation CWOf(NoteLocation loc)
    {
        if ((int)loc >= 0x00 && (int)loc <= 0x07)
        {   // Outer ring
            return (NoteLocation)(((int)loc + 1) % 8);
        }
        else if ((int)loc >= 0x10 && (int)loc <= 0x17)
        {   // Inner ring
            return (NoteLocation)(((int)loc - 0x10 + 1) % 8 + 0x10);
        }
        else
        {   // Center
            return NoteLocation.C;
        }
    }
    private static NoteLocation InOf(NoteLocation loc)
    {
        if (loc == NoteLocation.C)
        {
            return NoteLocation.C;
        }
        else
        {
            return (NoteLocation)(((int)loc % 0x8) + 0x10);
        }
    }
    private static NoteLocation OutOf(NoteLocation loc)
    {
        if (loc == NoteLocation.C)
        {
            return NoteLocation.C;
        }
        else
        {
            return (NoteLocation)((int)loc % 0x8);
        }
    }
}