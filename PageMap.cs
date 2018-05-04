using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{

    /*
     * Class contains the mapping information for a specific page
     * in a processes page table
     */
    public class PageMap
    {
        public int mapNum;
        public Boolean inMemory;

        /*
         * Constructor to set intance variables
         * @param n the frame to which a page maps to
         * @param m a boolean whether the frame is in memory or not
         */
        public PageMap(int n, Boolean m)
        {
            this.mapNum = n;
            this.inMemory = m;
        }
    }
}
