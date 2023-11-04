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

namespace BigChess;

public class ChessCartridge : BasicGameCartridge, IHotReloadable
{
    private readonly Assets _assets;
    private readonly Camera _camera;
    private readonly DiegeticUi _diegeticUI;
    private readonly ChessGame _game;
    private readonly ChessInput _input;
    private SpriteSheet _spriteSheet = null!;
    private readonly UiState _uiState;

    public ChessCartridge(IRuntime runtime) : base(runtime)
    {
        _assets = new Assets();
        _input = new ChessInput();
        _game = new ChessGame();
        _uiState = new UiState();
        _diegeticUI = new DiegeticUi(_uiState);
        _camera = new Camera(Constants.RenderResolution.ToRectangleF(), Constants.RenderResolution);

        _game.Pieces.Add(new ChessPiece
            {Position = new Point(3, 3), PieceType = PieceType.Knight, Color = PieceColor.Black});
        _game.Pieces.Add(new ChessPiece
            {Position = new Point(3, 5), PieceType = PieceType.Knight, Color = PieceColor.White});

        _input.ClickedSquare += TrySelectPieceAt;
    }

    public override CartridgeConfig CartridgeConfig => new(Constants.RenderResolution);

    public void OnHotReload()
    {
        Client.Debug.Log("Hot Reloaded!");
    }

    private void TrySelectPieceAt(Point position)
    {
        var piece = _game.GetPieceAt(position);
        if (piece != null)
        {
            _uiState.SelectedPiece = piece;
        }
        else
        {
            _uiState.SelectedPiece = null;
        }
    }

    public override void OnCartridgeStarted()
    {
        _spriteSheet = _assets.GetAsset<SpriteSheet>("Pieces");
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var layer = hitTestStack.AddLayer(_camera.ScreenToCanvas, Depth.Middle);
        HandleCameraControls(input, layer);

        foreach (var boardRectangle in Constants.BoardRectangles())
        {
            layer.AddZone(boardRectangle.PixelRect, Depth.Middle,
                () => { _input.OnHoverSquare(input, boardRectangle.GridPosition); });
        }
    }

    private void HandleCameraControls(ConsumableInput input, HitTestStack layer)
    {
        if (input.Mouse.GetButton(MouseButton.Middle, true).IsDown)
        {
            _camera.ViewBounds = _camera.ViewBounds.Moved(-input.Mouse.Delta(layer.WorldMatrix));
        }

        var scrollDelta = input.Mouse.ScrollDelta(true);

        if (scrollDelta > 0 && _camera.ViewBounds.Height > Constants.TileSize * 8)
        {
            _camera.ViewBounds = _camera.ViewBounds.GetZoomedInBounds(30,
                input.Mouse.Position(layer.WorldMatrix));
        }

        if (scrollDelta < 0)
        {
            _camera.ViewBounds = _camera.ViewBounds.GetZoomedOutBounds(30,
                input.Mouse.Position(layer.WorldMatrix));
        }

        _camera.ViewBounds = _camera.ViewBounds.ConstrainedTo(Constants.TotalBoardSizePixels.ToRectangleF()
            .Inflated(_camera.ViewBounds.Width - Constants.TileSize, _camera.ViewBounds.Height - Constants.TileSize));
    }

    public override void Update(float dt)
    {
        _diegeticUI.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        foreach (var rectangle in Constants.BoardRectangles())
        {
            var color = Color.Green.DesaturatedBy(0.45f);

            if (rectangle.IsLight)
            {
                color = Color.YellowGreen.DimmedBy(0.25f);
            }

            painter.DrawRectangle(rectangle.PixelRect, new DrawSettings {Color = color, Depth = Depth.Back});
        }

        foreach (var piece in _game.Pieces)
        {
            _spriteSheet.DrawFrameAsRectangle(painter, Constants.FrameIndex(piece.PieceType, piece.Color),
                piece.PixelRectangle, new DrawSettings {Depth = Depth.Middle});
        }

        _diegeticUI.Draw(painter);

        painter.EndSpriteBatch();
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        var content = new RealFileSystem("DynamicContent");

        foreach (var fileName in content.GetFilesAt(".", "png"))
        {
            var texture = Texture2D.FromFile(Client.Graphics.Device, content.RootPath + "/" + fileName);
            yield return new VoidLoadEvent(fileName,
                () => { _assets.AddAsset(fileName.RemoveFileExtension(), new TextureAsset(texture)); });
        }

        yield return new VoidLoadEvent("SpriteSheet",
            () =>
            {
                _assets.AddAsset("Pieces",
                    new GridBasedSpriteSheet(_assets.GetTexture("pieces"), new Point(Constants.PieceRenderSize)));
            });
    }
}