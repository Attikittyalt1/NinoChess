using MathNet.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NinoChess.Pieces;
using System;
using System.Collections.Generic;

namespace NinoChess;
public class MyGame : Game
{
    private Vector2 MinScale => Vector2.One;
    private Vector2 MaxScale => Vector2.One;
    private Vector2 CurrentScale = Vector2.One;
    private Position CurrentSize
    {
        get => ScalePosition(BaseSize);
        set => CurrentScale = new Vector2((float)value.X / BaseSize.X, (float)value.Y / BaseSize.Y).Clamp(MinScale, MaxScale);
    }
    private Position TrueWindowSize => new(Window.ClientBounds.Width, Window.ClientBounds.Height);
    private Position MarginOffset => (TrueWindowSize - CurrentSize) / 2;
    private Position BaseSize => Position.MultiplyComponentWise(_grid.Dimensions, _gridCellSize + _gridBorderSize) + _gridOffset + _gridBorderSize;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _squareTexture;
    private Dictionary<PieceID, IDrawableTextureWithTokens> _pieceTextures;

    private Grid? _grid;
    private BoardState? _board;

    private Position _gridOffset => Position.Zero;
    private Position _tokenOffset => new(_gridCellSize.X - _tokenSize.X * 5 / 4, _tokenSize.Y / 4);
    private Position _tokenSize => Position.Unit * 16;
    private Position _gridCellSize => Position.Unit * 64;
    private Position _gridBorderSize => Position.Unit * 2;

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
        _graphics.PreferredBackBufferWidth = CurrentSize.X;
        _graphics.PreferredBackBufferHeight = CurrentSize.Y;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += new EventHandler<EventArgs>((sender, e) =>
        {
            CurrentSize = TrueWindowSize;
        });
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _squareTexture = new Texture2D(GraphicsDevice, 1, 1);
        _squareTexture.SetData([Color.White]);

        _pieceTextures = [];
        _pieceTextures.Add(PieceID.Pawn, new DrawableSprite(
            Content.Load<Texture2D>("Assets/Pieces/Pawn_Color"),
            Content.Load<Texture2D>("Assets/Pieces/Pawn_Shading")
            ));
        Add(PieceID.Knight, Color.SaddleBrown);
        Add(PieceID.Bishop, Color.CornflowerBlue);
        Add(PieceID.Dannel, Color.IndianRed);
        AddWithTokens(PieceID.Scholar, Color.DarkBlue, [Color.DarkBlue, Color.DarkRed]);
        Add(PieceID.King, Color.YellowGreen);
        Add(PieceID.Moog, Color.DeepPink);
        Add(PieceID.Rook, Color.DarkGreen);
        Add(PieceID.Nuldar, Color.DarkTurquoise);

        void Add(PieceID id, Color color)
        {
            _pieceTextures.Add(id, new DrawableSimpleSprite(
                _squareTexture,
                color,
                _gridCellSize
            ));
        }

        void AddWithTokens(PieceID id, Color color, Color[] tokens)
        {
            _pieceTextures.Add(id, new DrawableSimpleSprite(
                _squareTexture,
                color,
                _gridCellSize,
                _tokenOffset,
                _tokenSize,
                tokens
            ));
        }
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
            var pixelPosition = draggingHandler.CurrentPosition + ConvertGridPositionToPixelPosition(draggingHandler.DraggingData.InitialPiecePosition) - draggingHandler.InitialPosition;

            var piecePosition = draggingHandler.DraggingData.InitialPiecePosition;

            DrawCellForeground(pixelPosition, _board.GetPieceAt(piecePosition));
            
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
            bool isEmpty = _board.IsEmpty(position) || (draggingHandler.IsDragging && (draggingHandler.DraggingData?.IsDraggingPiece ?? false) && draggingHandler.DraggingData.InitialPiecePosition == position);
            
            DrawCell(ConvertGridPositionToPixelPosition(position), position, isEmpty);
        }
    }

    private void DrawCell(Position pixelPos, Position gridPos, bool isEmpty)
    {
        DrawCellBackground(pixelPos, gridPos);

        if (!isEmpty)
        {
            DrawCellForeground(pixelPos, _board.GetPieceAt(gridPos));
        }
    }

    private void DrawCellBackground(Position pixelPos, Position gridPos)
    {
        var boardColor = (gridPos.X + gridPos.Y).IsEven() ? Color.Tan : Color.Beige;

        DrawCell(pixelPos - _gridBorderSize / 2, boardColor, _gridCellSize + _gridBorderSize);
    }

    private void DrawCellForeground(Position pixelPos, Piece piece)
    {
        var (id, allegience) = ((PieceID)(Enum)piece.ID, piece.Allegience);

        var alignmentColor = allegience switch
        {
            Allegience.White => Color.White,
            Allegience.Black => Color.DarkGray,
            _ => Color.Black
        };

        if (_pieceTextures.TryGetValue(id, out var value))
        {
            value.Draw(new(_spriteBatch, CurrentScale, alignmentColor, pixelPos), piece.CurrentTokenIndex);
        } else
        {
            DrawCell(pixelPos, Color.Black, _gridCellSize);
        }
    }

    private void DrawCell(Position pos, Color color, Position size)
    {
        _spriteBatch.Draw(
            _squareTexture,
            pos,
            null,
            color,
            0f,
            Vector2.Zero,
            ScalePosition(size),
            SpriteEffects.None,
            0f
        );
    }

    private Position ScalePosition(Position pos) => (Position) MyExtensions.MultiplyComponentWise(pos, CurrentScale);
    private Position ConvertGridPositionToPixelPosition(Position pos) => ScalePosition(_gridOffset + _gridBorderSize + Position.MultiplyComponentWise(FlipYRegardingBoardSize(pos), _gridCellSize + _gridBorderSize)) + MarginOffset;
    private Position ConvertPixelPositionToGridPosition(Position pos) => FlipYRegardingBoardSize((Position) MyExtensions.DivideComponentWise(MyExtensions.DivideComponentWise(pos - MarginOffset, CurrentScale) - (_gridOffset + _gridBorderSize), _gridCellSize + _gridBorderSize));
    private bool IsPixelPositionOnCell(Position pos)
    {
        var boardPos = MyExtensions.DivideComponentWise(pos - MarginOffset, CurrentScale) - (_gridOffset + _gridBorderSize);

        if (!boardPos.IsBetween(Vector2.Zero, Position.MultiplyComponentWise(_grid.Dimensions, _gridCellSize + _gridBorderSize), true, false))
        {
            return false;
        }

        return MyExtensions.ModulusComponentWise(boardPos, _gridCellSize + _gridBorderSize).IsBetween(Vector2.Zero, _gridCellSize, true, false);
    }

    private Position FlipYRegardingBoardSize(Position pos) => new Position(pos.X, _grid.Dimensions.Y - 1 - pos.Y);
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

public interface IDrawableTexture
{
    public record DrawInfo(SpriteBatch spriteBatch, Vector2 scale, Color color, Position pos);
    public void Draw(DrawInfo info);
}

public interface IDrawableTextureWithTokens : IDrawableTexture
{
    void IDrawableTexture.Draw(DrawInfo info) => Draw(info, -1);
    public void Draw(DrawInfo info, int TokenIndex);
}

public class DrawableSprite : IDrawableTextureWithTokens
{
    private readonly Texture2D _colorTexture;
    private readonly Texture2D _shadingTexture;
    private readonly Texture2D[] _tokens;


    public DrawableSprite(Texture2D color, Texture2D shading)
    {
        _colorTexture = color;
        _shadingTexture = shading;
        _tokens = [];
    }

    public DrawableSprite(Texture2D color, Texture2D shading, Texture2D[] tokens)
    {
        _colorTexture = color;
        _shadingTexture = shading;
        _tokens = tokens;
    }

    public void Draw(IDrawableTexture.DrawInfo info, int TokenIndex)
    {
        info.spriteBatch.Draw(
            _colorTexture,
            info.pos,
            null,
            info.color,
            0f,
            Vector2.Zero,
            info.scale,
            SpriteEffects.None,
            0f
        );

        info.spriteBatch.Draw(
            _shadingTexture,
            info.pos,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            info.scale,
            SpriteEffects.None,
            0f
        );

        if (TokenIndex >= 0)
        {
            info.spriteBatch.Draw(
            _tokens[TokenIndex],
            info.pos,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            info.scale,
            SpriteEffects.None,
            0f
        );
        }
    }
}

public class DrawableSimpleSprite : IDrawableTextureWithTokens
{
    private readonly Texture2D _texture;
    private readonly Color _color;
    private readonly Vector2 _size;
    private readonly Vector2 _tokenOffset;
    private readonly Vector2 _tokenSize;
    private readonly Color[] _tokens;


    public DrawableSimpleSprite(Texture2D texture, Color color, Vector2 size)
    {
        _texture = texture;
        _color = color;
        _size = size;
        _tokenOffset = default;
        _tokenSize = default;
        _tokens = [];
    }

    public DrawableSimpleSprite(Texture2D texture, Color color, Vector2 size, Vector2 tokenOffset, Vector2 tokenSize, Color[] tokens)
    {
        _texture = texture;
        _color = color;
        _size = size;
        _tokenOffset = tokenOffset;
        _tokenSize = tokenSize;
        _tokens = tokens;
    }

    public void Draw(IDrawableTexture.DrawInfo info, int tokenIndex)
    {
        info.spriteBatch.Draw(
            _texture,
            info.pos,
            null,
            info.color,
            0f,
            Vector2.Zero,
            MyExtensions.MultiplyComponentWise(info.scale, _size),
            SpriteEffects.None,
            0f
        );

        info.spriteBatch.Draw(
            _texture,
            info.pos,
            null,
            _color with { A = 127},
            0f,
            Vector2.Zero,
            MyExtensions.MultiplyComponentWise(info.scale, _size),
            SpriteEffects.None,
            0f
        );

        if (tokenIndex >= 0)
        {
            info.spriteBatch.Draw(
                _texture,
                info.pos + _tokenOffset,
                null,
                _tokens[tokenIndex],
                0f,
                Vector2.Zero,
                MyExtensions.MultiplyComponentWise(info.scale, _tokenSize),
                SpriteEffects.None,
                0f
            );
        }
    }
}