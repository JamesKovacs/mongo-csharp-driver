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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers.KnownSerializers;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators
{
    internal class TranslationContext
    {
        // private fields
        private readonly SymbolTable _symbolTable;
        private readonly KnownSerializersRegistry _knownKnownSerializersRegistry;

        // constructors
        public TranslationContext()
            : this(new SymbolTable())
        {
        }

        public TranslationContext(KnownSerializersRegistry knownSerializersRegistry)
            : this(new SymbolTable(), knownSerializersRegistry)
        {
        }

        public TranslationContext(SymbolTable symbolTable)
            : this(symbolTable, new KnownSerializersRegistry())
        {
        }

        public TranslationContext(SymbolTable symbolTable, KnownSerializersRegistry knownSerializersRegistry)
        {
            _symbolTable = Ensure.IsNotNull(symbolTable, nameof(symbolTable));
            _knownKnownSerializersRegistry = Ensure.IsNotNull(knownSerializersRegistry, nameof(knownSerializersRegistry));
        }

        // public properties
        public SymbolTable SymbolTable => _symbolTable;

        public KnownSerializersRegistry KnownSerializersRegistry => _knownKnownSerializersRegistry;

        // public methods
        public override string ToString()
        {
            return $"{{ SymbolTable : {_symbolTable} }}";
        }

        public TranslationContext WithSymbol(ParameterExpression parameter, Symbol symbol)
        {
            var newSymbolTable = _symbolTable.WithSymbol(parameter, symbol);
            var newContext = new TranslationContext(newSymbolTable, _knownKnownSerializersRegistry);
            return newContext;
        }

        public TranslationContext WithSymbolAsCurrent(ParameterExpression parameter, Symbol symbol)
        {
            var newSymbolTable = _symbolTable.WithSymbolAsCurrent(parameter, symbol);
            var newContext = new TranslationContext(newSymbolTable, _knownKnownSerializersRegistry);
            return newContext;
        }

        public TranslationContext WithSymbols(params (ParameterExpression, Symbol)[] symbols)
        {
            var newSymbolTable = _symbolTable.WithSymbols(symbols);
            var newContext = new TranslationContext(newSymbolTable, _knownKnownSerializersRegistry);
            return newContext;
        }
    }
}
