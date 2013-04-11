﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RenderingLibrary.Math.Geometry;
using Gum.DataTypes;
using Gum.ToolStates;
using Gum.DataTypes.Variables;
using Gum.Managers;
using RenderingLibrary.Graphics;
using Microsoft.Xna.Framework.Graphics;
using RenderingLibrary.Content;
using RenderingLibrary;
using System.Collections;
using Gum.RenderingLibrary;
using Microsoft.Xna.Framework;
using FlatRedBall.AnimationEditorForms.Controls;

namespace Gum.Wireframe
{
    #region Enums

    public enum InstanceFetchType
    {
        InstanceInCurrentElement,
        DeepInstance
    }

    #endregion

    public partial class WireframeObjectManager
    {
        #region Fields

        ElementSave mElementShowing;

        static WireframeObjectManager mSelf;

        List<LineRectangle> mLineRectangles = new List<LineRectangle>();
        List<Sprite> mSprites = new List<Sprite>();
        List<Text> mTexts = new List<Text>();
        List<SolidRectangle> mSolidRectangles = new List<SolidRectangle>();

        WireframeEditControl mEditControl;


        #endregion

        #region Properties

        IEnumerable<IPositionedSizedObject> AllIpsos
        {
            get
            {
                foreach (Sprite sprite in mSprites)
                {
                    yield return sprite;
                }

                foreach (Text text in mTexts)
                {
                    yield return text;
                }
                foreach (LineRectangle rectangle in mLineRectangles)
                {
                    yield return rectangle;
                }
                foreach (SolidRectangle solidRectangle in mSolidRectangles)
                {
                    yield return solidRectangle;
                }
            }

        }

        public ElementSave ElementShowing
        {
            get;
            private set;
        }

        public static WireframeObjectManager Self
        {
            get
            {
                if (mSelf == null)
                {
                    mSelf = new WireframeObjectManager();
                }
                return mSelf;
            }
        }

        #endregion

        public void Initialize(WireframeEditControl editControl)
        {
            mEditControl = editControl;
            mEditControl.ZoomChanged += new EventHandler(HandleControlZoomChange);
        }

        void HandleControlZoomChange(object sender, EventArgs e)
        {
            Renderer.Self.Camera.Zoom = mEditControl.PercentageValue/100.0f;
        }

        private void ClearAll()
        {
            foreach (LineRectangle rectangle in mLineRectangles)
            {
                ShapeManager.Self.Remove(rectangle);
            }
            mLineRectangles.Clear();

            foreach (Sprite sprite in mSprites)
            {
                SpriteManager.Self.Remove(sprite);
            }
            mSprites.Clear();

            foreach (Text text in mTexts)
            {
                TextManager.Self.Remove(text);
            }
            mTexts.Clear();

            foreach (SolidRectangle solidRectangle in mSolidRectangles)
            {
                ShapeManager.Self.Remove(solidRectangle);
            }
            mSolidRectangles.Clear();
        }

        public void RefreshAll(bool force)
        {
            ElementSave elementSave = SelectedState.Self.SelectedElement;

            RefreshAll(force, elementSave);
        }

        public void RefreshAll(bool force, ElementSave elementSave)
        {
            if (elementSave == null)
            {
                ClearAll();
            }

            else if (elementSave != null && (force || elementSave != ElementShowing))
            {

                ClearAll();

                LoaderManager.Self.CacheTextures = false;
                LoaderManager.Self.CacheTextures = true;

                IPositionedSizedObject rootIpso = null;

                if ((elementSave is ScreenSave) == false)
                {
                    if (elementSave.BaseType == "Sprite" || elementSave.Name == "Sprite")
                    {
                        rootIpso = CreateSpriteFor(elementSave);
                    }
                    else if (elementSave.BaseType == "Text" || elementSave.Name == "Text")
                    {
                        rootIpso = CreateTextFor(elementSave);
                    }
                    else if (elementSave.BaseType == "ColoredRectangle" || elementSave.Name == "ColoredRectangle")
                    {
                        rootIpso = CreateSolidRectangleFor(elementSave);
                    }
                    else
                    {
                        rootIpso = CreateRectangleFor(elementSave);
                    }
                }

                List<ElementSave> elementStack = new List<ElementSave>();

                elementStack.Add(elementSave);

                foreach (InstanceSave instance in elementSave.Instances)
                {
                    IPositionedSizedObject child = CreateRepresentationForInstance(instance, null, elementStack, rootIpso);
                }

                elementStack.Remove(elementSave);

            }

            ElementShowing = elementSave;
        }

        public IPositionedSizedObject GetSelectedRepresentation()
        {
            if (!SelectionManager.Self.HasSelection)
            {
                return null;
            }
            else if (SelectedState.Self.SelectedInstance != null)
            {
                return GetRepresentation(SelectedState.Self.SelectedInstance);
            }
            else if (SelectedState.Self.SelectedElement != null)
            {
                return GetRepresentation(SelectedState.Self.SelectedElement);
            }
            else
            {
                throw new Exception("The SelectionManager believes it has a selection, but there is no selected instance or element");
            }
        }

        public IPositionedSizedObject GetRepresentation(ElementSave elementSave)
        {
#if DEBUG
            if (elementSave == null)
            {
                throw new NullReferenceException("The argument elementSave is null");
            }
#endif
            foreach (IPositionedSizedObject ipso in AllIpsos)
            {
                if (ipso.Tag == elementSave)
                {
                    return ipso;
                }
            }

            return null;
        }

        public IPositionedSizedObject GetRepresentation(InstanceSave instanceSave)
        {
            foreach (IPositionedSizedObject ipso in AllIpsos)
            {
                if (ipso.Tag == instanceSave)
                {
                    return ipso;
                }
            }
            return null;
        }
        
        public Text GetText(InstanceSave instanceSave)
        {
            foreach (Text text in mTexts)
            {
                if (text.Name == instanceSave.Name)
                {
                    return text;
                }
            }

            return null;

        }

        public Text GetText(ElementSave elementSave)
        {
            foreach (Text text in mTexts)
            {
                if (text.Name == elementSave.Name)
                {
                    return text;
                }
            }
            return null;
        }
        

        /// <summary>
        /// Returns the InstanceSave that uses this representation or the
        /// instance that has a a contained instance that uses this representation.
        /// </summary>
        /// <param name="representation">The representation in question.</param>
        /// <returns>The InstanceSave or null if one isn't found.</returns>
        public InstanceSave GetInstance(IPositionedSizedObject representation, InstanceFetchType fetchType)
        {
            ElementSave selectedElement = SelectedState.Self.SelectedElement;


            string prefix = selectedElement.Name + ".";
            if (selectedElement is ScreenSave)
            {
                prefix = "";
            }

            return GetInstance(representation, selectedElement, prefix, fetchType);

        }

        public InstanceSave GetInstance(IPositionedSizedObject representation, ElementSave instanceContainer, string prefix, InstanceFetchType fetchType)
        {
            if (instanceContainer == null)
            {
                return null;
            }

            InstanceSave toReturn = null;

            string qualifiedName = representation.GetAttachmentQualifiedName();

            // strip off the guide name if it starts with a guide
            qualifiedName = StripGuideName(qualifiedName);
            

            foreach (InstanceSave instanceSave in instanceContainer.Instances)
            {
                if (prefix + instanceSave.Name == qualifiedName)
                {
                    toReturn = instanceSave;
                    break;
                }
            }            


            if (toReturn == null)
            {
                foreach (InstanceSave instanceSave in instanceContainer.Instances)
                {
                    ElementSave instanceElement = instanceSave.GetBaseElementSave();

                    toReturn = GetInstance(representation, instanceElement, prefix + instanceSave.Name + ".", fetchType);

                    if (toReturn != null)
                    {
                        if (fetchType == InstanceFetchType.DeepInstance)
                        {
                            // toReturn will be toReturn, no need to do anything
                        }
                        else // fetchType == InstanceInCurrentElement
                        {
                            toReturn = instanceSave;
                        }
                        break;
                    }
                }
            }

            return toReturn;
        }

        private string StripGuideName(string qualifiedName)
        {
            foreach (NamedRectangle rectangle in ObjectFinder.Self.GumProjectSave.Guides)
            {
                if (qualifiedName.StartsWith(rectangle.Name + "."))
                {
                    return qualifiedName.Substring(rectangle.Name.Length + 1);
                }
            }

            return qualifiedName;
        }


        public bool IsRepresentation(IPositionedSizedObject ipso)
        {
            return mLineRectangles.Contains(ipso) || mSprites.Contains(ipso) || mTexts.Contains(ipso) || mSolidRectangles.Contains(ipso);
        }

        public ElementSave GetElement(IPositionedSizedObject representation)
        {
            if (SelectedState.Self.SelectedElement != null && 
                SelectedState.Self.SelectedElement.Name == representation.Name)
            {
                return SelectedState.Self.SelectedElement;
            }

            return null;
        }

        public T GetIpsoAt<T>(float x, float y, IList<T> list) where T : IPositionedSizedObject
        {
            foreach(T ipso in list)
            {
                if (ipso.HasCursorOver(x, y))
                {
                    return ipso;
                }
            }
            return default(T);
        }


        internal void UpdateScalesAndPositionsForSelectedChildren()
        {
            List<ElementSave> elementStack = new List<ElementSave>();
            elementStack.Add(SelectedState.Self.SelectedElement);
            foreach (IPositionedSizedObject selectedIpso in SelectionManager.Self.SelectedIpsos)
            {
                UpdateScalesAndPositionsForSelectedChildren(selectedIpso, selectedIpso.Tag as InstanceSave, elementStack);
            }
        }

        void UpdateScalesAndPositionsForSelectedChildren(IPositionedSizedObject ipso, InstanceSave instanceSave, List<ElementSave> elementStack)
        {
            ElementSave selectedElement = null;

            if (instanceSave == null)
            {
                selectedElement = elementStack.Last();
            }
            else
            {
                selectedElement = ObjectFinder.Self.GetElementSave(instanceSave.BaseType);

                elementStack.Add(selectedElement);
            }
            foreach (IPositionedSizedObject child in ipso.Children)
            {
                InstanceSave childInstance = GetInstance(child, InstanceFetchType.DeepInstance);
                if (childInstance == null)
                {
                    continue;
                }

                StateSave stateSave = new StateSave();
                RecursiveVariableFinder rvf = new RecursiveVariableFinder(childInstance, selectedElement);
                FillStateWithVariables(rvf, stateSave, WireframeObjectManager.Self.PositionAndSizeVariables);

                List<VariableSave> exposedVariables = GetExposedVariablesForThisInstance(childInstance, instanceSave, elementStack);
                foreach (VariableSave variable in exposedVariables)
                {
                    stateSave.SetValue(variable.Name, variable.Value);
                }

                SetIpsoWidthAndPositionAccordingToUnitValueAndTypes(child, selectedElement, stateSave);



                UpdateScalesAndPositionsForSelectedChildren(child, childInstance, elementStack);
            }
            elementStack.Remove(selectedElement);
        }
    }
}