using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RoguelikeSkeleton.Input;
using RoguelikeSkeleton.World;

namespace RoguelikeSkeleton.Rendering;

/// <summary>
/// The WinForms front-end: owns the window, draws the grid with GDI+, and
/// asks InputHandler to turn key presses into ICommands. Game, Map, and
/// Entity have zero knowledge that this class exists - that separation is
/// exactly what let the console version get swapped out for this one
/// without touching any game logic.
/// </summary>
public class GameForm : Form
{
    private const int StatusBarHeight = 32;

    private readonly Game _game;
    private readonly Font _font;
    private readonly int _cellWidth;
    private readonly int _cellHeight;
    private bool _gameOver;

    public GameForm()
    {
        _game = new Game();
        _game.Initialize();

        _font = new Font("Consolas", 16f, FontStyle.Regular, GraphicsUnit.Pixel);

        // Measure the font once so the grid lines up regardless of the
        // font's exact metrics on this machine. Uses a throwaway bitmap
        // rather than CreateGraphics() so it doesn't force the window's
        // handle to be created this early.
        using (var bmp = new Bitmap(1, 1))
        using (var g = Graphics.FromImage(bmp))
        {
            var size = g.MeasureString("#", _font);
            _cellWidth = (int)Math.Ceiling(size.Width);
            _cellHeight = (int)Math.Ceiling(size.Height);
        }

        Text = "Roguelike Skeleton";
        DoubleBuffered = true;
        BackColor = Color.Black;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        ClientSize = new Size(
            _game.Map.Width * _cellWidth,
            _game.Map.Height * _cellHeight + StatusBarHeight);

        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_gameOver)
            return;

        if (e.KeyCode == Keys.Escape)
        {
            Close();
            return;
        }

        var command = InputHandler.Translate(e.KeyCode);
        if (command is null)
            return;

        _game.ExecuteTurn(command);

        if (!_game.Player.IsAlive)
            _gameOver = true;

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(Color.Black);

        for (int y = 0; y < _game.Map.Height; y++)
        {
            for (int x = 0; x < _game.Map.Width; x++)
            {
                var tile = _game.Map.GetTile(x, y);
                var color = tile.Type == TileType.Wall ? Color.DimGray : Color.Gray;
                DrawGlyph(g, tile.Glyph, x, y, color);
            }
        }

        foreach (var entity in _game.AllEntities.Where(en => en.IsAlive))
            DrawGlyph(g, entity.Glyph, entity.X, entity.Y, entity.Color);

        DrawStatusBar(g);

        if (_gameOver)
            DrawGameOverOverlay(g);
    }

    private void DrawGlyph(Graphics g, char glyph, int x, int y, Color color)
    {
        using var brush = new SolidBrush(color);
        g.DrawString(glyph.ToString(), _font, brush, x * _cellWidth, y * _cellHeight);
    }

    private void DrawStatusBar(Graphics g)
    {
        int y = _game.Map.Height * _cellHeight;
        using var bg = new SolidBrush(Color.FromArgb(20, 20, 20));
        g.FillRectangle(bg, 0, y, ClientSize.Width, StatusBarHeight);

        using var textBrush = new SolidBrush(Color.White);
        string status = $"HP {_game.Player.Health}/{_game.Player.MaxHealth}   " +
                         "Arrows/WASD move, Space wait, Esc quit";
        g.DrawString(status, _font, textBrush, 6, y + 4);
    }

    private void DrawGameOverOverlay(Graphics g)
    {
        using var overlay = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
        g.FillRectangle(overlay, 0, 0, ClientSize.Width, ClientSize.Height);

        using var bigFont = new Font("Consolas", 28f, FontStyle.Bold, GraphicsUnit.Pixel);
        using var textBrush = new SolidBrush(Color.OrangeRed);
        const string message = "YOU DIED";
        var size = g.MeasureString(message, bigFont);
        g.DrawString(message, bigFont, textBrush,
            (ClientSize.Width - size.Width) / 2,
            (ClientSize.Height - size.Height) / 2);
    }
}
