using Microsoft.AspNetCore.Mvc;
using Monitoring.Api.Infrastructure;
using Monitoring.Contracts.DeviceEvents;
using Monitoring.Domain.DeviceEventServices;
using Monitoring.Domain.Mappers;
using Monitoring.Shared.GuidProviders;

namespace Monitoring.Api.Controllers;

/// <summary>
/// Контроллер для работы с событиями.
/// </summary>
[ApiController]
[Route("api/events")]
public class EventController
{
    private readonly IDeviceEventService _deviceEventService;
    private readonly IDeviceEventLightMapper _deviceEventLightMapper;
    private readonly IDeviceEventMapper _deviceEventMapper;
    private readonly IGuidProvider _guidProvider;

    /// <summary>
    /// Конструктор класса <see cref="EventController"/>.
    /// </summary>
    /// <param name="deviceEventService"><see cref="IDeviceEventService"/>.</param>
    /// <param name="deviceEventLightMapper"><see cref="IDeviceEventLightMapper"/>.</param>
    /// <param name="deviceEventMapper"><see cref="IDeviceEventMapper"/>.</param>
    /// <param name="guidProvider"><see cref="IGuidProvider"/>.</param>
    public EventController(
        IDeviceEventService deviceEventService,
        IDeviceEventLightMapper deviceEventLightMapper,
        IDeviceEventMapper deviceEventMapper,
        IGuidProvider guidProvider)
    {
        _deviceEventService = deviceEventService;
        _deviceEventLightMapper = deviceEventLightMapper;
        _deviceEventMapper = deviceEventMapper;
        _guidProvider = guidProvider;
    }

    /// <summary>
    /// Добавляет событие.
    /// </summary>
    /// <param name="deviceEvent">Событие.</param>
    /// <param name="cancellationToken">Токен для отмены запроса.</param>
    /// <returns>Id добавленного события.</returns>
    [HttpPut]
    public async Task<BaseResponse<Guid>> AddEvent(DeviceEventDto deviceEvent, CancellationToken cancellationToken)
    {
        var events = new[] { _deviceEventMapper.MapFromDto(deviceEvent) };
        events[0].Id = _guidProvider.NewGuid();
        await _deviceEventService.AddEvents(events, cancellationToken);
        return events[0].Id.ToResponse();
    }

    /// <summary>
    /// Добавляет события.
    /// </summary>
    /// <param name="deviceEvents">События.</param>
    /// <param name="cancellationToken">Токен для отмены запроса.</param>
    /// <returns>Пустой <see cref="BaseResponse{T}"/> в случае успешного добавления событий, либо ошибки в случае их возникновения.</returns>
    [HttpPut("bulk")]
    public async Task<BaseResponse<object>> AddEvents(IReadOnlyList<DeviceEventDto> deviceEvents, CancellationToken cancellationToken)
    {
        var events = deviceEvents.Select(_deviceEventMapper.MapFromDto).ToArray();

        for (var i = 0; i < events.Length; i++)
        {
            events[i].Id = _guidProvider.NewGuid();
        }

        await _deviceEventService.AddEvents(events, cancellationToken);
        return BaseResponseExtensions.EmptySuccess();
    }

    /// <summary>
    /// Возвращает список событий указанного девайса.
    /// </summary>
    /// <param name="deviceId">Id девайса.</param>
    /// <param name="cancellationToken">Токен для отмены запроса.</param>
    /// <returns>Список событий указанного девайса.</returns>
    /// <remarks>Если девайса не существует, список будет пустым.</remarks>
    [HttpGet("by-device")]
    public async Task<BaseResponse<EventCollectionDto>> GetEventsByDevice(Guid deviceId, CancellationToken cancellationToken)
    {
        var events = await _deviceEventService.GetEventsByDevice(deviceId, cancellationToken);
        var result = new EventCollectionDto
        {
            DeviceId = deviceId,
            Events = events.Select(_deviceEventLightMapper.MapToDto).ToArray(),
        };
        return result.ToResponse();
    }
}
