
/// Rule token
val token: lexbuf: LexBuffer<char> -> token
/// Rule fs_type
val fs_type: lexbuf: LexBuffer<char> -> token
/// Rule header
val header: p: obj -> buff: obj -> lexbuf: LexBuffer<char> -> token
/// Rule code
val code: p: obj -> buff: obj -> lexbuf: LexBuffer<char> -> token
/// Rule codestring
val codestring: buff: obj -> lexbuf: LexBuffer<char> -> token
/// Rule comment
val comment: lexbuf: LexBuffer<char> -> token
