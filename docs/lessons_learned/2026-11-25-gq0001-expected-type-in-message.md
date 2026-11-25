# GQ0001: expected type name in message

## Task

Make the GQ0001 message (`GlobalTypeQualificationAnalyzer`) name the expected replacement, like GQ0002 does:
`Type reference '{0}' must be replaced by '{1}'`.

## Notes

- Expected name comes from `ITypeSymbol.ToDisplayString(...)` with `SymbolDisplayFormat.FullyQualifiedFormat`.
- `FullyQualifiedFormat` includes `SymbolDisplayMiscellaneousOptions.UseSpecialTypes`, so `List<int>` would print as
  `global::System.Collections.Generic.List<int>`, which itself violates GQ0002. Remove that option to get
  `global::System.Collections.Generic.List<global::System.Int32>`.
- The pattern `symbol is not ITypeSymbol or IDynamicTypeSymbol` cannot bind a variable; rewrite as
  `symbol is not ITypeSymbol type || type is IDynamicTypeSymbol` to use the symbol afterwards.
- Message assertions live in `GlobalTypeQualificationAnalyzerTests`; all had to be updated with the new text.
