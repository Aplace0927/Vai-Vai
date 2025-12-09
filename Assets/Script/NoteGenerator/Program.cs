using System;
using SimaiParser;
using Notes;
public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: SimaiParser <path_to_simai_file>");
            return;
        }

        string filePath = args[0];
        SimaiParser.SimaiParser parser = new SimaiParser.SimaiParser(filePath);
        parser.PrintMusicData();
    }
}