using Microsoft.AspNetCore.Http.HttpResults;
using PasswordGenerator.Api.Application;

namespace PasswordGenerator.Api.Endpoints;

public static class PasswordEndpoints
{
    public static IEndpointRouteBuilder MapPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/passwords").WithTags("Passwords");

        group.MapPost("/", CreateAsync)
            .WithName("CreatePassword")
            .WithSummary("Gera uma senha forte")
            .WithDescription("Gera uma senha com 16 a 128 caracteres (padrão 16), contendo maiúscula, minúscula, dígito e caractere especial, sem espaços. A senha é armazenada e o GUID retornado serve para consultá-la.")
            .ProducesValidationProblem();

        group.MapGet("/{id}", GetAsync)
            .WithName("GetPassword")
            .WithSummary("Consulta uma senha pelo GUID")
            .WithDescription("Retorna a senha associada ao GUID. Responde 400 se o id não for um GUID válido e 404 se não existir.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Created<CreatePasswordResponse>> CreateAsync(
        CreatePasswordRequest? request, PasswordService service, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request?.Length, cancellationToken);

        return TypedResults.Created($"/passwords/{created.Id}", created);
    }

    private static async Task<Results<Ok<PasswordResponse>, ProblemHttpResult>> GetAsync(
        string id, PasswordService service, CancellationToken cancellationToken)
    {
        var password = await service.GetAsync(id, cancellationToken);

        return password is null
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Senha não encontrada.")
            : TypedResults.Ok(password);
    }
}
