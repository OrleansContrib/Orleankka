/*
 * Copyright 2007-2011 JetBrains s.r.o.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;

namespace Orleankka
{
    namespace Annotations
    {
        /// <summary>
        ///   Indicates that the function argument should be string literal and match one  of the parameters of the caller
        ///   function.
        ///   For example, <see cref="ArgumentNullException" /> has such parameter.
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
        sealed class InvokerParameterNameAttribute : Attribute
        {}

        /// <summary>
        ///   Indicates that the marked method is assertion method, i.e. it halts control flow if one of the conditions is
        ///   satisfied.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        sealed class AssertionMethodAttribute : Attribute
        {}
    }
}
