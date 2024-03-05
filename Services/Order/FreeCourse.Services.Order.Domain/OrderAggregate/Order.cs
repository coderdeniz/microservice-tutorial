using FreeCourse.Services.Order.Core;

namespace FreeCourse.Services.Order.Domain.OrderAggregate
{
    public class Order : Entity, IAggregateRoot
    {
        public DateTime CreatedDate { get; set; }
        
        // owned entity types
        public Address Address { get; set; }

        public string BuyerId { get; set; }

    }
}
