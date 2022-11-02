using Microsoft.EntityFrameworkCore;

namespace Buttons.Data
{
    [Index(nameof(Path), IsUnique = true)]
    public class Button : DateEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public ButtonStatus Status { get; set; }
        public virtual Crop Crop { get; set; } = new Crop();

        public string OwnerId { get; set; }
        public virtual Owner Owner { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public Button(Owner owner)
        {
            Owner = owner;
            OwnerId = owner.Id;
        }

#nullable disable
        private Button() { }
#nullable restore
    }
}
