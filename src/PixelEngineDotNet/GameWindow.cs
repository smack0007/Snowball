using System;
using System.ComponentModel;

namespace PixelEngineDotNet
{
    public partial class GameWindow : IDisposable
    {
        private string _title = "";
        private int _x;
        private int _y;
        private int _width;
        private int _height;

        public IntPtr Handle
        {
            get { return PlatformGetHandle(); }
        }

        public string Title
        {
            get { return _title; }

            set
            {
                if (value != _title)
                {
                    _title = value;
                    PlatformSetTitle(value);
                }
            }
        }

        public int X
        {
            get { return _x; }

            set
            {
                if (value != _x)
                {
                    _x = value;
                    PlatformSetPosition(_x, _y);
                }
            }
        }

        public int Y
        {
            get { return _y; }

            set
            {
                if (value != _y)
                {
                    _y = value;
                    PlatformSetPosition(_x, _y);
                }
            }
        }

        public int Width
        {
            get { return _width; }

            set
            {
                if (value != _width)
                {
                    _width = value;
                    PlatformSetSize(_width, _height);
                }
            }
        }

        public int Height
        {
            get { return _height; }

            set
            {
                if (value != _height)
                {
                    _height = value;
                    PlatformSetSize(_width, _height);
                }
            }
        }

        public bool IsClosed { get; private set; }

        public event EventHandler<EventArgs>? PositionChanged;

        public event EventHandler<EventArgs>? SizeChanged;

        public event EventHandler<CancelEventArgs>? Closing;

        public event EventHandler<EventArgs>? Closed;

        public GameWindow(Size size)
        {
            PlatformInitialize(size);
        }

        ~GameWindow()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            PlatformDispose(disposing);
        }

        private bool TriggerClose()
        {
            CancelEventArgs cancelEventArgs = new CancelEventArgs();
            OnClosing(cancelEventArgs);

            if (cancelEventArgs.Cancel)
                return false;

            OnClose(EventArgs.Empty);

            return true;
        }

        private void TriggerPositionChanged(int x, int y)
        {
            _x = x;
            _y = y;

            OnPositionChanged(EventArgs.Empty);
        }

        private void TriggerSizeChanged(int width, int height)
        {
            _width = width;
            _height = height;

            OnSizeChanged(EventArgs.Empty);
        }

        protected virtual void OnPositionChanged(EventArgs e)
        {
            PositionChanged?.Invoke(this, e);
        }

        protected virtual void OnSizeChanged(EventArgs e)
        {
            SizeChanged?.Invoke(this, e);
        }

        protected virtual void OnClosing(CancelEventArgs e)
        {
            Closing?.Invoke(this, e);
        }

        protected virtual void OnClose(EventArgs e)
        {
            IsClosed = true;
            Closed?.Invoke(this, e);
        }

        public void Show()
        {
            PlatformShow();
        }

        public void Hide()
        {
            PlatformHide();
        }

        public void Run(Action onIdle)
        {
            if (onIdle == null)
                throw new ArgumentNullException(nameof(onIdle));

            Show();

            while (true)
            {
                if (IsClosed)
                    break;

                PlatformPollEvents();

                onIdle();
            }
        }
    }
}
