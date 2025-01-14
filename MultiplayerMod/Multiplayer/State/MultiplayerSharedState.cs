﻿using System;
using System.Collections.Generic;

namespace MultiplayerMod.Multiplayer.State;

[Serializable]
public class MultiplayerSharedState {
    public Dictionary<IPlayer, PlayerSharedState> Players = new();
}
