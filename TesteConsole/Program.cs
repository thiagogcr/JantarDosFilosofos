using System;
using System.Threading;

namespace TesteConsole
{
    public class Program
    {
        public static void Main()
        {
            philofork philofork = new philofork();//cria objeto 
            new Philo(0, 10, 1000, philofork);//Cria uma thread do filosofo
            new Philo(1, 20, 1000, philofork);//Cria uma thread do filosofo
            new Philo(2, 30, 1000, philofork);//Cria uma thread do filosofo
            new Philo(3, 40, 1000, philofork);//Cria uma thread do filosofo
            new Philo(4, 50, 1000, philofork);//Cria uma thread do filosofo
        }
    }
}