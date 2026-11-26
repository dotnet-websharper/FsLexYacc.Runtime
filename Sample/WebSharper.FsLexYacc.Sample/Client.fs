namespace WebSharper.FsLexYacc.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open FSharp.Text.Lexing

[<JavaScript>]
module Client =

    // The template is the whole document, loaded from the DOM.
    type IndexTemplate = Template<"index.html", ClientLoad.FromDocument>

    // ------------------------------------------------------------------
    // Theme (light by default, persisted in localStorage)
    // ------------------------------------------------------------------

    let themeStorageKey = "websharper-fslexyacc-theme"
    let theme = Var.Create "light"
    let themeLabel = Var.Create "🌙 Dark mode"

    let applyTheme () =
        JS.Document.DocumentElement.SetAttribute("data-theme", theme.Value)

        themeLabel.Value <-
            if theme.Value = "dark" then "☀️ Light mode" else "🌙 Dark mode"

    let toggleTheme () =
        theme.Value <- (if theme.Value = "dark" then "light" else "dark")

        try
            JS.Window.LocalStorage.SetItem(themeStorageKey, theme.Value)
        with _ ->
            ()

        applyTheme ()

    // ------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------

    let jsonInput =
        Var.Create """{"name":"FsLexYacc","ok":true,"values":[1,2.5,null,"héllo 🌍"]}"""

    let jsonResult = Var.Create "Press “Parse”."
    let tokenResult = Var.Create "Press “Tokenize”."
    let helloInput = Var.Create "hello hello"
    let helloResult = Var.Create "Press “Count”."

    let asciiInput = Var.Create "1 + 2 * (3 - 4) / 5"
    let asciiResult = Var.Create "Press “Tokenize”."
    let apiResult = Var.Create "Press a button."

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    let describeError (e: exn) = "error: " + e.Message

    /// Seed the buffer's end position with the first line, so that reported
    /// positions are 1-based (the FsLexYacc idiom).
    let startAtFirstLine (lexbuf: LexBuffer<'T>) : LexBuffer<'T> =
        lexbuf.EndPos <- Position.FirstLine "(input)"
        lexbuf

    /// string -> byte[] without System.Text.Encoding (not available in the browser).
    let bytesOfString (s: string) : byte[] =
        Array.init s.Length (fun i -> byte (int s.[i]))

    let bytesToString (b: byte[]) : string =
        System.String(b |> Array.map char)

    // ------------------------------------------------------------------
    // JSON parser (Unicode lexer + fsyacc parser)
    // ------------------------------------------------------------------

    let jsonTokenName (tok: JsonParser.token) =
        match tok with
        | JsonParser.EOF -> "EOF"
        | JsonParser.INT _ -> "INT"
        | JsonParser.FLOAT _ -> "FLOAT"
        | JsonParser.ID _ -> "ID"
        | JsonParser.STRING _ -> "STRING"
        | JsonParser.TRUE -> "TRUE"
        | JsonParser.FALSE -> "FALSE"
        | JsonParser.NULL -> "NULL"
        | JsonParser.LEFT_BRACE -> "LEFT_BRACE"
        | JsonParser.RIGHT_BRACE -> "RIGHT_BRACE"
        | JsonParser.LEFT_BRACK -> "LEFT_BRACK"
        | JsonParser.RIGHT_BRACK -> "RIGHT_BRACK"
        | JsonParser.COLON -> "COLON"
        | JsonParser.COMMA -> "COMMA"

    let parseJson (text: string) : string =
        try
            let lexbuf = LexBuffer<char>.FromString text |> startAtFirstLine

            match JsonParser.start JsonLexer.read lexbuf with
            | Some value -> JsonParsing.JsonValue.print value
            | None -> "(empty input)"
        with e ->
            describeError e

    let tokenizeJson (text: string) : string =
        try
            let lexbuf = LexBuffer<char>.FromString text |> startAtFirstLine
            let lines = ResizeArray<string>()
            let mutable finished = false
            let mutable count = 0

            while not finished && count < 1000 do
                let tok = JsonLexer.read lexbuf
                let lexeme = LexBuffer<_>.LexemeString lexbuf
                let sp = lexbuf.StartPos
                let ep = lexbuf.EndPos

                lines.Add(
                    sprintf "%-12s \"%s\"%s line %d, col %d-%d"
                        (jsonTokenName tok)
                        lexeme
                        (String.replicate (max 1 (14 - lexeme.Length)) " ")
                        sp.Line
                        sp.Column
                        ep.Column
                )

                count <- count + 1

                if tok = JsonParser.EOF then
                    finished <- true

            String.concat "\n" lines
        with e ->
            describeError e

    // ------------------------------------------------------------------
    // "Hello" counter
    // ------------------------------------------------------------------

    let countHello (text: string) : string =
        try
            let lexbuf = LexBuffer<char>.FromString text |> startAtFirstLine

            sprintf "tokens matched: %d" (HelloParser.start HelloLexer.tokenstream lexbuf)
        with e ->
            describeError e

    // ------------------------------------------------------------------
    // ASCII lexer (byte buffer + AsciiTables)
    // ------------------------------------------------------------------

    let asciiTokenName (tok: AsciiLexer.Token) =
        match tok with
        | AsciiLexer.INT n -> sprintf "INT(%d)" n
        | AsciiLexer.LPAREN -> "LPAREN"
        | AsciiLexer.RPAREN -> "RPAREN"
        | AsciiLexer.PLUS -> "PLUS"
        | AsciiLexer.MINUS -> "MINUS"
        | AsciiLexer.STAR -> "STAR"
        | AsciiLexer.SLASH -> "SLASH"
        | AsciiLexer.WHITESPACE -> "WHITESPACE"
        | AsciiLexer.NEWLINE -> "NEWLINE"
        | AsciiLexer.EOF -> "EOF"
        | AsciiLexer.INVALID s -> "INVALID(" + s + ")"

    let tokenizeAscii (text: string) : string =
        try
            let lexbuf = LexBuffer<byte>.FromBytes(bytesOfString text) |> startAtFirstLine
            let lines = ResizeArray<string>()
            let mutable finished = false
            let mutable count = 0

            while not finished && count < 1000 do
                let tok = AsciiLexer.token lexbuf
                let lexeme = bytesToString lexbuf.Lexeme
                let sp = lexbuf.StartPos

                lines.Add(
                    sprintf "%-12s \"%s\"%s line %d, col %d"
                        (asciiTokenName tok)
                        lexeme
                        (String.replicate (max 1 (10 - lexeme.Length)) " ")
                        sp.Line
                        sp.Column
                )

                count <- count + 1

                if tok = AsciiLexer.EOF then
                    finished <- true

            String.concat "\n" lines
        with e ->
            describeError e

    // ------------------------------------------------------------------
    // LexBuffer and Position API
    // ------------------------------------------------------------------

    let inspectLexBuffer () =
        let lexbuf = LexBuffer<char>.FromChars("true".ToCharArray()) |> startAtFirstLine
        let _ = JsonLexer.read lexbuf
        lexbuf.BufferLocalStore.["demo"] <- box 42
        let lines = ResizeArray<string>()

        lines.Add("FromChars \"true\" -> LexemeString = " + LexBuffer<_>.LexemeString lexbuf)
        lines.Add("LexemeLength = " + string lexbuf.LexemeLength)
        lines.Add("LexemeChar 0 = '" + string (lexbuf.LexemeChar 0) + "'")
        lines.Add("Lexeme = [" + (lexbuf.Lexeme |> Array.map string |> String.concat ", ") + "]")
        lines.Add("IsPastEndOfStream = " + string lexbuf.IsPastEndOfStream)
        lines.Add("BufferLocalStore[\"demo\"] = " + string (lexbuf.BufferLocalStore.["demo"]))
        String.concat "\n" lines

    let inspectPosition () =
        let p0 = Position.FirstLine "demo.json"
        let p1 = p0.NextLine
        let p2 = p1.EndOfToken 5
        let p3 = p2.ShiftColumnBy 2
        let lines = ResizeArray<string>()

        lines.Add(sprintf "FirstLine \"demo.json\": line=%d col=%d offset=%d" p0.Line p0.Column p0.AbsoluteOffset)
        lines.Add(sprintf "NextLine:            line=%d col=%d bol=%d" p1.Line p1.Column p1.StartOfLineAbsoluteOffset)
        lines.Add(sprintf "EndOfToken 5:        offset=%d" p2.AbsoluteOffset)
        lines.Add(sprintf "ShiftColumnBy 2:     offset=%d col=%d" p3.AbsoluteOffset p3.Column)
        String.concat "\n" lines

    // ------------------------------------------------------------------
    // Entry point
    // ------------------------------------------------------------------

    let presetObject = """{"name":"FsLexYacc","ok":true,"values":[1,2.5,null]}"""

    let presetUnicode =
        """{"greeting":"Γειά σου κόσμε","russian":"Привет мир","emoji":"🌍🌎🌏","mixed":[1,"a",true]}"""

    let presetBad = """{"name": "FsLexYacc", "values": [1, 2,}"""

    [<SPAEntryPoint>]
    let Main () =
        // Restore the stored theme before the first render.
        let stored =
            try
                JS.Window.LocalStorage.GetItem(themeStorageKey)
            with _ ->
                null

        if stored = "dark" || stored = "light" then
            theme.Value <- stored

        applyTheme ()

        IndexTemplate()
            .themeLabel(themeLabel.V)
            .JsonInput(jsonInput)
            .JsonResult(jsonResult.V)
            .TokenResult(tokenResult.V)
            .HelloInput(helloInput)
            .HelloResult(helloResult.V)
            .AsciiInput(asciiInput)
            .AsciiResult(asciiResult.V)
            .ApiResult(apiResult.V)
            .ToggleTheme(fun _ -> toggleTheme ())
            .ParseJson(fun _ -> jsonResult.Value <- parseJson jsonInput.Value)
            .PresetJsonObject(fun _ ->
                jsonInput.Value <- presetObject
                jsonResult.Value <- parseJson presetObject)
            .PresetJsonUnicode(fun _ ->
                jsonInput.Value <- presetUnicode
                jsonResult.Value <- parseJson presetUnicode)
            .PresetJsonBad(fun _ ->
                jsonInput.Value <- presetBad
                jsonResult.Value <- parseJson presetBad)
            .TokenizeJson(fun _ -> tokenResult.Value <- tokenizeJson jsonInput.Value)
            .CountHello(fun _ -> helloResult.Value <- countHello helloInput.Value)
            .TokenizeAscii(fun _ -> asciiResult.Value <- tokenizeAscii asciiInput.Value)
            .InspectLexBuffer(fun _ -> apiResult.Value <- inspectLexBuffer ())
            .InspectPosition(fun _ -> apiResult.Value <- inspectPosition ())
            .Bind()
