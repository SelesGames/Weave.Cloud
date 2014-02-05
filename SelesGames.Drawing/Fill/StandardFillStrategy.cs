using System.Drawing;

namespace SelesGames.Drawing.Fill
{
    internal class StandardFillStrategy : IFillStrategy
    {
        Brush fill;

        public StandardFillStrategy(Brush fill)
        {
            this.fill = fill;
        }

        public Brush CreateBrush(Image image, int targetWidth, int targetHeight)
        {
            return fill;
        }
    }
}