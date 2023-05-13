using Pixela.Core;
using System.Diagnostics;

namespace Pixela.Test
{
    internal class Program
    {
        static void Main()
        {
            Filter filter = new(new float[]
                {1,0,-1,1,0,-1,1,0,-1}
                ,false);
            string inputDirectory = @"C:\Users\Diego\Pictures\Pixela_samples";
            string outputDirectory = @"C:\Users\Diego\Pictures\Pixela_output";
            Directory.CreateDirectory(outputDirectory);
            string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            string[] imageFiles = Directory.GetFiles(inputDirectory)
                .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                .ToArray();
            Stopwatch stopwatch = new Stopwatch();
            foreach (string imageFile in imageFiles)
            {
                stopwatch.Restart();
                        Image image = new(imageFile,true);
                        image.ApplyFilter(filter, true);
                string outputFilename = Path.GetFileNameWithoutExtension(imageFile) + "_filtered.png";
                string outputPath = Path.Combine(outputDirectory, outputFilename);
                        image.Save(outputPath);
                stopwatch.Stop();
                Console.WriteLine($"Filtered {image.Height * image.Width} pixels in {stopwatch.ElapsedMilliseconds}ms. {image.Height * image.Width / stopwatch.ElapsedMilliseconds} pixels per ms.");
            }
        }
    }
}