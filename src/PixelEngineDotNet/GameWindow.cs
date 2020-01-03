using System;
using System.ComponentModel;

namespace PixelEngineDotNet
{
    public abstract class GameWindow : IDisposable
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

        public Action? PositionChanged;

        public Action? SizeChanged;

        public Action<CancelEventArgs>? Closing;

        public Action? Closed;

        internal GameWindow(Size size)
        {
            PlatformInitialize(size);
        }

        protected abstract void PlatformInitialize(Size size);

        public void Dispose()
        {
            PlatformDispose();
        }

        protected abstract void PlatformDispose();

        protected abstract IntPtr PlatformGetHandle();

        protected abstract void PlatformSetTitle(string title);

        protected abstract void PlatformSetPosition(int x, int y);

        protected abstract void PlatformSetSize(int width, int height);

        protected bool TriggerClose()
        {
            CancelEventArgs cancelEventArgs = new CancelEventArgs();
            OnClosing(cancelEventArgs);

            if (cancelEventArgs.Cancel)
                return false;

            OnClose(EventArgs.Empty);

            return true;
        }

        protected void TriggerPositionChanged(int x, int y)
        {
            _x = x;
            _y = y;

            OnPositionChanged();
        }

        protected void TriggerSizeChanged(int width, int height)
        {
            _width = width;
            _height = height;

            OnSizeChanged();
        }

        protected virtual void OnPositionChanged()
        {
            PositionChanged?.Invoke();
        }

        protected virtual void OnSizeChanged()
        {
            SizeChanged?.Invoke();
        }

        protected virtual void OnClosing(CancelEventArgs e)
        {
            Closing?.Invoke(e);
        }

        protected virtual void OnClose(EventArgs e)
        {
            IsClosed = true;
            Closed?.Invoke();
        }

        public void Show()
        {
            PlatformShow();
        }

        protected abstract void PlatformShow();

        public void Hide()
        {
            PlatformHide();
        }

        protected abstract void PlatformHide();

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

        protected abstract void PlatformPollEvents();
    }
}
