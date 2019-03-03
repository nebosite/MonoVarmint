using System;

namespace MonoVarmint.Widgets
{
    public class AlignmentTuple
    {
        public Alignment? X { get; set; }
        public Alignment? Y { get; set; }

        public AlignmentTuple(string parseText)
        {
            var parts = parseText.Split(',');
            if(parts.Length > 2)
            {
                throw new ArgumentException("Too many parts in alignment format.");
            }

            X = !string.IsNullOrWhiteSpace(parts[0]) ? (Alignment?) UIHelpers.GetValueFromText(typeof(Alignment), parts[0]) : null;
            if(parts.Length == 1)
            {
                Y = X;
            }
            else
            {
                Y = !string.IsNullOrWhiteSpace(parts[1]) ? (Alignment?)UIHelpers.GetValueFromText(typeof(Alignment), parts[1]) : null;
            }
        }

        public AlignmentTuple(Alignment? x, Alignment? y)
        {
            if (x == Alignment.Top) x = Alignment.Left;
            if (x == Alignment.Bottom) x = Alignment.Right;
            X = x;

            if (y == Alignment.Left) y = Alignment.Top;
            if (y == Alignment.Right) y = Alignment.Bottom;
            Y = y;
        }
    }

    public enum Alignment
    {
        Stretch = 0,
        Left = 1,
        Top = 1,
        Center = 2,
        Right = 3,
        Bottom = 4,
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
