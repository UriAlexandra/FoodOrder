using System.Net;
using System.Security.Claims;
using FoodOrder.Shared.Realtime;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace FoodOrder.Client.Services;

public enum RealtimeState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}

/// <summary>
/// Valós idejű rendelésértesítések a szerver /hubs/orders hubjáról. Bejelentkezéskor kapcsolódik (a hitelesítő
/// sütit a böngésző küldi), kijelentkezéskor bont. Megszakadáskor magától újracsatlakozik; utána a Reconnected
/// esemény jelzi az oldalaknak, hogy töltsék újra az adataikat, így a kimaradt események sem vesznek el.
/// </summary>
public class OrderUpdatesClient : IAsyncDisposable
{
    private static readonly TimeSpan StartRetryDelay = TimeSpan.FromSeconds(5);

    private readonly NavigationManager _navigation;
    private readonly AuthenticationStateProvider _auth;
    private readonly ILogger<OrderUpdatesClient> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private HubConnection? _connection;
    private CancellationTokenSource? _startCts;
    private string? _connectedUser;
    private bool _initialized;

    public OrderUpdatesClient(NavigationManager navigation, AuthenticationStateProvider auth, ILogger<OrderUpdatesClient> logger)
    {
        _navigation = navigation;
        _auth = auth;
        _logger = logger;
        _auth.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public RealtimeState State { get; private set; } = RealtimeState.Disconnected;

    /// <summary>Egy rendelés megváltozott (azonosító + verzió; a részleteket az API-ból kell lekérni).</summary>
    public event Func<OrderChangedEvent, Task>? OrderChanged;

    /// <summary>A kapcsolat megszakadás után helyreállt: a megjelenített adatokat újra kell tölteni.</summary>
    public event Func<Task>? Reconnected;

    public event Action? StateChanged;

    /// <summary>Az alkalmazás indulásakor hívjuk: ha van bejelentkezett felhasználó, kapcsolódik.</summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        var state = await _auth.GetAuthenticationStateAsync();
        await ApplyUserAsync(state.User);
    }

    /// <summary>A hubkapcsolat felépítése. A tesztkörnyezet felülírhatja (pl. más átvitel).</summary>
    protected virtual HubConnection CreateConnection() =>
        new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri(OrderHubContract.Path.TrimStart('/')))
            .WithAutomaticReconnect(new KeepTryingRetryPolicy())
            .Build();

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> stateTask)
    {
        try
        {
            await ApplyUserAsync((await stateTask).User);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "A valós idejű kapcsolat be- vagy kijelentkezéskori frissítése sikertelen.");
        }
    }

    private async Task ApplyUserAsync(ClaimsPrincipal user)
    {
        var email = user.Identity?.IsAuthenticated == true ? user.FindFirst(ClaimTypes.Email)?.Value : null;

        await _gate.WaitAsync();
        try
        {
            // Ugyanaz a felhasználó: a meglévő kapcsolat marad. Másik felhasználó: új kapcsolat, mert a
            // csoporttagságot a szerver csatlakozáskor, a süti alapján dönti el.
            if (email is not null && email == _connectedUser && _connection is not null)
                return;

            await StopCoreAsync();
            if (email is not null)
                StartCore(email);
        }
        finally
        {
            _gate.Release();
        }
    }

    private void StartCore(string user)
    {
        var connection = CreateConnection();
        connection.On<OrderChangedEvent>(OrderHubContract.OrderChanged, RaiseOrderChangedAsync);
        connection.Reconnecting += _ =>
        {
            SetState(RealtimeState.Reconnecting);
            return Task.CompletedTask;
        };
        connection.Reconnected += async _ =>
        {
            SetState(RealtimeState.Connected);
            await RaiseReconnectedAsync();
        };
        connection.Closed += _ =>
        {
            if (ReferenceEquals(connection, _connection))
                SetState(RealtimeState.Disconnected);
            return Task.CompletedTask;
        };

        _connection = connection;
        _connectedUser = user;
        _startCts = new CancellationTokenSource();
        _ = StartWithRetryAsync(connection, _startCts.Token);
    }

    // Az első kapcsolódás is sikertelen lehet (pl. a szerver épp újraindul): 5 másodpercenként újrapróbáljuk.
    private async Task StartWithRetryAsync(HubConnection connection, CancellationToken ct)
    {
        var failedBefore = false;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                SetState(failedBefore ? RealtimeState.Reconnecting : RealtimeState.Connecting);
                await connection.StartAsync(ct);
                SetState(RealtimeState.Connected);
                if (failedBefore)
                    await RaiseReconnectedAsync();
                return;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Nincs érvényes bejelentkezés: nincs értelme újrapróbálni, az API-hívás a bejelentkezésre visz.
                SetState(RealtimeState.Disconnected);
                return;
            }
            catch (Exception ex)
            {
                failedBefore = true;
                SetState(RealtimeState.Reconnecting);
                _logger.LogWarning(ex, "A valós idejű kapcsolat nem jött létre, {Delay} múlva újrapróbálom.", StartRetryDelay);
                try
                {
                    await Task.Delay(StartRetryDelay, ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private async Task StopCoreAsync()
    {
        _startCts?.Cancel();
        _startCts?.Dispose();
        _startCts = null;

        var connection = _connection;
        _connection = null;
        _connectedUser = null;

        if (connection is not null)
        {
            try
            {
                await connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "A valós idejű kapcsolat bontása közben hiba történt.");
            }
        }

        SetState(RealtimeState.Disconnected);
    }

    private async Task RaiseOrderChangedAsync(OrderChangedEvent change)
    {
        if (OrderChanged is not { } handlers) return;

        foreach (var handler in handlers.GetInvocationList().Cast<Func<OrderChangedEvent, Task>>())
        {
            try
            {
                await handler(change);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "A(z) #{OrderId} rendelés változásának feldolgozása sikertelen.", change.OrderId);
            }
        }
    }

    private async Task RaiseReconnectedAsync()
    {
        if (Reconnected is not { } handlers) return;

        foreach (var handler in handlers.GetInvocationList().Cast<Func<Task>>())
        {
            try
            {
                await handler();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Az adatok újratöltése az újracsatlakozás után sikertelen.");
            }
        }
    }

    private void SetState(RealtimeState state)
    {
        if (State == state) return;
        State = state;
        StateChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        _auth.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        await StopCoreAsync();
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }
}
