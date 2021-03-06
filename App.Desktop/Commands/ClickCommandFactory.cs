﻿using System.Drawing;
using DigitalGlass.ViewModel;

namespace DigitalGlass.Commands
{
    /// <summary>
    /// Maps the canvashost mode into which command to execute on click
    /// </summary>
    public class CanvasHostCommandFactory
    {
        public static ICanvasHostCommand Create(CanvasHostViewModel viewModel,  CanvasHostMode mode)
        {
            switch (mode)
            {
                case CanvasHostMode.ColorFill:
                    return new FindCellCommand(viewModel);

                case CanvasHostMode.PlaceLED:
                    return new PlaceLedCommand(viewModel);

                case CanvasHostMode.CreateTouchRegion:
                    return new CreateTouchRegionCommand(viewModel);

                case CanvasHostMode.PlaceLEDWithoutCell:
                    return new FindCellCommand(viewModel, System.Drawing.Color.FromArgb(0, 0, 0, 0)); //Transparent white

                case CanvasHostMode.None:
                default:
                    return null;
            }


        }
    }
}