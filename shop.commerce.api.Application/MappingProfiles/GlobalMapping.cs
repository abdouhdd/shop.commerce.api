namespace shop.commerce.api.Application.MappingProfiles
{
    using AutoMapper;
    using shop.commerce.api.domain.Views;
    using shop.commerce.api.domain.Entities;

    public class GlobalMapping : Profile
    {
        public GlobalMapping()
        {

        }

        #region produits

        public void Prestation()
        {
            // prestation
            CreateMap<Product, ProductView>().ReverseMap();
            CreateMap<Product, ProductDetailView>().ReverseMap();
            CreateMap<Category, CategoryView>().ReverseMap();
            CreateMap<Category, CategoryModelView>().ReverseMap();

        }

        #endregion


    }
}
