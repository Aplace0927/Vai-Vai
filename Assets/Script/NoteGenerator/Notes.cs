using System.Runtime.InteropServices;

namespace Notes
{
    public enum NoteLocation
    {
        A1 = 0, A2 = 1, A3 = 2, A4 = 3, A5 = 4, A6 = 5, A7 = 6, A8 = 7, // Outer Circle + Button
        B1, B2, B3, B4, B5, B6, B7, B8, // Inner Circle
        C                               // Center
    };

    public enum NoteType { TAP, HOLD, SLIDE };
    public enum SlideDirection { STRAIGHT, CCW, CW, SHORTEST }

    public class Note
    {
        public Note(
            NoteType noteType,
            NoteLocation noteLocation,
            double tapTime,
            Nullable<double> endTime,
            bool isAdjusted,
            bool isBreakNote,
            List<NoteLocation> slideTuples
        )
        {
            this.noteType = noteType;
            this.noteLocation = noteLocation;
            this.tapTime = tapTime;
            this.endTime = endTime;
            this.isAdjusted = isAdjusted;
            this.isBreakNote = isBreakNote;
            this.slideTuples = slideTuples ?? new List<NoteLocation>();
        }
        public NoteLocation noteLocation { get; set; }
        public NoteType noteType { get; set; }
        public double tapTime { get; set; }
        public Nullable<double> endTime { get; set; }
        public bool isAdjusted { get; set; }
        public bool isBreakNote { get; set; }
        public List<NoteLocation> slideTuples { get; set; }

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
}