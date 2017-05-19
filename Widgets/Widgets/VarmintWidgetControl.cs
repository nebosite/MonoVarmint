using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetControl - Special class for implementing controls
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Control")]
    public class VarmintWidgetControl : VarmintWidget
    {
        public string Class { get; set; }

       //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetControl(): base()
        {
        }
    }
}