using System.Globalization;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Enums;

namespace Gamestore.BLL.Tests.TestData.Orders;

public static class OrderTestData
{
    public static Guid UserId => Guid.Parse("123e4567-e89b-12d3-a456-426614174000");

    public static List<Order> GetOrders()
    {
        return
        [
            new Order
            {
                Id = Guid.Parse("003234dd-8a58-4e6f-8b0a-a6cc34c9a042"),
                Date = DateTime.Parse("2024-10-15T09:15:00Z", CultureInfo.InvariantCulture),
                CustomerId = UserId,
                Status = OrderStatus.Cancelled,
                OrderGames =
                [
                    new OrderGame
                    {
                        ProductId = Guid.Parse("39fc2c6d-f7dc-4cd7-85a0-79b3f62ff88e"),
                        Price = 49.99m,
                        Quantity = 1,
                        Discount = 0,
                    },
                    new OrderGame
                    {
                        ProductId = Guid.Parse("4f1c650a-883b-4b47-a989-9d3e8de54233"),
                        Price = 34.99m,
                        Quantity = 1,
                        Discount = 10,
                    },
                ],
            },
            new Order
            {
                Id = Guid.Parse("275c9bae-177e-4df9-9e97-883f1c5c23e7"),
                Date = DateTime.Parse("2023-09-30T14:50:00Z", CultureInfo.InvariantCulture),
                CustomerId = UserId,
                Status = OrderStatus.Paid,
                OrderGames =
                [
                    new OrderGame
                    {
                        ProductId = Guid.Parse("53aa902d-111c-42c4-8136-65208d3b30d6"),
                        Price = 19.99m,
                        Quantity = 3,
                        Discount = 0,
                    },
                ],
            },
            new Order
            {
                Id = Guid.Parse("640511b0-b13c-4c2d-91d5-eb779e762cc1"),
                Date = DateTime.Parse("2023-08-05T18:30:00Z", CultureInfo.InvariantCulture),
                CustomerId = UserId,
                Status = OrderStatus.Cancelled,
                OrderGames =
                [
                    new OrderGame
                    {
                        ProductId = Guid.Parse("6158b497-e7a4-47e5-9c50-3a5d2a2c7b61"),
                        Price = 44.99m,
                        Quantity = 2,
                        Discount = 20,
                    },
                    new OrderGame
                    {
                        ProductId = Guid.Parse("725d84fa-51a7-44a5-9466-8cfad1b56de4"),
                        Price = 54.99m,
                        Quantity = 1,
                        Discount = 5,
                    },
                    new OrderGame
                    {
                        ProductId = Guid.Parse("1f3b9c7b-5c99-4bcb-bb89-1a9b7d190005"),
                        Price = 31.99m,
                        Quantity = 2,
                        Discount = 8,
                    },
                ],
            },
            new Order
            {
                Id = Guid.Parse("af4ef139-bab4-444d-8c8f-54316b586bc3"),
                Date = DateTime.Parse("2023-11-02T12:00:00Z", CultureInfo.InvariantCulture),
                CustomerId = UserId,
                Status = OrderStatus.Paid,
                OrderGames =
                [
                    new OrderGame
                    {
                        ProductId = Guid.Parse("84bc2cd6-6c29-4b4a-b8cb-7b20137aa1c9"),
                        Price = 42.99m,
                        Quantity = 1,
                        Discount = 12,
                    },
                    new OrderGame
                    {
                        ProductId = Guid.Parse("9ed3a711-0e27-434b-bc87-3a48a93d276c"),
                        Price = 27.99m,
                        Quantity = 2,
                        Discount = 0,
                    },
                ],
            },
            new Order
            {
                Id = Guid.Parse("ece87865-dc1c-4be8-8170-b2dfc473e866"),
                Date = DateTime.Parse("2023-10-01T12:25:00Z", CultureInfo.InvariantCulture),
                CustomerId = UserId,
                Status = OrderStatus.Paid,
                OrderGames =
                [
                    new OrderGame
                    {
                        ProductId = Guid.Parse("1f3b9c7b-5c99-4bcb-bb89-1a9b7d190002"),
                        Price = 69.99m,
                        Quantity = 1,
                        Discount = 5,
                    },
                ],
            },
            new Order
            {
                Id = Guid.Parse("f4e5dd23-dbe6-4eca-9119-f7a1f30bd7ab"),
                Date = DateTime.Parse("2023-09-28T17:15:00Z", CultureInfo.InvariantCulture),
                CustomerId = UserId,
                Status = OrderStatus.Paid,
                OrderGames =
                [
                    new OrderGame
                    {
                        ProductId = Guid.Parse("1f3b9c7b-5c99-4bcb-bb89-1a9b7d190003"),
                        Price = 49.99m,
                        Quantity = 1,
                        Discount = 0,
                    },
                    new OrderGame
                    {
                        ProductId = Guid.Parse("aab8570a-037e-4988-b139-b63f5d5f5e7e"),
                        Price = 31.99m,
                        Quantity = 1,
                        Discount = 8,
                    },
                ],
            },
            new Order
            {
                Id = Guid.Parse("c4c39ca5-7b26-45d9-969d-368a25d28b79"),
                Date = DateTime.Parse("2023-08-19T08:45:00Z", CultureInfo.InvariantCulture),
                CustomerId = UserId,
                Status = OrderStatus.Paid,
                OrderGames =
                [
                    new OrderGame
                    {
                        ProductId = Guid.Parse("39fc2c6d-f7dc-4cd7-85a0-79b3f62ff88e"),
                        Price = 49.99m,
                        Quantity = 4,
                        Discount = 0,
                    },
                ],
            },
            new Order
            {
                Id = Guid.Parse("b1433c7f-fb8c-4e82-92d3-8f3dc176aa9a"),
                Date = DateTime.Parse("2025-8-15T09:15:00Z", CultureInfo.InvariantCulture),
                CustomerId = UserId,
                Status = OrderStatus.Open,
                OrderGames =
                [
                    new OrderGame
                    {
                        ProductId = Guid.Parse("11b07e1f-1cf2-4c0d-b0a2-d46fd3d74a11"),
                        Price = 39.99m,
                        Quantity = 1,
                        Discount = 5,
                    },
                    new OrderGame
                    {
                        ProductId = Guid.Parse("22954e2e-6659-4662-8d7c-9ac04f3a0ea6"),
                        Price = 29.99m,
                        Quantity = 2,
                        Discount = 15,
                    },
                ],
            },
        ];
    }

    public static List<Order> GetPaidAndCancelledOrders()
    {
        return GetOrders()
            .Where(o => o.Status is OrderStatus.Paid or OrderStatus.Cancelled)
            .ToList();
    }

    public static List<Order> GetOrdersWithGame(Guid productId)
    {
        return GetOrders()
            .Where(o => o.OrderGames.Any(og => og.ProductId == productId))
            .ToList();
    }

    public static Order GetOrder(OrderStatus? status = null)
    {
        return new Order
        {
            Id = Guid.Parse("003234dd-8a58-4e6f-8b0a-a6cc34c9a042"),
            MongoOrderId = 1,
            Date = DateTime.Parse("2024-10-15T09:15:00Z", CultureInfo.InvariantCulture),
            CustomerId = UserId,
            Status = status ?? OrderStatus.Cancelled,
            OrderGames =
            [
                new OrderGame
                {
                    OrderId = Guid.Parse("003234dd-8a58-4e6f-8b0a-a6cc34c9a042"),
                    MongoOrderId = 1,
                    ProductId = Guid.Parse("39fc2c6d-f7dc-4cd7-85a0-79b3f62ff88e"),
                    Price = 49.99m,
                    Quantity = 1,
                    Discount = 0,
                },
                new OrderGame
                {
                    OrderId = Guid.Parse("003234dd-8a58-4e6f-8b0a-a6cc34c9a042"),
                    MongoOrderId = 1,
                    ProductId = Guid.Parse("4f1c650a-883b-4b47-a989-9d3e8de54233"),
                    Price = 34.99m,
                    Quantity = 1,
                    Discount = 10,
                },
            ],
        };
    }

    public static OrderGame GetOrderGame(Guid? orderId = null)
    {
        return new OrderGame
        {
            OrderId = orderId ?? Guid.NewGuid(),
            ProductId = Guid.Parse("1f3b9c7b-5c99-4bcb-bb89-1a9b7d190005"),
            Price = 31.99m,
            Quantity = 3,
            Discount = 8,
        };
    }

    public static OrderGame GetOrderGame(Game game)
    {
        return new OrderGame
        {
            OrderId = Guid.NewGuid(),
            ProductId = game.Id,
            Price = game.Price,
            Quantity = 1,
            Discount = game.Discount,
        };
    }

    public static Order GetEmptyOrder()
    {
        return new Order
        {
            Id = Guid.Parse("003234dd-8a58-4e6f-8b0a-a6cc34c9a042"),
            MongoOrderId = 2,
            Date = DateTime.Parse("2024-10-15T09:15:00Z", CultureInfo.InvariantCulture),
            CustomerId = UserId,
            Status = OrderStatus.Checkout,
            OrderGames = [],
        };
    }

    public static Order GetEmptyCart(Guid? customerId = null)
    {
        return new Order
        {
            Id = Guid.Parse("b1433c7f-fb8c-4e82-92d3-8f3dc176aa9a"),
            Date = DateTime.Parse("2025-10-15T09:15:00Z", CultureInfo.InvariantCulture),
            CustomerId = customerId ?? UserId,
            Status = OrderStatus.Open,
            OrderGames = [],
        };
    }

    public static Order GetCart(Guid? customerId = null)
    {
        return new Order
        {
            Id = Guid.Parse("b1433c7f-fb8c-4e82-92d3-8f3dc176aa9a"),
            Date = DateTime.Parse("2025-10-15T09:15:00Z", CultureInfo.InvariantCulture),
            CustomerId = customerId ?? UserId,
            Status = OrderStatus.Open,
            OrderGames =
            [
                new OrderGame
                {
                    ProductId = Guid.Parse("11b07e1f-1cf2-4c0d-b0a2-d46fd3d74a11"),
                    Price = 39.99m,
                    Quantity = 1,
                    Discount = 5,
                },
                new OrderGame
                {
                    ProductId = Guid.Parse("22954e2e-6659-4662-8d7c-9ac04f3a0ea6"),
                    Price = 29.99m,
                    Quantity = 2,
                    Discount = 15,
                },
            ],
        };
    }
}