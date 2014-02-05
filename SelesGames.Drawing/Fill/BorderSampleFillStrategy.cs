using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SelesGames.Drawing.Fill
{
    public class BorderSampleFillStrategy : IFillStrategy
    {
        public Brush CreateBrush(Image image, int targetWidth, int targetHeight)
        {
            var width = image.Width;
            var height = image.Height;

            var aspectRatio = (double)width / (double)height;
            var targetAspectRatio = (double)targetWidth / (double)targetHeight;

            var bitmap = image.ToBitmap();
            ColorAggregate average;

            // original aspect ratio is taller than the target aspect ratio
            if (aspectRatio < targetAspectRatio)
            {
                // we only want to sample the left and right columns in this case
                var range = Enumerable.Range(0, height);
                
                var leftColumn = range.Select(i => bitmap.GetPixel(0, i)).ToArray();
                var rightColumn = range.Select(i => bitmap.GetPixel(width - 1, i)).ToArray();

                var leftAverage = Sum(leftColumn);
                var rightAverage = Sum(rightColumn);
                average = (leftAverage + rightAverage) / 2;
            }

            // original aspect ratio is wider than the target aspect ratio
            else
            {
                // we only want to sample the top and bottom rows in this case
                var range = Enumerable.Range(0, width);

                var topRow = range.Select(i => bitmap.GetPixel(i, 0)).ToArray();
                var bottomRow = range.Select(i => bitmap.GetPixel(i, height - 1)).ToArray();

                var topAverage = Sum(topRow);
                var bottomAverage = Sum(bottomRow);
                average = (topAverage + bottomAverage) / 2;
            }

            return new SolidBrush(Color.FromArgb(
                average.A,
                average.R,
                average.G,
                average.B));
        }

        ColorAggregate Sum(IEnumerable<Color> colors)
        {
            var aggregate = new ColorAggregate();
            int index = 0;

            foreach (var color in colors)
            {
                aggregate += color;
                index++;
            }
            return aggregate / index;
        }
    }
}
