using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace TetrisBot
{
    class ScreenGrab
    {
        private Bitmap _bitmap;

        private Rectangle? _app;
        private Rectangle app
        {
            get
            {
                if (!_app.HasValue)
                {
                    _app = FindAppBounds();
                }

                return _app.Value;
            }
        }

        public ScreenGrab()
        {
            Size scr = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
            _bitmap = new Bitmap(scr.Width, scr.Height);
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.CopyFromScreen(new Point(0,0), new Point(0,0), _bitmap.Size);
            }

            using (Bitmap b = _bitmap.Clone(FindBoardBounds(), _bitmap.PixelFormat))
            {
                b.Save(@"C:\tetris\2\4.png");
            }
        }


        private Color GetAverageColor(Rectangle rect)
        {
            rect = CropRectangle(rect);

            int r = 0;
            int g = 0;
            int b = 0;

            int i = 0;

            for (int x = rect.X; x < rect.X + rect.Width; x++)
            {
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    Color c = _bitmap.GetPixel(x, y);
                    r += c.R;
                    g += c.G;
                    b += c.B;
                    i++;
                }
            }

            r /= i;
            g /= i;
            b /= i;

            return Color.FromArgb(r, g, b);
        }

        private Rectangle CropRectangle(Rectangle r)
        {
            if (r.Y + r.Height > _bitmap.Height)
            {
                r.Height = _bitmap.Height - r.Y;
            }

            if (r.X + r.Width > _bitmap.Width)
            {
                r.Width = _bitmap.Width - r.X;
            }

            return r;
        }

        private Bitmap GetCropped(Rectangle r)
        {
            r = CropRectangle(r);

            return _bitmap.Clone(r, _bitmap.PixelFormat);
        }

        private Rectangle FindBoardBounds()
        {
            const int fromTop = 150;
            const int fromLeft = 90;
            const int width = 180;
            const int height = 360;

            int left = 0;
            int top = 0;

            {
                RollingWindow<int> lums = new RollingWindow<int>(10);
                for (int i = (int)(fromLeft * .9); i < fromTop * 1.1; i++)
                {
                    Color c = _bitmap.GetPixel(app.X + fromLeft + (width / 2), app.Y + i);

                    lums.Add((int)(c.GetBrightness() * 100));

                    if (lums.HasValue(2))
                    {
                        if (lums.Get(2) == 0 && lums.Get(1) == 16 && lums.Get(0) == 13)
                        {
                            top = i;
                            break;
                        }
                    }
                }
            }

            {
                RollingWindow<int> lums = new RollingWindow<int>(10);
                for (int i = (int)(fromLeft * .9); i < fromLeft * 1.1; i++)
                {
                    Color c = _bitmap.GetPixel(app.X + i, app.Y + fromTop + (height / 2));

                    lums.Add((int)(c.GetBrightness() * 100));

                    if (lums.HasValue(1))
                    {
                        if (lums.Get(1) == 0 && lums.Get(0) == 13)
                        {
                            left = i;
                            break;
                        }
                    }
                }
            }

            return new Rectangle(new Point(app.X + left, app.Y + top), new Size(width, height));
        }
        private Rectangle FindAppBounds()
        {
            const int squareSize = 10;
            const int applet_width = 760;

            Rectangle testRegion = new Rectangle(new Point(0,0), new Size(_bitmap.Width, _bitmap.Height));

            var fill = new List<List<bool>>();

            int row = 0;

            // Run a rolling window across the canvas
            for (int x = 0; x < _bitmap.Size.Width; x += squareSize)
            {
                fill.Add(new List<bool>());
                for (int y = 0; y < _bitmap.Size.Height; y += squareSize)
                {
                    Rectangle window = new Rectangle(new Point(x, y), new Size(squareSize, squareSize));
                    Color avg = GetAverageColor(window);
                    int hue = (int)avg.GetHue();
                    int bright = (int)(avg.GetBrightness() * 100);

                    if (bright < 95)
                    {
                        fill[row].Add(false);
                    }
                    else
                    {
                        fill[row].Add(true);
                    }
                }

                row++;
            }

            // Find the top of the map
            Rectangle coords = new Rectangle(new Point(0, 0), new Size(0, 0));
            for (int i = 1; i < fill.Count - 1; i++)
            {
                for (int j = 1; j < fill[i].Count - 1; j++)
                {
                    // If the cell is not white, don't bother.
                    if (fill[i][j] == true)
                    {
                        continue;
                    }
                    // Conintue: look for the top-left of the white square
                    int left = i;
                    int top = j;
                    int right = i;
                    int bottom = j;
                    while (left > 0)
                    {
                        if (fill[--left][j] == true)
                        {
                            left++;
                            break;
                        }
                    }

                    while (right < fill.Count - 1)
                    {
                        if (fill[++right][j] == true)
                        {
                            right--;
                            break;
                        }
                    }

                    while (top > 0)
                    {
                        if (fill[i][--top] == true)
                        {
                            left++;
                            break;
                        }
                    }

                    while (bottom < fill[i].Count - 1)
                    {
                        if (fill[i][++bottom] == true)
                        {
                            bottom--;
                            break;
                        }
                    }

                    right++;
                    bottom++;

                    int dx = right - left;
                    int dy = bottom - top;

                    if (dx <= 0 || dy <= 0)
                    {
                        continue;
                    }

                    Rectangle new_coords = new Rectangle(new Point(testRegion.X + (left * squareSize), testRegion.Y + (top * squareSize)), new Size(dx * squareSize, dy * squareSize));
                    if ((applet_width - squareSize) < new_coords.Width && (applet_width + squareSize) > new_coords.Width)
                    {
                        coords = new_coords;
                    }
                }
            }

            return coords;
        }
    }
}
