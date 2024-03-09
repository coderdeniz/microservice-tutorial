using FreeCourse.Services.Order.Core;

namespace FreeCourse.Services.Order.Domain.OrderAggregate
{
    /*
        OrderItem kendi başına eklenmesin diye OrderId(diğer sınıftan tutan değer) eklemedik
        
     */
    public class OrderItem : Entity
    {
        public string ProductId { get; private set; }
        public string ProductName { get; private set; }
        public string PictureUrl { get; private set; }
        public decimal Price { get; private set; }

        public OrderItem()
        {}

        public OrderItem(string productId, string productName, string pictureUrl, decimal price)
        {
            ProductId = productId;
            ProductName = productName;
            PictureUrl = pictureUrl;
            Price = price;
        }

        public void UpdateOrderItem(string ProductName, string PictureUrl, decimal Price)
        {
            this.ProductName = ProductName;
            this.PictureUrl= PictureUrl;
            this.Price = Price;            
        }
    }
}
