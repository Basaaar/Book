using Bulky_WebRazor_Temp.Data;
using Bulky_WebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.CompilerServices;

namespace Bulky_WebRazor_Temp.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public List<Category> CategoryList {  get; set; }
        public IndexModel(ApplicationDbContext _d)
        {
            this._db = _d;
        }
        public void OnGet()//Method name not changed.If ypo
        {
            CategoryList=_db.Categories.ToList();
        }
    }
}
