using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITest.Common;
using Microsoft.VisualStudio.TestTools.UITest.Common.UIMap;
using System.Collections.Generic;

namespace UIMapToolbox.Core
{
    /// <summary>
    /// Wraps UITest class by adding additional methods for UIMap Toolbox. Was not able to subclass UITest, since
    /// only way to load a .uitest file is by calling static method UITest.Create().
    /// </summary>
    public class UIMapFile
    {
        /// <summary>
        /// Reference to wrapped UITest instance.
        /// Need to wrap UITest this way, since only way to load UITest is by calling static Create() method.
        /// </summary>
        public UITest UITest { get; private set; }
        
        public bool IsModified { get; set; }

        /// <summary>
        /// Loads UIMap from specified file
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public void Load(string fileName)
        {
            this.UITest = UITest.Create(fileName);
        }

        /// <summary>
        /// Creates UIMap instance from specified file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static UIMapFile Create(string fileName)
        {
            UIMapFile uiMapFile = new UIMapFile();
            uiMapFile.Load(fileName);

            return uiMapFile;
        }

        /// <summary>
        /// Finds the UI object in UIMap file.
        /// </summary>
        /// <param name="uiMapFile">The UI map file.</param>
        /// <param name="path">The path to UI element, e.g. "NotepadUIMap1.UIUntitledNotepadWindow.UIApplicationMenuBar.UIEditMenuItem" (i.e. including root).</param>
        /// <returns></returns>
        public UIObject FindUIObject(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return null;

            string[] pathElements = path.Split('.');

            // first two elements are UIMap and top-level window, which are handled especially
            UIMap uiMap = this.UITest.Maps.Where(map => map.Id == pathElements[0]).FirstOrDefault();
            if (uiMap == null)
                return null;

            TopLevelElement topLevelWindow = uiMap.TopLevelWindows.Where(e => e.Id == pathElements[1]).FirstOrDefault();
            if (topLevelWindow == null)
                return null;

            UIObject currentObject = topLevelWindow;
            Collection<UIObject> descendants = topLevelWindow.Descendants;

            // now recursively find element
            for (int i = 2; i < pathElements.Length; i++)
            {
                string pathElement = pathElements[i];
                if (String.IsNullOrWhiteSpace(pathElement))
                    continue;

                currentObject = descendants.Where(e => e.Id == pathElement).FirstOrDefault();
                if (currentObject == null)
                    return null;

                descendants = currentObject.Descendants;
            }

            return currentObject;
        }

        /// <summary>
        /// Move UI object internally in UIMap
        /// </summary>
        /// <param name="uiMapFile">UIMap file.</param>
        /// <param name="srcElementPath">The source element path.</param>
        /// <param name="destParentPath">The destination parent path.</param>
        public void MoveUIObject(string srcElementPath, string destParentPath)
        {
            this.MoveUIObject(this, srcElementPath, destParentPath);
        }

        /// <summary>
        /// Move UI object from other UIMap into current UIMap
        /// </summary>
        /// <param name="srcUIMapFile">The source UIMap file.</param>
        /// <param name="srcElementPath">The source element path.</param>
        /// <param name="destParentPath">Path of destination parent.</param>
        public void MoveUIObject(UIMapFile srcUIMapFile, string srcElementPath, string destParentPath)
        {
            // source element
            UIObject srcElement = srcUIMapFile.FindUIObject(srcElementPath);
            if (srcElement == null)
                throw new NullReferenceException(String.Format("Could not find source UIObject with path '{0}'", srcElementPath));

            // source element parent
            string srcParentPath = srcElementPath.Substring(0, srcElementPath.LastIndexOf('.'));

            // not supported to move actions between different files
            if (srcUIMapFile != this)
            {
                // find references in actions
                foreach (UITestAction action in srcUIMapFile.UITest.ExecuteActions.Actions)
                {
                    if ((action.UIObjectName != null) && (action.UIObjectName.StartsWith(srcElementPath)))
                        throw new NotSupportedException("Your source UIMap file contains actions referencing elements that are moved. This is not supported");
                }
            }

            if (srcElement is TopLevelElement)
            {
                // can only be moved to root
                if (destParentPath.Contains('.'))
                    throw new ArgumentException(String.Format("A top level element ({0}) can only be moved to UIMap root", srcElementPath));

                TopLevelElement srcTopLevelElement = (TopLevelElement)srcElement;
                UIMap destUIMap = this.UITest.Maps.Where(m => m.Id == destParentPath).FirstOrDefault();
                if (destUIMap == null)
                    throw new NullReferenceException(String.Format("Could not find UIMap '{0}' in destination UIMap file", destParentPath));

                UIMap srcUIMap = srcUIMapFile.UITest.Maps.Where(m => m.Id == srcParentPath).FirstOrDefault();
                if (srcUIMap == null)
                    throw new NullReferenceException(String.Format("Could not find UIMap '{0}' in source UIMap file", srcParentPath));

                TopLevelElement destTopLevelElement = destUIMap.TopLevelWindows.Where(t => t.Id == srcTopLevelElement.Id).FirstOrDefault();
                if (destTopLevelElement == null)
                {
                    // doesn't exist in destination, so simply move it
                    destUIMap.TopLevelWindows.Add(srcTopLevelElement);
                    srcUIMap.TopLevelWindows.Remove(srcTopLevelElement);
                }
                else
                {
                    // a top level element with same id already exists in destination, so we need to merge
                    RecursivelyMergeElements(srcTopLevelElement, destTopLevelElement);

                    srcUIMapFile.DeleteUIObject(srcElementPath);
                }

                this.IsModified = true;
                srcUIMapFile.IsModified = true;
            }
            else
            {
                // just a "normal" UIObject (not top-level)

                UIObject srcParent = srcUIMapFile.FindUIObject(srcParentPath);
                if (srcParent == null)
                    throw new NullReferenceException(String.Format("Could not find source parent UIObject with path '{0}'", srcParentPath));

                // destination element parent
                UIObject destParent = this.FindUIObject(destParentPath);
                if (destParent == null)
                    throw new NullReferenceException(String.Format("Could not find destination parent UIObject with path '{0}'", destParentPath));

                // find references in actions and move them
                string destElementPath = String.Format("{0}.{1}", destParentPath, srcElement.Id);
                foreach (UITestAction action in this.UITest.ExecuteActions.Actions)
                {
                    if ((action.UIObjectName != null) && (action.UIObjectName.StartsWith(srcElementPath)))
                        action.UIObjectName = action.UIObjectName.Replace(srcElementPath, destElementPath);
                }

                // see if element already exists in destination (if so, we need to merge)
                UIObject destObject = destParent.Descendants.Where(d => d.Id == srcElement.Id).FirstOrDefault();
                if (destObject == null)
                {
                    // just move it
                    destParent.Descendants.Add(srcElement);
                    srcParent.Descendants.Remove(srcElement);
                }
                else
                {
                    // we need to recursively move elements
                    RecursivelyMergeElements(srcElement, destObject);
                    srcUIMapFile.DeleteUIObject(srcElementPath);
                }

                this.IsModified = true;
                srcUIMapFile.IsModified = true;
            }
        }

        private void RecursivelyMergeElements(UIObject srcParent, UIObject destParent)
        {
            for (int i = srcParent.Descendants.Count - 1; i >= 0; i--)
            {
                UIObject uiChild = srcParent.Descendants[i];

                // see if it already exists in parent
                UIObject destChild = destParent.Descendants.Where(d => d.Id == uiChild.Id).FirstOrDefault();
                if (destChild == null)
                {
                    // didn't exists, so we can just move it
                    destParent.Descendants.Add(uiChild);
                    srcParent.Descendants.Remove(uiChild);
                }
                else
                {
                    // call recursively
                    RecursivelyMergeElements(uiChild, destChild);
                }
            }
        }

        /// <summary>
        /// Deletes the UI object with the specified path from UIMap.
        /// </summary>
        /// <param name="uiMapFile">The UI map file.</param>
        /// <param name="path">The path.</param>
        public void DeleteUIObject(string path)
        {
            // find element
            UIObject element = this.FindUIObject(path);
            if (element == null)
                return;

            // find references in actions and delete them (backwards, since we're deleting elements in collection)
            for (int i = this.UITest.ExecuteActions.Actions.Count - 1; i >= 0; i--)
            {
                UITestAction action = this.UITest.ExecuteActions.Actions[i];

                if ((action.UIObjectName != null) && (action.UIObjectName.StartsWith(path)))
                {
                    this.UITest.ExecuteActions.Delete(action.Id);
                    this.IsModified = true;
                }
            }

            if (element is TopLevelElement)
            {
                // assuming only one UIMap in a file...
                this.UITest.Maps[0].TopLevelWindows.Remove(element as TopLevelElement);
            }
            else
            {
                // source element parent
                string srcParentPath = path.Substring(0, path.LastIndexOf('.'));
                UIObject srcParent = this.FindUIObject(srcParentPath);
                if (srcParent == null)
                    throw new NullReferenceException(String.Format("Could not find source parent UIObject with path '{0}'", srcParentPath));
                
                // delete object itself
                srcParent.Descendants.Remove(element);
            }

            this.IsModified = true;
        }

        /// <summary>
        /// Renames the UI object.
        /// </summary>
        /// <param name="uiMapFile">The UI map file.</param>
        /// <param name="elementPath">The element path, e.g. "NotepadUIMapWithActions.UIUntitledNotepadWindow".</param>
        /// <param name="newId">The new id of element, e.g. "UINotepadWindow" (not full path).</param>
        /// <returns>Path to renamed element, e.g. "NotepadUIMapWithActions.UINotepadWindow"</returns>
        public string RenameUIObject(string elementPath, string newId)
        {
            // rename by moving UI element in same UIMap
            // find source element
            UIObject srcElement = this.FindUIObject(elementPath);
            if (srcElement == null)
                throw new NullReferenceException(String.Format("Could not find source UIObject with path '{0}'", elementPath));

            // source element parent
            string parentPath = elementPath.Substring(0, elementPath.LastIndexOf('.'));

            // verify that we don't already have element with new name
            string newPath = String.Format("{0}.{1}", parentPath, newId);
            UIObject destUIObject = this.FindUIObject(newPath);
            if (destUIObject != null)
                throw new ArgumentException(String.Format("An UI element with path '{0}' already exists in UIMap", newPath));

            srcElement.Id = newId;

            // find references in actions and rename them
            foreach (UITestAction action in this.UITest.ExecuteActions.Actions)
            {
                if ((action.UIObjectName != null) && (action.UIObjectName.StartsWith(elementPath)))
                    action.UIObjectName = action.UIObjectName.Replace(elementPath, newPath);
            }

            this.IsModified = true;

            return newPath;
        }

        /// <summary>
        /// Gets the name of the UIMap.
        /// </summary>
        /// <returns></returns>
        public string GetUIMapName()
        {
            if ((this.UITest.Maps == null) || (this.UITest.Maps.Count == 0))
                return String.Empty;

            return this.UITest.Maps[0].Id;
        }

        /// <summary>
        /// Saves the specified UI test file (and sets IsModified = false afterwards)
        /// </summary>
        /// <param name="uiTestFile">The UI test file.</param>
        public void Save(string uiTestFile)
        {
            this.UITest.Save(uiTestFile);
            this.IsModified = false;
        }

        /// <summary>
        /// Access to UIMaps in wrapped UITest instance.
        /// </summary>
        public Collection<Microsoft.VisualStudio.TestTools.UITest.Common.UIMap.UIMap> Maps
        {
            get { return this.UITest.Maps; }
        }

        /// <summary>
        /// Access to actions in wrapped UITest instance.
        /// </summary>
        public ActionList ExecuteActions
        {
            get { return this.UITest.ExecuteActions; }
        }
    }
}