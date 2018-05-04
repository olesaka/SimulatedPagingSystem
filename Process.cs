using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    /*
    * Class represents a process in memory
    */
    public class Process
    {
        public int pid;
        public int textSize;
        public int dataSize;
        public PageMap[] textPageTable;
        public PageMap[] dataPageTable;
        const int PAGESIZE = 512;

        /*
         * Constructor to set instance variables
         * @param id the process ID
         * @param ts the size of the text segment
         * @param ds the size of the data segment
         */
        public Process(int id, int ts, int ds)
        {
            this.pid = id;
            this.textSize = ts;
            this.dataSize = ds;
            setPageTables();
        }

        /*
         * Sets the length of the text and data segment page table arrays
         */
        private void setPageTables()
        {
            int text = (int)this.textSize / PAGESIZE;
            int data = (int)this.dataSize / PAGESIZE;
            if (this.textSize % PAGESIZE != 0)
            {
                text++;
            }
            if (this.dataSize % PAGESIZE != 0)
            {
                data++;
            }
            this.textPageTable = new PageMap[text];
            this.dataPageTable = new PageMap[data];
        }

        /*
         * creates a new page with the given parameters and adds
         * it at the appropriate index to the text page table
         */
        public void addPageToTextTable(int index, int pNum, Boolean mem)
        {
            this.textPageTable[index] = new PageMap(pNum, mem);
        }

        /*
         * creates a new page with the given parameters and adds
         * it at the appropriate index to the data page table
         */
        public void addPageToDataTable(int index, int pNum, Boolean mem)
        {
            this.dataPageTable[index] = new PageMap(pNum, mem);
        }
    }
}
