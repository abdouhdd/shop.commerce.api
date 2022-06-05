using Microsoft.Extensions.Logging;
using shop.commerce.api.domain.Enum;
using shop.commerce.api.domain.Views;
using shop.commerce.api.domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class OrderRepository : IOrderRepository
    {
        
       
        public Order GetOrderBy(string orderNumber)
        {
            Order order = _entity.Where(o => o.OrderNumber == orderNumber).SingleOrDefault();
            return order;
        }

        public OrderView GetOrderViewBy(string orderNumber)
        {
            var query = from order in _entity
                        join oi in _context.OrderItems on order.Id equals oi.OrderId
                        join p in _context.Products on oi.ProductId equals p.Id
                        where order.OrderNumber == orderNumber
                        select new { order, oi, p };

            OrderView orderView = null;
            foreach (var item in query)
            {
                if (orderView == null)
                {
                    orderView = new OrderView
                    {
                        Email = item.order.Email,
                        FullName = item.order.FullName,
                        Username = item.order.Username,
                        OrderNumber = item.order.OrderNumber,
                        OrdersNote = item.order.OrdersNote,
                        Phone = item.order.Phone,
                        PaymentMethod = item.order.PaymentMethod,
                        Address = item.order.Address,
                        Country = item.order.Country,
                        IsPaid = item.order.IsPaid,
                        Status = item.order.Status,
                        ZipCode = item.order.ZipCode,
                        TotalAmount = item.order.TotalAmount,
                        TotalQty = item.order.TotalQty,
                        City = item.order.City,
                        AddressIp = item.order.AddressIp,
                        Browser = item.order.Browser,
                        Items = new List<OrderItemView>(),
                    };
                }
                if (item.oi != null)
                {
                    orderView.Items.Add(new OrderItemView
                    {
                        OrderItemNumber = item.oi.OrderItemNumber,
                        Name = item.p.Name,
                        ShortName = item.p.ShortName,
                        Qty = item.oi.Qty,
                        Price = item.oi.Price,
                        TotalPrice = item.oi.TotalPrice,
                        Rating = item.p.Rating,
                        NewPrice = item.p.NewPrice,
                        OldPrice = item.p.OldPrice,
                        Slug = item.p.Slug,
                        Image = item.p.Image,
                        Offer = item.p.Offer, //Calcul.Offer(item.oi.OldPrice, item.oi.NewPrice),
                        IsOffer = item.p.IsOffer, //Calcul.IsOffer(item.oi.OldPrice, item.oi.NewPrice, v.SpecialFromDate, v.SpecialToDate)
                    });
                }
            }
            return orderView;
        }

        public IEnumerable<OrderView> GetOrdersView(EnumOrderStatus? status, string search)
        {
            bool searchBy = !string.IsNullOrWhiteSpace(search);
            var query = from order in _entity
                        join oi in _context.OrderItems on order.Id equals oi.OrderId
                        join p in _context.Products on oi.ProductId equals p.Id
                        where (status == null || order.Status == status) && (!searchBy || order.SearchTerms.Contains(search.ToLower()))
                        orderby order.Id descending
                        select new { order, oi, p };

            List<OrderView> ordersView = new List<OrderView>();
            OrderView orderView = null;
            int id = 0;
            List<OrderItemView> items = null;
            int c = query.Count();
            foreach (var item in query)
            {
                //Console.WriteLine($"{id} > {item.order.Id}");
                if (item.order.Id != id)
                {
                    items = new List<OrderItemView>();
                    orderView = new OrderView
                    {
                        Email = item.order.Email,
                        FullName = item.order.FullName,
                        Username = item.order.Username,
                        OrderNumber = item.order.OrderNumber,
                        Phone = item.order.Phone,
                        OrdersNote = item.order.OrdersNote,
                        PaymentMethod = item.order.PaymentMethod,
                        Address = item.order.Address,
                        Country = item.order.Country,
                        IsPaid = item.order.IsPaid,
                        Status = item.order.Status,
                        ZipCode = item.order.ZipCode,
                        TotalAmount = item.order.TotalAmount,
                        TotalQty = item.order.TotalQty,
                        City = item.order.City,
                        Items = items,
                        //DeliveredAt = item.order.DeliveredAt,
                        //ProcessAt = item.order.ProcessAt,
                        CreatedAt = item.order.InsertDate,
                    };
                    ordersView.Add(orderView);
                }
                if (item.oi != null)
                {
                    items.Add(new OrderItemView
                    {
                        OrderItemNumber = item.oi.OrderItemNumber,
                        Name = item.p.Name,
                        Qty = item.oi.Qty,
                        Price = item.oi.Price,
                        TotalPrice = item.oi.TotalPrice,
                        Rating = item.p.Rating,
                        NewPrice = item.p.NewPrice,
                        OldPrice = item.p.OldPrice,
                        Slug = item.p.Slug,
                        Image = item.p.Image,
                        Offer = item.p.Offer, //Calcul.Offer(item.oi.OldPrice, item.oi.NewPrice),
                        IsOffer = item.p.IsOffer, //Calcul.IsOffer(item.oi.OldPrice, item.oi.NewPrice, v.SpecialFromDate, v.SpecialToDate)
                    });
                }
                id = item.order.Id;
            }
            return ordersView;
        }

        public OrderView GetOrderView(string orderNumber)
        {
            var query = from order in _entity
                        join oi in _context.OrderItems on order.Id equals oi.OrderId
                        join p in _context.Products on oi.ProductId equals p.Id
                        where order.OrderNumber == orderNumber
                        select new { order, oi, p };

            OrderView orderView = null;
            List<OrderItemView> items = null;
            foreach (var item in query)
            {
                if (orderView == null)
                {
                    items = new List<OrderItemView>();
                    orderView = new OrderView
                    {
                        Id = item.order.Id,
                        Email = item.order.Email,
                        FullName = item.order.FullName,
                        Username = item.order.Username,
                        OrderNumber = item.order.OrderNumber,
                        Phone = item.order.Phone,
                        OrdersNote = item.order.OrdersNote,
                        PaymentMethod = item.order.PaymentMethod,
                        Address = item.order.Address,
                        Country = item.order.Country,
                        IsPaid = item.order.IsPaid,
                        Status = item.order.Status,
                        ZipCode = item.order.ZipCode,
                        TotalAmount = item.order.TotalAmount,
                        TotalQty = item.order.TotalQty,
                        City = item.order.City,
                        CreatedAt = item.order.InsertDate,
                        Items = items,
                    };
                }
                if (item.oi != null)
                {
                    items.Add(new OrderItemView
                    {
                        OrderItemNumber = item.oi.OrderItemNumber,
                        Name = item.p.Name,
                        Qty = item.oi.Qty,
                        Price = item.oi.Price,
                        TotalPrice = item.oi.TotalPrice,
                        Rating = item.p.Rating,
                        NewPrice = item.p.NewPrice,
                        OldPrice = item.p.OldPrice,
                        Slug = item.p.Slug,
                        Image = item.p.Image,
                        Offer = item.p.Offer, //Calcul.Offer(item.oi.OldPrice, item.oi.NewPrice),
                        IsOffer = item.p.IsOffer, //Calcul.IsOffer(item.oi.OldPrice, item.oi.NewPrice, v.SpecialFromDate, v.SpecialToDate)
                    });
                }
            }
            return orderView;
        }

        public OrderItemView GetOrderDetailsView(string orderItemNumber)
        {
            OrderItemView orderItemView = null;
            var query = from order in _entity
                        join oi in _context.OrderItems on order.Id equals oi.OrderId
                        join p in _context.Products on oi.ProductId equals p.Id
                        where oi.OrderItemNumber == orderItemNumber
                        select new OrderItemView
                        {
                            OrderItemNumber = oi.OrderItemNumber,
                            Name = p.Name,
                            Qty = oi.Qty,
                            Price = oi.Price,
                            TotalPrice = oi.TotalPrice,
                            Rating = p.Rating,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            Slug = p.Slug,
                            Image = p.Image,
                            Offer = p.Offer, //Calcul.Offer(oi.OldPrice, oi.NewPrice),
                            IsOffer = p.IsOffer, //Calcul.IsOffer(oi.OldPrice, oi.NewPrice, v.SpecialFromDate, v.SpecialToDate)
                        };
            orderItemView = query.SingleOrDefault();
            return orderItemView;
        }

        public int UpdateOrderStatus(Order order, EnumOrderStatus status)
        {
            order.Status = status;
            int output = _context.SaveChanges();
            OrderTracking orderTracking = new OrderTracking
            {
                OrderId = order.Id,
                Status = status,
                Date = DateTime.UtcNow
            };
            _context.OrderTrackings.Add(orderTracking);
            output = _context.SaveChanges();
            return output;
        }

        public IEnumerable<OrderTracking> GetOrderTrackings(int orderId)
        {
            IEnumerable<OrderTracking> orderTrackings = _context.OrderTrackings.Where(ot => ot.OrderId == orderId).ToList();
            return orderTrackings;
        }
    }

    public partial class OrderRepository : Repository<Order, int>
    {
        //private readonly ShopContext _context;

        //public OrderRepository(ShopContextFactory shopContextFactory)
        //{
        //    this._context = shopContextFactory.GetShopContext();
        //}

        public OrderRepository(ShopContext context,
            ILoggerFactory loggerFactory)
            : base(context, loggerFactory)
        {
        }
    }
}
