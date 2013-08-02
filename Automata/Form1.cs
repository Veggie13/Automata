using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Automata
{
    public partial class Form1 : Form
    {
        private Automata _auto;
        private System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private uint _state = 1;

        public Form1()
        {
            InitializeComponent();
            this.KeyUp += new KeyEventHandler(Form1_KeyUp);
            this.MouseDown += new MouseEventHandler(Form1_MouseDown);
            this.MouseMove += new MouseEventHandler(Form1_MouseMove);
            this.Paint += new PaintEventHandler(Form1_Paint);
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            this.DoubleBuffered = true;
        }

        struct CPoint
        {
            public CPoint(Point pt, uint s)
            {
                point = pt;
                state = s;
            }
            public Point point;
            public uint state;
        }
        List<CPoint> _points = new List<CPoint>();
        Point _last;
        void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & System.Windows.Forms.MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left)
            {
                plot(e.Location);
                _last = e.Location;
            }
        }

        bool _running = true;
        Thread _thread;
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _running = false;
            _thread.Join();
        }

        void plot(Point pt)
        {
            lock (_points)
                _points.Add(new CPoint(pt, _state));
        }

        void swap(ref int a, ref int b)
        {
            int t = a;
            a = b;
            b = t;
        }

        void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & System.Windows.Forms.MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left)
            {
                int x0 = _last.X, y0 = _last.Y;
                int x1 = e.X, y1 = e.Y;

                bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
                if (steep)
                {
                    swap(ref x0, ref y0);
                    swap(ref x1, ref y1);
                }
                if (x0 > x1)
                {
                    swap(ref x0, ref x1);
                    swap(ref y0, ref y1);
                }

                int dx = x1 - x0;
                int dy = Math.Abs(y1 - y0);
                int err = dx / 2;
                int ys = (y0 < y1) ? 1 : -1;
                int y = y0;

                for (int x = x0; x <= x1; x++)
                {
                    if (steep)
                        plot(new Point(y, x));
                    else
                        plot(new Point(x, y));
                    err = err - dy;
                    if (err < 0)
                    {
                        y += ys;
                        err += dx;
                    }
                }
                _last = e.Location;
            }
        }

        bool _paused = false, _step = false;
        void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == '1')
                _state = 1;
            else if (e.KeyValue == '2')
                _state = 11;
            else if (e.KeyValue == '3')
                _state = 21;
            else if (e.KeyValue == ' ')
                _paused = !_paused;
            else if (e.KeyValue == 'S')
                _step = true;
        }

        int _scale = 1;
        Pen[] _pens = { Pens.White,
            Pens.Red, Pens.Red, Pens.Red, Pens.Red, Pens.Red,
            Pens.Red, Pens.Red, Pens.Red, Pens.Red, Pens.Red,
            Pens.Blue, Pens.Blue, Pens.Blue, Pens.Blue, Pens.Blue,
            Pens.Blue, Pens.Blue, Pens.Blue, Pens.Blue, Pens.Blue,
            Pens.LightGreen, Pens.LightGreen, Pens.LightGreen, Pens.LightGreen, Pens.LightGreen,
            Pens.LightGreen, Pens.LightGreen, Pens.LightGreen, Pens.LightGreen, Pens.LightGreen };
        Bitmap _buffer;
        void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = Graphics.FromImage(_buffer);
            _auto.Render((r, c, state) =>
            {
                Pen p = _pens[state];
                    /**/
                //p = new Pen(Color.FromArgb(8 * (int)state, 8 * (int)state, 8 * (int)state));

                g.FillRectangle(p.Brush, _scale *c, _scale * r, _scale, _scale);
            });
            e.Graphics.DrawImage(_buffer, 0, 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _auto = new RPSAutomata((uint)(this.DisplayRectangle.Height / _scale), (uint)(this.DisplayRectangle.Width / _scale));
            _timer.Interval = 100;
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();

            _buffer = new Bitmap(this.DisplayRectangle.Width, this.DisplayRectangle.Height);

            _thread = new Thread(this.process);
            _thread.Start();
        }

        bool _drawn = false;
        void process()
        {
            while (_running)
            {
                if (!_paused || _step)
                {
                    _auto.AdvanceGrid();
                    _step = false;
                }
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            lock (_points)
            {
                foreach (CPoint pt in _points)
                    _auto.SetState((uint)pt.point.Y, (uint)pt.point.X, pt.state);
                _points.Clear();
            }
            this.Invalidate();
        }


    }
}
