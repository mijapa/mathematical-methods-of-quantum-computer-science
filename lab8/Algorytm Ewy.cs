using Quantum;
using Quantum.Operations;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace QuantumConsole
{
	public class QuantumTest
	{	
		public static Tuple<int, int> FractionalApproximation(int a, int b, int width)
        {
            double f = (double)a / (double)b;
            double g = f;
            int i, num2 = 0, den2 = 1, num1 = 1, den1 = 0, num = 0, den = 0;
            int max = 1 << width;

            do
            {
                i = (int)g;  // integer part
                g = 1.0 / (g - i);  // reciprocal of the fractional part

                if (i * den1 + den2 > max) // if denominator is too big
                {
                    break;
                }

                // new numerator and denominator
                num = i * num1 + num2;
                den = i * den1 + den2;

                // previous nominators and denominators are memorized
                num2 = num1;
                den2 = den1;
                num1 = num;
                den1 = den;

            }
            while (Math.Abs(((double)num / (double)den) - f) > 1.0 / (2 * max));
            // this condition is from Shor algorithm

            return new Tuple<int, int>(num, den);
        }
        
        public static int FindPeriod(int N, int a) {
			ulong ulongN = (ulong)N;
			int width = (int)Math.Ceiling(Math.Log(N, 2));
 
			Console.WriteLine("Width for N: {0}", width);
			Console.WriteLine("Total register width (7 * w + 2) : {0}", 7 * width + 2);
			
			QuantumComputer comp = QuantumComputer.GetInstance();
			
			//input register
			Register regX = comp.NewRegister(0, 2 * width);
			
			// output register (must contain 1):
			Register regX1 = comp.NewRegister(1, width + 1);
			
			// perform Walsh-Hadamard transform on the input register
			// input register can contains N^2 so it is 2*width long
			Console.WriteLine("Applying Walsh-Hadamard transform on the input register...");
			comp.Walsh(regX);
			
			// perform exp_mod_N
			Console.WriteLine("Applying f(x) = a^x mod N ...");
			comp.ExpModulo(regX, regX1, a, N);
			
			// output register is no longer needed
			regX1.Measure();
			
			// perform Quantum Fourier Transform on the input register
			Console.WriteLine("Applying QFT on the input register...");
			comp.QFT(regX);
			
			comp.Reverse(regX);
			
			// getting the input register
			int Q = (int)(1 << 2 * width);
			int inputMeasured = (int)regX.Measure();
			Console.WriteLine("Input measured = {0}", inputMeasured);
			Console.WriteLine("Q = {0}", Q);
			
			Tuple<int, int> result = FractionalApproximation(inputMeasured, Q, 2 * width - 1);
 
			Console.WriteLine("Fractional approximation:  {0} / {1}", result.Item1, result.Item2);
			
			int period = result.Item2;
			
			if(BigInteger.ModPow(a, period, N) == 1) {
				Console.WriteLine("Success !!!    period = {0}", period);
				return period;
			}
			
			int maxMult = (int)(Math.Sqrt(N)) + 1;
			int mult = 2;
			while(mult < maxMult) 
			{
				Console.WriteLine("Trying multiply by {0} ...", mult);
				period = result.Item2 * mult;
				if(BigInteger.ModPow(a, period, N) == 1) 
				{
					Console.WriteLine("Success !!!    period = {0}", period);
					return period;
				}
				else 
				{		
					mult++;
				}
			}
			
			Console.WriteLine("Failure !!!    Period not found, try again.");
			return -1;
		}
		
		public static int FindModularMultiplicativeInverse(int a, int b) {
		
			//korzystamy z pary rownan
			//au + bv = w
			//ax + by = z
			//oraz warunku
			//NWD (a,b) = NWD (w,z)
			
			//ustalamy wartości początkowe współczynników
			int u = 1, w = a; //v=0
			int x = 0, z = b; //y=1
			int q; 

			//w pętli modyfikujemy współczynniki równań
			while(w != 0) {
				if(w < z) 
				{
				//zamieniamy ze soba wspolczynniki rownan
					q = u; u = x; x = q;
					q = w; w = z; z = q;
				}
				//obliczamy iloraz całkowity
				q = w / z;
				//odejmujemy rownania wymnozone przez q
				u -= q*x;
				w -= q*z;
			}
			if(z == 1) {
				if(x < 0){
				//ujemne x sprowadzamy do wartosci dodatnich
					x += b;
				}
				Console.WriteLine("Odwrtonoscia liczby {0} modulo {1} jest {2}", a, b, x);
			}
			else {
			//dla z roznego od 1 nie istniej odwrotnosc modulo
				Console.WriteLine("BLAD: Nie znaleziono odwrotnosci modulo.");
			}
			return x;
		}
		
		public static void Main()
		{
		//ALGORYTM EWY

			int N = 55; //N = pq
			int c = 17; //klucz publiczny
			int a = 9; //wiadomosc Alice
		
			Console.WriteLine("a = {0}", a);
			Console.WriteLine("N = {0}", N);
			Console.WriteLine("c = {0}", c);
			int b = (int) BigInteger.ModPow(a, c, N);	//zaszyfrowana wiadomosc Alice
			Console.WriteLine("b = {0}", b);
			Console.WriteLine();
			
			//Znajdujemy rzad zakodowanej wiadomoci b w GN
			int r = FindPeriod(N, b);
			
			Console.WriteLine();
			Console.WriteLine("period = {0}", r);
			
			//znajdujemy klucz prywatny d` taki, ze cd` = 1 mod r
			int d = FindModularMultiplicativeInverse(c, r);	
			Console.WriteLine("d` = {0}", d);
			
			//odkodowujemy a = b^d` mod N
			int da = (int) BigInteger.ModPow(b,d, N);

			Console.WriteLine("Wiadomosc: {0}.", da);
		}
	}
}
