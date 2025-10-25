using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using WebATB.Data;
using WebATB.Data.Entities;
using WebATB.Interfaces;
using WebATB.Models.Helpers;
using WebATB.Models.Product;

namespace WebATB.Controllers;

public class ProductsController(AppATBDbContext dbContext,
    IMapper mapper,
    IImageService imageService) : Controller
{
    public IActionResult Index()
    {
        var model = dbContext.Products
            .Where(p => p.IsDeleted != true)
            .ProjectTo<ProductItemModel>(mapper.ConfigurationProvider)
            .ToList();

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ProductCreateModel model = new ProductCreateModel();
        model.Categories = dbContext.Categories
            .ProjectTo<SelectItemHelper>(mapper.ConfigurationProvider)
            .ToList();
        return View(model);
    }

    [HttpPost]
    public IActionResult Create(ProductCreateModel model)
    {
        //var request = this.Request;
            if (!ModelState.IsValid)
        {
            model.Categories = dbContext.Categories
                .ProjectTo<SelectItemHelper>(mapper.ConfigurationProvider)
                .ToList();
            return View(model);
        }
        var entity = mapper.Map<ProductEntity>(model);
        dbContext.Products.Add(entity);
        dbContext.SaveChanges();
        if (model.Images != null && model.Images.Length > 0)
        {
            short priority = 1;
            foreach (var image in model.Images)
            {
                if (image.Length > 0)
                {
                    var imageName = imageService.SaveImageAsync(image).Result;
                    var imageEntity = new ProductImageEntity
                    {
                        ProductId = entity.Id,
                        Priority = priority++,
                        Name = imageName
                    };
                    dbContext.ProductImages.Add(imageEntity);
                }
            }
            dbContext.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public IActionResult Update(int id)
    {
        var model = dbContext.Products.
            ProjectTo<ProductUpdateModel>(mapper.ConfigurationProvider)
            .FirstOrDefault(c => c.Id == id);

        model.Categories = dbContext.Categories
            .ProjectTo<SelectItemHelper>(mapper.ConfigurationProvider)
            .ToList();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Update(ProductUpdateModel model)
    {
        var entity = dbContext.Products.FirstOrDefault(c => c.Id == model.Id);
        if (entity == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.Categories = dbContext.Categories
                .ProjectTo<SelectItemHelper>(mapper.ConfigurationProvider)
                .ToList();
            return View(model);
        }

        var oldImages = dbContext.ProductImages.Where(c => c.ProductId == model.Id).ToList();
        if (oldImages.Count > 0)
        {
            foreach (var image in oldImages)
            {
                await imageService.DeleteImageAsync(image.Name);
                dbContext.Remove(image);
            }
        }

        if (model.Images != null && model.Images.Length > 0)
        {
            short priority = 1;
            foreach (var image in model.Images)
            {
                if (image.Length > 0)
                {
                    var imageName = await imageService.SaveImageAsync(image);
                    dbContext.ProductImages.Add(new ProductImageEntity
                    {
                        ProductId = entity.Id,
                        Priority = priority++,
                        Name = imageName
                    });
                }
            }
        }

        mapper.Map(model, entity);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var category = dbContext.Products.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        category.IsDeleted = true;
        dbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

}