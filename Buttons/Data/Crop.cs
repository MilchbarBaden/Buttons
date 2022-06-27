namespace Buttons.Data
{
    public class Crop
    {
        public (double x, double y) Scale { get; set; } = (1.0, 1.0);
        public (double x, double y) Offset { get; set; } = (0.0, 0.0);
        public (double x, double y) Size { get; set; } = (0.0, 0.0);
    }
}
