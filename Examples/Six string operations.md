StrintOps.ting:

    CleanUp = FixWhitespace >> FixHyphenation >> SplitIntoSentences >> .First

    FixWhitespace = string..\Replace("\n"," ")

    FixHyphenation = string..\Replace("- ","")

    SplitIntoSentences = string..\(Split ". ") >> .\Map EnsureEndsWithPeriod

    EnsureEndsWithPeriod = string s \ s.EndsWith "." then s else s + "."


StringOpsTest

def test_fix_whitespace():
    assert fix_whitespace("new\nline") == "new line"

def test_fix_hyphenation():
    assert fix_hyphenation("auto- mobile") == "automobile"

def test_clean_up():
    input = "Relating to the state transient lodging tax; creating\nnew provisions; amending ORS 284.131 and\n320.305; prescribing an effective date; and pro-\nviding for revenue raising that requires approval\nby a three-fifths majority.\nWhereas Enrolled House Bill 2267 (chapter 818,"
    assert clean_up(input) == "Relating to the state transient lodging tax; creating new provisions; amending ORS 284.131 and 320.305; prescribing an effective date; and providing for revenue raising that requires approval by a three-fifths majority."
view rawstring_ops_test.py hosted with ❤ by GitHub
Haskell






    "a b c".\Split " "