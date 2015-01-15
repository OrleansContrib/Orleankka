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
        ///   Indicates that marked method builds string by format pattern and (optional) arguments.
        ///   Parameter, which contains format string, should be given in constructor.
        ///   The format string should be in <see cref="string.Format(IFormatProvider,string,object[])" /> -like form
        /// </summary>
        [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        sealed class StringFormatMethodAttribute : Attribute
        {
            /// <summary>
            ///   Initializes new instance of StringFormatMethodAttribute
            /// </summary>
            /// <param name="formatParameterName">Specifies which parameter of an annotated method should be treated as format-string</param>
            public StringFormatMethodAttribute(string formatParameterName)
            {
                FormatParameterName = formatParameterName;
            }

            /// <summary>
            ///   Gets format parameter name
            /// </summary>
            public string FormatParameterName { get; private set; }
        }

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

        /// <summary>
        ///   Indicates that the value of marked element could be <c>null</c> sometimes, so the check for <c>null</c> is necessary
        ///   before its usage
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false,
            Inherited = true)]
        sealed class CanBeNullAttribute : Attribute
        {}

        /// <summary>
        ///   Indicates that the value of marked element could never be <c>null</c>
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false,
            Inherited = true)]
        sealed class NotNullAttribute : Attribute
        {}

        /// <summary>
        ///   Indicates that the value of marked type (or its derivatives) cannot be compared using '==' or '!=' operators.
        ///   There is only exception to compare with <c>null</c>, it is permitted
        /// </summary>
        [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
        sealed class CannotApplyEqualityOperatorAttribute : Attribute
        {}

        /// <summary>
        ///   Tells code analysis engine if the parameter is completely handled when the invoked method is on stack.
        ///   If the parameter is delegate, indicates that delegate is executed while the method is executed.
        ///   If the parameter is enumerable, indicates that it is enumerated while the method is executed.
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
        sealed class InstantHandleAttribute : Attribute
        {}

        /// <summary>
        ///   Indicates that method doesn't contain observable side effects.
        ///   The same as <see cref="System.Diagnostics.Contracts.PureAttribute" />
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, Inherited = true)]
        sealed class PureAttribute : Attribute
        {}
    }
}
