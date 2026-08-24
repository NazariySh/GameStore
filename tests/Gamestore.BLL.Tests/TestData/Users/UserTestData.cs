using Gamestore.BLL.DTOs.Users;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Tests.TestData.Users;

public static class UserTestData
{
    public static List<User> GetUsers()
    {
        return
        [
            new User
            {
                Id = Guid.Parse("c0d3a1b4-9d2f-4a76-8d65-72a4c3e51a11"),
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                UserRoles =
                [
                    new UserRole
                    {
                        RoleId = Guid.Parse("e2a4c9f1-53b7-4b1a-8a72-5f3e7adc9123"),
                    },
                    new UserRole
                    {
                        RoleId = Guid.Parse("4c7b2e65-9f32-46c5-b182-7a4d9e2c45f6"),
                    },
                    new UserRole
                    {
                        RoleId = Guid.Parse("12d8a7b9-65de-4f4c-93a1-8b1f27e3c678"),
                    },
                    new UserRole
                    {
                        RoleId = Guid.Parse("7f9c3e24-b6a1-43d7-a8c5-2e1d3b49f5a9"),
                    },
                ],
            },
            new User
            {
                Id = Guid.Parse("d1f4b6c7-8a3e-42c7-bf90-123a45c6de22"),
                UserName = "manager",
                NormalizedUserName = "MANAGER",
                PasswordHash = "AQAAAAEA",
                UserRoles =
                [
                    new UserRole
                    {
                        RoleId = Guid.Parse("4c7b2e65-9f32-46c5-b182-7a4d9e2c45f6"),
                    },
                    new UserRole
                    {
                        RoleId = Guid.Parse("12d8a7b9-65de-4f4c-93a1-8b1f27e3c678"),
                    },
                    new UserRole
                    {
                        RoleId = Guid.Parse("7f9c3e24-b6a1-43d7-a8c5-2e1d3b49f5a9"),
                    },
                ],
            },
            new User
            {
                Id = Guid.Parse("e2a5c7d8-6f4a-49c2-9d23-98b5d7f4cd33"),
                UserName = "moderator",
                NormalizedUserName = "MODERATOR",
                PasswordHash = "AQAAAAEA",
                UserRoles =
                [
                    new UserRole
                    {
                        RoleId = Guid.Parse("12d8a7b9-65de-4f4c-93a1-8b1f27e3c678"),
                    },
                    new UserRole
                    {
                        RoleId = Guid.Parse("7f9c3e24-b6a1-43d7-a8c5-2e1d3b49f5a9"),
                    },
                ],
            },
            new User
            {
                Id = Guid.Parse("f3b6d8e9-1c2b-48d9-a23d-12c6a78de444"),
                UserName = "user1",
                NormalizedUserName = "USER1",
                PasswordHash = "AQAAAAEA",
                UserRoles =
                [
                    new UserRole
                    {
                        RoleId = Guid.Parse("7f9c3e24-b6a1-43d7-a8c5-2e1d3b49f5a9"),
                    },
                ],
            },
            new User
            {
                Id = Guid.Parse("a4c7e9f1-2d3c-4f0a-b34e-45d7a89ef555"),
                UserName = "user2",
                NormalizedUserName = "USER2",
                PasswordHash = "AQAAAAEA",
                UserRoles =
                [
                    new UserRole
                    {
                        RoleId = Guid.Parse("7f9c3e24-b6a1-43d7-a8c5-2e1d3b49f5a9"),
                    },
                ],
            },
        ];
    }

    public static User GetUser()
    {
        return GetUsers()[0];
    }

    public static CreateUserRequest GetCreateUserRequest()
    {
        return new CreateUserRequest
        {
            User = new UserCreateDto
            {
                Name = "testuser",
            },
            Password = "Test@1234",
            Roles = new List<Guid>
            {
                Guid.Parse("7f9c3e24-b6a1-43d7-a8c5-2e1d3b49f5a9"),
            },
        };
    }

    public static CreateUserRequest GetInvalidCreateUserRequest()
    {
        return new CreateUserRequest
        {
            User = new UserCreateDto
            {
                Name = string.Empty,
            },
            Password = string.Empty,
            Roles = new List<Guid>(),
        };
    }

    public static UpdateUserRequest GetUpdateUserRequest(Guid? userId = null)
    {
        return new UpdateUserRequest
        {
            User = new UserUpdateDto
            {
                Id = userId ?? Guid.NewGuid(),
                Name = "updateduser",
            },
            Password = "Updated@1234",
            Roles = new List<Guid>
            {
                Guid.Parse("7f9c3e24-b6a1-43d7-a8c5-2e1d3b49f5a9"),
                Guid.Parse("e2a4c9f1-53b7-4b1a-8a72-5f3e7adc9123"),
            },
        };
    }

    public static UpdateUserRequest GetInvalidUpdateUserRequest(Guid? userId = null)
    {
        return new UpdateUserRequest
        {
            User = new UserUpdateDto
            {
                Id = userId ?? Guid.NewGuid(),
                Name = string.Empty,
            },
            Password = string.Empty,
            Roles = new List<Guid>(),
        };
    }
}