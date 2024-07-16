using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Console;
using Content.Shared.Administration;
using Content.Server.Administration;
using Robust.Shared.Prototypes;
using System.Linq;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Audio.Systems;
using Content.Shared.Imperial.ErtCall;

namespace Content.Server.Imperial.ErtCall;

[AdminCommand(AdminFlags.Admin)]
public sealed class CallErt : LocalizedCommands
{
    public string Description => Loc.GetString("callertcommand-desc");
    public string Help => Loc.GetString("callertcommand-help");
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override string Command => "callert";

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var options = _prototype
                .EnumeratePrototypes<ErtCallPresetPrototype>()
                .Select(p => new CompletionOption(p.ID, p.Desc));

            return CompletionResult.FromHintOptions(options.OrderBy(x => x.Value, StringComparer.Ordinal).ToArray(), Loc.GetString("callertcommand-id-preset"));
        }

        return CompletionResult.Empty;
    }

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0) //123
        {
            shell.WriteError(Loc.GetString("callertcommand-error-args0"));
            _entity.System<SharedAudioSystem>().PlayGlobal("/Audio/Imperial/ErtCall/noert.ogg", Filter.Broadcast(), true, AudioParams.Default.WithVolume(-2f));
            return;
        }
        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("callertcommand-error-args1"));
            return;
        }
        var ertSpawnSystem = _entity.System<CallErtSystem>();
        var protoId = args[0];
        var prototypeManager = _prototype;
        if (!prototypeManager.TryIndex<ErtCallPresetPrototype>(protoId, out var proto))
        {
            shell.WriteError(Loc.GetString("callertcommand-error-prest-not-found", ("protoid", protoId)));
            return;
        }
        if (ertSpawnSystem.SpawnErt(proto))
        {
            _entity.System<SharedAudioSystem>().PlayGlobal("/Audio/Imperial/ErtCall/yesert.ogg", Filter.Broadcast(), true, AudioParams.Default.WithVolume(-5f));
            shell.WriteLine(Loc.GetString("callertcommand-preset-loaded", ("protoid", protoId)));
            return;
        }
        else
        {
            shell.WriteError(Loc.GetString("callertcommand-error-when-load-grid"));
            return;
        }
    }
}
