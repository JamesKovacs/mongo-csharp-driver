/* Copyright 2010-present MongoDB Inc.
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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Stages;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToSetStageTranslators
{
    internal static class ExpressionToSetStageTranslator
    {
        public static AstStage Translate(TranslationContext context, IBsonSerializer inputSerializer, LambdaExpression expression)
        {
            if (inputSerializer is not IBsonDocumentSerializer documentSerializer)
            {
                throw new ExpressionNotSupportedException(expression, because: $"serializer {inputSerializer.GetType()} does not implement IBsonDocumentSerializer");
            }

            if (IsNewAnonymousClass(expression, out var newExpression))
            {
                return TranslateNewAnonymousClass(context, documentSerializer, newExpression);
            }

            if (IsNewWithMemberInitializers(expression, out var memberInitExpression))
            {
                return TranslateNewWithMemberInitializers(context, documentSerializer, memberInitExpression);
            }

            throw new ExpressionNotSupportedException(expression, because: "expression is not valid for Set");
        }

        private static bool IsNewAnonymousClass(LambdaExpression expression, out NewExpression newExpression)
        {
            newExpression = expression.Body as NewExpression;
            return
                newExpression != null &&
                newExpression.Type.IsAnonymous();
        }

        private static bool IsNewWithMemberInitializers(LambdaExpression expression, out MemberInitExpression memberInitExpression)
        {
            memberInitExpression = expression.Body as MemberInitExpression;
            return
                memberInitExpression != null &&
                memberInitExpression.NewExpression.Constructor is var constructor &&
                (IsDefaultConstructor(constructor) || IsCopyConstructor(constructor)) &&
                memberInitExpression.Bindings.Count > 0;

            static bool IsDefaultConstructor(ConstructorInfo constructor)
                => constructor.GetParameters().Length == 0;

            static bool IsCopyConstructor(ConstructorInfo constructor)
                =>
                    constructor.GetParameters() is var parameters &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType == constructor.DeclaringType;
        }

        private static AstStage TranslateNewAnonymousClass(TranslationContext context, IBsonDocumentSerializer documentSerializer, NewExpression newExpression)
        {
            var members = newExpression.Members;
            var arguments = newExpression.Arguments;

            var fields = new List<AstComputedField>();
            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var valueExpression = PartialEvaluator.EvaluatePartially(arguments[i]);
                var computedField = CreateComputedField(context, documentSerializer, member, valueExpression);
                fields.Add(computedField);
            }

            return AstStage.Set(fields);
        }

        private static AstStage TranslateNewWithMemberInitializers(TranslationContext context, IBsonDocumentSerializer documentSerializer, MemberInitExpression memberInitExpression)
        {
            var bindings = memberInitExpression.Bindings;

            var fields = new List<AstComputedField>();
            for (var i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding is not MemberAssignment assignment)
                {
                    throw new ExpressionNotSupportedException(memberInitExpression, because: $"the member initializer for {binding.Member.Name} is not a simple assignment");
                }

                var member = binding.Member;
                var valueExpression = PartialEvaluator.EvaluatePartially(assignment.Expression);
                var computedField = CreateComputedField(context, documentSerializer, member, valueExpression);
                fields.Add(computedField);
            }

            return AstStage.Set(fields);
        }

        private static AstComputedField CreateComputedField(TranslationContext context, IBsonDocumentSerializer documentSerializer, MemberInfo member, Expression valueExpression)
        {
            string elementName;
            AstExpression valueAst;
            if (documentSerializer.TryGetMemberSerializationInfo(member.Name, out var serializationInfo))
            {
                elementName = serializationInfo.ElementName;
                var memberSerializer = serializationInfo.Serializer;

                if (valueExpression is ConstantExpression constantValueExpression)
                {
                    var value = constantValueExpression.Value;
                    var serializedValue = SerializationHelper.SerializeValue(memberSerializer, value);
                    valueAst = AstExpression.Constant(serializedValue);
                }
                else
                {
                    var valueTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, valueExpression);
                    ThrowIfMemberAndValueSerializersAreNotCompatible(valueExpression, memberSerializer, valueTranslation.Serializer);
                    valueAst = valueTranslation.Ast;
                }
            }
            else
            {
                elementName = member.Name;
                var valueTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, valueExpression);
                valueAst = valueTranslation.Ast;
            }

            return AstExpression.ComputedField(elementName, valueAst);
        }

        private static void ThrowIfMemberAndValueSerializersAreNotCompatible(Expression expression, IBsonSerializer memberSerializer, IBsonSerializer valueSerializer)
        {
            // TODO: depends on CSHARP-3315
            if (!memberSerializer.Equals(valueSerializer))
            {
                throw new ExpressionNotSupportedException(expression, because: "member and value serializers are not compatible");
            }
        }
    }
}
