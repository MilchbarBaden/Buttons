using Buttons.Data;

namespace Buttons.Models
{
    public class ButtonViewModel
    {
        public string Path { get; }
        public Crop Crop { get; }

        public ButtonViewModel(string path, Crop crop)
        {
            Path = path;
            Crop = crop;
        }
    }
}
