namespace Sunrise.Utility;

public abstract class PluginModule
{
    protected virtual List<PluginModule> SubModules { get; } = [];

    public bool IsEnabled { get; private set; }

    public void Enable()
    {
        if (IsEnabled)
            return;

        OnEnabled();
        IsEnabled = true;

        EventManager.RegisterEvents<ServerEvents>(this);

        foreach (PluginModule module in SubModules)
            module.Enable();
    }

    public void Disable()
    {
        if (!IsEnabled)
            return;

        OnDisabled();
        IsEnabled = false;

        EventManager.UnregisterEvents<ServerEvents>(this);

        foreach (PluginModule module in SubModules)
            module.Disable();
    }

    protected virtual void OnEnabled() { }
    protected virtual void OnDisabled() { }
    
    [PluginAPI.Events.PluginEvent(PluginAPI.Enums.ServerEventType.WaitingForPlayers)]
    protected virtual void OnReset() { }
}