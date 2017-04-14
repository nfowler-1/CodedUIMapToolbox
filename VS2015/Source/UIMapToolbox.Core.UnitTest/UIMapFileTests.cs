using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITest.Common.UIMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace UIMapToolbox.Core.UnitTest
{
    [CodedUITest]
    public class UIMapFileTests
    {
        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void LoadUIMapFile()
        {
            UIMapFile uiMap = UIMapFile.Create(@"TestData\MainUIMap.uitest");

            Assert.IsNotNull(uiMap);
            Assert.IsFalse(uiMap.IsModified);
            Assert.AreEqual(1, uiMap.Maps.Count(), "Wrong number of UIMaps in file");
            Assert.AreEqual("MainUIMap", uiMap.Maps[0].Id, "Wrong id of UIMap in file");
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void MoveUIElement()
        {
            // we want to move
            //   MainUIMap.UIScanJourCaptia104915Window.UIF1Frame1.UICaptiaFrontpgDocument.UIDocuments2Cell
            // to
            //   MainUIMap.UICaptiaWindow.UICaptiaAppFrameFrame.UIAppFrameDocument.UIF1Frame.UIF1Document.UIDocuments2Cell
            const string srcElementPath = "MainUIMap.UIScanJourCaptia104915Window.UIF1Frame1.UICaptiaFrontpgDocument.UIDocuments2Cell";
            const string destElementPath = "MainUIMap.UICaptiaWindow.UICaptiaAppFrameFrame.UIAppFrameDocument.UIF1Frame.UIF1Document.UIDocuments2Cell";
            const string destElementParentPath = "MainUIMap.UICaptiaWindow.UICaptiaAppFrameFrame.UIAppFrameDocument.UIF1Frame.UIF1Document";

            UIMapFile uiMap = UIMapFile.Create(@"TestData\MainUIMap.uitest");

            Assert.IsFalse(uiMap.IsModified);

            // verify that element hasn't been moved yet
            UIObject srcElement = uiMap.FindUIObject(destElementPath);
            Assert.IsNull(srcElement);

            // find element and verify that it is in expected place
            srcElement = uiMap.FindUIObject(srcElementPath);
            Assert.IsNotNull(srcElement, "Could not find UI Object 'UIDocuments2Cell'");

            uiMap.MoveUIObject(srcElementPath, destElementParentPath);

            Assert.IsTrue(uiMap.IsModified);

            // save, load and verify structure
            uiMap.Save("MainUIMap_MovedObject.uitest");
            uiMap = UIMapFile.Create("MainUIMap_MovedObject.uitest");

            // verify that element now has been moved
            srcElement = uiMap.FindUIObject(destElementPath);
            Assert.IsNotNull(srcElement);
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void MergeWhenMovingAndParentWithSameNameExists()
        {
            // we want to move
            //   NotepadUIMap1.UIFindWindow.UIItemWindow (with child UIFindwhatEdit)
            // to
            //   NotepadUIMap1.UIReplaceWindow (with existing child UIItemWindow.UIReplacewithEdit)
            const string srcElementPath = "NotepadUIMap1.UIFindWindow.UIItemWindow";
            const string destElementPath = "NotepadUIMap1.UIReplaceWindow.UIItemWindow";
            const string destElementParentPath = "NotepadUIMap1.UIReplaceWindow";

            UIMapFile uiMap = UIMapFile.Create(@"TestData\NotepadUIMap1.uitest");

            // remember number of children on target parent (to verify that it is not changed afterwards)
            UIObject srcElement = uiMap.FindUIObject(destElementParentPath);
            Assert.IsNotNull(srcElement);
            int parentDescendantsBeforeMove = srcElement.Descendants.Count;

            // verify that element hasn't been moved yet
            srcElement = uiMap.FindUIObject(destElementPath + ".UIFindwhatEdit");
            Assert.IsNull(srcElement);

            // find element and verify that it is in expected place
            srcElement = uiMap.FindUIObject(srcElementPath);
            Assert.IsNotNull(srcElement, String.Format("Could not find UI Object '{0}'", srcElementPath));

            uiMap.MoveUIObject(srcElementPath, destElementParentPath);

            // verify that element now has been merged/moved as expected
            srcElement = uiMap.FindUIObject(srcElementPath);
            Assert.IsNull(srcElement); // has been moved

            // verify that we haven't added (but merged)
            srcElement = uiMap.FindUIObject(destElementParentPath);
            Assert.IsNotNull(srcElement);
            Assert.AreEqual(parentDescendantsBeforeMove, srcElement.Descendants.Count);

            srcElement = uiMap.FindUIObject(destElementPath + ".UIReplacewithEdit");
            Assert.IsNotNull(srcElement); // was there also before moving. just to make sure it hasn't been deleted by accident

            srcElement = uiMap.FindUIObject(destElementPath + ".UIFindwhatEdit");
            Assert.IsNotNull(srcElement); // the moved element
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void MoveUIObjectBetweenDifferentUIMapFiles()
        {
            // we want to move
            //   NotepadUIMap2.UIUntitledNotepadWindow.UIApplicationMenuBar.UIEditMenuItem
            // to
            //   NotepadUIMap1.UIUntitledNotepadWindow.UIApplicationMenuBar

            const string srcElementPath = "NotepadUIMap2.UIUntitledNotepadWindow.UIApplicationMenuBar.UIEditMenuItem";
            const string destElementPath = "NotepadUIMap1.UIUntitledNotepadWindow.UIApplicationMenuBar.UIEditMenuItem";
            const string destElementParentPath = "NotepadUIMap1.UIUntitledNotepadWindow.UIApplicationMenuBar";

            UIMapFile sourceUIMap = UIMapFile.Create(@"TestData\NotepadUIMap2.uitest");
            UIMapFile destUIMap = UIMapFile.Create(@"TestData\NotepadUIMap1.uitest");

            // verify that element hasn't been moved yet
            UIObject uiObject = destUIMap.FindUIObject(destElementPath);
            Assert.IsNull(uiObject, String.Format("Did not expect to find '{0}' in destination yet", destElementPath));

            // find element and verify that it is in expected place
            uiObject = sourceUIMap.FindUIObject(srcElementPath);
            Assert.IsNotNull(uiObject, String.Format("Could not find UI Object '{0}'", srcElementPath));

            destUIMap.MoveUIObject(sourceUIMap, srcElementPath, destElementParentPath);

            // verify that element now has been merged/moved as expected
            uiObject = sourceUIMap.FindUIObject(srcElementPath);
            Assert.IsNull(uiObject, String.Format("Did not expect to find '{0}' in source", srcElementPath));

            // verify that it now exists in destination
            uiObject = destUIMap.FindUIObject(destElementPath);
            Assert.IsNotNull(uiObject, String.Format("Could not find '{0}' in destination", destElementPath));
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void MoveTopLevelElementToOtherUIMapFile()
        {
            // we want to move
            //   NotepadUIMap1.UIFindWindow
            // to
            //   NotepadUIMap2

            const string srcElementPath = "NotepadUIMap1.UIFindWindow";
            const string destElementPath = "NotepadUIMap2.UIFindWindow";
            const string destElementParentPath = "NotepadUIMap2";

            UIMapFile sourceUIMap = UIMapFile.Create(@"TestData\NotepadUIMap1.uitest");
            UIMapFile destUIMap = UIMapFile.Create(@"TestData\NotepadUIMap2.uitest");

            // verify that element hasn't been moved yet
            UIObject uiObject = destUIMap.FindUIObject(destElementPath);
            Assert.IsNull(uiObject, String.Format("Did not expect to find '{0}' in destination yet", destElementPath));

            // find element and verify that it is in expected place
            uiObject = sourceUIMap.FindUIObject(srcElementPath);
            Assert.IsNotNull(uiObject, String.Format("Could not find UI Object '{0}'", srcElementPath));

            destUIMap.MoveUIObject(sourceUIMap, srcElementPath, destElementParentPath);

            // verify that element now has been merged/moved as expected
            uiObject = sourceUIMap.FindUIObject(srcElementPath);
            Assert.IsNull(uiObject, String.Format("Did not expect to find '{0}' in source", srcElementPath));

            // verify that it now exists in destination
            uiObject = destUIMap.FindUIObject(destElementPath);
            Assert.IsNotNull(uiObject, String.Format("Could not find '{0}' in destination", destElementPath));
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void MergeTopLevelElementToOtherUIMapFile()
        {
            // we want to move
            //   NotepadUIMap2.UIUntitledNotepadWindow
            // to
            //   NotepadUIMap1 (with existing UIUntitledNotepadWindow child)

            const string srcElementPath = "NotepadUIMap2.UIUntitledNotepadWindow";
            const string destElementPath = "NotepadUIMap1.UIUntitledNotepadWindow";
            const string destElementParentPath = "NotepadUIMap1";

            UIMapFile sourceUIMap = UIMapFile.Create(@"TestData\NotepadUIMap2.uitest");
            UIMapFile destUIMap = UIMapFile.Create(@"TestData\NotepadUIMap1.uitest");

            // remember number of top level elements on target (to verify that it is not changed afterwards)
            int destTopLevelCountBeforeMove = destUIMap.Maps[0].TopLevelWindows.Count;

            // verify that we already have destination top element
            UIObject uiObject = destUIMap.FindUIObject(destElementPath);
            Assert.IsNotNull(uiObject, String.Format("Expected to find '{0}' in destination", destElementPath));

            // verify that element hasn't been moved yet
            uiObject = destUIMap.FindUIObject(destElementPath + ".UIUntitledNotepadTitleBar");
            Assert.IsNull(uiObject, String.Format("Did not expect to find '{0}.UIUntitledNotepadTitleBar' in destination yet", destElementPath));

            // find element and verify that it is in expected place
            uiObject = sourceUIMap.FindUIObject(srcElementPath);
            Assert.IsNotNull(uiObject, String.Format("Could not find UI Object '{0}'", srcElementPath));

            destUIMap.MoveUIObject(sourceUIMap, srcElementPath, destElementParentPath);

            // verify that element now has been merged/moved as expected
            uiObject = sourceUIMap.FindUIObject(srcElementPath);
            Assert.IsNull(uiObject, String.Format("Did not expect to find '{0}' in source", srcElementPath));

            // verify that it now exists in destination
            uiObject = destUIMap.FindUIObject(destElementPath + ".UIUntitledNotepadTitleBar");
            Assert.IsNotNull(uiObject, String.Format("Expected to find '{0}.UIUntitledNotepadTitleBar' in destination now", destElementPath));

            // verify that we actually have merged top level element (and not added)
            int destTopLevelCountAfterMove = destUIMap.Maps[0].TopLevelWindows.Count;
            Assert.AreEqual(destTopLevelCountBeforeMove, destTopLevelCountAfterMove, "Different number of top level elements after move");
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void FindUIObject()
        {
            UIMapFile uiMap = UIMapFile.Create(@"TestData\MainUIMap.uitest");

            UIObject uiObject = uiMap.FindUIObject("MainUIMap.UIScanJourCaptiasjora0Window.UIF3Frame.UICaptiaCase2011000028Document.UITitleEdit");
            Assert.IsNotNull(uiObject);
            Assert.AreEqual("UITitleEdit", uiObject.Id);
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void FindTopLevelUIObject()
        {
            UIMapFile uiMap = UIMapFile.Create(@"TestData\MainUIMap.uitest");

            UIObject uiObject = uiMap.FindUIObject("MainUIMap.UIScanJourCaptiasjora0Window");
            Assert.IsNotNull(uiObject);
            Assert.AreEqual("UIScanJourCaptiasjora0Window", uiObject.Id);
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void VerifyThatNonexistingElementIsNotFound()
        {
            UIMapFile uiMap = UIMapFile.Create(@"TestData\MainUIMap.uitest");

            UIObject uiObject = uiMap.FindUIObject("MainUIMap.UIScanJourCaptiasjora0Window.NonExistingId");
            Assert.IsNull(uiObject);
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void TestDeleteElement()
        {
            UIMapFile uiMap = UIMapFile.Create(@"TestData\NotepadUIMap1.uitest");

            string path = "NotepadUIMap1.UIUntitledNotepadWindow.UIApplicationMenuBar.UIFileMenuItem";

            // first verify that we can find object from given path
            UIObject uiObject = uiMap.FindUIObject(path);
            Assert.IsNotNull(uiObject);

            // delete it
            uiMap.DeleteUIObject(path);

            // verify that we no longer can find it
            uiObject = uiMap.FindUIObject(path);
            Assert.IsNull(uiObject);
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void TestDeleteTopLevelElement()
        {
            UIMapFile uiMap = UIMapFile.Create(@"TestData\NotepadUIMap1.uitest");

            string path = "NotepadUIMap1.UIUntitledNotepadWindow";

            // first verify that we can find object from given path
            UIObject uiObject = uiMap.FindUIObject(path);
            Assert.IsNotNull(uiObject);

            // delete it
            uiMap.DeleteUIObject(path);

            // verify that we no longer can find it
            uiObject = uiMap.FindUIObject(path);
            Assert.IsNull(uiObject);
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void VerifyActionIsStillValidAfterMovingUIElement()
        {
            // we want to move
            //   NotepadUIMapWithActions.UITesttxtNotepadWindow.UIApplicationMenuBar
            // to
            //   NotepadUIMapWithActions.UIUntitledNotepadWindow
            // and verify that actions are still intact

            const string srcElementPath = "NotepadUIMapWithActions.UITesttxtNotepadWindow.UIApplicationMenuBar";
            const string destElementPath = "NotepadUIMapWithActions.UIUntitledNotepadWindow.UIApplicationMenuBar.UIFileMenuItem";
            const string destElementParentPath = "NotepadUIMapWithActions.UIUntitledNotepadWindow";

            UIMapFile uiMap = UIMapFile.Create(@"TestData\NotepadUIMapWithActions.uitest");

            // verify that element hasn't been moved yet
            UIObject srcElement = uiMap.FindUIObject(destElementPath);
            Assert.IsNull(srcElement);

            // find element and verify that it is in expected place
            srcElement = uiMap.FindUIObject(srcElementPath);
            Assert.IsNotNull(srcElement, String.Format("Could not find source UI Object '{0}'", srcElementPath));

            // verify actions before we move element
            Assert.AreEqual(8, uiMap.ExecuteActions.Count, "Unexpected number of actions in UIMap");
            Assert.AreEqual("NotepadUIMapWithActions.UITesttxtNotepadWindow.UIApplicationMenuBar.UIFileMenuItem.UIPageSetupMenuItem", uiMap.ExecuteActions.Actions[3].UIObjectName);

            uiMap.MoveUIObject(srcElementPath, destElementParentPath);

            // save, load and verify structure
            uiMap.Save("NotepadUIMapWithActions_MovedObject.uitest");
            uiMap = UIMapFile.Create("NotepadUIMapWithActions_MovedObject.uitest");

            // verify that element now has been moved
            srcElement = uiMap.FindUIObject(destElementPath);
            Assert.IsNotNull(srcElement);

            // and verify actions
            Assert.AreEqual(8, uiMap.ExecuteActions.Count, "Unexpected number of actions in UIMap");
            Assert.AreEqual("NotepadUIMapWithActions.UIUntitledNotepadWindow.UIApplicationMenuBar.UIFileMenuItem.UIPageSetupMenuItem", uiMap.ExecuteActions.Actions[3].UIObjectName);
        }

        [TestMethod]
        [DeploymentItem("TestData", "TestData")]
        public void RenameUIElement()
        {
            // we want to rename
            //   NotepadUIMapWithActions.UIUntitledNotepadWindow
            // to
            //   NotepadUIMapWithActions.UINotepadWindow
            // and verify that actions are still intact

            const string originalElementPath = "NotepadUIMapWithActions.UIUntitledNotepadWindow";
            const string newName = "UINotepadWindow";
            const string newElementPath = "NotepadUIMapWithActions.UINotepadWindow";

            UIMapFile uiMap = UIMapFile.Create(@"TestData\NotepadUIMapWithActions.uitest");

            // not modified yet
            Assert.IsFalse(uiMap.IsModified);

            // verify that element hasn't been renamed yet
            UIObject srcElement = uiMap.FindUIObject(newElementPath);
            Assert.IsNull(srcElement);

            // find element and verify that it is in expected place
            srcElement = uiMap.FindUIObject(originalElementPath);
            Assert.IsNotNull(srcElement, String.Format("Could not find source UI Object '{0}'", originalElementPath));

            // verify actions before we move element
            Assert.AreEqual(8, uiMap.ExecuteActions.Count, "Unexpected number of actions in UIMap");
            Assert.AreEqual("NotepadUIMapWithActions.UIUntitledNotepadWindow.UIApplicationMenuBar.UIHelpMenuItem.UIAboutNotepadMenuItem", uiMap.ExecuteActions.Actions[0].UIObjectName);

            uiMap.RenameUIObject(originalElementPath, newName);

            Assert.IsTrue(uiMap.IsModified);

            // save, load and verify structure
            uiMap.Save("NotepadUIMapWithActions_RenamedObject.uitest");
            uiMap = UIMapFile.Create("NotepadUIMapWithActions_RenamedObject.uitest");

            // verify that element has been renamed
            srcElement = uiMap.FindUIObject(newElementPath);
            Assert.IsNotNull(srcElement);

            // and verify actions
            Assert.AreEqual(8, uiMap.ExecuteActions.Count, "Unexpected number of actions in UIMap");
            Assert.AreEqual("NotepadUIMapWithActions.UINotepadWindow.UIApplicationMenuBar.UIHelpMenuItem.UIAboutNotepadMenuItem", uiMap.ExecuteActions.Actions[0].UIObjectName);

            Assert.IsFalse(uiMap.IsModified);
        }
    }
}