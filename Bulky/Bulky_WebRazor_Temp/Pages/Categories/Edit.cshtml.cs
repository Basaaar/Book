using Bulky_WebRazor_Temp.Data;
using Bulky_WebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky_WebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
       
        public Category Category { get; set; }
        public EditModel(ApplicationDbContext _d)
        {
            this._db = _d;
        }
        public void OnGet(int? id)
        {
            if(id!=null && id!=0)
            {
                Category = _db.Categories.Find(id);
            }
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(Category);
                _db.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToPage("Index", "Categories");
            }
            return Page();

        }
       
    }
}
