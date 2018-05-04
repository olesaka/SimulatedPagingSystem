using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    /*
     * Class simulates the functionality of an operating system's
     * memory allocation and maintenance.
     */
    public class OperatingSystem
    {
        const int MEMSIZE = 4096;
        const int FRAMESIZE = 512;
        public Frame[] frames;
        public Frame[] diskPages;
        public List<Frame[]> mstates;
        public List<Frame[]> dstates;
        public List<Process> processes;
        public List<Process[]> pstates;
        public List<int> freeFrames;
        public int frameCount;

        /*
         * Constructor to set instance variables
         */
        public OperatingSystem()
        {
            this.frames = new Frame[(int)MEMSIZE / FRAMESIZE];
            this.diskPages = new Frame[(int)MEMSIZE / FRAMESIZE];
            this.mstates = new List<Frame[]>();
            this.mstates.Add(new Frame[(int)MEMSIZE / FRAMESIZE]);
            this.dstates = new List<Frame[]>();
            this.dstates.Add(new Frame[(int)MEMSIZE / FRAMESIZE]);
            this.processes = new List<Process>();
            this.pstates = new List<Process[]>();
            this.pstates.Add(new Process[0]);
            this.freeFrames = new List<int>();
            SetFreeFrames();
            this.frameCount = 0;
        }

        /*
         * Private function to set all the frames to free initially
         */
        private void SetFreeFrames()
        {
            int total = this.frames.Length + this.diskPages.Length;
            for (int i = 0; i < total; i++)
            {
                this.freeFrames.Add(i);
            }
        }

        /*
         * Function uses the list of free frames to assign the given processes's
         * page tables into memory and disk space
         */
        public void Map(Process p)
        {
            // put all text pages into memory first
            for (int i = 0; i < p.textPageTable.Length; i++)
            {
                // if there aren't any free frames in memory, then move to disk
                if (this.freeFrames[0] < this.frames.Length)
                {
                    this.frames[this.freeFrames[0]] = new Frame(p.pid, "Text Page", i);
                    p.addPageToTextTable(i, this.freeFrames[0], true);
                }
                else
                {
                    this.diskPages[this.freeFrames[0] % this.frames.Length] = new Frame(p.pid, "Text Page", i);
                    p.addPageToTextTable(i, this.freeFrames[0] % this.frames.Length, false);
                }
                this.freeFrames.RemoveAt(0);
            }

            // put all data pages into memory 
            for (int i = 0; i < p.dataPageTable.Length; i++)
            {
                // if there aren't any free frames in memory, then move to disk
                if (this.freeFrames[0] < this.frames.Length)
                {
                    this.frames[this.freeFrames[0]] = new Frame(p.pid, "Data Page", i);
                    p.addPageToDataTable(i, this.freeFrames[0], true);
                }
                else
                {
                    this.diskPages[this.freeFrames[0] % this.frames.Length] = new Frame(p.pid, "Data Page", i);
                    p.addPageToDataTable(i, this.freeFrames[0] % this.frames.Length, false);
                }
                this.freeFrames.RemoveAt(0);
            }
        }


        /*
         * Removes a process from memory and from the list of processes
         * @param id the id of the process to remove
         */
        public void removeProcess(int id)
        {
            // remove all instances of the process from main memory
            for (int i = 0; i < this.frames.Length; i++)
            {
                if (this.frames[i] != null && this.frames[i].pid == id)
                {
                    this.frames[i] = null;
                    this.freeFrames.Add(i);
                }
            }
            // remove all instances of the process from virtual memory
            for (int i = 0; i < this.diskPages.Length; i++)
            {
                if (this.diskPages[i] != null && this.diskPages[i].pid == id)
                {
                    this.diskPages[i] = null;
                    this.freeFrames.Add(i + this.frames.Length);
                }
            }
            this.freeFrames.Sort();

            //remove the process from the list of processes
            for (int i = 0; i < this.processes.Count; i++)
            {
                if (this.processes[i].pid == id)
                {
                    this.processes.Remove(this.processes[i]);
                    break;
                }
            }
        }

        /*
         * Stores the states of both main memory and 
         * virtual memory by making a copy of each
         */
        public void StoreState()
        {
            this.mstates.Add((Frame[])this.frames.Clone());
            this.dstates.Add((Frame[])this.diskPages.Clone());
        }

        /*
         * Function makes a deep copy of all of the 
         * current processes and stores them 
         */
        public void CopyList(List<Process> p)
        {
            //create a process array to hold all copied processes
            Process[] pr = new Process[p.Count];
            for (int i = 0; i < pr.Length; i++)
            {
                // create a new process object with the same parameters
                pr[i] = new Process(p[i].pid, p[i].textSize, p[i].dataSize);
                // create new pageMap objects for each location in the text page table
                // and copy over instance variables
                for (int j = 0; j < p[i].textPageTable.Length; j++)
                {
                    pr[i].textPageTable[j] = new PageMap(p[i].textPageTable[j].mapNum, p[i].textPageTable[j].inMemory);
                }
                // create new pageMap objects for each locatoin in the data page table
                // and copy over instance variables
                for (int j = 0; j < p[i].dataPageTable.Length; j++)
                {
                    pr[i].dataPageTable[j] = new PageMap(p[i].dataPageTable[j].mapNum, p[i].dataPageTable[j].inMemory);
                }

            }
            // add the copied process array to the list of process states
            this.pstates.Add(pr);
        }


        /*
         * Loads a given page from virutal memory into main memory
         * @param id the id of the process that owns the page
         * @param isText boolean that is true if the page is a text page, false otherwise
         * @param pageNum the index of the given page in its respective list
         */
        public void LoadPageFromDisk(int id, Boolean isText, int pageNum)
        {
            //find the process with the given id
            foreach (Process p in this.processes)
            {
                if (p != null && p.pid == id)
                {
                    //look for a free frame in memory to put the process in

                    if (this.freeFrames.Count != 0 && this.freeFrames[0] < this.frames.Length)
                    {
                        // move the text page into memory and update the processes page table
                        if (isText)
                        {
                            Frame f = this.diskPages[p.textPageTable[pageNum].mapNum];
                            this.frames[this.freeFrames[0]] = f;
                            this.diskPages[p.textPageTable[pageNum].mapNum] = null;
                            this.freeFrames.Add(p.textPageTable[pageNum].mapNum + this.frames.Length);
                            p.textPageTable[pageNum].mapNum = this.freeFrames[0];
                            p.textPageTable[pageNum].inMemory = true;
                            this.freeFrames.RemoveAt(0);

                        }
                        // move the data page into memory and update the processes page table
                        else
                        {
                            Frame f = this.diskPages[p.dataPageTable[pageNum].mapNum];
                            this.frames[this.freeFrames[0]] = f;
                            this.diskPages[p.dataPageTable[pageNum].mapNum] = null;
                            this.freeFrames.Add(p.dataPageTable[pageNum].mapNum + this.frames.Length);
                            p.dataPageTable[pageNum].mapNum = this.freeFrames[0];
                            p.dataPageTable[pageNum].inMemory = true;
                            this.freeFrames.RemoveAt(0);
                        }
                        this.freeFrames.Sort();
                        return;
                    }

                    // no free frames were found in memory, so remove a page
                    // based on the current frame counter
                    Frame mf = this.frames[this.frameCount];
                    int diskIndex;
                    // swap the text page in virtual memory with the page in main memory
                    // and update the page tables of the known process
                    if (isText)
                    {
                        diskIndex = p.textPageTable[pageNum].mapNum;
                        this.frames[this.frameCount] = this.diskPages[diskIndex];
                        this.diskPages[diskIndex] = mf;
                        p.textPageTable[pageNum].mapNum = this.frameCount;
                        p.textPageTable[pageNum].inMemory = true;
                    }
                    // swap the data page in irtual memory with the page in main memory
                    // and update the page tables of the known process
                    else
                    {
                        diskIndex = p.dataPageTable[pageNum].mapNum;
                        this.frames[this.frameCount] = this.diskPages[diskIndex];
                        this.diskPages[diskIndex] = mf;
                        p.dataPageTable[pageNum].mapNum = this.frameCount;
                        p.dataPageTable[pageNum].inMemory = true;
                    }
                    // find the process of the swapped frame and update its page table accordingly
                    mf = this.diskPages[diskIndex];
                    if (mf.type.Equals("Text Page"))
                    {
                        SetPageToFalse(mf.pid, true, mf.pageNum, diskIndex);
                    }
                    else
                    {
                        SetPageToFalse(mf.pid, false, mf.pageNum, diskIndex);
                    }
                    // increment the frame counter and reset it once
                    // it exceeds the size of memory
                    this.frameCount++;
                    if (this.frameCount > 7)
                    {
                        this.frameCount = 0;
                    }
                    return;
                }
            }
        }

        /*
         * sets the given page's boolean value to false because
         * it is no longer in main memory
         * @param id the id of the process that owns the page
         * @param type true if text table, false if data table
         * @param page the index of the given table
         * @param map the new mapping location of the specific page
         */
        public void SetPageToFalse(int id, Boolean type, int page, int map)
        {
            // find the process that owns the id
            foreach (Process p in this.processes)
            {
                if (p != null && p.pid == id)
                {
                    // set the text page to false and update its mapping
                    if (type)
                    {
                        p.textPageTable[page].mapNum = map;
                        p.textPageTable[page].inMemory = false;
                    }
                    // set the data page to false and update its mapping
                    else
                    {
                        p.dataPageTable[page].mapNum = map;
                        p.dataPageTable[page].inMemory = false;
                    }
                }
            }
        }

        /*
         * Function returns true if the given process id exists already
         * otherwise false
         */
        public Boolean ProcessExists(int id)
        {
            foreach (Process p in this.processes)
            {
                if (p.pid == id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
