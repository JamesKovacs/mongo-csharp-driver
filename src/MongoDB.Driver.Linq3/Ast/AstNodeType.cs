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

namespace MongoDB.Driver.Linq3.Ast
{
    internal enum AstNodeType
    {
        AddFieldsStage,
        AllFilterOperation,
        AndExpression,
        AndFilter,
        BinaryExpression,
        BitsAllClearFilterOperation,
        BitsAllSetFilterOperation,
        BitsAnyClearFilterOperation,
        BitsAnySetFilterOperation,
        BucketAutoStage,
        BucketStage,
        CollStatsStage,
        ComparisonFilterOperation,
        ComputedArrayExpression,
        ComputedDocumentExpression,
        CondExpression,
        ConstantExpression,
        ConvertExpression,
        CountStage,
        CurrentOpStage,
        CustomAccumulatorExpression,
        DateFromIsoWeekPartsExpression,
        DateFromPartsExpression,
        DateFromStringExpression,
        DatePartExpression,
        DateToPartsExpression,
        DateToStringExpression,
        ElemMatchFilterOperation,
        ExistsFilterOperation,
        ExprFilter,
        FacetStage,
        FieldExpression,
        FieldOperationFilter,
        FilterExpression,
        FilterField,
        FindProjection,
        FunctionExpression,
        GeoIntersectsFilterOperation,
        GeoNearStage,
        GeoWithinBoxFilterOperation,
        GeoWithinCenterFilterOperation,
        GeoWithinCenterSphereFilterOperation,
        GeoWithinFilterOperation,
        GraphLookupStage,
        GroupStage,
        GtFilter,
        IncludeFieldProjectSpecification,
        IndexOfArrayExpression,
        IndexOfBytesExpression,
        IndexOfCPExpression,
        IndexStatsStage,
        InFilterOperation,
        JsonSchemaFilter,
        LetExpression,
        LimitStage,
        ListLocalSessionsStage,
        ListSessionsStage,
        LookupStage,
        LTrimExpression,
        MapExpression,
        MatchesEverythingFilter,
        MatchesNothingFilter,
        MatchStage,
        MergeStage,
        ModFilterOperation,
        NaryExpression,
        NearFilterOperation,
        NearSphereFilterOperation,
        NinFilterOperation,
        NorFilter,
        NotFilterOperation,
        OrExpression,
        OrFilter,
        OutStage,
        Pipeline,
        PlanCacheStatsStage,
        ProjectStage,
        RangeExpression,
        RedactStage,
        ReduceExpression,
        RegexFilterOperation,
        RegexExpression,
        ReplaceAllExpression,
        ReplaceOneExpression,
        ReplaceRootStage,
        RTrimExpression,
        SetFieldProjectSpecification,
        SetStage,
        SizeFilterOperation,
        SliceExpression,
        SortStage,
        SwitchExpression,
        TernaryExpression,
        TextFilter,
        TrimExpression,
        TypeFilterOperation,
        UnaryExpression,
        UnionWithStage,
        UnsetStage,
        UnwindStage,
        WhereFilter,
        ZipExpression
    }
}
