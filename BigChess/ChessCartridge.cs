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
    private readonly ChessBoard _board;
    private readonly ChessInput _input;
    private SpriteSheet _spriteSheet = null!;
    private readonly UiState _uiState;
    private readonly ChessGameState _gameState;

    public ChessCartridge(IRuntime runtime) : base(runtime)
    {
        _assets = new Assets();
        _gameState = new ChessGameState();
        _input = new ChessInput(_gameState);
        _board = new ChessBoard();
        _uiState = new UiState();
        _diegeticUI = new DiegeticUi(_uiState, _board, _assets, _gameState);
        _camera = new Camera(Constants.RenderResolution.ToRectangleF(), Constants.RenderResolution);

        _board.AddPiece(new ChessPiece
            {Position = new Point(3, 3), PieceType = PieceType.Knight, Color = PieceColor.Black});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(5, 4), PieceType = PieceType.Pawn, Color = PieceColor.Black});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(7, 3), PieceType = PieceType.Pawn, Color = PieceColor.Black});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(4, 5), PieceType = PieceType.Knight, Color = PieceColor.White});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(6, 5), PieceType = PieceType.Knight, Color = PieceColor.Black});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(10, 10), PieceType = PieceType.Bishop, Color = PieceColor.White});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(15, 10), PieceType = PieceType.Rook, Color = PieceColor.White});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(5, 10), PieceType = PieceType.Pawn, Color = PieceColor.White});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(16, 11), PieceType = PieceType.Pawn, Color = PieceColor.White});

        _board.AddPiece(new ChessPiece
            {Position = new Point(16, 5), PieceType = PieceType.Queen, Color = PieceColor.White});
        
        _board.AddPiece(new ChessPiece
            {Position = new Point(13, 4), PieceType = PieceType.King, Color = PieceColor.White});
        
        _input.ClickedSquare += ClickOnSquare;
        _input.DragInitiated += DragInitiated;
        _input.DragComplete += DragComplete;
        _input.DragCancelled += DragCancelled;
    }

    private void DragCancelled()
    {
        _diegeticUI.ClearDrag();
        _uiState.SelectedPiece = null;
    }

    public override CartridgeConfig CartridgeConfig => new(Constants.RenderResolution, SamplerState.AnisotropicWrap);

    public void OnHotReload()
    {
        Client.Debug.Log("Hot Reloaded!");
    }
    
    private void DragInitiated(Point position)
    {
        var piece = _board.GetPieceAt(position);

        if (IsSelectable(piece))
        {
            AttemptSelect(piece);
            _diegeticUI.BeginDrag(piece!.Value);
        }
    }
    
    private void DragComplete(Point dragStart, Point position)
    {
        if (SelectedPieceCanMoveTo(position))
        {
            _board.MovePiece(_uiState.SelectedPiece!.Value.Id, position);
            _diegeticUI.DropDraggedPiece(position);
            _uiState.SelectedPiece = null;
        }
        else
        {
            DragCancelled();
        }
    }

    private void ClickOnSquare(Point position)
    {
        _diegeticUI.ClearDrag();
        var piece = _board.GetPieceAt(position);

        if (piece == _uiState.SelectedPiece)
        {
            return;
        }

        if (SelectedPieceCanMoveTo(position))
        {
            _board.MovePiece(_uiState.SelectedPiece!.Value.Id, position);
            _uiState.SelectedPiece = null;
        }
        else
        {
            if (!AttemptSelect(piece))
            {
                _uiState.SelectedPiece = null;
            }
        }
            
    }

    private bool SelectedPieceCanMoveTo(Point position)
    {
        return _uiState.SelectedPiece.HasValue && _uiState.SelectedPiece.Value.GetValidMoves(_board).Contains(position);
    }

    private bool AttemptSelect(ChessPiece? piece)
    {
        if (IsSelectable(piece))
        {
            _uiState.SelectedPiece = piece;
            return true;
        }

        return false;
    }

    private bool IsSelectable(ChessPiece? piece)
    {
        return piece.HasValue && piece.Value.Color == _gameState.CurrentTurn;
    }

    public override void OnCartridgeStarted()
    {
        _spriteSheet = _assets.GetAsset<SpriteSheet>("Pieces");
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var layer = hitTestStack.AddLayer(_camera.ScreenToCanvas, Depth.Middle);
        HandleCameraControls(input, layer);

        layer.AddInfiniteZone(Depth.Back, ()=> _input.OnHoverVoid(input));
        foreach (var boardRectangle in Constants.BoardRectangles())
        {
            layer.AddZone(boardRectangle.PixelRect, Depth.Middle,
                () => { _input.OnHoverSquare(input, boardRectangle.GridPosition); });
        }
        
        _diegeticUI.UpdateInput(input, layer);
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
        _diegeticUI.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        DrawBoard(painter);
        _diegeticUI.Draw(painter, _camera);
    }

    private void DrawBoard(Painter painter)
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