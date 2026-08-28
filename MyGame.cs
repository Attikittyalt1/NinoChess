using MathNet.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NinoChess.Moves;
using NinoChess.Pieces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
    private BoardState? _board;

    private Position _gridOffset => Position.Zero;
    private Position _gridCellSize => Position.Unit * 64;
    private Position _gridBorderSize => Position.Unit * 2;
    private Point _textureSize => Position.Unit * 64;

    record PieceDraggingData(bool IsDraggingPiece, Position InitialPiecePosition) : IDisposable
    {
        public void Dispose()
        {

        }
    }
    private DraggingHandler<PieceDraggingData> draggingHandler;

    public MyGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _grid = new(8);
        _board = new(_grid);

        SetupBoard();

        draggingHandler = new();
        draggingHandler.OnDragBegin += (o, e) =>
        {
            var handler = (DraggingHandler<PieceDraggingData>)o;

            if (IsPixelPositionOnCell(handler.InitialPosition))
            {
                var pos = ConvertPixelPositionToGridPosition(handler.InitialPosition);

                if (_board.ContainsPosition(pos) && _board.HasPieceAt(pos))
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

                if (_board.ContainsPosition(pos) && _board.IsValidMove(data.InitialPiecePosition, pos))
                {
                    _board.ExecuteMove(data.InitialPiecePosition, pos);
                    return;
                }
            }
        };

        InitializeWindow();

        AddPieces();
        AddMoves();

        base.Initialize();
    }

    private void SetupBoard()
    {
        Create(new Pawn(_board, new(0, 1), Transformation.Identity, Allegience.White));
        Create(new Pawn(_board, new(1, 1), Transformation.Identity, Allegience.White));
        Create(new Nuldar(_board, new(2, 1), Transformation.Identity, Allegience.White));
        Create(new Dannel(_board, new(3, 1), Transformation.Identity, Allegience.White));
        Create(new Dannel(_board, new(4, 1), Transformation.Identity, Allegience.White));
        Create(new Nuldar(_board, new(5, 1), Transformation.Identity, Allegience.White));
        Create(new Pawn(_board, new(6, 1), Transformation.Identity, Allegience.White));
        Create(new Pawn(_board, new(7, 1), Transformation.Identity, Allegience.White));
        Create(new Rook(_board, new(0, 0), Transformation.Identity, Allegience.White));
        Create(new Knight(_board, new(1, 0), Transformation.Identity, Allegience.White));
        Create(new Scholar(_board, new(2, 0), Transformation.Identity, Allegience.White));
        Create(new King(_board, new(3, 0), Transformation.Identity, Allegience.White));
        Create(new Moog(_board, new(4, 0), Transformation.Identity, Allegience.White));
        Create(new Scholar(_board, new(5, 0), Transformation.Identity, Allegience.White));
        Create(new Knight(_board, new(6, 0), Transformation.Identity, Allegience.White));
        Create(new Rook(_board, new(7, 0), Transformation.Identity, Allegience.White));

        Create(new Pawn(_board, new(0, 6), Transformation.Flip, Allegience.Black));
        Create(new Pawn(_board, new(1, 6), Transformation.Flip, Allegience.Black));
        Create(new Nuldar(_board, new(2, 6), Transformation.Flip, Allegience.Black));
        Create(new Dannel(_board, new(3, 6), Transformation.Flip, Allegience.Black));
        Create(new Dannel(_board, new(4, 6), Transformation.Flip, Allegience.Black));
        Create(new Nuldar(_board, new(5, 6), Transformation.Flip, Allegience.Black));
        Create(new Pawn(_board, new(6, 6), Transformation.Flip, Allegience.Black));
        Create(new Pawn(_board, new(7, 6), Transformation.Flip, Allegience.Black));
        Create(new Rook(_board, new(0, 7), Transformation.Flip, Allegience.Black));
        Create(new Knight(_board, new(1, 7), Transformation.Flip, Allegience.Black));
        Create(new Scholar(_board, new(2, 7), Transformation.Flip, Allegience.Black));
        Create(new King(_board, new(3, 7), Transformation.Flip, Allegience.Black));
        Create(new Moog(_board, new(4, 7), Transformation.Flip, Allegience.Black));
        Create(new Scholar(_board, new(5, 7), Transformation.Flip, Allegience.Black));
        Create(new Knight(_board, new(6, 7), Transformation.Flip, Allegience.Black));
        Create(new Rook(_board, new(7, 7), Transformation.Flip, Allegience.Black));

        void Create(Piece piece)
        {
            _board.CreatePieceAt(piece.Position, new(piece));
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
                tokens.Select(pos => new Rectangle(_textureSize, pos)).ToArray(), new()
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

            DrawPiece(pixelPosition, _board.GetPieceAt(piecePosition));

            DrawPieceMoves(piecePosition);
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

            bool isEmpty = _board.IsEmpty(position) || (draggingHandler.IsDragging && (draggingHandler.DraggingData?.IsDraggingPiece ?? false) && draggingHandler.DraggingData.InitialPiecePosition == position);

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
        DrawPiece(ConvertGridPositionToPixelPosition(gridPos), _board.GetPieceAt(gridPos));
    }

    private void DrawPieceMoves(Position gridPos)
    {
        foreach (var move in _board.GetValidMovesFrom(gridPos))
        {
            if (_moveColors.TryGetValue((MoveID)(Enum)move.ID, out var color))
            {
                Debug.WriteLine("test");
                DrawBorder(move.Target, color);
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
    private Position ConvertPixelPositionToGridPosition(Position pos) => FlipYRegardingBoardSize((Position) MyExtensions.DivideComponentWise(pos - MarginOffset - _gridOffset - _gridBorderSize, _gridCellSize + _gridBorderSize));
    private bool IsPixelPositionOnCell(Position pos)
    {
        var boardPos = pos - MarginOffset - _gridOffset - _gridBorderSize;

        if (!boardPos.IsBetween(Position.Zero, Position.MultiplyComponentWise(_grid.Dimensions, _gridCellSize + _gridBorderSize), true, false))
        {
            return false;
        }

        return MyExtensions.ModulusComponentWise(boardPos, _gridCellSize + _gridBorderSize).IsBetween(Vector2.Zero, _gridCellSize, true, false);
    }

    private Position FlipYRegardingBoardSize(Position pos) => new Position(pos.X, _grid.Dimensions.Y - 1 - pos.Y);

    private Rectangle ConvertAtlasPointToRectangle(Point p) => new Rectangle(new(_textureSize.X * p.X, _textureSize.Y * p.Y), _textureSize);
}

public class DraggingHandler<TData>
    where TData : class, IDisposable
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
                    DraggingData?.Dispose();
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
            return [(0, colors.primary), (1, colors.secondary), (data.tokenIndex + 2, Color.White)];
        }
        else
        {
            return [(0, colors.primary), (1, colors.secondary)];
        }
    }
}
