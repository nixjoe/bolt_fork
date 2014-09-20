﻿using UnityEngine;
using System.Collections;
using System.Text;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Level1")]
public class ServerConnectCallbacks : BoltCallbacks {
  public override void Connected(BoltConnection c) {
    c.userToken = new Player();
    c.GetPlayer().connection = c;
    c.GetPlayer().name = "CLIENT:" + c.remoteEndPoint.Port;
  }

  public override void Disconnected(BoltConnection arg) {

  }
}
