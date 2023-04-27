using Pixela.Core;

namespace Pixela.Test
{
    internal class Program
    {
        static void Main()
        {
            Image image = new(@"C:\Users\Diego\Pictures\test.png");
            Filter filter = new(new float[] {1,1,1,1,1,1,1,1,1 },true, 1f/9f);
            image.ApplyFilter(filter,true);
            image.Save("result.png");
        }
    }
}