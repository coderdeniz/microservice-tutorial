using FreeCourse.Services.Order.Core;

namespace FreeCourse.Services.Order.Domain.OrderAggregate
{
    // ef core features
    // owned types
    // shadow property
    // backing field
    public class Order : Entity, IAggregateRoot
    {
        public DateTime CreatedDate { get; private set; }
        
        // owned entity types
        // default olarak adres için ayrı tablo oluşturmuyor.
        // başka aggregate'lar value object'leri kullanabilir
        public Address Address { get; private set; }

        public string BuyerId { get; private set; }

        // başka aggregate kullanamaz
        // backing field, efCore burayı doldur diyoruz dış dünyaya sadece okuma olarak aç
        private readonly List<OrderItem> _orderItems;

        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        public Order()
        {}

        public Order(Address address, string buyerId)
        {
            _orderItems= new List<OrderItem>();
            CreatedDate = DateTime.Now;
            Address = address;
            BuyerId = buyerId;
        }

        public void AddOrderItem(string productID, string productName, decimal price, string pictureUrl)
        {
            var existProduct = _orderItems.Any(x => x.ProductId == productID);

            if (!existProduct)
            {
                var newOrderItem = new OrderItem(productID, productName, pictureUrl, price);
                _orderItems.Add(newOrderItem);
            }           
        }

        public decimal GetTotalPrice => _orderItems.Sum(x => x.Price);  

    }
}
