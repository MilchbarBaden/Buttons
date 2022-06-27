namespace Buttons.Data
{
    public class Button : DateEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public ButtonStatus Status { get; set; }
        public Crop Crop { get; set; } = new Crop();

        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }
}
