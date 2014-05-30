using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TesteConsole
{
   public class Philo
    {
        int n;
        int thinkDelay;
        int eatDelay;
        int left, right;
        philofork philofork;

        public Philo(int n, int thinkDelay, int eatDelay, philofork philofork)
        {
            this.n = n;//Id
            this.thinkDelay = thinkDelay; //tempo de pensar
            this.eatDelay = eatDelay; //tempo de comer
            this.philofork = philofork;
            left = n == 0 ? 4 : n - 1;//mao esqueda condição para ver se o primeiro filosofo 
            right = (n + 1) % 5;//Mao direita
            new Thread(new ThreadStart(Run)).Start();//inicia a thread
        }

        public void Run()
        {

            for (; ; )
            {

                try
                {
                    Thread.Sleep(thinkDelay);//espera de pensar
                    philofork.Get(left, right);//pega o garfo
                    Console.WriteLine("Philosopher " + n + " is eating...");//impirme quem está comendo
                    Thread.Sleep(eatDelay);//espera o tempo de comer
                    philofork.Put(left, right);//coloca os garfos na mesa
                }

                catch
                {
                    return;//para não para o fluxo de execução
                }
            }
        }
    }
}
