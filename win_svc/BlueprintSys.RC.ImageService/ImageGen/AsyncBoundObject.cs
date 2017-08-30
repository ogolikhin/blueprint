using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlueprintSys.RC.ImageService.ImageGen
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class AsyncBoundObject
    {

        public int Width { get; private set; }
        public int Height { get; private set; }
        public double Scale { get; private set; }


        public string ErrorMessage { get; private set; }

        public TaskCompletionSource<bool> RenderCompletionSource { get; private set; }

        private CancellationTokenSource _renderTimeoutCancellationToken;

        public void Reset(int timeout)
        {
            Width = 0;
            Height = 0;
            Scale = 0;
            ErrorMessage = null;
            RenderCompletionSource = new TaskCompletionSource<bool>();

            _renderTimeoutCancellationToken = new CancellationTokenSource(timeout);
            _renderTimeoutCancellationToken.Token.Register(() =>
            {
                RenderCompletionSource.TrySetCanceled();
            }, false);
        }

        public void RenderingDone(double width, double height, double scale)
        {
            DisposeRenderTimeoutCancellationToken();

            Width = (int) Math.Ceiling(width);
            Height = (int) Math.Ceiling(height);
            Scale = scale;

            RenderCompletionSource.SetResult(true);
        }

        public void Error(string msg)
        {
            DisposeRenderTimeoutCancellationToken();

            Width = 0;
            Height = 0;
            ErrorMessage = msg;

            RenderCompletionSource.SetResult(false);
        }

        private void DisposeRenderTimeoutCancellationToken()
        {
            _renderTimeoutCancellationToken.Dispose();
            _renderTimeoutCancellationToken = null;
        }
    }
}