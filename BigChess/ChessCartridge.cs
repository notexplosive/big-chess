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
    private readonly BoardData _boardData;
    private readonly Camera _camera;
    private readonly DiegeticUi _diegeticUi;
    private readonly EditorSession _editorSession;
    private readonly GameSession _gameSession;
    private readonly ChessInput _input;
    private readonly OverlayUi _overlayUi;
    private readonly Rail _promptRail = new();
    private readonly UiState _uiState;
    private Session? _currentSession;

    public ChessCartridge(IRuntime runtime) : base(runtime)
    {
        _boardData = new BoardData();
        _assets = new Assets();
        _input = new ChessInput();
        _uiState = new UiState();
        var board = new ChessBoard(_boardData);
        var gameState = new ChessGameState(board, _boardData);
        _diegeticUi = new DiegeticUi(_uiState, board, _assets, _input, _boardData);
        _overlayUi = new OverlayUi(gameState, runtime, _boardData, () => IsEditMode);
        var spawnPrompt = new PromotionPrompt(gameState, runtime, _assets, true, new List<string>
        {
            nameof(PieceType.Pawn),
            nameof(PieceType.Queen),
            nameof(PieceType.Bishop),
            nameof(PieceType.Knight),
            nameof(PieceType.Rook),
            nameof(PieceType.King)
        });
        var promotionPrompt = new PromotionPrompt(gameState, runtime, _assets, false, new List<string>
        {
            nameof(PieceType.Queen),
            nameof(PieceType.Bishop),
            nameof(PieceType.Knight),
            nameof(PieceType.Rook)
        });
        var savePrompt = new SavePrompt(runtime);
        var openPrompt = new OpenPrompt(runtime);
        var editorCommandsPrompt = new EditorCommandsPrompt(runtime, board, _boardData);

        _camera = new Camera(Constants.RenderResolution.ToRectangleF(), Constants.RenderResolution);
        _camera.CenterPosition = _boardData.TotalBoardSizePixels().ToVector2() / 2f;
        _camera.ZoomOutFrom(
            (int) (_boardData.TotalBoardSizePixels().X * runtime.Window.RenderResolution.AspectRatio() * 1.5f),
            _camera.CenterPosition);

        _gameSession = new GameSession(gameState, _uiState, board, _diegeticUi, promotionPrompt);
        _editorSession = new EditorSession(gameState, board, _diegeticUi, spawnPrompt, savePrompt, openPrompt,
            editorCommandsPrompt, _boardData);

        _promptRail.Add(savePrompt);
        _promptRail.Add(promotionPrompt);
        _promptRail.Add(spawnPrompt);
        _promptRail.Add(openPrompt);
        _promptRail.Add(editorCommandsPrompt);

        _input.SquareClicked += ClickOn;
        _input.DragInitiated += DragInitiated;
        _input.DragSucceeded += DragSucceeded;
        _input.DragFinished += DragFinished;

        CurrentSession = _gameSession;
    }

    public bool IsEditMode => CurrentSession is EditorSession;

    private Session? CurrentSession
    {
        get => _currentSession;

        set
        {
            _currentSession?.OnExit();
            _currentSession = value;
            value?.OnEnter();
        }
    }

    public override CartridgeConfig CartridgeConfig => new(Constants.RenderResolution, SamplerState.AnisotropicWrap);

    public void OnHotReload()
    {
        Client.Debug.Log("Hot Reloaded!");
    }

    private void DragFinished(Point? position)
    {
        CurrentSession?.DragFinished(position);
    }

    private void DragInitiated(Point position)
    {
        CurrentSession?.DragInitiated(position);
    }

    private void DragSucceeded(Point dragStart, Point position)
    {
        CurrentSession?.DragSucceeded(position);
    }

    private void ClickOn(Point position, MouseButton mouseButton)
    {
        CurrentSession?.ClickOn(position, mouseButton);
    }

    public override void OnCartridgeStarted()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack screenLayer)
    {
        if (input.Keyboard.GetButton(Keys.F4).WasPressed)
        {
            if (IsEditMode)
            {
                CurrentSession = _gameSession;
            }
            else
            {
                CurrentSession = _editorSession;
            }

            _uiState.SelectedPiece = null;
        }

        CurrentSession?.UpdateInput(input, screenLayer);

        var worldLayer = screenLayer.AddLayer(_camera.ScreenToCanvas, Depth.Middle);
        var overlayLayer = screenLayer.AddLayer(Matrix.Identity, Depth.Middle - 100);
        var promptsLayer = screenLayer.AddLayer(Matrix.Identity, Depth.Middle - 200);

        HandleCameraControls(input, worldLayer);

        worldLayer.AddInfiniteZone(Depth.Back, () => _input.OnHoverVoid(input));
        foreach (var boardRectangle in _boardData.BoardRectangles())
        {
            worldLayer.AddZone(boardRectangle.PixelRect, Depth.Middle,
                () => { _input.SetHoveredSquare(input, boardRectangle.GridPosition); });
        }

        _overlayUi.UpdateInput(input, overlayLayer);
        _promptRail.UpdateInput(input, promptsLayer);
        _diegeticUi.UpdateInput(input, worldLayer);
    }

    private void HandleCameraControls(ConsumableInput input, HitTestStack layer)
    {
        var windowRectangle = Runtime.Window.RenderResolution.ToRectangleF();
        var boardRectangle = _boardData.TotalBoardSizePixels().ToRectangleF();
        if (windowRectangle.Envelopes(boardRectangle))
        {
            var aspectRatio = windowRectangle.Size.AspectRatio();
            var cameraViewSize = new Vector2(aspectRatio * boardRectangle.LongSide, boardRectangle.LongSide);
            var cameraOffset = RectangleF.FromSizeAlignedWithin(cameraViewSize.ToRectangleF(), boardRectangle.Size, Alignment.Center).Location;
            _camera.ViewBounds = new RectangleF(-cameraOffset, cameraViewSize);
            return;
        }

        if (input.Mouse.GetButton(MouseButton.Middle, true).IsDown)
        {
            _camera.ViewBounds = _camera.ViewBounds.Moved(-input.Mouse.Delta(layer.WorldMatrix));
        }

        var scrollDelta = input.Mouse.ScrollDelta(true);

        var zoomAmount = 60;
        if (scrollDelta > 0 && _camera.ViewBounds.Height > Constants.TileSizePixels * 8)
        {
            _camera.ViewBounds = _camera.ViewBounds.GetZoomedInBounds(zoomAmount,
                input.Mouse.Position(layer.WorldMatrix));
        }

        if (scrollDelta < 0)
        {
            _camera.ViewBounds = _camera.ViewBounds.GetZoomedOutBounds(zoomAmount,
                input.Mouse.Position(layer.WorldMatrix));
        }

        _camera.ViewBounds = _camera.ViewBounds.ConstrainedTo(boardRectangle
            .Inflated(_camera.ViewBounds.Width - Constants.TileSizePixels,
                _camera.ViewBounds.Height - Constants.TileSizePixels));
    }

    public override void Update(float dt)
    {
        _diegeticUi.Update(dt);
        _overlayUi.Update(dt);
        _promptRail.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        _promptRail.EarlyDraw(painter);

        if (IsEditMode)
        {
            painter.Clear(Color.DarkRed.DesaturatedBy(0.8f));
        }
        else
        {
            painter.Clear(Color.Blue.DesaturatedBy(0.8f));
        }

        DrawBoard(painter);
        _diegeticUi.Draw(painter, _camera.CanvasToScreen);
        _overlayUi.Draw(painter);
        _promptRail.Draw(painter);
    }

    private void DrawBoard(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        var extraDesaturate = 0f;

        if (IsEditMode)
        {
            extraDesaturate = 0.25f;
        }

        foreach (var boardCell in _boardData.BoardRectangles())
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
                    new GridBasedSpriteSheet(_assets.GetTexture("pieces"), new Point(Constants.PieceSizePixels)));
            });
    }
}
