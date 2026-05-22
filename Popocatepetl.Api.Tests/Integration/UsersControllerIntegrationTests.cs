using System.Net;
using System.Net.Http.Json;
using Popocatepetl.Api.Controllers;
using Popocatepetl.Api.Dtos;
using Popocatepetl.Api.Tests.TestUtilities;

namespace Popocatepetl.Api.Tests.Integration;

public sealed class UsersControllerIntegrationTests
{
    [Fact]
    public async Task UserCrudFlow_PersistsChangesFromControllerToDatabase()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest("alice@example.com"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        created.Should().NotBeNull();
        created!.Email.Should().Be("alice@example.com");
        created.CreatedAt.Should().NotBe(default);

        var getCreated = await client.GetFromJsonAsync<UserResponse>($"/api/users/{created.Id}");
        getCreated.Should().NotBeNull();
        getCreated!.Email.Should().Be("alice@example.com");
        getCreated.CreatedAt.Should().Be(created.CreatedAt);

        var users = await client.GetFromJsonAsync<List<UserResponse>>("/api/users");
        users.Should().NotBeNull();
        users!.Should().ContainSingle(u => u.Id == created.Id && u.Email == "alice@example.com");

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/users/{created.Id}",
            new UpdateUserRequest("alice+updated@example.com"));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updated = await client.GetFromJsonAsync<UserResponse>($"/api/users/{created.Id}");
        updated.Should().NotBeNull();
        updated!.Email.Should().Be("alice+updated@example.com");
        updated.CreatedAt.Should().Be(created.CreatedAt);

        var deleteResponse = await client.DeleteAsync($"/api/users/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDelete = await client.GetAsync($"/api/users/{created.Id}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
