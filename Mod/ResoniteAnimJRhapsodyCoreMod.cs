using HarmonyLib;
using FrooxEngine;
using ResoniteModLoader;

namespace Jworkz.ResoniteAnimJRhapsody.Core;

public class ResoniteAnimJRhapsodyCoreMod : ResoniteMod
{
  public override string Name => BuildInfo.Name;
  public override string Author => BuildInfo.Author;
  public override string Version => BuildInfo.Version;
  public override string Link => BuildInfo.Link;

  [AutoRegisterConfigKey]
  private static readonly ModConfigurationKey<bool> KEY_ENABLE =
    new ModConfigurationKey<bool>("enabled", $"Enables the {BuildInfo.Name} mod", () => true);

  private static ModConfiguration _config;
      
  private Harmony _harmony;

  public bool IsEnabled { get; private set; }


  /// <summary>
  /// Defines the metadata for the mod and other mod configurations.
  /// </summary>
  /// <param name="builder">The mod configuration definition builder responsible for building and adding details about this mod.</param>
  public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder) =>
    builder.Version(Version).AutoSave(true);

  public override void OnEngineInit()
  {
    _harmony = new Harmony(BuildInfo.ModId);
    _config = GetConfiguration();
    _config.OnThisConfigurationChanged += OnConfigurationChanged;

    Engine.Current.OnReady += OnCurrentEngineReady;
  }

  private void RefreshMod()
  {
    var isEnabledInConfig = _config.GetValue(KEY_ENABLE);

    if (IsEnabled != isEnabledInConfig)
    {
      IsEnabled = isEnabledInConfig;

      if (IsEnabled) { TurnModOn(); }
      else { TurnModOff(); }
    }
  }

  private void TurnModOn()
  {
    _harmony.PatchAll();
  }

  private void TurnModOff()
  {
    _harmony.UnpatchAll(_harmony.Id);
  }

  private void OnConfigurationChanged(ConfigurationChangedEvent @event) => RefreshMod();

  private void OnCurrentEngineReady() => RefreshMod();
}
