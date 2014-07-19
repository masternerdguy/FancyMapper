﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FancyMirrorTest.fancy;

namespace FancyMirrorTest.test
{
    public class ComplexTestModel
    {
        [Mirror("ComplexTestObject.Name")]
        public string PoorName { get; set; }

        public TestModel NestedModel { get; set; }

        public new string ToString()
        {
            return "PoorName: " + PoorName + ", NestedModel: ("+NestedModel.ToString()+")";
        }
    }
}
