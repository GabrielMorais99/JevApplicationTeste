using JevApplicationTeste.Api.Models;
using JevApplicationTeste.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JevApplicationTeste.Api.Controllers;

[ApiController]
[Route("chamados")]
public sealed class ChamadosController(ClassificacaoService service) : ControllerBase
{
    [HttpPost("classificar")]
    public async Task<ActionResult<ClassificarResponse>> Classificar(ClassificarRequest request, CancellationToken ct)
    {
        try { return Ok(await service.ClassificarAsync(request.Descricao, ct)); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { return Problem(statusCode: 504, title: "Tempo limite ao consultar o Jev."); }
        catch (JevIntegrationException ex)
        { return Problem(statusCode: 502, title: ex.Message); }
        catch (HttpRequestException)
        { return Problem(statusCode: 502, title: "Não foi possível conectar ao Jev."); }
    }
}
