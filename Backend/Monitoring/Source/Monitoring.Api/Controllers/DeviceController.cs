using System.Data.SqlTypes;
using Infotecs.Monitoring.Api.Controllers;
using Infotecs.Monitoring.Bll.DeviceBizRules;
using Infotecs.Monitoring.Dal;
using Infotecs.Monitoring.Shared.Paginations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Infotecs.Monitoring.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DeviceController : ControllerBase
{
    private readonly ILogger<DeviceController> _logger;
    private readonly IDeviceBizRule _deviceBizRule;

    public DeviceController(ILogger<DeviceController> logger, IDeviceBizRule deviceBizRule)
    {
        _logger = logger;
        _deviceBizRule = deviceBizRule;
    }

    [HttpPost]
    public async ValueTask<BaseResponse<Guid>> RegisterDevice(DeviceInfo device, CancellationToken cancellationToken)
    {
        var result = await _deviceBizRule.RegisterDevice(device, cancellationToken);
        return result.ToResponse();
    }

    [HttpGet]
    public async ValueTask<BaseResponse<IReadOnlyList<DeviceInfo>>> GetAll(Pagination pagination, CancellationToken cancellationToken)
    {
        var result = await _deviceBizRule.GetAll(pagination, cancellationToken);
        return result.ToResponse();
    }

    [HttpGet]
    public async ValueTask<BaseResponse<DeviceStatistics>> GetFullStatistics(Guid deviceId, CancellationToken cancellationToken)
    {
        var result = await _deviceBizRule.GetFullStatistics(deviceId, cancellationToken);
        return result.ToResponse();
    }

    [HttpGet]
    public async ValueTask<BaseResponse<DeviceStatistics>> GetStatistics(Guid deviceId, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken cancellationToken)
    {
        var result = await _deviceBizRule.GetStatistics(deviceId, dateFrom, dateTo, cancellationToken);
        return result.ToResponse();
    }
}