using System.Collections.Generic;
using ExplogineCore;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BigChess;

public class ChessCartridge : BasicGameCartridge, IHotReloadable
{
    private Assets _assets;
    private SpriteSheet _spriteSheet;
    private Camera _camera;
    public override CartridgeConfig CartridgeConfig => new(Constants.RenderResolution);

    public ChessCartridge(IRuntime runtime) : base(runtime)
    {
    }

    public override void OnCartridgeStarted()
    {
        _spriteSheet = _assets.GetAsset<SpriteSheet>("Pieces");
        _camera = new Camera(Constants.RenderResolution.ToRectangleF(), Constants.RenderResolution);
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Mouse.GetButton(MouseButton.Middle, true).IsDown)
        {
            var newBounds = _camera.ViewBounds;
            newBounds = newBounds.Moved(-input.Mouse.Delta(hitTestStack.WorldMatrix * _camera.ScreenToCanvas));
            // newBounds = newBounds.ConstrainedTo(Constants.TotalBoardSizePixels.ToRectangleF().Inflated(Constants.TileSize, Constants.TileSize));
            _camera.ViewBounds = newBounds;
        }

        var scrollDelta = input.Mouse.ScrollDelta(true);
        
        if (scrollDelta > 0 && _camera.ViewBounds.Height > Constants.TileSize * 8)
        {
            _camera.ViewBounds = _camera.ViewBounds.GetZoomedInBounds(30, input.Mouse.Position(hitTestStack.WorldMatrix * _camera.ScreenToCanvas));
        }
        
        if (scrollDelta < 0)
        {
            _camera.ViewBounds = _camera.ViewBounds.GetZoomedOutBounds(30, input.Mouse.Position(hitTestStack.WorldMatrix * _camera.ScreenToCanvas));
        }
    }

    public override void Update(float dt)
    {
    }

    public override void Draw(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        _spriteSheet.DrawFrameAsRectangle(painter, 0, new RectangleF(0,0,Constants.TileSize,Constants.TileSize), new DrawSettings());

        for (var y = 0; y < Constants.BoardLength; y++)
        {
            for (var x = 0; x < Constants.BoardLength; x++)
            {
                var color = Color.Green.DimmedBy(0.25f);

                if (x % 2 != y % 2)
                {
                    color = Color.YellowGreen.DimmedBy(0.25f);
                }

                painter.DrawRectangle(new RectangleF(new Vector2(x,y) * Constants.TileSize, new Vector2(Constants.TileSize)), new DrawSettings{Color = color, Depth = Depth.Back});
            }
        }
        
        painter.EndSpriteBatch();
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        _assets = new Assets();
        var content = new RealFileSystem("DynamicContent");

        foreach (var fileName in content.GetFilesAt(".", "png"))
        {
            var texture = Texture2D.FromFile(Client.Graphics.Device, content.RootPath + "/" + fileName);
            yield return new VoidLoadEvent(fileName,
                ()=>
                {
                    _assets.AddAsset(fileName.RemoveFileExtension(), new TextureAsset(texture));
                });
        }

        yield return new VoidLoadEvent("SpriteSheet", () =>
        {
            _assets.AddAsset("Pieces", new GridBasedSpriteSheet(_assets.GetTexture("pieces"), new Point(Constants.PieceRenderSize)));
        });
    }

    public void OnHotReload()
    {
        Client.Debug.Log("Hot Reloaded!");
    }
}