﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Samus.FileManipulation
{
    public class Extractions
    {
        public static bool FileIsReady(string sFileName)
        {
            FileStream stream = null;
            FileInfo file = new FileInfo(sFileName);
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return true;
        }
        public static string[] GetFlopCardNumbers(string[] lines)
        {
            File.AppendAllText(Program.DebugBotPath, "Getting Flop card numbers..." + System.Environment.NewLine);

            foreach (string line in lines) //try catch this
            {
                if (line.Contains("FLOP"))
                {
                    return Regex.Split(line, @"\D+"); //returns string array of numbers, which are the  flop cards.
                }    
            }
            throw new Exception();
        }

        public static string[] GetFileInfo(string path)
        {
            File.AppendAllText(Program.DebugBotPath, "Getting file info..." + System.Environment.NewLine);
            while (true)//check if file is ready to read.
            {
                if (FileIsReady(path))
                {
                    return File.ReadAllLines(path);
                }
            }
        }
    }
}
