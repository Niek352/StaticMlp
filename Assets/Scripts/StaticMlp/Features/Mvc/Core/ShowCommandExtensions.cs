using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.EcsUi.Mvc
{
    public static class ShowCommandExtensions
    {
        public static UniTask Execute<TView, TInputData>(
            this ref ShowCommand<TView, TInputData> command,
            IController controller,
            ViewOrdering ordering,
            CancellationToken ct)
            where TView : IView
        {
            var typedController = (IController<TView, TInputData>)controller;
            return typedController.LaunchViewLifeCycleAsync(ordering, command.InputData, ct);
        }
    }
}
