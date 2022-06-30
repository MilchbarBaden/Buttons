using Buttons.Data;

namespace Buttons.Models
{
    public class ButtonViewModel
    {
        public int Id { get; set; }
        public string Path { get; }
        public Crop Crop { get; }

        public ButtonViewModel(int id, string path, Crop crop)
        {
            Id = id;
            Path = path;
            Crop = crop;
        }
    }
}
