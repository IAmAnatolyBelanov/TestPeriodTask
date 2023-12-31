using System.Net.Http.Json;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using FluentAssertions.Execution;
using Monitoring.Api.Infrastructure;
using Monitoring.Contracts.DeviceInfo;
using Monitoring.Dal.Models;

namespace Monitoring.IntegrationTests.Devices;

/// <summary>
/// Тесты для проверки работы с <see cref="DeviceInfo"/>.
/// </summary>
public class DevicesTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    /// <summary>
    /// Конструктор класса <see cref="DevicesTests"/>.
    /// </summary>
    /// <param name="appFactory"><see cref="AppFactory"/>.</param>
    public DevicesTests(AppFactory appFactory) => _factory = appFactory;

    /// <summary>
    /// Должен успешно зарегистрировать девайс.
    /// </summary>
    /// <param name="device">Девайс.</param>
    /// <returns><see cref="Task"/>.</returns>
    [Theory]
    [AutoData]
    public async Task RegisterDevice_PossibleDevice_Success(DeviceInfoDto device)
    {
        var client = _factory.CreateClient();

        var (httpResponse, result) = await RegisterDevice(client, device);

        using (new AssertionScope())
        {
            httpResponse.Should().BeSuccessful();

            result.Error.Should().BeNull();
            result.Data.Should().BeNull();
        }
    }

    /// <summary>
    /// Должен успешно получить информацию о зарегистрированном девайсе.
    /// </summary>
    /// <param name="device">Девайс.</param>
    /// <returns><see cref="Task"/>.</returns>
    [Theory]
    [AutoData]
    public async Task GetDevice_RegisteredDevice_Success(DeviceInfoDto device)
    {
        var client = _factory.CreateClient();

        await RegisterDevice(client, device);

        var result = await GetDevice(client, device.Id);

        using (new AssertionScope())
        {
            result.Error.Should().BeNull();
            result.Data.Should().NotBeNull();
        }

        using (new AssertionScope())
        {
            result.Data.Should().BeEquivalentTo(device, options => options.Excluding(x => x.LastUpdate));
            result.Data.LastUpdate.ToUnixTimeSeconds().Should().Be(device.LastUpdate.ToUnixTimeSeconds());
        }
    }

    /// <summary>
    /// Должен успешно обновить информацию о зарегистрированном девайсе.
    /// </summary>
    /// <param name="deviceId">Id девайса.</param>
    /// <param name="firstVariantDevice">Первичная информация о девайсе.</param>
    /// <param name="secondVariantDevice">Обновлённая информация о девайсе.</param>
    /// <returns><see cref="Task"/>.</returns>
    [Theory]
    [AutoData]
    public async Task RegisterDevice_UpdateExistingDevice_Success(Guid deviceId, DeviceInfoDto firstVariantDevice, DeviceInfoDto secondVariantDevice)
    {
        firstVariantDevice.Id = deviceId;
        secondVariantDevice.Id = deviceId;

        var client = _factory.CreateClient();

        await RegisterDevice(client, firstVariantDevice);
        var firstResult = await GetDevice(client, firstVariantDevice.Id);

        await RegisterDevice(client, secondVariantDevice);
        var secondResult = await GetDevice(client, secondVariantDevice.Id);

        using (new AssertionScope())
        {
            firstResult.Error.Should().BeNull();
            firstResult.Data.Should().NotBeNull();
            secondResult.Error.Should().BeNull();
            secondResult.Data.Should().NotBeNull();
        }

        using (new AssertionScope())
        {
            firstResult.Data.Should().BeEquivalentTo(firstVariantDevice, options => options.Excluding(x => x.LastUpdate));
            firstResult.Data.LastUpdate.ToUnixTimeSeconds().Should().Be(firstVariantDevice.LastUpdate.ToUnixTimeSeconds());
        }

        using (new AssertionScope())
        {
            secondResult.Data.Should().BeEquivalentTo(secondVariantDevice, options => options.Excluding(x => x.LastUpdate));
            secondResult.Data.LastUpdate.ToUnixTimeSeconds().Should().Be(secondVariantDevice.LastUpdate.ToUnixTimeSeconds());
        }
    }

    /// <summary>
    /// Должен получить пустой ответ при запросе не зарегистрированного девайса.
    /// </summary>
    /// <param name="deviceId">Id несуществующего девайса.</param>
    /// <returns><see cref="Task"/>.</returns>
    [Theory]
    [AutoData]
    public async Task GetDevice_NotRegistered_Success(Guid deviceId)
    {
        var client = _factory.CreateClient();

        var result = await GetDevice(client, deviceId);

        using (new AssertionScope())
        {
            result.Error.Should().BeNull();
            result.Data.Should().BeNull();
        }
    }

    private async Task<(HttpResponseMessage HttpResponse, BaseResponse<object> BaseResponse)> RegisterDevice(HttpClient client, DeviceInfoDto device)
    {
        using var responseMessage = await client.PostAsJsonAsync("/api/devices/register-device", device);
        var responseContent = await responseMessage.Content.ReadAsStringAsync();

        var response = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseResponse<object>>(responseContent)!;

        return (responseMessage, response);
    }

    private async Task<BaseResponse<DeviceInfoDto>> GetDevice(HttpClient client, Guid deviceId)
    {
        using var responseMessage = await client.GetAsync($"/api/devices/{deviceId}");
        var responseContent = await responseMessage.Content.ReadAsStringAsync();

        var response = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseResponse<DeviceInfoDto>>(responseContent)!;

        return response;
    }
}
