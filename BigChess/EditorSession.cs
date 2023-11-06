using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace BigChess;

public class EditorSession : Session
{
    private readonly ChessBoard _board;
    private readonly DiegeticUi _diegeticUi;
    private readonly SavePrompt _savePrompt;
    private readonly OpenPrompt _openPrompt;
    private readonly IFileSystem _scenarioFiles;
    private readonly EditorCommandsPrompt _editorCommandsPrompt;
    private readonly ChessGameState _gameState;
    private readonly PromotionPrompt _spawnPrompt;

    public EditorSession(ChessGameState gameState, ChessBoard board, DiegeticUi diegeticUi,
        PromotionPrompt spawnPrompt, SavePrompt savePrompt, OpenPrompt openPrompt, EditorCommandsPrompt editorCommandsPrompt)
    {
        _scenarioFiles = Client.Debug.RepoFileSystem.GetDirectory("Scenarios");
        _gameState = gameState;
        _board = board;
        _spawnPrompt = spawnPrompt;
        _diegeticUi = diegeticUi;
        _savePrompt = savePrompt;
        _openPrompt = openPrompt;
        _editorCommandsPrompt = editorCommandsPrompt;
    }

    public override void DragInitiated(Point position)
    {
        var piece = _board.GetPieceAt(position);

        if (piece != null)
        {
            _diegeticUi.BeginDrag(piece!.Value);
        }
    }

    public override void ClickOn(Point position, MouseButton mouseButton)
    {
        if (_board.IsEmptySquare(position))
        {
            _spawnPrompt.Request(pieceType =>
            {
                _board.AddPiece(new ChessPiece
                    {PieceType = pieceType, Position = position, Color = _gameState.CurrentTurn});
            });
        }
        else if (mouseButton == MouseButton.Right)
        {
            _board.CapturePiece(_board.GetPieceAt(position)!.Value.Id);
        }
    }

    public override void DragSucceeded(Point dragStart, Point position)
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
    }

    public override void DragFinished(Point? position)
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack screenLayer)
    {
        if (input.Keyboard.GetButton(Keys.W).WasPressed)
        {
            _gameState.NextTurn();
        }

        if (!IsPromptOpen())
        {
            if (input.Keyboard.Modifiers.Control)
            {
                if (input.Keyboard.GetButton(Keys.S, true).WasPressed)
                {
                    _savePrompt.Request(fileName =>
                    {
                        var json = JsonConvert.SerializeObject(_board.Serialize(), Formatting.Indented);
                        _scenarioFiles.WriteToFile(fileName, json);
                    });
                }

                if (input.Keyboard.GetButton(Keys.O, true).WasPressed)
                {
                    _openPrompt.Request(scenario => { _board.Deserialize(scenario); });
                }
            }

            if (input.Keyboard.GetButton(Keys.E).WasPressed)
            {
                _editorCommandsPrompt.Open();
            }
        }
    }
    
    private bool IsPromptOpen()
    {
        return _savePrompt.IsOpen || _openPrompt.IsOpen || _spawnPrompt.IsOpen;
    }
}
