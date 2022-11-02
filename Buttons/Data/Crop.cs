using Microsoft.EntityFrameworkCore;

namespace Buttons.Data
{
    [Owned]
    public class Crop
    {
        public double ScaleX { get; set; } = 1.0;
        public double ScaleY { get; set; } = 1.0;
        public double OffsetX { get; set; } = 0.0;
        public double OffsetY { get; set; } = 0.0;
        public double Width { get; set; } = 0.0;
        public double Height { get; set; } = 0.0;

        public Crop Clone() => new()
        {
            ScaleX = ScaleX,
            ScaleY = ScaleY,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            Width = Width,
            Height = Height,
        };
    }
}
