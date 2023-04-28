using Pixela.Core;
using System.Diagnostics;

namespace Pixela.Test
{
    internal class Program
    {
        static void Main()
        {
            Filter filter = new(new float[]
                {4,4,4,
                0,-24,0,
                4,4,4 }
                , false);
            string inputDirectory = @"C:\Users\Diego\Pictures\Testing";
            string outputDirectory = @"C:\Users\Diego\Pictures\Filtered";
            Directory.CreateDirectory(outputDirectory);
            string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            string[] imageFiles = Directory.GetFiles(inputDirectory)
                .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                .ToArray();
            foreach (string imageFile in imageFiles)
            {
                Image image = new(imageFile);
                image.ApplyFilter(filter, true);
                string outputFilename = Path.GetFileNameWithoutExtension(imageFile) + "_filtered.png";
                string outputPath = Path.Combine(outputDirectory, outputFilename);
                image.Save(outputPath);
            }
        }
    }
}