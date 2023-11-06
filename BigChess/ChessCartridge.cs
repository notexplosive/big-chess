using System.Collections.Generic;
using ExplogineCore;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BigChess;

public class ChessCartridge : BasicGameCartridge, IHotReloadable
{
    private readonly Assets _assets;
    private readonly Camera _camera;
    private readonly DiegeticUi _diegeticUi;
    private readonly EditorSession _editorSession;
    private readonly GameSession _gameSession;
    private readonly ChessGameState _gameState;
    private readonly ChessInput _input;
    private readonly Rail _promptRail = new();
    private readonly UiState _uiState;
    private bool _isEditMode;

    public ChessCartridge(IRuntime runtime) : base(runtime)
    {
        _assets = new Assets();
        _gameState = new ChessGameState();
        _uiState = new UiState(_gameState);
        _input = new ChessInput(_uiState);
        var board = new ChessBoard(_gameState);
        _diegeticUi = new DiegeticUi(_uiState, board, _assets, _gameState, _input);
        var spawnPrompt = new PromotionPrompt(_gameState, runtime, _assets, true, new List<string>
        {
            nameof(PieceType.Pawn),
            nameof(PieceType.Queen),
            nameof(PieceType.Bishop),
            nameof(PieceType.Knight),
            nameof(PieceType.Rook),
            nameof(PieceType.King)
        });
        var promotionPrompt = new PromotionPrompt(_gameState, runtime, _assets, false, new List<string>
        {
            nameof(PieceType.Queen),
            nameof(PieceType.Bishop),
            nameof(PieceType.Knight),
            nameof(PieceType.Rook)
        });
        var savePrompt = new SavePrompt(runtime);
        var openPrompt = new OpenPrompt(runtime);
        var editorCommandsPrompt = new EditorCommandsPrompt(runtime, board);

        _camera = new Camera(Constants.RenderResolution.ToRectangleF(), Constants.RenderResolution);
        _camera.CenterPosition = Constants.TotalBoardSizePixels.ToVector2() / 2f;
        _camera.ZoomOutFrom(
            (int) (Constants.TotalBoardSizePixels.X * runtime.Window.RenderResolution.AspectRatio() * 1.5f),
            _camera.CenterPosition);

        _gameSession = new GameSession(_gameState, _uiState, board, _diegeticUi, promotionPrompt);
        _editorSession = new EditorSession(_gameState, board, _diegeticUi, spawnPrompt, savePrompt, openPrompt,
            editorCommandsPrompt);

        _promptRail.Add(savePrompt);
        _promptRail.Add(promotionPrompt);
        _promptRail.Add(spawnPrompt);
        _promptRail.Add(openPrompt);
        _promptRail.Add(editorCommandsPrompt);

        _input.SquareClicked += ClickOn;
        _input.DragInitiated += DragInitiated;
        _input.DragSucceeded += DragSucceeded;
        _input.DragFinished += DragFinished;
        _gameState.TurnChanged += AnnounceTurn;
    }

    public override CartridgeConfig CartridgeConfig => new(Constants.RenderResolution, SamplerState.AnisotropicWrap);

    public void OnHotReload()
    {
        Client.Debug.Log("Hot Reloaded!");
    }

    private void AnnounceTurn(PieceColor color)
    {
        Client.Debug.Log($"{color} to move");
    }

    private void DragFinished(Point? position)
    {
        if (_isEditMode)
        {
            _editorSession.DragFinished(position);
        }
        else
        {
            _gameSession.DragFinished(position);
        }
    }

    private void DragInitiated(Point position)
    {
        if (_isEditMode)
        {
            _editorSession.DragInitiated(position);
        }
        else
        {
            _gameSession.DragInitiated(position);
        }
    }

    private void DragSucceeded(Point dragStart, Point position)
    {
        if (_isEditMode)
        {
            _editorSession.DragSucceeded(dragStart, position);
        }
        else
        {
            _gameSession.DragSucceeded(dragStart, position);
        }
    }

    private void ClickOn(Point position, MouseButton mouseButton)
    {
        if (_isEditMode)
        {
            _editorSession.ClickOn(position, mouseButton);
        }
        else
        {
            _gameSession.ClickOn(position, mouseButton);
        }
    }

    public override void OnCartridgeStarted()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack screenLayer)
    {
        if (input.Keyboard.GetButton(Keys.Q).WasPressed)
        {
            _isEditMode = !_isEditMode;
            _uiState.SelectedPiece = null;
            Client.Debug.Log($"EDIT MODE: {_isEditMode}");
        }

        if (_isEditMode)
        {
            _editorSession.UpdateInput(input, screenLayer);
        }
        else
        {
            _gameSession.UpdateInput(input, screenLayer);
        }

        var worldLayer = screenLayer.AddLayer(_camera.ScreenToCanvas, Depth.Middle);
        _promptRail.UpdateInput(input, screenLayer);
        HandleCameraControls(input, worldLayer);

        worldLayer.AddInfiniteZone(Depth.Back, () => _input.OnHoverVoid(input));
        foreach (var boardRectangle in Constants.BoardRectangles())
        {
            worldLayer.AddZone(boardRectangle.PixelRect, Depth.Middle,
                () => { _input.SetHoveredSquare(input, boardRectangle.GridPosition); });
        }

        _diegeticUi.UpdateInput(input, worldLayer);
    }

    private void HandleCameraControls(ConsumableInput input, HitTestStack layer)
    {
        if (input.Mouse.GetButton(MouseButton.Middle, true).IsDown)
        {
            _camera.ViewBounds = _camera.ViewBounds.Moved(-input.Mouse.Delta(layer.WorldMatrix));
        }

        var scrollDelta = input.Mouse.ScrollDelta(true);

        var zoomAmount = 60;
        if (scrollDelta > 0 && _camera.ViewBounds.Height > Constants.TileSize * 8)
        {
            _camera.ViewBounds = _camera.ViewBounds.GetZoomedInBounds(zoomAmount,
                input.Mouse.Position(layer.WorldMatrix));
        }

        if (scrollDelta < 0)
        {
            _camera.ViewBounds = _camera.ViewBounds.GetZoomedOutBounds(zoomAmount,
                input.Mouse.Position(layer.WorldMatrix));
        }

        _camera.ViewBounds = _camera.ViewBounds.ConstrainedTo(Constants.TotalBoardSizePixels.ToRectangleF()
            .Inflated(_camera.ViewBounds.Width - Constants.TileSize, _camera.ViewBounds.Height - Constants.TileSize));
    }

    public override void Update(float dt)
    {
        _diegeticUi.Update(dt);
        _promptRail.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        _promptRail.EarlyDraw(painter);

        painter.Clear(Color.Blue.DimmedBy(0.95f));
        DrawBoard(painter);
        _diegeticUi.Draw(painter, _camera);
        _promptRail.Draw(painter);
    }

    private void DrawBoard(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        var extraDesaturate = 0f;

        if (_isEditMode)
        {
            extraDesaturate = 0.25f;
        }

        foreach (var boardCell in Constants.BoardRectangles())
        {
            var lightColor = new Color(90, 141, 0).DesaturatedBy(extraDesaturate);
            var darkColor = new Color(34, 104, 34).DesaturatedBy(extraDesaturate);

            if (boardCell.SectionX % 2 != boardCell.SectionY % 2)
            {
                lightColor = Color.Pink.DimmedBy(0.2f).DesaturatedBy(0.35f)
                    .DesaturatedBy(extraDesaturate);
                darkColor = Color.Maroon.BrightenedBy(0.1f).DesaturatedBy(0.35f)
                    .DesaturatedBy(extraDesaturate);

                /*
                lightColor = new Color(191, 164, 117).DesaturatedBy(extraDesaturate);
                darkColor = new Color(100, 32, 32).DesaturatedBy(extraDesaturate);
                */
            }

            var color = darkColor;

            if (boardCell.IsLight)
            {
                color = lightColor;
            }

            painter.DrawRectangle(boardCell.PixelRect, new DrawSettings {Color = color, Depth = Depth.Back});
        }

        painter.EndSpriteBatch();
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        var content = Client.Debug.RepoFileSystem.GetDirectory("DynamicContent");

        foreach (var fileName in content.GetFilesAt(".", "png"))
        {
            var texture = Texture2D.FromFile(Client.Graphics.Device, content.GetCurrentDirectory() + "/" + fileName);
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
