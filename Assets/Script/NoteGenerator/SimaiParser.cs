using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using Notes;

namespace SimaiParser
{
    public class LevelData
    {
        public string LevelString { get; set; }
        public string RawNoteString { get; set; }
        public List<Note> NoteString { get; set; }

        public LevelData(
            string levelString = "",
            string rawNoteString = ""
        )
        {
            LevelString = levelString;
            RawNoteString = rawNoteString;
            NoteString = new List<Note>();
        }
        public void ParseNotes()
        {
            double NoteTiming = 0.0f;
            double CurrentMeasureFrequency = 0.0f;
            double MusicBPM = 0.0f;
            List<Note> parsedNotes = new List<Note> { };

            while (RawNoteString.Length > 0)
            {
                switch (RawNoteString[0])
                {
                    case '(': // BPM
                        MusicBPM = GetBPM();
                        break;
                    case '{': // Notes per measure
                        double NotesPerMeasure = GetNotesPerMeasure();
                        CurrentMeasureFrequency = 60.0f / MusicBPM * 4.0f / NotesPerMeasure;
                        break;
                    case 'E': // End of Notes
                        RawNoteString = RawNoteString.Remove(0, 1);
                        // System.Console.WriteLine("End of Notes");
                        return;
                    default:
                        // '{N}xx1/xx2,yy1/yy2/yy3,zzz,,,ww1/ww2,,,'
                        List<string> SameMeasureNoteSegment = GetNoteSegment().Trim().Split(',').ToList();
                        // ['xx1/xx2', 'yy1/yy2/yy3', 'zzz', '', '', 'ww1/ww2', '', '', '']
                        foreach (string noteData in SameMeasureNoteSegment)
                        {
                            // noteData: 'yy1/yy2/yy3'
                            if (noteData.Length == 0)
                            {
                                NoteTiming += CurrentMeasureFrequency;
                                continue;
                            }
                            List<string> sameTimeNotes = noteData.Trim().Split('/').ToList();
                            foreach (string sameTimeNote in sameTimeNotes)
                            {
                                if (sameTimeNote.Length == 0) continue;
                                NoteString = NoteString.Concat(ParseSingleNote(sameTimeNote, NoteTiming, CurrentMeasureFrequency)).ToList();
                            }
                            NoteTiming += CurrentMeasureFrequency;
                        }
                        break;
                }
            }
        }
        private double GetNotesPerMeasure() // {N}
        {
            int startIndex = RawNoteString.IndexOf("{");
            int endIndex = RawNoteString.IndexOf("}", startIndex);

            if (startIndex == -1 || endIndex == -1)
            {
                throw new System.Exception("Notes per measure not found in note string.");
            }

            string NotesPerMeasureString = RawNoteString.Substring(startIndex + 1, endIndex - startIndex - 1);

            RawNoteString = RawNoteString.Remove(startIndex, endIndex - startIndex + 1);
            return double.Parse(NotesPerMeasureString);
        }
        private double GetBPM() // (BPM)
        {
            int startIndex = RawNoteString.IndexOf("(");
            int endIndex = RawNoteString.IndexOf(")", startIndex);

            if (startIndex == -1 || endIndex == -1)
            {
                throw new System.Exception("BPM not found in note string.");
            }

            string bpmString = RawNoteString.Substring(startIndex + 1, endIndex - startIndex - 1);

            RawNoteString = RawNoteString.Remove(startIndex, endIndex - startIndex + 1);
            return double.Parse(bpmString);
        }
        private string GetNoteSegment()
        {
            int nearestTokenIndex = new[] { 'E', '{', '(' }
                .Select(c =>
                {
                    int idx = RawNoteString.IndexOf(c);
                    return idx != -1 ? idx : RawNoteString.Length;
                })
                .Min();

            string noteSegment = RawNoteString.Substring(0, nearestTokenIndex).Trim();
            RawNoteString = RawNoteString.Remove(0, nearestTokenIndex);
            return noteSegment;
        }

        private enum __ParseState
        {
            INIT, FINI, INVALID,
            LOCATION_PARSED, HOLD_PARSED, SLIDE_TYPE_PARSED, SLIDE_DEST_PARSED, SLIDE_DURATION_PARSED
        }
        private static (string? location, string remaining) ConsumeLocation(string noteData)    // LOC
        {
            if (noteData.Length == 0) return (null, noteData);
            if ("12345678".Contains(noteData[0]))
            {
                return (noteData[0].ToString(), noteData[1..]);
            }
            return (null, noteData);
        }
        private static (string? breakFlag, string remaining) ConsumeBreakFlag(string noteData)    // ADJ:B
        {
            if (noteData.Length == 0) return (null, noteData);
            if (noteData[0] == 'b')
            {
                return ("b", noteData[1..]);
            }
            return (null, noteData);
        }
        private static (string? nonAdjustFlag, string remaining) ConsumeNonAdjustFlag(string noteData)    // ADJ:X
        {
            if (noteData.Length == 0) return (null, noteData);
            if (noteData[0] == 'x')
            {
                return ("x", noteData[1..]);
            }
            return (null, noteData);
        }
        private static (string? starFlag, string remaining) ConsumeStarFlag(string noteData)                // ADJ:*
        {
            if (noteData.Length == 0) return (null, noteData);
            if (noteData[0] == '*')
            {
                return ("*", noteData[1..]);
            }
            return (null, noteData);

        }
        private static (string? fireworkFlag, string remaining) ConsumeFireworkFlag(string noteData)        // ADJ:F
        {
            if (noteData.Length == 0) return (null, noteData);
            if (noteData[0] == 'f')
            {
                return ("f", noteData[1..]);
            }
            return (null, noteData);
        }
        private static (string? slideType, string remaining) ConsumeSlideType(string noteData)      // SLD
        {
            if (noteData.Length == 0) return (null, noteData);
            if ("-<>^".Contains(noteData[0]))
            {
                return (noteData[0].ToString(), noteData[1..]);
            }
            return (null, noteData);
        }
        private static (string? lastingBeat, string remaining) ConsumeDuration(string noteData)     // DUR
        {
            int startIndex = noteData.IndexOf("[");
            int coloneIndex = noteData.IndexOf(":", startIndex);
            int endIndex = noteData.IndexOf("]", startIndex);
            if (startIndex != 0 || coloneIndex == -1 || endIndex == -1)
            {
                return (null, noteData);
            }
            string denominator = noteData.Substring(startIndex + 1, coloneIndex - startIndex - 1);
            string numerator = noteData.Substring(coloneIndex + 1, endIndex - coloneIndex - 1);
            return (numerator + "/" + denominator, noteData.Remove(0, endIndex - startIndex + 1));
        }
        private static (string? holdFlag, string remaining) ConsumeHoldFlag(string noteData)         // HLD
        {
            if (noteData.Length == 0) return (null, noteData);
            if (noteData[0] != 'h')
            {
                return (null, noteData);
            }
            return ("h", noteData[1..]);
        }
        private static (string? eof, string remaining) ConsumeEOF(string noteData) // EOF
        {
            if (noteData.Length == 0) return (";", noteData);   // EOF is optional: Not null then ok

            return (null, noteData);
        }
        private List<Note> ParseSingleNote(string noteData, double currentTimeStamp, double unitMeasure)
        {

            __ParseState currentState = __ParseState.INIT;

            List<Note> noteList = new List<Note> { };

            List<NoteLocation> slideTuples = new List<NoteLocation> { };
            List<double> slideDurations = new List<double> { };

            string notes = noteData.Trim();

            bool isBreakNote = false;
            bool isAdjusted = true;

            // Variables (can't init)
            NoteLocation noteLocation = default!;
            SlideDirection slideDirection = default!;

            bool TryParse(Func<string, (string?, string)> parser, Action<string> onSuccess, __ParseState nextState)
            {
                var (token, remaining) = parser(notes);

                if (token != null)
                {
                    notes = remaining;        // Update the string cursor
                    currentState = nextState; // Update the state machine
                    onSuccess(token);         // Run the side-effect
                    return true;              // Signal success to stop checking others
                }
                return false;
            }

            while (true)
            {
                //System.Console.WriteLine($"State: {currentState}, Remaining: '{notes}'");
                switch (currentState)
                {
                    case __ParseState.INIT:
                        _ = TryParse(ConsumeLocation, (token) =>
                            {
                                noteLocation = NotesCollection.OuterRingNotes(token);
                                slideTuples.Add(noteLocation);
                                isBreakNote = false;
                                isAdjusted = true;
                            }, __ParseState.LOCATION_PARSED) ||
                            TryParse((s) => (s, ""), (token) => { throw new ArgumentException("Parse failed in State: " + currentState.ToString()); }, __ParseState.INVALID);
                        break;
                    case __ParseState.LOCATION_PARSED:
                        _ = TryParse(ConsumeEOF, (token) =>
                            {
                                Note tapNote = new Note(
                                    NoteType.TAP,
                                    noteLocation,
                                    currentTimeStamp,
                                    null,
                                    isAdjusted,
                                    isBreakNote,
                                    new List<NoteLocation>()
                                );
                                noteList.Add(tapNote);
                            }, __ParseState.FINI) ||
                            TryParse(ConsumeBreakFlag, (token) => { isBreakNote = true; }, __ParseState.LOCATION_PARSED) ||
                            TryParse(ConsumeNonAdjustFlag, (token) => { isAdjusted = false; }, __ParseState.LOCATION_PARSED) ||
                            TryParse(ConsumeFireworkFlag, (token) => { /* Ignore for now */ }, __ParseState.LOCATION_PARSED) ||
                            TryParse(ConsumeHoldFlag, (token) => { /* For only transition*/}, __ParseState.HOLD_PARSED) ||
                            TryParse(ConsumeSlideType, (token) =>
                            {
                                slideDirection = token switch
                                {
                                    "-" => SlideDirection.STRAIGHT,
                                    "<" => SlideDirection.CCW,
                                    ">" => SlideDirection.CW,
                                    "^" => SlideDirection.SHORTEST,
                                    _ => throw new ArgumentException("Invalid slide direction character"),
                                };
                            }, __ParseState.SLIDE_TYPE_PARSED) ||
                            TryParse((s) => (s, ""), (token) => { throw new ArgumentException("Parse failed in State: " + currentState.ToString()); }, __ParseState.INVALID);
                        break;
                    case __ParseState.HOLD_PARSED:
                        _ = TryParse(ConsumeDuration, (token) =>
                            {
                                Note holdNote = new Note(
                                    NoteType.HOLD,
                                    noteLocation,
                                    currentTimeStamp,
                                    currentTimeStamp + unitMeasure * double.Parse(token.Split('/')[0]) / double.Parse(token.Split('/')[1]),
                                    isAdjusted,
                                    isBreakNote,
                                    new List<NoteLocation>()
                                );
                                noteList.Add(holdNote);
                            }, __ParseState.FINI) ||
                            TryParse(ConsumeBreakFlag, (token) => { isBreakNote = true; }, __ParseState.HOLD_PARSED) ||
                            TryParse(ConsumeNonAdjustFlag, (token) => { isAdjusted = false; }, __ParseState.HOLD_PARSED) ||
                            TryParse(ConsumeFireworkFlag, (token) => { /* Ignore for now */ }, __ParseState.HOLD_PARSED) ||
                            TryParse((s) => (s, ""), (token) => { throw new ArgumentException("Parse failed in State: " + currentState.ToString()); }, __ParseState.INVALID);
                        break;
                    case __ParseState.SLIDE_TYPE_PARSED:
                        _ = TryParse(ConsumeLocation, (token) =>
                        {
                            // Considering slide destination, check every outer ring note
                            slideTuples.AddRange(NotesCollection.InterpolateSlides(
                                noteLocation,
                                NotesCollection.OuterRingNotes(token),
                                slideDirection
                            ));
                        }, __ParseState.SLIDE_DEST_PARSED) ||
                            TryParse((s) => (s, ""), (token) => { throw new ArgumentException("Parse failed in State: " + currentState.ToString()); }, __ParseState.INVALID);
                        break;
                    case __ParseState.SLIDE_DEST_PARSED:
                        _ = TryParse(ConsumeBreakFlag, (token) => { isBreakNote = true; }, __ParseState.SLIDE_DEST_PARSED) ||
                            TryParse(ConsumeNonAdjustFlag, (token) => { isAdjusted = false; }, __ParseState.SLIDE_DEST_PARSED) ||
                            TryParse(ConsumeFireworkFlag, (token) => { /* Ignore for now */ }, __ParseState.SLIDE_DEST_PARSED) ||
                            TryParse(ConsumeDuration, (token) => { slideDurations.Add(Double.Parse(token.Split('/')[0]) / Double.Parse(token.Split('/')[1])); }, __ParseState.SLIDE_DURATION_PARSED) ||
                            TryParse(ConsumeSlideType, (token) =>
                            {
                                slideDirection = token switch
                                {
                                    "-" => SlideDirection.STRAIGHT,
                                    "<" => SlideDirection.CCW,
                                    ">" => SlideDirection.CW,
                                    "^" => SlideDirection.SHORTEST,
                                    _ => throw new ArgumentException("Invalid slide direction character"),
                                };
                            }, __ParseState.SLIDE_TYPE_PARSED) ||
                            TryParse((s) => (s, ""), (token) => { throw new ArgumentException("Parse failed in State: " + currentState.ToString()); }, __ParseState.INVALID);
                        break;
                    case __ParseState.SLIDE_DURATION_PARSED:
                        _ = TryParse(ConsumeStarFlag, (token) =>
                            {
                                noteList.Add(new Note(NoteType.SLIDE,
                                    slideTuples.First<NoteLocation>(),
                                    currentTimeStamp,
                                    currentTimeStamp + unitMeasure * slideDurations.Sum(),
                                    isAdjusted,
                                    isBreakNote,
                                    slideTuples
                                ));
                                noteLocation = slideTuples.Last<NoteLocation>();
                                slideTuples.Clear();
                                slideTuples.Add(noteLocation);
                                currentTimeStamp += unitMeasure * slideDurations.Sum();
                                slideDurations.Clear();
                            }, __ParseState.LOCATION_PARSED) ||
                            TryParse(ConsumeEOF, (token) =>
                            {
                                noteList.Add(new Note(NoteType.SLIDE,
                                    slideTuples.First<NoteLocation>(),
                                    currentTimeStamp,
                                    currentTimeStamp + unitMeasure * slideDurations.Sum(),
                                    isAdjusted,
                                    isBreakNote,
                                    slideTuples
                                ));
                            }, __ParseState.FINI) ||
                            TryParse(ConsumeSlideType, (token) =>
                            {
                                slideDirection = token switch
                                {
                                    "-" => SlideDirection.STRAIGHT,
                                    "<" => SlideDirection.CCW,
                                    ">" => SlideDirection.CW,
                                    "^" => SlideDirection.SHORTEST,
                                    _ => throw new ArgumentException("Invalid slide direction character"),
                                };
                            }, __ParseState.SLIDE_TYPE_PARSED) ||
                            TryParse((s) => (s, ""), (token) => { throw new ArgumentException("Parse failed in State: " + currentState.ToString()); }, __ParseState.INVALID);
                        break;
                    case __ParseState.FINI:
                        return noteList;
                    case __ParseState.INVALID:
                        throw new ArgumentException("Parse failed in State: " + currentState.ToString());
                }
            }

            throw new ArgumentException("Parse did not reach final state.");
        }
    }
    public class MusicData
    {
        public enum Difficulty { BASIC, ADVANCED, EXPERT, MASTER, ReMASTER };
        public MusicData(
            LevelData[] levelInfo,
            string title = "",
            string artist = "",
            double bpm = 0.0f
        )
        {
            LevelInfo = levelInfo ?? new LevelData[] { };
            Title = title;
            Artist = artist;
            BPM = bpm;
        }
        public string Title { get; set; }
        public string Artist { get; set; }
        public double BPM { get; set; }

        public LevelData[] LevelInfo { get; set; }

    }

    public class SimaiParser
    {
        public MusicData MusicData;
        private string fileText;

        public SimaiParser(string filePath)
        {
            fileText = File.ReadAllText(filePath, Encoding.UTF8);

            MusicData = new MusicData(
                title: GetFieldString("&title") ?? "Unknown Title",
                artist: GetFieldString("&artist") ?? "Unknown Artist",
                bpm: double.Parse(GetFieldString("&wholebpm") ?? "0"),
                levelInfo: new LevelData[5]
            );
            foreach (MusicData.Difficulty difficulty in Enum.GetValues(typeof(MusicData.Difficulty)))
            {
                int diffi = (int)difficulty;
                string? levelStr = GetFieldString("&lv_" + (diffi + 1).ToString());
                string? rawNoteStr = GetFieldString("&inote_" + (diffi + 1).ToString());
                if (levelStr == null || rawNoteStr == null)
                {
                    continue;
                }
                MusicData.LevelInfo[diffi] = new LevelData(
                    levelString: levelStr,
                    rawNoteString: rawNoteStr
                );
                MusicData.LevelInfo[diffi].ParseNotes();
            }
        }

        public void PrintMusicData()
        {
            System.Console.WriteLine($"Title: {MusicData.Title}");
            System.Console.WriteLine($"Artist: {MusicData.Artist}");
            System.Console.WriteLine($"BPM: {MusicData.BPM}");
            for (int i = 0; i < MusicData.LevelInfo.Length; i++)
            {
                System.Console.WriteLine($"Difficulty {(MusicData.Difficulty)i}: Level {MusicData.LevelInfo[i]?.LevelString ?? "N/A"} - Notes Count: {MusicData.LevelInfo[i]?.NoteString.Count ?? -1}");
            }
        }
        private string? GetFieldString(string fieldName)
        {
            int startIndex = fileText.IndexOf(fieldName);
            if (startIndex == -1)
            {
                return null;
            }
            startIndex += fieldName.Length + 1;
            int endIndex = fileText.IndexOf("&", startIndex);
            if (endIndex == -1)
            {
                endIndex = fileText.Length;
            }
            string res = fileText.Substring(startIndex, endIndex - startIndex).Trim();
            return res;
        }
    }
}