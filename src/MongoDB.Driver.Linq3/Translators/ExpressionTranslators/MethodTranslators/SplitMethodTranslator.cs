﻿/* Copyright 2010-present MongoDB Inc.
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
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq3.Ast.Expressions;
using MongoDB.Driver.Linq3.Misc;

namespace MongoDB.Driver.Linq3.Translators.ExpressionTranslators.MethodTranslators
{
    public static class SplitMethodTranslator
    {
        private static readonly MethodInfo[] __splitMethods = new[]
        {
            StringMethod.SplitWithChars,
            StringMethod.SplitWithCharsAndCount,
            StringMethod.SplitWithCharsAndCountAndOptions,
            StringMethod.SplitWithCharsAndOptions,
            StringMethod.SplitWithStringsAndCountAndOptions,
            StringMethod.SplitWithStringsAndOptions
        };

        private static readonly MethodInfo[] __splitWithCharsMethods = new[]
        {
            StringMethod.SplitWithChars,
            StringMethod.SplitWithCharsAndCount,
            StringMethod.SplitWithCharsAndCountAndOptions,
            StringMethod.SplitWithCharsAndOptions
        };

        private static readonly MethodInfo[] __splitWithCountMethods = new[]
        {
            StringMethod.SplitWithCharsAndCount,
            StringMethod.SplitWithCharsAndCountAndOptions,
            StringMethod.SplitWithStringsAndCountAndOptions,
        };

        private static readonly MethodInfo[] __splitWithOptionsMethods = new[]
       {
            StringMethod.SplitWithCharsAndCountAndOptions,
            StringMethod.SplitWithCharsAndOptions,
            StringMethod.SplitWithStringsAndCountAndOptions,
            StringMethod.SplitWithStringsAndOptions
        };

        private static readonly MethodInfo[] __splitWithStringsMethods = new[]
        {
            StringMethod.SplitWithStringsAndCountAndOptions,
            StringMethod.SplitWithStringsAndOptions
        };

        public static TranslatedExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            if (method.IsOneOf(__splitMethods))
            {
                var instance = expression.Object;
                var translatedInstance = ExpressionTranslator.Translate(context, instance);

                var arguments = expression.Arguments;
                var separators = arguments[0];
                if (!(separators is ConstantExpression separatorsConstantExpression))
                {
                    goto notSupported;
                }

                string delimiter;
                if (method.IsOneOf(__splitWithCharsMethods))
                {
                    var separatorChars = (char[])separatorsConstantExpression.Value;
                    if (separatorChars.Length != 1)
                    {
                        goto notSupported;
                    }
                    delimiter = new string(separatorChars[0], 1);
                }
                else if (method.IsOneOf(__splitWithStringsMethods))
                {
                    var separatorStrings = (string[])separatorsConstantExpression.Value;
                    if (separatorStrings.Length != 1)
                    {
                        goto notSupported;
                    }
                    delimiter = separatorStrings[0];
                }
                else
                {
                    goto notSupported;
                }

                Expression count = null;
                if (method.IsOneOf(__splitWithCountMethods))
                {
                    count = arguments[1];
                }

                var optionsValue = StringSplitOptions.None;
                if (method.IsOneOf(__splitWithOptionsMethods))
                {
                    var options = arguments.Last();
                    if (!(options is ConstantExpression optionsConstantExpression))
                    {
                        goto notSupported;
                    }

                    optionsValue = (StringSplitOptions)optionsConstantExpression.Value;
                }

                var translation = (AstExpression)new AstBinaryExpression(AstBinaryOperator.Split, translatedInstance.Translation, delimiter);
                if (optionsValue == StringSplitOptions.RemoveEmptyEntries)
                {
                    translation = new AstFilterExpression(
                        input: translation,
                        cond: new AstBinaryExpression(AstBinaryOperator.Ne, new AstFieldExpression("$$item"), ""),
                        @as: "item");
                }
                if (count != null)
                {
                    var translatedCount = ExpressionTranslator.Translate(context, count);
                    translation = new AstSliceExpression(translation, translatedCount.Translation);
                }

                var serializer = new ArraySerializer<string>(new StringSerializer());
                return new TranslatedExpression(expression, translation, serializer);
            }

        notSupported:
            throw new ExpressionNotSupportedException(expression);
        }
    }
}
