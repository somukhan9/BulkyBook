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
                string productPath = @"images\product";
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string extension = Path.GetExtension(file.FileName);
                    string newFileName = Guid.NewGuid().ToString() + "." + extension;
                    string newFilePath = Path.Combine(webRootPath, productPath);

                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        if (System.IO.File.Exists(Path.Combine(webRootPath, product.ImageUrl.TrimStart('\\'))))
                        {
                            System.IO.File.Delete(Path.Combine(webRootPath, product.ImageUrl.TrimStart('\\')));
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(newFilePath, newFileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    product.ImageUrl = @"\" + productPath + @"\" + newFileName;
                }
                if (product.Id == 0)
                {
                    _unitOfWork.Product.Add(product);
                    TempData["success"] = "Product created successfully.";
                }
                else
                {
                    _unitOfWork.Product.Update(product);
                    TempData["success"] = "Product updated successfully.";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
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
            Product? product = _unitOfWork.Product.Get(u => u.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            string webRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(product.ImageUrl))
            {
                if (System.IO.File.Exists(Path.Combine(webRootPath, product.ImageUrl)))
                {
                    System.IO.File.Delete(Path.Combine(webRootPath, product.ImageUrl));
                }
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
