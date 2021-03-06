﻿using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class DatumsTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void Level()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Level.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //ensure that the level count is the same
            var levelColl = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            levelColl.OfClass(typeof(Autodesk.Revit.DB.Level));
            Assert.AreEqual(levelColl.ToElements().Count(), 6);

            //change the number and run again
            var numNode = (DoubleInput)dynRevitSettings.Controller.DynamoModel.Nodes.First(x => x is DoubleInput);
            numNode.Value = "0..20..2";
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //ensure that the level count is the same
            levelColl = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            levelColl.OfClass(typeof(Autodesk.Revit.DB.Level));
            Assert.AreEqual(levelColl.ToElements().Count(), 11);
        }
    }
}
