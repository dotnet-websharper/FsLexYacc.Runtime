
/// Rule token
val token: lexbuf: LexBuffer<char> -> token
/// Rule string
val string: p: obj -> buff: obj -> lexbuf: LexBuffer<char> -> token
/// Rule code
val code: p: obj -> buff: obj -> lexbuf: LexBuffer<char> -> token
/// Rule codestring
val codestring: buff: obj -> lexbuf: LexBuffer<char> -> token
/// Rule comment
val comment: p: obj -> lexbuf: LexBuffer<char> -> token
