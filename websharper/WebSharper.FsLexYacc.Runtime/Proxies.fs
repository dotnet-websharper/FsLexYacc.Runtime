// WebSharper proxies for the BCL members that FsLexYacc.Runtime uses but that
// WebSharper's standard library does not model.
//
// Only the Unicode classification is needed by generated lexers/parsers: the
// string/array based LexBuffer factories (FromString, FromChars, FromBytes)
// are what make sense in a browser. The reader/stream based factories and
// UnicodeFileAsLexbuf live behind `#if !JAVASCRIPT` in the runtime sources
// because browsers have no synchronous file IO.
namespace FsLexYaccProxies

open WebSharper
open WebSharper.JavaScript

module internal UnicodeCategories =

    /// Test a character against a JavaScript Unicode property escape (`\p{...}`).
    [<Inline "new RegExp($0, \"u\").test($1)">]
    let test (pattern: string) (ch: char) : bool = X<bool>

/// `System.Globalization.CharUnicodeInfo.GetUnicodeCategory` drives the
/// category-based transitions of `UnicodeTables`. Classify a character with the
/// Unicode property escapes built into the JS regex engine, returning the
/// matching `System.Globalization.UnicodeCategory` value.
[<Proxy(typeof<System.Globalization.CharUnicodeInfo>)>]
type internal CharUnicodeInfoProxy() =

    static member GetUnicodeCategory(ch: char) : System.Globalization.UnicodeCategory =
        let isCat = UnicodeCategories.test

        let category =
            if isCat @"\p{Lu}" ch then 0 // UppercaseLetter
            elif isCat @"\p{Ll}" ch then 1 // LowercaseLetter
            elif isCat @"\p{Lt}" ch then 2 // TitlecaseLetter
            elif isCat @"\p{Lm}" ch then 3 // ModifierLetter
            elif isCat @"\p{Lo}" ch then 4 // OtherLetter
            elif isCat @"\p{Mn}" ch then 5 // NonSpacingMark
            elif isCat @"\p{Mc}" ch then 6 // SpacingCombiningMark
            elif isCat @"\p{Me}" ch then 7 // EnclosingMark
            elif isCat @"\p{Nd}" ch then 8 // DecimalDigitNumber
            elif isCat @"\p{Nl}" ch then 9 // LetterNumber
            elif isCat @"\p{No}" ch then 10 // OtherNumber
            elif isCat @"\p{Zs}" ch then 11 // SpaceSeparator
            elif isCat @"\p{Zl}" ch then 12 // LineSeparator
            elif isCat @"\p{Zp}" ch then 13 // ParagraphSeparator
            elif isCat @"\p{Cc}" ch then 14 // Control
            elif isCat @"\p{Cf}" ch then 15 // Format
            elif isCat @"\p{Cs}" ch then 16 // Surrogate
            elif isCat @"\p{Co}" ch then 17 // PrivateUse
            elif isCat @"\p{Pc}" ch then 18 // ConnectorPunctuation
            elif isCat @"\p{Pd}" ch then 19 // DashPunctuation
            elif isCat @"\p{Ps}" ch then 20 // OpenPunctuation
            elif isCat @"\p{Pe}" ch then 21 // ClosePunctuation
            elif isCat @"\p{Pi}" ch then 22 // InitialQuotePunctuation
            elif isCat @"\p{Pf}" ch then 23 // FinalQuotePunctuation
            elif isCat @"\p{Po}" ch then 24 // OtherPunctuation
            elif isCat @"\p{Sm}" ch then 25 // MathSymbol
            elif isCat @"\p{Sc}" ch then 26 // CurrencySymbol
            elif isCat @"\p{Sk}" ch then 27 // ModifierSymbol
            elif isCat @"\p{So}" ch then 28 // OtherSymbol
            else 29 // OtherNotAssigned

        LanguagePrimitives.EnumOfValue category
