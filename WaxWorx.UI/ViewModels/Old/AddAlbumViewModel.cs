using Microsoft.AspNetCore.Mvc.Rendering;

namespace WaxWorx.UI.ViewModels
{
    public class AddAlbumViewModel
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public string Condition { get; set; }

        public IEnumerable<SelectListItem> GenreOptions { get; set; }
        public IEnumerable<SelectListItem> ConditionOptions { get; set; }
    }
}
