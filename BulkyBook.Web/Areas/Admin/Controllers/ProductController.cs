using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Web.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll("Category").ToList();
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductViewModel productViewModel = new ProductViewModel
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(category => new SelectListItem
                {
                    Text = category.Name,
                    Value = category.Id.ToString()
                }),
            };

            if (id == null || id == 0)
            {
                return View(productViewModel);
            }
            else
            {
                productViewModel.CategoryList = _unitOfWork.Category.GetAll().Select(category => new SelectListItem
                {
                    Text = category.Name,
                    Value = category.Id.ToString()
                });
                productViewModel.Product = _unitOfWork.Product.Get(u => u.Id == id, "Category");
                return View(productViewModel);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel productViewModel, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                Product product = productViewModel.Product!;
                if (product.Id == 0)
                {
                    if (file != null)
                    {
                        string webRootPath = _webHostEnvironment.WebRootPath;
                        string extension = Path.GetExtension(file.FileName);

                        string newFileName = Guid.NewGuid().ToString() + extension;
                        string productPath = @"images\product";
                        string newFilePath = Path.Combine(webRootPath, productPath);

                        using (var fileStream = new FileStream(Path.Combine(newFilePath, newFileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        product.ImageUrl = Path.Combine(newFilePath, newFileName).Replace("\\", "/");
                    }
                    _unitOfWork.Product.Add(product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product created successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    if (file != null)
                    {
                        string webRootPath = _webHostEnvironment.WebRootPath;
                        string extension = Path.GetExtension(file.FileName);

                        string newFileName = Guid.NewGuid().ToString() + "." + extension;
                        string productPath = @"\images\product";
                        string newFilePath = Path.Combine(webRootPath, productPath);

                        if (string.IsNullOrEmpty(product.ImageUrl))
                        {
                            string existingFile = Path.Combine(product.ImageUrl);
                            System.IO.File.Delete(existingFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(newFilePath, newFileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        product.ImageUrl = Path.Combine(newFilePath, newFileName);
                    }
                    _unitOfWork.Product.Add(product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product updated successfully.";
                    return RedirectToAction("Index");
                }

            }
            else
            {
                productViewModel.CategoryList = _unitOfWork.Category.GetAll().Select(category => new SelectListItem
                {
                    Text = category.Name,
                    Value = category.Id.ToString()
                });
                return View(productViewModel);
            }
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? obj = _unitOfWork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost, ActionName(name: "Delete")]
        public IActionResult DeletePost(int? id)
        {
            Product? obj = _unitOfWork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
