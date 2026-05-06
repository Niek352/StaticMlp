namespace Code.EcsUi.Mvc
{
    public readonly struct ShowCommand<TView, TInputData>
        where TView : IView
    {
        public readonly TInputData InputData;

        public ShowCommand(TInputData inputData)
        {
            InputData = inputData;
        }
    }
}
