using System.Diagnostics;

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
			Width = pixelValues.GetLength(0);
			Height = pixelValues.GetLength(1);

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
            object syncLock = new object();
            Parallel.For(0, Width, i =>
            {
                for (int j = 0; j < Height; j++)
                {
                    float result = ApplyFilterToPixel(i, j, layerIndex, filter);
                    lock (syncLock)
                    {
                        maxValueFound = result > maxValueFound ? result : maxValueFound;
                        minValueFound = result < minValueFound ? result : minValueFound;
                    }
                    output[i, j] = result;
                }
            });

            if (applyTransform)
            {
                return new Layer(Layers[layerIndex].Type, ApplyLinearTransform(output, maxValueFound, minValueFound));
            }
            else
            {
                return new Layer(Layers[layerIndex].Type, TrunkValues(output));
            }
        }
        private float ApplyFilterToPixel(int pixelXPos, int pixelYPos, int layerIndex, Filter filter)
		{
			int displacement = (filter.MaskSize - 1) / 2;
			float result = 0;
			int filterPosX = 0;
			int filterPosY;
			for (int xPos = pixelXPos - displacement; (filterPosX < filter.MaskSize) && (xPos < Width); xPos++)
			{
				filterPosY = 0;
				for (int yPos = pixelYPos - displacement; (filterPosY < filter.MaskSize) && (yPos < Height); yPos++)
				{
					if (xPos < 0 || yPos < 0 || xPos >= Width || yPos >= Height)
					{
						filterPosY++;
						continue;
					}
					else
					{
						result += this[xPos, yPos, layerIndex] * filter[filterPosX, filterPosY];
						filterPosY++;
					}
				}
				filterPosX++;
			}
			return result;
		}
        private static byte[,] TrunkValues(float[,] rawOutput, byte maxValue = 255)
        {
            int rows = rawOutput.GetLength(0);
            int cols = rawOutput.GetLength(1);
            byte[,] normalizedOutput = new byte[rows, cols];
            Parallel.For(0, rows, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    normalizedOutput[i, j] = (byte)(rawOutput[i, j] % maxValue);
                }
            });
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
			using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(filename);

			var output = new Layer[]
			{
		new Layer(ValueType.R, new byte[image.Width, image.Height]),
		new Layer(ValueType.G, new byte[image.Width, image.Height]),
		new Layer(ValueType.B, new byte[image.Width, image.Height]),
		new Layer(ValueType.A, new byte[image.Width, image.Height])
			};
			Parallel.For(0, image.Width, x =>
			{
				for (int y = 0; y < image.Height; y++)
				{
					output[0][x, y] = image[x, y].R;
					output[1][x, y] = image[x, y].G;
					output[2][x, y] = image[x, y].B;
					output[3][x, y] = image[x, y].A;
				}
			});
			return output;
		}
        public void Save(string filename)
        {
            Image<Rgba32> output = new(Width, Height);
            Parallel.For(0, Width, i =>
            {
                for (int j = 0; j < Height; j++)
                {
                    output[i, j] = new Rgba32(this[i, j, 0], this[i, j, 1], this[i, j, 2]);
                }
            });
            output.SaveAsPng(filename);
        }
    }
}