using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Application;

namespace Pedidos.Api.Controllers;

[ApiController, Route("api/pedidos"), Authorize]
public sealed class PedidosController(PedidoService service) : ControllerBase
{
    [HttpGet]
    public Task<Page<PedidoDto>> List(CancellationToken cancellationToken, [FromQuery] string? search = null,
        [FromQuery] string? estado = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => service.ListAsync(search, estado, page, pageSize, cancellationToken);

    [HttpGet("{id:int}")]
    public Task<PedidoDto> Get(int id, CancellationToken cancellationToken) => service.GetAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<PedidoDto>> Create(PedidoInput input, CancellationToken cancellationToken)
    {
        var pedido = await service.CreateAsync(input, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id:int}")]
    public Task<PedidoDto> Update(int id, PedidoInput input, CancellationToken cancellationToken)
        => service.UpdateAsync(id, input, cancellationToken);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}