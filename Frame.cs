using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    /*
     *  Class represents a frame within physical memory
     */
    public class Frame
    {
        public int pid;
        public string type;
        public int pageNum;

        /*
         * Constructor to set instance variables
         * @param p the process ID
         * @param t the data type, either text or data
         * @param pn the page number in the frame
         */
        public Frame(int p, string t, int pn)
        {
            this.pid = p;
            this.type = t;
            this.pageNum = pn;
        }
    }
}
