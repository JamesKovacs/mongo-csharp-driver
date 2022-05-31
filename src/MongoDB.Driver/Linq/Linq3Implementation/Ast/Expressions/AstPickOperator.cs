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

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions
{
    internal enum AstPickOperator
    {
        Bottom,
        BottomN,
        FirstN,
        LastN,
        MaxN,
        MinN,
        Top,
        TopN
    }

    internal static class AstPickOperatorExtensions
    {
        public static string Render(this AstPickOperator @operator)
        {
            return @operator switch
            {
                AstPickOperator.Bottom => "$bottom",
                AstPickOperator.BottomN => "$bottomN",
                AstPickOperator.FirstN => "$firstN",
                AstPickOperator.LastN => "$lastN",
                AstPickOperator.MaxN => "$maxN",
                AstPickOperator.MinN => "$minN",
                AstPickOperator.Top => "$top",
                AstPickOperator.TopN => "$topN",
                _ => throw new InvalidOperationException($"Invalid operator: {@operator}.")
            };
        }

        public static AstPickAccumulatorOperator ToAccumulatorOperator(this AstPickOperator @operator)
        {
            return @operator switch
            {
                AstPickOperator.Bottom => AstPickAccumulatorOperator.Bottom,
                AstPickOperator.BottomN => AstPickAccumulatorOperator.BottomN,
                AstPickOperator.FirstN => AstPickAccumulatorOperator.FirstN,
                AstPickOperator.LastN => AstPickAccumulatorOperator.LastN,
                AstPickOperator.MaxN => AstPickAccumulatorOperator.MaxN,
                AstPickOperator.MinN => AstPickAccumulatorOperator.MinN,
                AstPickOperator.Top => AstPickAccumulatorOperator.Top,
                AstPickOperator.TopN => AstPickAccumulatorOperator.TopN,
                _ => throw new InvalidOperationException($"Invalid operator: {@operator}.")
            };
        }
    }
}
