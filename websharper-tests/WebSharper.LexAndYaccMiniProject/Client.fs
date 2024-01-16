namespace WebSharper.LexAndYaccMiniProject

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    open FSharp.Text.Lexing
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>
    let testLexerAndParserFromString text = 
        let lexbuf = LexBuffer<char>.FromString text

        let countFromParser = Parser.start Lexer.tokenstream lexbuf

        countFromParser

    let People =
        ListModel.FromSeq [
            "John"
            "Paul"
        ]


    [<SPAEntryPoint>]
    let Main () =
        let newName = Var.Create ""
        let tokens = Var.Create 0
        IndexTemplate.Main()
            .ListContainer(
                People.View.DocSeqCached(fun (name: string) ->
                    IndexTemplate.ListItem().Name(name).Doc()
                )
            )
            .Name(newName)
            .Add(fun _ ->
                People.Add(newName.Value)
                newName.Value <- ""
            )
            .TestParser(fun x ->
                printfn "attempt"
                testLexerAndParserFromString x.Vars.ParserText.Value 
                |> tokens.Set
            )
            .TokenCount(V(string tokens.V))
            .Doc()
        |> Doc.RunById "main"
