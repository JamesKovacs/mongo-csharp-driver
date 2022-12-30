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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Optimizers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Stages;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Translators;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators;

namespace MongoDB.Driver.Linq.Linq3Implementation
{
    internal static class GroupingAndProjectStageDefinitions
    {
        public static GroupingAndProjectStageDefinitions<TInput, IGrouping<TValue, TInput>, TOutput> ForBucket<TInput, TValue, TOutput>(
            Expression<Func<TInput, TValue>> groupBy,
            IEnumerable<TValue> boundaries,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> output,
            AggregateBucketOptions<TValue> options = null)
        {
            var bucketStage = new BucketStageDefinition<TInput, TValue, TOutput>(groupBy, boundaries, output, options);
            var projectStage = new ProjectStageDefinition<TInput, IGrouping<TValue, TInput>, TOutput>(bucketStage);
            return Create(bucketStage, projectStage);
        }

        public static GroupingAndProjectStageDefinitions<TInput, IGrouping<AggregateBucketAutoResultId<TValue>, TInput>, TOutput> ForBucketAuto<TInput, TValue, TOutput>(
            Expression<Func<TInput, TValue>> groupBy,
            int buckets,
            Expression<Func<IGrouping<AggregateBucketAutoResultId<TValue>, TInput>, TOutput>> output,
            AggregateBucketAutoOptions options = null)
        {
            var bucketAutoStage = new BucketAutoStageDefinition<TInput, TValue, TOutput>(groupBy, buckets, output, options);
            var projectStage = new ProjectStageDefinition<TInput, IGrouping<AggregateBucketAutoResultId<TValue>, TInput>, TOutput>(bucketAutoStage);
            return Create(bucketAutoStage, projectStage);
        }

        public static GroupingAndProjectStageDefinitions<TInput, IGrouping<TValue, TInput>, TOutput> ForGroup<TInput, TValue, TOutput>(
            Expression<Func<TInput, TValue>> groupBy,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> output)
        {
            var groupStage = new GroupStageDefinition<TInput, TValue, TOutput>(groupBy, output);
            var projectStage = new ProjectStageDefinition<TInput, IGrouping<TValue, TInput>, TOutput>(groupStage);
            return Create(groupStage, projectStage);
        }

        private static GroupingAndProjectStageDefinitions<TInput, TGrouping, TOutput> Create<TInput, TGrouping, TOutput>(
            GroupingStageDefinition<TInput, TGrouping, TOutput> groupingStage,
            ProjectStageDefinition<TInput, TGrouping, TOutput> projectStage)
        {
            return new GroupingAndProjectStageDefinitions<TInput, TGrouping, TOutput>(groupingStage, projectStage);
        }
    }

    /// <summary>
    /// Represents a pair of grouping and project stage definitions.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TGrouping">The grouping type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public sealed class GroupingAndProjectStageDefinitions<TInput, TGrouping, TOutput>
    {
        private readonly PipelineStageDefinition<TInput, TGrouping> _groupingStage;
        private readonly PipelineStageDefinition<TGrouping, TOutput> _projectStage;

        /// <summary>
        /// Creates a new instance of a GroupingAndProjectStageDefinitions.
        /// </summary>
        /// <param name="groupingStage">The grouping stage.</param>
        /// <param name="projectStage">The project stage.</param>
        public GroupingAndProjectStageDefinitions(
            PipelineStageDefinition<TInput, TGrouping> groupingStage,
            PipelineStageDefinition<TGrouping, TOutput> projectStage)
        {
            _groupingStage = groupingStage;
            _projectStage = projectStage;
        }

        /// <summary>
        /// The grouping stage.
        /// </summary>
        public PipelineStageDefinition<TInput, TGrouping> GroupingStage => _groupingStage;

        /// <summary>
        /// The project stage.
        /// </summary>
        public PipelineStageDefinition<TGrouping, TOutput> ProjectStage => _projectStage;

        /// <summary>
        /// Deconstructs a GroupingAndProjectStageDefinitions.
        /// </summary>
        /// <param name="groupingStage">The grouping stage.</param>
        /// <param name="projectStage">The project stage.</param>
        public void Deconstruct(
            out PipelineStageDefinition<TInput, TGrouping> groupingStage,
            out PipelineStageDefinition<TGrouping, TOutput> projectStage)
        {
            groupingStage = _groupingStage;
            projectStage = _projectStage;
        }
    }

    internal abstract class GroupingStageDefinition<TInput, TGrouping, TOutput> : PipelineStageDefinition<TInput, TGrouping>
    {
        private readonly Expression<Func<TGrouping, TOutput>> _projection;
        private RenderedPipelineStageDefinition<TGrouping> _renderedGroupingStage;
        private RenderedPipelineStageDefinition<TOutput> _renderedProjectStage;

        public GroupingStageDefinition(Expression<Func<TGrouping, TOutput>> projection)
        {
            _projection = projection;
        }

        public override RenderedPipelineStageDefinition<TGrouping> Render(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry, LinqProvider linqProvider)
        {
            if (linqProvider != LinqProvider.V3)
            {
                throw new InvalidOperationException($"{GetType().Name} is only intended for use with LINQ3.");
            }

            var groupingStage = RenderGroupingStage(inputSerializer, serializerRegistry, out var groupingOutputSerializer);

            var partiallyEvaluatedProjection = (Expression<Func<TGrouping, TOutput>>)PartialEvaluator.EvaluatePartially(_projection);
            var context = TranslationContext.Create(partiallyEvaluatedProjection, groupingOutputSerializer);
            var projectionTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, partiallyEvaluatedProjection, groupingOutputSerializer, asRoot: true);
            var (projectStage, projectionSerializer) = ProjectionHelper.CreateProjectStage(projectionTranslation);

            var pipeline = AstPipeline.Empty(inputSerializer).AddStages(projectionSerializer, groupingStage, projectStage);
            var optimizedPipeline = AstPipelineOptimizer.Optimize(pipeline);
            if (optimizedPipeline.Stages.Count != 2)
            {
                throw new Exception("Expected there to be two stages.");
            }
            var optimizedGroupingStage = optimizedPipeline.Stages[0];
            var optimizedProjectStage = optimizedPipeline.Stages[1];

            _renderedGroupingStage = new RenderedPipelineStageDefinition<TGrouping>(OperatorName, (BsonDocument)optimizedGroupingStage.Render(), groupingOutputSerializer);
            _renderedProjectStage = new RenderedPipelineStageDefinition<TOutput>("$project", (BsonDocument)optimizedProjectStage.Render(), (IBsonSerializer<TOutput>)projectionSerializer);

            return _renderedGroupingStage;
        }

        public RenderedPipelineStageDefinition<TOutput> RenderProjectStage(IBsonSerializer<TGrouping> inputSerializer, IBsonSerializerRegistry serializerRegistry, LinqProvider linqProvider)
        {
            if (_renderedProjectStage == null)
            {
                throw new InvalidOperationException("Grouping stage must be rendered first.");
            }

            return _renderedProjectStage;
        }

        protected abstract AstStage RenderGroupingStage(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry, out IBsonSerializer<TGrouping> groupingOutputSerializer);
    }

    internal sealed class BucketStageDefinition<TInput, TValue, TOutput> : GroupingStageDefinition<TInput, IGrouping<TValue, TInput>, TOutput>
    {
        private readonly IEnumerable<TValue> _boundaries;
        private readonly Expression<Func<TInput, TValue>> _groupBy;
        private readonly AggregateBucketOptions<TValue> _options;

        public BucketStageDefinition(
            Expression<Func<TInput, TValue>> groupBy,
            IEnumerable<TValue> boundaries,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> output,
            AggregateBucketOptions<TValue> options)
            : base(output)
        {
            _groupBy = groupBy;
            _boundaries = boundaries;
            _options = options;
        }

        public override string OperatorName => "$bucket";

        protected override AstStage RenderGroupingStage(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry, out IBsonSerializer<IGrouping<TValue, TInput>> groupingOutputSerializer)
        {
            var partiallyEvaluatedGroupBy = (Expression<Func<TInput, TValue>>)PartialEvaluator.EvaluatePartially(_groupBy);
            var context = TranslationContext.Create(partiallyEvaluatedGroupBy, inputSerializer);
            var groupByTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, partiallyEvaluatedGroupBy, inputSerializer, asRoot: true);

            var valueSerializer = (IBsonSerializer<TValue>)groupByTranslation.Serializer;
            var serializedBoundaries = SerializationHelper.SerializeValues(valueSerializer, _boundaries);
            var serializedDefault = _options != null && _options.DefaultBucket.HasValue ? SerializationHelper.SerializeValue(valueSerializer, _options.DefaultBucket.Value) : null;
            var pushElements = AstExpression.AccumulatorField("_elements", AstUnaryAccumulatorOperator.Push, AstExpression.Var("ROOT"));
            groupingOutputSerializer = IGroupingSerializer.Create(valueSerializer, inputSerializer);

            return AstStage.Bucket(
                groupByTranslation.Ast,
                serializedBoundaries,
                serializedDefault,
                new[] { pushElements });
        }
    }

    internal sealed class BucketAutoStageDefinition<TInput, TValue, TOutput> : GroupingStageDefinition<TInput, IGrouping<AggregateBucketAutoResultId<TValue>, TInput>, TOutput>
    {
        private readonly int _buckets;
        private readonly Expression<Func<TInput, TValue>> _groupBy;
        private readonly AggregateBucketAutoOptions _options;

        public BucketAutoStageDefinition(
            Expression<Func<TInput, TValue>> groupBy,
            int buckets,
            Expression<Func<IGrouping<AggregateBucketAutoResultId<TValue>, TInput>, TOutput>> output,
            AggregateBucketAutoOptions options)
            : base(output)
        {
            _groupBy = groupBy;
            _buckets = buckets;
            _options = options;
        }

        public override string OperatorName => "$bucketAuto";

        protected override AstStage RenderGroupingStage(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry, out IBsonSerializer<IGrouping<AggregateBucketAutoResultId<TValue>, TInput>> groupingOutputSerializer)
        {
            var partiallyEvaluatedGroupBy = (Expression<Func<TInput, TValue>>)PartialEvaluator.EvaluatePartially(_groupBy);
            var context = TranslationContext.Create(partiallyEvaluatedGroupBy, inputSerializer);
            var groupByTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, partiallyEvaluatedGroupBy, inputSerializer, asRoot: true);

            var valueSerializer = (IBsonSerializer<TValue>)groupByTranslation.Serializer;
            var keySerializer = AggregateBucketAutoResultIdSerializer.Create(valueSerializer);
            var serializedGranularity = _options != null && _options.Granularity.HasValue ? _options.Granularity.Value.Value : null;
            var pushElements = AstExpression.AccumulatorField("_elements", AstUnaryAccumulatorOperator.Push, AstExpression.Var("ROOT"));
            groupingOutputSerializer = IGroupingSerializer.Create(keySerializer, inputSerializer);

            return AstStage.BucketAuto(
                groupByTranslation.Ast,
                _buckets,
                serializedGranularity,
                new[] { pushElements });
        }
    }

    internal sealed class GroupStageDefinition<TInput, TValue, TOutput> : GroupingStageDefinition<TInput, IGrouping<TValue, TInput>, TOutput>
    {
        private readonly Expression<Func<TInput, TValue>> _groupBy;

        public GroupStageDefinition(
            Expression<Func<TInput, TValue>> groupBy,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> output)
            : base(output)
        {
            _groupBy = groupBy;
        }

        public override string OperatorName => "$group";

        protected override AstStage RenderGroupingStage(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry, out IBsonSerializer<IGrouping<TValue, TInput>> groupingOutputSerializer)
        {
            var partiallyEvaluatedGroupBy = (Expression<Func<TInput, TValue>>)PartialEvaluator.EvaluatePartially(_groupBy);
            var context = TranslationContext.Create(partiallyEvaluatedGroupBy, inputSerializer);
            var groupByTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, partiallyEvaluatedGroupBy, inputSerializer, asRoot: true);

            var pushElements = AstExpression.AccumulatorField("_elements", AstUnaryAccumulatorOperator.Push, AstExpression.Var("ROOT"));
            var valueSerializer = (IBsonSerializer<TValue>)groupByTranslation.Serializer;
            groupingOutputSerializer = IGroupingSerializer.Create(valueSerializer, inputSerializer);

            return AstStage.Group(groupByTranslation.Ast, pushElements);
        }
    }

    internal sealed class ProjectStageDefinition<TInput, TGrouping, TOutput> : PipelineStageDefinition<TGrouping, TOutput>
    {
        private readonly GroupingStageDefinition<TInput, TGrouping, TOutput> _groupingStage;

        public ProjectStageDefinition(GroupingStageDefinition<TInput, TGrouping, TOutput> groupingStage)
        {
            _groupingStage = groupingStage;
        }

        public override string OperatorName => "$project";

        public override RenderedPipelineStageDefinition<TOutput> Render(IBsonSerializer<TGrouping> inputSerializer, IBsonSerializerRegistry serializerRegistry, LinqProvider linqProvider)
        {
            return _groupingStage.RenderProjectStage(inputSerializer, serializerRegistry, linqProvider);
        }
    }
}
