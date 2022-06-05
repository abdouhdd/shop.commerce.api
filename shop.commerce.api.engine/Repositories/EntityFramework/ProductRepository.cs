using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using shop.commerce.api.domain.Extensions;
using shop.commerce.api.domain.Filters;
using shop.commerce.api.domain.Request;
using shop.commerce.api.domain.Views;
using shop.commerce.api.domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class ProductRepository : IProductRepository
    {
        public List<Product> FindProductsByIds(int[] ids)
        {
            List<Product> result = _entity.Where(p => ids.Contains(p.Id)).ToList();
            return result;
        }

        public List<Product> FindAllProducts()
        {
            List<Product> result = _entity.ToList();
            return result;
        }
        
        public bool AllProducts(Func<Product, bool> predicate)
        {
            bool result = _entity.All(predicate);
            return result;
        }

        public List<Product> GetProductByPosition(int from, int to)
        {
            List<Product> result = _entity.Where(p => p.Position >= from && (to == -1 || p.Position <= to)).ToList();
            return result;
        }

        public List<ProductView> GetProductsViewUser(ProductFilterUser productFilter)
        {
            bool isNotSearch = string.IsNullOrWhiteSpace(productFilter.Search);
            string search = !isNotSearch ? productFilter.Search.Replace("_", " ") : "";

            int[] categories = productFilter.Categories?.Length > 0 ? productFilter.Categories : null;
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join i in _context.ProductImages on p.Id equals i.ProductId
                        where p.Active && i.IsMaster && i.Position == 0 && (categories == null || (p.CategoryId.HasValue && categories.Contains(p.CategoryId.Value))) && (isNotSearch || p.SearchTerms.Contains(search.ToLower()))
                        orderby p.CountView descending
                        select new ProductView
                        {
                            Id = p.Id,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Active = p.Active,
                            Image = i.Filename,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                            Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                            Category = c.Name,
                            Rating = p.Rating,
                            CountView = p.CountView,
                        };
            return query.ToList();
        }

        public ResultPage<ProductView> GetProductsViewUserPage(ProductFilterUser productFilter)
        {
            try
            {
                bool isNotSearch = string.IsNullOrWhiteSpace(productFilter.Search);
                string search = !isNotSearch ? productFilter.Search.Replace("_", " ") : "";
                int[] categories = productFilter.Categories;
                bool filterByPrice = productFilter.Price?.Length == 2;
                decimal priceMin = 0;
                decimal priceMax = 0;
                if (filterByPrice)
                {
                    priceMin = productFilter.Price[0];
                    priceMax = productFilter.Price[1];
                }
                var query = from p in _entity
                            join c in _context.Categories on p.CategoryId equals c.Id
                            join i in _context.ProductImages on p.Id equals i.ProductId
                            where p.Active && i.IsMaster && i.Position == 0 && (!filterByPrice || (p.NewPrice > priceMin && p.NewPrice < priceMax)) && (categories == null || categories.Contains(p.CategoryId ?? 0)) && (isNotSearch || p.SearchTerms.Contains(search.ToLower()))
                            orderby p.Position
                            // orderby p.Rating descending, p.Position ascending, p.CountSale descending, p.CountView descending
                            select new ProductView
                            {
                                Id = p.Id,
                                Name = p.Name,
                                ShortName = p.ShortName,
                                Active = p.Active,
                                Image = i.Filename,
                                NewPrice = p.NewPrice,
                                OldPrice = p.OldPrice,
                                Slug = p.Slug,
                                IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                                Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                                Category = c.Name,
                                Rating = p.Rating,
                                CountView = p.CountView
                            };
                //int id = 0;

                //Func<ProductView, bool> filter = (p) => {
                //    bool ok = false;
                //    if (p.Id != id)
                //    {
                //        ok = true;
                //    }
                //    return ok;
                //};
                int totalCount = query.Count();
                ResultPage<ProductView> resultPage = ResultPage<ProductView>.PageData(query, productFilter.PageNumber, productFilter.Length, totalCount);
                return resultPage;
            }
            catch (Exception)
            {
                return ResultPage<ProductView>.PageData(new ProductView[0], 1, 1, 0);
            }
        }

        public List<ProductView> GetRelatedProducts(int categoryId, int count)
        {
            var query = from p in _entity
                        // join c in _context.Categories on p.CategoryId equals c.Id
                        join i in _context.ProductImages on p.Id equals i.ProductId
                        where p.Active && i.IsMaster && i.Position == 0 && p.CategoryId == categoryId
                        orderby p.CountView descending
                        select new ProductView
                        {
                            Id = p.Id,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Active = p.Active,
                            Image = i.Filename,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                            Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                            //Category = c.Name,
                            Rating = p.Rating,
                            CountView = p.CountView,
                            Quantity = p.Quantity
                        };
            return query.Take(count).ToList();
        }

        public List<ProductView> GetLatestProductsViewUser(int count)
        {
            // int categoryId = productFilter.CategoryId.GetValueOrDefault();
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join i in _context.ProductImages on p.Id equals i.ProductId
                        where p.Active && i.IsMaster && i.Position == 0 //&& (categoryId == 0 || p.CategoryId == categoryId)
                        orderby p.CountView descending
                        select new ProductView
                        {
                            Id = p.Id,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Active = p.Active,
                            Image = i.Filename,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                            Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                            Category = c.Name,
                            Rating = p.Rating,
                            CountView = p.CountView
                        };
            return query.Take(count).ToList();
        }

        public List<ProductView> GetProductsViewAdmin(ProductFilterAdmin productFilter)
        {
            bool isNotSearch = string.IsNullOrWhiteSpace(productFilter.Search);
            string search = !isNotSearch ? productFilter.Search.Replace("_", " ") : "";

            int categoryId = productFilter.CategoryId.GetValueOrDefault();
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join i in _context.ProductImages on p.Id equals i.ProductId
                        where p.Active && i.IsMaster && i.Position == 0 && (categoryId == 0 || p.CategoryId == categoryId) && (isNotSearch || p.SearchTerms.Contains(search.ToLower()) && (productFilter.Seller == null || p.Admin == productFilter.Seller))
                        select new ProductView
                        {
                            Id = p.Id,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Active = p.Active,
                            Image = i.Filename,
                            Quantity = p.Quantity,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                            Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                            Category = c.Name,
                            Rating = p.Rating
                        };
            return query.ToList();
        }

        public ResultPage<ProductView> GetProductsViewAdminPage(ProductFilterAdmin productFilter, int[] categories)
        {
            bool isNotSearch = string.IsNullOrWhiteSpace(productFilter.Search);
            string search = !isNotSearch ? productFilter.Search.Replace("_", " ") : "";
            //int categoryId = productFilter.CategoryId.GetValueOrDefault();
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join i in _context.ProductImages on p.Id equals i.ProductId
                        where i.IsMaster && i.Position == 0 && (categories == null || (p.CategoryId.HasValue && categories.Contains(p.CategoryId.Value))) && (isNotSearch || p.SearchTerms.Contains(search.ToLower())) && (productFilter.Seller == null || p.Admin == productFilter.Seller)
                        orderby p.Position
                        //orderby p.Rating descending, p.Position ascending
                        select new ProductView
                        {
                            Id = p.Id,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Active = p.Active,
                            Image = i.Filename,
                            Quantity = p.Quantity,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                            Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                            Category = c.Name,
                            Rating = p.Rating,
                            CountView = p.CountView,
                            Position = p.Position
                        };
            int totalCount = query.Count();
            ResultPage<ProductView> resultPage = ResultPage<ProductView>.PageData(query, productFilter.PageNumber, productFilter.Length, totalCount);
            return resultPage;
        }

        public ProductDetailView GetProductDetailView(string slug)
        {
            ProductDetailView productDetailView = null;
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join image in _context.ProductImages on p.Id equals image.ProductId
                        orderby image.Position ascending
                        where p.Active && p.Slug == slug
                        select new { Product = p, Category = c.Name, Image = image };
            List<string> images = new List<string>();
            foreach (var item in query)
            {
                if (productDetailView == null)
                {
                    productDetailView = new ProductDetailView
                    {
                        Id = item.Product.Id,
                        CategoryId = item.Product.CategoryId,
                        Slug = item.Product.Slug,
                        Name = item.Product.Name,
                        Description = item.Product.Description,
                        Details = item.Product.Details,
                        Specification = item.Product.Specification,
                        MainCharacteristics = item.Product.MainCharacteristics,
                        TechnicalDescription = item.Product.TechnicalDescription,
                        General = item.Product.General,
                        Garantie = item.Product.Garantie,
                        VenduWith = item.Product.VenduWith,
                        Active = item.Product.Active,
                        Category = item.Category,
                        Image = item.Image.Filename,
                        NewPrice = item.Product.NewPrice,
                        OldPrice = item.Product.OldPrice,
                        IsOffer = Calcul.IsOffer(item.Product.OldPrice, item.Product.NewPrice),
                        Offer = Calcul.Offer(item.Product.OldPrice, item.Product.NewPrice),
                        Rating = item.Product.Rating,
                        MetaTitle = item.Product.MetaTitle,
                        MetaDescription = item.Product.MetaDescription,
                        MetaKeywords = item.Product.MetaKeywords,
                        Quantity = item.Product.Quantity,
                        CountView = item.Product.CountView
                    };
                }
                images.Add(item.Image.Filename);
            }
            if (productDetailView != null)
            {
                productDetailView.Images = images.ToArray();
            }
            return productDetailView;
        }

        public List<string> FindImages(int productId)
        {
            List<string> photos = _context.ProductImages.Where(i => i.ProductId == productId).Select(i => i.Filename).ToList();
            return photos;
        }

        //public int SaveImages(List<ProductImage> productImages)
        //{
        //    _context.ProductImages.AddRange(productImages);
        //    int output = _context.SaveChanges();
        //    return output;
        //}

        public int DeleteImages(List<ProductImage> productImages)
        {
            _context.ProductImages.RemoveRange(productImages);
            int output = _context.SaveChanges();
            return output;
        }

        public Product GetProductBy(string slug)
        {
            return _entity.Where(p => p.Slug == slug).SingleOrDefault();
        }

        public Product[] GetProductsBySlugs(string[] slugs)
        {
            return _entity.Where(p => slugs.Contains(p.Slug)).ToArray();
        }

        public List<ProductImage> GetImages(int productId)
        {
            return _context.ProductImages.Where(p => p.ProductId == productId).ToList();
        }
        
        public List<ProductImage> GetImagesBySlug(string slug)
        {
            var query = from p in _entity
                        join img in _context.ProductImages
                        on p.Id equals img.ProductId
                        where p.Slug == slug
                        select img;

            //return _context.ProductImages.Join(_entity, (img) => img.ProductId, (p) => p.Id, (img,p) => img)
            //    .Where(p => p.slug == slug).ToList();
            return query.ToList();
        }

        //public int UpdateProductCountView(int productId)
        //{
        //    Product product = _entity.Find(productId);
        //    int output = 0;
        //    if (product != null)
        //    {
        //        product.CountView++;
        //        output = _context.SaveChanges();
        //    }
        //    //int output = _context.Database.ExecuteSqlRaw("Update 'products' set 'CountView' = CountView + 1");
        //    return output;
        //}

        public List<ProductView> GetProductsTopRating(int count)
        {
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join i in _context.ProductImages on p.Id equals i.ProductId
                        where p.Active && i.IsMaster && i.Position == 0
                        orderby p.Rating descending, p.CountView descending
                        select new ProductView
                        {
                            Id = p.Id,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Active = p.Active,
                            Image = i.Filename,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                            Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                            Category = c.Name,
                            Rating = p.Rating,
                            CountView = p.CountView
                        };
            return query.Take(count).ToList();
        }

        public List<ProductView> GetProductsOffer(int count)
        {
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join i in _context.ProductImages on p.Id equals i.ProductId
                        where p.Active && i.IsMaster && i.Position == 0 && p.IsOffer == true
                        select new ProductView
                        {
                            Id = p.Id,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Active = p.Active,
                            Image = i.Filename,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                            Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                            Category = c.Name,
                            Rating = p.Rating,
                            CountView = p.CountView
                        };
            return query.Take(count).ToList();
        }

        public List<ProductView> GetProductsTopSale(int count)
        {
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join i in _context.ProductImages on p.Id equals i.ProductId
                        where p.Active && i.IsMaster && i.Position == 0
                        orderby p.CountSale descending
                        select new ProductView
                        {
                            Id = p.Id,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Active = p.Active,
                            Image = i.Filename,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                            Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                            Category = c.Name,
                            Rating = p.Rating,
                            CountView = p.CountView
                        };
            return query.Take(count).ToList();
        }

        public int RemoveProduct(string slug)
        {
            Product product = _entity.Where(p => p.Slug == slug).SingleOrDefault();
            int output = 0;
            if (product != null)
            {
                _entity.Remove(product);
                output = _context.SaveChanges();
            }
            return output;
        }
        
        public int RemoveProduct(Product product)
        {
            int output = 0;
            if (product != null)
            {
                _entity.Remove(product);
                output = _context.SaveChanges();
            }
            return output;
        }

        public (string filename,string slug) RemoveProductImage(string imageGuid)
        {
            ProductImage[] images = _context.ProductImages.ToArray();
            ProductImage productImage = images.Where(p => p.Id == imageGuid).SingleOrDefault();
            int output = 0;
            if (productImage != null)
            {
                string slug = _entity.Where(s => s.Id == productImage.ProductId).Single().Slug;
                _context.ProductImages.Remove(productImage);
                output = _context.SaveChanges();
                return (productImage.Filename, slug);
            }
            return ("", "");
        }

        public ProductEdit GetProductEdit(string slug)
        {
            ProductEdit productDetailView = null;
            var query = from p in _entity
                        join c in _context.Categories on p.CategoryId equals c.Id
                        join image in _context.ProductImages on p.Id equals image.ProductId into pimage
                        from pi in pimage.DefaultIfEmpty()
                        where p.Slug == slug
                        select new
                        {
                            Id = p.Id,
                            Slug = p.Slug,
                            Name = p.Name,
                            ShortName = p.ShortName,
                            Description = p.Description,
                            Details = p.Details,
                            Specification = p.Specification,
                            MainCharacteristics = p.MainCharacteristics,
                            TechnicalDescription = p.TechnicalDescription,
                            General = p.General,
                            Garantie = p.Garantie,
                            VenduWith = p.VenduWith,
                            Active = p.Active,
                            CategoryId = c.Id,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Rating = p.Rating,
                            MetaTitle = p.MetaTitle,
                            MetaDescription = p.MetaDescription,
                            MetaKeywords = p.MetaKeywords,
                            Quantity = p.Quantity,
                            CountView = p.CountView,
                            Image = pi
                        };
            List<ImageView> imageViews = new List<ImageView>();
            foreach (var p in query)
            {
                if (productDetailView == null)
                {
                    productDetailView = new ProductEdit
                    {
                        Id = p.Id,
                        Slug = p.Slug,
                        Name = p.Name,
                        ShortName = p.ShortName,
                        Description = p.Description,
                        Details = p.Details,
                        Specification = p.Specification,
                        MainCharacteristics = p.MainCharacteristics,
                        TechnicalDescription = p.TechnicalDescription,
                        General = p.General,
                        Garantie = p.Garantie,
                        VenduWith = p.VenduWith,
                        Active = p.Active,
                        CategoryId = p.CategoryId,
                        NewPrice = p.NewPrice,
                        OldPrice = p.OldPrice,
                        Rating = p.Rating,
                        MetaTitle = p.MetaTitle,
                        MetaDescription = p.MetaDescription,
                        MetaKeywords = p.MetaKeywords,
                        Quantity = p.Quantity,
                        CountView = p.CountView,
                        IsOffer = Calcul.IsOffer(p.OldPrice, p.NewPrice),
                        Offer = Calcul.Offer(p.OldPrice, p.NewPrice),
                    };
                }
                if (p.Image != null)
                {
                    imageViews.Add(new ImageView
                    {
                        Image = p.Image.Filename,
                        ImageGuid = p.Image.Id,
                        Position = p.Image.Position
                    });
                }
                productDetailView.Images = imageViews.ToArray();
            }
            return productDetailView;
        }
    }

    public partial class ProductRepository : Repository<Product, int>
    {
        //private readonly ShopContext _context;

        //public ProductRepository(ShopContextFactory shopContextFactory)
        //{
        //    this._context = shopContextFactory.GetShopContext();
        //}

        public ProductRepository(ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory)
        {
        }
    }
}
