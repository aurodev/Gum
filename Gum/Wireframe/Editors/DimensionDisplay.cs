﻿using Microsoft.Xna.Framework;
using RenderingLibrary;
using RenderingLibrary.Graphics;
using RenderingLibrary.Math;
using RenderingLibrary.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Wireframe.Editors
{
    #region Enums

    public enum WidthOrHeight
    {
        Width,
        Height
    }

    #endregion

    class DimensionDisplay
    {
        #region Fields/Properties

        Line endCap1;
        Line endCap2;

        Line middleLine;
        Text dimensionDisplayText;

        float Zoom => systemManagers.Renderer.Camera.Zoom;

        SystemManagers systemManagers;

        #endregion

        public void AddToManagers(SystemManagers systemManagers)
        {
            void AddLineToManagers(Line line) => systemManagers.ShapeManager.Add(line);

            middleLine = new Line(systemManagers);
            AddLineToManagers(middleLine);

            dimensionDisplayText = new Text(systemManagers);
            dimensionDisplayText.RenderBoundary = false;
            dimensionDisplayText.Width = 0;
            dimensionDisplayText.Height = 0;

            systemManagers.TextManager.Add(dimensionDisplayText);
            this.systemManagers = systemManagers;

            endCap1 = new Line(systemManagers);
            AddLineToManagers(endCap1);

            endCap2 = new Line(systemManagers);
            AddLineToManagers(endCap2);
        }

        public void SetColor(Color color)
        {
            endCap1.Color = color;
            endCap2.Color = color;

            middleLine.Color = color;
            dimensionDisplayText.Color = color;
        }

        public void SetVisible(bool isVisible)
        {
            endCap1.Visible = endCap2.Visible = middleLine.Visible = dimensionDisplayText.Visible = isVisible;
        }

        public void Destroy()
        {
            systemManagers.ShapeManager.Remove(endCap1);
            systemManagers.ShapeManager.Remove(endCap2);

            systemManagers.ShapeManager.Remove(middleLine);

            systemManagers.TextManager.Remove(dimensionDisplayText);

        }

        public void Activity(GraphicalUiElement objectToUpdateTo, WidthOrHeight widthOrHeight)
        {
            var asIpso = objectToUpdateTo as IRenderableIpso;

            var left = asIpso.GetAbsoluteX();
            var absoluteWidth = asIpso.Width;

            var top = asIpso.GetAbsoluteY();
            var absoluteHeight = asIpso.Height;

            var topLeft = new Vector2(left, top);

            float fromBodyOffset = 26 / Zoom;
            float endCapLength = 12 / Zoom;
            dimensionDisplayText.FontScale = 1/Zoom;

            var rotationMatrix = asIpso.GetAbsoluteRotationMatrix();
            var rotatedLeftDirection = new Vector2(rotationMatrix.Left.X, rotationMatrix.Left.Y);
            var rotatedRightDirection = new Vector2(rotationMatrix.Right.X, rotationMatrix.Right.Y);
            var rotatedUpDirection = new Vector2(rotationMatrix.Down.X, rotationMatrix.Down.Y);
            var rotatedDownDirection = new Vector2(rotationMatrix.Up.X, rotationMatrix.Up.Y);
            var extraTextOffset = 4;

            if(widthOrHeight == WidthOrHeight.Width)
            {
                string suffix = null;
                switch(objectToUpdateTo.WidthUnits)
                {
                    case DataTypes.DimensionUnitType.MaintainFileAspectRatio:
                        suffix = " File Aspect Ratio";
                        break;
                    case DataTypes.DimensionUnitType.Percentage:
                        suffix = "% Container";
                        break;
                    case DataTypes.DimensionUnitType.PercentageOfOtherDimension:
                        suffix = "% Height";
                        break;
                    case DataTypes.DimensionUnitType.PercentageOfSourceFile:
                        suffix = "% File";
                        break;
                    case DataTypes.DimensionUnitType.Ratio:
                        suffix = " Ratio Container";
                        break;
                    case DataTypes.DimensionUnitType.RelativeToChildren:
                        suffix = " Relative to Children";
                        break;
                    case DataTypes.DimensionUnitType.RelativeToContainer:
                        suffix = " Relative to Container";
                        break;
                }

                middleLine.SetPosition(topLeft + rotatedUpDirection * fromBodyOffset);
                middleLine.RelativePoint = rotatedRightDirection * absoluteWidth;

                if(suffix != null)
                {
                    dimensionDisplayText.RawText = $"{objectToUpdateTo.Width:0.0}{suffix}\n({absoluteWidth:0.0} px)"; 

                }
                else
                {
                    dimensionDisplayText.RawText = absoluteWidth.ToString("0.0");
                }

                dimensionDisplayText.SetPosition(
                    topLeft + 
                    rotatedUpDirection * (fromBodyOffset + extraTextOffset + dimensionDisplayText.WrappedTextHeight) + 
                    rotatedRightDirection * (-dimensionDisplayText.WrappedTextWidth/2 + absoluteWidth/2)  );
                //dimensionDisplayText.X = middleLine.X + absoluteWidth / 2.0f - dimensionDisplayText.WrappedTextWidth / 2.0f;
                //dimensionDisplayText.Y = top - fromBodyOffset - dimensionDisplayText.WrappedTextHeight;
                dimensionDisplayText.HorizontalAlignment = HorizontalAlignment.Center;
                dimensionDisplayText.VerticalAlignment = VerticalAlignment.Center;

                endCap1.SetPosition(middleLine.GetPosition() + rotatedUpDirection * endCapLength / 2.0f);
                endCap1.RelativePoint = rotatedDownDirection * endCapLength;

                endCap2.SetPosition(middleLine.GetPosition() + rotatedRightDirection * absoluteWidth + rotatedUpDirection * endCapLength / 2.0f);
                endCap2.RelativePoint = rotatedDownDirection * endCapLength;

            }
            else // height
            {
                string suffix = null;
                switch (objectToUpdateTo.HeightUnits)
                {
                    case DataTypes.DimensionUnitType.MaintainFileAspectRatio:
                        suffix = " File Aspect Ratio";
                        break;
                    case DataTypes.DimensionUnitType.Percentage:
                        suffix = "% Container";
                        break;
                    case DataTypes.DimensionUnitType.PercentageOfOtherDimension:
                        suffix = "% Width";
                        break;
                    case DataTypes.DimensionUnitType.PercentageOfSourceFile:
                        suffix = "% File";
                        break;
                    case DataTypes.DimensionUnitType.Ratio:
                        suffix = " Ratio Container";
                        break;
                    case DataTypes.DimensionUnitType.RelativeToChildren:
                        suffix = " Relative to Children";
                        break;
                    case DataTypes.DimensionUnitType.RelativeToContainer:
                        suffix = " Relative to Container";
                        break;
                }

                // up is 0,1,0, which is actually down for Gum. Confusing, I know, but this results in the correct math
                middleLine.X = (topLeft + rotatedLeftDirection * fromBodyOffset).X;
                middleLine.Y = (topLeft + rotatedLeftDirection * fromBodyOffset).Y;

                middleLine.RelativePoint = rotatedDownDirection * absoluteHeight;

                if (suffix != null)
                {
                    dimensionDisplayText.RawText = $"{objectToUpdateTo.Height:0.0}{suffix}\n({absoluteHeight:0.0} px)";

                }
                else
                {
                    dimensionDisplayText.RawText = absoluteHeight.ToString("0.0");
                }

                var dimensionDisplayPosition = topLeft + rotatedDownDirection * .5f * absoluteHeight + rotatedLeftDirection * (fromBodyOffset + extraTextOffset);

                dimensionDisplayText.X = dimensionDisplayPosition.X - dimensionDisplayText.WrappedTextWidth;
                dimensionDisplayText.Y = dimensionDisplayPosition.Y - dimensionDisplayText.WrappedTextHeight / 2.0f;
                dimensionDisplayText.VerticalAlignment = VerticalAlignment.Center;

                endCap1.SetPosition(middleLine.GetPosition() + rotatedLeftDirection * endCapLength / 2.0f);
                endCap1.RelativePoint = rotatedRightDirection * endCapLength;

                endCap2.SetPosition(middleLine.GetPosition() + middleLine.RelativePoint + rotatedLeftDirection * endCapLength / 2.0f);
                endCap2.RelativePoint = rotatedRightDirection * endCapLength;
            }




        }
    }
}
