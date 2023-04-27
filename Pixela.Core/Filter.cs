using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixela.Core
{
    public class Filter
    {
        public int MaskSize { get; set; }
        public float[,] Mask { get; set; }
        public bool ApplyFactor { get; set; }
        public float Factor { get; set; }
        public Filter(float[,] mask, bool applyFactor = true, float factor = 1)
        {
            MaskSize = mask.GetLength(0);
            Mask = mask;
            ApplyFactor = applyFactor;
            Factor = factor;
        }
        public float this[int x, int y]
        {
            get
            {
                return Mask[x, y];
            }
            set
            {
                Mask[x, y] = value;
            }
        }
        public Filter(float[] maskArray, bool applyFactor = true, float factor = 1)
        {
            var output = GetMatrix(maskArray);
            Mask = output.Item1;
            MaskSize = output.Item2;
            ApplyFactor = applyFactor;
            Factor = factor;
        }
        private static Tuple<float[,], int> GetMatrix(float[] maskArray)
        {
            var size = (int)Math.Sqrt(maskArray.Length);
            var output = new float[size, size];
            for (int i = 0; i < maskArray.Length; i++)
            {
                output[i / size, i % size] = maskArray[i];
            }
            return new Tuple<float[,], int>(output, size);
        }
    }
}
