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

using System.Linq.Expressions;
using MongoDB.Driver.Linq3.Ast.Filters;

namespace MongoDB.Driver.Linq3.Translators.FilterTranslators
{
    public static class OrExpressionTranslator
    {
        public static AstFilter Translate(TranslationContext context, BinaryExpression expression)
        {
            var leftExpression = expression.Left;
            var rightExpression = expression.Right;

            if (leftExpression.Type == typeof(bool) && rightExpression.Type == typeof(bool))
            {
                var leftTranslation = FilterTranslator.Translate(context, leftExpression);
                var rightTranslation = FilterTranslator.Translate(context, rightExpression);
                return new AstOrFilter(leftTranslation, rightTranslation);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
