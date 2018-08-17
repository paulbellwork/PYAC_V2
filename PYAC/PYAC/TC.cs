using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYAC
{
    public class TC
    {
        private string Number;

        public TC(string number)
        {
            Number = number;
        }

        public bool IsTCValid()
        {
            if(int.TryParse(Number, out int n))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

   

