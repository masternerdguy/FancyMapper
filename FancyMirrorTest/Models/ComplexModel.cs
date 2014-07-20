﻿using FancyMirrorTest.Fancy;

namespace FancyMirrorTest.Models
{
    public class ComplexModel
    {
        public ComplexModel()
        {
            NestedModel = new SimpleModel();
        }

        [Mirror("SimpleObject.SomeString")]
        [Mirror("ComplexObject.Name")]
        [Mirror("OverlyComplexObject.NestedComplexObject.Name")]
        public string PoorName { get; set; }

        [Mirror("SimpleObject")]
        [Mirror("ComplexObject.NestedObject", WalkChildren = true)]
        [Mirror("OverlyComplexObject.NestedComplexObject.NestedObject", WalkChildren = true)]
        public SimpleModel NestedModel { get; set; }

        public new string ToString()
        {
            return "PoorName: " + PoorName + ", NestedModel: ("+NestedModel.ToString()+")";
        }
    }
}
