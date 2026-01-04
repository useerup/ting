### Grammar-shift operators

If the grammar-shift operator is the last token on a line, the expression list will start indented on the next non-blank line, and be delimited by line breaks.

If the grammar-shift operator is **not** the last token on a line, then the remainder of the line is parsed and white-space separated expressions

#### The `>>>` fold left-to-right operator

    Identifier = [h]+t => >> >>> (WordBeginChar h) (ZeroOrMore WordFollowChar t)

#### The `<<<` fold right-to-left operator

#### The `(---)` operator

#### The `record` `---` operator

#### The `[---]` operator

#### The `{---}` operator

