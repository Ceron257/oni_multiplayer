﻿using System;
using System.Linq;
using MultiplayerMod.Core.Logging;

namespace MultiplayerMod.Multiplayer.Commands.Screens.Consumable;

[Serializable]
public class PermitConsumableToMinion : IMultiplayerCommand {

    private static Core.Logging.Logger log = LoggerFactory.GetLogger<PermitConsumableToMinion>();

    private string properName;
    private string consumableId;
    private bool isAllowed;

    public PermitConsumableToMinion(string properName, string consumableId, bool isAllowed) {
        this.properName = properName;
        this.consumableId = consumableId;
        this.isAllowed = isAllowed;
    }

    public void Execute() {
        var minionIdentity =
            global::Components.LiveMinionIdentities.Items.FirstOrDefault(
                minion => minion.GetProperName() == properName
            );
        if (minionIdentity == null) {
            log.Warning($"Minion {properName} is not found.");
            return;
        }
        var consumableConsumer = minionIdentity.GetComponent<ConsumableConsumer>();
        consumableConsumer.SetPermitted(consumableId, isAllowed);

        RefreshTable();
    }

    private void RefreshTable() {
        var screen = ManagementMenu.Instance.consumablesScreen;
        foreach (var row in screen.rows) {
            var minion = row.GetIdentity();
            foreach (var widget in row.widgets.Where(entry => entry.Key is ConsumableInfoTableColumn)
                         .Select(entry => entry.Value)) {
                screen.on_load_consumable_info(minion, widget);
            }
        }
    }
}
