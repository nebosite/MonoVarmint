using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetTextInput - simple text input widget
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("TextInput")]
    public class VarmintWidgetTextInput : VarmintWidget
    {
        public float FontSize { get; set; }
        public string FontName { get; set; }
        public int CursorSpot { get; set; }

        float _margin;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetTextInput()
        {
            this.FontSize = 0.1f;
            this.OnRender += Render;
            this.OnInputCharacter += ProcessInputCharacter;
            this.OnTap += VarmintWidgetTextInput_OnTap;
            CursorSpot = 0;
            Content = "";
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// VarmintWidgetTextInput_OnTap
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState VarmintWidgetTextInput_OnTap(VarmintWidget widget, Vector2 location)
        {
            var startOffset = GetTextStartOffset();
            var text = GetTextToDisplay();
            for(int i = 0; i< text.Length; i++)
            {
                startOffset += Renderer.MeasureText(text[i].ToString(), FontName, FontSize);
                if(location.X < startOffset.X)
                {
                    CursorSpot = i;
                    break;
                }
            }
            if (location.X >= startOffset.X) CursorSpot = text.Length;
            return EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ProcessInputCharacter
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState ProcessInputCharacter(char c)
        {
            var text = GetTextToDisplay();
            var leftOfCursor = text.Substring(0, CursorSpot);
            var rightOfCusor = text.Substring(CursorSpot);

            if (c == '\b')
            {
                if (leftOfCursor.Length > 0) leftOfCursor = leftOfCursor.Substring(0,CursorSpot - 1);
                CursorSpot--;
                if (CursorSpot < 0) CursorSpot = 0;
            }
            else
            {
                leftOfCursor += c;
                CursorSpot++;
            }
            Content = leftOfCursor + rightOfCusor;
            return EventHandledState.Handled;  
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetTextStartOffset
        /// </summary>
        //--------------------------------------------------------------------------------------
        Vector2 GetTextStartOffset()
        {
            Vector2 alignedOffset = Vector2.Zero;
            _margin = 0f;
            if (WrapContent) _margin = Size.X;
            Vector2 textSize = Renderer.MeasureText(GetTextToDisplay(), FontName, FontSize, _margin);

            switch (HorizontalContentAlignment)
            {
                case HorizontalContentAlignment.Left: break;
                case HorizontalContentAlignment.Center: alignedOffset.X += (Size.X - textSize.X) / 2; break;
                case HorizontalContentAlignment.Right: alignedOffset.X += (Size.X - textSize.X); break;
            }

            switch (VerticalContentAlignment)
            {
                case VerticalContentAlignment.Top: break;
                case VerticalContentAlignment.Center: alignedOffset.Y += (Size.Y - textSize.Y) / 2; break;
                case VerticalContentAlignment.Bottom: alignedOffset.Y += (Size.Y - textSize.Y); break;
            }
            return alignedOffset;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetTextToDisplay
        /// </summary>
        //--------------------------------------------------------------------------------------
        string GetTextToDisplay()
        {
            var textToDisplay = (Content == null) ? "" : Content.ToString();
            if (CursorSpot > textToDisplay.Length) CursorSpot = textToDisplay.Length;
            if (CursorSpot < 0) CursorSpot = 0;
            return textToDisplay;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Render(GameTime gameTime, VarmintWidget widget)
        {
            var textToDisplay = GetTextToDisplay();

            Renderer.DrawBox(Vector2.Zero, Size, BackgroundColor);
            Vector2 alignedOffset = Vector2.Zero + GetTextStartOffset();

            var leftOfCursor = textToDisplay.Substring(0, CursorSpot);
            var rightOfCusor = textToDisplay.Substring(CursorSpot);
            var leftSize = Renderer.MeasureText(leftOfCursor, FontName, FontSize);
            Renderer.DrawText(leftOfCursor, FontName, FontSize, alignedOffset, ForegroundColor, _margin);
            Renderer.DrawText(rightOfCusor, FontName, FontSize, alignedOffset + new Vector2(leftSize.X, 0), ForegroundColor, _margin);

            if((gameTime.TotalGameTime.TotalSeconds * 2) % 2.0 > 1.0)
            {
                Renderer.DrawLine(
                    alignedOffset + new Vector2(leftSize.X, 0),
                    alignedOffset + leftSize, FontSize * .2f, ForegroundColor);
            }
        }
    }
}