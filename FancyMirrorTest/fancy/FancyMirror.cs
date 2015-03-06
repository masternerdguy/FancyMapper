﻿/*
 * Copyright (C) 2014 Nathan Wiehoff
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 *   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 *   IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FancyMirrorTest.Fancy
{
    public static class FancyMirror
    {
        /// <summary>
        /// Maps the provided property with the value of the property in the source object that is referenced
        /// using the path in the mirror.
        /// </summary>
        /// <param name="mirror"></param>
        /// <param name="property"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void MapMirror(MirrorAttribute mirror, PropertyInfo property, object source, object destination)
        {
            string[] route = mirror.Path.Split('.');
            //the first element is always the class
            string className = mirror.Class;
            string srcTypeName = FancyUtil.AttemptToDeproxyName(source);
            //verify the type of the target object
            if (srcTypeName == className)
            {
                //must be solved using recursion
                if (route.Length == 1)
                {
                    //re-evaluate in the context of the child
                    var lp = FancyUtil.GetValueOfProperty(property, destination);
                    FancyUtil.Mirror(source, lp);
                }
                else if (route.Length > 0)
                {
                    var sourceProp = RecursiveRouteMirror(route, 1, 10, source);
                    if (mirror.WalkChildren)
                    {
                        var s = FancyUtil.GetValueOfProperty(sourceProp.Item1, sourceProp.Item2);
                        var d = FancyUtil.GetValueOfProperty(property, destination);
                        if (s == null)
                        {
                            /*
                             * The source object has not been instantiated. This means there's no way we could
                             * clone the child properties it specifies. Data attributes are too limited for 
                             * the null substitution value to be used because it can't accept anything that
                             * could be construed as not being known at compile time (must be constant).
                             */
                            throw new NullReferenceException("Unable to map to property " + property.Name + " because it is null in the source");
                        }
                        else
                        {
                            FancyUtil.Mirror(s, d);
                        }
                    }
                    else
                    {
                        FancyUtil.SetValueOfProperty(property, sourceProp.Item2, destination, sourceProp.Item1, mirror.NullSubstitute);
                    }
                }
                else
                {
                    throw new Exception("Empty routes cannot be evaluated");
                }
            }
            else
            {
                throw new Exception("A single MirrorAttribute can only be used to map to one class");
            }
        }

        /// <summary>
        /// Walks down the route recursively locating properties until the end point is reached, and then returns
        /// that final property.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="routeIndex"></param>
        /// <param name="limit"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Tuple<PropertyInfo, object> RecursiveRouteMirror(string[] route, int routeIndex, int limit, object source)
        {
            if (routeIndex > limit)
            {
                throw new Exception("Aborted recursive routing, will not attempt to use an index greater than " + limit);
            }

            //get the current route position
            string current = route[routeIndex];
            //get list of properties on source
            List<PropertyInfo> props = FancyUtil.GetPropertiesOnObject(source).ToList();
            //select the one that matches this step of the route
            PropertyInfo prop = props.SingleOrDefault(x => x.Name == current);
            if (prop == null)
            {
                throw new Exception(
                    string.Format("The route '{0}' provided by this MirrorAttribute is not a valid path to the target property", string.Join(".", route)));
            }
            else
            {
                //is this the end of the chain?
                if (route.Length - 1 == routeIndex)
                {
                    return new Tuple<PropertyInfo, object>(prop, source);
                }
                else
                {
                    //go further down the chain
                    return RecursiveRouteMirror(route, routeIndex + 1, limit, FancyUtil.GetValueOfProperty(prop, source));
                }
            }
        }
    }
}
