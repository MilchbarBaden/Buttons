using Buttons.Data;

namespace Buttons.Models
{
    public record ButtonViewModel(int Id, string Path, string Status, Crop Crop);

    public record ButtonListViewModel(IList<ButtonViewModel> Items);

    public record NameViewModel(string Name);
}
