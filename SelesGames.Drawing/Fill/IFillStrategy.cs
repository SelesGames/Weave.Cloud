using System.Drawing;

namespace SelesGames.Drawing.Fill
{
    public interface IFillStrategy
    {
        Brush CreateBrush(Image image, int targetWidth, int targetHeight);
    }
}
