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
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver.Linq3.Ast.Filters;
using MongoDB.Driver.Linq3.Misc;
using MongoDB.Driver.Linq3.Reflection;
using MongoDB.Driver.Linq3.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators;

namespace MongoDB.Driver.Linq3.Translators.ExpressionToFilterTranslators.ExpressionTranslators
{
    internal static class MemberExpressionToFilterTranslator
    {
        public static AstFilter Translate(TranslationContext context, MemberExpression expression)
        {
            var memberInfo = expression.Member;

            if (memberInfo is PropertyInfo propertyInfo)
            {
                if (propertyInfo.Is(NullableProperty.HasValue))
                {
                    var fieldExpression = expression.Expression;
                    var field = ExpressionToFilterFieldTranslator.Translate(context, fieldExpression);
                    return AstFilter.Ne(field, BsonNull.Value);
                }

                if (propertyInfo.PropertyType == typeof(bool))
                {
                    var field = ExpressionToFilterFieldTranslator.Translate(context, expression);
                    return AstFilter.Eq(field, true);
                }
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
