﻿using BigChess;
using ExplogineDesktop;
using ExplogineMonoGame;
using Microsoft.Xna.Framework;

var config = new WindowConfigWritable
{
    WindowSize = new Point(1600, 900),
    Title = "NotExplosive.net"
};
Bootstrap.Run(args, new WindowConfig(config), runtime => new HotReloadCartridge(runtime, new ChessCartridge(runtime)));
