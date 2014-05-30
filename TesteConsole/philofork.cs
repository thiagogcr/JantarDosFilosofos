using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TesteConsole
{
    public class philofork
    {
        bool[] fork = new bool[5];

        public void Get(int left, int right)
        {
            lock (this)
            {
                while (fork[left] || fork[right])
                    Monitor.Wait(this);//enquanto nã libera ele espera
                fork[left] = true; //usa as mãos
                fork[right] = true;//usa as mãos
            }
        }

        public void Put(int left, int right)
        {
            lock (this)
            {
                fork[left] = false; //libera as mãos
                fork[right] = false;//libera as mãos
                Monitor.PulseAll(this);
            }
        }
    }
}
