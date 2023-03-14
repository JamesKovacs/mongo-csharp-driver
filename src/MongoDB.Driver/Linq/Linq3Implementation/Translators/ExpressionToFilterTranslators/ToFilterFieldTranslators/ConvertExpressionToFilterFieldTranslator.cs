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

using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators
{
    internal static class ConvertExpressionToFilterFieldTranslator
    {
        public static AstFilterField Translate(TranslationContext context, UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.TypeAs)
            {
                var field = ExpressionToFilterFieldTranslator.Translate(context, expression.Operand);
                var fieldType = field.Serializer.ValueType;
                var targetType = expression.Type;

                // must check for enum conversions before numeric conversions
                if (IsConvertEnumToUnderlyingType(fieldType, targetType))
                {
                    return TranslateConvertEnumToUnderlyingType(field, targetType);
                }

                if (IsNumericConversion(fieldType, targetType))
                {
                    return TranslateNumericConversion(field, targetType);
                }

                if (IsConvertToNullable(fieldType, targetType))
                {
                    return TranslateConvertToNullable(field);
                }

                if (IsConvertToBaseType(fieldType, targetType))
                {
                    return TranslateConvertToBaseType(field, targetType);
                }

                if (IsConvertToDerivedType(fieldType, targetType))
                {
                    return TranslateConvertToDerivedType(field, targetType);
                }
            }

            throw new ExpressionNotSupportedException(expression);
        }

        private static bool IsConvertEnumToUnderlyingType(Type fieldType, Type targetType)
        {
            return
                fieldType.IsEnumOrNullableEnum(out _, out var underlyingType) &&
                targetType.IsSameAsOrNullableOf(underlyingType);
        }

        private static bool IsConvertToBaseType(Type fieldType, Type targetType)
        {
            return fieldType.IsSubclassOf(targetType);
        }

        private static bool IsConvertToDerivedType(Type fieldType, Type targetType)
        {
            return targetType.IsSubclassOf(fieldType);
        }

        private static bool IsConvertToNullable(Type fieldType, Type targetType)
        {
            return
                targetType.IsConstructedGenericType &&
                targetType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                targetType.GetGenericArguments()[0] == fieldType;
        }

        private static bool IsNumericConversion(Type fieldType, Type targetType)
        {
            return NumericConversionSerializer.IsNumericConversion(fieldType, targetType);
        }

        private static AstFilterField TranslateConvertEnumToUnderlyingType(AstFilterField field, Type targetType)
        {
            var fieldSerializer = field.Serializer;
            var fieldType = fieldSerializer.ValueType;

            IBsonSerializer enumSerializer;
            if (fieldType.IsNullable())
            {
                var nullableSerializer = (INullableSerializer)fieldSerializer;
                enumSerializer = nullableSerializer.ValueSerializer;
            }
            else
            {
                enumSerializer = fieldSerializer;
            }

            IBsonSerializer targetSerializer;
            var enumUnderlyingTypeSerializer = EnumUnderlyingTypeSerializer.Create(enumSerializer);
            if (targetType.IsNullable())
            {
                targetSerializer = NullableSerializer.Create(enumUnderlyingTypeSerializer);
            }
            else
            {
                targetSerializer = enumUnderlyingTypeSerializer;
            }

            return AstFilter.Field(field.Path, targetSerializer);
        }

        private static AstFilterField TranslateConvertToBaseType(AstFilterField field, Type baseType)
        {
            var derivedTypeSerializer = field.Serializer;
            var derivedType = derivedTypeSerializer.ValueType;
            var targetSerializer = DowncastingSerializer.Create(baseType, derivedType, derivedTypeSerializer);
            return AstFilter.Field(field.Path, targetSerializer);
        }

        private static AstFilterField TranslateConvertToDerivedType(AstFilterField field, Type targetType)
        {
            var targetSerializer = BsonSerializer.LookupSerializer(targetType);
            return AstFilter.Field(field.Path, targetSerializer);
        }

        private static AstFilterField TranslateConvertToNullable(AstFilterField field)
        {
            var nullableSerializer = NullableSerializer.Create(field.Serializer);
            return AstFilter.Field(field.Path, nullableSerializer);
        }

        private static AstFilterField TranslateNumericConversion(AstFilterField field, Type targetType)
        {
            var sourceSerializer = field.Serializer;
            var sourceType = sourceSerializer.ValueType;
            var targetSerializer = (sourceSerializer is IBsonNumericSerializer numericSerializer && numericSerializer.HasNumericRepresentation) ?
                BsonSerializer.LookupSerializer(targetType) :
                NumericConversionSerializer.Create(sourceType, targetType, sourceSerializer);
            return AstFilter.Field(field.Path, targetSerializer);
        }
    }
}
