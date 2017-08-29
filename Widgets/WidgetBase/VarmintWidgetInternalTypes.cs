namespace MonoVarmint.Widgets
{
    public enum HorizontalContentAlignment
    {
        Left,
        Center,
        Right
    };

    public enum VerticalContentAlignment
    {
        Top,
        Center,
        Bottom
    };

    public enum TouchMoveType
    {
        Move,
        Leave,
        Enter
    };

    public partial class VarmintWidget
    {
        private const string BindingContextPropertyName = "BindingContext";

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// EventHandledState
        /// </summary>
        //--------------------------------------------------------------------------------------
        public enum EventHandledState
        {
            NotHandled,
            Handled
        }
    }
}
