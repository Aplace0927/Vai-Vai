using System.IO;
using System.Text;

namespace SimaiParser
{
    public enum NoteLocation
    {
        A1, A2, A3, A4, A5, A6, A7, A8, // Outer Circle + Button
        B1, B2, B3, B4, B5, B6, B7, B8, // Inner Circle
        C                               // Center
    }
    public enum Difficulty { BASIC, ADVANCED, EXPERT, MASTER, ReMASTER };
    public class NoteEntity
    {
        public NoteLocation[] Locations { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public bool Adjusted { get; set; } = false;

    }
    public class LevelData
    {
        public string LevelString { get; set; }
        public string RawNoteString { get; set; }
        public NoteEntity[] NoteString { get; set; }
        public void ParseNotes()
        {
            float NoteTiming = 0.0f;
            float CurrentMeasureFrequency = 0.0f;

            float MusicBPM = GetBPM();
            while (RawNoteString.Length > 0)
            {
                switch (RawNoteString[0])
                {
                    case 'E': // End of Notes
                        return;
                    case '{': // Notes per measure
                        float NotesPerMeasure = GetNotesPerMeasure();
                        CurrentMeasureFrequency = 60.0f / MusicBPM * 4.0f / NotesPerMeasure;
                        break;
                    default:
                        string NoteSegment = GetNoteSegment();
                        System.Console.WriteLine(NoteTiming.ToString() + " " + NoteSegment);
                        NoteTiming += CurrentMeasureFrequency;
                        break;
                }
            }
        }
        private float GetNotesPerMeasure()
        {
            int startIndex = RawNoteString.IndexOf("{");
            int endIndex = RawNoteString.IndexOf("}", startIndex);

            if (startIndex == -1 || endIndex == -1)
            {
                throw new System.Exception("Notes per measure not found in note string.");
            }

            string NotesPerMeasureString = RawNoteString.Substring(startIndex + 1, endIndex - startIndex - 1);

            RawNoteString = RawNoteString.Remove(startIndex, endIndex - startIndex + 1);
            return float.Parse(NotesPerMeasureString);
        }
        private float GetBPM()
        {
            int startIndex = RawNoteString.IndexOf("(");
            int endIndex = RawNoteString.IndexOf(")", startIndex);

            if (startIndex == -1 || endIndex == -1)
            {
                throw new System.Exception("BPM not found in note string.");
            }

            string bpmString = RawNoteString.Substring(startIndex + 1, endIndex - startIndex - 1);

            RawNoteString = RawNoteString.Remove(startIndex, endIndex - startIndex + 1);
            return float.Parse(bpmString);
        }
        private string GetNoteSegment()
        {
            int NextComma = RawNoteString.IndexOf(",") == -1 ? RawNoteString.Length : RawNoteString.IndexOf(",");
            int NextBrace = RawNoteString.IndexOf("{") == -1 ? RawNoteString.Length : RawNoteString.IndexOf("{");

            string NoteSegment = RawNoteString.Substring(0, Math.Min(NextComma, NextBrace)).Trim();
            if (NextComma < NextBrace)
            {
                RawNoteString = RawNoteString.Remove(0, NextComma + 1);
            }
            else
            {
                RawNoteString = RawNoteString.Remove(0, NextBrace);
            }

            return NoteSegment;
        }
    }
    public class MusicData
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public float BPM { get; set; }

        public LevelData[] LevelInfo { get; set; }

    }

    public class SimaiParser
    {
        public MusicData MusicData = new MusicData();
        private string fileText;

        public SimaiParser(string filePath)
        {
            fileText = File.ReadAllText(filePath, Encoding.UTF8);

            MusicData.Title = GetFieldString("&title");
            MusicData.Artist = GetFieldString("&artist");
            MusicData.BPM = float.Parse(GetFieldString("&wholebpm"));
            MusicData.LevelInfo = new LevelData[5];

            foreach (Difficulty difficulty in (Difficulty[])System.Enum.GetValues(typeof(Difficulty)))
            {
                int diffi = (int)difficulty;
                MusicData.LevelInfo[diffi] = new LevelData();
                MusicData.LevelInfo[diffi].LevelString = GetFieldString("&lv_" + (diffi + 1).ToString());
                MusicData.LevelInfo[diffi].RawNoteString = GetFieldString("&inote_" + (diffi + 1).ToString());
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
                System.Console.WriteLine($"Difficulty {(Difficulty)i}: Level {MusicData.LevelInfo[i].LevelString}");
                System.Console.WriteLine($"Notes: {MusicData.LevelInfo[i].RawNoteString}"); // Should be empty after parsing
            }
        }
        private string GetFieldString(string fieldName)
        {
            int startIndex = fileText.IndexOf(fieldName) + fieldName.Length + 1;
            int endIndex = fileText.IndexOf("&", startIndex);
            if (endIndex == -1)
            {
                endIndex = fileText.Length;
            }
            return fileText.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}