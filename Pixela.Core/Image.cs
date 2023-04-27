using System.IO;

namespace Pixela.Core
{
    public enum ValueType
    {
        R,
        G,
        B,
        A,
        Grayscale
    }

    public struct Layer
    {
        public ValueType Type { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public byte[,] Pixels { get; set; }
        public Layer(ValueType type, int width, int height)
        {
            Type = type;
            Height = height;
            Width = width;
            Pixels = new byte[width, height];
        }
        public Layer(ValueType type, byte[,] pixelValues)
        {
            Type = type;
            Pixels = pixelValues;
            Height = pixelValues.GetLength(0);
            Width = pixelValues.GetLength(1);

        }
        public byte this[int x, int y]
        {
            get
            {
                return Pixels[x, y];
            }
            set
            {
                Pixels[x, y] = (byte)value;
            }
        }
    }

    public class Image
    {
        public byte LayerDepth { get; set; }
        public Layer[] Layers { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public Image(Layer[] layers)
        {
            Layers = layers;
            Height = layers[0].Height;
            Width = layers[0].Width;
            LayerDepth = (byte)layers.Length;
        }
        public Image(string filePath)
        {
            Layers = ReadLayersFromFile(filePath);
            Height = Layers[0].Height;
            Width = Layers[0].Width;
            LayerDepth = (byte)Layers.Length;
        }
        public byte this[int x, int y, int layer]
        {
            get
            {
                return Layers[layer][x, y];
            }
            set
            {
                Layers[layer][x, y] = (byte)value;
            }
        }
        public void ApplyFilter(Filter filter, bool applyTransform)
        {
            Layer[] newLayers = new Layer[LayerDepth];
            for (int i = 0; i < LayerDepth; i++)
            {
                newLayers[i] = ApplyFilterToLayer(i, filter, applyTransform);
            }
            Layers = newLayers;
        }
        private Layer ApplyFilterToLayer(int layerIndex, Filter filter, bool applyTransform)
        {
            float[,] output = new float[Width, Height];
            float maxValueFound = float.MinValue;
            float minValueFound = float.MaxValue;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    float result = ApplyFilterToPixel(i, j, layerIndex, filter);
                    maxValueFound = result > maxValueFound ? result : maxValueFound;
                    minValueFound = result < minValueFound ? result : minValueFound;
                    output[i, j] = result;
                }
            }

            if (applyTransform)
            {
                return new Layer(Layers[layerIndex].Type, ApplyLinearTransform(output, maxValueFound, minValueFound));
            }
            else
            {
                return new Layer(Layers[layerIndex].Type, TrunkValues(output));
            }
        }
        private float ApplyFilterToPixel(int x, int y, int layerIndex, Filter filter)
        {
            int displacement = (filter.MaskSize - 1) / 2;
            float result = 0;
            int filterPosX = 0;
            int filterPosY = 0;
            for (int i = x - displacement; i < x + displacement; i++)
            {
                for (int j = y - displacement; j < y + displacement; j++)
                {
                    result += this[i, j, layerIndex] * filter[filterPosX, filterPosY];
                    filterPosX++;
                }
                filterPosY++;
            }
            return result;
        }
        private static byte[,] TrunkValues(float[,] rawOutput, byte maxValue = 255)
        {
            byte[,] normalizedOutput = new byte[rawOutput.GetLength(0), rawOutput.GetLength(1)];
            for (int i = 0; i < rawOutput.GetLength(0); i++)
            {
                for (int j = 0; j < rawOutput.GetLength(1); j++)
                {
                    normalizedOutput[i, j] = (byte)(rawOutput[i, j] % maxValue);
                }
            }
            return normalizedOutput;
        }
        private static byte[,] ApplyLinearTransform(float[,] rawOutput, float maxValueFound, float minValueFound)
        {
            double scalingFactor = (255 - 0) / (double)(maxValueFound - minValueFound);
            byte[,] normalizedOutput = new byte[rawOutput.GetLength(0), rawOutput.GetLength(1)];
            for (int i = 0; i < rawOutput.GetLength(0); i++)
            {
                for (int j = 0; j < rawOutput.GetLength(1); j++)
                {
                    normalizedOutput[i, j] = (byte)((rawOutput[i, j] - minValueFound) * scalingFactor);
                }
            }
            return normalizedOutput;
        }
        private static Layer[] ReadLayersFromFile(string filename)
        {
            Layer[] output = new Layer[4];
            using Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(filename);
            output[0] = new Layer(ValueType.R, new byte[image.Width, image.Height]);
            output[1] = new Layer(ValueType.G, new byte[image.Width, image.Height]);
            output[2] = new Layer(ValueType.B, new byte[image.Width, image.Height]);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    output[0][i, j] = image[i, j].R;
                    output[1][i, j] = image[i, j].G;
                    output[2][i, j] = image[i, j].B;
                }
            }
            return output;
        }
        public void Save(string filename)
        {
            Image<Rgba32> output = new(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    output[i, j] = new Rgba32(this[i, j, 0], this[i, j, 1], this[i, j, 2]);
                }
            }
            output.SaveAsPng(filename);
        }
    }
}