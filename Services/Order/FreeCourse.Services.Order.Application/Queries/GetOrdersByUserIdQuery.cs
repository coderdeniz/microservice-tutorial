﻿using FreeCourse.Services.Order.Application.Dtos;
using FreeCourse.Shared.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeCourse.Services.Order.Application.Queries
{
    // mediatr bu sınıfı handle edecek sınıfı kendisi bulacak
    public class GetOrdersByUserIdQuery : IRequest<Response<List<OrderDto>>>
    {
        public string UserId { get; set; }
    }
}
