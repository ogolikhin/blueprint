using System;
using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    public class AsyncBoundObject
    {

        public int Width { get; private set; }
        public int Height { get; private set; }

        public TaskCompletionSource<bool> TaskCompletionSource { get; private set; }

        public void Reset()
        {
            Width = 0;
            Height = 0;
            TaskCompletionSource = new TaskCompletionSource<bool>();
        }

        public void RenderingDone(double width, double height)
        {
            Width = (int) Math.Ceiling(width);
            Height = (int) Math.Ceiling(height);

            TaskCompletionSource.SetResult(true);
        }
    }
}