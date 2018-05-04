using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Program simulates the functionaity of an Operating System
 * in regards to managing memory for user processes
 * @Author Andrew Olesak
 */
namespace Project3
{
    /*
     * Class interacts with Windows Forms
     */
    public partial class Form1 : Form
    {
        TextBox[] pMemory;
        TextBox[] diskMemory;
        TextBox[] type;
        TextBox[] page;
        TextBox[] frame;
        TextBox[] disk;
        Label[] label;
        String command;
        Button next;
        Button previous;
        Button enter;
        OperatingSystem os;
        int stateCounter;
        const int MAXPROCESSES = 6;

        /*
         * Class manages all action handlers and delegates user
         * commands appropriately
         */
        public Form1()
        {
            InitializeComponent();
            pMemory = new TextBox[] { F0, F1, F2, F3, F4, F5, F6, F7 };
            diskMemory = new TextBox[] { D0, D1, D2, D3, D4, D5, D6, D7 };
            type = new TextBox[] { T0, T1, T2, T3, T4, T5 };
            page = new TextBox[] { P0, P1, P2, P3, P4, P5 };
            frame = new TextBox[] { FR0, FR1, FR2, FR3, FR4, FR5 };
            disk = new TextBox[] { DK0, DK1, DK2, DK3, DK4, DK5 };
            label = new Label[] { L0, L1, L2, L3, L4, L5 };
            command = "";
            next = new Button();
            previous = new Button();
            enter = new Button();
            os = new OperatingSystem();
            this.stateCounter = 0;
        }
        

        /*
         * Function takes commands from the user to add, delete, and manipulate processes
         */
        private void button1_Click(object sender, EventArgs e)
        {
            // check to make sure that the simulator is at its current state and not a previous one
            if (this.stateCounter != this.os.mstates.Count-1)
            {
                countLabel.Text = "Please hit Next until the current state is reached";
                return;
            }
            try
            {
                this.command = cmd.Text;
                // check if a process is to be removed
                if (this.command.Length>=6 && this.command.Substring(2, 4).Equals("Halt"))
                {
                    // remove the process
                    int id = Int32.Parse(this.command.Substring(0, 1));
                    // check if the process exists
                    if (!this.os.ProcessExists(id))
                    {
                        countLabel.Text = "Sorry, that process ID doesn't exit";
                        return;
                    }
                    this.os.removeProcess(id);
                    countLabel.Text = "";
                }
                // check if a page is to be moved from disk space to main memory
                else if(this.command.Length>=10 && this.command.Substring(2, 3).Equals("use")){
                    // parse the command
                    int id = Int32.Parse(this.command.Substring(0, 1));
                    int page = Int32.Parse(this.command.Substring(11, 1));
                    // check if the process exists
                    if (!this.os.ProcessExists(id))
                    {
                        countLabel.Text = "Sorry, that process ID doesn't exit";
                        return;
                    }
                    // load the page from the correct tpye
                    if(this.command.Substring(6, 4).Equals("text"))
                    {
                        this.os.LoadPageFromDisk(id, true, page);
                    }else if(this.command.Substring(6, 4).Equals("data"))
                    {
                        // this.os.LoadPage(id, "Data", page);
                        this.os.LoadPageFromDisk(id, false, page);
                    }
                    countLabel.Text = "";
                }
                else
                {
                    // add process
                    //check to see if there is room for another process
                    if(this.os.processes.Count >= MAXPROCESSES)
                    {
                        countLabel.Text = "Sorry, you have reached the maximum number of processes";
                        this.cmd.Text = "";
                        return;
                    }
                    string[] tokens = this.command.Split(' ');
                    int id = Int32.Parse(tokens[0]);
                    int tsize = Int32.Parse(tokens[1]);
                    int dsize = Int32.Parse(tokens[2]);
                    // check if the processes already exists
                    if (this.os.ProcessExists(id))
                    {
                        countLabel.Text = "Sorry, that process ID already exists";
                        return;
                    }
                    Process p = new Process(id, tsize, dsize);
                    // if there is enough room in memory or on disk, then map the processes
                    if ((p.textPageTable.Length+p.dataPageTable.Length) <= this.os.freeFrames.Count)
                    {
                        countLabel.Text = "";
                        this.os.Map(p);
                        this.os.processes.Add(p);
                    }
                    else
                    {
                        countLabel.Text = "Sorry, process is too big";
                        return;
                    }

                }
                // increment the state counter and copy the current states of memory, disk space, and processes
                this.stateCounter++;
                this.os.StoreState();
                this.os.CopyList(this.os.processes);
                // update the GUI for memory, disk space, and processes
                upDateGUI(this.os.frames, this.pMemory);
                upDateGUI(this.os.diskPages, this.diskMemory);
                UpdatePageTables(this.os.processes.ToArray());
                this.cmd.Text = "";


            }catch
            {
                countLabel.Text = "Sorry, wrong command";
            }
        }

        /*
         * updates lists of text boxes for memory and disk space
         * @param memory, array of Frame objects to use as data
         * @param display, array of TextBoxes to put corresponding data in
         */
        public void upDateGUI(Frame[] memory, TextBox[] display)
        {
            for(int i=0; i<display.Length; i++)
            {
                if (memory[i] != null)
                {
                    String pageInfo;
                    Frame f = memory[i];
                    pageInfo = "P"+f.pid.ToString() +" "+f.type+" "+f.pageNum.ToString();
                    display[i].Text = pageInfo;
                }
                else
                {
                    display[i].Text = "";
                }
            }
        }

        /*
         * updates the tables containing processes
         * @param processes, array of current processes in the system
         */
        public void UpdatePageTables(Process[] processes)
        {
            // loop through every set of process tables
            for(int i=0; i<type.Length; i++)
             {
                // clear all tables to update
                type[i].Clear();
                page[i].Clear();
                frame[i].Clear();
                disk[i].Clear();
                label[i].Text = "";
                // continue to fill in page tables as long as there
                // are processes that exist
                if (i<processes.Length)
                {
                    // fill in the text portion of the page table
                    label[i].Text = "P" + processes[i].pid.ToString();
                    for (int j = 0; j < processes[i].textPageTable.Length; j++)
                    {
                        if (j == 0)
                        {
                            type[i].Text += "Text" + Environment.NewLine;
                        }
                        else
                        {
                            type[i].Text += Environment.NewLine;
                        }
                        if (processes[i].textPageTable[j].inMemory)
                        {
                            page[i].Text += j.ToString() + Environment.NewLine; ;
                            frame[i].Text += processes[i].textPageTable[j].mapNum.ToString() + Environment.NewLine;
                            disk[i].Text += Environment.NewLine;
                        }
                        else
                        {
                            page[i].Text += j.ToString() + Environment.NewLine;
                            frame[i].Text += Environment.NewLine;
                            disk[i].Text += processes[i].textPageTable[j].mapNum.ToString() + Environment.NewLine;
                        }

                    }
                    // fill in the remaining data portion of the page table
                    for (int j = 0; j < processes[i].dataPageTable.Length; j++)
                    {
                        if (j == 0)
                        {
                            type[i].Text += "Data" + Environment.NewLine;
                        }
                        else
                        {
                            type[i].Text += Environment.NewLine;
                        }
                        if (processes[i].dataPageTable[j].inMemory)
                        {
                            page[i].Text += j.ToString() + Environment.NewLine; ;
                            frame[i].Text += processes[i].dataPageTable[j].mapNum.ToString() + Environment.NewLine;
                            disk[i].Text += Environment.NewLine;
                        }
                        else
                        {
                            page[i].Text += j.ToString() + Environment.NewLine;
                            frame[i].Text += Environment.NewLine;
                            disk[i].Text += processes[i].dataPageTable[j].mapNum.ToString() + Environment.NewLine;
                        }
                    }
                }
            }
        }


        /*
         * changes the state of the simulator to the previous version
         */
        private void previousButton_Click(object sender, EventArgs e)
        {
            // check that the counter doesn't go out of bounds
            if (this.stateCounter - 1 >= 0)
            {
                // update the label and GUIs and decrement the counter
                WarnLabel.Text = "";
                this.stateCounter--;
                upDateGUI(this.os.mstates[this.stateCounter], this.pMemory);
                upDateGUI(this.os.dstates[this.stateCounter], this.diskMemory);
                UpdatePageTables(this.os.pstates[this.stateCounter]);
                WarnLabel.Text = "";
            }
            else
            {
                WarnLabel.Text = "Sorry, too far backward";
            }
        }

        /*
         * Changes the state of the simulator to the next version
         */
        private void nextButton_Click(object sender, EventArgs e)
        {
            // check that the counter doesn't go out of bounds
            if (this.stateCounter + 1 < this.os.mstates.Count)
            {
                // update the label and GUIs and increment the counter
                WarnLabel.Text = "";
                this.stateCounter++;
                upDateGUI(this.os.mstates[this.stateCounter], this.pMemory);
                upDateGUI(this.os.dstates[this.stateCounter], this.diskMemory);
                UpdatePageTables(this.os.pstates[this.stateCounter]);
                //if the current state is reached remove the warning label
                if(this.stateCounter == this.os.mstates.Count - 1)
                {
                    countLabel.Text = "";
                    WarnLabel.Text = "Current State";
                }
            }
            else
            {
                WarnLabel.Text = "Sorry, too far forward";
            }
        }
        
    }
}