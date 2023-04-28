using Pixela.Core;
using System.Diagnostics;

namespace Pixela.Test
{
    internal class Program
    {
        static void Main()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Inicializar imagen
            Image image = new(@"C:\Users\Diego\Pictures\test.png");
            //Inicializar filtro
            Filter filter = new(new float[] 
                {4,4,4,
                0,-24,0,
                4,4,4 }
                ,false);
            //Aplicar filtro
            image.ApplyFilter(filter,true);
            //Guardar imagen
            image.Save("result.png");
            stopwatch.Stop();
            Console.WriteLine($"Time elapsed: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}