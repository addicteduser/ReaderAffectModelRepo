﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Preprocessing
{
    /// <summary>
    /// This class outputs windowed csv's with a 50% overlap
    /// </summary>
    class doWindow
    {
        //StreamReader eeg;
        //StreamWriter write;
        String filename;
        String destinationPath;
        List<Interval> list = new List<Interval>();
        int skipTime;
        int windowInterval;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Full name of path to be windowed</param>
        /// <param name="directory">Output directory</param>
        /// <param name="skip">Skip first n seconds</param>
        /// <param name="interval">Length of each window</param>
        public doWindow(String source, String directory, int skip, int interval)
        {
            filename = source;
            destinationPath = directory;
            skipTime = skip;
            windowInterval = interval;
            ThreadStart tsEven = new ThreadStart(doEven);
            Thread tEven = new Thread(tsEven);
            tEven.Start();
            if(!tEven.IsAlive)
                doOdd();
        }

        /*
        public void doEven()
        {
            StreamReader eeg = new StreamReader(filename);
            StreamWriter write = new StreamWriter("temp");
            int count = 0;
            //remove column
            if (!eeg.EndOfStream)
            {
                eeg.ReadLine();
            }

            DateTime current;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            bool isFirst = true;
            bool equalEndTime = false;
            bool beforeEndTime = false;
            bool afterEndTime = false;
            bool equalStartTime = false;
            bool afterStartTime = false;
            bool beforeStartTime = false;

            //traverse succeeding rows
            while (!eeg.EndOfStream)
            {
                String line = eeg.ReadLine();
                String[] values = line.Split(',');
                current = TimeDateMethods.toDateTime(values[1]);
                if (isFirst)
                {
                    startTime = current;
                    endTime = current.AddSeconds(skipTime);
                    write = new StreamWriter(destinationPath + (2*count) + ".csv");
                    isFirst = false;
                    list.Add(new Interval(startTime, endTime));
                    beforeEndTime = current.CompareTo(endTime) == -1;
                    equalEndTime = current.CompareTo(endTime) == 0; 
                    afterEndTime = current.CompareTo(endTime) == 1;
                    beforeStartTime = current.CompareTo(startTime) == -1;
                    equalStartTime = current.CompareTo(startTime) == 0;
                    afterStartTime = current.CompareTo(startTime) == 1;
                }
                else if ((equalEndTime || beforeEndTime) && (equalStartTime || afterStartTime))
                {
                    write.WriteLine(line);   
                }
                else if (afterEndTime)
                {
                    write.Close();
                    count++;
                    isFirst = true;
                }
                

            }
            eeg.Close();
        }
        */
        private void doEven()
        {
            DateTime current = new DateTime();
            DateTime endTime = new DateTime();
            DateTime startTime = new DateTime();
            StreamReader eeg = new StreamReader(filename);
            StreamWriter write;
            bool writeToFile = false;
            int count = 0;

            //skip column headers
            eeg.ReadLine();
            while (!eeg.EndOfStream)
            {
                
                    int name = (count * windowInterval);

                    write = new StreamWriter(destinationPath+name + ".csv"); //Console.WriteLine("create file {0}", count);
                    //write.WriteLine(columnHeaders);     //write column headers
                    String line = eeg.ReadLine();
                    String[] temp = line.Split(',');
                    current = TimeDateMethods.toDateTime(line);
                    startTime = TimeDateMethods.toDateTime(line);
                    endTime = startTime.AddSeconds(windowInterval/2);
                    writeToFile = true;
                    count++;
                    list.Add(new Interval(startTime, endTime));
                

                while (!eeg.EndOfStream && writeToFile) //append created file
                {
                    line = eeg.ReadLine();
                    temp = line.Split(',');
                    current = TimeDateMethods.toDateTime(line);

                    bool equalEndTime = current.CompareTo(endTime) == 0;
                    bool beforeEndTime = current.CompareTo(endTime) == -1;
                    bool equalStartTime = current.CompareTo(startTime) == 0;
                    bool afterStartTime = current.CompareTo(startTime) == 1;

                    if ((equalEndTime || beforeEndTime) && (equalStartTime || afterStartTime))
                    {
                        write.WriteLine(line);
                    }
                    else
                    {
                        writeToFile = false;
                    }
                }
                write.Close(); Console.WriteLine("CLOSED!");
            }
            eeg.Close();

            doOdd();
            Console.WriteLine("Finished reading file");
        }

        private void doOdd()
        {
            DirectoryInfo di = new DirectoryInfo(destinationPath);
            IEnumerable<FileInfo> files = di.EnumerateFiles("*.csv");
            FileInfo[] fileList = files.ToArray();
            StreamReader read = new StreamReader(filename);

            //skip column header
            if (!read.EndOfStream)
                read.ReadLine();

            bool equalEndTime = false;
            bool beforeEndTime = false;
            bool afterEndTime = false;
            bool equalStartTime = false;
            bool afterStartTime = false;
            bool beforeStartTime = false;

            for (int i = 0; i < list.Count; i++)
            {
                //StreamReader read = new StreamReader(fileList[i].FullName);
                Console.WriteLine("READING : " + list[i].StartTime);
                StreamWriter write = new StreamWriter(destinationPath+ (i * windowInterval + 1) + ".csv");
                DateTime current;
                DateTime startTime = list[i].StartTime.AddSeconds(windowInterval/2);
                DateTime endTime = list[i].EndTime.AddSeconds(windowInterval/2);
                bool end = false;
                while (!read.EndOfStream && !end)
                {
                    String line = read.ReadLine(); //Console.WriteLine(line);
                    String[] temp = line.Split(',');
                    current = TimeDateMethods.toDateTime(line);
                    
                    beforeEndTime = current.CompareTo(endTime) == -1;
                    equalEndTime = current.CompareTo(endTime) == 0;
                    afterEndTime = current.CompareTo(endTime) == 1;
                    beforeStartTime = current.CompareTo(startTime) == -1;
                    equalStartTime = current.CompareTo(startTime) == 0;
                    afterStartTime = current.CompareTo(startTime) == 1;

                    if ((equalEndTime || beforeEndTime) && (equalStartTime || afterStartTime))
                    {
                        write.WriteLine(line);
                    }
                    else if(afterEndTime)
                    {
                        end = true;
                    }
                }
            }
            read.Close();
        }

        private void doSkip(StreamReader stream, int remove)
        {
            int changes = 0;
            String previous = "";

            while (changes < remove && !stream.EndOfStream)
            {
                String line = stream.ReadLine();
                String current = line.Split(',')[1];

                if (!current.Equals(previous))
                {
                    previous = current;
                    changes++;
                }
            }
        }
    }

        class Interval
    {
        private DateTime start;
        private DateTime end;

        public DateTime StartTime { get { return start; } set { start = value; } }
        public DateTime EndTime { get { return end; } set { end = value; } }
        public Interval(DateTime s, DateTime e)
        {
            start = s;
            end = e;
        }
    }
}
