using Buttons.Data;

namespace Buttons.Models
{
    public record ButtonViewModel(int Id, string Path, string Status, Crop Crop);

    public record ButtonListViewModel(IList<ButtonViewModel> Items);

    public record NameViewModel(string Name);

    public record AdminButtonViewModel(int Id, string Path, string Status, Crop Crop, string Name, string OwnerId, string OwnerName) :
        ButtonViewModel(Id, Path, Status, Crop);

    public record AdminButtonListViewModel(IList<AdminButtonViewModel> Items);
}
