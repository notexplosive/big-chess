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
    private readonly Assets _assets;
    private readonly ChessBoard _board;
    private readonly Camera _camera;
    private readonly DiegeticUi _diegeticUi;
    private readonly ChessGameState _gameState;
    private readonly ChessInput _input;
    private readonly PromotionUi _promotionUi;
    private readonly PromotionUi _spawnUi;
    private readonly UiState _uiState;
    private bool _isEditMode;

    public ChessCartridge(IRuntime runtime) : base(runtime)
    {
        _assets = new Assets();
        _gameState = new ChessGameState();
        _input = new ChessInput(_gameState);
        _board = new ChessBoard(_gameState);
        _uiState = new UiState();
        _diegeticUi = new DiegeticUi(_uiState, _board, _assets, _gameState, _input);
        _spawnUi = new PromotionUi(_gameState, runtime, _assets, _board, true, new List<string>
        {
            nameof(PieceType.Pawn),
            nameof(PieceType.Queen),
            nameof(PieceType.Bishop),
            nameof(PieceType.Knight),
            nameof(PieceType.Rook),
            nameof(PieceType.King)
        });
        _promotionUi = new PromotionUi(_gameState, runtime, _assets, _board, false, new List<string>
        {
            nameof(PieceType.Queen),
            nameof(PieceType.Bishop),
            nameof(PieceType.Knight),
            nameof(PieceType.Rook)
        });
        _camera = new Camera(Constants.RenderResolution.ToRectangleF(), Constants.RenderResolution);

        _board.AddPiece(new ChessPiece
            {Position = new Point(3, 1), PieceType = PieceType.Pawn, Color = PieceColor.White});

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

        _board.AddPiece(new ChessPiece
            {Position = new Point(13, 2), PieceType = PieceType.Rook, Color = PieceColor.White});

        _board.AddPiece(new ChessPiece
            {Position = new Point(6, 4), PieceType = PieceType.Rook, Color = PieceColor.White});

        _board.AddPiece(new ChessPiece
            {Position = new Point(20, 4), PieceType = PieceType.Rook, Color = PieceColor.White});

        _board.AddPiece(new ChessPiece
            {Position = new Point(13, 10), PieceType = PieceType.Rook, Color = PieceColor.White});

        _input.SquareClicked += ClickOn;
        _input.DragInitiated += DragInitiated;
        _input.DragSucceeded += DragSucceeded;
        _input.DragFinished += DragFinished;
        _gameState.PromotionRequested += RequestPromotion;
    }

    public override CartridgeConfig CartridgeConfig => new(Constants.RenderResolution, SamplerState.AnisotropicWrap);

    public void OnHotReload()
    {
        Client.Debug.Log("Hot Reloaded!");
    }

    private void RequestPromotion(ChessPiece piece)
    {
        _promotionUi.Request(type => { _board.Promote(_gameState.PendingPromotionId, type); });
    }

    private void DragFinished(Point? position)
    {
        _diegeticUi.ClearDrag();
        
        if (_uiState.SelectedPiece.HasValue && _uiState.SelectedPiece.Value.Position != position)
        {
            _uiState.SelectedPiece = null;
        }
    }

    private void DragInitiated(Point position)
    {
        if (_isEditMode)
        {
            var piece = _board.GetPieceAt(position);

            if (piece != null)
            {
                _diegeticUi.BeginDrag(piece!.Value);
            }
        }
        else
        {
            var piece = _board.GetPieceAt(position);

            if (IsSelectable(piece))
            {
                AttemptSelect(piece);
                _diegeticUi.BeginDrag(piece!.Value);
            }
        }
    }

    private void DragSucceeded(Point dragStart, Point position)
    {
        if (_isEditMode)
        {
            var id = _diegeticUi.DraggedId;
            if (id.HasValue)
            {
                var piece = _board.GetPieceFromId(id.Value);

                if (piece.HasValue)
                {
                    _board.MovePiece(new ChessMove(piece.Value, position));
                }
            }

            _diegeticUi.DropDraggedPiece(position);
        }
        else
        {
            var move = GetSelectedPieceValidMoveTo(position);
            if (move != null)
            {
                _board.MovePiece(move);
                _diegeticUi.DropDraggedPiece(position);
                _uiState.SelectedPiece = null;
            }
            else
            {
                DragFinished(position);
            }
        }
    }

    private void ClickOn(Point position, MouseButton mouseButton)
    {
        if (_isEditMode)
        {
            if (mouseButton == MouseButton.Right)
            {
                if (_board.IsEmptySquare(position))
                {
                    _spawnUi.Request(pieceType =>
                    {
                        _board.AddPiece(new ChessPiece
                            {PieceType = pieceType, Position = position, Color = _gameState.CurrentTurn});
                    });
                }
                else
                {
                    _board.CapturePiece(_board.GetPieceAt(position)!.Value.Id);
                }
            }
        }
        else
        {
            _diegeticUi.ClearDrag();
            var piece = _board.GetPieceAt(position);

            if (piece == _uiState.SelectedPiece)
            {
                return;
            }

            var move = GetSelectedPieceValidMoveTo(position);
            if (move != null)
            {
                _board.MovePiece(move);
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
    }

    private ChessMove? GetSelectedPieceValidMoveTo(Point position)
    {
        if (_uiState.SelectedPiece.HasValue)
        {
            var selectedPiece = _uiState.SelectedPiece.Value;
            var validMoves = selectedPiece.GetValidMoves(_board);
            var index = validMoves.FindIndex(move => move.Position == position);
            if (index == -1)
            {
                return null;
            }

            return validMoves[index];
        }

        return null;
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
            if (input.Keyboard.GetButton(Keys.W).WasPressed)
            {
                _gameState.NextTurn();
                Client.Debug.Log($"{_gameState.CurrentTurn} to move");
            }
        }

        var worldLayer = screenLayer.AddLayer(_camera.ScreenToCanvas, Depth.Middle);
        HandleCameraControls(input, worldLayer);

        worldLayer.AddInfiniteZone(Depth.Back, () => _input.OnHoverVoid(input));
        foreach (var boardRectangle in Constants.BoardRectangles())
        {
            worldLayer.AddZone(boardRectangle.PixelRect, Depth.Middle,
                () => { _input.SetHoveredSquare(input, boardRectangle.GridPosition); });
        }

        _diegeticUi.UpdateInput(input, worldLayer);
        _promotionUi.UpdateInput(input, screenLayer);
        _spawnUi.UpdateInput(input, screenLayer);
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
        _promotionUi.Update(dt);
        _spawnUi.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        DrawBoard(painter);
        _diegeticUi.Draw(painter, _camera);
        _promotionUi.Draw(painter);
        _spawnUi.Draw(painter);
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
