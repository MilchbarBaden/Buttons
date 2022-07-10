namespace Buttons.Models
{
    public class ButtonListViewModel
    {
        public IList<ButtonViewModel> Items { get; set; }

        public ButtonListViewModel(IList<ButtonViewModel> items) => Items = items;
    }
}
