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
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Driver.Linq3.Ast.Filters;
using MongoDB.Driver.Linq3.Misc;
using MongoDB.Driver.Linq3.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators;

namespace MongoDB.Driver.Linq3.Translators.ExpressionToFilterTranslators.MethodTranslators
{
    public static class StringExpressionToRegexFilterTranslator
    {
        // private static fields
        private static MethodInfo[] __modifierMethods;
        private static MethodInfo[] __translatableMethods;

        // static constructor
        static StringExpressionToRegexFilterTranslator()
        {
            __modifierMethods = new[]
            {
                StringMethod.ToLower,
                StringMethod.ToUpper,
                StringMethod.Trim,
                StringMethod.TrimEnd,
                StringMethod.TrimStart,
                StringMethod.TrimWithChars
            };

            __translatableMethods = new[]
            {
                StringMethod.Contains,
                StringMethod.EndsWith,
                StringMethod.EndsWithWithComparisonType,
                StringMethod.EndsWithWithIgnoreCaseAndCulture,
                StringMethod.StartsWith,
                StringMethod.StartsWithWithComparisonType,
                StringMethod.StartsWithWithIgnoreCaseAndCulture
            };
        }

        // public static methods
        public static bool  CanTranslate(Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression)
            {
                return methodCallExpression.Method.IsOneOf(__translatableMethods);
            }

            return false;
        }

        public static AstFilter Translate(TranslationContext context, Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression)
            {
                var method = methodCallExpression.Method;
                if (method.IsOneOf(__translatableMethods))
                {
                    switch (method.Name)
                    {
                        case "StartsWith":
                        case "Contains":
                        case "EndsWith":
                            return TranslateStartsWithOrContainsOrEndsWith(context, methodCallExpression);
                    }
                }
            }

            throw new ExpressionNotSupportedException(expression);
        }

        // private static methods
        private static AstFilter CreateFilter(AstFilterField field, Modifiers modifiers, string pattern)
        {
            var combinedPattern = "^" + modifiers.LeadingPattern + pattern + modifiers.TrailingPattern + "$";
            if (combinedPattern.StartsWith("^.*"))
            {
                combinedPattern = combinedPattern.Substring(3);
            }
            if (combinedPattern.EndsWith(".*$"))
            {
                combinedPattern = combinedPattern.Remove(combinedPattern.Length - 3);
            }
            var options = CreateOptions(modifiers);
            return AstFilter.Regex(field, combinedPattern, options);
        }

        private static string CreateOptions(Modifiers modifiers)
        {
            return (modifiers.IgnoreCase || modifiers.ToLower || modifiers.ToUpper) ? "is" : "s";
        }

        private static TValue GetConstantValue<TValue>(Expression expression)
        {
            if (expression is ConstantExpression constantExpression)
            {
                return (TValue)constantExpression.Value;
            }

            throw new ExpressionNotSupportedException(expression);
        }

        private static string GetEscapedTrimChars(MethodCallExpression methodCallWithTrimCharsExpression)
        {
            var method = methodCallWithTrimCharsExpression.Method;
            var arguments = methodCallWithTrimCharsExpression.Arguments;

            if (method.Is(StringMethod.Trim))
            {
                return null;
            }
            else
            {
                var trimCharsExpression = arguments[0];
                var trimChars = GetConstantValue<char[]>(trimCharsExpression);
                if (trimChars == null || trimChars.Length == 0)
                {
                    return null;
                }
                else
                {
                    return EscapeTrimChars(trimChars);
                }
            }

            string EscapeTrimChars(char[] trimChars)
            {
                var result = new StringBuilder();
                foreach (var c in trimChars)
                {
                    switch (c)
                    {
                        case ' ': result.Append("\\ "); break; // space doesn't really need to be escaped but LINQ2 escaped it
                        case '.': result.Append("\\."); break; // dot doesn't really need to be escaped (in a character class) but LINQ2 escaped it
                        case '-': result.Append("\\-"); break;
                        case '^': result.Append("\\^"); break;
                        case '\t': result.Append("\\t"); break;
                        default: result.Append(c); break;
                    }
                }
                return result.ToString();
            }
        }

        private static Modifiers TranslateCulture(Modifiers modifiers, Expression cultureExpression)
        {
            var culture = GetConstantValue<CultureInfo>(cultureExpression);
            if (culture.Equals(CultureInfo.CurrentCulture))
            {
                return modifiers;
            }

            throw new ExpressionNotSupportedException(cultureExpression);
        }

        private static AstFilter TranslateStartsWithOrContainsOrEndsWith(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            var (field, modifiers) = TranslateField(context, expression.Object);
            var value = GetConstantValue<string>(arguments[0]);

            if (method.IsOneOf(StringMethod.StartsWithWithComparisonType, StringMethod.EndsWithWithComparisonType))
            {
                modifiers = TranslateComparisonType(modifiers, arguments[1]);
            }
            if (method.IsOneOf(StringMethod.StartsWithWithIgnoreCaseAndCulture, StringMethod.EndsWithWithIgnoreCaseAndCulture))
            {
                modifiers = TranslateIgnoreCase(modifiers, arguments[1]);
                modifiers = TranslateCulture(modifiers, arguments[2]);
            }

            if (IsImpossibleMatch(modifiers, value))
            {
                return AstFilter.MatchesNothing(field);
            }
            else
            {
                return CreateFilter(field, modifiers, CreatePattern(method.Name, value));
            }

            string CreatePattern(string methodName, string value)
            {
                return methodName switch
                {
                    "Contains" => ".*" + Regex.Escape(value) + ".*",
                    "EndsWith" => ".*" + Regex.Escape(value),
                    "StartsWith" => Regex.Escape(value) + ".*",
                    _ => throw new InvalidOperationException()
                };
            }

            bool IsImpossibleMatch(Modifiers modifiers, string value)
            {
                return
                    (modifiers.ToLower && value != value.ToLower()) ||
                    (modifiers.ToUpper && value != value.ToUpper());
            }
        }

        private static Modifiers TranslateComparisonType(Modifiers modifiers, Expression comparisonTypeExpression)
        {
            var comparisonType = GetConstantValue<StringComparison>(comparisonTypeExpression);
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return modifiers;

                case StringComparison.CurrentCultureIgnoreCase:
                    modifiers.IgnoreCase = true;
                    return modifiers;
            }

            throw new ExpressionNotSupportedException(comparisonTypeExpression);
        }

        private static (AstFilterField, Modifiers) TranslateField(TranslationContext context, Expression fieldExpression)
        {
            if (fieldExpression is MethodCallExpression fieldMethodCallExpression &&
                fieldMethodCallExpression.Method.IsOneOf(__modifierMethods))
            {
                var (field, modifiers) = TranslateField(context, fieldMethodCallExpression.Object);
                modifiers = TranslateModifier(modifiers, fieldMethodCallExpression);
                return (field, modifiers);
            }
            else
            {
                var field = ExpressionToFilterFieldTranslator.Translate(context, fieldExpression);
                return (field, new Modifiers());
            }
        }

        private static Modifiers TranslateIgnoreCase(Modifiers modifiers, Expression ignoreCaseExpression)
        {
            modifiers.IgnoreCase = GetConstantValue<bool>(ignoreCaseExpression);
            return modifiers;
        }

        private static Modifiers TranslateModifier(Modifiers modifiers, MethodCallExpression modifierExpression)
        {
            switch (modifierExpression.Method.Name)
            {
                case "ToLower": return TranslateToLower(modifiers, modifierExpression);
                case "ToUpper": return TranslateToUpper(modifiers, modifierExpression);
                case "Trim": return TranslateTrim(modifiers, modifierExpression);
                case "TrimEnd": return TranslateTrimEnd(modifiers, modifierExpression);
                case "TrimStart": return TranslateTrimStart(modifiers, modifierExpression);
            }

            throw new ExpressionNotSupportedException(modifierExpression);
        }

        private static Modifiers TranslateToLower(Modifiers modifiers, MethodCallExpression toLowerExpression)
        {
            modifiers.ToLower = true;
            modifiers.ToUpper = false;
            return modifiers;
        }

        private static Modifiers TranslateToUpper(Modifiers modifiers, MethodCallExpression toUpperExpression)
        {
            modifiers.ToUpper = true;
            modifiers.ToLower = false;
            return modifiers;
        }

        private static Modifiers TranslateTrim(Modifiers modifiers, MethodCallExpression trimExpression)
        {
            var trimChars = GetEscapedTrimChars(trimExpression);

            if (trimChars == null)
            {
                modifiers.LeadingPattern = modifiers.LeadingPattern + @"\s*(?!\s)";
                modifiers.TrailingPattern = @"(?<!\s)\s*" + modifiers.TrailingPattern;
            }
            else
            {
                var set = Regex.Escape(trimChars);
                modifiers.LeadingPattern = modifiers.LeadingPattern + @"[" + set + "]*(^[" + set + "])";
                modifiers.TrailingPattern = @"(?<[^" + set + "])[" + set + "]*" + modifiers.TrailingPattern;
            }

            return modifiers;
        }

        private static Modifiers TranslateTrimEnd(Modifiers modifiers, MethodCallExpression trimEndExpression)
        {
            var trimChars = GetEscapedTrimChars(trimEndExpression);

            if (trimChars == null)
            {
                modifiers.TrailingPattern = @"(?<!\s)\s*" + modifiers.TrailingPattern;
            }
            else
            {
                modifiers.TrailingPattern = @"(?<=[^" + trimChars + "])[" + trimChars + "]*" + modifiers.TrailingPattern;
            }

            return modifiers;
        }

        private static Modifiers TranslateTrimStart(Modifiers modifiers, MethodCallExpression trimStartExpression)
        {
            var trimChars = GetEscapedTrimChars(trimStartExpression);

            if (trimChars == null)
            {
                modifiers.LeadingPattern = modifiers.LeadingPattern + @"\s*(?!\s)";
            }
            else
            {
                modifiers.LeadingPattern = modifiers.LeadingPattern + @"[" + trimChars + "]*(?=[^" + trimChars + "])";
            }

            return modifiers;
        }

        // nested types
        private class Modifiers
        {
            public Modifiers()
            {
                IgnoreCase = false;
                ToLower = false;
                ToUpper = false;
                LeadingPattern = "";
                TrailingPattern = "";
            }

            public bool IgnoreCase { get; set; }
            public bool ToLower { get; set; }
            public bool ToUpper { get; set; }
            public string LeadingPattern { get; set; }
            public string TrailingPattern { get; set; }
        }
    }
}
