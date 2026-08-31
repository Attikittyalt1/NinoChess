using MathNet.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NinoChess.Events;
using NinoChess.Moves;
using NinoChess.Mutations;
using NinoChess.Networking;
using NinoChess.Pieces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NinoChess;
public class MyGame : Game
{
    private Position TrueWindowSize => new(Window.ClientBounds.Width, Window.ClientBounds.Height);
    private Position MarginOffset => (TrueWindowSize - BaseSize) / 2;
    private Position BaseSize => Position.MultiplyComponentWise(_grid.Dimensions, _gridCellSize + _gridBorderSize) + _gridOffset + _gridBorderSize;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _atlas;
    private Dictionary<PieceID, DrawablePiece> _pieceTextures;
    private Dictionary<MoveID, Color> _moveColors;

    private Grid? _grid;
    private TurnManager? _turnManager;
    private ClientConnectionLayer? _layer;
    private Client? _client;
    private readonly IPEndPoint _ep;

    private Position _gridOffset => Position.Zero;
    private Position _gridCellSize => Position.Unit * 64;
    private Position _gridBorderSize => Position.Unit * 2;
    private Point _textureSize => Position.Unit * 64;

    private bool _undoPrevDown = false;
    private bool _redoPrevDown = false;

    record PieceDraggingData(bool IsDraggingPiece, Position InitialPiecePosition)
    {
       
    }
    private DraggingHandler<PieceDraggingData> draggingHandler;

    public MyGame(IPEndPoint ep)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _ep = ep;
    }

    protected override void Initialize()
    {
        _grid = new(8);
        var boardState = new BoardStateData(_grid);
        var eventService = new EventService();
        var mutationService = new MutationService();

        _turnManager = new(boardState, mutationService, eventService);
        _layer = new(_turnManager);
        _client = new(_layer);

        SetupBoard(boardState, eventService, mutationService);

        draggingHandler = new();
        draggingHandler.OnDragBegin += (o, e) =>
        {
            var handler = (DraggingHandler<PieceDraggingData>)o;

            if (IsPixelPositionOnCell(handler.InitialPosition))
            {
                var pos = ConvertPixelPositionToGridPosition(handler.InitialPosition);

                if (_grid.ContainsPosition(pos) && _grid.HasPieceAt(pos))
                {
                    handler.DraggingData = new(true, pos);
                    return;
                }
            }

            handler.DraggingData = new(false, default);
        };

        draggingHandler.OnDragEnd += (o, e) =>
        {
            var handler = (DraggingHandler<PieceDraggingData>)o;
            var data = handler.DraggingData;

            if (!data.IsDraggingPiece)
            {
                return;
            }

            if (IsPixelPositionOnCell(handler.CurrentPosition))
            {
                var pos = ConvertPixelPositionToGridPosition(handler.CurrentPosition);

                var info = new MoveInfo(data.InitialPiecePosition, pos);

                if (_grid.ContainsPosition(pos) && _turnManager.IsValid(info) && _layer.UndoBuffer == 0 && _layer.NetworkMoveBuffer.Count == 0)
                {
                    _turnManager.Do(info);
                    _layer.LocalMoveBuffer.Enqueue(info);

                    _layer.Input.TrySetResult(CustomPacket.FromTurn(_layer.LocalMoveBuffer.Peek(), _turnManager.Turn - _layer.LocalMoveBuffer.Count));
                    return;
                }
            }
        };

        InitializeWindow();

        AddPieces();
        AddMoves();

        Task.Run(async () =>
        {
            await _client.StartAsync();
            await _client.ConnectAsync(_ep);
        });

        base.Initialize();
    }

    public static void SetupBoard(BoardStateData boardState, EventService eventService, MutationService mutationService)
    {
        Create(new Pawn { Position = new(0, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(1, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(2, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(3, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(4, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(5, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(6, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(7, 1), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Rook { Position = new(0, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Knight { Position = new(1, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Scholar { Position = new(2, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Moog { Position = new(3, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new King { Position = new(4, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Scholar { Position = new(5, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Knight { Position = new(6, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });
        Create(new Rook { Position = new(7, 0), Orientation = Transformation.Identity, Allegience = Allegience.White, BoardState = boardState, EventService = eventService });

        Create(new Pawn { Position = new(0, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(1, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(2, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(3, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(4, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(5, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(6, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Pawn { Position = new(7, 6), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Rook { Position = new(0, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Knight { Position = new(1, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Scholar { Position = new(2, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Moog { Position = new(3, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new King { Position = new(4, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Scholar { Position = new(5, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Knight { Position = new(6, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });
        Create(new Rook { Position = new(7, 7), Orientation = Transformation.Flip, Allegience = Allegience.Black, BoardState = boardState, EventService = eventService });

        void Create(Piece piece)
        {
            var sender = boardState;
            var args = new Event_Create
            {
                MutationService = mutationService,
                Piece = piece
            };

            piece.OnCreate(sender, args);

            new Mutation_Create { Board = boardState.Board, Piece = piece}.Execute();

            eventService.Get<Event_Create>()?.Invoke(sender, args);
        }
    }

    private void InitializeWindow()
    {
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = BaseSize.X;
        _graphics.PreferredBackBufferHeight = BaseSize.Y;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _atlas = Content.Load<Texture2D>("Assets/_spritesheet");
    }

    private void AddPieces()
    {
        _pieceTextures = [];
        Add(PieceID.Pawn, new(0, 1), new(1, 1));
        Add(PieceID.King, new(2, 1), new(3, 1));
        Add(PieceID.Bishop, new(4, 1), new(5, 1));
        Add(PieceID.Knight, new(6, 1), new(7, 1));
        Add(PieceID.Rook, new(0, 2), new(1, 2));
        Add(PieceID.Moog, new(2, 2), new(3, 2));
        Add(PieceID.Dannel, new(4, 2), new(5, 2));
        Add(PieceID.Nuldar, new(6, 2), new(7, 2));
        AddWithTokens(PieceID.Scholar, new(0, 3), new(1, 3), [new(6, 0), new(7, 0)]);

        void Add(PieceID id, Point primary, Point secondary)
        {
            AddWithTokens(id, primary, secondary, []);
        }
        void AddWithTokens(PieceID id, Point primary, Point secondary, Point[] tokens)
        {
            _pieceTextures.Add(id, new DrawablePiece(
                ConvertAtlasPointToRectangle(primary),
                ConvertAtlasPointToRectangle(secondary),
                tokens.Select(ConvertAtlasPointToRectangle).ToArray(), new()
                {
                    [Allegience.White] = (Color.White, Color.Black),
                    [Allegience.Black] = (Color.Black, Color.White),
                }
                ));

        }
    }

    private void AddMoves()
    {
        _moveColors = [];
        _moveColors.Add(MoveID.AttackBlockable, Color.FromHSV(0f, 1f, 1f));
        _moveColors.Add(MoveID.AttackUnblockable, Color.FromHSV(0f, 0.6f, 1f));
        _moveColors.Add(MoveID.RangedAttackUnblockable, Color.FromHSV(30, 0.6f, 1f));
        _moveColors.Add(MoveID.MoveBlockable, Color.FromHSV(200f, 1f, 1f));
        _moveColors.Add(MoveID.FirstMoveBlockable, Color.FromHSV(200f, 1f, 1f));
        _moveColors.Add(MoveID.MoveUnblockable, Color.FromHSV(200f, 0.6f, 1f));
        _moveColors.Add(MoveID.MoveOrAttackBlockable, Color.FromHSV(270f, 1f, 1f));
        _moveColors.Add(MoveID.MoveOrAttackUnblockable, Color.FromHSV(270f, 0.6f, 1f));
        _moveColors.Add(MoveID.SwapBlockable, Color.FromHSV(310, 1f, 1f));
        _moveColors.Add(MoveID.MoveOrSwapBlockable, Color.FromHSV(310, 0.6f, 1f));
        _moveColors.Add(MoveID.AlternateSwapUnblockable, Color.FromHSV(330, 0.6f, 1f));
    }

    protected override void Update(GameTime gameTime)
    {
        for (int i = 0; i < _layer.UndoBuffer; i++)
        {
            _turnManager.Undo();
        }

        while (_layer.NetworkMoveBuffer.TryDequeue(out var move))
        {
            _turnManager.Do(move);
        }

        UpdateInputs(gameTime);

        base.Update(gameTime);
    }

    private void UpdateInputs(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
        MouseState mouseState = Mouse.GetState();


        if (gamePadState.Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        draggingHandler.Update(gameTime, mouseState);

        if (_undoPrevDown != keyboardState.IsKeyDown(Keys.Z))
        {
            if (_undoPrevDown == true)
            {
                _turnManager.Undo();
            }

            _undoPrevDown = keyboardState.IsKeyDown(Keys.Z);
        }

        

        if (_redoPrevDown != keyboardState.IsKeyDown(Keys.Y))
        {
            if (_redoPrevDown == true)
            {
                _turnManager.Redo();
            }

            _redoPrevDown = keyboardState.IsKeyDown(Keys.Y);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkSlateGray);

        _spriteBatch.Begin(blendState: BlendState.AlphaBlend);

        DrawBoard();

        if (draggingHandler.IsDragging && (draggingHandler.DraggingData?.IsDraggingPiece ?? false))
        {
            var piecePosition = draggingHandler.DraggingData.InitialPiecePosition;

            var pixelPosition = draggingHandler.CurrentPosition + ConvertGridPositionToPixelPosition(piecePosition) - draggingHandler.InitialPosition;

            DrawPieceMoves(piecePosition);

            DrawPiece(pixelPosition, _grid.GetPieceAt(piecePosition));
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawBoard()
    {
        DrawGrid();
    }

    private void DrawGrid()
    {
        foreach (var position in _grid.GetValidPositions())
        {
            DrawTile(position);

            bool isEmpty = !_grid.HasPieceAt(position) || (draggingHandler.IsDragging && (draggingHandler.DraggingData?.IsDraggingPiece ?? false) && draggingHandler.DraggingData.InitialPiecePosition == position);

            if (!isEmpty)
            {
                DrawPiece(position);
            }
        }
    }

    private void DrawTile(Position gridPos)
    {
        var boardColor = (gridPos.X + gridPos.Y).IsEven() ? Color.Tan : Color.Beige;
        var pixelPos = ConvertGridPositionToPixelPosition(gridPos);

        DrawFromAtlas(pixelPos, new(0,0), boardColor);
        DrawBorder(gridPos, boardColor);
    }

    private void DrawBorder(Position gridPos, Color color)
    {
        var pixelPos = ConvertGridPositionToPixelPosition(gridPos);

        DrawFromAtlas(pixelPos + new Position(0, -_gridBorderSize.Y / 2), new(4, 0), color);
        DrawFromAtlas(pixelPos + new Position(-_gridBorderSize.X / 2, 0), new(5, 0), color);
        DrawFromAtlas(pixelPos + new Position(0, _gridCellSize.Y - _gridBorderSize.Y / 2), new(4, 0), color);
        DrawFromAtlas(pixelPos + new Position(_gridCellSize.X - _gridBorderSize.X / 2, 0), new(5, 0), color);
    }

    private void DrawPiece(Position gridPos)
    {
        DrawPiece(ConvertGridPositionToPixelPosition(gridPos), _grid.GetPieceAt(gridPos));
    }

    private void DrawPieceMoves(Position gridPos)
    {
        foreach (var move in _turnManager.GetValidMovesFrom(gridPos))
        {
            if (_moveColors.TryGetValue((MoveID)(Enum)move.ID, out var color))
            {
                DrawBorder(move.MoveInfo.Target, color);
            }
        }
    }

    private void DrawPiece(Position pixelPos, Piece piece)
    {
        var id = (PieceID)(Enum)piece.ID;

        if (_pieceTextures.TryGetValue(id, out var value))
        {
            value.Draw(pixelPos, new(_spriteBatch, _atlas), (piece.Allegience, piece.CurrentTokenIndex));
        } else
        {
            DrawFromAtlas(pixelPos, new(1,0), Color.White);
        }
    }

    private void DrawFromAtlas(Position pixelPos, Point atlasPos, Color color)
    {
        _spriteBatch.Draw(
            _atlas,
            pixelPos,
            ConvertAtlasPointToRectangle(atlasPos),
            color,
            0f,
            Vector2.Zero,
            Vector2.One,
            SpriteEffects.None,
            0f
        );
    }

    private Position ConvertGridPositionToPixelPosition(Position pos) => MarginOffset + _gridOffset + _gridBorderSize + Position.MultiplyComponentWise(FlipYRegardingBoardSize(pos), _gridCellSize + _gridBorderSize);
    private Position ConvertPixelPositionToGridPosition(Position pos) => FlipYRegardingBoardSize((Position) Util.DivideComponentWise(pos - MarginOffset - _gridOffset - _gridBorderSize, _gridCellSize + _gridBorderSize));
    private bool IsPixelPositionOnCell(Position pos)
    {
        var boardPos = pos - MarginOffset - _gridOffset - _gridBorderSize;

        if (!boardPos.IsBetween(Position.Zero, Position.MultiplyComponentWise(_grid.Dimensions, _gridCellSize + _gridBorderSize), true, false))
        {
            return false;
        }

        return Util.ModulusComponentWise(boardPos, _gridCellSize + _gridBorderSize).IsBetween(Vector2.Zero, _gridCellSize, true, false);
    }

    private Position FlipYRegardingBoardSize(Position pos) => new Position(pos.X, _grid.Dimensions.Y - 1 - pos.Y);

    private Rectangle ConvertAtlasPointToRectangle(Point p) => new Rectangle(new(_textureSize.X * p.X, _textureSize.Y * p.Y), _textureSize);
}

public class DraggingHandler<TData>
    where TData : class
{
    public bool IsDragging
    {
        get; private set
        {
            if (field != value)
            {
                field = value;

                if (value)
                {
                    InitialPosition = CurrentPosition;
                    OnDragBegin?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    OnDragEnd?.Invoke(this, EventArgs.Empty);
                    DraggingData = null;
                }
            }
        }
    }

    public TData? DraggingData;
    public Position InitialPosition { get; private set;  }
    public Position CurrentPosition { get; private set; }
    public Position TotalPositionDelta => CurrentPosition - InitialPosition;

    public class DragMouseUpdateEventArgs(Position recentPositionDelta, TimeSpan elapsedGameTime) : EventArgs
    {
        public Position RecentPositionDelta => recentPositionDelta;
        public TimeSpan ElapsedGameTime => elapsedGameTime;
    }
    public event EventHandler? OnDragMouseUpdate;
    public event EventHandler? OnDragBegin;
    public event EventHandler? OnDragEnd;

    public void Update(GameTime gameTime, MouseState mouseState)
    {
        var PrevPosition = CurrentPosition;
        CurrentPosition = mouseState.Position;

        if (IsDragging)
        {
            OnDragMouseUpdate?.Invoke(this, new DragMouseUpdateEventArgs(CurrentPosition - PrevPosition, gameTime.ElapsedGameTime));
        }

        IsDragging = mouseState.LeftButton == ButtonState.Pressed;
    }
}

public class DrawableSprite(params Rectangle[] components)
{
    public record DrawInfo(SpriteBatch SpriteBatch, Texture2D Atlas);

    public void Draw(Position pos, DrawInfo info, List<(int componentIndex, Color color)> drawnComponents)
    {
        foreach (var (componentIndex, color) in drawnComponents)
        {
            info.SpriteBatch.Draw(
                info.Atlas,
                pos,
                components[componentIndex],
                color,
                0f,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                0f
            );
        }
    }
}

public abstract class DrawableSprite<TData>(params Rectangle[] components) : DrawableSprite(components)
{
    public void Draw(Position pos, DrawInfo info, TData data)
    {
        Draw(pos, info, GetColorData(data));
    }

    public abstract List<(int componentIndex, Color color)> GetColorData(TData data);
}

public class DrawablePiece(Rectangle primary, Rectangle secondary, Rectangle[] tokens, Dictionary<Allegience, (Color primary, Color secondary)> colorMap) : DrawableSprite<(Allegience allegience, int tokenIndex)>([..Enumerable.Concat([primary, secondary], tokens)])
{

    public override List<(int componentIndex, Color color)> GetColorData((Allegience allegience, int tokenIndex) data)
    {
        var colors = colorMap[data.allegience];

        if (data.tokenIndex >= 0)
        {
            return [(0, colors.primary), (1, colors.secondary), (data.tokenIndex + 2, new Color(255, 255, 255, 63))];
        }
        else
        {
            return [(0, colors.primary), (1, colors.secondary)];
        }
    }
}
