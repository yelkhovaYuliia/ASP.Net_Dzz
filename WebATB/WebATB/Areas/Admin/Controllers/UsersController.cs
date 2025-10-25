using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebATB.Areas.Admin.Models.Users;
using WebATB.Data;
using WebATB.Data.Entities.Idenity;
using WebATB.Data.Entities.Identity;

namespace WebATB.Areas.Admin.Controllers;

[Area("Admin")]
public class UsersController(AppATBDbContext dbContext, IMapper mapper, UserManager<UserEntity> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = await dbContext.Users
            .ProjectTo<UserItemVM>(mapper.ConfigurationProvider)
            .ToListAsync();

        return View(users);
    }

    public async Task<IActionResult> Ban(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            // logout user
            await userManager.UpdateSecurityStampAsync(user);
        }
        return RedirectToAction("Index");
    }
}
