using System.Collections.Generic;
using System.Linq;
using ChessCommon;
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
    private readonly BoardData _boardData;
    private readonly DiegeticUi _diegeticUi;
    private readonly EditorCommandsPrompt _editorCommandsPrompt;
    private readonly ChessGameState _gameState;
    private readonly OpenPrompt _openPrompt;
    private readonly SavePrompt _savePrompt;
    private readonly IFileSystem _scenarioFiles;
    private readonly PromotionPrompt _spawnPrompt;

    public EditorSession(ChessGameState gameState, ChessBoard board, DiegeticUi diegeticUi,
        PromotionPrompt spawnPrompt, SavePrompt savePrompt, OpenPrompt openPrompt,
        EditorCommandsPrompt editorCommandsPrompt, BoardData boardData)
    {
        _scenarioFiles = Client.Debug.RepoFileSystem.GetDirectory("Scenarios");
        _gameState = gameState;
        _board = board;
        _spawnPrompt = spawnPrompt;
        _diegeticUi = diegeticUi;
        _savePrompt = savePrompt;
        _openPrompt = openPrompt;
        _editorCommandsPrompt = editorCommandsPrompt;
        _boardData = boardData;
    }

    public override void DragInitiated(Point position)
    {
        var piece = _board.Pieces.GetPieceAt(position);

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
                _board.Pieces.AddPiece(new ChessPiece
                    {PieceType = pieceType, Position = position, Color = _gameState.CurrentTurn});
            });
        }
        else if (mouseButton == MouseButton.Right)
        {
            _board.Pieces.CapturePiece(_board.Pieces.GetPieceAt(position)!.Value.Id);
        }
    }

    public override void DragSucceeded(Point position)
    {
        var id = _diegeticUi.DraggedId;
        if (id.HasValue)
        {
            var piece = _board.Pieces.GetPieceFromId(id.Value);

            if (piece.HasValue)
            {
                _board.Pieces.ExecuteMove(new ChessMove(piece.Value, position));
            }
        }
    }

    public override void DragFinished(Point? position)
    {
        _diegeticUi.ClearDrag();
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack screenLayer)
    {
        if (input.Keyboard.GetButton(Keys.Tab).WasPressed)
        {
            _gameState.ForceNextTurn();
        }

        if (!IsPromptOpen())
        {
            if (input.Keyboard.Modifiers.Control)
            {
                if (input.Keyboard.GetButton(Keys.S, true).WasPressed)
                {
                    _savePrompt.Request(fileName =>
                    {
                        var json = JsonConvert.SerializeObject(new SerializedScenario
                        {
                            Board = _board.Pieces.Serialize(),
                            BoardData = _boardData.Serialize()
                        }, Formatting.Indented);
                        _scenarioFiles.WriteToFile(fileName, json);
                    });
                }

                if (input.Keyboard.GetButton(Keys.O, true).WasPressed)
                {
                    _openPrompt.Request(scenario =>
                    {
                        _boardData.PopulateFromScenario(scenario);
                        _board.Pieces.PopulateFromScenario(scenario);
                    });
                }
            }

            if (input.Keyboard.GetButton(Keys.OemTilde).WasPressed)
            {
                _editorCommandsPrompt.Open();
            }
        }
    }

    public override void OnExit()
    {
        foreach (var prompt in AllPrompts())
        {
            prompt.Cancel();
        }
    }

    public override void OnEnter()
    {
    }

    private bool IsPromptOpen()
    {
        return AllPrompts().Any(prompt => prompt.IsOpen);
    }

    private IEnumerable<Prompt> AllPrompts()
    {
        yield return _savePrompt;
        yield return _openPrompt;
        yield return _spawnPrompt;
        yield return _editorCommandsPrompt;
    }
}
